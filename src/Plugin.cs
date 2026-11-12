using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace SailwindReceiptAlert;

[BepInPlugin(Id, Name, Version)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string Id = "skeptic043.sailwind.receiptalert";
    public const string Name = "Sailwind Receipt Alert";
    public const string Version = "1.1.1";

    internal const string ReminderText = "Don't forget your trade receipt!";
    internal const string CollectedText = "Trade receipt collected!";

    internal static ManualLogSource Log = null!;
    private static ConfigEntry<bool> enabledSetting = null!;
    private static ConfigEntry<bool> autoCollectSetting = null!;

    private void Awake()
    {
        Log = Logger;
        enabledSetting = Config.Bind("General", "Enabled", true,
            "Handle uncollected trade receipts when leaving a port desk area.");
        autoCollectSetting = Config.Bind("General", "AutoCollectReceipt", false,
            "Automatically collect an available trade receipt on exit instead of showing a reminder.");
        new Harmony(Id).PatchAll(typeof(Plugin).Assembly);
    }

    // Called when the player leaves a trade and mission desk area. Settings are
    // read here so Configuration Manager changes apply immediately.
    internal static void OnLeftDesk()
    {
        // Without the notification UI the player would not see the result, so
        // the receipt is left waiting.
        NotificationUi notifications = NotificationUi.instance;
        EconomyUIReceiptScribe scribe = EconomyUIReceiptScribe.instance;
        if (!notifications || !scribe)
            return;

        ExitAction action = ReceiptExit.Decide(
            enabledSetting.Value,
            scribe.ReceiptAvailable(),
            autoCollectSetting.Value,
            TradeReceiptsUI.instance);

        if (action == ExitAction.Collect && scribe.RequestCurrentReceipt())
            notifications.ShowNotification(CollectedText);
        else if (action == ExitAction.Remind)
            notifications.ShowNotification(ReminderText);
    }
}
