namespace STS2SkinChanger.Core;

internal sealed record MixedCardArtDirectoryConflict(
    string DirectoryPath,
    string FolderName,
    IReadOnlyList<string> ActiveModNames,
    IReadOnlyList<string> UnloadedModNames);

internal sealed class MixedCardArtDirectoryNoticeState
{
    private readonly object _sync = new();
    private IReadOnlyList<MixedCardArtDirectoryConflict> _pending = [];
    private bool _mainMenuReached;
    private bool _shownThisLaunch;

    internal void Report(IReadOnlyList<MixedCardArtDirectoryConflict> conflicts)
    {
        if (conflicts.Count == 0)
        {
            return;
        }

        lock (_sync)
        {
            if (_shownThisLaunch)
            {
                return;
            }

            _pending = conflicts.ToArray();
        }
    }

    internal void MarkMainMenuReached()
    {
        lock (_sync)
        {
            _mainMenuReached = true;
        }
    }

    internal bool TryTakeForMainMenu(
        out IReadOnlyList<MixedCardArtDirectoryConflict> conflicts,
        bool deferDisplay = false,
        bool suppressDisplay = false)
    {
        lock (_sync)
        {
            if (!_mainMenuReached || _shownThisLaunch || _pending.Count == 0 ||
                deferDisplay || suppressDisplay)
            {
                conflicts = [];
                return false;
            }

            conflicts = _pending;
            _pending = [];
            _shownThisLaunch = true;
            return true;
        }
    }

    internal void RestoreForRetry(IReadOnlyList<MixedCardArtDirectoryConflict> conflicts)
    {
        if (conflicts.Count == 0)
        {
            return;
        }

        lock (_sync)
        {
            _pending = conflicts.ToArray();
            _shownThisLaunch = false;
        }
    }

    internal void Reset()
    {
        lock (_sync)
        {
            _pending = [];
            _mainMenuReached = false;
            _shownThisLaunch = false;
        }
    }
}
