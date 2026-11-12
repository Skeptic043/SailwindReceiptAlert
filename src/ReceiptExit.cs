namespace SailwindReceiptAlert;

internal enum ExitAction
{
    None,
    Remind,
    Collect
}

internal static class ReceiptExit
{
    // What leaving a desk area does. Collecting needs the trade receipts book,
    // so without it the player gets the reminder instead.
    internal static ExitAction Decide(bool enabled, bool receiptWaiting, bool autoCollect, bool receiptBookReady)
    {
        if (!enabled || !receiptWaiting)
            return ExitAction.None;
        return autoCollect && receiptBookReady ? ExitAction.Collect : ExitAction.Remind;
    }
}
