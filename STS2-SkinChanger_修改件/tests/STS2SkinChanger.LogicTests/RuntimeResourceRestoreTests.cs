using STS2SkinChanger.Core;

internal static class RuntimeResourceRestoreTests
{
    internal static void Run()
    {
        for (var i = 0; i < 500; i++)
        {
            var selection = i % 2 == 0 ? "MeirinWatcherSkin" : "__base__";
            Require(RuntimeResourceRestorePolicy.GetGroups("watcher", [], null, selection).Count == 0,
                "A local preview must not remount its own selection after creating a live model.");
            Require(RuntimeResourceRestorePolicy.GetGroups("watcher", [], selection, selection).Count == 0,
                "An identical multiplayer scope also needs no self restore.");
        }
        Require(RuntimeResourceRestorePolicy.GetGroups("watcher", ["regent"], null, "__base__").SetEquals(["regent"]),
            "Foreign groups touched by a binary dependency must still be restored.");
        Require(RuntimeResourceRestorePolicy.GetGroups("watcher", ["regent"], "remote-skin", "__base__")
            .SetEquals(["regent", "watcher"]), "A different remote choice must restore local and foreign groups.");
        Console.WriteLine("Runtime resource restore passed: 500 local switches have no self remount; remote and foreign restores retained.");
    }

    private static void Require(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
    }
}
