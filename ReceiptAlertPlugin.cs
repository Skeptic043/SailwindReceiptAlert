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
    public const string PluginVersion = "1.0.0";

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

    internal static bool WarnIfReceiptAvailable()
    {
        ReceiptAlertPlugin? plugin = _instance;
        if (plugin == null || plugin._enabled?.Value != true)
            return false;

        EconomyUIReceiptScribe scribe = EconomyUIReceiptScribe.instance;
        if (!scribe || !scribe.ReceiptAvailable())
            return false;

        NotificationUi notification = NotificationUi.instance;
        if (!notification)
            return false;

        notification.ShowNotification(WarningText);
        return true;
    }

    internal static void LogPlayerEvent(string phase, Collider collider, string area, bool? notificationCalled)
    {
        ReceiptAlertPlugin? plugin = _instance;
        if (plugin == null)
            return;

        try
        {
            EconomyUIReceiptScribe scribe = EconomyUIReceiptScribe.instance;
            string receipt = "unknown";
            if (scribe)
            {
                try
                {
                    receipt = scribe.ReceiptAvailable() ? "available" : "unavailable";
                }
                catch (Exception error)
                {
                    receipt = $"check-failed:{error.GetType().Name}";
                }
            }

            plugin.Logger.LogDebug(
                $"Player-tagged collider {phase} receipt zone at {area}: " +
                $"collider={collider.name} ({collider.GetType().Name}, id={collider.GetInstanceID()}), " +
                $"scribe={(bool)scribe}, receipt={receipt}, notification={(bool)NotificationUi.instance}, " +
                $"enabled={plugin._enabled?.Value == true}, notificationCalled={notificationCalled?.ToString() ?? "n/a"}.");
        }
        catch (Exception error)
        {
            plugin.Logger.LogDebug($"Could not log receipt zone {phase} event at {area}: {error.GetType().Name}.");
        }
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
    private const int MaxPlayerEventLogs = 12;
    private bool _callbackFailureLogged;
    private int _playerEventLogs;

    private void OnTriggerEnter(Collider other)
    {
        if (other && other.CompareTag("Player"))
            LogPlayerEvent("entered", other, null);
    }

    private void OnTriggerExit(Collider other)
    {
        // PortDude itself uses the Player tag to identify the player collider.
        if (!other || !other.CompareTag("Player"))
            return;

        bool notificationCalled = false;
        try
        {
            notificationCalled = ReceiptAlertPlugin.WarnIfReceiptAvailable();
        }
        catch (Exception error)
        {
            if (!_callbackFailureLogged)
            {
                _callbackFailureLogged = true;
                ReceiptAlertPlugin.LogSetupFailure($"Receipt warning failed at {transform.parent?.name}", error);
            }
        }

        LogPlayerEvent("exited", other, notificationCalled);
    }

    private void LogPlayerEvent(string phase, Collider other, bool? notificationCalled)
    {
        if (_playerEventLogs >= MaxPlayerEventLogs)
            return;

        _playerEventLogs++;
        ReceiptAlertPlugin.LogPlayerEvent(phase, other, transform.parent?.name ?? name, notificationCalled);
        if (_playerEventLogs == MaxPlayerEventLogs)
            ReceiptAlertPlugin.LogSetup($"Receipt zone at {transform.parent?.name ?? name} reached its player-event debug log limit.");
    }
}
