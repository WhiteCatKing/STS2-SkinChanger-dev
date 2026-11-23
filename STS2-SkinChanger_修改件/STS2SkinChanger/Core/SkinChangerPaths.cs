using Godot;
using System.Runtime.CompilerServices;
using STS2SkinChanger.Pck;

namespace STS2SkinChanger.Core;

internal static class SkinChangerPaths
{
    private static readonly Lazy<SkinChangerStorage> Current = new(() =>
    {
        var storage = new SkinChangerStorage(OS.GetUserDataDir(), Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        PckArchive.WriteOutput = (path, bytes, write) => WritePckOutput(storage.PckBudget, path, bytes, write);
        try { storage.MigrateConfiguration(); }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        { ModLog.Warn("配置迁移未完成，将保留并尝试读取旧配置：" + e.Message); }
        foreach (var warning in storage.Warnings) ModLog.Warn(warning);
        try { storage.StartCacheSession(); }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        { ModLog.Warn("皮肤缓存目录无法打开，部分换肤可能不可用：" + e.Message); }
        ModLog.Info("配置目录：" + storage.RootDirectory);
        return storage;
    });

    internal static void WritePckOutput(CacheWriteBudget budget, string path, long bytes, Action write)
    {
        try { budget.Write(path, bytes, write); }
        catch (IOException error)
        {
            var detail = error.Message;
            if (error is CacheWriteRejectedException rejected)
                rejected.DisplayMessage = Ui.CacheWriteNotice.Report(rejected.Reason);
            ModLog.Warn("皮肤资源包写入已停止：" + detail);
            throw;
        }
    }
    public static SkinChangerStorage Storage => Current.Value;
    public static string Configuration(string name) => Storage.ConfigurationFile(name);
    public static string CacheDirectory => Storage.CacheDirectory;
    public static string WorkshopCacheDirectory => Path.Combine(CacheDirectory, "workshop");
    public static string RuntimeCacheDirectory => Path.Combine(CacheDirectory, "runtime");
    public static string GitHubCacheDirectory => Path.Combine(CacheDirectory, "github");
    // Recognized results must outlive a session: rebuilding them costs a request per repository.
    public static string GitHubCatalogPath => Configuration("github_skins.json");
    // Which folder belongs to which repository is written once, at install time, and has to
    // survive every catalog format change - it is the ownership record uninstall relies on.
    public static string GitHubInstallLedgerPath => Configuration("github_installs.json");
    public static void WriteCache(string path, Action write)
    {
        if (!Storage.WriteCache(path, write)) throw new OperationCanceledException("Skin Changer is quitting.");
    }
    internal static bool LoadResourcePack(GlobalWriteSite site, string path, bool replaceFiles,
        [CallerMemberName] string caller = "", bool throwOnBlock = false)
    {
        Storage.PckBudget.Protect(path);
        return GlobalResourceWrites.MountPack(site, path, replaceFiles, caller, throwOnBlock);
    }
    public static void Initialize() => _ = Storage;
}
