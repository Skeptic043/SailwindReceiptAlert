using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;

namespace SailwindReceiptAlert.Tests;

internal static class Program
{
    private static int failures;

    private static void Check(string name, bool condition)
    {
        Console.WriteLine((condition ? "PASS: " : "FAIL: ") + name);
        if (!condition) failures++;
    }

    private static int Main()
    {
        ExitDecisions();
        GameHooks();
        Console.WriteLine(failures == 0 ? "All checks passed." : $"{failures} check(s) failed.");
        return failures == 0 ? 0 : 1;
    }

    private static void ExitDecisions()
    {
        Check("no receipt waiting does nothing",
            ReceiptExit.Decide(enabled: true, receiptWaiting: false, autoCollect: true, receiptBookReady: true) == ExitAction.None);
        Check("disabled does nothing",
            ReceiptExit.Decide(enabled: false, receiptWaiting: true, autoCollect: true, receiptBookReady: true) == ExitAction.None);
        Check("waiting receipt gets a reminder",
            ReceiptExit.Decide(enabled: true, receiptWaiting: true, autoCollect: false, receiptBookReady: true) == ExitAction.Remind);
        Check("auto-collect collects",
            ReceiptExit.Decide(enabled: true, receiptWaiting: true, autoCollect: true, receiptBookReady: true) == ExitAction.Collect);
        Check("auto-collect without the receipt book reminds instead",
            ReceiptExit.Decide(enabled: true, receiptWaiting: true, autoCollect: true, receiptBookReady: false) == ExitAction.Remind);
    }

    // Members the plugin binds by name at runtime. Members it calls directly are
    // already checked by the plugin build against the same game assembly.
    private static void GameHooks()
    {
        string managed = typeof(Program).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(a => a.Key == "GameManagedDir").Value!;
        using ModuleDefinition game = ModuleDefinition.ReadModule(Path.Combine(managed, "Assembly-CSharp.dll"));

        TypeDefinition? portDude = game.GetType("PortDude");
        Check("PortDude.Awake() exists",
            portDude?.Methods.Any(m => m.Name == "Awake" && !m.HasParameters) == true);

        TypeDefinition? notifications = game.GetType("NotificationUi");
        Check("NotificationUi.ShowNotification(string, float) exists",
            notifications?.Methods.Any(m => m.Name == "ShowNotification" && m.Parameters.Count == 2 &&
                m.Parameters[0].ParameterType.FullName == "System.String" &&
                m.Parameters[1].ParameterType.FullName == "System.Single") == true);
        Check("NotificationUi.UI is a GameObject field",
            notifications?.Fields.Any(f => f.Name == "UI" && f.FieldType.FullName == "UnityEngine.GameObject") == true);
    }
}
