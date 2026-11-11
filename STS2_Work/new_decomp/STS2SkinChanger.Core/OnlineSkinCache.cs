using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.RegularExpressions.Generated;
using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Multiplayer.Transport.Steam;
using MegaCrit.Sts2.Core.Nodes;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Pck;
using Steamworks;

namespace STS2SkinChanger.Core;

internal static class OnlineSkinCache
{
	private sealed record WorkshopDetails(string Title, ulong Size);

	private sealed record SafePackage(string Fingerprint, int FileCount, ulong TotalBytes);

	private sealed record CachedProvider(string OptionId, string Fingerprint, string PckPath);

	private sealed record LocalSourceCacheEntry(long Length, DateTime LastWriteTimeUtc, OnlineSkinSource Source);

	private sealed record LocalSourceFailureCacheEntry(long Length, DateTime LastWriteTimeUtc, string Detail);

	private const ulong GameAppId = 2868840uL;

	private const int MaxArchiveEntries = 100000;

	private const int MaxSafeFiles = 2048;

	private const ulong MaxSafeFileSize = 67108864uL;

	private const ulong MaxSafePackageSize = 134217728uL;

	private const int MaxTextResourceSize = 2097152;

	private static readonly object Sync = new object();

	private static readonly Queue<SkinChangerNetMessage> Pending = new Queue<SkinChangerNetMessage>();

	private static readonly HashSet<string> PendingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly HashSet<string> DeclinedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, OnlineSkinCacheFailure> BlockingFailures = new Dictionary<string, OnlineSkinCacheFailure>(StringComparer.OrdinalIgnoreCase);

	private static readonly HashSet<string> AcknowledgedFailureKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, CachedProvider> Providers = new Dictionary<string, CachedProvider>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, LocalSourceCacheEntry> LocalSources = new Dictionary<string, LocalSourceCacheEntry>(StringComparer.OrdinalIgnoreCase);

	private static readonly HashSet<string> LocalSourceBuilds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, LocalSourceFailureCacheEntry> LocalSourceFailures = new Dictionary<string, LocalSourceFailureCacheEntry>(StringComparer.OrdinalIgnoreCase);

	private static readonly ConcurrentQueue<(string GroupId, string OptionId)> LocalSourcesReady = new ConcurrentQueue<(string, string)>();

	private static readonly ConcurrentQueue<string> LocalSourceReports = new ConcurrentQueue<string>();

	private static readonly Dictionary<ulong, TaskCompletionSource<EResult>> DownloadWaiters = new Dictionary<ulong, TaskCompletionSource<EResult>>();

	private static readonly HashSet<ulong> SessionWorkshopInstalls = new HashSet<ulong>();

	private static readonly HashSet<string> AllowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{
		".png", ".webp", ".jpg", ".jpeg", ".ctex", ".ogg", ".mp3", ".wav", ".oggvorbisstr", ".mp3str",
		".sample", ".spatlas", ".spskel", ".atlas", ".skel", ".remap", ".import", ".tres", ".res", ".scn",
		".tscn"
	};

	private static readonly HashSet<string> TextExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".remap", ".import", ".tres", ".atlas", ".spatlas", ".tscn" };

	private static readonly HashSet<string> BinaryResourceExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".res", ".scn" };

	private static Callback<DownloadItemResult_t>? _downloadCallback;

	private static CancellationTokenSource? _sessionCancellation;

	private static string? _sessionDirectory;

	private static int _sessionGeneration;

	private static bool _processing;

	private static OnlineSkinCacheStage _progressStage;

	private static string _progressProviderId = string.Empty;

	private static ulong _progressWorkshopItemId;

	private static string _progressDetail = string.Empty;

	private static DateTime _progressExpiresAtUtc;

	internal static bool HasActiveSession
	{
		get
		{
			lock (Sync)
			{
				return _sessionDirectory != null || _sessionCancellation != null || Providers.Count > 0;
			}
		}
	}

	internal static void BeginSession()
	{
		EndSession();
		lock (Sync)
		{
			_sessionGeneration++;
			_sessionCancellation = new CancellationTokenSource();
			_sessionDirectory = Path.Combine(Path.GetTempPath(), "Gurio.SkinChanger", "online", $"{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Environment.ProcessId}-{Guid.NewGuid():N}-{_sessionGeneration:D3}");
			Directory.CreateDirectory(_sessionDirectory);
			ModLog.Info($"已创建本轮联机皮肤临时缓存（第 {_sessionGeneration} 轮）。");
			try
			{
				EnsureDownloadCallback();
			}
			catch (Exception ex)
			{
				ModLog.Info("当前联机方式无法使用 Steam 工坊临时缓存；缺少的皮肤将显示原皮：" + ex.GetBaseException().Message);
			}
		}
		CleanupOldSessionDirectories();
	}

	internal static void CleanupStaleSessionsAtStartup()
	{
		CleanupOldSessionDirectories();
	}

	internal static void EndSession()
	{
		CachedProvider[] source;
		string sessionDirectory;
		lock (Sync)
		{
			_sessionCancellation?.Cancel();
			_sessionCancellation?.Dispose();
			_sessionCancellation = null;
			foreach (TaskCompletionSource<EResult> value in DownloadWaiters.Values)
			{
				value.TrySetCanceled();
			}
			DownloadWaiters.Clear();
			SessionWorkshopInstalls.Clear();
			Pending.Clear();
			PendingKeys.Clear();
			DeclinedKeys.Clear();
			BlockingFailures.Clear();
			AcknowledgedFailureKeys.Clear();
			source = Providers.Values.ToArray();
			Providers.Clear();
			sessionDirectory = _sessionDirectory;
			_sessionDirectory = null;
			_processing = false;
			ResetProgressLocked();
		}
		foreach (CachedProvider item in source.Reverse())
		{
			try
			{
				SkinService.RemoveOnlineSessionProvider(item.OptionId);
			}
			catch (Exception ex)
			{
				ModLog.Warn("清理联机缓存皮肤 " + item.OptionId + " 失败：" + ex.GetBaseException().Message);
			}
		}
		if (TryDeleteDirectory(sessionDirectory))
		{
			ModLog.Info("已删除上一轮联机皮肤临时安全包；Steam 工坊目录未触碰。 ");
		}
		else if (!string.IsNullOrWhiteSpace(sessionDirectory) && Directory.Exists(sessionDirectory))
		{
			ModLog.Warn("上一轮联机皮肤临时安全包暂未删除：" + sessionDirectory + "；游戏资源系统释放后会在下次启动继续清理。 ");
		}
	}

	internal static void Tick(bool allowDownloads)
	{
		(string, string) result;
		while (LocalSourcesReady.TryDequeue(out result))
		{
			MultiplayerSkinSync.OnLocalOnlineMetadataReady(result.Item1, result.Item2);
		}
		string result2;
		while (LocalSourceReports.TryDequeue(out result2))
		{
			ModLog.Info(result2);
		}
		SkinChangerNetMessage skinChangerNetMessage;
		int sessionGeneration;
		CancellationToken token;
		lock (Sync)
		{
			if (!allowDownloads || !SkinService.ShouldReceiveMultiplayerSkinChanges() || _processing || Pending.Count == 0 || _sessionCancellation == null)
			{
				return;
			}
			NRun instance = NRun.Instance;
			if (((instance != null) ? instance.CombatRoom : null) != null)
			{
				return;
			}
			skinChangerNetMessage = Pending.Dequeue();
			PendingKeys.Remove(RequestKey(skinChangerNetMessage));
			_processing = true;
			sessionGeneration = _sessionGeneration;
			token = _sessionCancellation.Token;
		}
		TaskHelper.RunSafely(ProcessRequest(skinChangerNetMessage, sessionGeneration, token));
	}

	internal static bool HasPendingWork()
	{
		bool flag = SkinService.ShouldReceiveMultiplayerSkinChanges();
		lock (Sync)
		{
			return LocalSourceBuilds.Count > 0 || (flag && (_processing || Pending.Count > 0 || BlockingFailures.Count > 0));
		}
	}

	internal static OnlineSkinCacheProgress GetProgress()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		OnlineSkinCacheStage progressStage2;
		string progressProviderId;
		ulong progressWorkshopItemId;
		string progressDetail;
		lock (Sync)
		{
			OnlineSkinCacheStage progressStage = _progressStage;
			bool flag = (uint)(progressStage - 7) <= 1u;
			if (flag && DateTime.UtcNow >= _progressExpiresAtUtc)
			{
				ResetProgressLocked();
			}
			progressStage2 = _progressStage;
			progressProviderId = _progressProviderId;
			progressWorkshopItemId = _progressWorkshopItemId;
			progressDetail = _progressDetail;
		}
		ulong downloadedBytes = 0uL;
		ulong totalBytes = 0uL;
		if (progressStage2 == OnlineSkinCacheStage.Downloading && progressWorkshopItemId != 0L)
		{
			try
			{
				SteamUGC.GetItemDownloadInfo(new PublishedFileId_t(progressWorkshopItemId), ref downloadedBytes, ref totalBytes);
			}
			catch
			{
			}
		}
		return new OnlineSkinCacheProgress(progressStage2, progressProviderId, progressWorkshopItemId, downloadedBytes, totalBytes, progressDetail);
	}

	internal static void OnRemoteSkinLoadingPreferenceChanged(bool enabled)
	{
		if (enabled)
		{
			return;
		}
		lock (Sync)
		{
			Pending.Clear();
			PendingKeys.Clear();
			BlockingFailures.Clear();
			ResetProgressLocked();
		}
	}

	internal static void DiscardPendingSelectionsForPlayer(ulong playerNetId)
	{
		lock (Sync)
		{
			SkinChangerNetMessage[] array = Pending.Where((SkinChangerNetMessage message) => message.PlayerNetId != playerNetId).ToArray();
			Pending.Clear();
			PendingKeys.Clear();
			SkinChangerNetMessage[] array2 = array;
			foreach (SkinChangerNetMessage skinChangerNetMessage in array2)
			{
				Pending.Enqueue(skinChangerNetMessage);
				PendingKeys.Add(RequestKey(skinChangerNetMessage));
			}
			string[] array3 = (from pair in BlockingFailures
				where pair.Key.StartsWith($"remote:{playerNetId}:", StringComparison.OrdinalIgnoreCase)
				select pair.Key).ToArray();
			foreach (string key in array3)
			{
				BlockingFailures.Remove(key);
			}
		}
	}

	internal static bool QueueMissingSelection(SkinChangerNetMessage message)
	{
		if (!SkinService.ShouldReceiveMultiplayerSkinChanges() || !HasDownloadMetadata(message) || message.OptionId.Equals("__base__", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		string text = ProviderKey(message);
		lock (Sync)
		{
			string text2 = $"remote:{message.PlayerNetId}:{message.GroupId}:{message.OptionId}";
			BlockingFailures.Remove(text2);
			AcknowledgedFailureKeys.Remove(text2);
			if (BlockingFailures.Count == 0 && _progressStage == OnlineSkinCacheStage.Failed)
			{
				ResetProgressLocked();
			}
			if (Providers.ContainsKey(text))
			{
				return false;
			}
			string item = RequestKey(message);
			if (_sessionCancellation == null || DeclinedKeys.Contains(text) || !PendingKeys.Add(item))
			{
				return false;
			}
			Pending.Enqueue(message);
			SetProgressLocked(OnlineSkinCacheStage.WaitingForReady, message.ProviderId, message.WorkshopItemId, "", null);
		}
		ModLog.Info("检测到联机玩家使用本地缺少的皮肤 " + message.ProviderId + "，已记录并等待所有玩家准备后自动安全缓存。");
		return true;
	}

	internal static bool HasDownloadMetadata(SkinChangerNetMessage message)
	{
		IReadOnlyDictionary<string, VisualResourceBinding> bindings;
		if (message.WorkshopItemId != 0L && !string.IsNullOrWhiteSpace(message.ProviderId) && !string.IsNullOrWhiteSpace(message.SafeResourceFingerprint) && FingerprintRegex().IsMatch(message.SafeResourceFingerprint) && IsValidSafeResourceManifest(message.SafeResourceManifest))
		{
			return TryParseSafeResourceBindings(message.SafeResourceBindings, message.GroupId, out bindings);
		}
		return false;
	}

	internal static bool IsWaitingForDownloadMetadata(SkinChangerNetMessage message)
	{
		if (!string.IsNullOrWhiteSpace(message.ProviderId) && string.IsNullOrWhiteSpace(message.SafeResourceFingerprint))
		{
			return string.IsNullOrWhiteSpace(message.OnlineFailure);
		}
		return false;
	}

	internal static void ReportMissingMetadata(SkinChangerNetMessage message)
	{
		string providerId = (string.IsNullOrWhiteSpace(message.ProviderId) ? message.OptionId : message.ProviderId);
		string detail = (string.IsNullOrWhiteSpace(message.OnlineFailure) ? "对方没有提供可下载的工坊资源信息。" : message.OnlineFailure);
		AddBlockingFailure($"remote:{message.PlayerNetId}:{message.GroupId}:{message.OptionId}", providerId, detail);
		SetProgress(OnlineSkinCacheStage.Failed, providerId, message.WorkshopItemId, detail, TimeSpan.FromDays(1));
	}

	internal static void ReportWaitingForMetadata(SkinChangerNetMessage message)
	{
		SetProgress(OnlineSkinCacheStage.Preparing, message.ProviderId, message.WorkshopItemId, "", null);
	}

	internal static void ReportLocalDescriptionFailure(string groupId, string optionId, string providerId, ulong workshopItemId, string detail)
	{
		if (MultiplayerSkinSync.IsReadyGateActiveForOnlineCache())
		{
			SetProgress(OnlineSkinCacheStage.Failed, providerId, workshopItemId, detail, TimeSpan.FromDays(1));
		}
	}

	internal static bool TryPeekBlockingFailure(out OnlineSkinCacheFailure failure)
	{
		lock (Sync)
		{
			failure = BlockingFailures.Values.FirstOrDefault();
			return failure != null;
		}
	}

	internal static void AcknowledgeBlockingFailure(string key)
	{
		lock (Sync)
		{
			BlockingFailures.Remove(key);
			AcknowledgedFailureKeys.Add(key);
			if (BlockingFailures.Count == 0 && _progressStage == OnlineSkinCacheStage.Failed)
			{
				ResetProgressLocked();
			}
		}
	}

	internal static void ClearBlockingFailures()
	{
		lock (Sync)
		{
			BlockingFailures.Clear();
			if (_progressStage == OnlineSkinCacheStage.Failed)
			{
				ResetProgressLocked();
			}
		}
	}

	private static void AddBlockingFailure(string key, string providerId, string detail)
	{
		lock (Sync)
		{
			if (!AcknowledgedFailureKeys.Contains(key))
			{
				BlockingFailures[key] = new OnlineSkinCacheFailure(key, providerId, detail);
			}
		}
	}

	internal static bool TryGetCachedOption(SkinChangerNetMessage message, out string optionId)
	{
		lock (Sync)
		{
			if (Providers.TryGetValue(ProviderKey(message), out CachedProvider value))
			{
				optionId = value.OptionId;
				return true;
			}
		}
		optionId = string.Empty;
		return false;
	}

	internal static OnlineSkinDescriptionState TryDescribeLocalSelection(string groupId, string optionId, out OnlineSkinSource source, out string failureDetail)
	{
		source = null;
		failureDetail = string.Empty;
		SkinCatalog catalog = SkinService.Catalog;
		if (catalog == null)
		{
			failureDetail = "皮肤目录尚未初始化。";
			return OnlineSkinDescriptionState.Unavailable;
		}
		if (!catalog.TryGetVisualProviderSource(groupId, optionId, out string providerId, out string pckPath, out IReadOnlyList<string> safeResourceRoots, out IReadOnlyDictionary<string, VisualResourceBinding> resourceBindings))
		{
			catalog.TryGetVisualProviderId(groupId, optionId, out providerId);
			providerId = (string.IsNullOrWhiteSpace(providerId) ? optionId : providerId);
			failureDetail = "找不到该皮肤实际所属的 PCK 资源包。";
			source = new OnlineSkinSource(providerId, 0uL, string.Empty, string.Empty, string.Empty);
			return OnlineSkinDescriptionState.Unavailable;
		}
		if (!TryGetWorkshopItemId(pckPath, out var workshopItemId))
		{
			failureDetail = "该皮肤不是从 Steam 创意工坊目录加载的，无法自动下载。";
			source = new OnlineSkinSource(providerId, 0uL, string.Empty, string.Empty, string.Empty);
			return OnlineSkinDescriptionState.Unavailable;
		}
		int ignoredCount;
		IReadOnlyList<string> readOnlyList = FilterLocalSafeResourceRoots(safeResourceRoots, out ignoredCount);
		if (ignoredCount > 0)
		{
			ModLog.Info($"{providerId} 的联机资源清单已忽略 {ignoredCount} 个非静态/不可共享附带文件；" + "这些文件不会影响皮肤拥有者本机使用。 ");
		}
		int baselineSceneFallbackCount;
		IReadOnlyDictionary<string, VisualResourceBinding> filteredBindings = FilterLocalSafeResourceBindings(resourceBindings, readOnlyList, groupId, pckPath, out baselineSceneFallbackCount);
		if (baselineSceneFallbackCount > 0)
		{
			ModLog.Info($"{providerId} 的 {baselineSceneFallbackCount} 个场景引用了不可联机执行的脚本；" + "远端将使用游戏原场景承载该皮肤的静态模型和贴图。 ");
		}
		IReadOnlyList<string> manifestRoots = FilterManifestRootsForBindings(readOnlyList, resourceBindings, filteredBindings);
		string safeResourceManifest = string.Join('\n', manifestRoots);
		string safeResourceBindings = SerializeSafeResourceBindings(filteredBindings);
		if (!IsValidSafeResourceManifest(safeResourceManifest) || !TryParseSafeResourceBindings(safeResourceBindings, groupId, out IReadOnlyDictionary<string, VisualResourceBinding> _))
		{
			failureDetail = "该皮肤的安全资源清单过大或格式无效。";
			source = new OnlineSkinSource(providerId, workshopItemId, string.Empty, string.Empty, string.Empty);
			return OnlineSkinDescriptionState.Failed;
		}
		FileInfo info = new FileInfo(pckPath);
		string cacheKey = pckPath + "\n" + groupId + "\n" + optionId;
		lock (Sync)
		{
			if (LocalSources.TryGetValue(cacheKey, out LocalSourceCacheEntry value) && value.Length == info.Length && value.LastWriteTimeUtc == info.LastWriteTimeUtc)
			{
				source = value.Source;
				return OnlineSkinDescriptionState.Ready;
			}
			if (LocalSourceFailures.TryGetValue(cacheKey, out LocalSourceFailureCacheEntry value2) && value2.Length == info.Length && value2.LastWriteTimeUtc == info.LastWriteTimeUtc)
			{
				failureDetail = value2.Detail;
				source = new OnlineSkinSource(providerId, workshopItemId, string.Empty, safeResourceManifest, safeResourceBindings);
				return OnlineSkinDescriptionState.Failed;
			}
			if (!LocalSourceBuilds.Add(cacheKey))
			{
				source = new OnlineSkinSource(providerId, workshopItemId, string.Empty, safeResourceManifest, safeResourceBindings);
				return OnlineSkinDescriptionState.Preparing;
			}
			SetLocalPreparationProgressLocked(OnlineSkinCacheStage.Preparing, providerId, workshopItemId, "", null);
		}
		Task.Run(delegate
		{
			try
			{
				SafePackage safePackage = BuildSafePackage(pckPath, groupId, manifestRoots, filteredBindings, null);
				OnlineSkinSource source2 = new OnlineSkinSource(providerId, workshopItemId, safePackage.Fingerprint, safeResourceManifest, safeResourceBindings);
				lock (Sync)
				{
					LocalSources[cacheKey] = new LocalSourceCacheEntry(info.Length, info.LastWriteTimeUtc, source2);
				}
				LocalSourcesReady.Enqueue((groupId, optionId));
				LocalSourceReports.Enqueue(providerId + " 已准备联机安全资源指纹；缺少该皮肤的玩家可选择临时缓存静态素材。");
				SetLocalPreparationProgress(OnlineSkinCacheStage.Complete, providerId, workshopItemId, string.Empty, TimeSpan.FromSeconds(2.5));
			}
			catch (Exception ex)
			{
				string message = ex.GetBaseException().Message;
				lock (Sync)
				{
					LocalSourceFailures[cacheKey] = new LocalSourceFailureCacheEntry(info.Length, info.LastWriteTimeUtc, message);
				}
				LocalSourcesReady.Enqueue((groupId, optionId));
				LocalSourceReports.Enqueue(providerId + " 不支持安全联机缓存，将让缺少该皮肤的玩家显示原皮：" + message);
				SetLocalPreparationProgress(OnlineSkinCacheStage.Failed, providerId, workshopItemId, message, TimeSpan.FromDays(1));
			}
			finally
			{
				lock (Sync)
				{
					LocalSourceBuilds.Remove(cacheKey);
				}
			}
		});
		source = new OnlineSkinSource(providerId, workshopItemId, string.Empty, safeResourceManifest, safeResourceBindings);
		return OnlineSkinDescriptionState.Preparing;
	}

	private static async Task ProcessRequest(SkinChangerNetMessage request, int generation, CancellationToken cancellationToken)
	{
		try
		{
			if (!SkinService.ShouldReceiveMultiplayerSkinChanges())
			{
				return;
			}
			CachedProvider value;
			lock (Sync)
			{
				Providers.TryGetValue(ProviderKey(request), out value);
			}
			if (value != null)
			{
				ModLog.Info("本轮已缓存 " + request.ProviderId + " 的联机皮肤，直接复用本轮安全包；如已重新进入选角，说明缓存生命周期未跨回合保留。 ");
				MultiplayerSkinSync.RetryCachedSelection(request, value.OptionId);
				return;
			}
			SetProgress(OnlineSkinCacheStage.CheckingWorkshop, request.ProviderId, request.WorkshopItemId, "", null);
			WorkshopDetails details = await QueryWorkshopItem(request.WorkshopItemId, cancellationToken);
			if (!IsCurrentSession(generation, cancellationToken) || !SkinService.ShouldReceiveMultiplayerSkinChanges())
			{
				return;
			}
			NRun instance = NRun.Instance;
			if (((instance != null) ? instance.CombatRoom : null) != null)
			{
				Requeue(request);
				return;
			}
			SetProgress(OnlineSkinCacheStage.Downloading, request.ProviderId, request.WorkshopItemId, "", null);
			string workshopDirectory = await EnsureWorkshopItemAvailable(request.WorkshopItemId, cancellationToken);
			cancellationToken.ThrowIfCancellationRequested();
			if (!SkinService.ShouldReceiveMultiplayerSkinChanges())
			{
				return;
			}
			string sourcePck = FindProviderPck(workshopDirectory, request.ProviderId);
			string text = Path.Combine(_sessionDirectory ?? throw new OperationCanceledException(), request.WorkshopItemId.ToString(), BuildSessionOptionId(request));
			Directory.CreateDirectory(text);
			string outputPck = Path.Combine(text, "safe-resources.pck");
			if (!TryParseSafeResourceBindings(request.SafeResourceBindings, request.GroupId, out IReadOnlyDictionary<string, VisualResourceBinding> resourceBindings))
			{
				throw new InvalidDataException("对方提供的角色资源映射格式无效。 ");
			}
			SetProgress(OnlineSkinCacheStage.Verifying, request.ProviderId, request.WorkshopItemId, "", null);
			SafePackage safePackage = await Task.Run(() => BuildSafePackage(sourcePck, request.GroupId, ParseSafeResourceManifest(request.SafeResourceManifest), resourceBindings, outputPck), cancellationToken);
			cancellationToken.ThrowIfCancellationRequested();
			if (IsCurrentSession(generation, cancellationToken) && SkinService.ShouldReceiveMultiplayerSkinChanges())
			{
				if (!safePackage.Fingerprint.Equals(request.SafeResourceFingerprint, StringComparison.OrdinalIgnoreCase))
				{
					throw new InvalidDataException("发送方与 Steam 当前资源的安全指纹不同，可能是版本不一致。");
				}
				string text2 = (string.IsNullOrWhiteSpace(details.Title) ? request.ProviderId : details.Title);
				string text3 = BuildSessionOptionId(request);
				SetProgress(OnlineSkinCacheStage.Applying, text2, request.WorkshopItemId, "", null);
				if (!SkinService.TryRegisterOnlineSessionProvider(text3, text2 + OnlineCacheSuffix(), outputPck, request.GroupId, resourceBindings, out string error))
				{
					throw new InvalidDataException(error);
				}
				lock (Sync)
				{
					Providers[ProviderKey(request)] = new CachedProvider(text3, request.SafeResourceFingerprint, outputPck);
				}
				ModLog.Info($"已为联机玩家缓存 {text2} 的 {safePackage.FileCount} 个安全资源，共 {(double)safePackage.TotalBytes / 1024.0:F1} KiB；未加载 DLL、自定义脚本或 Shader。 ");
				MultiplayerSkinSync.RetryCachedSelection(request, text3);
				SetProgress(OnlineSkinCacheStage.Complete, text2, request.WorkshopItemId, string.Empty, TimeSpan.FromSeconds(3L));
			}
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception ex2)
		{
			string message = ex2.GetBaseException().Message;
			lock (Sync)
			{
				if (generation == _sessionGeneration)
				{
					DeclinedKeys.Add(ProviderKey(request));
				}
			}
			ModLog.Warn("无法在线缓存 " + request.ProviderId + "，远程玩家继续显示原皮：" + message);
			AddBlockingFailure($"remote:{request.PlayerNetId}:{request.GroupId}:{request.OptionId}", request.ProviderId, message);
			SetProgress(OnlineSkinCacheStage.Failed, request.ProviderId, request.WorkshopItemId, message, TimeSpan.FromDays(1));
		}
		finally
		{
			lock (Sync)
			{
				if (generation == _sessionGeneration)
				{
					_processing = false;
				}
			}
		}
	}

	private static void Requeue(SkinChangerNetMessage request)
	{
		lock (Sync)
		{
			string item = RequestKey(request);
			if (_sessionCancellation != null && PendingKeys.Add(item))
			{
				Pending.Enqueue(request);
			}
		}
	}

	private static bool IsCurrentSession(int generation, CancellationToken cancellationToken)
	{
		if (!cancellationToken.IsCancellationRequested)
		{
			return generation == _sessionGeneration;
		}
		return false;
	}

	private static async Task<WorkshopDetails> QueryWorkshopItem(ulong workshopItemId, CancellationToken cancellationToken)
	{
		PublishedFileId_t val = default(PublishedFileId_t);
		((PublishedFileId_t)(ref val))._002Ector(workshopItemId);
		UGCQueryHandle_t handle = SteamUGC.CreateQueryUGCDetailsRequest((PublishedFileId_t[])(object)new PublishedFileId_t[1] { val }, 1u);
		try
		{
			SteamCallResult<SteamUGCQueryCompleted_t> result = new SteamCallResult<SteamUGCQueryCompleted_t>(SteamUGC.SendQueryUGCRequest(handle), cancellationToken);
			try
			{
				SteamUGCQueryCompleted_t val2 = await result.Task;
				SteamUGCDetails_t val3 = default(SteamUGCDetails_t);
				if ((int)val2.m_eResult != 1 || !SteamUGC.GetQueryUGCResult(val2.m_handle, 0u, ref val3))
				{
					throw new IOException($"Steam 无法验证该工坊物品：{val2.m_eResult}。");
				}
				if ((ulong)val3.m_nConsumerAppID.m_AppId != 2868840)
				{
					throw new InvalidDataException("该工坊物品不属于杀戮尖塔 2。");
				}
				return new WorkshopDetails(((SteamUGCDetails_t)(ref val3)).m_rgchTitle, checked((ulong)val3.m_nFileSize));
			}
			finally
			{
				((IDisposable)result)?.Dispose();
			}
		}
		finally
		{
			SteamUGC.ReleaseQueryUGCRequest(handle);
		}
	}

	private static async Task<string> EnsureWorkshopItemAvailable(ulong workshopItemId, CancellationToken cancellationToken)
	{
		PublishedFileId_t itemId = new PublishedFileId_t(workshopItemId);
		EItemState val = (EItemState)SteamUGC.GetItemState(itemId);
		if ((val & 4) != 0 && (val & 8) == 0 && TryGetInstallDirectory(itemId, out string directory))
		{
			ModLog.Info($"Steam 工坊皮肤 {workshopItemId} 已在本机；不会重复下载，但仍会为本轮重新生成安全资源包。 ");
			return directory;
		}
		TaskCompletionSource<EResult> taskCompletionSource;
		lock (Sync)
		{
			taskCompletionSource = new TaskCompletionSource<EResult>(TaskCreationOptions.RunContinuationsAsynchronously);
			DownloadWaiters[workshopItemId] = taskCompletionSource;
			SessionWorkshopInstalls.Add(workshopItemId);
		}
		if (!SteamUGC.DownloadItem(itemId, true))
		{
			lock (Sync)
			{
				DownloadWaiters.Remove(workshopItemId);
				SessionWorkshopInstalls.Remove(workshopItemId);
			}
			throw new IOException("Steam 拒绝开始下载该工坊物品。");
		}
		EResult val2 = await taskCompletionSource.Task.WaitAsync(TimeSpan.FromMinutes(5L), cancellationToken);
		if ((int)val2 != 1 || !TryGetInstallDirectory(itemId, out directory))
		{
			lock (Sync)
			{
				SessionWorkshopInstalls.Remove(workshopItemId);
			}
			throw new IOException($"Steam 下载失败：{val2}。");
		}
		return directory;
	}

	internal static bool ShouldSuppressRuntimeWorkshopInstall(ulong workshopItemId)
	{
		lock (Sync)
		{
			return SessionWorkshopInstalls.Contains(workshopItemId);
		}
	}

	private static void EnsureDownloadCallback()
	{
		if (_downloadCallback == null)
		{
			_downloadCallback = Callback<DownloadItemResult_t>.Create((DispatchDelegate<DownloadItemResult_t>)OnDownloadCompleted);
		}
	}

	private static void OnDownloadCompleted(DownloadItemResult_t result)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if ((ulong)result.m_unAppID.m_AppId == 2868840)
		{
			TaskCompletionSource<EResult> value;
			lock (Sync)
			{
				DownloadWaiters.Remove(result.m_nPublishedFileId.m_PublishedFileId, out value);
			}
			value?.TrySetResult(result.m_eResult);
		}
	}

	private static bool TryGetInstallDirectory(PublishedFileId_t itemId, out string directory)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		directory = string.Empty;
		ulong num = default(ulong);
		uint num2 = default(uint);
		if (SteamUGC.GetItemInstallInfo(itemId, ref num, ref directory, 4096u, ref num2))
		{
			return Directory.Exists(directory);
		}
		return false;
	}

	private static SafePackage BuildSafePackage(string sourcePck, string groupId, IReadOnlyCollection<string> explicitRoots, IReadOnlyDictionary<string, VisualResourceBinding> resourceBindings, string? outputPath)
	{
		PckArchive archive = PckArchive.Open(sourcePck, 100000u);
		HashSet<string> selected;
		Queue<string> queue;
		try
		{
			if (archive.Paths.Count > 100000)
			{
				throw new InvalidDataException("资源包文件数量异常。 ");
			}
			selected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			queue = new Queue<string>();
			Dictionary<string, byte[]> dictionary = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
			Dictionary<string, string> dictionary2 = Enumerable.SelectMany(resourceBindings, (KeyValuePair<string, VisualResourceBinding> binding) => binding.Value.Files.Select((string resourcePath) => new
			{
				ResourcePath = NormalizeResourcePath(resourcePath),
				SourcePath = NormalizeResourcePath(binding.Key)
			})).GroupBy(binding => binding.ResourcePath, StringComparer.OrdinalIgnoreCase).ToDictionary(group => group.Key, group => group.First().SourcePath, StringComparer.OrdinalIgnoreCase);
			foreach (string item2 in archive.Paths.Where((string path) => AllowedExtensions.Contains(Path.GetExtension(path)) && SkinCatalog.IsSafeOnlineResourceRootForGroup(path, groupId) && !SceneResourceChainRequiresExecutableResource(archive, path, new HashSet<string>(StringComparer.OrdinalIgnoreCase))))
			{
				Enqueue(item2);
			}
			foreach (string explicitRoot in explicitRoots)
			{
				string text = NormalizeResourcePath(explicitRoot);
				if (!archive.Contains(text))
				{
					throw new InvalidDataException("安全资源清单引用了工坊包中不存在的文件：" + text + "。");
				}
				Enqueue(text);
			}
			string result;
			while (queue.TryDequeue(out result))
			{
				ValidateFile(result);
				string extension = Path.GetExtension(result);
				byte[] array = archive.ReadFile(result);
				if (extension.Equals(".tscn", StringComparison.OrdinalIgnoreCase))
				{
					if (!dictionary2.TryGetValue(result, out var value))
					{
						throw new InvalidDataException("在线场景缺少角色资源目标映射：" + result + "。");
					}
					array = SanitizeOnlineTextScene(value, result, array);
				}
				dictionary[result] = array;
				if (!TextExtensions.Contains(extension) && !BinaryResourceExtensions.Contains(extension))
				{
					continue;
				}
				if (TextExtensions.Contains(extension) && array.Length > 2097152)
				{
					throw new InvalidDataException("文本资源过大：" + result + "。");
				}
				string @string = Encoding.UTF8.GetString(array);
				if (TextExtensions.Contains(extension) && !extension.Equals(".tscn", StringComparison.OrdinalIgnoreCase) && ContainsForbiddenContent(@string))
				{
					throw new InvalidDataException("资源含脚本、Shader 或可执行引用：" + result + "。");
				}
				if (BinaryResourceExtensions.Contains(extension) && ForbiddenBinaryResourceRegex().IsMatch(@string))
				{
					throw new InvalidDataException("二进制场景内嵌了可执行脚本或扩展：" + result + "。");
				}
				foreach (Match item3 in ResourcePathRegex().Matches(@string))
				{
					string text2 = NormalizeResourcePath(item3.Value);
					if (!IsExecutableResourcePath(text2) && !SceneResourceChainRequiresExecutableResource(archive, text2, new HashSet<string>(StringComparer.OrdinalIgnoreCase)) && !SkinService.IsBaseGameResource(text2) && !SkinService.IsBaseGameResource(text2 + ".remap") && !SkinService.IsBaseGameResource(text2 + ".import"))
					{
						if (archive.Contains(text2))
						{
							Enqueue(text2);
						}
						else if (archive.Contains(text2 + ".remap"))
						{
							Enqueue(text2 + ".remap");
						}
						else if (archive.Contains(text2 + ".import"))
						{
							Enqueue(text2 + ".import");
						}
						else if (!extension.Equals(".spatlas", StringComparison.OrdinalIgnoreCase) || !text2.EndsWith(".atlas", StringComparison.OrdinalIgnoreCase))
						{
							throw new InvalidDataException("资源依赖未包含在同一工坊包中：" + text2 + "。");
						}
					}
				}
				if (!extension.Equals(".spatlas", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".atlas", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				string input = (extension.Equals(".spatlas", StringComparison.OrdinalIgnoreCase) ? @string.Replace("\\n", "\n", StringComparison.Ordinal) : @string);
				foreach (string item4 in from Match match in AtlasPageRegex().Matches(input)
					select match.Groups[1].Value)
				{
					string text3 = NormalizeResourcePath(result.Substring(0, result.LastIndexOf('/') + 1) + item4);
					if (archive.Contains(text3))
					{
						Enqueue(text3);
					}
					else if (archive.Contains(text3 + ".remap"))
					{
						Enqueue(text3 + ".remap");
					}
					else if (archive.Contains(text3 + ".import"))
					{
						Enqueue(text3 + ".import");
					}
				}
			}
			if (selected.Count == 0 || selected.Count > 2048)
			{
				throw new InvalidDataException("没有可安全隔离的静态角色资源，或资源数量超限。 ");
			}
			ulong num = 0uL;
			List<string> list = new List<string>(selected.Count);
			checked
			{
				foreach (string item5 in selected.Order<string>(StringComparer.OrdinalIgnoreCase))
				{
					byte[] array2 = dictionary[item5];
					ulong num2 = (ulong)array2.LongLength;
					num += num2;
					if (num2 > 67108864 || num > 134217728)
					{
						throw new InvalidDataException("安全资源缓存超过大小限制。 ");
					}
					string text4 = Convert.ToHexString(SHA256.HashData(array2));
					list.Add(item5.ToLowerInvariant() + "\n" + text4);
				}
				string fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("\n", list))));
				if (outputPath != null)
				{
					PckArchive.Write(outputPath, dictionary);
				}
				return new SafePackage(fingerprint, selected.Count, num);
			}
		}
		finally
		{
			if (archive != null)
			{
				((IDisposable)archive).Dispose();
			}
		}
		void Enqueue(string path)
		{
			string item = NormalizeResourcePath(path);
			if (selected.Add(item))
			{
				queue.Enqueue(item);
			}
		}
		void ValidateFile(string path)
		{
			string extension2 = Path.GetExtension(path);
			if (!AllowedExtensions.Contains(extension2))
			{
				throw new InvalidDataException("不允许在线加载此资源类型：" + path + "。");
			}
			if (!archive.Contains(path))
			{
				throw new InvalidDataException("资源包缺少文件：" + path + "。");
			}
			if (archive.GetFileSize(path) > 67108864)
			{
				throw new InvalidDataException("单个资源过大：" + path + "。");
			}
		}
	}

	private static bool IsValidSafeResourceManifest(string? manifest)
	{
		if (manifest == null || manifest.Length > 65536)
		{
			return false;
		}
		IReadOnlyList<string> readOnlyList = ParseSafeResourceManifest(manifest);
		if (readOnlyList.Count <= 2048)
		{
			return readOnlyList.All((string path) => path.StartsWith("res://", StringComparison.OrdinalIgnoreCase) && !path.Contains("..", StringComparison.Ordinal) && path.Length <= 512 && AllowedExtensions.Contains(Path.GetExtension(path)));
		}
		return false;
	}

	private static IReadOnlyList<string> FilterLocalSafeResourceRoots(IEnumerable<string> roots, out int ignoredCount)
	{
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		ignoredCount = 0;
		foreach (string root in roots)
		{
			string text = NormalizeResourcePath(root);
			if (!text.StartsWith("res://", StringComparison.OrdinalIgnoreCase) || text.Contains("..", StringComparison.Ordinal) || text.Length > 512 || !AllowedExtensions.Contains(Path.GetExtension(text)))
			{
				ignoredCount++;
			}
			else
			{
				hashSet.Add(text);
			}
		}
		return hashSet.Order<string>(StringComparer.OrdinalIgnoreCase).ToArray();
	}

	private static IReadOnlyDictionary<string, VisualResourceBinding> FilterLocalSafeResourceBindings(IReadOnlyDictionary<string, VisualResourceBinding> bindings, IReadOnlyCollection<string> acceptedRoots, string groupId, string sourcePck, out int baselineSceneFallbackCount)
	{
		baselineSceneFallbackCount = 0;
		HashSet<string> accepted = acceptedRoots.ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, VisualResourceBinding> dictionary = new Dictionary<string, VisualResourceBinding>(StringComparer.OrdinalIgnoreCase);
		PckArchive archive = PckArchive.Open(sourcePck, 100000u);
		try
		{
			foreach (KeyValuePair<string, VisualResourceBinding> binding in bindings)
			{
				string text = NormalizeResourcePath(binding.Key);
				string text2 = NormalizeResourcePath(binding.Value.SourcePath);
				if (!text.Contains("..", StringComparison.Ordinal) && text.Length <= 512 && !text2.Contains("..", StringComparison.Ordinal) && text2.Length <= 512 && text2.StartsWith("res://", StringComparison.OrdinalIgnoreCase) && SkinCatalog.IsSafeOnlineResourceRootForGroup(text, groupId))
				{
					string[] array = (from path in binding.Value.Files.Select(NormalizeResourcePath)
						where accepted.Contains(path)
						select path).Distinct<string>(StringComparer.OrdinalIgnoreCase).Order<string>(StringComparer.OrdinalIgnoreCase).ToArray();
					if (IsScenePath(text) && array.Any((string path) => SceneResourceChainRequiresExecutableResource(archive, path, new HashSet<string>(StringComparer.OrdinalIgnoreCase))))
					{
						baselineSceneFallbackCount++;
					}
					else if (array.Length != 0)
					{
						dictionary[text] = new VisualResourceBinding(text2, array);
					}
				}
			}
			return dictionary;
		}
		finally
		{
			if (archive != null)
			{
				((IDisposable)archive).Dispose();
			}
		}
	}

	private static bool SceneResourceChainRequiresExecutableResource(PckArchive archive, string path, HashSet<string> visited)
	{
		path = NormalizeResourcePath(path);
		if (!visited.Add(path) || !archive.Contains(path))
		{
			return false;
		}
		if (IsExecutableResourcePath(path) && !SkinService.IsBaseGameResource(path))
		{
			return true;
		}
		string extension = Path.GetExtension(path);
		string @string = Encoding.UTF8.GetString(archive.ReadFile(path));
		if (extension.Equals(".scn", StringComparison.OrdinalIgnoreCase))
		{
			string[] array = (from Match match in BinaryExecutableResourcePathRegex().Matches(@string)
				select NormalizeResourcePath(match.Value)).ToArray();
			if (array.Any((string path) => !SkinService.IsBaseGameResource(path)))
			{
				return true;
			}
			if (ForbiddenBinaryResourceRegex().IsMatch(@string) && array.Length == 0)
			{
				return true;
			}
		}
		if (!extension.Equals(".remap", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		return (from Match match in ResourcePathRegex().Matches(@string)
			select NormalizeResourcePath(match.Value)).Any((string dependency) => SceneResourceChainRequiresExecutableResource(archive, dependency, visited));
	}

	private static bool IsScenePath(string path)
	{
		if (!path.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase) && !path.EndsWith(".scn", StringComparison.OrdinalIgnoreCase) && !path.EndsWith(".tscn.remap", StringComparison.OrdinalIgnoreCase))
		{
			return path.EndsWith(".scn.remap", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static bool IsExecutableResourcePath(string path)
	{
		switch (Path.GetExtension(path).ToLowerInvariant())
		{
		case ".dll":
		case ".cs":
		case ".gd":
		case ".gdc":
		case ".gdshader":
		case ".gdextension":
			return true;
		default:
			return false;
		}
	}

	private static IReadOnlyList<string> FilterManifestRootsForBindings(IReadOnlyCollection<string> candidateRoots, IReadOnlyDictionary<string, VisualResourceBinding> originalBindings, IReadOnlyDictionary<string, VisualResourceBinding> filteredBindings)
	{
		HashSet<string> originalBoundRoots = originalBindings.Values.SelectMany((VisualResourceBinding binding) => binding.Files).Select(NormalizeResourcePath).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		HashSet<string> acceptedBoundRoots = filteredBindings.Values.SelectMany((VisualResourceBinding binding) => binding.Files).Select(NormalizeResourcePath).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		return candidateRoots.Where((string path) => !originalBoundRoots.Contains(path) || acceptedBoundRoots.Contains(path) || !IsScenePath(path)).Order<string>(StringComparer.OrdinalIgnoreCase).ToArray();
	}

	private static string SerializeSafeResourceBindings(IReadOnlyDictionary<string, VisualResourceBinding> bindings)
	{
		return string.Join('\n', bindings.OrderBy<KeyValuePair<string, VisualResourceBinding>, string>((KeyValuePair<string, VisualResourceBinding> binding) => binding.Key, StringComparer.OrdinalIgnoreCase).SelectMany((KeyValuePair<string, VisualResourceBinding> binding) => binding.Value.Files.Select((string resourcePath) => binding.Key + "\t" + binding.Value.SourcePath + "\t" + resourcePath)));
	}

	private static bool TryParseSafeResourceBindings(string? manifest, string groupId, out IReadOnlyDictionary<string, VisualResourceBinding> bindings)
	{
		bindings = new Dictionary<string, VisualResourceBinding>(StringComparer.OrdinalIgnoreCase);
		if (manifest == null || manifest.Length > 65536)
		{
			return false;
		}
		if (string.IsNullOrWhiteSpace(manifest))
		{
			return true;
		}
		Dictionary<string, (string, HashSet<string>)> dictionary = new Dictionary<string, (string, HashSet<string>)>(StringComparer.OrdinalIgnoreCase);
		string[] array = manifest.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		if (array.Length > 2048)
		{
			return false;
		}
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			string[] array3 = array2[i].Split('\t');
			if (array3.Length != 3)
			{
				return false;
			}
			string text = array3[0];
			string text2 = array3[1];
			string text3 = array3[2];
			if (!text.Equals(NormalizeResourcePath(text), StringComparison.Ordinal) || !text2.Equals(NormalizeResourcePath(text2), StringComparison.Ordinal) || !text3.Equals(NormalizeResourcePath(text3), StringComparison.Ordinal) || text.Contains("..", StringComparison.Ordinal) || text2.Contains("..", StringComparison.Ordinal) || text3.Contains("..", StringComparison.Ordinal) || text.Length > 512 || text2.Length > 512 || text3.Length > 512 || !text2.StartsWith("res://", StringComparison.OrdinalIgnoreCase) || !AllowedExtensions.Contains(Path.GetExtension(text3)) || !SkinCatalog.IsSafeOnlineResourceRootForGroup(text, groupId))
			{
				return false;
			}
			if (!dictionary.TryGetValue(text, out var value))
			{
				value = (dictionary[text] = (text2, new HashSet<string>(StringComparer.OrdinalIgnoreCase)));
			}
			else if (!value.Item1.Equals(text2, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			value.Item2.Add(text3);
		}
		bindings = dictionary.ToDictionary<KeyValuePair<string, (string, HashSet<string>)>, string, VisualResourceBinding>((KeyValuePair<string, (string SourcePath, HashSet<string> Files)> pair) => pair.Key, (KeyValuePair<string, (string SourcePath, HashSet<string> Files)> pair) => new VisualResourceBinding(pair.Value.SourcePath, pair.Value.Files.Order<string>(StringComparer.OrdinalIgnoreCase).ToArray()), StringComparer.OrdinalIgnoreCase);
		return true;
	}

	private static byte[] SanitizeOnlineTextScene(string sourcePath, string providerPath, byte[] providerBytes)
	{
		if (!SkinService.TryReadBaseGameResource(sourcePath, out byte[] bytes))
		{
			throw new InvalidDataException("在线场景的角色目标不属于游戏原始资源：" + sourcePath + "。");
		}
		string @string = Encoding.UTF8.GetString(providerBytes);
		string string2 = Encoding.UTF8.GetString(bytes);
		string[] baselineScripts = (from Match match in SceneExecutableResourceRegex().Matches(string2)
			where match.Groups[1].Value.Equals("Script", StringComparison.OrdinalIgnoreCase)
			select match.Groups[2].Value).Where(SkinService.IsBaseGameResource).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
		int replacementIndex = 0;
		@string = SceneExecutableResourceRegex().Replace(@string, delegate(Match match)
		{
			string value = match.Groups[1].Value;
			string value2 = match.Groups[2].Value;
			if (SkinService.IsBaseGameResource(value2))
			{
				return match.Value;
			}
			if (!value.Equals("Script", StringComparison.OrdinalIgnoreCase) || replacementIndex >= baselineScripts.Length)
			{
				throw new InvalidDataException($"在线场景含无法替换的自定义 {value}：{providerPath} -> {value2}。");
			}
			string newValue = baselineScripts[replacementIndex++];
			return match.Value.Replace(value2, newValue, StringComparison.Ordinal);
		});
		if (UnsafeTextSceneTokenRegex().IsMatch(@string) || SceneExecutableResourceRegex().Matches(@string).Cast<Match>().Any((Match match) => !SkinService.IsBaseGameResource(match.Groups[2].Value)))
		{
			throw new InvalidDataException("在线场景仍含不可安全加载的脚本、Shader 或扩展：" + providerPath + "。");
		}
		return Encoding.UTF8.GetBytes(@string);
	}

	private static IReadOnlyList<string> ParseSafeResourceManifest(string? manifest)
	{
		if (!string.IsNullOrWhiteSpace(manifest))
		{
			return manifest.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
		}
		return Array.Empty<string>();
	}

	private static bool ContainsForbiddenContent(string text)
	{
		return ForbiddenTextRegex().IsMatch(text);
	}

	private static string FindProviderPck(string workshopDirectory, string providerId)
	{
		string[] array = Directory.EnumerateFiles(workshopDirectory, "*.pck", SearchOption.TopDirectoryOnly).ToArray();
		string[] array2 = array.Where((string path) => Path.GetFileNameWithoutExtension(path).Equals(providerId, StringComparison.OrdinalIgnoreCase)).ToArray();
		string obj = ((array2.Length == 1) ? array2[0] : ((array.Length == 1) ? array[0] : null)) ?? throw new FileNotFoundException("下载内容中找不到唯一的皮肤 PCK。", providerId);
		if ((File.GetAttributes(obj) & FileAttributes.ReparsePoint) != 0)
		{
			throw new InvalidDataException("在线皮肤 PCK 不能是符号链接或重解析点。 ");
		}
		return obj;
	}

	private static bool TryGetWorkshopItemId(string path, out ulong workshopItemId)
	{
		workshopItemId = 0uL;
		string input = path.Replace('\\', '/');
		Match match = WorkshopPathRegex().Match(input);
		if (match.Success)
		{
			return ulong.TryParse(match.Groups[1].Value, out workshopItemId);
		}
		return false;
	}

	private static string NormalizeResourcePath(string path)
	{
		string text = path.Replace('\\', '/');
		string text2;
		if (!text.StartsWith("res://", StringComparison.OrdinalIgnoreCase))
		{
			text2 = text;
		}
		else
		{
			string text3 = text;
			text2 = text3.Substring(6, text3.Length - 6);
		}
		string text4 = text2;
		while (text4.Contains("//", StringComparison.Ordinal))
		{
			text4 = text4.Replace("//", "/", StringComparison.Ordinal);
		}
		return "res://" + text4.TrimStart('/');
	}

	private static string ProviderKey(SkinChangerNetMessage message)
	{
		return message.WorkshopItemId + "\n" + message.GroupId + "\n" + message.OptionId + "\n" + message.SafeResourceFingerprint;
	}

	private static string RequestKey(SkinChangerNetMessage message)
	{
		return message.PlayerNetId + "\n" + ProviderKey(message);
	}

	private static string BuildSessionOptionId(SkinChangerNetMessage message)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(message.WorkshopItemId + "\n" + message.GroupId + "\n" + message.SafeResourceFingerprint);
		return "__online_" + Convert.ToHexString(SHA256.HashData(bytes)).Substring(0, 24);
	}

	private static string OnlineCacheSuffix()
	{
		return ModLocalization.CurrentLanguage switch
		{
			"zhs" => " · 联机缓存", 
			"zht" => " · 連線快取", 
			"jpn" => " · オンラインキャッシュ", 
			"kor" => " · 온라인 캐시", 
			_ => " · Online cache", 
		};
	}

	private static void SetProgress(OnlineSkinCacheStage stage, string providerId, ulong workshopItemId, string detail = "", TimeSpan? lifetime = null)
	{
		lock (Sync)
		{
			SetProgressLocked(stage, providerId, workshopItemId, detail, lifetime);
		}
	}

	private static void SetLocalPreparationProgress(OnlineSkinCacheStage stage, string providerId, ulong workshopItemId, string detail = "", TimeSpan? lifetime = null)
	{
		lock (Sync)
		{
			SetLocalPreparationProgressLocked(stage, providerId, workshopItemId, detail, lifetime);
		}
	}

	private static void SetLocalPreparationProgressLocked(OnlineSkinCacheStage stage, string providerId, ulong workshopItemId, string detail = "", TimeSpan? lifetime = null)
	{
		OnlineSkinCacheStage progressStage = _progressStage;
		if ((uint)(progressStage - 2) > 4u)
		{
			SetProgressLocked(stage, providerId, workshopItemId, detail, lifetime);
		}
	}

	private static void SetProgressLocked(OnlineSkinCacheStage stage, string providerId, ulong workshopItemId, string detail = "", TimeSpan? lifetime = null)
	{
		_progressStage = stage;
		_progressProviderId = providerId;
		_progressWorkshopItemId = workshopItemId;
		_progressDetail = detail;
		_progressExpiresAtUtc = (lifetime.HasValue ? (DateTime.UtcNow + lifetime.Value) : DateTime.MaxValue);
	}

	private static void ResetProgressLocked()
	{
		_progressStage = OnlineSkinCacheStage.Idle;
		_progressProviderId = string.Empty;
		_progressWorkshopItemId = 0uL;
		_progressDetail = string.Empty;
		_progressExpiresAtUtc = DateTime.MinValue;
	}

	private static void CleanupOldSessionDirectories()
	{
		string path2 = Path.Combine(Path.GetTempPath(), "Gurio.SkinChanger", "online");
		if (!Directory.Exists(path2))
		{
			return;
		}
		int num = 0;
		foreach (string item in from path in Directory.EnumerateDirectories(path2)
			where !path.Equals(_sessionDirectory, StringComparison.OrdinalIgnoreCase)
			select path)
		{
			if (TryDeleteDirectory(item))
			{
				num++;
			}
		}
		if (num > 0)
		{
			ModLog.Info($"已清理 {num} 个过期的联机皮肤临时缓存目录。 ");
		}
	}

	private static bool TryDeleteDirectory(string? directory)
	{
		if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
		{
			return false;
		}
		try
		{
			Directory.Delete(directory, recursive: true);
			return true;
		}
		catch (Exception ex)
		{
			ModLog.Info("联机皮肤缓存仍被游戏资源系统占用，将在下次启动时清理：" + ex.GetBaseException().Message);
			return false;
		}
	}

	[GeneratedRegex("^[0-9A-Fa-f]{64}$")]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex FingerprintRegex()
	{
		return _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__FingerprintRegex_47.Instance;
	}

	[GeneratedRegex("/workshop/content/2868840/([0-9]+)(?:/|$)", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex WorkshopPathRegex()
	{
		return _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__WorkshopPathRegex_48.Instance;
	}

	[GeneratedRegex("res://[^\\x00\\\"'\\r\\n\\t\\]\\[(){}<>]+", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex ResourcePathRegex()
	{
		return _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__ResourcePathRegex_49.Instance;
	}

	[GeneratedRegex("(?im)^([^\\r\\n]+\\.(?:png|webp|jpe?g))\\s*$")]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex AtlasPageRegex()
	{
		return _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__AtlasPageRegex_50.Instance;
	}

	[GeneratedRegex("(?i)(?:ext_resource[^\\r\\n]*(?:Script|Shader)|script\\s*=|shader\\s*=|ShaderMaterial|GDExtension|type\\s*=\\s*[\\\"'](?:Shader|GDScript|CSharpScript|GDExtension|PackedScene)[\\\"']|\\.(?:dll|cs|gd|gdc|gdshader)(?:\\\"|'|\\s|$))")]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex ForbiddenTextRegex()
	{
		return _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__ForbiddenTextRegex_51.Instance;
	}

	[GeneratedRegex("(?i)(?:GDScript|CSharpScript|GDExtension|ExtensionLibrary|local://Shader_)")]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex ForbiddenBinaryResourceRegex()
	{
		return _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__ForbiddenBinaryResourceRegex_52.Instance;
	}

	[GeneratedRegex("(?i)res://[^\\x00]*?\\.(?:dll|cs|gd|gdc|gdshader|gdextension)(?=\\x00|$)")]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex BinaryExecutableResourcePathRegex()
	{
		return _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__BinaryExecutableResourcePathRegex_53.Instance;
	}

	[GeneratedRegex("(?im)^\\[ext_resource[^\\]]*type=\\\"(Script|Shader)\\\"[^\\]]*path=\\\"(res://[^\\\"]+)\\\"[^\\]]*\\]$")]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex SceneExecutableResourceRegex()
	{
		return _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__SceneExecutableResourceRegex_54.Instance;
	}

	[GeneratedRegex("(?i)(?:GDExtension|ExtensionLibrary|type\\s*=\\s*\\\"(?:GDScript|CSharpScript|Shader)\\\"|shader/code\\s*=|(?m)^code\\s*=)")]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex UnsafeTextSceneTokenRegex()
	{
		return _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__UnsafeTextSceneTokenRegex_55.Instance;
	}
}
