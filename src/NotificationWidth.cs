using HarmonyLib;
using UnityEngine;

namespace SailwindReceiptAlert;

// Receipt messages are longer than most notifications, so their parchment is
// drawn 20% wider. Every notification sets the width, so other messages always
// get the normal size back.
[HarmonyPatch(typeof(NotificationUi), nameof(NotificationUi.ShowNotification), typeof(string), typeof(float))]
internal static class NotificationWidth
{
    private const float ReceiptWidth = 1.2f;
    private static Vector3? normalScale;
    private static bool missingLogged;

    private static void Postfix(string notification, GameObject ___UI)
    {
        Transform parchment = ___UI.transform.Find("notification UI");
        if (!parchment)
        {
            if (!missingLogged)
                Plugin.Log.LogWarning("Could not find the notification parchment; receipt messages use the normal width.");
            missingLogged = true;
            return;
        }

        Vector3 normal = normalScale ??= parchment.localScale;
        bool receipt = notification == Plugin.ReminderText || notification == Plugin.CollectedText;
        parchment.localScale = receipt ? new Vector3(normal.x * ReceiptWidth, normal.y, normal.z) : normal;
    }
}
