# 白猫 1.0.41.16 核查与 Astra 复核修复交付记录（`#353` + `#354` + `#359`）

- **工作树**：`D:\mod\工作2\上游修复_试合并0541\STS2-SkinChanger_修改件`（detached 在白猫 `05edf34`，叠加我方全部改动，未提交、未部署）
- **需求来源**：房间 `STS2皮肤-1114` 消息 `#353`、`#354`（依据 `#352` 只读核查清单 `D:\mod\工作2\docs\handoff\白猫1.0.41核查_猫猫_20261124.md` 经云拍板的第一批 5 项修复 + 7a 纯文本事件项处理）与 `#359`（依据 `#358` Astra 完整复核 `D:\mod\工作2\docs\handoff\Astra完整复核_20261124.md` 的 4 项修复 + 2 项顺带补齐）
- **明确不改项**：假商人缓存键、`CharacterSelectUiScope` 时序、包名特判、跨版本反射（归白猫回函）以及全局写入 IL 静态门禁（记为待办）。

---

## 1. 事件图鉴普通事件不再请求不存在的背景场景（7b）

### 改动位置

- `STS2SkinChanger/Ui/EventCompendiumPreview.cs`：
  - 新增 `ResolveBackgroundScenePath(EventModel)`（反射缓存读取 `EventModel.BackgroundScenePath`，回退 `res://scenes/events/background_scenes/<id>.tscn`）与 `ShouldRequestBackgroundScene(EventModel, string, Func<string, bool>?)`；
  - 在 `EventCompendiumPreview.Create` 的 `host.Ready` 回调中，调用 `model.CreateBackgroundScene()` 前先判定 `ShouldRequestBackgroundScene(model, groupId)`（仅当所选皮肤/原版存在该事件资源 `HasEventResource` 或 `ResourceLoader.Exists(path)` 为真时才调用，不再对无背景场景的普通事件盲调触发 Godot `Cannot open file`）。BaseLib 自定义事件的 `CustomBackgroundScenePath` 路径保持不变。
- `STS2SkinChanger/Ui/ContextualSkinControls.cs`：
  - 新增 `ShouldReplaceCachedScene(string groupId, string resourcePath, PackedScene? currentResult)` 并接入 `ReplaceCachedScene`；
  - 遇到 `result == null`，或事件分组（`EventSkinPolicy.IsEventGroup(groupId)`）在当前选择下 `HasEventResource` 为 `false` 时直接返回、不去接管；先古分组（非 `event:*`）在原版返回非空 `PackedScene` 时照常接管替换。

### 对应测试

- `PostReviewFixesTests.VerifyEventBackgroundSceneDoubleGuardAndNineAncients`（`--test-postreview-fixes` 及无参全量 `RuntimeTests`）：验证普通事件 `PUNCH_OFF`（原版与 `NSFW-Event` 两种选择）不再请求 `background_scenes/punch_off.tscn`、`ReplaceCachedScene` 遇 `null` 或无 `HasEventResource` 不调 `GetOrLoadRuntimeScene` 且 0 报错；验证全部 9 个先古（`darv` / `neow` / `nonupeipe` / `orobas` / `pael` / `tanx` / `tezcatara` / `vakuu` / `the_architect`）的 `background_scenes/<id>.tscn` 照常可替换。

### 给云的实机验证方法

1. 打开「其它图鉴 → 事件」，连续翻看 10 个以上普通事件（含重拳出击等），并在原版与事件皮肤之间切换。
2. 检查 `godot.log`：应不再出现 `Cannot open file` + `[ERROR] 最终接管场景 res://scenes/events/background_scenes/<事件>.tscn 失败：独立皮肤资源不是场景`。
3. 切到「其它图鉴 → 先古」，逐个翻看先古并切换先古皮肤，确认背景与替换效果与之前完全一致。

---

## 2. `SkinService.ApplySelection` 使用切换前选择快照（4.2⑤）

### 改动位置

- `STS2SkinChanger/Core/SkinService.cs`：
  - 修复 `ApplySelection` 在先写入 `Config.Selections[update.Key] = update.Value` 后又读 `Config.GetSelection(update.Key)` 导致把新值当成旧值的缺陷；
  - 新增 `FindReplacedMultiGroupFullRuntimeProvider` 与 `CollectSelectionTransitionOptions`，均改用早已保存的 `previousSelections` 快照：
    1. 多分组完整运行提供者被替换时能正确触发 `RequestFullOverlayReassert`；
    2. `ProviderRuntimeCacheReset.ResetForSelections` 同时收到切出旧选择与切入新选择两侧，正确清空旧包第三方 DLL 中的静态缓存（如 `_hurtSkelCache`）。

### 对应测试

- `PostReviewFixesTests.VerifyApplySelectionUsesPreviousSelectionsForReassertAndCacheReset`：构造包含多分组完整运行提供者 `MultiFullPack`（2 个视觉分组 + 动态加载含 `_hurtSkelCache` 的提供者程序集）的真实 `SkinCatalog`，走 `SkinService.ApplySelection("jaw_worm", "SoloSkin")`，断言 `_needsFullOverlayReassert` 变为 `true` 且旧包静态 `_hurtSkelCache` 被清空；同选项重复切换不触发重申。

### 给云的实机验证方法

1. 装有多分组完整运行包或带运行时缓存的半血爆衣皮肤包时，先选该包再切到另一皮肤或「游戏默认」。
2. 查 `godot.log`：切出带静态缓存的第三方 DLL 包时应看到 `[外观一致性] 已清空 <旧包> 的 N 个运行时资源缓存`，切回后模型纹理不残留旧包图集。

---

## 3. 补齐异常隔离与全程序集门禁覆盖

### 改动位置

- `STS2SkinChanger/Ui/CharacterSelectUiScope.cs`：`CharacterSelectUiClosePatch.Prefix`（`NCharacterSelectScreen.OnSubmenuClosed`）接入 `UiInjectionGuard.Run`。
- `STS2SkinChanger/Ui/ContextualSkinControls.cs`：
  - `ContinueRunPortraitPatch.Postfix`（`NContinueRunInfo.ShowInfo`）接入 `UiInjectionGuard.Run`，并在 `RefreshContinueRunPortrait` 补齐 `info`/`save?.Players` 空引用守卫；
  - `BestiaryRuntimeProviderScopePatch.Postfix`（`NBestiary.OnSubmenuClosed`）接入 `UiInjectionGuard.Run`。
- `STS2SkinChanger/Core/Features/Card/CardPortraitPerformanceView.cs`：`CardPortraitPerformanceClosePatch.Postfix`（`NCardLibrary.OnSubmenuClosed`）接入 `UiInjectionGuard.Run`（与同文件 `OpenPatch`/`GridPatch` 对齐）。
- `STS2SkinChanger/Core/Features/Merchant/FakeMerchantBattleFigure.cs`：
  - `FakeMerchantBattleFigureReadyPatch.Postfix`（热路径 `NCreature._Ready`）新增零分配快速路径 `HasPendingShopFigure` 检查，并以 `try/catch` + `CombatAppearanceBoundary.Warn` 隔离异常；
  - `FakeMerchantBattleFigureCapturePatch.Prefix`（`NFakeMerchant.FoulPotionThrown`）补齐 `try/catch` + `CombatAppearanceBoundary.Warn`。
- `STS2SkinChanger/Core/Features/Background/BackgroundSkinService.cs`：`BackgroundSkinRoomPatch.Postfix` 新增的 `CurrentEncounter` 与 `FakeMerchantBackgroundNudge.Attach` 包进 `try/catch` 告警，避免异常打断背景设置。
- `tests/STS2SkinChanger.RuntimeTests/UiInjectionGuardTests.cs`：
  - 移除 `STS2SkinChanger.Ui` 命名空间限制，改为扫描**全程序集**所有 `[HarmonyPatch]` 类；
  - `LifecycleTargets` 扩充 `OnSubmenuClosed`，`AuditedScreenPatches` 新增 `ContinueRunPortraitPatch`；
  - 所有非 UI 注入生命周期补丁（战斗边界、卡牌管线、Transpiler、多人作用域 Finalizer、商人场景桥、主菜单诊断调度等）均在 `ExcludedNonUiInjectionLifecyclePatches` 逐项写明排除原因；预期计数更新为 **28 类 / 34 钩子**。

### 对应测试

- `UiInjectionGuardTests`（`--test-ui-injection-guard`）：全程序集 28 类 / 34 钩子门禁、白名单无陈旧项、异常隔离与 Postfix 链保活。
- `PostReviewFixesTests.VerifyNewlyGuardedHooksIsolateExceptions`：向 `CharacterSelectUiClosePatch`、`ContinueRunPortraitPatch`、`FakeMerchantBattleFigureCapturePatch`、`FakeMerchantBattleFigureReadyPatch`、`BackgroundSkinRoomPatch` 注入故障，验证异常被捕获记录、不向外抛出。

### 给云的实机验证方法

- 正常反复进出选人页、关闭怪物图鉴/卡牌库、在主菜单悬停「继续游戏」提示框、触发假商人扔药进战，均无异常日志，且原有头像/退场/战斗骨骼行为不受影响。

---

## 4. 延迟调用接入 `DeferredDispatch` + 机制 1 第一步三项收尾

### 改动位置

- `STS2SkinChanger/Core/DeferredDispatch.cs`：
  - 新增 `DeferredDispatchRisk.HighRiskEnterOnly`：执行前立即把 `ENTER` 落盘，正常完成时仅保留在内存环中**不写盘**（托管异常 `FAIL` 仍落盘），减少高频重试 Spine 任务的磁盘刷盘与日志噪声；
  - 新增 `WrapPollCallback` / `RunTrackedPoll` / `GetIdlePollCount`：周期性空轮询（回调返回 `false`）只累加计数，不逐条追加环形缓冲与面包屑，待真正执行有效工作时在 `result` 附带 `ok(idle=N)` 汇总前置空轮询次数。
- 接入 `DeferredDispatch` 的 6 处调用点（均已补齐 6 项 `[延迟调度]` 注释，零执行时机改动）：
  1. `Core/Features/Merchant/FakeMerchantEventFrame.cs`：`Schedule`（`CallDeferred`，`Normal`）与 `FitAsync`（`RunTrackedPoll`，`Normal`）；
  2. `Core/Features/Merchant/MerchantRuntimeAppearance.cs`：`CompleteSelectedMerchantNodeReady` 的 `NextFrameThenSpineReady`（Critical `CallDeferred` + `RunWhenSpineReady` 回调 `RunTracked` `HighRisk`）；
  3. `Ui/CharacterSelectUiScope.cs`：`Reopen`（`CallDeferred`，`Normal`）；
  4. `Core/Features/Merchant/FakeMerchantBattleFigure.cs`：`RebuildFromShopPreviewAsync`（`RunTrackedPoll`，`Normal`）与 `Attach` 的 `RunWhenSpineReady`（`RunTracked`，`HighRiskEnterOnly`）；
  5. `Ui/ContextualSkinControls.cs`：`ApplyProviderVariantLabels`（小红帽修法，`CallDeferred`，`Normal`）。
- 机制 1 第一步实测后的三项收尾：
  1. `Ui/EmperorCrabCompat/HalfHpCrabSkinPatch.cs`：新增 `IsTargetModLoaded` 检查（查已加载类型/程序集与 `ModManager.Mods`），未安装 `erowatcher` 时直接跳过 `StartInstallRetry()`，不再白跑 5 分钟（300 次）轮询；启动后亦改走 `DeferredDispatch.RunTrackedPoll`；
  2. `Core/SkinPreloader.cs`、`Core/DeferredProviderActivator.cs` 与上述轮询点：改走 `WrapPollCallback` / `RunTrackedPoll`，空轮询只计次数、不刷面包屑；
  3. `Ui/OstyModelPreviewControls.cs`：`OstyModelPreview.FitModel` 改为 `DeferredDispatchRisk.HighRiskEnterOnly`，只把 `ENTER` 落盘。

### 对应测试

- `DeferredDispatchTests`（`--test-deferred-dispatch`）：
  - `VerifyHighRiskEnterOnlyFlushesEnterWithoutDone`（验证磁盘只落 `ENTER`，正常 `DONE` 不写盘，异常 `FAIL` 落盘）；
  - `VerifyPeriodicEmptyPollsOnlyCountWithoutBreadcrumbs`（40 次空轮询零面包屑、计数精确为 40，首次有效工作汇总 `ok(idle=40)`）；
  - `VerifyHalfHpCrabSkipsRetryWhenTargetModAbsent`（无目标 Mod 返回 `false`、有目标 Mod 返回 `true` 及 IL 门控校验）；
  - `VerifyPostReviewDeferredSitesUseWrapper`（IL 核验全部迁移点均走 `DeferredDispatch` 且 `OstyModelPreview.FitModel` 传 `HighRiskEnterOnly`）。

### 给云的实机验证方法

1. 未装 `erowatcher` 正常启动进主菜单、进选人页切骨妹再正常退出。
2. 查看 `godot.log`：应看到 `[EmperorCrabSkinFix] 未检测到目标 Mod（erowatcher），跳过半血爆衣 5 分钟重试`。
3. 查看 `<SkinChanger存储目录>\diagnostics\<session>\deferred-breadcrumbs.log`：
   - 不应再被 `SkinPreloader.Tick` / `HalfHpCrabSkinPatch.InstallRetry` 的空轮询刷满；
   - `OstyModelPreview.FitModel` 正常完成时只出现 `[ENTER][HIGH]` 行，不刷 `[DONE]` 行。
4. 选小红帽（可先删 `%APPDATA%\SlayTheSpire2\skin_changer_variant_labels.txt` 复测），下拉变体词正常显示且无重入告警。

---

## 5. 假商人战斗背景微调改为 `STS2SC_DEBUG == "1"` 调试开关

### 改动位置

- `STS2SkinChanger/Core/Features/Background/FakeMerchantBackgroundNudge.cs`：新增 `IsDebugEnabled()`（与 `DebugSnapshot.EnsureInstalled` 一致，严格比对环境变量 `STS2SC_DEBUG == "1"`），`Attach` 首行门控；默认构建下不挂节点、不监听 `_Input`、不吞 `U/O/I/K/J/L` 键。

### 对应测试

- `PostReviewFixesTests.VerifyFakeMerchantBackgroundNudgeRequiresDebugEnv`：验证未设/设 `"0"`/`"true"` 时 `IsDebugEnabled()` 为 `false` 且 `Attach` 提前返回不触碰背景节点，仅 `STS2SC_DEBUG=1` 时为 `true`，并核对 IL 中 `IsDebugEnabled` 早于 `AddChild`。

### 给云的实机验证方法

1. 默认不设置 `STS2SC_DEBUG` 进入假商人战斗，按 `U / O / I / J / K / L`，背景位置与缩放不应变动，`godot.log` 无「假商人战斗背景微调」日志，其它 Mod 热键不被拦截。
2. 若需要调试微调，带 `STS2SC_DEBUG=1` 启动游戏后该热键仍可照常启用。

---

---

## 6. 纯文本事件皮肤标注「仅文本」、跳过原版图重打包与跟随分类语义（7a / `#354`）

### 改动位置

- **判定（按包内实际资源，零包名硬编码）**：
  - `STS2SkinChanger/Catalog/SkinCatalog.cs`：`SkinOption` 新增 `IsTextOnly` 属性；`GetSelectionProviderIds` 对 `option.IsTextOnly` 返回空列表，防止仅文本事件把包记入 `Config.VisualProviderPriority`。
  - `STS2SkinChanger/Catalog/SkinCatalog.Events.cs`：`AddEventSkinGroups` 按该事件 `assets.Count == 0`（无图/场景/特效/动画/音频资源，仅本地化文本改动）设置 `IsTextOnly`；新增 `IsEventOptionTextOnly`、`HasVisualEventOption`、`IsEventSelectionVisuallyActive` 供运行时与事务判定复用。
- **显示（15 语本地化入口）**：
  - `STS2SkinChanger/Core/ModLocalization.cs` 与 `STS2SkinChanger/Core/ModLocalization.Events.cs`：新增 `ModText.EventTextOnlySuffix` 与覆盖全部 15 种支持语言的 `EventTextOnlySuffixTexts`（中文 `zhs` 为 `（仅文本）`，英文 `eng` 为 ` (text only)`），新增 `ModLocalization.FormatEventOptionLabel(string name, bool isTextOnly)`。
  - `STS2SkinChanger/Core/Features/Ancient/AncientCompendium.cs`（`PopulateSkinDropdown`）：下拉选项统一走 `ModLocalization.FormatEventOptionLabel(option.Name, option.IsTextOnly)`。
  - `STS2SkinChanger/Core/SkinService.EventPriorities.cs` 与 `STS2SkinChanger/Ui/AncientCompendium.EventPriorities.cs`：`EventPriorityOptionState` 增加 `IsTextOnly`（仅当某包在该地区覆盖的事件全部为仅文本时置 `true`，带图混包不加后缀），地区优先级列表同步走 `FormatEventOptionLabel`。
- **不再为仅文本项白打包/隔离加载原版图**：
  - `STS2SkinChanger/Core/Features/Event/EventSkinRuntime.cs`：新增 `HasActiveVisualSelection(groupId)` 与 `ShouldUseIsolatedVisuals(groupId)`（当前解析选择为仅文本，或该事件所有候选皆为仅文本时返回 `false`）；接入 `TryLoadInitialPortrait`、`ResolvePortraitArgument`、`RefreshCurrent`、`RefreshVfx` 与 `EventSkinVfxPatch.Prefix`，直接复用游戏原图与原特效，不再打包 5.4 MB 原版事件图。
  - `STS2SkinChanger/Ui/EventCompendiumPreview.cs`：初始立绘回退路径仅在 `ShouldUseIsolatedVisuals(groupId)` 为真时调用 `LoadTexture`，否则直接读 `PreloadManager.Cache.GetTexture2D(path)`。
  - `STS2SkinChanger/Ui/ContextualSkinControls.cs`：`ReplaceCachedTexture` 与 `ShouldReplaceCachedScene` 对事件分组均先查 `EventSkinRuntime.ShouldUseIsolatedVisuals(groupId)`，为 `false` 时不走 `GetOrLoadRuntimeResource` / `GetOrLoadRuntimeScene`。
- **跟随分类与视觉生效边界**：
  - `STS2SkinChanger/Core/SkinService.EventPriorities.cs`（`ChangeEventPriorityConfiguration`）与 `STS2SkinChanger/Core/SkinService.cs`（`ApplySelection`）：跟随分类解析到仅文本包时，实际选择与直接选中完全一致（`HasActiveVisualSelection == false`、`ShouldUseIsolatedVisuals == false`、不写入 `VisualProviderPriority`、不生成私有包不重打包原版图，文本照常生效）；纯文本间切换（前后均无视觉生效项）跳过无意义的 `MountOverlay`，从带图包切到仅文本包则照常重挂覆盖还原基线。

### 对应测试

- `PostReviewFixesTests.VerifyTextOnlyEventOptionsDisplayNoRepackAndFollowCategory`（`--test-postreview-fixes` 及无参全量 `RuntimeTests`）：
  - 构造同包混装夹具（`MixedPack` 对 `event:alpha` 有图+文、对 `event:punch_off` 仅文本）、全包纯文本夹具（`PureTextPack`）与带图夹具（`VisualPunchPack`），验证按包内实际资源逐事件判定 `IsTextOnly`；
  - 验证中文 `（仅文本）`、英文 ` (text only)` 与全部 15 种语言翻译及 `AncientCompendiumScreen` UI 接线；
  - 验证直接选中与「跟随分类」解析命中仅文本包时均零 `GetOrPrepareRuntimeOverlay`、零 `LoadTexture`、零 `ReplaceCachedTexture` 接管、`HasActiveVisualSelection == false`、不进 `VisualProviderPriority`，且中/英文事件正文替换均生效；
  - 验证带图包切回跟随分类（解析到仅文本包）时照常还原基线、清出视觉生效状态且不再为仅文本包打包原版图。

### 给云的实机验证方法

1. 打开「其它图鉴 → 事件」，切到「重拳出击」（`event:punch_off`）：
   - 中文界面下拉应显示 `NSFW-Event（仅文本）`；切英文界面后应显示 `NSFW-Event (text only)`；
   - 同包的带图事件（如 `this_or_that`）下拉不带「仅文本」后缀。
2. 在「重拳出击」上反复切换「游戏默认」「跟随分类」「NSFW-Event（仅文本）」：
   - 切到 `NSFW-Event（仅文本）` 或「跟随分类」时，右侧剧情正文与选项文案正常切换，左侧插画保持游戏原图；
   - 查看 `godot.log` 与临时缓存目录：不再为 `punch_off.png` 生成新的 5.4 MB 私有 `.pck` 隔离加载记录。

---

## 7. Astra 完整复核 4 项修复 + 2 项顺带补齐（`#359`）

### 7.1 同名不同逻辑 ID 预激活不再绕过 `0.80` 类型重合度门槛（阻塞项）

- **改动位置**：
  - `STS2SkinChanger/ProtectedCharacters/ProviderSettingsVariantOptions.cs`：`FindRicherSiblingAssemblyPath` 增加可选筛选委托 `Func<string, bool>? isEligiblePath = null`，允许调用方限制候选路径边界。
  - `STS2SkinChanger/Core/ManagedSkinModLoader.cs`：`ActivateSelectedProviders` 改由 `IsSameLogicalProviderAssemblyPath` 将 `FindRicherSiblingAssemblyPath` 预激活严格收窄到**同一逻辑 ID 的重导出副本**（`NormalizeProviderIdentity(richerId) == NormalizeProviderIdentity(providerId)`）；跨逻辑 ID 的执行者选择一律从玩家当前选中的 `providerId` 进入中心 `GetOrLoadProviderAssembly`，统一受自写类型集合重合度 `>= 0.80` 与“更精简 → 更完整”单向门禁约束。
- **对应测试**：
  - `TwinAssemblyBehaviorDelegationTests.VerifySameSettingsContractLowOverlapSelectSmallerDoesNotRunUnselectedLarger`：构造两份不同逻辑 ID、同名程序集、相同 `SettingsVariantContract` 但类型重合度 `< 0.80` 的包 A（小）与 B（大），先单选 A 激活，断言 `InitCounts[B] == 0`、`ACTIVE_A == True`、`ACTIVE_B == False`、运行 A 自身行为；同用例同时验证同逻辑 ID 重导出副本仍可经预激活复用更完整副本，且既有精简包先选 / 完整包先选路径保持不变。

### 7.2 委托姊妹包的选角重放、房间热切换过滤与地区环境授权按执行者解析

- **改动位置**：
  - `STS2SkinChanger/Core/ManagedSkinModLoader.cs`：
    - `ReplaySelectedCharacterPresentation`：先经 `ResolveBehaviorProviderId(providerId)` 解析再查 `ActiveProviderRuntimes`；
    - `ReplaySelectedRoomReadyBehaviors`：将非空 `onlyProviderId` 经 `ResolveBehaviorProviderId` 解析后再匹配活跃运行时；
    - 新增 `IsRunEnvironmentAuthorizedForBehavior` 与 `SyncRunEnvironmentAuthorizationForActiveRuntimes`；`ConfigureRunEnvironmentProviders`、`ActivateProvider`（`runEnvironmentEnabled`）、`RunEnvironmentCallbackPrefix` 以及 `BehaviorProviderAliases.TryAdd` 建立新委托的两处（`GetOrLoadProviderAssembly`、`TryReuseSiblingBehavior`）均改按实时 `ResolveBehaviorProviderId` 解析授权，确保先调用 `ConfigureRunEnvironmentProviders([A])` 再建立 `A -> B` 委托时执行者 B 即时获得地区环境授权，且切回原版清空授权后立即撤销。
- **对应测试**：
  - `TwinAssemblyBehaviorDelegationTests.VerifyDelegatedCallbacksAndRunEnvironmentAuthorization`：覆盖 A 选中、B 执行的全链路——先以 A 配地区授权再激活 A（校验 B `RunEnvironmentEnabled == true` 与 `RunEnvironmentCallbackPrefix` 放行，清空后撤销、重授权恢复），以及按 A 调用 `ReplaySelectedCharacterPresentation`（含 `afterReplay`）和 `ReplaySelectedRoomReadyBehaviors(room, onlyProviderId: A)` 均命中执行者 B，切回原版停用 B 后均停止生效。

### 7.3 假商人战斗分组（`fake_merchant_monster`）合并帐篷资源后清除 `IsTextOnly`

- **改动位置**：
  - `STS2SkinChanger/Catalog/SkinCatalog.Events.cs`：`AddFakeMerchantBackgroundProviders` 向已存在选项合并 `groupAssets` 时，同步更新 `IsTextOnly = merged.Count == 0 && !old.IsRuntimeProvider`，避免带帐篷/背景图的假商人事件包在 `fake_merchant_monster` 上被误标为仅文本。
- **对应测试**：
  - `PostReviewFixesTests.VerifyFakeMerchantBattleGroupClearsTextOnlyWhenTentAssetsMerged`：单包同时含 `FAKE_MERCHANT.*` + `PUNCH_OFF.*` 文本与 2 张假商人帐篷/背景图，断言 `fake_merchant_monster`（`Assets=2, IsTextOnly=false, GetSelectionProviderIds=[pack]`）、`event:fake_merchant_sale_background`（`Assets=2, IsTextOnly=false, GetSelectionProviderIds=[pack]`）与 `event:punch_off`（`Assets=0, IsTextOnly=true, GetSelectionProviderIds=[]`）三者均准确。

### 7.4 事件房间内跨纯文本选项反复切换不再叠加原生 VFX

- **改动位置**：
  - `STS2SkinChanger/Core/Features/Event/EventSkinRuntime.cs`：
    - `EventSkinVfxPatch.Postfix` 为 `EventModel.CreateVfx` 返回的原生 VFX 同步打上 `VfxSourceMeta` 与 `VfxOriginMeta = "native"` 标记，隔离加载的 VFX 标记为 `"isolated:<selection>"`；
    - `RefreshVfx` 改为通过 `ReconcileTrackedVfxNodes` 对带标记的 VFX 节点按来源对账：当前来源未变（如原生/纯文本/跟随分类间切换，或同带图包重复刷新）时原位保留既有节点、零重复创建；来源变化时再清理旧追踪节点并替换为目标 VFX；全程不触碰作者自挂的无标记立绘子节点。
- **对应测试**：
  - `PostReviewFixesTests.VerifyEventRoomVfxDoesNotDuplicateAcrossTextOnlySwitches`：在同一事件房间内连续在游戏默认、纯文本包、跟随分类（命中纯文本）、带图视觉包之间来回切换，断言追踪 VFX 节点总数始终严格等于 `1`、原生间切零重建，且作者挂在立绘上的无标记装饰子节点始终完好保留。

### 7.5 顺带补齐：`FrameworkRegistryCooperation.TrackControl` 与 4 个内部延迟回调测试断言

- **改动位置**：
  - `STS2SkinChanger/Core/FrameworkRegistryCooperation.cs`：`TrackControl` 接入 `UiInjectionGuard.Run("FrameworkRegistryCooperation.TrackControl", ...)`，为程序式 `SkinSelector._Ready` 补丁提供一致的异常隔离与计数告警；
  - `tests/STS2SkinChanger.RuntimeTests/UiInjectionGuardTests.cs`：新增 `VerifyProgrammaticUiHooksUseGuard`，断言 28 个 `[HarmonyPatch]` 类（34 钩子）+ 1 处程序式 UI 钩子全部受守卫保护；
  - `tests/STS2SkinChanger.RuntimeTests/DeferredDispatchTests.cs`：新增 `AssertInnerCallbackCallsDeferredDispatch`，扫描编译器生成状态机/闭包，精确断言 `FakeMerchantEventFrame.FitStep`、`FakeMerchantBattleFigure.ProbeStep`、`FakeMerchantBattleFigure.AttachSpineReady`（`HighRiskEnterOnly`）与 `MerchantAppearance.SpineNodeReady`（`HighRisk`）4 个内部回调均调用 `DeferredDispatch`。

### 给云的实机验证方法（`#359` 新增重点）

1. **假商人事件与战斗**：若装有带假商人帐篷/背景图的事件包，检查怪物图鉴「假商人（`fake_merchant_monster`）」与假商人卖场背景下拉，不应出现「（仅文本）」后缀，且切换与进战换图正常；同包中无图事件（如重拳出击）仍带「（仅文本）」。
2. **事件房间内反复切换纯文本选项**：在局内带粒子/特效的事件房（如带 `CreateVfx` 特效的事件）连续在「游戏默认」「纯文本事件皮肤」「跟随分类」之间切换多次，画面粒子密度不应叠加变浓、不卡顿。
3. **同名姊妹包（如贞德两套不同档位包）**：先选精简版贞德再进选人页、进图热切、切回原版，确认选角立绘重放、房间热切换与地区环境行为均跟随当前选择正常生效与撤销。

---

## 8. 门禁与验证结果

所有构建与测试产出均隔离输出至 `%TEMP%\sts2-gates-gemini-postreview`（未覆盖工作区 `bin/Release`），`tools/run-gates.ps1` 与 `tools/check-protected-characters.ps1` 留交 Claude 统一验收，未执行 `git commit`、未部署到游戏目录、未启动游戏。

| 分支 | 配置 / 游戏参考目录 | 四工程构建（compat / main / Logic / Runtime） | `LogicTests` | 无参全量 `RuntimeTests` | 新增/扩充专项单项 | 非外部 `SEPARATE`（8 项） |
| --- | --- | --- | --- | --- | --- | --- |
| **Stable** | `Release` / 正式版 `v0.111.1` | 全部通过，**0 warnings / 0 errors** | PASS | PASS（`total=43, PASS=38, FAIL=0, NOT RUN=0, SEPARATE=5, GAP=0`，末行 `Workshop policy tests passed.`） | `--test-ui-injection-guard`（28 类 / 34 钩子 + 1 程序式钩子）PASS；`--test-deferred-dispatch` PASS；`--test-postreview-fixes` PASS | 8/8 PASS |
| **Beta** | `ReleaseBeta` / 测试版 `v0.113.1` | 全部通过，**0 warnings / 0 errors** | PASS | PASS（`total=43, PASS=38, FAIL=0, NOT RUN=0, SEPARATE=5, GAP=0`，末行 `Workshop policy tests passed.`） | `--test-ui-injection-guard`（28 类 / 34 钩子 + 1 程序式钩子）PASS；`--test-deferred-dispatch` PASS；`--test-postreview-fixes` PASS | 8/8 PASS |

- **8 项非外部 `SEPARATE` 明细**（Stable / Beta 均全绿）：`--test-pack-selection-cache`、`--test-relic-bundle-residency`、`--test-runtime-bundle-residency`、`--test-card-cache-reuse`、`--test-merchant-lifecycle`、`--test-mode-background`、`--test-protected-boundaries`、`--test-global-write-fallback`。
- **行尾与编码核验**：`#353` + `#354` + `#359` 合计新增/修改的 34 个文件（26 个生产源码、6 个测试与清单文件、2 个文档）全部检查为纯 LF、无 BOM（`crlf=0, bom=False`）。
- **未跟踪新文件提醒（交 Claude 打包/暂存确认）**：`tests/STS2SkinChanger.RuntimeTests/PostReviewFixesTests.cs`、`docs/refactor/postreview-fixes-gemini.md`。
- **已改文件总表（34 项）**：
  - 生产代码（26）：`STS2SkinChanger/Catalog/SkinCatalog.cs`、`STS2SkinChanger/Catalog/SkinCatalog.Events.cs`、`STS2SkinChanger/Core/FrameworkRegistryCooperation.cs`、`STS2SkinChanger/Core/ManagedSkinModLoader.cs`、`STS2SkinChanger/Core/ModLocalization.cs`、`STS2SkinChanger/Core/ModLocalization.Events.cs`、`STS2SkinChanger/Core/SkinService.cs`、`STS2SkinChanger/Core/SkinService.EventPriorities.cs`、`STS2SkinChanger/Core/DeferredDispatch.cs`、`STS2SkinChanger/Core/SkinPreloader.cs`、`STS2SkinChanger/Core/DeferredProviderActivator.cs`、`STS2SkinChanger/Core/Features/Ancient/AncientCompendium.cs`、`STS2SkinChanger/Core/Features/Background/BackgroundSkinService.cs`、`STS2SkinChanger/Core/Features/Background/FakeMerchantBackgroundNudge.cs`、`STS2SkinChanger/Core/Features/Card/CardPortraitPerformanceView.cs`、`STS2SkinChanger/Core/Features/Event/EventSkinRuntime.cs`、`STS2SkinChanger/Core/Features/Merchant/FakeMerchantBattleFigure.cs`、`STS2SkinChanger/Core/Features/Merchant/FakeMerchantEventFrame.cs`、`STS2SkinChanger/Core/Features/Merchant/MerchantRuntimeAppearance.cs`、`STS2SkinChanger/ProtectedCharacters/ProviderSettingsVariantOptions.cs`、`STS2SkinChanger/Ui/AncientCompendium.EventPriorities.cs`、`STS2SkinChanger/Ui/CharacterSelectUiScope.cs`、`STS2SkinChanger/Ui/ContextualSkinControls.cs`、`STS2SkinChanger/Ui/EmperorCrabCompat/HalfHpCrabSkinPatch.cs`、`STS2SkinChanger/Ui/EventCompendiumPreview.cs`、`STS2SkinChanger/Ui/OstyModelPreviewControls.cs`。
  - 测试与清单（6）：`tests/STS2SkinChanger.RuntimeTests/UiInjectionGuardTests.cs`、`tests/STS2SkinChanger.RuntimeTests/DeferredDispatchTests.cs`、`tests/STS2SkinChanger.RuntimeTests/PostReviewFixesTests.cs`、`tests/STS2SkinChanger.RuntimeTests/TwinAssemblyBehaviorDelegationTests.cs`、`tests/STS2SkinChanger.RuntimeTests/Program.cs`、`tests/required-fixes.json`。
  - 文档与规范（2）：`docs/agents/maintenance-rules.md`、`docs/refactor/postreview-fixes-gemini.md`。
