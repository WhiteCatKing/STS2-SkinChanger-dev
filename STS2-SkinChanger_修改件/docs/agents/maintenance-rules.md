# 皮肤切换器 · 协作维护规矩

来源：维护者（yuinn 一方）《给 AI 的维护规矩》，2026-11-21 更新到 1.0.36.106；本文件为项目内正式副本。配套流程见 [maintenance-workflow.md](maintenance-workflow.md)（每一步怎么做、做到什么程度算完）。
接手或继续开发前，先读本文件，再读下列既有记录：

- [docs/refactor/fix-upstream-1.0.35-gpt.md](../refactor/fix-upstream-1.0.35-gpt.md)
- [docs/refactor/fix-upstream-1.0.35-dsh.md](../refactor/fix-upstream-1.0.35-dsh.md)
- [docs/refactor/fix-upstream-1.0.35-gemini.md](../refactor/fix-upstream-1.0.35-gemini.md)

## 开工前

- 只改和本次任务相关的文件；不批量改格式、不改换行（保持 LF）。

## 不许丢的修复（改到相关代码时必须保留，并保持对应测试通过）

- 多档角色已加载的运行包在独立纹理预算内保留。当前有持有者的包不可按预算或压力淘汰，来源变更时旧条目退出查找，已有视图自然归还。门禁 `runtime-bundle-residency`。
- 提权选包只对每个提供者解析一次包路径。文件状态最多保留两秒，选择或目录索引变动时刷新。共享随从包、完整运行包和体积门槛照原规则判断。门禁 `pack-selection-cache`。

- 卡图跨切换复用须验证私有依赖闭包、物理来源与导入参数。原版和不同包各自取图，包更新须失效。按底图字节做 LRU，外部视图放手后由游戏线程释放。门禁 `card-cache-reuse`。

- 完整视觉包挂载后保留路径归属（`MountSelectedLargeRuntimeProviderPacks` / `MountedOverlayOwners`）——否则 CZN 等大包每次切换重写上百 MB，几次就闪退。测试：`LargeProviderOverlayTests`。
- 隔离卡图按会话缓存并显式释放（`CardPortraitSessionCache`）——否则领卡牌奖励时在终结器线程崩溃。测试：`CardPortraitSessionTests`。
- Spine 贴图页预检与导入产物映射（`SkinCatalog.SpinePreflight`、`ImportedTexturePathPolicy`）——缺页的图集不许交给 Spine，否则原生崩溃（魅魔战士、铁甲恶魔）。测试：`SpineResourcePreflightTests`。
- 缓存总预算 16 GB 与写包前空间检查（`CacheWriteBudget`），拒绝时游戏内提示（`CacheWriteNotice`）。
- 同 ID 同名程序集复用（涅奥商人两份包）。测试：`ProviderAssemblyReuseTests`。
- 奥契丝修复包的商店 / 火堆回调按玩家选择限制（`OrchisSkinFixCompat`），**不允许整包禁用该修复包**。测试：`OrchisRoomCallbackTests`。
- `CharSelectTakeoverSuppressor`（禁止第三方改骨妹选人背景大图，防循环报错崩溃）必须保留，且支持晚载补装。测试：`LateTakeoverGuardTests`。
- 联机大厅补丁不绑定 `ChangeCharacter` 的第三个参数名（正式版 / 测试版名字不同）。

1.0.36.102 新增（2026-11-19）：

- Fool_Waifu 自恢复补丁的拦截（`FoolWaifuRuntimeCompat`）：未选中 Fool_Waifu 时不许它把切换器停掉的补丁装回来（含头像、选人按钮、商店 `AttachMerchant`）——否则其他愚者皮肤头像被占、商店 Hilde 缩小。测试：`FoolWaifuIdentityTests`。
- 角色专属先古对话随选择（`SkinCatalog.CharacterDialogue`、`HildeAncientDialogueCompat`）：整表都是 `*.talk.<角色>.*` 的 `ancients.json` 跟随该角色当前皮肤；Hilde 写入 `AncientDialogueSet` 只在选中时生成。设置 / UI 文本表仍全局。测试：`CharacterDialogueLocalizationTests`。
- 商人热切换：第三方 `_Ready` 前缀**进树后**才重放，只跑一次（`MerchantRuntimeAppearance`）——否则切换后商人点不了。测试：`--test-merchant-lifecycle`。
- 纯商人提供者不得修改玩家商店形象 `NMerchantCharacter`（`ManagedSkinModLoader` 剥离其补丁，保留 `NMerchantButton` / `NMerchantHand`）。测试：`MerchantPlayerCharacterIsolationTests`。
- 恐惧 / 极速选人：模式界面背景重做（`CharacterSelectModeBackgroundPatch`、`SetSelectedCharacterVisuals` 后置）、自绘叠加层的包在新背景入树后重挂（`ProviderModeSelectOverlayBridge`）、恐惧界面 `GrayScaler` / `BigRotatingContainer` 只在选中带点击切档的皮肤时放通点击。测试：`--test-mode-background`（`CharacterSelectModeBackgroundTests`）。
- AncientWaifus：遗物独立 PNG 切片 + `RelicModel.BigIcon`（`RelicSkinResources`）、时间线 `EpochModel.Portrait` / `RealPortrait`（`TimelineEpochSkinResources`）、CznStyleUI 图鉴兼容（`CznRelicCollectionCompat`）、触摸区由先古动画启动时挂载（`InteractiveTouchBoxBridge.AttachToScene`，**不要改回全局 `SceneTree.NodeAdded` 监听**，性能原因）。测试：`AncientWaifusSupportTests`。

1.0.36.103 / .104 新增（2026-11-20）：
- **战斗内建模型绝不抛出切换器自己的异常**（`MultiplayerSkinSync` 的 `CreateVisuals` / `NCreature._Ready` 补丁、`CombatAppearanceBoundary`）：余量不足等外观失败降级为原版模型，游戏原生异常原样交还；战斗中不清缓存、不强制 GC——否则联机进战斗时队友/怪物缺失、模型叠在一起、不同步。测试：`CreatureVisualLifecycleTests`。
- **卡面会话缓存必须能淘汰**（`CardPortraitSessionCache`，LRU + 只对真正失败的路径负缓存）——否则浏览大量卡牌后后续卡面全部回退原版。测试：`CardPortraitSessionTests`。
- 卡图匹配：借用其他已知卡池路径的 Mod 卡（如久留美借铁甲 Strike/Defend）不得跨卡池匹配；目录（`SkinCatalog`）与运行时（`SkinService`）**共用同一套**路径身份解析与匹配谓词，不要再各写一份。测试：`CardPoolOwnershipTests`（含真实目录构建到运行时命中的端到端用例）。
- 商人热切换只传递按钮的视觉状态，不传 `MouseFilter` / `FocusMode`（涅奥包会把原按钮设为 Ignore）。测试：`MerchantLifecycleTests`（连续 120 次切换）。
- 恐惧 / 极速选人：提供者选人叠加层按当前选中提供者**通用**挂接（单参数 `Attach(Control)` / `AttachBackground`、双参数 `CharSelectCore.Attach(bg, Profile)`），同帧只挂一次；界面隐藏时停用其点击轮询器（`SyncExclusiveScreenPollers`），**不要写死包名或角色**。背景节点名查找不区分大小写（`SILENT_bg`）。测试：`CharacterSelectModeBackgroundTests`。
- 奥契丝：`TextReplacePatch` 文本只在骨妹选中奥契丝时生效；模型 / 缩放 / Snap / 层级按奥斯提自身选择门控。奥斯提热切后按游戏召唤偏移归位；同时识别正式版 `OstyScaleToSize` 与测试版 `ScaleAndPositionOsty`。测试：`OrchisRoomCallbackTests`、`CreatureVisualLifecycleTests`。

1.0.36.105 新增（2026-11-21）：
- **原版规范路径不许被提供者全局改写**：卡图运行包只发布私有路径，导出的二进制卡图由 `CardBinaryResource` 改写依赖和偏移，不再写规范路径的 `.remap/.import`（已删 `CanonicalResourceOwnershipTracker` 那套桥接）；提供者 PCK 里和游戏本体共享图集、原版卡图导入产物同名的文件，在安全包、`BuildOverlay`、`BuildCardOverlay`、`BindRuntimeDependency` 都要剔除（`ProtectGlobalCanonicalPackFile` / `IsSharedGameAtlasPath`，名单从当前游戏 PCK 动态读，不写死哈希）。测试：`CardPoolOwnershipTests`、`FoolCardPackIsolationTests`。
- 带 `*.cardartpack.json` 的纯素材卡包也必须隔离原 PCK（`ProbeSkinProviders` 接入 `CardArtPackScanner`）。
- 原版卡缺默认图集时回退同卡 Beta 图（`ResolveGameCardPortraitFallback`）；先古卡说明底图 / 边框按实际 PCK 解析新旧目录，找不到就不画（否则游戏画 NOPE 占位）。
- 选角头像：完整提供者包挂载前过滤选角头像规范入口（`PrepareSelectPortraitSafePack`）；借用别的角色头像路径的内容角色（响木天音）用自身私有图（`HibikiSelectPortrait`）。**不要恢复 `CharSelectStageCycle.EnsureProviderPackMounted` 那种对原始 PCK 直接 `LoadResourcePack` 的写法**。测试：`CharacterSelectPortraitIsolationTests`、`JoanStageBindingTests`。
- 商人房间场景按实际 PCK 解析（正式版 `merchant_room`，测试版 `standard_` / `blitz_merchant_room`），不写死单一版本。测试：`MerchantRoomPathTests`。
- 独立愚者皮肤合同表（`IndependentFoolSkinContract`）新增 NonoFoolSkin；预览走 `IndependentFoolSkinCompat.ApplyPreview` 的按包分支。测试：`IndependentFoolSkinTests`、`NonoFoolPreviewTests`。
- 提供者补丁接管：`CardModel.PortraitPath` 一类卡图 getter 归切换器接管（日奈）；**只拦卡图路径，不拦 `CharacterModel.IconTexture` 等头像补丁**。测试：`HinaUiOstyRegressionTests`。
- UI 注入：Godot 控件**先入树、再调用依赖 `_Ready` 的方法**（`NLibraryStatTickbox.SetLabel` 在入树前调用会空引用，打断同一 `_Ready` 后置链上其他 Mod 的回调，例如 RitsuLib 同步图鉴头像）。

1.0.36.106 新增（2026-11-21）：
- **对局清理不得让别人手里的卡图失效**：`CardPortraitSessionCache.Clear()` 只放掉自己的引用；贴图确实不可达后才在游戏线程 `Dispose`（`TrackForGameThreadRelease` / `DrainRetiredTextures`）。**不要改回清理时直接 `Dispose` 所有发出去的贴图**，否则第三方跨局缓存（AI_silent）会拿到已释放对象，联机断线重进后手牌消失、出牌卡在左上角。防终结器线程崩溃的保护仍然有效。测试：`CardPortraitSessionTests`。
- **卡牌管线异常隔离**（`CardPipelineExceptionBoundary`）：`NCard._EnterTree / _Ready / UpdateVisuals / Reload` 上的 finalizer，抛出点在第三方 Mod 时让调用方继续，同一程序集 + 方法只记一条 `[卡牌管线]` 日志；游戏本体和切换器自己的异常原样抛出。测试：`CardPipelineExceptionTests`。
- **加载顺序风险提醒**（`LoadOrderRiskDetector`）：排在切换器前面的皮肤包，如果给卡面挂了全局补丁、或带和游戏本体同名的共享图集文件，每次启动都提醒；检测到 Load Order Manager 时只提示，不自动移动。测试：`LoadOrderRiskNoticeTests`。

1.0.37.4 / 1.0.38 新增（2026-11-22）：
- **正式版图鉴「切换 / 其他图鉴」入口保护**（`Core/VakuuVanillaArt.cs` 的 `VakuuVanillaArtPatch.Prepare()`）：测试版独有方法 `Vakuu.ConfigureBackgroundScene` 在正式版缺失时由 `Prepare()` 整类跳过，不能仅靠 `TargetMethods()` 空产出，否则 Harmony 抛异常导致整个 `ancient` 模块回滚、图鉴入口消失。测试：`EntryPatchIsolationTests`、`--test-vakuu-vanilla`（`VakuuVanillaArtTests`，需游戏 PCK）。
- **UI 注入点外层异常隔离**（`Ui/UiInjectionGuard.cs`）：切换器在游戏节点生命周期（`_Ready` / `_EnterTree` / `OnSubmenuOpened`）及界面刷新钩子里注入 UI 时统一经 `UiInjectionGuard.Run` / `RunWhenInTree` 隔离，同点位仅首次记录完整堆栈、后续仅累计次数，绝不向外抛截断同链其他 Mod 回调；`CardPipelineExceptionBoundary` 与 `CombatAppearanceBoundary` 维持各自规则，不套此护栏。测试：`UiInjectionGuardTests`。
- **提供者补丁四路径精准发现与归属守恒**（`Core/ProviderPatchIndex.cs`、`Core/ManagedSkinModLoader.cs`）：首次激活保留初始化前全量预扫与逐条归属比对（不能删预扫、不能只比总数），动态/懒加载提供者（含 `FoolWaifu` 激活与停用）及蓝图覆盖未确认完整路径保留全量发现；仅在命中完整蓝图的重复激活走「蓝图全观察目标 + 声明式 `[HarmonyPatch]` 目标 + 受管目标固定候选集」精准查询，且命中目标后仍逐条按归属筛选；泄漏校验仅核对本次实际卸载过的目标并在受管列表为空时跳过。测试：`ProviderPatchCaptureTests`（门禁 `provider-patch-capture`）。
- **延迟回调与轮询执行记录 + 崩溃面包屑**（`Core/DeferredDispatch.cs`、`Core/CrashDiagnostics.cs`、`Diagnostics/CrashWatch.ps1`）：新增 `CallDeferred` / `ProcessFrame` 重试 / `CreateTimer` 轮询调用点须走 `DeferredDispatch` 包装器并写明 6 项注释（原触发时机、最早可执行帧、顺序依赖、能否合并、重试上限、取消条件）；Critical 任务必须在原调用点单独入队，高风险（原生闪退先例）任务标记 `DeferredDispatchRisk.HighRisk` 以便执行前先落盘 `ENTER` 面包屑；连续重入告警只观察不截断，异常语义与原调用一致。门禁：`deferred-dispatch-records`（`DeferredDispatchTests`）。
- **同名程序集姊妹包行为委托**（`Core/ManagedSkinModLoader.cs`：`GetOrLoadProviderAssembly`、`TryFindRicherTwinSibling`、`TryReuseSiblingBehavior`、`GetAssemblyTypeOverlapRatio`、`DeactivateProvidersExcept`、`ActivateSelectedProviders`、`IsProviderAssemblyFor`）：不同逻辑 ID 但程序集简单名、culture、公钥令牌一致且已登记的姊妹包，须经只读 PE 元数据核验作者类型定义全名集合 Jaccard 重合度 `>= 0.80`（`TwinAssemblyTypeOverlapThreshold`，按双文件戳缓存）；所有程序集加载入口统一在 `GetOrLoadProviderAssembly` 决策，首次加载其中任意一份时优先加载作者类型数更全的那份，并**只允许精简版 → 更全版单向委托**（写入 `BehaviorProviderAliases`，资源仍取自所选包）；若更全版已被提前加载的精简版占坑，必须拒绝反向委托并打出明确警告。同逻辑 ID 原路径不加此阈值与方向限制；类型重合不足的模板克隆 Mod 拒绝委托并保留原冲突警告。停用须保留执行者；重选经蓝图重挂补丁且不重复初始化；`IsProviderAssemblyFor` 与重放入口须识别别名。测试：`TwinAssemblyBehaviorDelegationTests`、`ProviderAssemblyReuseTests`（门禁 `twin-assembly-behavior-delegation`）。

## 改完必须做

1. 正式版和测试版**两种引用都构建一次**，都要 0 错误（`AnyCPU`）：
   - 正式版：`dotnet build STS2SkinChanger/STS2SkinChanger.csproj -c Release -p:GameAssemblyDir=<正式版目录>`
   - 测试版：`dotnet build STS2SkinChanger/STS2SkinChanger.csproj -c ReleaseBeta -p:GameAssemblyDir=<测试版目录>`
   - 本机现行取值：正式版 `C:\sts2refs\stable`（v0.107.1 基线副本；**正式版已更新到 v0.111.1，建议换成 v0.111.1 的副本**，我们这边的验证用的是 v0.111.1）、测试版 `D:\steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64`（v0.113.1）。
2. 跑 LogicTests 和**完整无参数 RuntimeTests**，必须从头跑到尾（末行 `Workshop policy tests passed.`）。完整套件现在是全绿的，不允许让它重新变红；某条测试失败，先判断是「测试过时」还是「真 bug」，**不许为了变绿去削弱断言**。
3. 运行 RuntimeTests 前先调用 `SetErrorMode(3)`，免得失败时弹调试器窗口卡住。
4. 新增界面文字走 `ModLocalization` / `ModText`，补齐 15 种语言。
5. 动到受保护角色（十字军、贞德、选人背景）或共享加载入口时，加跑 `tools/check-protected-characters.ps1`（真实包回归）。
6. 改了运行时取数据的路径，要有"用真实目录构建 → 运行时命中"的端到端用例（11-20 卡面缓存就是离线全绿、实机全挂）。
7. 画面、点击、加载顺序相关的改动，交付时附实机测试步骤（含预期结果），由维护者在游戏里确认后再提交。

## 碰到这些要停下来问维护者，不要自己决定

- 产品行为的取舍（例如未选中的皮肤要不要保留文字、上限给多少、要不要禁用某个第三方包）。
- 需要删除玩家文件、改注册表、改其他 Mod 文件的改动。
- 原因没查清的问题：先写结论和证据，不要先改。

## 交接格式（每次干完写进 docs/refactor/ 下的记录）

- 改了什么（文件和函数）、为什么改（根因）、怎么验证的（构建、哪些测试组、实机步骤）、还剩什么没做或没验证。
- 无法离线验证的画面问题，写明「需实机确认」和具体步骤。

## 排查第三方 Mod 时

- 先看日志和崩溃诊断，再动代码。
- 常见的三类坑（2026-11-19 实例）：① 包会自己把补丁装回来（Fool_Waifu），停用后要确认它没有自恢复；② 资源放在包的私有路径、由 DLL 重定向（AncientWaifus、Fool_Waifu 的头像），资源隔离拦不住，要按入口处理；③ 包只在标准选人界面挂自己的叠加层，恐惧 / 极速是另外的界面。
- 包更新后功能失效：先对比新旧两版 DLL（旧版通常还在工坊目录的备份或用户手里），再看切换器哪条路径没接上。
- 皮肤包作者让玩家「把它排在切换器上面」的说法不要采纳：切换器必须排第一。
- 加载顺序：游戏按设置里同 ID **最后一行**的位置排序，忽略 `loadPriority`；同 ID 本地 + 工坊并存会让切换器被挤到后面。排查「皮肤大面积失效 + 主界面报重复 ID」时先查这个。
- "选了游戏默认还显示别的包的图"：先查有没有包（不论选没选中）带着和游戏本体**同名**的规范路径或 `.godot/imported` 产物被全局挂载（日志 `Loading Godot PCK …`），再查切换器自己有没有把规范路径写进运行包。
- 定位新旧问题用 A/B：保留上一版 DLL，换回去让用户只看几个点；判断某份日志属于哪个构建，用日志里能区分构建的特征（例如"保留 N 个原作者行为补丁"），不要只看文件时间。部署时保留最近两版即可。
- 同一程序集名的两个不同 Mod 不能同时运行（如 Raven 与狐猎nsfw 都叫 `SilentSkinMod`）；功能不全时先查这个。
- 怀疑某个第三方包本身坏了：用 `ModApiScan` 扫它调用的游戏接口和补丁参数（工具源码由 yuinn 一方提供，需要时向其索取）。

## 工坊发布（2026-11-24 白猫要求）
- 工坊发的那一套，就是「这次修好后打包给本地用」的同一版本、同一产物：修好 → 双引用构建 → 部署本地（两份安装包 + 游戏 `mods`）→ 作者自测 → 直接把同一份产物上传工坊。**不许为发布另起一套构建。**
- **更新日志**：以同一工坊条目上一次实际发布的包为基线，对照本次包、提交和验收记录，逐项覆盖期间所有实际发布的新增、调整和修复（含漏发后本次入包的改动）。中英双语只写工坊「改动说明」，简介不放版本公告。
- 发布前逐项核对，任何一项对不上就停下来查：
  - 版本号四处一致：`STS2SkinChanger.csproj` 四字段、`Entry.InternalTestVersion`、`SkinChanger.json`、`workshop/workshop.json`。
  - 三个文件与上传目录逐字节一致：`Gurio.SkinChanger.dll`、`thunninoiSkinManager.dll`、`SkinChanger.json`（比 SHA256，不比大小）。
  - 包内 `SkinChanger.json` `version` 与工坊文案的 `version` 是同一个号。
- 注意 DLL 同尺寸不代表同版本：PE 对齐会把只改版本号的产物压成相同字节数（1.0.40 与 1.0.41 都是 3,781,632 B，哈希不同），只能比 SHA256 或直接读包内 `SkinChanger.json`。
- **严禁回旧提交挑一套发布**：发现问题就在当前版本上修，不许 checkout 历史提交、拿它的产物顶替，也不许拿旧 DLL 配新文案。
- `thunninoiSkinManager.dll` 是本 Mod 自带的行为隔离兼容层（`compat/ThunninoiSkinManagerCompat`，产物名与第三方框架同名），必须随包发布。它缺失不会让本 Mod 报错（`FrameworkCompatibilityLayer` 只记一条 warn 后返回），但依赖 thunninoi 框架的皮肤包会失去内置接口、交回游戏加载器；玩家没装原版管理器时这些包就不生效。
