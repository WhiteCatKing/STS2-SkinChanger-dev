using Microsoft.Win32;
using System.Diagnostics;
using System.Text.Json;

// 给 SlayTheSpire2.exe 开用户级 WER 本地转储：原生崩溃也会落 dump（不需要管理员）。
// 只在监视器运行期间生效：开启前备份用户原有的 LocalDumps 设置，退出时还原原值；
// 监视器被强杀时，下一次启动会先用备份还原，再重新开启，不会把用户的设置永久改掉。
internal static class WerDumpConfig
{
    private const string ExeName = "SlayTheSpire2.exe";
    private const string DefaultBaseKeyPath = @"Software\Microsoft\Windows\Windows Error Reporting\LocalDumps";
    private const string BackupValueName = "__watch_backup";
    private const string CreatedValueName = "__watch_created";

    // 1 = mini dump（只有栈和模块信息，几 MB）；2 = 完整内存。这里默认小转储，份数也只留 2 份。
    private const int DumpTypeMini = 1;
    private const int DumpCount = 2;

    private static bool _owned;

    public static string DumpFolder { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Sts2CrashWatch",
        "dumps");

    private sealed record SavedValue(string Name, string Kind, string? Text, int Number);

    private sealed record Backup(bool BaseExisted, bool AppExisted, List<SavedValue> Values);

    public static void Enable() => Enable(DefaultBaseKeyPath);

    public static void Disable() => Disable(DefaultBaseKeyPath);

    internal static void Enable(string baseKeyPath)
    {
        try
        {
            Directory.CreateDirectory(DumpFolder);

            // 上一次被强杀留下的备份：先还原，别把残留当成用户的原设置。
            RestoreFromBackup(baseKeyPath);

            var baseExisted = OpenBase(baseKeyPath) != null;
            var appExisted = false;
            var saved = new List<SavedValue>();

            using (var existingBase = OpenBase(baseKeyPath))
            {
                if (existingBase != null)
                {
                    using var existingApp = existingBase.OpenSubKey(ExeName);
                    if (existingApp != null)
                    {
                        appExisted = true;
                        saved.AddRange(CaptureValues(existingApp));
                    }
                }
            }

            using var baseKey = Registry.CurrentUser.CreateSubKey(baseKeyPath, writable: true);
            if (baseKey == null)
            {
                return;
            }

            using var appKey = baseKey.CreateSubKey(ExeName, writable: true);
            if (appKey == null)
            {
                return;
            }

            appKey.SetValue("DumpFolder", DumpFolder, RegistryValueKind.ExpandString);
            appKey.SetValue("DumpType", DumpTypeMini, RegistryValueKind.DWord);
            appKey.SetValue("DumpCount", DumpCount, RegistryValueKind.DWord);

            var backup = new Backup(baseExisted, appExisted, saved);
            baseKey.SetValue(BackupValueName, JsonSerializer.Serialize(backup), RegistryValueKind.String);
            if (!baseExisted)
            {
                baseKey.SetValue(CreatedValueName, "1", RegistryValueKind.String);
            }

            _owned = true;
            Console.WriteLine($"[转储] 已为用户级 WER 开启原生崩溃转储（小转储，最多 {DumpCount} 份）: {DumpFolder}");
        }
        catch (Exception exception)
        {
            Console.WriteLine("[转储] 开启 WER 转储失败（不影响其他检测）: " + exception.Message);
        }
    }

    internal static void Disable(string baseKeyPath)
    {
        if (!_owned)
        {
            return;
        }

        try
        {
            if (RestoreFromBackup(baseKeyPath))
            {
                Console.WriteLine("[转储] 已还原原有 WER 设置（保留本次生成的 dump 文件）。");
            }
        }
        catch
        {
            // 还原失败不阻塞退出；下一次启动还会再试一次。
        }
        finally
        {
            _owned = false;
        }
    }

    /// <summary>用备份还原（开启前的原值），并清掉我们写下的标记。返回是否真的还原了什么。</summary>
    private static bool RestoreFromBackup(string baseKeyPath)
    {
        using var baseKey = OpenBase(baseKeyPath, writable: true);
        if (baseKey == null)
        {
            return false;
        }

        Backup? backup = null;
        if (baseKey.GetValue(BackupValueName) is string json)
        {
            try
            {
                backup = JsonSerializer.Deserialize<Backup>(json);
            }
            catch (JsonException)
            {
                backup = null;
            }
        }

        if (backup == null)
        {
            baseKey.DeleteValue(BackupValueName, throwOnMissingValue: false);
            baseKey.DeleteValue(CreatedValueName, throwOnMissingValue: false);
            return false;
        }

        if (!backup.AppExisted)
        {
            baseKey.DeleteSubKeyTree(ExeName, throwOnMissingSubKey: false);
        }
        else
        {
            using (var appKey = baseKey.CreateSubKey(ExeName, writable: true))
            {
                if (appKey != null)
                {
                    var restored = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var value in backup.Values)
                    {
                        ApplyValue(appKey, value);
                        restored.Add(value.Name);
                    }

                    // 开启前不存在的值要删掉（我们的 DumpFolder/DumpType/DumpCount）。
                    foreach (var name in appKey.GetValueNames())
                    {
                        if (!restored.Contains(name))
                        {
                            appKey.DeleteValue(name, throwOnMissingValue: false);
                        }
                    }
                }
            }
        }

        baseKey.DeleteValue(BackupValueName, throwOnMissingValue: false);
        baseKey.DeleteValue(CreatedValueName, throwOnMissingValue: false);

        if (!backup.BaseExisted)
        {
            // 只在我们创建、且已经空了的情况下删除这个父键，别动别的程序的 LocalDumps 项。
            using var probe = OpenBase(baseKeyPath);
            if (probe != null && probe.SubKeyCount == 0)
            {
                try
                {
                    Registry.CurrentUser.DeleteSubKey(baseKeyPath, throwOnMissingSubKey: false);
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
        }

        return true;
    }

    private static List<SavedValue> CaptureValues(RegistryKey key)
    {
        var values = new List<SavedValue>();
        foreach (var name in key.GetValueNames())
        {
            try
            {
                var kind = key.GetValueKind(name);
                switch (kind)
                {
                    case RegistryValueKind.String:
                        values.Add(new SavedValue(name, "String", key.GetValue(name) as string, 0));
                        break;
                    case RegistryValueKind.ExpandString:
                        values.Add(new SavedValue(name, "ExpandString", key.GetValue(name) as string, 0));
                        break;
                    case RegistryValueKind.DWord:
                        values.Add(new SavedValue(name, "DWord", null, Convert.ToInt32(key.GetValue(name) ?? 0)));
                        break;
                    case RegistryValueKind.QWord:
                        values.Add(new SavedValue(name, "QWord", null, Convert.ToInt32(Math.Min(
                            Convert.ToInt64(key.GetValue(name) ?? 0), int.MaxValue))));
                        break;
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        return values;
    }

    private static void ApplyValue(RegistryKey key, SavedValue value)
    {
        switch (value.Kind)
        {
            case "String":
                key.SetValue(value.Name, value.Text ?? string.Empty, RegistryValueKind.String);
                break;
            case "ExpandString":
                key.SetValue(value.Name, value.Text ?? string.Empty, RegistryValueKind.ExpandString);
                break;
            case "DWord":
                key.SetValue(value.Name, value.Number, RegistryValueKind.DWord);
                break;
            case "QWord":
                key.SetValue(value.Name, (long)value.Number, RegistryValueKind.QWord);
                break;
        }
    }

    private static RegistryKey? OpenBase(string baseKeyPath, bool writable = false) =>
        Registry.CurrentUser.OpenSubKey(baseKeyPath, writable);
}
