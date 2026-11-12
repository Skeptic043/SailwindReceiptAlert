using System;
using HarmonyLib;
using UnityEngine;

namespace SailwindReceiptAlert;

// A trigger around each port's trade and mission desk. Leaving it hands off to
// Plugin.OnLeftDesk.
public sealed class ReceiptZone : MonoBehaviour
{
    // World metres, independent of the desk's own scale.
    private static readonly Vector3 Size = new Vector3(12f, 4f, 12f);
    private static readonly Vector3 Offset = new Vector3(0f, 0.5f, 0f);

    internal static void Attach(Transform desk)
    {
        GameObject zone = new GameObject("Receipt Alert Zone");
        zone.SetActive(false);
        zone.transform.SetParent(desk, false);

        Vector3 scale = desk.lossyScale;
        zone.transform.localPosition = new Vector3(Offset.x / scale.x, Offset.y / scale.y, Offset.z / scale.z);
        zone.transform.localScale = new Vector3(1f / scale.x, 1f / scale.y, 1f / scale.z);

        // A kinematic body makes the trigger fire even where the desk has no Rigidbody.
        Rigidbody body = zone.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;

        BoxCollider trigger = zone.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = Size;

        zone.AddComponent<ReceiptZone>();
        zone.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        // PortDude identifies the player collider by the same tag.
        if (other.CompareTag("Player"))
            Plugin.OnLeftDesk();
    }
}

[HarmonyPatch(typeof(PortDude), "Awake")]
internal static class PortDudeAwakePatch
{
    private static void Postfix(PortDude __instance)
    {
        // Keep a failure here from interrupting the game's own desk setup.
        try
        {
            ReceiptZone.Attach(__instance.transform);
        }
        catch (Exception error)
        {
            Plugin.Log.LogWarning($"Could not add the receipt zone at {__instance.name}: {error}");
        }
    }
}
