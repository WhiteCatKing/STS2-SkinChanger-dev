namespace STS2SkinChanger.Core;

internal static class QuitExitCleanup
{
	public static string Clean(SkinChangerStorage storage)
	{
		storage.StopCacheWrites();
		CacheCleanupReport cacheCleanupReport = storage.CleanCaches();
		int num = cacheCleanupReport.Files.Count + cacheCleanupReport.Directories.Count;
		return $"退出缓存清理：已删除 {cacheCleanupReport.RemovedFiles} 个文件；" + ((num + cacheCleanupReport.Errors.Count == 0) ? "清理完成。" : $"未完成的 {num} 项缓存清理将在下次启动重试。") + ((cacheCleanupReport.Errors.Count == 0) ? "" : ("部分目录无法检查：" + string.Join("；", cacheCleanupReport.Errors)));
	}
}
