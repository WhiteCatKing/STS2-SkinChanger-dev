using System;
using System.IO;
using Godot;

namespace STS2SkinChanger.Core;

internal static class SkinChangerPaths
{
	private static readonly Lazy<SkinChangerStorage> Current = new Lazy<SkinChangerStorage>(delegate
	{
		SkinChangerStorage skinChangerStorage = new SkinChangerStorage(OS.GetUserDataDir(), Path.GetTempPath(), Guid.NewGuid().ToString("N"));
		try
		{
			skinChangerStorage.MigrateConfiguration();
		}
		catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException) ? 1 : 0) != 0)
		{
			ModLog.Warn("配置迁移未完成，将保留并尝试读取旧配置：" + ex.Message);
		}
		foreach (string warning in skinChangerStorage.Warnings)
		{
			ModLog.Warn(warning);
		}
		try
		{
			skinChangerStorage.StartCacheSession();
		}
		catch (Exception ex2) when (((ex2 is IOException || ex2 is UnauthorizedAccessException) ? 1 : 0) != 0)
		{
			ModLog.Warn("皮肤缓存目录无法打开，部分换肤可能不可用：" + ex2.Message);
		}
		ModLog.Info("配置目录：" + skinChangerStorage.RootDirectory);
		return skinChangerStorage;
	});

	public static SkinChangerStorage Storage => Current.Value;

	public static string CacheDirectory => Storage.CacheDirectory;

	public static string WorkshopCacheDirectory => Path.Combine(CacheDirectory, "workshop");

	public static string RuntimeCacheDirectory => Path.Combine(CacheDirectory, "runtime");

	public static string GitHubCacheDirectory => Path.Combine(CacheDirectory, "github");

	public static string GitHubCatalogPath => Configuration("github_skins.json");

	public static string GitHubInstallLedgerPath => Configuration("github_installs.json");

	public static string Configuration(string name)
	{
		return Storage.ConfigurationFile(name);
	}

	public static void WriteCache(string path, Action write)
	{
		if (!Storage.WriteCache(path, write))
		{
			throw new OperationCanceledException("Skin Changer is quitting.");
		}
	}

	public static void Initialize()
	{
		_ = Storage;
	}
}
