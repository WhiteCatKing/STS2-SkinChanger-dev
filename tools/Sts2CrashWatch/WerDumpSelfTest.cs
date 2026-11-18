using Microsoft.Win32;

// 离线自测：用一次性注册表路径验证「备份原值 → 开启 → 还原」和强退残留恢复，不碰用户真实的 LocalDumps。
internal static class WerDumpSelfTest
{
    private const string RootPath = @"Software\Sts2CrashWatchSelfTest";

    internal static int Run()
    {
        var failures = 0;
        try
        {
            failures += ExistingSettingsAreRestored();
            failures += MissingKeyIsRemoved();
            failures += LeftoverBackupIsRecovered();
        }
        finally
        {
            try { Registry.CurrentUser.DeleteSubKeyTree(RootPath, throwOnMissingSubKey: false); } catch { }
        }

        Console.WriteLine(failures == 0 ? "[自测] WER 备份/还原 通过。" : $"[自测] WER 备份/还原 失败 {failures} 项。");
        return failures == 0 ? 0 : 1;
    }

    private static int ExistingSettingsAreRestored()
    {
        var path = FreshPath();
        var failures = 0;
        using (var baseKey = Registry.CurrentUser.CreateSubKey(path, writable: true)!)
        using (var app = baseKey.CreateSubKey("SlayTheSpire2.exe", writable: true)!)
        {
            app.SetValue("DumpType", 2, RegistryValueKind.DWord);
            app.SetValue("DumpCount", 10, RegistryValueKind.DWord);
            app.SetValue("DumpFolder", @"C:\UserChosen", RegistryValueKind.ExpandString);
            app.SetValue("Extra", "keep", RegistryValueKind.String);
        }

        WerDumpConfig.Enable(path);
        failures += Require(ReadDword(path, "DumpType") == 1, "Enable 应改成小转储（DumpType=1）");
        failures += Require(ReadDword(path, "DumpCount") == 2, "Enable 应把份数降到 2");
        failures += Require(ReadString(path, "__watch_backup") != null, "Enable 应写入备份");

        WerDumpConfig.Disable(path);
        failures += Require(ReadDword(path, "DumpType") == 2, "Disable 应还原用户原来的 DumpType");
        failures += Require(ReadDword(path, "DumpCount") == 10, "Disable 应还原用户原来的 DumpCount");
        failures += Require(ReadAppString(path, "DumpFolder") == @"C:\UserChosen", "Disable 应还原用户原来的 DumpFolder");
        failures += Require(ReadAppString(path, "Extra") == "keep", "Disable 不应删掉用户自己的值");
        failures += Require(ReadString(path, "__watch_backup") == null, "Disable 应清掉备份标记");
        failures += Require(ReadString(path, "__watch_created") == null, "Disable 应清掉创建标记");
        return failures;
    }

    private static int MissingKeyIsRemoved()
    {
        var path = FreshPath();
        WerDumpConfig.Enable(path);
        var created = ReadString(path, "__watch_created") != null;
        WerDumpConfig.Disable(path);
        var stillThere = Registry.CurrentUser.OpenSubKey(path) != null;
        return Require(created, "父键原本不存在时应记下创建标记")
             + Require(!stillThere, "原本不存在的父键在 Disable 后应被删掉");
    }

    private static int LeftoverBackupIsRecovered()
    {
        // 模拟「监视器被强杀」：Enable 之后不调 Disable，直接再 Enable；
        // 第二次必须先用备份还原，而不是把我们的 1/2 当成用户原值再备份一遍。
        var path = FreshPath();
        var failures = 0;
        using (var baseKey = Registry.CurrentUser.CreateSubKey(path, writable: true)!)
        using (var app = baseKey.CreateSubKey("SlayTheSpire2.exe", writable: true)!)
        {
            app.SetValue("DumpType", 2, RegistryValueKind.DWord);
            app.SetValue("DumpCount", 10, RegistryValueKind.DWord);
        }

        WerDumpConfig.Enable(path);
        WerDumpConfig.Enable(path);
        var backup = ReadString(path, "__watch_backup") ?? string.Empty;
        var numbers = System.Text.Json.JsonDocument.Parse(backup).RootElement
            .GetProperty("Values").EnumerateArray()
            .Where(value => value.GetProperty("Name").GetString() is "DumpType" or "DumpCount")
            .Select(value => value.GetProperty("Number").GetInt32())
            .ToArray();
        failures += Require(numbers.Contains(2) && numbers.Contains(10), "残留恢复后备份里应是用户原来的 DumpType=2 / DumpCount=10");
        failures += Require(!numbers.Contains(1), "备份里不能出现我们写进去的 DumpType=1 / DumpCount=1");

        WerDumpConfig.Disable(path);
        failures += Require(ReadDword(path, "DumpType") == 2, "残留恢复后 Disable 仍应还原原值");
        return failures;
    }

    private static string FreshPath() => RootPath + @"\" + Guid.NewGuid().ToString("N");

    private static int? ReadDword(string path, string name)
    {
        using var key = Registry.CurrentUser.OpenSubKey(path + @"\SlayTheSpire2.exe");
        return key?.GetValue(name) is int value ? value : null;
    }

    private static string? ReadString(string path, string name)
    {
        using var key = Registry.CurrentUser.OpenSubKey(path);
        return key?.GetValue(name) as string;
    }

    private static string? ReadAppString(string path, string name)
    {
        using var key = Registry.CurrentUser.OpenSubKey(path + @"\SlayTheSpire2.exe");
        return key?.GetValue(name) as string;
    }

    private static int Require(bool condition, string message)
    {
        if (condition)
        {
            return 0;
        }

        Console.WriteLine("[自测] 失败: " + message);
        return 1;
    }
}
