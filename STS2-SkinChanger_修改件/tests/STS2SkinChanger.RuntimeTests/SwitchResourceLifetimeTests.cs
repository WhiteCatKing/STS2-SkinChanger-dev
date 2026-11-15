using System.Reflection;
using HarmonyLib;
using STS2SkinChanger.Core;

internal static class SwitchResourceLifetimeTests
{
    private static SkinChangerStorage _storage = null!;
    private static int _mounts;
    private sealed class Lease : IDisposable
    {
        internal int Disposals;
        public void Dispose() => Disposals++;
    }

    internal static void Run()
    {
        var root = Path.Combine(Path.GetTempPath(), "SC-Switch-Lifetime-" + Guid.NewGuid().ToString("N"));
        _storage = new SkinChangerStorage(root, root, "test");
        var harmony = new Harmony("sc.tests.switch-resource-lifetime");
        try
        {
            harmony.Patch(AccessTools.PropertyGetter(typeof(SkinChangerPaths), "Storage"), prefix: Patch(nameof(Storage)));
            foreach (var name in new[] { "Info", "Warn" })
                harmony.Patch(AccessTools.Method(typeof(ModLog), name), prefix: Patch(nameof(Skip)));
            harmony.Patch(AccessTools.Method(typeof(SkinService), "MountOverlay"), prefix: Patch(nameof(Mount)));
            var type = typeof(SkinService).GetNestedType("MountedRuntimeResourceScope", BindingFlags.NonPublic)!;
            for (var i = 0; i < 500; i++)
            {
                var lease = new Lease();
                var scope = (IDisposable)Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null, [new HashSet<string>(), lease], null)!;
                scope.Dispose(); scope.Dispose();
                Require(lease.Disposals == 1, "Scope disposal must be idempotent.");
            }
            Require(_mounts == 0, "Empty restore scopes must not enter MountOverlay.");
            var foreign = (IDisposable)Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null, [new HashSet<string> { "regent" }, new Lease()], null)!;
            foreign.Dispose(); foreign.Dispose();
            Require(_mounts == 1, "Foreign restore must run exactly once.");

            OverlayCacheStore.BeginSession("switch-test");
            var pinned = OverlayCacheStore.ReservePath("abc");
            File.WriteAllBytes(pinned, new byte[10]);
            var stale = Path.Combine(OverlayCacheStore.DirectoryPath, "unmounted.pck");
            File.WriteAllBytes(stale, new byte[10]);
            AccessTools.Method(typeof(OverlayCacheStore), "TrimToLimit").Invoke(null, [10L]);
            Require(File.Exists(pinned) && !File.Exists(stale), "Trim must preserve pinned archives and remove unused archives.");
            File.Delete(pinned); // simulate external removal; never recreate a mounted native name
            var replacement = OverlayCacheStore.ReservePath("abc");
            Require(!replacement.Equals(pinned, StringComparison.OrdinalIgnoreCase), "A previously pinned name must never be reused.");
            File.WriteAllBytes(replacement, new byte[10]);
            AccessTools.Method(typeof(OverlayCacheStore), "TrimToLimit").Invoke(null, [0L]);
            Require(File.Exists(replacement), "Even budget pressure must not remove an active archive.");
            Console.WriteLine("Switch resource lifetime passed: 500 actual scope disposals, foreign restore, pinned-file pruning and no native-name reuse.");
        }
        finally
        {
            harmony.UnpatchAll(harmony.Id);
            _storage.Dispose();
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    private static HarmonyMethod Patch(string name) => new(typeof(SwitchResourceLifetimeTests), name);
    private static bool Storage(ref SkinChangerStorage __result) { __result = _storage; return false; }
    private static bool Skip() => false;
    private static bool Mount() { _mounts++; return false; }
    private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
}
