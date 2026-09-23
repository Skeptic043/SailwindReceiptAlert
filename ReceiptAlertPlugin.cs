using System;
using System.Reflection;
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
    public const string PluginVersion = "1.1.0";

    internal const string WarningText = "Don't forget your trade receipt!";
    internal const string CollectedText = "Trade receipt collected!";
    private const float ParchmentWidthMultiplier = 1.2f;
    internal static readonly Vector3 ZoneSize = new Vector3(12f, 4f, 12f);
    internal static readonly Vector3 ZoneOffset = new Vector3(0f, 0.5f, 0f);

    private static ReceiptAlertPlugin? _instance;
    private static readonly FieldInfo? NotificationUiField = AccessTools.Field(typeof(NotificationUi), "UI");
    private static bool _issuingReceiptNotification;
    private static bool _parchmentSetupFailureLogged;
    private static bool _parchmentSetupLogged;
    private static bool _receiptStorageFailureLogged;
    private static Transform? _widenedParchment;
    private static Vector3 _originalParchmentScale;
    private static ReceiptAlertParchmentReset? _parchmentReset;
    private ConfigEntry<bool>? _enabled;
    private ConfigEntry<bool>? _autoCollectReceipt;
    private Harmony? _harmony;

    private void Awake()
    {
        _instance = this;
        _enabled = Config.Bind("General", "Enabled", true,
            "Handle uncollected trade receipts when leaving a port desk area.");
        _autoCollectReceipt = Config.Bind("General", "AutoCollectReceipt", false,
            "Automatically collect an available trade receipt on exit instead of showing a reminder.");
        _harmony = new Harmony(PluginId);
        _harmony.PatchAll(typeof(PortDudeAwakePatch));
        _harmony.PatchAll(typeof(NotificationUiShowNotificationPatch));
        Logger.LogInfo($"{PluginName} {PluginVersion} loaded; PortDude zone size {ZoneSize} m.");
    }

    private void OnDestroy()
    {
        RestoreParchmentWidth();
        if (_parchmentReset)
            Destroy(_parchmentReset);
        _parchmentReset = null;
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

    internal static bool HandleReceiptOnExit()
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

        if (plugin._autoCollectReceipt?.Value == true)
        {
            TradeReceiptsUI receipts = TradeReceiptsUI.instance;
            if (!receipts)
            {
                if (!_receiptStorageFailureLogged)
                {
                    _receiptStorageFailureLogged = true;
                    LogSetupFailure("Trade receipt storage is unavailable; falling back to the reminder.");
                }
                return ShowReceiptNotification(notification, WarningText);
            }

            if (!scribe.RequestCurrentReceipt())
                return false;

            return ShowReceiptNotification(notification, CollectedText);
        }

        return ShowReceiptNotification(notification, WarningText);
    }

    private static bool ShowReceiptNotification(NotificationUi notification, string message)
    {
        _issuingReceiptNotification = true;
        try
        {
            notification.ShowNotification(message);
        }
        catch
        {
            RestoreParchmentWidth();
            throw;
        }
        finally
        {
            _issuingReceiptNotification = false;
        }
        return true;
    }

    internal static void OnNotificationShown(NotificationUi notification, string message)
    {
        if (!_issuingReceiptNotification ||
            (!string.Equals(message, WarningText, StringComparison.Ordinal) &&
             !string.Equals(message, CollectedText, StringComparison.Ordinal)))
        {
            RestoreParchmentWidth();
            return;
        }

        try
        {
            WidenParchment(notification);
        }
        catch (Exception error)
        {
            RestoreParchmentWidth();
            LogParchmentSetupFailure($"Could not widen receipt notification parchment: {error}");
        }
    }

    private static void WidenParchment(NotificationUi notification)
    {
        if (_widenedParchment)
        {
            GameObject? currentUi = NotificationUiField?.GetValue(notification) as GameObject;
            if (currentUi != null && currentUi && _widenedParchment != null &&
                _widenedParchment.IsChildOf(currentUi.transform))
                return;
            RestoreParchmentWidth();
        }

        GameObject? ui = NotificationUiField?.GetValue(notification) as GameObject;
        if (ui == null || !ui)
        {
            LogParchmentSetupFailure("Could not find Sailwind's notification UI; receipt notification will use its normal size.");
            return;
        }

        Transform? parchment = ui.transform.Find("notification UI");
        Transform? parchmentMesh = parchment ? parchment.Find("notification_UI") : null;
        if (parchment == null || !parchment || parchmentMesh == null || !parchmentMesh ||
            !parchmentMesh.GetComponent<SkinnedMeshRenderer>())
        {
            LogParchmentSetupFailure("Could not find Sailwind's notification parchment; receipt notification will use its normal size.");
            return;
        }

        ReceiptAlertParchmentReset reset = ui.GetComponent<ReceiptAlertParchmentReset>();
        if (!reset)
            reset = ui.AddComponent<ReceiptAlertParchmentReset>();

        _parchmentReset = reset;
        _widenedParchment = parchment;
        _originalParchmentScale = parchment.localScale;
        parchment.localScale = new Vector3(
            _originalParchmentScale.x * ParchmentWidthMultiplier,
            _originalParchmentScale.y,
            _originalParchmentScale.z);
        if (!_parchmentSetupLogged)
        {
            _parchmentSetupLogged = true;
            LogSetup($"Receipt notification parchment width set to {ParchmentWidthMultiplier:0.0}x while the message is visible.");
        }
    }

    internal static void RestoreParchmentWidth(ReceiptAlertParchmentReset? reset = null)
    {
        if (reset != null && !ReferenceEquals(_parchmentReset, reset))
            return;

        if (_widenedParchment != null && _widenedParchment)
            _widenedParchment.localScale = _originalParchmentScale;
        _widenedParchment = null;
    }

    internal static void OnParchmentResetDestroyed(ReceiptAlertParchmentReset reset)
    {
        RestoreParchmentWidth(reset);
        if (ReferenceEquals(_parchmentReset, reset))
            _parchmentReset = null;
    }

    private static void LogParchmentSetupFailure(string message)
    {
        if (_parchmentSetupFailureLogged)
            return;
        _parchmentSetupFailureLogged = true;
        LogSetupFailure(message);
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
                $"enabled={plugin._enabled?.Value == true}, autoCollect={plugin._autoCollectReceipt?.Value == true}, " +
                $"notificationCalled={notificationCalled?.ToString() ?? "n/a"}.");
        }
        catch (Exception error)
        {
            plugin.Logger.LogDebug($"Could not log receipt zone {phase} event at {area}: {error.GetType().Name}.");
        }
    }
}

[HarmonyPatch(typeof(NotificationUi), "ShowNotification", typeof(string), typeof(float))]
internal static class NotificationUiShowNotificationPatch
{
    [HarmonyPostfix]
    private static void Postfix(NotificationUi __instance, string __0)
    {
        ReceiptAlertPlugin.OnNotificationShown(__instance, __0);
    }
}

public sealed class ReceiptAlertParchmentReset : MonoBehaviour
{
    private void OnDisable()
    {
        ReceiptAlertPlugin.RestoreParchmentWidth(this);
    }

    private void OnDestroy()
    {
        ReceiptAlertPlugin.OnParchmentResetDestroyed(this);
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
            notificationCalled = ReceiptAlertPlugin.HandleReceiptOnExit();
        }
        catch (Exception error)
        {
            if (!_callbackFailureLogged)
            {
                _callbackFailureLogged = true;
                ReceiptAlertPlugin.LogSetupFailure($"Receipt handling failed at {transform.parent?.name}", error);
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
