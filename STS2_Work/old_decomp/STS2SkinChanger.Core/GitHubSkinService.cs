using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using STS2SkinChanger.Catalog;

namespace STS2SkinChanger.Core;

internal static class GitHubSkinService
{
	private sealed record StagedPackage(string Directory, SkinModDescriptor[] Descriptors, string Digest)
	{
		public SkinModDescriptor? Select(string repoName)
		{
			int num = GitHubPackageLayout.SelectIndex(Descriptors.Select((SkinModDescriptor mod) => mod.RootPath).ToArray(), Directory, repoName);
			if (num >= 0)
			{
				return Descriptors[num];
			}
			return null;
		}
	}

	private const int MaxInfoChecksPerRefresh = 60;

	private const int MaxReleaseChecksPerRefresh = 10;

	private static readonly GitHubSkinCatalog Catalog = new GitHubSkinCatalog();

	private static readonly GitHubInstallLedger Ledger = new GitHubInstallLedger();

	private static readonly HashSet<ulong> Installing = new HashSet<ulong>();

	private static readonly HashSet<ulong> Scanning = new HashSet<ulong>();

	private static readonly HashSet<string> RestartPending = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static GitHubSkinClient? _client;

	private static WorkshopCatalogItem[] _items = Array.Empty<WorkshopCatalogItem>();

	private static IReadOnlyDictionary<ulong, GitHubSkinEntry> _entriesById = new Dictionary<ulong, GitHubSkinEntry>();

	private static IReadOnlyDictionary<ulong, GitHubInstall> _installsById = new Dictionary<ulong, GitHubInstall>();

	public static int Revision { get; private set; }

	public static bool Busy { get; private set; }

	public static bool Failed { get; private set; }

	public static bool RateLimited { get; private set; }

	public static string Progress { get; private set; } = "";

	public static WorkshopCatalogItem[] Items => _items;

	public static IReadOnlyDictionary<ulong, GitHubSkinEntry> EntriesById => _entriesById;

	public static GitHubSkinEntry? Entry(ulong id)
	{
		return _entriesById.GetValueOrDefault(id);
	}

	public static string Title(ulong id)
	{
		return Entry(id)?.Name ?? "";
	}

	public static string CoverUrl(ulong id)
	{
		return Entry(id)?.CoverUrl ?? "";
	}

	public static string RepoUrl(ulong id)
	{
		return Entry(id)?.Repo.Url ?? "";
	}

	public static string Description(ulong id)
	{
		return Entry(id)?.Description ?? "";
	}

	public static string Readme(ulong id)
	{
		return Entry(id)?.Readme ?? "";
	}

	public static bool IsInstalling(ulong id)
	{
		return Installing.Contains(id);
	}

	public static bool IsScanning(ulong id)
	{
		return Scanning.Contains(id);
	}

	public static bool IsRecognized(ulong id)
	{
		GitHubSkinEntry gitHubSkinEntry = Entry(id);
		if ((object)gitHubSkinEntry != null)
		{
			return gitHubSkinEntry.Recognition == GitHubRecognition.Recognized;
		}
		return false;
	}

	public static bool IsPendingRestart(ulong id)
	{
		GitHubSkinEntry gitHubSkinEntry = Entry(id);
		if ((object)gitHubSkinEntry != null && InstallState(id) != 0)
		{
			return RestartPending.Contains(gitHubSkinEntry.FullName);
		}
		return false;
	}

	public static GitHubInstallState InstallState(ulong id)
	{
		if (!_installsById.TryGetValue(id, out GitHubInstall value))
		{
			return GitHubInstallState.NotInstalled;
		}
		return GitHubInstallPolicy.Classify(value.Version, value.Digest, Entry(id)?.ReleaseTag, Entry(id)?.AssetDigest);
	}

	private static GitHubSkinClient Client()
	{
		return _client ?? (_client = new GitHubSkinClient(new HttpClientHandler
		{
			AllowAutoRedirect = false
		}, "STS2-SkinChanger/1.0.19.0"));
	}

	public static void StartRefresh()
	{
		if (!Busy)
		{
			RefreshAsync(restore: true);
		}
	}

	public static async Task RefreshAsync(bool restore)
	{
		if (Busy)
		{
			return;
		}
		Busy = true;
		Failed = false;
		RateLimited = false;
		try
		{
			if (restore)
			{
				try
				{
					Catalog.Restore(SkinChangerPaths.GitHubCatalogPath);
				}
				catch (Exception ex)
				{
					ModLog.Warn("GitHub 皮肤缓存不可用，将重新拉取：" + ex.Message);
				}
				try
				{
					Ledger.Restore(SkinChangerPaths.GitHubInstallLedgerPath);
				}
				catch (Exception ex2)
				{
					ModLog.Warn("GitHub 安装记录不可用，将重新建立：" + ex2.Message);
				}
				RefreshInstalls();
				ReconcileFolders();
				CleanLeftovers();
				RefreshInstalls();
				BackfillDigests();
				PublishItems();
			}
			Client();
			using CancellationTokenSource timeout = new CancellationTokenSource(TimeSpan.FromMinutes(30L));
			CancellationToken token = timeout.Token;
			GitHubRepoInfo[] source = await FetchReposAsync(token);
			Catalog.Merge(source.Select(GitHubSkinCatalog.FromRepo));
			RefreshInstalls();
			PublishItems();
			SaveCatalog();
			await CheckInfoAsync(token);
			await CheckReleasesAsync(token);
			BackfillDigests();
			ModLog.Info($"GitHub 皮肤列表刷新完成：{Catalog.Entries.Length} 个仓库，已识别 {Catalog.Entries.Count((GitHubSkinEntry entry) => entry.Recognition == GitHubRecognition.Recognized)} 个，本地已安装 {Ledger.Count} 个。");
		}
		catch (GitHubRateLimitedException ex3)
		{
			RateLimited = true;
			Failed = true;
			ModLog.Warn("GitHub 接口受限，保留缓存列表：" + ex3.Message);
		}
		catch (Exception ex4)
		{
			Failed = true;
			ModLog.Warn("GitHub 皮肤列表刷新失败，保留缓存列表：" + ex4.GetBaseException().Message);
		}
		finally
		{
			Busy = false;
			Progress = "";
		}
	}

	private static async Task<GitHubRepoInfo[]> FetchReposAsync(CancellationToken token)
	{
		List<GitHubRepoInfo> result = new List<GitHubRepoInfo>();
		for (int page = 1; page <= 3; page++)
		{
			Progress = $"{page}/{3}";
			GitHubRepoInfo[] array = GitHubSkinPolicy.ParseSearch(await Client().SearchPageAsync(page, token));
			result.AddRange(array);
			if (array.Length < 100)
			{
				break;
			}
		}
		return result.DistinctBy<GitHubRepoInfo, string>((GitHubRepoInfo info) => info.Repo.FullName, StringComparer.OrdinalIgnoreCase).ToArray();
	}

	private static async Task CheckInfoAsync(CancellationToken token)
	{
		GitHubSkinEntry[] pending = (from entry in Catalog.Entries.Where(delegate(GitHubSkinEntry entry)
			{
				GitHubRecognition recognition = entry.Recognition;
				return (recognition == GitHubRecognition.Pending || recognition == GitHubRecognition.Failed) ? true : false;
			})
			orderby entry.PushedAt descending
			select entry).Take(60).ToArray();
		if (pending.Length == 0)
		{
			return;
		}
		for (int index = 0; index < pending.Length; index++)
		{
			token.ThrowIfCancellationRequested();
			Progress = $"{index + 1}/{pending.Length}";
			GitHubSkinEntry entry2 = pending[index];
			try
			{
				GitHubSkinEntry entry3 = entry2;
				string info = await Client().InfoFileAsync(entry2.Repo, token);
				Catalog.Publish(Recognize(entry3, info));
			}
			catch (GitHubRateLimitedException)
			{
				RateLimited = true;
				throw;
			}
			catch (Exception ex2)
			{
				string message = ex2.GetBaseException().Message;
				Catalog.Publish(entry2 with
				{
					Recognition = GitHubRecognition.Failed,
					Error = message.Substring(0, Math.Min(512, message.Length))
				});
			}
			PublishItems();
			SaveCatalog();
		}
	}

	private static GitHubSkinEntry Recognize(GitHubSkinEntry entry, string? info)
	{
		GitHubInfoResult gitHubInfoResult = GitHubInfoPolicy.Read(info, entry.Id);
		WorkshopCatalogItem item = gitHubInfoResult.Item;
		if ((object)item == null)
		{
			return entry with
			{
				Recognition = GitHubRecognition.Unrecognized,
				Error = gitHubInfoResult.Error,
				Targets = Array.Empty<WorkshopTarget>(),
				RestartRequired = true
			};
		}
		return entry with
		{
			Recognition = GitHubRecognition.Recognized,
			Error = "",
			Targets = item.Targets,
			RestartRequired = item.RestartRequired
		};
	}

	private static async Task CheckReleasesAsync(CancellationToken token)
	{
		GitHubSkinEntry[] wanted = GitHubReleasePolicy.ReleaseChecks(Catalog.Entries, (ulong id) => _installsById.ContainsKey(id), 10);
		for (int index = 0; index < wanted.Length; index++)
		{
			token.ThrowIfCancellationRequested();
			Progress = $"{index + 1}/{wanted.Length}";
			GitHubSkinEntry entry = wanted[index];
			try
			{
				GitHubSkinEntry entry2 = await WithReleaseAsync(entry, token);
				Catalog.Publish(entry2);
			}
			catch (GitHubRateLimitedException)
			{
				RateLimited = true;
				throw;
			}
			catch (Exception ex2)
			{
				ModLog.Info("GitHub 版本检查跳过（" + entry.FullName + "）：" + ex2.GetBaseException().Message);
			}
			PublishItems();
			SaveCatalog();
		}
	}

	private static async Task<GitHubSkinEntry> WithReleaseAsync(GitHubSkinEntry entry, CancellationToken token)
	{
		return GitHubReleasePolicy.WithRelease(entry, await Client().LatestReleaseAsync(entry.Repo, token));
	}

	private static async Task<StagedPackage> StageAsync(GitHubSkinEntry entry, CancellationToken token)
	{
		GitHubSkinEntry gitHubSkinEntry = await WithReleaseAsync(entry, token);
		Catalog.Publish(gitHubSkinEntry);
		PublishItems();
		SaveCatalog();
		if (gitHubSkinEntry.AssetUrl.Length == 0)
		{
			throw new InvalidOperationException("This release has no downloadable zip package.");
		}
		string text = Path.Combine(SkinChangerPaths.GitHubCacheDirectory, entry.Id.ToString());
		string package = Path.Combine(text, "package");
		try
		{
			if (Directory.Exists(package))
			{
				Directory.Delete(package, recursive: true);
			}
			Directory.CreateDirectory(text);
		}
		catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException) ? 1 : 0) != 0)
		{
			throw new InvalidOperationException("The download folder is unavailable: " + ex.Message, ex);
		}
		string zip = Path.Combine(text, "package.zip");
		string declared = GitHubSkinPolicy.NormalizeDigest(gitHubSkinEntry.AssetDigest);
		long num = (File.Exists(zip) ? new FileInfo(zip).Length : (-1));
		string text2 = ((num == gitHubSkinEntry.AssetSize) ? GitHubInstallPolicy.DigestOfFile(zip) : "");
		string text3 = (GitHubInstallPolicy.CanReusePackage(declared, gitHubSkinEntry.AssetSize, num, text2) ? text2 : "");
		if (text3.Length == 0)
		{
			await Client().DownloadAsync(new Uri(gitHubSkinEntry.AssetUrl), zip, token);
			text3 = GitHubInstallPolicy.DigestOfFile(zip);
		}
		if (declared.Length > 0 && !text3.Equals(declared, StringComparison.OrdinalIgnoreCase))
		{
			TryDeleteFile(zip);
			throw new InvalidDataException("下载的文件与 GitHub 公布的校验值不一致，请稍后重试。");
		}
		Extract(zip, package);
		return new StagedPackage(package, WorkshopSubmissionScanner.ReadDescriptors(package), text3);
	}

	private static void Extract(string zipPath, string destination)
	{
		Directory.CreateDirectory(destination);
		using ZipArchive zipArchive = ZipFile.OpenRead(zipPath);
		long num = 0L;
		int num2 = 0;
		string fullPath = Path.GetFullPath(destination);
		foreach (ZipArchiveEntry entry in zipArchive.Entries)
		{
			string relative;
			switch (GitHubZipPolicy.Classify(entry.FullName, entry.Length, num2++, num, out relative))
			{
			case GitHubZipEntry.Reject:
				throw new InvalidDataException("Unsafe archive entry: " + entry.FullName);
			case GitHubZipEntry.Skip:
				continue;
			}
			num += entry.Length;
			string fullPath2 = Path.GetFullPath(Path.Combine(fullPath, relative));
			if (!fullPath2.StartsWith(fullPath + Path.DirectorySeparatorChar, StringComparison.Ordinal))
			{
				throw new InvalidDataException("Archive entry escapes the staging folder.");
			}
			Directory.CreateDirectory(Path.GetDirectoryName(fullPath2));
			entry.ExtractToFile(fullPath2, overwrite: true);
		}
	}

	public static async Task<string> ReadmeAsync(ulong id, CancellationToken token)
	{
		GitHubSkinEntry entry = Entry(id);
		if ((object)entry == null || entry.Readme.Length > 0)
		{
			return Readme(id);
		}
		try
		{
			string text = GitHubReadmePolicy.ToExcerpt(await Client().ReadmeAsync(entry.Repo, token), 800);
			if (text.Length == 0)
			{
				return "";
			}
			Catalog.Publish(entry with
			{
				Readme = text
			});
			RefreshEntries();
			SaveCatalog();
			return text;
		}
		catch (OperationCanceledException)
		{
			return "";
		}
		catch (Exception ex2)
		{
			ModLog.Info("GitHub 仓库简介不可用（" + entry.FullName + "）：" + ex2.GetBaseException().Message);
			return "";
		}
	}

	public static async Task<GitHubScanResult> ScanAsync(ulong id, CancellationToken token)
	{
		GitHubSkinEntry entry = Entry(id) ?? throw new InvalidOperationException("Unknown GitHub repository.");
		if (!Scanning.Add(id))
		{
			return new GitHubScanResult(null, Array.Empty<string>(), "busy");
		}
		string root = Path.Combine(SkinChangerPaths.GitHubCacheDirectory, entry.Id.ToString());
		try
		{
			StagedPackage staged = await StageAsync(entry, token);
			SkinModDescriptor descriptor = staged.Select(entry.RepoName);
			WorkshopScanResult workshopScanResult = await SkinService.RunWorkshopScan(() => Scan(entry, staged.Directory, descriptor), token);
			if (workshopScanResult.Item == null)
			{
				string text = ((workshopScanResult.Error.Length > 0) ? workshopScanResult.Error : "unrecognized");
				ModLog.Warn("GitHub 皮肤 " + entry.FullName + " 扫描未识别：" + text);
				return new GitHubScanResult(null, Array.Empty<string>(), text);
			}
			string[] array = WorkshopSubmissionCodec.Encode(workshopScanResult.Item, "1.0.19.0", GameVersion());
			ModLog.Info($"GitHub 皮肤 {entry.FullName} 扫描完成：识别到 {workshopScanResult.Item.Targets.Length} 项，生成 {array.Length} 段信息码。");
			return new GitHubScanResult(workshopScanResult.Item, array, "");
		}
		finally
		{
			TryDelete(root);
			Scanning.Remove(id);
		}
	}

	private static WorkshopScanResult Scan(GitHubSkinEntry entry, string package, SkinModDescriptor? descriptor)
	{
		if (SkinService.Catalog == null)
		{
			return new WorkshopScanResult(null, entry.Name, "The game catalog is not ready yet.");
		}
		(string, CardCatalogEntry[], SkinModDescriptor[]) tuple = SkinService.CaptureWorkshopScanContext();
		return WorkshopSubmissionScanner.Scan(new WorkshopLocalSubmission(entry.Id, descriptor?.Name ?? entry.Name, package), tuple.Item1, tuple.Item2, GameVersion(), Dependencies(), tuple.Item3);
	}

	private static string GameVersion()
	{
		return AccessTools.Field(typeof(ModManager), "_gameVersion")?.GetValue(null)?.ToString() ?? "";
	}

	private static Dictionary<string, string> Dependencies()
	{
		return ModManager.Mods.Where((Mod mod) => (int)mod.state == 1 && mod.manifest?.id != null).GroupBy<Mod, string>((Mod mod) => mod.manifest.id, StringComparer.OrdinalIgnoreCase).ToDictionary<IGrouping<string, Mod>, string, string>((IGrouping<string, Mod> group) => group.Key, (IGrouping<string, Mod> group) => group.First().manifest.version ?? "", StringComparer.OrdinalIgnoreCase);
	}

	public static async Task InstallAsync(ulong id, CancellationToken token)
	{
		if (!Installing.Add(id))
		{
			return;
		}
		try
		{
			GitHubSkinEntry entry = Entry(id) ?? throw new InvalidOperationException("Unknown GitHub skin.");
			StagedPackage stagedPackage = await StageAsync(entry, token);
			SkinModDescriptor skinModDescriptor = stagedPackage.Select(entry.RepoName);
			string packageId = skinModDescriptor?.Id ?? "";
			string canonicalFolder = GitHubInstallPolicy.FolderName(packageId, entry.RepoName);
			string text = (((object)skinModDescriptor == null) ? entry.ReleaseTag : (GitHubInstallPolicy.ReadManifestVersion(skinModDescriptor.RootPath ?? stagedPackage.Directory, skinModDescriptor.Id) ?? entry.ReleaseTag));
			GitHubPlacement placement = GitHubFolderSwap.Place(GitHubInstallPolicy.ModsDirectory(OS.GetExecutablePath()), Ledger.Find(entry.FullName)?.Folder, canonicalFolder, packageId, stagedPackage.Digest, stagedPackage.Directory, IsLoadedFolder, ModLog.Warn);
			CompleteInstall(entry, placement, packageId, text ?? "", stagedPackage.Digest);
		}
		catch (Exception ex) when (!(ex is OperationCanceledException))
		{
			throw new InvalidOperationException(ex.GetBaseException().Message, ex);
		}
		finally
		{
			Installing.Remove(id);
		}
	}

	private static void CompleteInstall(GitHubSkinEntry entry, GitHubPlacement placement, string packageId, string version, string digest)
	{
		Ledger.Record(entry.FullName, new GitHubInstall(placement.Folder, packageId, version, digest));
		Ledger.ClearDelete(entry.FullName, placement.Folder);
		string disabled = placement.Disabled;
		if (disabled != null && disabled.Length > 0)
		{
			Ledger.SetDelete(entry.FullName, disabled);
		}
		RestartPending.Add(entry.FullName);
		SaveLedger();
		RefreshInstalls();
		PublishItems();
		string disabled2 = placement.Disabled;
		ModLog.Info((disabled2 != null && disabled2.Length > 0) ? $"GitHub 皮肤 {entry.FullName} 已更新到 mods/{placement.Folder}；旧目录 mods/{placement.Disabled} 将在下次启动游戏时移除。" : $"GitHub 皮肤 {entry.FullName} 已安装到 mods/{placement.Folder}；下次启动游戏生效。");
	}

	public static async Task UninstallAsync(ulong id)
	{
		GitHubSkinEntry entry = Entry(id) ?? throw new InvalidOperationException("Unknown GitHub skin.");
		GitHubInstall gitHubInstall = Ledger.Find(entry.FullName);
		if ((object)gitHubInstall == null)
		{
			throw new InvalidOperationException("Skin is not installed.");
		}
		string modsDirectory = GitHubInstallPolicy.ModsDirectory(OS.GetExecutablePath());
		string target = GitHubInstallPolicy.UninstallTarget(modsDirectory, gitHubInstall.Folder, gitHubInstall.PackageId);
		bool loaded = target != null && IsLoadedFolder(target);
		if (loaded)
		{
			if (!GitHubFolderSwap.Disable(target, ModLog.Warn))
			{
				throw new InvalidOperationException("无法停用正在使用的皮肤目录：mods/" + gitHubInstall.Folder);
			}
			DropInstall(entry.FullName);
			Ledger.SetDelete(entry.FullName, gitHubInstall.Folder);
		}
		else
		{
			if (target != null)
			{
				await Task.Run(delegate
				{
					Directory.Delete(target, recursive: true);
				});
			}
			DropInstall(entry.FullName);
		}
		SaveLedger();
		RefreshInstalls();
		PublishItems();
		ModLog.Info(loaded ? ("GitHub 皮肤 " + entry.FullName + " 正被本次游戏使用；文件将在下次启动游戏时移除：" + target) : ("GitHub 皮肤 " + entry.FullName + " 已卸载；下次启动游戏生效。"));
	}

	private static void DropInstall(string fullName)
	{
		Ledger.Remove(fullName);
		GitHubPendingOp gitHubPendingOp = Ledger.ClearPending(fullName);
		if ((object)gitHubPendingOp != null)
		{
			string staging = gitHubPendingOp.Staging;
			if (staging != null && staging.Length > 0)
			{
				TryDelete(gitHubPendingOp.Staging);
			}
		}
		RestartPending.Remove(fullName);
	}

	private static void RefreshInstalls()
	{
		string text = ModsDirectory();
		if (text == null)
		{
			return;
		}
		bool flag = false;
		KeyValuePair<string, GitHubInstall>[] array = Ledger.Installs.ToArray();
		foreach (KeyValuePair<string, GitHubInstall> keyValuePair in array)
		{
			keyValuePair.Deconstruct(out var key, out var value);
			string fullName = key;
			GitHubInstall gitHubInstall = value;
			string text2 = Path.Combine(text, gitHubInstall.Folder);
			if (!Directory.Exists(text2))
			{
				Ledger.Remove(fullName);
				flag = true;
				continue;
			}
			string text3 = GitHubInstallPolicy.ReadManifestVersion(text2, gitHubInstall.PackageId);
			if (text3 != null && text3 != gitHubInstall.Version)
			{
				Ledger.Record(fullName, gitHubInstall with
				{
					Version = text3
				});
				flag = true;
			}
		}
		if (flag)
		{
			SaveLedger();
		}
		RestartPending.RemoveWhere((string name) => Ledger.Find(name) == null);
		Dictionary<ulong, GitHubInstall> dictionary = new Dictionary<ulong, GitHubInstall>();
		GitHubSkinEntry[] entries = Catalog.Entries;
		foreach (GitHubSkinEntry gitHubSkinEntry in entries)
		{
			GitHubInstall gitHubInstall2 = Ledger.Find(gitHubSkinEntry.FullName);
			if ((object)gitHubInstall2 != null)
			{
				dictionary[gitHubSkinEntry.Id] = gitHubInstall2;
			}
		}
		_installsById = dictionary;
	}

	private static void BackfillDigests()
	{
		bool flag = false;
		GitHubSkinEntry[] entries = Catalog.Entries;
		foreach (GitHubSkinEntry gitHubSkinEntry in entries)
		{
			GitHubInstall gitHubInstall = Ledger.Find(gitHubSkinEntry.FullName);
			if ((object)gitHubInstall != null)
			{
				string text = GitHubInstallPolicy.BackfillDigest(gitHubInstall.Version, gitHubInstall.Digest, gitHubSkinEntry.ReleaseTag, gitHubSkinEntry.AssetDigest);
				if (text.Length != 0 && Ledger.Record(gitHubSkinEntry.FullName, gitHubInstall with
				{
					Digest = text
				}))
				{
					flag = true;
					ModLog.Info("GitHub 皮肤 " + gitHubSkinEntry.FullName + " 的安装记录已补记发布校验值，此后按包内容判断更新。");
				}
			}
		}
		if (flag)
		{
			SaveLedger();
			RefreshInstalls();
			PublishItems();
		}
	}

	private static string? ModsDirectory()
	{
		try
		{
			return GitHubInstallPolicy.ModsDirectory(OS.GetExecutablePath());
		}
		catch (Exception ex) when (((ex is IOException || ex is InvalidOperationException || ex is NotSupportedException) ? 1 : 0) != 0)
		{
			ModLog.Warn("无法定位游戏 mods 目录：" + ex.Message);
			return null;
		}
	}

	private static void ReconcileFolders()
	{
		string text = ModsDirectory();
		if (text == null)
		{
			return;
		}
		bool flag = MigrateLegacyPending(text);
		(string, string)[] array = Ledger.Deletes.SelectMany<KeyValuePair<string, string[]>, (string, string)>((KeyValuePair<string, string[]> pair) => pair.Value.Select((string folder) => (FullName: pair.Key, Folder: folder))).ToArray();
		if (array.Length != 0)
		{
			HashSet<string> liveFolders = Ledger.Installs.Values.Select((GitHubInstall install) => install.Folder).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
			HashSet<string> hashSet = GitHubFolderSwap.Sweep(text, array, liveFolders, ModLog.Info).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
			(string, string)[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				var (fullName, text2) = array2[i];
				if (!hashSet.Contains(text2))
				{
					flag |= Ledger.ClearDelete(fullName, text2);
				}
			}
		}
		if (flag)
		{
			SaveLedger();
		}
	}

	private static bool MigrateLegacyPending(string mods)
	{
		if (Ledger.Pending.Count == 0)
		{
			return false;
		}
		bool result = false;
		KeyValuePair<string, GitHubPendingOp>[] array = Ledger.Pending.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<string, GitHubPendingOp> keyValuePair = array[i];
			var (text2, gitHubPendingOp2) = (KeyValuePair<string, GitHubPendingOp>)(ref keyValuePair);
			if (gitHubPendingOp2.Kind == GitHubModsOperationKind.Remove)
			{
				Ledger.ClearPending(text2);
				Ledger.SetDelete(text2, gitHubPendingOp2.Folder);
				result = true;
				ModLog.Info($"GitHub 皮肤 {text2} 的旧版退出卸载步骤已改为下次启动时删除 mods/{gitHubPendingOp2.Folder}。");
				continue;
			}
			GitHubInstall gitHubInstall = Ledger.Find(text2);
			if (gitHubInstall == null || !Directory.Exists(gitHubPendingOp2.Staging))
			{
				Ledger.ClearPending(text2);
				TryDelete(gitHubPendingOp2.Staging);
				result = true;
				ModLog.Warn($"GitHub 皮肤 {text2} 的旧版待安装包已不在磁盘上，目录保持原样（需要时可卸载后重新安装：{gitHubPendingOp2.Staging}）。");
				continue;
			}
			try
			{
				GitHubPlacement gitHubPlacement = GitHubFolderSwap.Place(mods, gitHubInstall.Folder, gitHubInstall.Folder, gitHubInstall.PackageId, gitHubInstall.Digest, gitHubPendingOp2.Staging, IsLoadedFolder, ModLog.Warn);
				Ledger.Record(text2, gitHubInstall with
				{
					Folder = gitHubPlacement.Folder
				});
				Ledger.ClearDelete(text2, gitHubPlacement.Folder);
				string disabled = gitHubPlacement.Disabled;
				if (disabled != null && disabled.Length > 0)
				{
					Ledger.SetDelete(text2, disabled);
				}
				Ledger.ClearPending(text2);
				TryDelete(gitHubPendingOp2.Staging);
				RestartPending.Add(text2);
				result = true;
				ModLog.Info($"GitHub 皮肤 {text2} 的旧版待安装包已写入 mods/{gitHubPlacement.Folder}；下次启动游戏生效。");
			}
			catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException || ex is InvalidOperationException) ? 1 : 0) != 0)
			{
				ModLog.Warn("GitHub 皮肤 " + text2 + " 的旧版待安装包暂时写不进去，下次启动再试：" + ex.GetBaseException().Message);
			}
		}
		return result;
	}

	private static void CleanLeftovers()
	{
		string text = ModsDirectory();
		if (text == null || !Directory.Exists(text))
		{
			return;
		}
		foreach (string item in Directory.EnumerateDirectories(text))
		{
			string fileName = Path.GetFileName(item);
			if (GitHubModsPolicy.IsLeftoverFolderName(fileName) && !IsLoadedFolder(item))
			{
				ModLog.Info("清理 GitHub 皮肤安装残留：" + fileName);
				TryDelete(item);
			}
		}
	}

	public static bool IsPendingRemoval(ulong id)
	{
		GitHubSkinEntry gitHubSkinEntry = Entry(id);
		if ((object)gitHubSkinEntry != null && InstallState(id) == GitHubInstallState.NotInstalled)
		{
			return Ledger.HasDelete(gitHubSkinEntry.FullName);
		}
		return false;
	}

	private static bool IsLoadedFolder(string path)
	{
		try
		{
			string b = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
			foreach (Mod mod in ModManager.Mods)
			{
				string path2 = mod.path;
				if (path2 != null && path2.Length > 0 && string.Equals(Path.TrimEndingDirectorySeparator(Path.GetFullPath(path2)), b, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("无法确认皮肤目录是否已被游戏加载：" + ex.Message);
		}
		return false;
	}

	private static void TryDelete(string directory)
	{
		try
		{
			if (Directory.Exists(directory))
			{
				Directory.Delete(directory, recursive: true);
			}
		}
		catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException) ? 1 : 0) != 0)
		{
			ModLog.Warn("临时目录清理失败：" + ex.Message);
		}
	}

	private static void TryDeleteFile(string path)
	{
		try
		{
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}
		catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException) ? 1 : 0) != 0)
		{
			ModLog.Warn("临时文件清理失败：" + ex.Message);
		}
	}

	private static void PublishItems()
	{
		RefreshEntries();
		Revision++;
	}

	private static void RefreshEntries()
	{
		_entriesById = Catalog.Entries.ToDictionary((GitHubSkinEntry entry) => entry.Id);
		_items = Catalog.Entries.Select((GitHubSkinEntry entry) => new WorkshopCatalogItem(entry.Id, (entry.Recognition == GitHubRecognition.Recognized) ? entry.Targets : Array.Empty<WorkshopTarget>())).ToArray();
	}

	private static void SaveCatalog()
	{
		try
		{
			Catalog.Save(SkinChangerPaths.GitHubCatalogPath, null);
		}
		catch (Exception ex)
		{
			ModLog.Warn("GitHub 皮肤缓存写入失败：" + ex.Message);
		}
	}

	private static void SaveLedger()
	{
		try
		{
			Ledger.Save(SkinChangerPaths.GitHubInstallLedgerPath, null);
		}
		catch (Exception ex)
		{
			ModLog.Warn("GitHub 安装记录写入失败：" + ex.Message);
		}
	}
}
