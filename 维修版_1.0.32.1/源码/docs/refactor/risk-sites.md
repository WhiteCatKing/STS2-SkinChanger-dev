# 风险点清单（②）

基线 `092ec98 (1.0.31.93)`。位置 = `文件:行号`；方法名由向上回溯最近声明得到，仅供参考。

## 1. 写盘（80 处）

口径按任务书：`File.Write*/Copy/Move/Create/AppendAll*`、`new FileStream`、`OpenWrite`、`PckArchive.Write*`、`Directory.CreateDirectory`。
另有 **48 处** `资源/配置 .Save(...)`（Godot `Resource.Save`、配置序列化）不在本表口径内，但它们同样落盘，`DiskGuard` 接入时要一起考虑；**12 处** `new FileStream(..., FileAccess.Read)` 是只读的 IL/元数据扫描，已从表里剔除。

| 位置 | 所在方法 | API | 写的是什么 | 上限 |
|---|---|---|---|---|
| `STS2SkinChanger/Core/CharacterSkinBundleRunState.cs:41` | Save | Directory.CreateDirectory | 目录创建（不写数据） | ⚠ 无 |
| `STS2SkinChanger/Core/CharacterSkinBundleRunState.cs:43` | Save | new FileStream( | 日志 | ⚠ 无 |
| `STS2SkinChanger/Core/CharacterSkinBundleRunState.cs:48` | Save | File.Move | 日志 | ⚠ 无 |
| `STS2SkinChanger/Core/CharacterSkinBundleRunState.cs:49` | Save | File.Copy | 日志 | ⚠ 无 |
| `STS2SkinChanger/Core/DebugSnapshot.cs:74` | PollWatch | File.Delete | 日志 | ⚠ 无 |
| `STS2SkinChanger/Core/DebugSnapshot.cs:110` | PollWatch | Directory.CreateDirectory | 目录创建（不写数据） | ⚠ 无 |
| `STS2SkinChanger/Core/DebugSnapshot.cs:144` | PollWatch | File.AppendAllText | 日志 | ⚠ 无 |
| `STS2SkinChanger/Core/DebugSnapshot.cs:204` | OnFrame | File.Delete | 其他/未分类 | ⚠ 无 |
| `STS2SkinChanger/Core/DebugSnapshot.cs:233` | PollPick | File.Delete | 其他/未分类 | ⚠ 无 |
| `STS2SkinChanger/Core/DebugSnapshot.cs:293` | OnPostDraw | Directory.CreateDirectory | 目录创建（不写数据） | ⚠ 无 |
| `STS2SkinChanger/Core/GitHubFolderSwap.cs:30` | InvalidOperationException | Directory.CreateDirectory | 目录创建（不写数据） | ⚠ 无 |
| `STS2SkinChanger/Core/GitHubFolderSwap.cs:83` | Disable | File.Move | 其他/未分类 | ⚠ 无 |
| `STS2SkinChanger/Core/GitHubFolderSwap.cs:188` | CopyDirectory | Directory.CreateDirectory | 目录创建（不写数据） | ⚠ 无 |
| `STS2SkinChanger/Core/GitHubFolderSwap.cs:194` | CopyDirectory | Directory.CreateDirectory | 目录创建（不写数据） | ⚠ 无 |
| `STS2SkinChanger/Core/GitHubFolderSwap.cs:195` | CopyDirectory | File.Copy | 其他/未分类 | ⚠ 无 |
| `STS2SkinChanger/Core/GitHubInstallLedger.cs:133` | GitHubInstallLedgerState | Directory.CreateDirectory | 目录创建（不写数据） | 文件内另有上限常量 |
| `STS2SkinChanger/Core/GitHubInstallLedger.cs:135` | GitHubInstallLedgerState | File.WriteAllText | 其他/未分类 | 文件内另有上限常量 |
| `STS2SkinChanger/Core/GitHubInstallLedger.cs:136` | GitHubInstallLedgerState | File.Move | 其他/未分类 | 文件内另有上限常量 |
| `STS2SkinChanger/Core/GitHubSkinCatalog.cs:111` | Save | Directory.CreateDirectory | 目录创建（不写数据） | MaxCacheBytes |
| `STS2SkinChanger/Core/GitHubSkinCatalog.cs:113` | Save | File.WriteAllText | 缓存 | MaxEntries |
| `STS2SkinChanger/Core/GitHubSkinCatalog.cs:114` | Save | File.Move | 缓存 | MaxEntries |
| `STS2SkinChanger/Core/GitHubSkinClient.cs:123` | InvalidDataException | Directory.CreateDirectory | 目录创建（不写数据） | MaxDownloadBytes |
| `STS2SkinChanger/Core/GitHubSkinClient.cs:128` | InvalidDataException | File.Create | 下载 | MaxDownloadBytes |
| `STS2SkinChanger/Core/GitHubSkinClient.cs:139` | InvalidDataException | File.Move | 下载 | MaxDownloadBytes |
| `STS2SkinChanger/Core/GitHubSkinClient.cs:143` | InvalidDataException | File.Delete | 其他/未分类 | 文件内另有上限常量 |
| `STS2SkinChanger/Core/GitHubSkinService.cs:236` | InvalidOperationException | Directory.CreateDirectory | 目录创建（不写数据） | ⚠ 无 |
| `STS2SkinChanger/Core/GitHubSkinService.cs:266` | Extract | Directory.CreateDirectory | 目录创建（不写数据） | ⚠ 无 |
| `STS2SkinChanger/Core/GitHubSkinService.cs:280` | InvalidDataException | Directory.CreateDirectory | 目录创建（不写数据） | ⚠ 无 |
| `STS2SkinChanger/Core/GitHubSkinService.cs:627` | TryDeleteFile | File.Delete | 日志 | ⚠ 无 |
| `STS2SkinChanger/Core/MlmLaunchPlanSync.cs:166` | JsonArray | File.WriteAllText | 其他/未分类 | ⚠ 无 |
| `STS2SkinChanger/Core/ModThemePresets.cs:119` | Save | Directory.CreateDirectory | 目录创建（不写数据） | ⚠ 无 |
| `STS2SkinChanger/Core/ModThemePresets.cs:123` | Save | File.WriteAllText | 配置 | ⚠ 无 |
| `STS2SkinChanger/Core/ModThemePresets.cs:124` | Save | File.Move | 其他/未分类 | ⚠ 无 |
| `STS2SkinChanger/Core/ModThemePresets.cs:126` | Save | File.Delete | 其他/未分类 | ⚠ 无 |
| `STS2SkinChanger/Core/ModThemePresets.cs:128` | Save | File.Copy | 其他/未分类 | ⚠ 无 |
| `STS2SkinChanger/Core/ModThemeSettings.cs:143` | Save | Directory.CreateDirectory | 目录创建（不写数据） | ⚠ 无 |
| `STS2SkinChanger/Core/ModThemeSettings.cs:147` | Save | File.WriteAllText | 配置 | ⚠ 无 |
| `STS2SkinChanger/Core/ModThemeSettings.cs:148` | Save | File.Move | 配置 | ⚠ 无 |
| `STS2SkinChanger/Core/ModThemeSettings.cs:150` | Save | File.Delete | 配置 | ⚠ 无 |
| `STS2SkinChanger/Core/ModThemeSettings.cs:151` | Save | File.Copy | 配置 | ⚠ 无 |
| `STS2SkinChanger/Core/OnlineSkinCache.cs:142` | CancellationTokenSource | Directory.CreateDirectory | 目录创建（不写数据） | 文件内另有上限常量 |
| `STS2SkinChanger/Core/OnlineSkinCache.cs:790` | OperationCanceledException | Directory.CreateDirectory | 目录创建（不写数据） | 文件内另有上限常量 |
| `STS2SkinChanger/Core/OnlineSkinCache.cs:1228` | InvalidDataException | PckArchive.Write | 覆盖包 PCK | 文件内另有上限常量 |
| `STS2SkinChanger/Core/OverlayCacheStore.cs:58` | BeginSession | Directory.CreateDirectory | 目录创建（不写数据） | 文件内另有上限常量 |
| `STS2SkinChanger/Core/OverlayCacheStore.cs:59` | BeginSession | File.WriteAllText | 日志 | 文件内另有上限常量 |
| `STS2SkinChanger/Core/OverlayCacheStore.cs:104` | ReservePath | Directory.CreateDirectory | 目录创建（不写数据） | 文件内另有上限常量 |
| `STS2SkinChanger/Core/OverlayCacheStore.cs:165` | DeleteAll | File.Delete | 其他/未分类 | 文件内另有上限常量 |
| `STS2SkinChanger/Core/SkinChangerPaths.cs:33` | WriteCache | Storage.WriteCache | 缓存 | ⚠ 无 |
| `STS2SkinChanger/Core/SkinChangerStorage.cs:66` | IOException | Directory.CreateDirectory | 目录创建（不写数据） | 文件内另有上限常量 |
| `STS2SkinChanger/Core/SkinChangerStorage.cs:68` | IOException | File.Move | 配置 | 文件内另有上限常量 |
| `STS2SkinChanger/Core/SkinChangerStorage.cs:105` | IOException | Directory.CreateDirectory | 目录创建（不写数据） | 文件内另有上限常量 |
| `STS2SkinChanger/Core/SkinChangerStorage.cs:106` | IOException | new FileStream( | 缓存 | 文件内另有上限常量 |
| `STS2SkinChanger/Core/SkinChangerStorage.cs:116` | IOException | Directory.CreateDirectory | 目录创建（不写数据） | 文件内另有上限常量 |
| `STS2SkinChanger/Core/SkinChangerStorage.cs:220` | CleanSessionRoot | new FileStream( | 缓存 | 文件内另有上限常量 |
| `STS2SkinChanger/Core/SkinChangerStorage.cs:245` | CleanFile | File.Delete | 缓存 | 文件内另有上限常量 |
| `STS2SkinChanger/Core/SkinConfig.cs:291` | Save | Directory.CreateDirectory | 目录创建（不写数据） | ⚠ 无 |
| `STS2SkinChanger/Core/SkinConfig.cs:293` | Save | new FileStream( | 配置 | ⚠ 无 |
| `STS2SkinChanger/Core/SkinConfig.cs:300` | Save | File.Move | 其他/未分类 | ⚠ 无 |
| `STS2SkinChanger/Core/SkinConfig.cs:303` | Save | File.Copy | 日志 | ⚠ 无 |
| `STS2SkinChanger/Core/SkinService.CharacterSkinBundles.cs:528` | DeleteCharacterSkinBundleRunSnapshot | File.Delete | 日志 | ⚠ 无 |
| `STS2SkinChanger/Core/SkinService.cs:775` | RemoveOnlineSessionProvider | File.Delete | 覆盖包 PCK | 文件级: 8 GB |
| `STS2SkinChanger/Core/SkinService.cs:4196` | InvalidOperationException | SkinChangerPaths.WriteCache | 覆盖包 PCK | 文件级: 8 GB |
| `STS2SkinChanger/Core/SkinService.cs:6118` | GetOrPrepareRuntimeOverlay | SkinChangerPaths.WriteCache | 覆盖包 PCK | 文件级: 8 GB |
| `STS2SkinChanger/Core/SkinService.cs:7437` | MountArchiveOverlay | Directory.CreateDirectory | 目录创建（不写数据） | 文件级: 8 GB |
| `STS2SkinChanger/Core/SkinService.cs:7439` | MountArchiveOverlay | PckArchive.WriteFromArchives | 覆盖包 PCK | 文件级: 8 GB |
| `STS2SkinChanger/Core/SkinService.cs:7655` | TryDelete | File.Delete | 其他/未分类 | 文件级: 8 GB |
| `STS2SkinChanger/Core/SkinService.cs:8451` | EnsureGameSceneOnTop | SkinChangerPaths.WriteCache | 覆盖包 PCK | 文件级: 8 GB |
| `STS2SkinChanger/Core/SkinService.cs:8574` | LoadGameSceneViaAlias | SkinChangerPaths.WriteCache | 覆盖包 PCK | 文件级: 8 GB |
| `STS2SkinChanger/Core/SkinService.cs:8626` | InstantiateGameCreatureVisuals | SkinChangerPaths.WriteCache | 覆盖包 PCK | 文件级: 8 GB |
| `STS2SkinChanger/Core/SkinWorkshopService.Community.cs:51` | CancellationTokenSource | SkinChangerPaths.WriteCache | 缓存 | ⚠ 无 |
| `STS2SkinChanger/Core/SkinWorkshopService.cs:116` | CancellationTokenSource | SkinChangerPaths.WriteCache | 缓存 | ⚠ 无 |
| `STS2SkinChanger/Core/SkinWorkshopService.cs:165` | Cover | SkinChangerPaths.WriteCache | 缓存 | ⚠ 无 |
| `STS2SkinChanger/Core/SkinWorkshopService.cs:167` | Cover | File.WriteAllBytes | 缓存 | ⚠ 无 |
| `STS2SkinChanger/Core/StartupWatchdog.cs:150` | WriteReport | Directory.CreateDirectory | 目录创建（不写数据） | ⚠ 无 |
| `STS2SkinChanger/Core/StartupWatchdog.cs:152` | WriteReport | File.WriteAllText | 日志 | ⚠ 无 |
| `STS2SkinChanger/Core/WorkshopCommunityCatalog.cs:18` | InvalidDataException | Directory.CreateDirectory | 目录创建（不写数据） | ⚠ 无 |
| `STS2SkinChanger/Core/WorkshopCommunityCatalog.cs:20` | InvalidDataException | File.WriteAllText | 缓存 | ⚠ 无 |
| `STS2SkinChanger/Core/WorkshopCommunityCatalog.cs:21` | InvalidDataException | File.Move | 缓存 | ⚠ 无 |
| `STS2SkinChanger/Pck/PckArchive.cs:288` | WriteEntries | Directory.CreateDirectory | 目录创建（不写数据） | 文件内另有上限常量 |
| `STS2SkinChanger/Pck/PckArchive.cs:290` | WriteEntries | new BinaryWriter( | 其他/未分类 | 文件内另有上限常量 |

## 2. 无界循环 / 每帧触发（7 处无限循环 + 9 处每帧钩子）

### 2.1 无限循环

| 位置 | 所在方法 | 循环头 | 退出条件 | 样本 |
|---|---|---|---|---|
| `STS2SkinChanger/Core/BackgroundSkinService.cs:573` | ListLayersInDirectory | while (true) | 有 break/return |                     break; |
| `STS2SkinChanger/Core/BackgroundSkinService.cs:2127` | ListLayerPaths | while (true) | 有 break/return |                     break; |
| `STS2SkinChanger/Core/BackgroundSkinService.cs:3671` | ListGameBackgroundTitles | while (true) | 有 break/return |                     break; |
| `STS2SkinChanger/Core/SkinService.cs:1792` | WarmResourcePackFilesAsync | while (true) | 有 break/return |                     break; |
| `STS2SkinChanger/Core/SkinService.cs:9346` | ApplyMonsterCategoryPriorityToSelections | while (true) | 有 break/return |                     return entries.First |
| `STS2SkinChanger/Core/SkinService.EventPriorities.cs:154` | BuildEventPrioritySelectionUpdates | while (true) | 有 break/return |         } |
| `STS2SkinChanger/Core/StartupWatchdog.cs:91` | Watch | while (true) | 有 break/return |                 return; |

### 2.2 每帧钩子里会触发重建/挂载/写盘

| 位置 | 钩子 | 会调用 | 说明 |
|---|---|---|---|
| `STS2SkinChanger/Core/DebugSnapshot.cs:44` | ProcessFrame += OnFrame | 直接调用:  | 每帧钩子（未直接命中重建/挂载/写盘，供人工判断） |
| `STS2SkinChanger/Core/MultiplayerSkinSync.cs:2675` | _Process | Tick → RefreshMultiplayerPlayerIcons, RefreshPlayerAppearance, RefreshPlayerTransforms, RefreshSessionRuntimeProviders | 每帧钩子会触发重建/挂载/刷新/写盘 |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:2573` | _Process | ProcessPendingSelection → ApplySelectionNow | 每帧钩子会触发重建/挂载/刷新/写盘 |
| `STS2SkinChanger/Ui/ProviderToolbarCanvas.cs:42` | _Process | 直接调用: UpdateCanvas | 每帧钩子（未直接命中重建/挂载/写盘，供人工判断） |
| `STS2SkinChanger/Ui/SkinWorkshopPanel.Introduction.cs:37` | ProcessFrame += UpdateHover | 直接调用: Tick, UpdateHover | 每帧钩子（未直接命中重建/挂载/写盘，供人工判断） |
| `STS2SkinChanger/Ui/StatefulCardArtView.cs:263` | _Process | 直接调用: UpdateVisuals | 每帧钩子（未直接命中重建/挂载/写盘，供人工判断） |
| `STS2SkinChanger/Ui/StatefulCombatEffects.cs:47` | _Process | 直接调用:  | 每帧钩子（未直接命中重建/挂载/写盘，供人工判断） |
| `STS2SkinChanger/Ui/StatefulCombatEffects.cs:122` | _Process | 直接调用:  | 每帧钩子（未直接命中重建/挂载/写盘，供人工判断） |
| `STS2SkinChanger/Ui/StatefulCombatEffects.cs:165` | _Process | 直接调用:  | 每帧钩子（未直接命中重建/挂载/写盘，供人工判断） |

## 3. 重试（34 处逻辑块）

按「同一方法内所有 retry/attempt/重试」聚成一块；**28 块没有可识别的数值上限**（标 ⚠）。「上限（数字）」是从块内代码里抽出的常量，可能是帧数/次数/秒，也可能是无关数字，需对照源码确认。

| 位置 | 所在方法 | retry 提及 | 上限（数字） | 首行 |
|---|---|---|---|---|
| `STS2SkinChanger/Core/DeferredRegistrationQueue.cs:49` | TryRegister | 1 | ⚠ 未见上限 | public int RetryPending( |
| `STS2SkinChanger/Core/FrameworkCompatibilityLayer.cs:179` | SynchronizeSelections | 1 | ⚠ 未见上限 | var completed = ProviderRegistrations.RetryPending( |
| `STS2SkinChanger/Core/MultiplayerSkinSync.cs:144` | SessionCharacterSelection | 4 | ⚠ 未见上限 | private static double _appearanceRetryCooldown; |
| `STS2SkinChanger/Core/MultiplayerSkinSync.cs:821` | ResetRoundStateForLobby | 1 | ⚠ 未见上限 | _appearanceRetryCooldown = 0; |
| `STS2SkinChanger/Core/MultiplayerSkinSync.cs:928` | DetachFromRun | 1 | ⚠ 未见上限 | _appearanceRetryCooldown = 0; |
| `STS2SkinChanger/Core/MultiplayerSkinSync.cs:1074` | Tick | 2 | 3 | RetryAdvertisementsWaitingForPlayerIdentity(); |
| `STS2SkinChanger/Core/MultiplayerSkinSync.cs:1129` | (未知) | 3 | ⚠ 未见上限 | if (_appearanceRetryCooldown > 0 \|\| |
| `STS2SkinChanger/Core/MultiplayerSkinSync.cs:1519` | RequestFullRemoteRefresh | 1 | ⚠ 未见上限 | _appearanceRetryCooldown = 0; |
| `STS2SkinChanger/Core/MultiplayerSkinSync.cs:2303` | DictionaryEquals | 1 | ⚠ 未见上限 | private static void RetryAdvertisementsWaitingForPlayerIdentity() |
| `STS2SkinChanger/Core/MultiplayerSkinSync.cs:2373` | RememberLocalAdvertisement | 1 | ⚠ 未见上限 | internal static void RetryCachedSelection( |
| `STS2SkinChanger/Core/OnlineSkinCache.cs:748` | ProcessRequest | 1 | ⚠ 未见上限 | MultiplayerSkinSync.RetryCachedSelection(request, alreadyCached.OptionId); |
| `STS2SkinChanger/Core/OnlineSkinCache.cs:857` | CachedProvider | 1 | 3 | MultiplayerSkinSync.RetryCachedSelection(request, sessionOptionId); |
| `STS2SkinChanger/Core/SkinChangerStorage.cs:137` | CacheCleanupReport | 6 | ⚠ 未见上限 | void Attempt(Action clean) |
| `STS2SkinChanger/Core/StatefulCardArtRuntime.cs:20` | (未知) | 1 | ⚠ 未见上限 | private static readonly Dictionary<string, long> RetryAt = new(StringComparer.OrdinalIgnoreCase); |
| `STS2SkinChanger/Core/StatefulCardArtRuntime.cs:41` | For | 2 | ⚠ 未见上限 | if (RetryAt.GetValueOrDefault(option.ProviderId) > System.Environment.TickCount64) return null; |
| `STS2SkinChanger/Core/StatefulCardArtRuntime.cs:59` | Runtime | 1 | ⚠ 未见上限 | RetryAt[option.ProviderId] = System.Environment.TickCount64 + 2000; |
| `STS2SkinChanger/Core/WorkshopCatalogSession.cs:10` | (未知) | 1 | ⚠ 未见上限 | internal readonly HashSet<ulong> Attempted = []; |
| `STS2SkinChanger/Core/WorkshopCatalogSession.cs:34` | Load | 2 | ⚠ 未见上限 | var missing = ids.Distinct().Where(id => !state.Attempted.Contains(id)).ToArray(); |
| `STS2SkinChanger/Ui/AncientCompendium.cs:358` | ScheduleDeferredAttach | 2 | 12 | if (state.DeferredScheduled \|\| state.DeferredAttempts >= 12) |
| `STS2SkinChanger/Ui/AncientCompendium.cs:424` | EnumerateDescendants | 1 | ⚠ 未见上限 | public int DeferredAttempts { get; set; } |
| `STS2SkinChanger/Ui/BestiaryEntranceSwitcher.cs:26` | (未知) | 1 | ⚠ 未见上限 | private static int _scanAttempts; |
| `STS2SkinChanger/Ui/BestiaryEntranceSwitcher.cs:114` | ScheduleScan | 2 | 16 | if (_scanScheduled \|\| _scanAttempts >= 16) |
| `STS2SkinChanger/Ui/EmperorCrabCompat/HalfHpCrabSkinPatch.cs:55` | (未知) | 3 | ⚠ 未见上限 | private static SceneTree _retryTree; |
| `STS2SkinChanger/Ui/EmperorCrabCompat/HalfHpCrabSkinPatch.cs:65` | Install | 1 | ⚠ 未见上限 | StartInstallRetry(); |
| `STS2SkinChanger/Ui/EmperorCrabCompat/HalfHpCrabSkinPatch.cs:224` | TryInstallHalfHpPrefix | 1 | ⚠ 未见上限 | StopInstallRetry(); |
| `STS2SkinChanger/Ui/EmperorCrabCompat/HalfHpCrabSkinPatch.cs:506` | GetRuntimeResourceMethod | 1 | ⚠ 未见上限 | private static void StartInstallRetry() |
| `STS2SkinChanger/Ui/EmperorCrabCompat/HalfHpCrabSkinPatch.cs:508` | StartInstallRetry | 11 | 18000 | if (_halfHpPrefixInstalled \|\| _retryTree != null) |
| `STS2SkinChanger/Ui/EmperorCrabCompat/HalfHpCrabSkinPatch.cs:556` | StopInstallRetry | 4 | ⚠ 未见上限 | if (_retryTree != null && _retryHandler != null) |
| `STS2SkinChanger/Ui/MerchantRuntimeAppearance.cs:861` | CreatePreviewPlayer | 1 | ⚠ 未见上限 | private static int _watchedRoomAttempts; |
| `STS2SkinChanger/Ui/MerchantRuntimeAppearance.cs:912` | TickRefreshAfterLoad | 5 | ⚠ 未见上限 | _watchedRoomAttempts = 0; |
| `STS2SkinChanger/Ui/OstyModelPreviewControls.cs:640` | RefreshModel | 1 | ⚠ 未见上限 | _fitAttempts = 0; |
| `STS2SkinChanger/Ui/OstyModelPreviewControls.cs:689` | (未知) | 2 | ⚠ 未见上限 | _fitAttempts = 0; |
| `STS2SkinChanger/Ui/OstyModelPreviewControls.cs:700` | ScheduleFit | 4 | 30 | _fitAttempts++; |
| `STS2SkinChanger/Ui/OstyModelPreviewControls.cs:868` | TryShowScene | 1 | ⚠ 未见上限 | _fitAttempts = 0; |