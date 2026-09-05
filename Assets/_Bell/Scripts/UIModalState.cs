using UnityEngine;

/// <summary>
/// Tiny shared flag so systems that don't otherwise reference each other —
/// PlayerInputHandler's idle-quip timer, PackPanelController, and
/// JournalPanelController — can agree on whether a fullscreen/panel UI is
/// currently up. Counted rather than a plain bool so two panels closing in
/// the wrong order can't leave it stuck open.
/// </summary>
public static class UIModalState
{
    private static int _openCount;

    public static bool IsAnyModalOpen => _openCount > 0;

    public static void NotifyOpened() => _openCount++;

    public static void NotifyClosed() => _openCount = Mathf.Max(0, _openCount - 1);
}
