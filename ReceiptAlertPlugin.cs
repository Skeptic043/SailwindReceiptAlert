using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace SailwindReceiptAlert;

[BepInPlugin(PluginId, PluginName, PluginVersion)]
public sealed class ReceiptAlertPlugin : BaseUnityPlugin
{
    public const string PluginId = "skeptic043.sailwind.receiptalert";
    public const string PluginName = "Sailwind Receipt Alert";
    public const string PluginVersion = "0.1.0";

    internal const string WarningText = "Don't forget your trade receipt!";
    internal static readonly Vector3 ZoneSize = new Vector3(12f, 4f, 12f);
    internal static readonly Vector3 ZoneOffset = new Vector3(0f, 0.5f, 0f);

    private static ReceiptAlertPlugin? _instance;
    private ConfigEntry<bool>? _enabled;
    private Harmony? _harmony;

    private void Awake()
    {
        _instance = this;
        _enabled = Config.Bind("General", "Enabled", true,
            "Warn when leaving a port desk area with an uncollected trade receipt.");
        _harmony = new Harmony(PluginId);
        _harmony.PatchAll(typeof(PortDudeAwakePatch));
        Logger.LogInfo($"{PluginName} {PluginVersion} loaded; PortDude zone size {ZoneSize} m.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        if (ReferenceEquals(_instance, this))
            _instance = null;
    }

    internal static void LogSetup(string message)
    {
        _instance?.Logger.LogDebug(message);
    }

    internal static void LogSetupFailure(string message, Exception? error = null)
    {
        ManualLogSource? log = _instance?.Logger;
        if (log == null)
            return;

        if (error == null)
            log.LogWarning(message);
        else
            log.LogWarning($"{message}: {error}");
    }

    internal static void WarnIfReceiptAvailable()
    {
        ReceiptAlertPlugin? plugin = _instance;
        if (plugin == null || plugin._enabled?.Value != true)
            return;

        EconomyUIReceiptScribe scribe = EconomyUIReceiptScribe.instance;
        if (!scribe || !scribe.ReceiptAvailable())
            return;

        NotificationUi notification = NotificationUi.instance;
        if (notification)
            notification.ShowNotification(WarningText);
    }
}

[HarmonyPatch(typeof(PortDude), "Awake")]
internal static class PortDudeAwakePatch
{
    [HarmonyPostfix]
    private static void Postfix(PortDude __instance)
    {
        if (!__instance)
        {
            ReceiptAlertPlugin.LogSetupFailure("Skipped receipt zone: PortDude is missing.");
            return;
        }

        GameObject? zoneObject = null;
        try
        {
            Transform anchor = __instance.transform;
            if (anchor.GetComponent<ReceiptAlertSkippedMarker>())
                return;

            foreach (Transform child in anchor)
            {
                if (child.GetComponent<ReceiptAlertZone>())
                {
                    ReceiptAlertPlugin.LogSetup($"Receipt zone already exists for {__instance.name}; skipped duplicate.");
                    return;
                }
            }

            Vector3 anchorScale = anchor.lossyScale;
            if (!ValidScale(anchorScale.x) || !ValidScale(anchorScale.y) || !ValidScale(anchorScale.z))
            {
                anchor.gameObject.AddComponent<ReceiptAlertSkippedMarker>();
                ReceiptAlertPlugin.LogSetupFailure($"Skipped receipt zone for {__instance.name}: invalid world scale {anchorScale}.");
                return;
            }

            zoneObject = new GameObject("Receipt Alert Zone");
            zoneObject.SetActive(false);
            zoneObject.transform.SetParent(anchor, false);
            zoneObject.transform.localPosition = new Vector3(
                ReceiptAlertPlugin.ZoneOffset.x / anchorScale.x,
                ReceiptAlertPlugin.ZoneOffset.y / anchorScale.y,
                ReceiptAlertPlugin.ZoneOffset.z / anchorScale.z);
            zoneObject.transform.localRotation = Quaternion.identity;
            zoneObject.transform.localScale = new Vector3(
                1f / anchorScale.x, 1f / anchorScale.y, 1f / anchorScale.z);

            // The kinematic body makes this trigger participate in physics even if
            // a particular PortDude has no Rigidbody in its scene hierarchy.
            Rigidbody body = zoneObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;

            BoxCollider trigger = zoneObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = ReceiptAlertPlugin.ZoneSize;
            zoneObject.AddComponent<ReceiptAlertZone>();
            zoneObject.SetActive(true);

            ReceiptAlertPlugin.LogSetup($"Created receipt zone for {__instance.name}.");
        }
        catch (Exception error)
        {
            if (zoneObject)
                UnityEngine.Object.Destroy(zoneObject);
            ReceiptAlertPlugin.LogSetupFailure($"Could not create receipt zone for {__instance.name}", error);
        }
    }

    private static bool ValidScale(float value)
    {
        return value > 0.0001f && !float.IsNaN(value) && !float.IsInfinity(value);
    }
}

// Scene-owned marker prevents repeated warnings if another mod re-invokes setup.
public sealed class ReceiptAlertSkippedMarker : MonoBehaviour
{
}

public sealed class ReceiptAlertZone : MonoBehaviour
{
    private bool _callbackFailureLogged;

    private void OnTriggerExit(Collider other)
    {
        // Require Sailwind's local controller as well as the tag used by PortDude.
        if (!other || !other.CompareTag("Player") || other != Refs.charController)
            return;

        try
        {
            ReceiptAlertPlugin.WarnIfReceiptAvailable();
        }
        catch (Exception error)
        {
            if (_callbackFailureLogged)
                return;

            _callbackFailureLogged = true;
            ReceiptAlertPlugin.LogSetupFailure($"Receipt warning failed at {transform.parent?.name}", error);
        }
    }
}
