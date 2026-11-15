# 商人模块边界勘察

- 执行：GPT（龙娘），2026-11-14；复核：Claude。
- 基线：本地 HEAD `092ec98`，1.0.31.93；结论来自本次读取的工作区源码。Claude 正在并行开发 Safety，本报告没有把其未交付代码作为既定接口。
- 范围：只读勘察；仅交付本文件。未改 .cs、未构建、未运行测试、未启动游戏、未部署、未 commit。
- 已读任务书、重构计划阶段 C、问题对照表第三节与经验教训、`docs/merchant-isolation-design.md`。下列源码位置均相对 `STS2-SkinChanger_修改件`；行号用于定位，后续修改可能漂移。
- 定位方式：当前没有可调用的 codebase-memory 图工具，PATH 中也无其 CLI；`rg` 不可用，采用 PowerShell `Select-String` 和按行读取。没有建立或修改索引。

## 1. 结论与现状纠偏

第一批可迁移的是“商人本体、手部、商人预览、商人提供者生命周期”。不能把整个 `MerchantRuntimeAppearance.cs` 按文件整体归为商人模块：它还承载商店内玩家模型、角色变体、休息点回放和联机相关行为。商人模块关闭时，这些角色功能仍可能需要运行。

当前三个必须先落实的合同：提供者按使用方持有；延迟回调带生命周期代次；主干负责覆盖层恢复。只搬补丁类、按本地配置开关，会留下活房间失去补丁、共享 DLL 被误停用及旧回调重新写节点的窗口。

旧隔离设计是方案，不是实现完成证明，以下差异以源码为准：

| 项目 | 当前源码与迁移约束 |
| --- | --- |
| 真/假商人分组 | `MerchantRuntimeAppearance.cs:36` 是 `merchant` / `fake_merchant_monster`。设计稿的 `fake_merchant` 是拟议名字；首迁保留现有配置 ID，不能直接改名。 |
| “只保留资源替换” | `TryRefreshMerchant`（:954）仍替换按钮与手容器；`TryRefreshFakeMerchant`（:1121）仍替换按钮；保留库存模型及购买状态，不等于没有节点重建。 |
| “关闭全部增强刷新” | `EnhancedMerchantRefreshEnabled=false`（:869）仅在 `TickRefreshAfterLoad`（:903）被检查。热切换、Ready bridge、进树前重放、位姿纠正没有受此开关统一控制。 |
| SL 看门狗 | 代码保留了房间 ID/选择跟踪和有界重试（:856–952），但当前总是提前返回。不能用报告里“240 帧重试已修”推断本基线 SL 已被该路径保障。现有失败分支在第 240 次标记完成；后续若替换调度器，需保持明确的次数/时间上限。 |
| 不双份执行 | 加载器将受管 Ready 补丁摘下后保存并重放（`ManagedSkinModLoader.cs:1321–1417`）；按钮又有进树前重放（`MerchantRuntimeAppearance.cs:1525`）和 Ready Prefix bridge（:2063）。这是需核验的重复调用路径，不能仅凭“补丁钉住”认为恰好执行一次。 |
| 防误摘外部包 | `VisualPatchGuard.cs:65–74` 当前条件是 `(IsManagedProviderAssembly || IsMerchantFacingPatch) && !IsProviderAssemblyActive`。名称带 Merchant 的非受管程序集仍能越过受管前置条件；不等于报告中严格的“只摘受管包”。 |
| 场景收窄豁免 | `ManagedSkinModLoader.cs:1059–1079` 除当前 merchant 选择外，还豁免所有 ID 含 Merchant 的活动提供者。它解决过早卸载，但不是按实际使用方释放的生命周期，也可能留下已经不用的提供者。 |
| 帐篷包检测 | `FakeMerchantEventFrame.cs:113–149` 先读取 settings.save，但仍有本地 `mods/NSFW-Event.json` 存在即命中的兜底；不能描述成完全没有目录/残留误判风险。 |
| PCK 停用 | `SkinService.cs:6250–6266` 明确按“已挂载 PCK 不能卸载”处理，依赖全组 reassert 恢复归属。仓内未找到 PCK 卸载实现；设计稿“卸载 pck”不能直接照搬。 |

以上是迁移约束与静态风险提示，不是新增实机故障结论。本次没有验证报告所述 a/b/c 包在本机的视觉表现，也没有把不在仓库的 .94/.95 修复视为已存在。

## 2. 入口 / 状态 / 补丁 / 对外依赖

### 2.1 入口与文件边界

| 文件、类、方法 | 入口和行为 | 边界归属 |
| --- | --- | --- |
| `Ui/MerchantRuntimeAppearance.cs:143–365`：`BeginSelectedMerchantNodeReady`、`EnsureSelectedMerchantProviderActive`、`CompleteSelectedMerchantNodeReady*`、`ResolveMerchantGroupId`、`RememberMerchantFigureProvider` | Ready 前保证行为激活并重放 Prefix；Button/Hand 下一帧再等 Spine 就绪后重放 Postfix；Inventory 直接完成；记录模型归属并绑定包设置。 | 商人 presenter / 提供者适配。游戏成员探测下沉 GameApi，包回放继续由受管加载器执行。 |
| 同文件 :472–499：`ScopePartTakeover`、`PrepareMerchantSelectionChange` | 记录部件原画；切换前还原旧库存回调，移除重放新增节点。 | 商人选择事务的清理阶段，需在旧选择被覆盖前调用。 |
| 同文件 :901–1219：`TickRefreshAfterLoad`、`TryRefreshMerchant`、`TryRefreshFakeMerchant` | 读档轮询目前停用；真商人替换按钮和手部并保留 Inventory；假商人替换按钮并恢复/重放房间、库存、手部呈现。 | 商人 presenter；真假入口不同，共用类型不能据此合并选择。 |
| 同文件 :1420–1690：`ReplaceMerchantButton*`、`InstantiateMerchantButton`、`ReplayNodeReadyBeforeTree`、`InstantiateMerchantPreviewScene`、`PreparePreviewNativeReferences`、`GetMerchantScenePath`、`LoadRuntimeOrBaseScene` | 实例化、节点归属、信号重连、进树前重放、预览引用准备、场景资源加载。 | 分拆为 presenter、预览和 GameApi。资源实例化作用域仍由 SkinService 提供。 |
| 同文件 :585–840：`InstallMerchantPreviewInputProxy`、`ApplyMerchantPreviewFocus`、`PrepareMerchantPreviewInteraction`、`PrepareFakeMerchantPreviewInteraction` | 预览悬停/控制器焦点、原生商店打开关闭、返回按钮、预览库存模型。 | 商人预览；预览购买阻断必须和预览实例同寿命。 |
| 同文件 :501：`TryCreateCurrentMerchantRoom` | 旧房间整体重建工厂仍保留；本次全主工程检索只找到声明，未见调用；旧 `NMerchantRoom.Create` 路线在当前 Ready 补丁旁已注明废止。 | 遗留候选，不列为当前有效入口；不能因为它存在就恢复整房间重建。 |
| `Ui/CharacterAppearanceScreen.cs:1050–1097`、:1653–1741 | `SelectMerchantTarget` 找 Catalog 分组；真商人选择链为 Prepare → ApplySelection → TryRefresh；失败重新 Apply 旧选择并刷新、更新按钮引用。假商人有独立选择/回滚链。 | 通用面板保留；调用商人窄接口，不内嵌商人生命周期。 |
| `Ui/AncientCompendium.cs:2249`、:2323、:2330 | 调用商人预览实例化及真假交互初始化。图鉴不是只读图片，使用真游戏节点。 | 通用图鉴保留调用端，商人模块提供预览租约和工厂。 |
| `Core/MultiplayerSkinSync.cs:1032–1054` | `Tick` 在联机参与/连接检查之前调用 `TickRefreshAfterLoad`，因此单机也会调用。目前商人函数内提前返回。底部 :2675/:2692/:2720 是不同 Tick 驱动。 | Tick 调度是主干；商人只注册/注销自己的工作项，不能卸载整个联机 Tick。 |
| `Core/SkinService.cs:1600`、:1655、:1908、:4477–4535、:5080–5180、:7031 | 预热、场景范围收窄、选择事务、资源作用域内实例化、提供者解析、本地和远端选择合并。 | 主干；通过上下文窄接口供商人使用。 |
| `Catalog/SkinCatalog.cs:1379`、:1862、:8576–8713、:9105–9124 | `IsGroupAnchorResource` 排除只有手的包被当整商人；`HasDirectVisualPatchTargetMetadata` 包含商人按钮/手；`TryGetPrimaryGroup` 和路径正则识别真假商人，角色 merchant 场景另归角色。:8710 的“不识别 merchant”注释与前面的实际分支不符。 | 目录扫描主干 + 商人分类规则。首迁保留已有分类输出，不重写整个探测器。 |
| `Catalog/SkinCatalog.Events.cs:189–240`、`Core/FakeMerchantEventArtPolicy.cs` | `FakeMerchantEventPresentation` / `ScanFakeMerchantEventPresentation` 收集事件帐篷/战斗背景资源；`AddFakeMerchantEventPresentation` 参与覆盖层。 | 假商人事件美术边界；与商人本体选择分开，挂载归主干。 |
| `Core/ManagedSkinModLoader.cs:1041–1084`、:1092–1424、:1489、:1779–1886、:2304/:2344、:2888 | 补装、激活、隔离和保存 Ready 回调、场景豁免、停用并恢复节点、回放和模型位姿归属。另有 :2528–2675 部件呈现及商人反馈控制。 | 加载器/程序集共享状态留主干；商人声明所需 provider 和策略。 |
| `Core/VisualPatchGuard.cs:34–122` | `RemoveProviderVisualPatches`、`ResolveProtectedRoots` 过滤受管/活动/受保护包，再精确摘回调。 | 全局隔离主干；禁止随商人模块卸掉整个 guard。 |
| `Core/PartTakeoverVisualScope.cs:29–111`、:340–362 | `Capture`、`Reconcile`、`RememberMerchantFigure`；只调商人本体/手、模型显示、位姿、语音和包反馈，不改房间及商店 UI。读取 `IsBaselinePartOverride` 选项。 | 首迁通过商人专用 facade 调用；保留第三方部件所有权。 |
| `Core/ManagedMerchantSettingsBridge.cs:26–160`、`MerchantSettingsContract.cs`、`MerchantSettingsAssemblyCompatibility.cs` | 改写并接入包自己的设置合同；`Bind`、`Refresh`、`RequestRefresh` 在执行时重新检查选择与节点；`ProviderSettingsModInfoPatch` 提供设置面板入口。 | 包兼容枝条；共享设置容器/程序集注册服务留主干，不能在停用时全局清空。 |
| `Core/FakeMerchantEventFrame.cs:48–157`、:249 | `Schedule` / `FitAsync` 最多等 300 帧后调整事件构图；包启用检测与帐篷资源有关。 | 假商人事件美术分支，不以真商人是否选皮肤为唯一激活条件。 |
| `Core/BackgroundSkinService.cs:3796–3811`：`FakeMerchantCombatArtPatch` | 假商人 encounter 在创建背景前挂事件美术；:785 另有预览/背景路径按需保证美术挂载。 | 事件/背景与商人交界；首迁保留原归属并声明依赖，不能漏掉战斗帐篷。 |

### 2.2 状态清单

| 位置 | 状态 | 停用要求 |
| --- | --- | --- |
| `MerchantRuntimeAppearance.cs:64–119` | 按实例 Type 缓存的 MerchantButton 字段、其他 FieldInfo/MethodInfo；`ReplayedInventoryAdditions` 弱引用列表；`ShopProviderRoots` 按玩家模型实例 ID 记录附加根。 | 反射缓存可留 GameApi；节点/新增物按房间清理。`ShopProviderRoots` 属于玩家外观支线，不能随商人 NPC 开关全清。 |
| 同文件 :53–59、:860–869 | 预览标记/焦点元数据；玩家/包根位姿基线；读档 `_watchedRoomId/Attempts/Done/Selection` 和增强刷新常量。 | 预览会话、房间代次分别持有，换房间/选项/停用时重置；不把常量关闭当作取消延迟任务。 |
| `Ui/RoomRuntimeScopeOwnership.cs:10–38` | 实例→scope lease 弱表；退出时带 expected lease 释放，防旧房间退出撤销新房间范围。 | 继续由主干协调，迁移不得退化成按 provider 名释放。 |
| `ManagedSkinModLoader.cs:1395–1417`、:2880–2916 | `ProviderRuntimeBlueprints`、`ActiveProviderRuntimes`、程序集→provider 身份、回放变更记录、figure/group 元数据。 | 程序集可能有多个资源来源和多个组使用；通过实际行为程序集身份与引用计数释放。 |
| `PartTakeoverVisualScope.cs:25`、`ManagedMerchantSettingsBridge.cs:15–24` | 活房间原始画面弱表；每程序集设置 session、targets、Pending/Applying。 | 原画还原和设置延迟任务必须覆盖预览及活房间；不触及未受管第三方全局状态。 |
| `SkinService.cs:136`、`SkinCatalog.Events.cs:189` | 假商人美术是否已挂及扫描缓存；全局覆盖路径归属另由 SkinService 管理。 | 逻辑停用不等于物理卸包；由主干重建/恢复优先级和缓存。 |

### 2.3 自有补丁装卸表

当前 Entry 自动装所有带 HarmonyPatch 的类（`Entry.cs:50`），另在 :72 手动装 concrete Ready。迁移时必须同时从原安装路径排除模块补丁，否则会装两次。建议模块专属 Harmony ID，但共享桥要拆方法/回调后按所有权处理，不能对 `Entry.ModId` 或第三方 owner 做全量 UnpatchAll。

| 类（均在 `Ui/MerchantRuntimeAppearance.cs`，另注明者除外） | 目标 / 回调 | 迁移归属 |
| --- | --- | --- |
| `MerchantRoomPrewarmPatch` :1798 | `NMapScreen.Open` Postfix | 商人预热；无商人需求时不做预热工作。 |
| `MerchantRoomCreateAppearancePatch` :1815 | `NMerchantRoom._Ready` First Prefix 获取 scope；Last Postfix Record/Capture | 房间生命周期共享桥；商人范围请求可移模块，lease 记账不能丢。 |
| `MerchantRoomConcreteReadyPatch` :1849 | 按名发现 Standard/Blitz 的 `_Ready`，手动 Postfix 做 Capture/CorrectDoubleAppliedPose | 商人；`PatchClasses` 自动枚举不会覆盖这个 Install，需要显式安装句柄。旧版无类型则跳过。 |
| `MerchantRoomPlayerAppearancePatch` :1889 | `NMerchantRoom._Ready` Prefix 阻断预览 gameplay；Postfix 本地玩家位姿、角色房间回放及商人 Reconcile | 混合类：拆为预览防护、角色外观、商人 Reconcile 三个职责；不能整类跟随 merchant=false 卸载。 |
| `MerchantRoomPreviewEnterTreePatch` :1948 | `NMerchantRoom._EnterTree` Prefix 只阻断预览 | 商人预览防护，有预览活着就保留。 |
| `MerchantRoomPreviewExitTreePatch` :1957 | `NMerchantRoom._ExitTree` Prefix 阻断预览；Postfix 释放真实房间 scope | 预览与共享房间清理；最后一个 owner 退出前不可卸清理。 |
| `FakeMerchantAppearancePatch` :1976 | `NFakeMerchant._Ready` Prefix 预览阻断/挂事件美术/记录 scope；Postfix 回放及 Reconcile | 假商人；事件美术工作要由独立需求驱动。 |
| `FakeMerchantPreviewEnterTreePatch` :2023 | `NFakeMerchant._EnterTree` Prefix | 假商人预览防护。 |
| `FakeMerchantPreviewExitTreePatch` :2032 | `NFakeMerchant._ExitTree` Prefix/Postfix | 假商人预览防护 + 活实例 scope 清理。 |
| `MerchantButtonProviderReadyBridgePatch` :2063 | `NMerchantButton._Ready` First Prefix / Last Postfix | 真假商人共用：Begin、记原始模型、Complete、记 provider。 |
| `MerchantInventoryProviderReadyBridgePatch` :2083 | `NMerchantInventory._Ready` Prefix/Postfix | 真假商人共用：Begin/Complete。 |
| `MerchantHandProviderReadyBridgePatch` :2097 | `NMerchantHand._Ready` Prefix/Postfix | 真假商人共用：Begin/Complete。 |
| `MerchantPreviewPurchaseBlockPatch` :2116 | `NMerchantSlot.OnSelected` Prefix 对预览返回完成 Task | 商人预览安全边界；预览销毁后才能卸，真实购买仍走原方法。 |
| `RestSitePlayerAppearancePatch` :2051 | `NRestSiteRoom._Ready` Postfix 回放角色呈现 | 角色/休息点；留在商人模块之外。 |
| `FakeMerchantEventFramePatch`（Core 同名文件 :249） | `NFakeMerchant._Ready` Postfix 启动 FitAsync | 事件美术子功能；退出要使异步任务失效。 |
| `ProviderSettingsModInfoPatch`（`Core/ManagedMerchantSettingsBridge.cs:154`） | `NModInfoContainer.Fill` Postfix | 共享设置入口；不能因为 NPC 模块关闭影响其他提供者设置。 |
| `FakeMerchantCombatArtPatch`（`Core/BackgroundSkinService.cs:3796`） | `EncounterModel.CreateBackground` First Prefix | 事件/背景边界，保留首次加载前挂美术的顺序。 |

额外两条“名字含商人但不能并入 NPC 模块”的补丁：`Core/MultiplayerRoomAppearancePatches.cs:23` 的 `MultiplayerMerchantPlayerVisualIsolationPatch`，目标 `NMerchantRoom.AfterRoomIsLoaded`，为每位玩家建立 `BeginPlayerRuntimeScope/RemoteRoomSceneLease`；`Ui/ProviderVariantPathPatch.cs:13` 的 `ProviderMerchantVariantPathPatch`，目标 `CharacterModel.get_MerchantAnimPath`，属于角色变体。关闭商人 NPC 功能不能关闭联机玩家区分和角色变体。

## 3. SkinService / SkinCatalog 依赖与窄接口

这里的“内部成员”区分直接调用和传递依赖：`MerchantRuntimeAppearance` 没有直接访问 SkinService 私有字段，也没有直接调用 Catalog；它依赖内部静态服务公开的方法。Catalog 依赖来自 SkinService、面板和 PartTakeover 等链路。迁移不应为了拆模块把私有字典全开放。

| 现有依赖 | 证据 | 建议给模块的窄接口（拟议，尚不存在） | 保留在主干的内容 |
| --- | --- | --- | --- |
| `GetSelectedFullRuntimeProvider` / `GetSelectedRuntimeProvider` | MerchantRuntimeAppearance :148/:326/:885/:1008/:1152/:1529；SkinService :5080/:5154 | 只读 `GetMerchantSelection(group)` 返回 option、视觉 provider、行为 provider、运行时资格、选择代次和生效 scope。保留两种资格的差别。 | GetVisualSelection 的本地/预览/联机上下文合成；完整运行时全组选中判定。不能以 `Config.Selections` 直接替代。 |
| `SkinService.Config.GetSelection`、`Get/SetCharacterCombatTransform` | MerchantRuntimeAppearance :390–408 | NPC 模块不需要角色位姿写接口。玩家商店外观另用 `ICharacterAppearanceContext`；NPC 切换只用受限的 `TryApplyMerchantSelection`。 | 配置存储、保存和联机位姿通知；本地玩家变换键及角色语义。 |
| `ApplySelection`、`LastError`、Catalog.Groups | CharacterAppearanceScreen :1058–1096/:1659；SkinService :1908 | `TryChangeSelection(group, option)` 返回结构化结果；模块负责清理/刷新，主干保有事务及失败回滚。选项读取给固定商人两组的快照。 | Catalog.BuildVisualSelectionTransaction（:2310）、持久化、全组联动、失败恢复；首迁先保持既有语义，再另审独立分组事务。 |
| `InstantiateRuntimeScene<T>`、`GetOrLoadRuntimeScene`、`PrewarmRuntimeSceneOverlay` | MerchantRuntimeAppearance :987/:1489/:1568/:1651/:1805；SkinService :4505 | `IMerchantSceneResources`：按组、房间类型预热和在资源 lease 内实例化；返回失败而非暴露挂载字典。 | Sync 锁、依赖闭包、路径归属、包栈、Preload/ResourceLoader 缓存；不能先返回 PackedScene 再脱离作用域 Instantiate。 |
| Focus 行为范围、`RoomRuntimeScopeOwnership.Record/Release` | MerchantRuntimeAppearance :192/:1819/:1991；SkinService :1655 | `AcquireMerchantUse(owner, group, purpose)` / 释放句柄；purpose 至少覆盖活房间、预览、切换中使用方。 | `_runtimeProviderBehaviorScope`、RuntimeProviderScopeLeases、远端 provider 并集、DeferredProviderActivator 协调；共享加载器生命周期。 |
| `MountFakeMerchantEventArt` / `EnsureFakeMerchantEventArtMounted` | SkinService :5033/:5067、:6647；Catalog.Events :189 | 独立 `EnsureFakeMerchantEventArt()` 资源需求，和 NPC 选择分离。 | `MountOverlayDelta`、`MountedOverlayOwners`、`_fakeMerchantEventArtMounted`、覆盖层恢复及写盘保护。 |
| Catalog 提供者/选项解析 | SkinService :5089–5097；Catalog :523/:1924/:2135/:2145/:2209/:2261/:2502 | `MerchantCatalogSnapshot` 给两个组的有效选项、是否部件选项、资源/行为身份及需求标志。已安装资源不等于所有 DLL 都要激活。 | `_groups`、`_cosmeticIndexes`、`_fullRuntimeProviderGroups`、同 ID 不同来源、provider 归一化、资源归组及全目录探测。 |
| PartTakeover 目录与选择 | PartTakeoverVisualScope :340–362/:513 | `Capture/Reconcile/RestoreMerchantParts(owner, group)`；描述部件原画、模型/手、语音归属。 | 包保护 roots、未受管包所有权及全局回调/资源隔离；不能把风格/UI 包改判为整包皮肤。 |

第三方提供者接口另经 loader facade：Acquire/Release 行为租约、ResolveBehaviorProviderId、受管 Ready 回放/恢复、设置 Bind。它们不是 GameApi；第三方程序集探测、原设置合同和程序集改写放兼容枝条。Provider 的来源后缀和行为身份要分别保存：不能只去掉 `::source:` 后缀就把两个选项当同一份美术；比较行为资格时使用统一解析后的身份，且坚持全等，不能用前缀 Contains。

## 4. GameApi 第一批候选

以下来自源码访问点，未对游戏 DLL 做新的反编译。适配层应返回“支持/缺失/失败”，按实际类型缓存能力；缺成员时停用对应子路径并保留原版，不能让类型加载或资源异常中断整个 Mod。类型缺失探测必须早于静态硬引用/补丁枚举。

| GameApi 能力 | 当前游戏类型/成员与定位 | 迁移注意 |
| --- | --- | --- |
| 商店类型与 Ready 目标 | `MegaCrit.Sts2.Core.Nodes.Rooms.NStandardMerchantRoom` / `NBlitzMerchantRoom`，按名找 `_Ready`；MerchantRuntimeAppearance :1849–1873。基类 `NMerchantRoom._Ready`。 | v0.107 无两个具体类；v0.113 基类 Ready 早于具体类字段初始化，仍需具体类完成后回调。发现目标去重，避免把继承同一方法补两次。 |
| 按钮引用读写 | `ResolveMerchantButtonField` :66–90 沿实际实例类型/基类找 `<MerchantButton>k__BackingField` 或 `_merchantButton`；NFakeMerchant backing field :92/:815 | 不固定从抽象 NMerchantRoom 查字段；统一 `TryGet/TrySetMerchantButton(owner)`，缓存实际 Type，失败不留半替换引用。 |
| 库存与继续按钮 | `NMerchantRoom.<Inventory>k__BackingField`、`_proceedButton`；`NFakeMerchant` 同名字段（:98–105） | 同时用于预览与实机；明确 capability 不满足时禁止可交互预览。 |
| 手部引用 | `NMerchantInventory.<MerchantHand>k__BackingField`（:96–97） | 替换后维持库存对象、购买状态及 BackButton 交互；不能重建库存模型。 |
| 假商人交互 | `NFakeMerchant.OnMerchantOpened` / `OpenInventory`（:94/:106） | 信号重连和预览打开路径通过受控调用，保留事件原回调。 |
| 房间旧重建字段 | `NMerchantRoom.<Room>k__BackingField`、`_players`、`_dialogue`（:108–113） | 仅旧 TryCreateCurrentMerchantRoom 使用；先列遗留，不作为新模块必须接口，不恢复过时工厂。 |
| 原生商人骨架与焦点 | `NMerchantButton._merchantSkeleton`；`MegaSkeleton.SetSkinByName`（:114–117/:669） | 预览焦点等 Spine 就绪，缺能力时保留基础预览，不补丁接管真实 RefreshFocus。 |
| 联机商店玩家创建 | `NMerchantRoom.AfterRoomIsLoaded` 和 Harmony 注入 `_players`、`_characterContainer`、`_playerVisuals`（MultiplayerRoomAppearancePatches :25–34） | 私有字段注入也是成员耦合；后续放 GameApi 的玩家房间能力，保持 NPC 模块之外。 |
| 场景路径与节点合同 | `standard_merchant_room.tscn` / `blitz_merchant_room.tscn` / 旧 `merchant_room.tscn` 版本能力；`fake_merchant.tscn`；`merchant_inventory.tscn`；`%MerchantVisual`、`%MerchantButton`、`%Inventory`、`%ProceedButton`、`%MerchantSelectionReticle`、`MerchantHandContainer` | 路径虽不是反射成员，也应由适配器统一选择。当前 :47 能分 Standard/Blitz，但 :1634 的 GetMerchantScenePath 对真商人固定回 Standard，热切/预热不能据此声称 Blitz 已覆盖。 |
| 商店玩家角色路径 | `CharacterModel.get_MerchantAnimPath`、`NMerchantCharacter`；MerchantRuntimeAppearance :1221 起，ProviderVariantPathPatch :12 | 保持角色/玩家功能；不要随商人 NPC 的 ShouldBeActive 关闭。 |

预览依赖的公开工厂（`Player.CreateForNewRun`、`RunState.CreateForTest`、`MerchantInventory.CreateForNormalMerchant`）也应经适配接口封装，尤其是 v0.113 删除旧 per-entry builder 的历史。第三方 `Settings.Save`、ApplyHand/ApplyLegs、初始化器、Harmony 回调属于包适配合同，不混入官方 GameApi。

## 5. ShouldBeActive 与生命周期方案

### 5.1 激活判定分两层

`ISkinFeature.ShouldBeActive(SkinConfig, SkinCatalog)` 是模块能力开关，不能直接等同“当前选中的 DLL 要运行”。建议它只读已有目录快照，满足任一项则需要商人模块：

1. `merchant` 或 `fake_merchant_monster` 中存在有效、非 Base 的可选外观，包括纯 PCK 外观、受管 DLL 外观以及 `IsBaselinePartOverride` 的 UI/风格包部件；不把只有手部的普通风格包误判为完整 runtime provider。
2. 生效的非 Base 选择可解析为这两组的有效选项。失效配置不应反复尝试初始化缺失包，交由目录/配置修复反馈。

两个组都没有有效选项且没有使用需求时不装 NPC 重放/预热/轮询。目录未就绪时标为待重新评估，不把首次 false 当本会话永久禁用。评估不得逐帧扫磁盘、读 DLL 或再扫 PCK。

草案的两个参数不足以判断活房间、预览、切换中或联机需求，必须由 FeatureRegistry 主干合并 usage lease：

`实际保留 = ShouldBeActive(配置,目录) || 活房间/预览租约 || 尚未完成的切换与清理租约`。

远端角色外观不应直接把 NPC 模块打开；共享 DLL 的远端/角色使用方由主干另计引用。若未来确实同步商人 NPC 选择，须明确新增其需求源，不能从目前 Tick 位于联机文件推断已有这套协议。

图鉴创建原生预览前要同步取得租约并保证防护补丁已装；即使预览的是 Base 也一样。未能取得完整预览能力时不创建可交互原生场景。所有已安装选项可使模块就绪，但仅被真实使用的 provider 才能装行为补丁、挂所需资源；不能把“有安装包”变成“全包初始化”。

### 5.2 Activate

1. 在主线程、按模块状态串行激活，建立代次和模块 breaker；先探测 GameApi 能力及变更入口，失败时只关闭不支持的子路径。
2. 从 Entry 原自动枚举中分离本模块回调；先装预览/房间生命周期防护，再装 Button/Inventory/Hand 桥和所需预热、假商人分支；显式登记 concrete Ready 手动目标及 callback。共享玩家与房间清理保持主干注册。
3. 通过资源服务保证所用包/覆盖层已就绪，再通过 provider lease 激活所选行为与设置适配。不要复制加载器的 blueprint/程序集字典，也不要再跑一次无条件 PatchAll。
4. 决定并落实每个回调的唯一执行权：首迁建议保留“加载器摘受管 Ready、bridge 重放”现状机制，但对同一 node/provider/代次只执行一次 Prefix，并保留 Spine 就绪后的 Postfix。若以后改成让包原补丁直接运行，则先对那些目标禁用所有重放及全局 managed-ready 隔离；两种方案不可混装。
5. 热切或延迟激活发生在现有房间时，调用一次受控刷新；不要直接再跑完整游戏 `_Ready`（会重复玩法初始化、信号和地图/库存逻辑）。读档如需恢复重试，单独设计有界工作项；当前关闭的 watchdog 不自动打开。

### 5.3 Deactivate

1. 先拒绝新工作并推进代次，使 CallDeferred、RunWhenSpineReady、设置 RequestRefresh 和 FitAsync 的旧任务在执行处退出。仅“当前 provider 仍等于同一个字符串”挡不住 A→B→A 或同 provider 新房间。
2. 在补丁/记录仍可用时，清理该 owner 的新增节点、设置目标和原画变更；保留 Inventory、已购物品、金币、库存开关、原生信号和 UI/风格包原画。恢复失败有独立日志和主干降级路径，不让 breaker 阻止清理本身。
3. 商人/假商人/预览/其他模块对同一行为程序集分别释放 lease；只有最后一个使用方释放才执行受管停用。当前 `DeactivateProvider` 是加载器私有方法，且 `DeactivateProvidersExcept` 有名称豁免，现成 API 不足；需要主干新增按使用方释放的合同，不能由模块反射调用私有方法或复制清理逻辑。
4. 释放资源使用权后由 SkinService 恢复规范路径优先级、预加载缓存及必要的全组 reassert；不承诺物理卸 PCK 或卸程序集。已挂资源不代表仍允许其后台行为和扫描运行。
5. 摘模块拥有的精确 target/callback，含手动 concrete Ready；不要整 owner 摘掉第三方/共享补丁。活预览必须先销毁再卸 Enter/Exit/Ready 与购买阻断；活房间必须先转交清理 owner 或等退出，再卸 scope Release。共享角色/联机补丁始终由各自模块管理。

熔断时也按上述收尾；模块“本会话禁用”不意味着允许把活的商店留在半初始化或丢失购买防护的状态。

### 5.4 时序风险与对策

| 时序 | 风险窗口 | 必须保留的保障 |
| --- | --- | --- |
| 读档、房间 Ready | 基类 Ready 先于 concrete 字段；资源/Spine 后到；当前 watchdog 不运行。 | concrete Ready 能力探测 + 资源就绪信号/有界重试；节点有效且不待释放后才应用，成功一次即停止。 |
| 换房间，新房间已创建旧房间才 Exit | 旧 Exit 清理可能撤新 scope，或者模块过早摘掉 Exit 清理。 | 实例 owner + expected scope lease；不能按同一 provider ID 复用旧清理权。 |
| 局内真/假商人热切 | 配置先变、provider 已停，而旧按钮/库存及延迟回调还活着；共享 DLL 仍服务另一组。 | Prepare 读旧选择，切换持独立 lease；统一失败回滚；最后使用方才停 DLL；不可按字串名常驻豁免代替引用管理。 |
| 按钮进树与下一帧 Spine ready | 进树前和 bridge 重放重叠；旧回调在切换后执行。 | 每节点/行为 provider/代次的 Prefix/Postfix 状态；回调执行点重新检查 owner、代次、选择和节点。 |
| 图鉴预览开关、功能停用 | 预览真实 NMerchantRoom/NFakeMerchant 没有真实 run/map；卸保护后可能运行玩法路径或购买。 | Acquire 在 Instantiate/AddChild 之前；最后预览释放后才卸预览防护和购买阻断。 |
| 联机玩家进入同一商店 | 仅看本地配置会漏掉远端玩家 runtime provider；卸掉混合类也会丢本地角色位姿/远端区分。 | 主干维护所有选择和程序集使用并集；保留 MultiplayerMerchantPlayerVisualIsolationPatch 与角色 MerchantAnimPath 变体。Tick 单机调度仍可用。 |
| 假商人事件转战斗 | NPC 选择仍 Base，但帐篷美术有需求；停用真商人会误卸事件资源请求。 | 事件/战斗背景能力独立判断并在场景绑定骨架前挂资源；保持现有 canonical redirect 抑制策略。 |

## 6. 最易回归的五组与实机验收

下表是交给 Claude 的后续实机验收计划，均未在本次执行。a/b/c 沿用问题报告用语：a/b 为 Merchant2CuteII / Merchant2CuteIIB 两个近名包，c 为 ATA_Merchant；现场须记录实际所选 option、视觉 provider、行为 provider、游戏版本和房间类型，不只看下拉文字。

| 组 | 覆盖报告第三节 | 实机步骤与通过标准 |
| --- | --- | --- |
| 1. 跨版本与房型 | 第 1、2 条：抽象基类/字段迁移、旧场景路径 | v0.113 Standard 商店首次进入→开关库存→热切→退出重入；可进入 Blitz 时同测，核对实取场景和字段宿主类型。旧 v0.107 可用时独立验证启动与商店；不可用则明确未验旧版。不得 TypeLoadException、缺字段硬崩或“不是场景”异常，库存/返回/继续均正常。 |
| 2. SL 与换房间收窄 | 第 3、6 条：SL 原皮、场景收窄摘商人补丁 | 分别选 a/b/c→进商店保存→退出读档→离开到非商店→再次进商店；让旧房间退出与新房间创建相邻。观察首帧/就绪后外观和 provider 活动日志：所选模型/手持续正确，无变大、原皮或收窄后无回放；不能把停用 watchdog 当已有补救。 |
| 3. 连续热切、近名 ID、重复位姿 | 第 4、5、7 条：a/b 左上角、c 原版、错包、双份位姿 | 在同一活商店执行 Base→a→b→c→a→Base，再 A→B→A 快速切；库存打开和关闭各做一轮，至少重复三轮。记录模型位置/缩放及 Prefix/Postfix 次数：每代每节点一次，a/b 不混，c 显示阿塔，本体/手一致、不累积位姿；买过的条目不复活、不重复扣款，返回不被新增 Control 吞掉。 |
| 4. 包所有权与共享使用方 | 第 8 条，兼第 6 条 | 覆盖游戏原生加载的非受管商人包、受管包、带商人部件的 UI/风格包三种场景；真假商人同 DLL 下设不同选项，另做两名同角色玩家不同皮肤的联机商店。仅切真商人，确认假商人/角色使用方不失补丁，风格 UI/字体/背景不被整包接管，声音按商人选择归属。图鉴 Base/包预览开库存、返回、关闭/停用后无玩法请求及残留画面。 |
| 5. 假商人帐篷与事件/战斗分界 | 第 9、10 条：未启用包被校正、事件名混淆 | 未启用 NSFW-Event 且工坊目录残留、本地清单残留、实际启用该包三种情况分别测；真/假商人选项独立切，进入事件再转遭遇战。未启用保持游戏原构图；启用时事件/战斗帐篷正确，NPC 身体/手随自己的组。报告备用入口是 `event fake_merchant` / `fight fake_merchant_event_encounter`，本次未核验控制台可用性，失败应走自然入口，不能用不存在的 `event fake_merchant_event` 判功能坏。 |

离线后续可补的有效断言：模块无需求→不装工作回调；纯 PCK/部件选项可唤起；共享 provider 最后使用方才释放；旧房间 lease 不撤新房间；A→B→A 的旧代次回调无效；缺 concrete 类型不影响基类和目录；唯一 Ready 执行权。现有 LogicTests 已有 MerchantProviderReadyPolicy、MerchantFigureRestorePolicy、预览焦点/层级、部件与事件美术策略及 scope lease 断言，但不能证明实际补丁只执行一次、Spine 正常或购买状态不变。本次未新增/运行测试。

## 7. 建议迁移交接顺序

1. Claude 先定 FeatureRegistry 上下文：usage lease、代次、资源服务、provider 共享释放合同、精确补丁句柄；Safety 保证异常出口与清理阶段可用。
2. 先封装 GameApi 的 concrete Ready、实例字段和房型场景，再拆混合的玩家/商人/预览回调归属，保持资源和回放策略一致。无需同时重写 Catalog 探测或提供者格式。
3. 首模块保留现有选择/资源事务，落实单次回放后再迁 presenter 和预览；完成上述五组中对应的构建、LogicTests、实机回归，才讨论用“原补丁直接运行”替换受管回放机制。

本报告仅给边界与迁移建议，不构成模块迁移已完成、旧缺陷已重现或部署放行结论。登记表由协调方在验收本产物后解除；本次按任务约束没有修改计划文件。

## 房间通知

已完成商人模块边界勘察（基于 1.0.31.93），仅写 merchant-module-map.md，未改源码、未构建、未 commit。
结论：商人本体/手部与角色、联机职责需拆开；迁移前先定共享提供者租约、延迟回调代次及补丁唯一执行权。文档含 GameApi 候选、装卸清单和五组实机验收；现有读档 watchdog 关闭、PCK 不能直接卸载等差异已标明。
文件：D:\mod\工作2\STS2-SkinChanger_修改件\docs\refactor\merchant-module-map.md
请 Claude 复核，并在接收产物后解除该文件登记。
