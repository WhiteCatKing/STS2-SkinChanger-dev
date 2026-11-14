using Microsoft.Win32;
using System.Diagnostics;

// 给 SlayTheSpire2.exe 开用户级 WER 本地转储：原生崩溃也会落 dump（不需要管理员）。
// 只在监视器运行期间生效，退出时移除自己创建的项，不污染系统设置。
internal static class WerDumpConfig
{
    private const string ExeName = "SlayTheSpire2.exe";

    private static readonly string[] BaseKeys =
    [
        @"Software\Microsoft\Windows\Windows Error Reporting\LocalDumps",
        @"Software\Microsoft\Windows\Windows Error Reporting\LocalDumps\" + ExeName
    ];

    private static bool _owned;

    public static string DumpFolder { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Sts2CrashWatch",
        "dumps");

    public static void Enable()
    {
        try
        {
            Directory.CreateDirectory(DumpFolder);

            using var baseKey = Registry.CurrentUser.CreateSubKey(BaseKeys[0], writable: true);
            if (baseKey == null)
            {
                return;
            }

            // 记录是否由我们创建（原本不存在则退出时删除）
            var existing = baseKey.GetValue("__watch_created") as string;
            baseKey.SetValue("__watch_created", "1", RegistryValueKind.String);

            using var appKey = baseKey.CreateSubKey(ExeName, writable: true);
            if (appKey == null)
            {
                return;
            }

            appKey.SetValue("DumpFolder", DumpFolder, RegistryValueKind.ExpandString);
            appKey.SetValue("DumpType", 2, RegistryValueKind.DWord);   // 2 = 完整转储
            appKey.SetValue("DumpCount", 10, RegistryValueKind.DWord);
            _owned = existing == null;
            Console.WriteLine("[转储] 已为用户级 WER 开启原生崩溃转储: " + DumpFolder);
        }
        catch (Exception exception)
        {
            Console.WriteLine("[转储] 开启 WER 转储失败（不影响其他检测）: " + exception.Message);
        }
    }

    public static void Disable()
    {
        if (!_owned)
        {
            return;
        }

        try
        {
            using var baseKey = Registry.CurrentUser.OpenSubKey(BaseKeys[0], writable: true);
            if (baseKey == null)
            {
                return;
            }

            baseKey.DeleteSubKeyTree(ExeName, throwOnMissingSubKey: false);
            baseKey.DeleteValue("__watch_created", throwOnMissingValue: false);
            Console.WriteLine("[转储] 已还原 WER 设置（保留本次生成的 dump 文件）。");
        }
        catch
        {
            // ignore
        }
    }
}
