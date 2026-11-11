using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Multiplayer.Transport.Steam;
using STS2SkinChanger.Catalog;
using Steamworks;

namespace STS2SkinChanger.Core;

internal static class SkinWorkshopService
{
	private static readonly WorkshopCommunityCatalog Community = new WorkshopCommunityCatalog();

	private static WorkshopCatalogItem[]? _combinedItems;

	private static bool _communityStarted;

	private static bool _communityBusy;

	private static int _communityRevision;

	private static string _communityProgress = "";

	private static bool _communityFailed;

	private static readonly Lazy<WorkshopCatalogItem[]> Items = new Lazy<WorkshopCatalogItem[]>(delegate
	{
		using Stream stream = typeof(Entry).Assembly.GetManifestResourceStream("STS2SkinChanger.Data.workshop-catalog.json");
		using StreamReader streamReader = new StreamReader(stream);
		return WorkshopCatalogPolicy.Parse(streamReader.ReadToEnd());
	});

	private static WorkshopCatalogItem[]? _unionItems;

	private static (int Community, int GitHub) _unionRevision = (Community: -1, GitHub: -1);

	private static readonly Dictionary<ulong, WorkshopDownload> Downloads = new Dictionary<ulong, WorkshopDownload>();

	private static readonly Dictionary<ulong, TaskCompletionSource<EResult>> Waiters = new Dictionary<ulong, TaskCompletionSource<EResult>>();

	private static readonly WorkshopCatalogSession<WorkshopDetails> SessionDetails = new WorkshopCatalogSession<WorkshopDetails>();

	private static Callback<DownloadItemResult_t>? _downloadCallback;

	private static readonly HttpClient Images = new HttpClient(new HttpClientHandler
	{
		AllowAutoRedirect = false
	})
	{
		Timeout = TimeSpan.FromSeconds(20L)
	};

	private static readonly SemaphoreSlim ImageGate = new SemaphoreSlim(3);

	private static readonly ConditionalWeakTable<SkinCatalog, HashSet<ulong>> ActiveItems = new ConditionalWeakTable<SkinCatalog, HashSet<ulong>>();

	private static readonly WorkshopSessionCache<WorkshopIntroduction> Introductions = new WorkshopSessionCache<WorkshopIntroduction>();

	private static readonly SemaphoreSlim IntroductionGate = new SemaphoreSlim(2);

	public static int CommunityRevision => _communityRevision;

	public static bool CommunityBusy => _communityBusy;

	public static bool CommunityFailed => _communityFailed;

	public static string CommunityProgress => _communityProgress;

	public static IReadOnlyList<WorkshopCodeIssue> CommunityIssues => Community.State.Issues;

	public static IReadOnlyDictionary<ulong, int> CommunityReplies { get; private set; } = new Dictionary<ulong, int>();

	public static IReadOnlyList<WorkshopCatalogItem> Catalog
	{
		get
		{
			(int, int) tuple = (CommunityRevision, GitHubSkinService.Revision);
			if (_unionItems != null)
			{
				(int, int) unionRevision = _unionRevision;
				(int, int) tuple2 = tuple;
				if (unionRevision.Item1 == tuple2.Item1 && unionRevision.Item2 == tuple2.Item2)
				{
					return _unionItems;
				}
			}
			_unionRevision = tuple;
			_unionItems = SkinSourceRoutingPolicy.CatalogUnion(_combinedItems ?? Items.Value, GitHubSkinService.Items);
			return _unionItems;
		}
	}

	private static string CacheRoot => SkinChangerPaths.WorkshopCacheDirectory;

	public static IReadOnlyDictionary<ulong, WorkshopDetails> CachedMetadata => SessionDetails.Cached(WorkshopText.SteamLanguage);

	internal static void StartCommunityRefresh()
	{
		if (!_communityStarted)
		{
			_communityStarted = true;
			RefreshCommunity(restore: true);
		}
	}

	public static async Task RefreshCommunity(bool restore = false)
	{
		if (_communityBusy)
		{
			return;
		}
		_communityBusy = true;
		_communityFailed = false;
		_communityProgress = "";
		try
		{
			string path = Path.Combine(CacheRoot, "community-catalog-v2.json");
			if (restore)
			{
				try
				{
					await Community.Restore(path);
					PublishCommunity();
				}
				catch (Exception ex)
				{
					ModLog.Warn("社区清单缓存不可用：" + ex.Message);
				}
			}
			CancellationTokenSource timeout = new CancellationTokenSource(TimeSpan.FromMinutes(15L));
			try
			{
				Progress<(int Current, int Total)> progress = new Progress<(int, int)>(delegate((int Current, int Total) p)
				{
					_communityProgress = $"{p.Current}/{p.Total}";
				});
				await Community.Replace(async delegate
				{
					WorkshopCodeRead read = await Task.Run(async () => WorkshopSubmissionCodec.Read(await WorkshopDiscussionSource.ReadAllPosts(WorkshopDiscussionSource.Fetch, progress, timeout.Token)), timeout.Token);
					return WorkshopSubmissionIntegrity.Verify(read, await QuerySubmissionIdentities(read.Candidates.Select((WorkshopCodeCandidate c) => c.Payload.Item.Id).Concat(read.Issues.Select((WorkshopCodeIssue i) => i.Id)), timeout.Token), Community.State);
				}, path, SkinChangerPaths.WriteCache);
				PublishCommunity();
				ModLog.Info($"工坊投稿帖完整刷新成功：{Community.Items.Length} 项，码错误 {CommunityIssues.Count} 项，合并后 {Catalog.Count} 项；未订阅或执行任何投稿内容。");
			}
			finally
			{
				if (timeout != null)
				{
					((IDisposable)timeout).Dispose();
				}
			}
		}
		catch (Exception ex2)
		{
			_communityFailed = true;
			ModLog.Warn("工坊投稿帖刷新失败，保留上次完整清单：" + ex2.GetBaseException().Message);
		}
		finally
		{
			_communityBusy = false;
			_communityProgress = "";
		}
	}

	private static void PublishCommunity()
	{
		WorkshopCatalogItem[] combinedItems = WorkshopSubmissionCode.Merge(Items.Value, Community.Items);
		CommunityReplies = (from entry in Community.State.Entries.Where(delegate(WorkshopVerifiedSubmission entry)
			{
				int? reply = entry.Reply;
				return reply.HasValue && reply.GetValueOrDefault() >= 0;
			})
			group entry by entry.Item.Id).ToDictionary((IGrouping<ulong, WorkshopVerifiedSubmission> group) => group.Key, (IGrouping<ulong, WorkshopVerifiedSubmission> group) => group.Max((WorkshopVerifiedSubmission entry) => entry.Reply.Value));
		_combinedItems = combinedItems;
		_communityRevision++;
	}

	private static async Task<Dictionary<ulong, WorkshopIdentity>> QuerySubmissionIdentities(IEnumerable<ulong> ids, CancellationToken token)
	{
		Dictionary<ulong, WorkshopIdentity> valid = new Dictionary<ulong, WorkshopIdentity>();
		SteamUGCDetails_t val2 = default(SteamUGCDetails_t);
		foreach (ulong[] chunk in ids.Where((ulong id) => id != 0).Distinct().Chunk(100))
		{
			token.ThrowIfCancellationRequested();
			UGCQueryHandle_t handle = SteamUGC.CreateQueryUGCDetailsRequest(((IEnumerable<ulong>)chunk).Select((Func<ulong, PublishedFileId_t>)((ulong id) => new PublishedFileId_t(id))).ToArray(), (uint)chunk.Length);
			if (handle == UGCQueryHandle_t.Invalid)
			{
				throw new IOException("Steam item verification unavailable.");
			}
			try
			{
				using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
				timeout.CancelAfter(TimeSpan.FromSeconds(25L));
				SteamCallResult<SteamUGCQueryCompleted_t> call = new SteamCallResult<SteamUGCQueryCompleted_t>(SteamUGC.SendQueryUGCRequest(handle), timeout.Token);
				try
				{
					SteamUGCQueryCompleted_t val = await call.Task;
					if ((int)val.m_eResult != 1)
					{
						throw new IOException("Steam item verification: " + ((object)(EResult)(ref val.m_eResult)/*cast due to .constrained prefix*/).ToString());
					}
					if (val.m_unNumResultsReturned > chunk.Length)
					{
						throw new IOException("Unexpected Steam item verification results.");
					}
					for (uint num = 0u; num < val.m_unNumResultsReturned; num++)
					{
						if (!SteamUGC.GetQueryUGCResult(handle, num, ref val2))
						{
							throw new IOException("Incomplete Steam item verification.");
						}
						EResult eResult = val2.m_eResult;
						if (((int)eResult != 1 && eResult - 8 > 1 && (int)eResult != 15) || 1 == 0)
						{
							throw new IOException("Transient Steam item verification failure: " + ((object)(EResult)(ref val2.m_eResult)/*cast due to .constrained prefix*/).ToString());
						}
						if ((int)val2.m_eResult == 1 && chunk.Contains(val2.m_nPublishedFileId.m_PublishedFileId))
						{
							valid[val2.m_nPublishedFileId.m_PublishedFileId] = new WorkshopIdentity(val2.m_nConsumerAppID.m_AppId, ((SteamUGCDetails_t)(ref val2)).m_rgchTitle ?? "");
						}
					}
				}
				finally
				{
					((IDisposable)call)?.Dispose();
				}
			}
			finally
			{
				SteamUGC.ReleaseQueryUGCRequest(handle);
			}
		}
		return valid;
	}

	public static async Task<WorkshopLocalSubmission[]> SubmissionCandidates(CancellationToken token)
	{
		HashSet<ulong> hashSet = WorkshopSubmissionCandidatePolicy.EligibleIds(SkinService.Catalog?.ExportWorkshopCatalog() ?? Array.Empty<WorkshopCatalogItem>(), Catalog);
		List<(ulong Id, string Path)> installed = new List<(ulong, string)>();
		foreach (ulong item in hashSet)
		{
			token.ThrowIfCancellationRequested();
			if (IsSubscribed(item) && TryInstalled(item, out string directory))
			{
				installed.Add((item, directory));
			}
		}
		return FilterSubmissionCandidates(await Task.Run(delegate
		{
			List<WorkshopLocalSubmission> list = new List<WorkshopLocalSubmission>();
			foreach (var item2 in installed)
			{
				token.ThrowIfCancellationRequested();
				try
				{
					string[] array = WorkshopSubmissionScanner.SubmittableNames(item2.Path);
					if (array.Length != 0)
					{
						list.Add(new WorkshopLocalSubmission(item2.Id, string.Join(" / ", array), item2.Path));
					}
				}
				catch (Exception ex) when (!(ex is OperationCanceledException))
				{
					ModLog.Info($"投稿候选 {item2.Id} 跳过：{ex.Message}");
				}
			}
			return list.OrderBy<WorkshopLocalSubmission, string>((WorkshopLocalSubmission c) => c.Name, StringComparer.CurrentCulture).ToArray();
		}, token));
	}

	internal static WorkshopLocalSubmission[] FilterSubmissionCandidates(IEnumerable<WorkshopLocalSubmission> candidates)
	{
		return WorkshopSubmissionCandidatePolicy.Filter(candidates, SkinService.Catalog?.ExportWorkshopCatalog() ?? Array.Empty<WorkshopCatalogItem>(), Catalog);
	}

	public static async Task<(string[] Codes, int Accepted, int Rejected)> ScanSubmissions(WorkshopLocalSubmission[] selected, IProgress<(int Current, int Total, string Name)> progress, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		string directory;
		WorkshopLocalSubmission[] installed = (from s in FilterSubmissionCandidates(selected)
			where IsSubscribed(s.Id) && TryInstalled(s.Id, out directory) && Path.GetFullPath(directory).Equals(Path.GetFullPath(s.Directory), StringComparison.OrdinalIgnoreCase)
			select s).ToArray();
		if (installed.Length == 0)
		{
			return (Codes: Array.Empty<string>(), Accepted: 0, Rejected: selected.Length);
		}
		(string GamePack, CardCatalogEntry[] Cards, SkinModDescriptor[] Baselines) snapshot = SkinService.CaptureWorkshopScanContext();
		string gameVersion = AccessTools.Field(typeof(ModManager), "_gameVersion")?.GetValue(null)?.ToString() ?? "";
		Dictionary<string, string> dependencies = ModManager.Mods.Where((Mod m) => (int)m.state == 1 && m.manifest?.id != null).GroupBy<Mod, string>((Mod m) => m.manifest.id, StringComparer.OrdinalIgnoreCase).ToDictionary<IGrouping<string, Mod>, string, string>((IGrouping<string, Mod> g) => g.Key, (IGrouping<string, Mod> g) => g.First().manifest.version ?? "", StringComparer.OrdinalIgnoreCase);
		return await SkinService.RunWorkshopScan(delegate
		{
			List<WorkshopCatalogItem> list = new List<WorkshopCatalogItem>();
			List<string> list2 = new List<string>();
			int num = selected.Length - installed.Length;
			for (int i = 0; i < installed.Length; i++)
			{
				token.ThrowIfCancellationRequested();
				WorkshopLocalSubmission workshopLocalSubmission = installed[i];
				progress.Report((i, installed.Length, workshopLocalSubmission.Name));
				WorkshopScanResult workshopScanResult = WorkshopSubmissionScanner.Scan(workshopLocalSubmission, snapshot.GamePack, snapshot.Cards, gameVersion, dependencies, snapshot.Baselines);
				if (workshopScanResult.Item != null)
				{
					try
					{
						list2.AddRange(WorkshopSubmissionCodec.Encode(workshopScanResult.Item, "1.0.27.70", gameVersion));
						list.Add(workshopScanResult.Item);
					}
					catch (Exception ex) when (!(ex is OperationCanceledException))
					{
						num++;
						ModLog.Warn($"投稿编码 {workshopLocalSubmission.Id} 失败：{ex.GetBaseException().Message}");
					}
				}
				else
				{
					num++;
					ModLog.Info($"投稿扫描 {workshopLocalSubmission.Id} 未识别：{workshopScanResult.Error}");
				}
				progress.Report((i + 1, installed.Length, workshopLocalSubmission.Name));
			}
			token.ThrowIfCancellationRequested();
			return (list2.ToArray(), Count: list.Count, rejected: num);
		}, token);
	}

	public static WorkshopDetails? CachedDetails(ulong id)
	{
		return CachedMetadata.GetValueOrDefault(id);
	}

	internal static bool CanReadMetadata(ulong id)
	{
		if (SkinSourceRoutingPolicy.IsSteamItem(id))
		{
			if (!Catalog.Any((WorkshopCatalogItem item) => item.Id == id))
			{
				return CommunityIssues.Any((WorkshopCodeIssue issue) => issue.Id == id);
			}
			return true;
		}
		return false;
	}

	public static async Task<Dictionary<ulong, WorkshopDetails>> Query(IReadOnlyList<ulong> ids, CancellationToken token)
	{
		ulong[] requested = ids.Where(CanReadMetadata).Distinct().Take(12)
			.ToArray();
		if (requested.Length == 0)
		{
			return new Dictionary<ulong, WorkshopDetails>();
		}
		ulong[] ids2 = WorkshopCodeDiagnostics.MetadataIds(Catalog, CommunityIssues.Where((WorkshopCodeIssue issue) => requested.Contains(issue.Id)));
		string language = WorkshopText.SteamLanguage;
		Dictionary<ulong, WorkshopDetails> details = await SessionDetails.Get(language, ids2, (ulong[] missing) => RefreshDetails(language, missing), token);
		return requested.Where(details.ContainsKey).ToDictionary((ulong id) => id, (ulong id) => details[id]);
	}

	private static async Task<Dictionary<ulong, WorkshopDetails>> RefreshDetails(string language, ulong[] missing)
	{
		string cache = Path.Combine(CacheRoot, language);
		Dictionary<ulong, WorkshopDetails> result = new Dictionary<ulong, WorkshopDetails>();
		ulong[] array = missing;
		for (int i = 0; i < array.Length; i++)
		{
			ulong id2 = array[i];
			try
			{
				string text = Path.Combine(cache, id2 + ".json");
				if (!File.Exists(text) || new FileInfo(text).Length >= 32768)
				{
					continue;
				}
				WorkshopDetails workshopDetails = JsonSerializer.Deserialize<WorkshopDetails>(await File.ReadAllTextAsync(text));
				if ((object)workshopDetails != null)
				{
					string title = workshopDetails.Title;
					if (title != null && title.Length > 0)
					{
						result[id2] = workshopDetails;
					}
				}
			}
			catch (IOException)
			{
			}
			catch (UnauthorizedAccessException)
			{
			}
			catch (JsonException)
			{
			}
		}
		SteamUGCDetails_t item = default(SteamUGCDetails_t);
		string text2 = default(string);
		foreach (ulong[] item2 in missing.Chunk(1000))
		{
			UGCQueryHandle_t handle = UGCQueryHandle_t.Invalid;
			uint i2;
			try
			{
				try
				{
					handle = SteamUGC.CreateQueryUGCDetailsRequest(((IEnumerable<ulong>)item2).Select((Func<ulong, PublishedFileId_t>)((ulong id) => new PublishedFileId_t(id))).ToArray(), (uint)item2.Length);
					SteamUGC.SetLanguage(handle, language);
					using CancellationTokenSource timeout = new CancellationTokenSource(TimeSpan.FromSeconds(25L));
					SteamCallResult<SteamUGCQueryCompleted_t> call = new SteamCallResult<SteamUGCQueryCompleted_t>(SteamUGC.SendQueryUGCRequest(handle), timeout.Token);
					try
					{
						SteamUGCQueryCompleted_t completed = await call.Task;
						if ((int)completed.m_eResult != 1)
						{
							continue;
						}
						i2 = 0u;
						while (true)
						{
							if (i2 < completed.m_unNumResultsReturned)
							{
								if (SteamUGC.GetQueryUGCResult(handle, i2, ref item) && (int)item.m_eResult == 1 && item2.Contains(item.m_nPublishedFileId.m_PublishedFileId) && (item.m_nConsumerAppID.m_AppId == 2868840 || CommunityIssues.Any((WorkshopCodeIssue issue) => issue.Id == item.m_nPublishedFileId.m_PublishedFileId)))
								{
									SteamUGC.GetQueryUGCPreviewURL(handle, i2, ref text2, 4096u);
									WorkshopDetails details = WorkshopMetadataReader.Read(item, text2 ?? "", Statistic);
									ulong publishedFileId = item.m_nPublishedFileId.m_PublishedFileId;
									result[publishedFileId] = details;
									try
									{
										string metadataPath = Path.Combine(cache, publishedFileId + ".json");
										await Task.Run(delegate
										{
											SkinChangerPaths.WriteCache(metadataPath, delegate
											{
												File.WriteAllText(metadataPath, JsonSerializer.Serialize(details));
											});
										});
									}
									catch (IOException)
									{
									}
									catch (UnauthorizedAccessException)
									{
									}
								}
								i2++;
								continue;
							}
							goto end_IL_01c9;
						}
					}
					finally
					{
						((IDisposable)call)?.Dispose();
					}
				}
				catch (Exception ex6)
				{
					ModLog.Info("工坊详情暂不可用，保留缓存：" + ex6.Message);
					goto end_IL_01c9;
				}
				end_IL_01c9:;
			}
			finally
			{
				if (handle != UGCQueryHandle_t.Invalid)
				{
					SteamUGC.ReleaseQueryUGCRequest(handle);
				}
			}
			ulong? Statistic(EItemStatistic statistic)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				ulong value = default(ulong);
				if (!SteamUGC.GetQueryUGCStatistic(handle, i2, statistic, ref value))
				{
					return null;
				}
				return value;
			}
		}
		ModLog.Info($"工坊资料增量刷新结束：{language}，{result.Count}/{missing.Length} 项；已有物品筛选/翻页复用内存缓存。");
		return result;
	}

	public static async Task<byte[]?> Cover(string url, CancellationToken token)
	{
		if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri) || uri.Scheme != "https" || (!uri.Host.EndsWith(".steamstatic.com", StringComparison.OrdinalIgnoreCase) && !uri.Host.EndsWith(".steamusercontent.com", StringComparison.OrdinalIgnoreCase) && !uri.Host.EndsWith(".akamaihd.net", StringComparison.OrdinalIgnoreCase) && !uri.Host.Equals("opengraph.githubassets.com", StringComparison.OrdinalIgnoreCase)))
		{
			return null;
		}
		await ImageGate.WaitAsync(token);
		try
		{
			string cache = Path.Combine(CacheRoot, "covers");
			string cachePath = Path.Combine(cache, Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(url))) + ".img");
			if (File.Exists(cachePath) && new FileInfo(cachePath).Length <= 2097152)
			{
				byte[] array = await File.ReadAllBytesAsync(cachePath, token);
				if (WorkshopCatalogPolicy.IsSafeCover(array))
				{
					return array;
				}
			}
			using (HttpResponseMessage response = await Images.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, token))
			{
				if (!response.IsSuccessStatusCode || response.Content.Headers.ContentLength > 2097152)
				{
					return null;
				}
				byte[] result;
				await using (Stream input = await response.Content.ReadAsStreamAsync(token))
				{
					using MemoryStream output = new MemoryStream();
					byte[] buffer = new byte[32768];
					byte[] bytes;
					while (true)
					{
						int num;
						if ((num = await input.ReadAsync(buffer, token)) > 0)
						{
							if (output.Length + num > 2097152)
							{
								result = null;
								break;
							}
							output.Write(buffer, 0, num);
							continue;
						}
						bytes = output.ToArray();
						if (!WorkshopCatalogPolicy.IsSafeCover(bytes))
						{
							result = null;
							break;
						}
						try
						{
							await Task.Run(delegate
							{
								SkinChangerPaths.WriteCache(cachePath, delegate
								{
									File.WriteAllBytes(cachePath, bytes);
									foreach (FileInfo item in (from file in new DirectoryInfo(cache).EnumerateFiles("*.img")
										orderby file.LastWriteTimeUtc descending
										select file).ToArray().Skip(128))
									{
										item.Delete();
									}
								});
							}, token);
						}
						catch (IOException)
						{
						}
						result = bytes;
						break;
					}
				}
				return result;
			}
			IL_0652:;
		}
		catch (HttpRequestException)
		{
			return null;
		}
		finally
		{
			ImageGate.Release();
		}
		byte[] result2;
		return result2;
	}

	public static bool IsActive(ulong id)
	{
		SkinCatalog catalog = SkinService.Catalog;
		if (catalog != null)
		{
			return ActiveItems.GetValue(catalog, (SkinCatalog c) => (from item in c.ExportWorkshopCatalog()
				select item.Id).ToHashSet()).Contains(id);
		}
		return false;
	}

	public static WorkshopDownload? DownloadState(ulong id)
	{
		return Downloads.GetValueOrDefault(id);
	}

	internal static bool IsHotRegisteredResourceMod(Mod mod)
	{
		WorkshopDownload? valueOrDefault = Downloads.GetValueOrDefault(SkinCatalog.WorkshopSourceId(mod.path));
		if (valueOrDefault == null)
		{
			return false;
		}
		return valueOrDefault.State == WorkshopTextKey.Ready;
	}

	public static string LoadTag(WorkshopCatalogItem item)
	{
		if (!SkinSourceRoutingPolicy.IsSteamItem(item.Id))
		{
			return WorkshopLoadTags.Classify(knownRestart: true, null, WorkshopLoadReason.None);
		}
		WorkshopDownload workshopDownload = DownloadState(item.Id);
		WorkshopTextKey? state = ((workshopDownload != null && workshopDownload.State == WorkshopTextKey.Ready && !workshopDownload.HotLoadVerified) ? ((WorkshopTextKey?)null) : workshopDownload?.State);
		return WorkshopLoadTags.Classify(item.RestartRequired, state, workshopDownload?.Reason ?? WorkshopLoadReason.None);
	}

	public static bool IsSubscribed(ulong id)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (SkinSourceRoutingPolicy.IsSteamItem(id))
		{
			return (SteamUGC.GetItemState(new PublishedFileId_t(id)) & 1) != 0;
		}
		return false;
	}

	public static bool IsInstalled(ulong id)
	{
		if (!SkinSourceRoutingPolicy.IsSteamItem(id))
		{
			return GitHubSkinService.InstallState(id) != GitHubInstallState.NotInstalled;
		}
		string directory;
		return TryInstalled(id, out directory);
	}

	public static double? Progress(ulong id)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		ulong num = default(ulong);
		ulong num2 = default(ulong);
		if (!SkinSourceRoutingPolicy.IsSteamItem(id) || !SteamUGC.GetItemDownloadInfo(new PublishedFileId_t(id), ref num, ref num2) || num2 == 0)
		{
			return null;
		}
		return Math.Clamp(100.0 * (double)num / (double)num2, 0.0, 100.0);
	}

	public static async Task Subscribe(ulong id)
	{
		if (!SkinSourceRoutingPolicy.IsSteamItem(id) || !Catalog.Any((WorkshopCatalogItem item) => item.Id == id) || (Downloads.GetValueOrDefault(id)?.Busy ?? false))
		{
			return;
		}
		WorkshopDownload status = new WorkshopDownload
		{
			Busy = true,
			BrowserSubscription = true
		};
		Downloads[id] = status;
		try
		{
			if (_downloadCallback == null)
			{
				_downloadCallback = Callback<DownloadItemResult_t>.Create((DispatchDelegate<DownloadItemResult_t>)delegate(DownloadItemResult_t ev)
				{
					//IL_0000: Unknown result type (might be due to invalid IL or missing references)
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					//IL_0017: Unknown result type (might be due to invalid IL or missing references)
					//IL_0018: Unknown result type (might be due to invalid IL or missing references)
					//IL_002c: Unknown result type (might be due to invalid IL or missing references)
					//IL_002d: Unknown result type (might be due to invalid IL or missing references)
					if (ev.m_unAppID.m_AppId == 2868840 && Waiters.TryGetValue(ev.m_nPublishedFileId.m_PublishedFileId, out TaskCompletionSource<EResult> value))
					{
						value.TrySetResult(ev.m_eResult);
					}
				});
			}
			using CancellationTokenSource timeout = new CancellationTokenSource(TimeSpan.FromMinutes(15L));
			PublishedFileId_t itemId = new PublishedFileId_t(id);
			SteamCallResult<RemoteStorageSubscribePublishedFileResult_t> subscribed = new SteamCallResult<RemoteStorageSubscribePublishedFileResult_t>(SteamUGC.SubscribeItem(itemId), timeout.Token);
			try
			{
				RemoteStorageSubscribePublishedFileResult_t val = await subscribed.Task;
				if ((int)val.m_eResult != 1 || val.m_nPublishedFileId != itemId)
				{
					throw new IOException(((object)(EResult)(ref val.m_eResult)/*cast due to .constrained prefix*/).ToString());
				}
			}
			finally
			{
				((IDisposable)subscribed)?.Dispose();
			}
			if (!TryInstalled(id, out string _))
			{
				TaskCompletionSource<EResult> taskCompletionSource = new TaskCompletionSource<EResult>(TaskCreationOptions.RunContinuationsAsynchronously);
				Waiters[id] = taskCompletionSource;
				if (!SteamUGC.DownloadItem(itemId, false))
				{
					throw new IOException("Steam DownloadItem rejected.");
				}
				if ((int)(await taskCompletionSource.Task.WaitAsync(timeout.Token)) != 1)
				{
					throw new IOException("Steam download failed.");
				}
			}
			if (!TryInstalled(id, out string directory2))
			{
				throw new IOException("Steam install is not complete.");
			}
			status.State = WorkshopTextKey.Checking;
			bool alreadyActive = IsActive(id);
			WorkshopDownload workshopDownload = status;
			WorkshopLoadReason reason = ((!alreadyActive) ? (await SkinService.TryRegisterWorkshopResources(directory2)) : WorkshopLoadReason.None);
			workshopDownload.Reason = reason;
			status.HotLoadVerified = !alreadyActive && status.Reason == WorkshopLoadReason.None;
			status.State = ((status.Reason == WorkshopLoadReason.None) ? WorkshopTextKey.Ready : ((status.Reason == WorkshopLoadReason.InvalidPackage) ? WorkshopTextKey.Failed : WorkshopTextKey.Restart));
			ModLog.Info($"工坊皮肤 {id}：{status.State}，原因={status.Reason}；安装目录 {directory2}");
		}
		catch (Exception ex)
		{
			if (status.State == WorkshopTextKey.Checking)
			{
				status.Reason = WorkshopLoadReason.IncompleteResources;
			}
			status.State = ((status.State == WorkshopTextKey.Checking) ? WorkshopTextKey.Restart : WorkshopTextKey.Failed);
			status.Error = ex.Message;
			ModLog.Error($"工坊皮肤 {id} 下载/检查失败：{ex}");
		}
		finally
		{
			status.Busy = false;
			Waiters.Remove(id);
			ModLog.Info($"工坊皮肤 {id} 结果保留在标签和操作按钮中；本面板订阅不弹重启提示。");
		}
	}

	public static async Task Unsubscribe(ulong id)
	{
		if (!SkinSourceRoutingPolicy.IsSteamItem(id) || !Catalog.Any((WorkshopCatalogItem item) => item.Id == id) || (Downloads.GetValueOrDefault(id)?.Busy ?? false))
		{
			return;
		}
		WorkshopDownload status = Downloads.GetValueOrDefault(id) ?? new WorkshopDownload();
		Downloads[id] = status;
		status.Busy = true;
		status.Removing = true;
		status.Error = "";
		try
		{
			using CancellationTokenSource timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30L));
			SteamCallResult<RemoteStorageUnsubscribePublishedFileResult_t> call = new SteamCallResult<RemoteStorageUnsubscribePublishedFileResult_t>(SteamUGC.UnsubscribeItem(new PublishedFileId_t(id)), timeout.Token);
			try
			{
				RemoteStorageUnsubscribePublishedFileResult_t val = await call.Task;
				if ((int)val.m_eResult != 1 || val.m_nPublishedFileId.m_PublishedFileId != id)
				{
					throw new IOException(((object)(EResult)(ref val.m_eResult)/*cast due to .constrained prefix*/).ToString());
				}
				status.Unsubscribed = true;
				ModLog.Info($"已取消订阅工坊皮肤 {id}；本次已加载资源保留，文件移除由 Steam 在退出后处理。");
			}
			finally
			{
				((IDisposable)call)?.Dispose();
			}
		}
		catch (Exception ex)
		{
			status.Error = ex.Message;
			ModLog.Warn($"取消订阅工坊皮肤 {id} 失败：{ex.Message}");
		}
		finally
		{
			status.Busy = false;
			status.Removing = false;
		}
	}

	private static bool TryInstalled(ulong id, out string directory)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		EItemState val = (EItemState)SteamUGC.GetItemState(new PublishedFileId_t(id));
		directory = "";
		ulong num = default(ulong);
		uint num2 = default(uint);
		if (WorkshopCatalogPolicy.CanUseInstalledFiles(((Enum)val).HasFlag((Enum)(object)(EItemState)4), ((Enum)val).HasFlag((Enum)(object)(EItemState)8), ((Enum)val).HasFlag((Enum)(object)(EItemState)16), ((Enum)val).HasFlag((Enum)(object)(EItemState)32)) && SteamUGC.GetItemInstallInfo(new PublishedFileId_t(id), ref num, ref directory, 4096u, ref num2))
		{
			return Directory.Exists(directory);
		}
		return false;
	}

	internal static bool DeferNativeNotice(Mod mod)
	{
		ulong key = SkinCatalog.WorkshopSourceId(mod.path);
		return Downloads.GetValueOrDefault(key)?.BrowserSubscription ?? false;
	}

	public static Task<WorkshopIntroduction> Introduction(ulong id, CancellationToken token)
	{
		if (!CanReadMetadata(id))
		{
			return Task.FromResult(new WorkshopIntroduction("", Array.Empty<string>(), Available: false));
		}
		string language = WorkshopText.SteamLanguage;
		return Introductions.Get(language + ":" + id, () => LoadIntroduction(id, language), token);
	}

	private static async Task<WorkshopIntroduction> LoadIntroduction(ulong id, string language)
	{
		await IntroductionGate.WaitAsync();
		UGCQueryHandle_t handle = UGCQueryHandle_t.Invalid;
		try
		{
			handle = SteamUGC.CreateQueryUGCDetailsRequest((PublishedFileId_t[])(object)new PublishedFileId_t[1]
			{
				new PublishedFileId_t(id)
			}, 1u);
			if (handle == UGCQueryHandle_t.Invalid || !SteamUGC.SetLanguage(handle, language) || !SteamUGC.SetReturnLongDescription(handle, true) || !SteamUGC.SetReturnAdditionalPreviews(handle, true))
			{
				return new WorkshopIntroduction("", Array.Empty<string>(), Available: false);
			}
			using CancellationTokenSource timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15L));
			SteamCallResult<SteamUGCQueryCompleted_t> call = new SteamCallResult<SteamUGCQueryCompleted_t>(SteamUGC.SendQueryUGCRequest(handle), timeout.Token);
			try
			{
				SteamUGCQueryCompleted_t val = await call.Task;
				SteamUGCDetails_t val2 = default(SteamUGCDetails_t);
				if ((int)val.m_eResult != 1 || val.m_unNumResultsReturned != 1 || !SteamUGC.GetQueryUGCResult(handle, 0u, ref val2) || (int)val2.m_eResult != 1 || val2.m_nPublishedFileId.m_PublishedFileId != id || (val2.m_nConsumerAppID.m_AppId != 2868840 && !CommunityIssues.Any((WorkshopCodeIssue issue) => issue.Id == id)))
				{
					return new WorkshopIntroduction("", Array.Empty<string>(), Available: false);
				}
				List<string> list = new List<string>();
				uint num = Math.Min(64u, SteamUGC.GetQueryUGCNumAdditionalPreviews(handle, 0u));
				string item = default(string);
				string text = default(string);
				EItemPreviewType val3 = default(EItemPreviewType);
				for (uint num2 = 0u; num2 < num; num2++)
				{
					if (SteamUGC.GetQueryUGCAdditionalPreview(handle, 0u, num2, ref item, 4096u, ref text, 1024u, ref val3) && (int)val3 == 0)
					{
						list.Add(item);
					}
				}
				string text2 = default(string);
				SteamUGC.GetQueryUGCPreviewURL(handle, 0u, ref text2, 4096u);
				return new WorkshopIntroduction(WorkshopIntroductionPolicy.Summary(((SteamUGCDetails_t)(ref val2)).m_rgchDescription), WorkshopIntroductionPolicy.Images(text2 ?? "", list));
			}
			finally
			{
				((IDisposable)call)?.Dispose();
			}
		}
		catch (Exception ex)
		{
			ModLog.Info($"工坊皮肤 {id} 简介暂不可用：{ex.Message}");
			return new WorkshopIntroduction("", Array.Empty<string>(), Available: false);
		}
		finally
		{
			if (handle != UGCQueryHandle_t.Invalid)
			{
				SteamUGC.ReleaseQueryUGCRequest(handle);
			}
			IntroductionGate.Release();
		}
	}
}
