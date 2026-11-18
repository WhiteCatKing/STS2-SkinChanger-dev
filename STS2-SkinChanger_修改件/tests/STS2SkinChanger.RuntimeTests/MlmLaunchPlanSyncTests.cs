using System.Text;
using STS2SkinChanger.Core;

internal static class MlmLaunchPlanSyncTests
{
    internal static void Run()
    {
        var root = Path.Combine(Path.GetTempPath(), "sc mlm atomic " + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var plan = Path.Combine(root, "mod_launch_last_plan.json");
            File.WriteAllText(plan, "{\"version\":1,\"entries\":[]}", new UTF8Encoding(false));

            MlmLaunchPlanSync.ReplaceFileAtomically(plan, "{\"version\":1,\"entries\":[{\"id\":\"a\"}]}\n");
            Require(File.ReadAllText(plan).Contains("\"a\""), "Atomic replace must install the new plan content.");
            Require(!Directory.EnumerateFiles(root, "*.tmp").Any(), "A successful replace must not leave temp files behind.");

            // 目标不可替换（这里用目录当目标）时：原文件不受影响，临时文件要清掉。
            var blocked = Path.Combine(root, "blocked.json");
            File.WriteAllText(blocked, "original", new UTF8Encoding(false));
            var directoryTarget = Path.Combine(root, "target-dir");
            Directory.CreateDirectory(directoryTarget);
            var failed = false;
            try
            {
                MlmLaunchPlanSync.ReplaceFileAtomically(directoryTarget, "payload");
            }
            catch (IOException)
            {
                failed = true;
            }
            catch (UnauthorizedAccessException)
            {
                failed = true;
            }

            Require(failed, "Replacing a directory must fail.");
            Require(Directory.Exists(directoryTarget), "A failed replace must not touch the target.");
            Require(!Directory.EnumerateFiles(root, "*.tmp").Any(), "A failed replace must clean its temp file.");
            Require(File.ReadAllText(blocked) == "original", "A failed replace must not modify other files.");

            MlmLaunchPlanSync.ReplaceFileAtomically(blocked, "new-content");
            Require(File.ReadAllText(blocked) == "new-content", "A normal file must be replaced with the new payload.");
            Console.WriteLine("MLM launch plan atomic save passed.");
        }
        finally
        {
            try { Directory.Delete(root, true); } catch { }
        }
    }

    private static void Require(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
    }
}
