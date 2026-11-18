using STS2SkinChanger.Core;
using STS2SkinChanger.Pck;

internal static class CacheWriteBudgetTests
{
    internal static void Run()
    {
        var root = Path.Combine(Path.GetTempPath(), "sc-budget-" + Guid.NewGuid().ToString("N"));
        var session = Path.Combine(root, "cache", "current");
        var overlay = Path.Combine(root, "overlay-cache");
        Directory.CreateDirectory(session);
        Directory.CreateDirectory(overlay);
        var previous = PckArchive.WriteOutput;
        try
        {
            var old = Path.Combine(overlay, "old.pck");
            File.WriteAllBytes(old, new byte[200]);
            File.SetLastAccessTimeUtc(old, DateTime.UtcNow.AddDays(-1));
            var budget = new CacheWriteBudget([Path.Combine(root, "cache"), overlay], 400);
            PckArchive.WriteOutput = budget.Write;
            var first = Path.Combine(session, "first.pck");
            var files = new Dictionary<string, byte[]> { ["res://test.bin"] = new byte[100] };
            PckArchive.Write(first, files);
            Require(File.Exists(first) && !File.Exists(old), "New PCK must reclaim an unused old overlay across roots.");
            using (var archive = PckArchive.Open(first))
                Require(archive.ReadFile("res://test.bin").Length == 100, "Budgeted output must remain a readable PCK.");
            var second = Path.Combine(overlay, "second.pck");
            var rejected = false;
            try { PckArchive.Write(second, files); }
            catch (IOException) { rejected = true; }
            Require(rejected && File.Exists(first) && !File.Exists(second),
                "The next write must reject before creating output when protected current files consume the budget.");
            // Reusing the same budget after a catalog reload retains the prior reservations.
            rejected = false;
            try { budget.Write(first, 1, () => File.WriteAllBytes(first, [0])); }
            catch (IOException) { rejected = true; }
            Require(rejected && new FileInfo(first).Length > 1, "A reserved native PCK name must survive reload without overwrite.");
            var failed = Path.Combine(session, "failed.pck");
            try { budget.Write(failed, 1, () => { File.WriteAllBytes(failed, [0]); throw new IOException("fixture"); }); }
            catch (IOException) { }
            Require(!File.Exists(failed), "A failed write must release its partial file and reservation.");
            budget.Write(failed, 1, () => File.WriteAllBytes(failed, [1]));
            Require(File.ReadAllBytes(failed)[0] == 1, "A failed reservation must be retryable.");
            CheckOtherSessionLease(root);
            CheckDiskSpace(root);
            Console.WriteLine("Shared cache budget passed: cross-root eviction, PCK entry point, protected files, reload lifetime, failed-write retry and live-session lease.");
        }
        finally
        {
            PckArchive.WriteOutput = previous;
            Directory.Delete(root, true);
        }
    }

    private static void CheckOtherSessionLease(string root)
    {
        var directory = Path.Combine(root, "leased");
        Directory.CreateDirectory(directory);
        var pack = Path.Combine(directory, "keep.pck");
        File.WriteAllBytes(pack, new byte[80]);
        using var lease = new FileStream(Path.Combine(directory, ".active"), FileMode.Create, FileAccess.ReadWrite, FileShare.None);
        var budget = new CacheWriteBudget([root], 80);
        var rejected = false;
        try { budget.Write(Path.Combine(root, "new.pck"), 1, () => throw new Exception("Writer must not run")); }
        catch (IOException) { rejected = true; }
        Require(rejected && File.Exists(pack), "Another active session's PCK must never be evicted.");
    }

    private static void Require(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
    }

    private static void CheckDiskSpace(string root)
    {
        var directory = Path.Combine(root, "space");
        Directory.CreateDirectory(directory);
        var output = Path.Combine(directory, "large.pck");
        var files = new Dictionary<string, byte[]> { ["res://large.bin"] = new byte[1024 * 1024] };
        long estimate = 0;
        var available = CacheWriteBudget.DiskReserve + 1024 * 1024;
        var budget = new CacheWriteBudget([directory], availableBytes: _ => available);
        PckArchive.WriteOutput = (path, bytes, write) => { estimate = bytes; budget.Write(path, bytes, write); };
        var rejected = false;
        try { PckArchive.Write(output, files); }
        catch (IOException) { rejected = true; }
        Require(rejected && !File.Exists(output) && estimate > files.Values.Sum(data => data.LongLength),
            "Disk preflight must include PCK header/alignment and reject before creating a partial pack.");
        available = CacheWriteBudget.DiskReserve + estimate;
        PckArchive.Write(output, files);
        Require(new FileInfo(output).Length <= estimate, "Actual PCK length must fit the reserved layout size.");
        using var archive = PckArchive.Open(output);
        var copied = Path.Combine(directory, "copy.pck");
        available = CacheWriteBudget.DiskReserve;
        rejected = false;
        try { PckArchive.WriteFromArchives(copied, new Dictionary<string, (PckArchive, string)>
            { ["res://large.bin"] = (archive, "res://large.bin") }); }
        catch (IOException) { rejected = true; }
        Require(rejected && !File.Exists(copied), "Archive streaming must use the same preflight before opening output.");
        var outside = Path.Combine(root, "outside-space.pck");
        rejected = false;
        try { PckArchive.Write(outside, files); }
        catch (IOException) { rejected = true; }
        Require(rejected && !File.Exists(outside), "PCKs outside the persistent cache still need disk-space preflight.");
        Console.WriteLine("PCK disk preflight passed: payload plus layout, exact free-space boundary, archive streaming and external output paths.");
    }
}
