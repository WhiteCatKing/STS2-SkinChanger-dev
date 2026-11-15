# 角色与随从模块勘察

GPT，2026-11-15。基于 `refactor/stageA-C-20261114` 的 `cd62cce`，版本 `1.0.31.109`。本次交付为源码勘察与迁移建议，实机验收列在第 5 节，执行留给迁移阶段。源码路径均相对 `STS2SkinChanger/`，文档路径相对仓库根目录。行号取本次基线，CSV 中的旧行号只用于找到原条目。

任务书指向的 `D:\mod\STS2SkinChanger_问题与修复对照.md` 当前不存在，本次采用找到的 `D:\mod\作者仓库\STS2SkinChanger_问题与修复对照.md`，重点使用第六节第 67–75 行。

## 1. 迁移边界

- 第一批适合迁移 16 个角色专属呈现补丁。选择入口、存读档恢复、联机作用域、跨模块资源分发继续由主干持有。16 个类列在第 2 节的“可按需”行，以及第 2.3 节前三行。依据为 `Core/FrameworkSkinRuntimePatches.cs:188–268,328–354,384–419,604–674`、`Core/ManagedCharacterAnimationBridge.cs:221–234`、`Core/OptionalCharacterVfx.cs:9–35`、`Ui/ContextualSkinControls.cs:3948–4000,4072–4125`。
- 随从的主体工作在选择事务和资源隔离。CSV 的 osty 分类只有名字恢复一个类，单凭这个类创建“随从关闭”开关，会漏掉默认外观也要恢复名字的情况。先建立随从窄入口，再决定是否存在值得独立装卸的补丁集合。依据为 `Core/CompanionDisplayNameGuard.cs:18–19,36–58,112–124,132–140`、`Core/SkinService.cs:8980–9078`。
- 现有场景收窄已管理第三方提供者的 DLL 行为，范围覆盖角色、怪物和环境。继续使用这条路径，角色 Feature 负责自己注册的补丁。依据为 `Core/SkinService.cs:1655–1711,7130–7214`、`Core/ManagedSkinModLoader.cs:1101–1125,1527–1558`、`Core/Features/FeatureRegistry.cs:158–175`。

## 2. 补丁逐项归属

### 2.1 阅读方式

CSV 有 character 41 行、osty 1 行。下表合并同一类的多个目标，保留新建单人/多人、读档单人/多人和暂停菜单两个目标。三个手动安装项另列。每个条目都给出当前源码位置，含义如下。

- “可按需”表示只承担皮肤呈现，可归角色 Feature 管理，卸载前仍须满足 IsInUse。
- “入口常驻”包括界面、玩家选择、存读档恢复和退出清理。
- “共享常驻”表示当前类跨模块，后续按明确接口拆分。

### 2.2 character 与 osty 全量条目

| 类或手动安装点 | 游戏目标或职责 | 归属与处理 | 当前源码 |
| --- | --- | --- | --- |
| FrameworkCombatVisualPatch | CharacterModel.CreateVisuals Prefix，场景工厂先于强转运行 | 可按需，角色。保留 First 优先级及 managed 场景标记 | `Core/FrameworkSkinRuntimePatches.cs:188–250` |
| FrameworkCharacterTrailPatch | CharacterModel.TrailPath | 可按需，角色拖尾 | `Core/FrameworkSkinRuntimePatches.cs:254–268` |
| ManagedCharacterAnimationReadyPatch | NCreature._Ready | 可按需，角色。TryDrive 限定 Player.Character | `Core/ManagedCharacterAnimationBridge.cs:46–55,221–227` |
| ManagedCharacterAnimationTriggerPatch | NCreature.SetAnimationTrigger | 可按需，角色，同上 | `Core/ManagedCharacterAnimationBridge.cs:230–234` |
| NecrobinderOptionalHeadReadyPatch | NNecrobinderVfx._Ready Transpiler | 可按需，角色缺头节点兼容。与下一类成对保留 | `Core/OptionalCharacterVfx.cs:9–28` |
| NecrobinderOptionalHeadAnimationPatch | NNecrobinderVfx.UpdateFlameVisibility | 可按需，角色。活 VFX 仍存在时保留补丁 | `Core/OptionalCharacterVfx.cs:32–35` |
| CharacterSkinBundleNewRunBindingPatch | RunManager.SetUpNewSingleplayer / SetUpNewMultiplayer | 入口常驻。开局随机与组合包记录涉及其他分类预设 | `Core/SkinService.CharacterSkinBundleRuns.cs:173–182` |
| CharacterSkinBundleSavedRunPatch | RunManager.SetUpSavedSingleplayer / SetUpSavedMultiplayer | 入口常驻。当前目录无角色皮肤时也可能有旧存档恢复记录 | `Core/SkinService.CharacterSkinBundleRuns.cs:186–202` |
| CharacterSkinBundleRunSavePatch | SaveManager.SaveRun | 入口常驻，保存本局预设 | `Core/SkinService.CharacterSkinBundleRuns.cs:206–212` |
| InRunCharacterAppearanceRuntimePatch | NRun._Ready Prefix / Postfix | 共享常驻，联机同步、全体外观运行节点、提供者范围。怪物和局内界面也依赖 | `Ui/CharacterAppearanceRuntime.cs:2585–2606,265–307` |
| TopBarRoomIconSkinTexturePatch | NTopBarRoomIcon.UpdateIcon Transpiler | 共享常驻，Boss/房间图标。替换压缩贴图加载入口，和怪物地图呈现共用 | `Ui/CharacterAppearanceRuntime.cs:2610–2627,2330–2352` |
| BossMapPointSkinPresentationPatch | NBossMapPoint._Ready | 共享常驻，主要服务怪物/Boss 提供者回调，应在后续怪物批次明确归属 | `Ui/CharacterAppearanceRuntime.cs:2631–2637,2330–2352` |
| CharacterAppearanceCreatureReadyPatch | NCreature._Ready | 共享常驻，角色、随从、怪物的基准捕获、变换包装、血条/意图 | `Ui/CharacterAppearanceRuntime.cs:2640–2645,640–659,958–975` |
| CharacterAppearancePlayerLayoutPatch | NCombatRoom.PositionPlayersAndPets | 共享常驻，角色与随从原生排布快照 | `Ui/CharacterAppearanceRuntime.cs:2648–2653,1540` |
| CombatRoomRuntimeProviderScopePatch | NCombatRoom.Create | 共享常驻，扫描 Allies/Enemies，覆盖角色、宠物和怪物 | `Ui/CharacterAppearanceRuntime.cs:2656–2662,450–477` |
| CombatRoomRuntimeProviderScopeExitPatch | NCombatRoom._ExitTree | 共享常驻，与上项共用场景生命周期 | `Ui/CharacterAppearanceRuntime.cs:2665–2670` |
| CharacterAppearanceBoundsPatch | NCreature.UpdateBounds(Node) | 共享常驻，角色/随从/怪物变换与命中框，含纯怪物缩放分支 | `Ui/CharacterAppearanceRuntime.cs:2673–2683,958–983` |
| CharacterAppearanceOrbPositionPatch | NCreature.SetOrbManagerPosition | 角色呈现，但本体外观编辑也使用。先随常驻变换服务保留，另行拆分 | `Ui/CharacterAppearanceRuntime.cs:2686–2694,1106–1123` |
| CharacterAppearancePauseMenuPatch | NPauseMenu._Ready / OnSubmenuOpened | 入口常驻，局内外观界面同时编辑角色、怪物和随从 | `Ui/CharacterAppearanceScreen.cs:2801–2811` |
| CharSelectStageCyclePatch | NCharacterSelectScreen.SelectCharacter | 入口常驻，战损/阶段切换控件 | `Ui/CharSelectStageCycle.cs:726–731` |
| RandomCharacterLobbySkinPatch | NCharacterSelectScreen.OnSubmenuOpened | 入口常驻，返回大厅恢复随机选择的基准 | `Ui/ContextualSkinControls.cs:3809–3820` |
| CharacterSelectionSkinPatch | NCharacterSelectScreen.SelectCharacter Prefix / Postfix | 入口常驻，布局捕获及选择器挂接 | `Ui/ContextualSkinControls.cs:3823–3830` |
| SingleplayerEmbarkSkinSelectorPatch | NCharacterSelectScreen.StartNewSingleplayerRun | 入口常驻，组合包应用、随机开局和选择器收起 | `Ui/ContextualSkinControls.cs:3833–3860` |
| MultiplayerEmbarkSkinSelectorPatch | NCharacterSelectScreen.StartNewMultiplayerRun | 入口常驻，按本地 Lobby 角色应用组合包 | `Ui/ContextualSkinControls.cs:3863–3890` |
| CharacterVisualResultPatch | CharacterModel.CreateVisuals Postfix | 可按需，角色。远端玩家交给 Creature 层最终写入者，保留变体与外部路径判据 | `Ui/ContextualSkinControls.cs:3948–4000` |
| MonsterVisualScalePatch | NCreatureVisuals.SetScaleAndHue | 共享常驻，实际按 MonsterScaleGroupMeta 恢复怪物缩放，也覆盖随从模型。Safe 键目前为 character，归属整理时一并处理 | `Ui/ContextualSkinControls.cs:4049–4053,3173–3196` |
| CharacterSelectIconResultPatch | CharacterModel.CharacterSelectIcon | 可按需，角色选人头像 | `Ui/ContextualSkinControls.cs:4072–4077` |
| CharacterSelectLockedIconResultPatch | CharacterModel.CharacterSelectLockedIcon | 可按需，角色锁定头像 | `Ui/ContextualSkinControls.cs:4080–4085` |
| CharacterIconResultPatch | CharacterModel.Icon | 可按需，角色头像节点，联机上下文继续由主干建立 | `Ui/ContextualSkinControls.cs:4088–4093,4128–4133` |
| CharacterIconTextureResultPatch | CharacterModel.IconTexture | 可按需，角色顶栏贴图 | `Ui/ContextualSkinControls.cs:4096–4105` |
| CharacterIconOutlineTextureResultPatch | CharacterModel.IconOutlineTexture | 可按需，角色头像描边 | `Ui/ContextualSkinControls.cs:4108–4117` |
| CharacterMapMarkerResultPatch | CharacterModel.MapMarker | 可按需，角色地图标记 | `Ui/ContextualSkinControls.cs:4120–4125` |
| CreatureIntentDepthPatch | NCreature.UpdateIntent | 共享常驻，主体只处理 IsMonster，服务怪物及有意图的随从。后续应与怪物呈现归属一起处理 Safe 键 | `Ui/CreatureIntentDepth.cs:40–50,151–156` |
| ProviderMerchantVariantPathPatch | CharacterModel.MerchantAnimPath | 共享常驻，角色变体在商人房的人物场景路径，与商人场景及预加载共用 | `Ui/ProviderVariantPathPatch.cs:12–22` |
| ProviderRestSiteVariantPathPatch | CharacterModel.RestSiteAnimPath | 共享常驻，角色变体火堆路径，与联机房间资源作用域共用 | `Ui/ProviderVariantPathPatch.cs:25–35` |
| CreatureVisualParentBridge.Register 手动 Transpiler | 第三方 visuals 对 NCreature 父节点的查询 | 共享常驻，变换包装跨角色/怪物/随从。自有 Harmony、已检查程序集集合和弱绑定需保留生命周期 | `Core/CreatureVisualParentBridge.cs:12–39` |
| ManagedCharacterAssetRegistrationGuard 手动 Prefix | 框架 CharacterAssetReplacement 注册入口 | 共享常驻。角色选择控制战斗、商人、火堆、UI 路径，且 Ensure 中安装 FrameworkCardVisualGuard | `Core/ManagedCharacterAssetRegistrationGuard.cs:29–69,101`、`Core/SkinService.cs:1707–1710` |
| ManualCharacterVariantBridge.BindScreen 手动 Postfix | 提供者自己的变体 Toggle | 入口常驻，观察作者控件及保存主动选择。按玩家渲染的 BeginScope 负责临时设置恢复 | `Core/ManualCharacterVariantBridge.cs:19–38,49–65` |
| OstyDisplayNameGuardPatch | LocString.GetFormattedText | osty 兼容常驻。选非 midori 或原版时也需要恢复游戏名字，保留 HarmonyAfter 和 Last | `Core/CompanionDisplayNameGuard.cs:18–19,36–58,112–140` |

### 2.3 core 中角色专属项及相邻共享项

以下覆盖 core 分类里实际属于角色/随从的项，并把容易误迁的相邻共享项列出。手动安装点继续由原安装器持有，FeaturePatch 当前只登记补丁类，迁移者须先设计安装/卸载配对。依据为 `Core/Features/FeatureRegistry.cs:171–175`、`Core/ManagedSkinModLoader.cs:3564–3582`。

| core 项 | 归属、建议 | 当前源码 |
| --- | --- | --- |
| FrameworkCharacterEnergyColorPatch | 可按需，角色契约提供 EnergyLabelColor，动态枚举 CharacterModel 子类 | `Core/FrameworkSkinRuntimePatches.cs:328–353` |
| FrameworkEnergyLayersPatch | 可按需，角色契约提供 NEnergyCounter 贴图层，依赖玩家角色 | `Core/FrameworkSkinRuntimePatches.cs:77–89,384–419` |
| FrameworkEntryAnimationPatch | 可按需，角色入场动画，目标按完整签名枚举，保留去重及 High 优先级 | `Core/FrameworkSkinRuntimePatches.cs:604–674` |
| FrameworkCardEnergyColorPatch | 共享常驻，角色契约值用于卡池能量描边，和卡牌呈现共用 | `Core/FrameworkSkinRuntimePatches.cs:64–74,356–380` |
| FrameworkOrbDarkenedColorPatch | 共享常驻，角色契约的 Orb 描述驱动球颜色，和充能球呈现/联机球作用域共用 | `Core/FrameworkSkinRuntimePatches.cs:108–123,459–480` |
| FrameworkRelicPackedIconPatch / FrameworkRelicOutlinePatch | 共享常驻，角色契约提供遗物图标，与遗物 UI 共用 | `Core/FrameworkSkinRuntimePatches.cs:484–529` |
| CharacterSkinBundleRunCleanupPatch | 入口常驻，退出恢复此前的卡牌、怪物等预设，与新建/保存/读档三类成套 | `Ui/ContextualSkinControls.cs:3893–3908`、`Core/SkinService.CharacterSkinBundles.cs:32–35,260,449–538` |
| CharSelectTakeoverSuppressor 两处手动安装 | 入口常驻，抑制第三方选人接管并接收独立随从选择通知 | `Core/CharSelectTakeoverSuppressor.cs:10–16,42,58–110` |
| FrameworkNativeUiPatch 两处手动安装 | 角色框架选人 UI 兼容常驻。debug 查询修复保留，AddEntry 的角色选择门控与入场动画共用 | `Core/FrameworkNativeUiPatch.cs:11–26` |
| FrameworkRegistryCooperation 的 Refresh/LoadPreview、前后循环、RefreshBg/RefreshButton/MultiplayerIconRefresh、PlayPreviewSprite、_Ready 手动安装 | 入口常驻，作者原生选择器与 SC 选择双向衔接，含预览和联机头像 | `Core/FrameworkRegistryCooperation.cs:35–75` |
| FrameworkRegistrySession 的 Prefix / Finalizer | 共享常驻，角色 registry 读写和 ResolvePower/Potion/Relic/Orb，一起服务球、药水、遗物等契约 | `Core/FrameworkRegistrySession.cs:151–171` |
| ManagedSkinModLoader.EnsureDirectCharacterSkinPathRouter 手动 Prefix | 角色专属路径路由，但必须随提供者程序集注册存在。由主干加载器持有 | `Core/ManagedSkinModLoader.cs:580–596,1199–1202` |
| ManagedSlotVisibilityBridge.BindPreview 手动 Postfix | 入口常驻，观察选中提供者的部件开关，保存状态后应用到独立实例 | `Core/ManagedSlotVisibilityBridge.cs:12–24,26–57` |
| ProviderMenuGateCompat.PatchGate、PatchOwnSelectionEntryPoints 三处手动安装 | 角色变体及战损的选择门控常驻。运行时提供者的原补丁仍由加载器收窄 | `Core/ProviderMenuGateCompat.cs:10,37,90–99,136–160` |
| ScopedSkinBehavior.Install 手动 Prefix / Postfix / Finalizer | 角色/玩家的 AppliesTo 兼容，主干拥有玩家作用域及安装记录 | `Core/ScopedSkinBehavior.cs:16–29,43–70,82–85` |
| PackHotkeySuppression.Suppress 手动 Prefix | 入口常驻，与 VariantHotkeyPoller 配对，防止作者快捷键与 SC 重复执行 | `Core/VariantHotkeyPoller.cs:42–50,301–339` |
| CachedSceneResultPatch / CachedTextureResultPatch | 共享常驻，按资源路径查任意分组。贴图还写 EventSkinRuntime 元数据 | `Ui/ContextualSkinControls.cs:3266–3319,4056–4069` |

其余 core 行保留全局职责。Mod 加载、Godot 脚本注册、原生框架程序集加载、manifest、退出、ModelDb/本地化、加载顺序提示、模组列表/主题/暂停菜单入口，以及加载器通用的 PatchAll/回调重装，都跨模块。ScopedMonster 路由和 run-environment guards 服务怪物/环境，StatefulExhaustInitializePatch 已属于带状态卡图。本次无须把这些项并入角色或随从。对应原清单为 `docs/refactor/patches.csv:5,33–35,63,66–69,78–80,165–173,217–218,221–226,237`，加载器当前安装点为 `Core/ManagedSkinModLoader.cs:466,478,3363,3531–3582`。

补充一个 CSV 分类之外的必要依赖。`OstyDefectImpostorCompat.Install` 在第三方 PatchAll 解析旧方法名时提供版本兼容，应在提供者加载前常驻。其语义要求覆盖“装了独立奥斯提但当前显示原版”的情况。`OstyImpostorCompat`/`OstyImpostorBridge` 还委托通用 SummonModCompat，未来随从模块不能绕开这套 body owner 判据。依据为 `Core/OstyDefectImpostorCompat.cs:11–18,40–67`、`Core/OstyImpostorCompat.cs:17,34–39`、`Core/OstyImpostorBridge.cs:69–91`、`Core/SummonModCompat.cs:59–89,145–190`。

## 3. 核心耦合与窄接口

### 3.1 SkinService 的依赖

| 领域 | 现有方法、状态与责任 | 模块可调用的边界 |
| --- | --- | --- |
| 选择读取 | GetVisualSelection/GetVisualSelections 允许联机临时选择；Config 持有本地持久选择；CharacterPreviewSelections 独立 | 只读已解析选择和契约，保留原读入口。依据 `Core/SkinService.cs:68,164–175,202–238` |
| 选择提交 | ApplySelection 校验、BuildVisualSelectionTransaction、资源缓存与挂载；随从提前转到独立事务 | facade 提交 group/option，复用完整事务。依据 `Core/SkinService.cs:1908–2007` |
| 角色场景/头像 | SelectionServesCombatScene、InstantiateManagedCharacterCreatureVisuals、ShouldDriveManagedCharacterAnimations、GetOrLoadCharacterIconResource、WithCharacterIconResource | 可向 CharacterFeature 暴露这些现有能力，隐藏 overlay、缓存字典和 Godot 挂载实现。依据 `Core/SkinService.cs:3073–3085,4635–4693,4834–4878` |
| 资源作用域 | BeginRuntimeResourceScope、LoadRuntimeScenes、LoadRuntimeResourcesForSelection、WithRuntimeResourcesForSelection；作用域退出负责恢复 | facade 返回现有 IDisposable，调用方保持原 using/finalizer 配对。依据 `Core/SkinService.cs:4721–4738,5904–6000` |
| 包挂载与恢复 | MountOverlay/MountOverlayCore、MountedLargeRuntimeProviderPacks、PromotedPackOrder、MountedCompanionOnlyProviderPacks；大包提权与全分组重申 | 保留主干单一所有者，Feature 停用交还正常选择/恢复入口。依据 `Core/SkinService.cs:78–87,6364–6433,6968–7061,7121` |
| 提供者激活及收窄 | _runtimeProviderBehaviorScope、RuntimeProviderScopeLeases；Focus/TryFocusRuntimeProviderBehaviorsOnGroups、GetActiveRuntimeProviders；本地与远端选择取并集 | 场景协调器统一调用，角色模块只上报自身可见组/活对象。依据 `Core/SkinService.cs:113–114,1655–1724,7130–7214` |
| 变体及组合 | GetSelectedManualCharacterVariant、IsSameManualVariantSource、角色组合包、随机开局和恢复 sidecar | UI 通过 facade 复用现有提交，暂时保留主干存储和跨分类事务。依据 `Core/SkinService.ManualCharacterVariants.cs:7–55`、`Core/SkinService.CharacterSkinBundles.cs:260–283,449–573`、`Core/SkinService.RandomCharacterSkins.cs:31–111` |
| 随从选择 | CompanionAutoFollow、CompanionSelectionChanged、随机状态、CompanionSelectionDecoupled、CompanionImpostorSkinId；ApplyCompanionSelection/ApplyCompanionGroupSelection | Companion facade 暴露选项/选择/随机/提交/刷新通知。独立事务及存储留在主干，配对使用独立 Mod body owner。依据 `Core/SkinService.cs:8812–8861,8880–8929,8947–9078` |
| 变换 | Get/SetCharacterCombatTransform、基准、wrapper、血条、意图、命中框，覆盖三类实体 | 交由共享外观服务，保留玩家选择保存及变换恢复链。依据 `Core/SkinService.cs:5609–5765`、`Ui/CharacterAppearanceRuntime.cs:640–691,958–983,1106–1128,1988` |

### 3.2 SkinCatalog 的依赖

- 分组与能力识别统一留主干。使用 IsCharacterAppearanceGroup 和 IsCompanionGroup，包含纯资源角色包、第三方角色及受支持的运行时包。CompanionGroups 当前是 necrobinder→osty，IsCompanionOnlyProvider 只看声明，用于下拉过滤。依据 `Catalog/SkinCatalog.cs:18–71,2342–2349`。
- 提供者能力判断包括 ProviderUsesManagedCharacterScene、ManagedGodotScripts、InteractiveRuntime、DirectCharacterRuntime、ScopedMonsterRuntime 和 FullRuntime。模块只消费当前目录快照，目录构建及发现仍留主干。依据 `Catalog/SkinCatalog.cs:2037–2105,2300`。
- BuildVisualSelectionTransaction 有完整提供者/共享骨架联动及自动跟随逻辑。BuildCompanionSelectionUpdates 由 CompanionAutoFollow 决定，手动随从选择走独立提交。仅用于行为启用的 GetFullySelectedFullRuntimeProvidersIgnoringCompanions 不得代替严格选项/事务规则。依据 `Catalog/SkinCatalog.cs:2373–2401,2475–2553`、`Core/SkinService.cs:1930–1933,8994–9026`。
- IsCompanionOnlyProviderSelection 根据实际选择判定，覆盖“提供者运行时组表只声明主人却带随从资源”的包。该判断同时参与 DLL 行为排除与 PCK 低优先级挂载，必须共用。依据 `Catalog/SkinCatalog.cs:2317–2339`、`Core/SkinService.cs:6998–7006,7164–7181`。
- BuildOverlay、BuildBaselineDependencyOverlay、BuildGameBaselineOverlay、BuildRuntimeResourceOverlay 同时负责选中资源、旧资源恢复和别名依赖。角色、随从 facade 返回准备好的资源/作用域，避免模块自行拼包或另存 baseline。依据 `Catalog/SkinCatalog.cs:3011,4768,4812,4851`、`Core/SkinService.cs:6433,7031–7049`。
- ResolveVisualProviderId 和 ResolveStoredVisualSelectionId 是 id/旧配置解析入口，变体展示选项与行为程序集身份各有解析需求，应保留现有分别处理的链路。依据 `Catalog/SkinCatalog.cs:2593–2633`、`Core/ManagedSkinModLoader.cs:1090–1105`。

### 3.3 ManagedSkinModLoader 与联机

- ProvidersByIdentity、ProviderAssemblies、ProviderRuntimeBlueprints、ProviderIdsByAssembly、激活运行记录和节点恢复属于同一个加载器生命周期。ActivateProvider 会加载程序集、建路由和运行初始化，DeactivateProvider 分步恢复节点/呈现、卸行为、停控制器/节点。保留主干所有权。依据 `Core/ManagedSkinModLoader.cs:41–86,1168–1205,1527–1558`。
- 角色特有的路由、呈现捕获/恢复、CreatureReady/CompanionReveal 重放可提供窄调用，具体提供者补丁分类与安装保留加载器。模块传 providerId 与对象，加载器判定执行者，保持原本重放次数。依据 `Core/ManagedSkinModLoader.cs:588,1810–1881,1998,2120,2250,2429,3564–3582`。
- DeactivateProvidersExcept 对选择集合与活动 key 两侧归一化，并保留已选商人。角色模块卸载时须经全局协调器重算活跃集合。依据 `Core/ManagedSkinModLoader.cs:1090–1125`。
- 联机按玩家完整选择映射，宠物用 PetOwner。BeginCreatureRuntimeScope 组合选择作用域和资源作用域，头像、火堆、球的上下文仍由原联机补丁建立。局内给队友手动换肤使用本地 fallback 记录。依据 `Core/MultiplayerSkinSync.cs:213–230,293–310,439–466,518–594`、`Ui/ContextualSkinControls.cs:3957–3962,4128–4133`。

### 3.4 建议 facade 的最小形状

以下是后续设计边界，成员优先转调同名现有方法，迁移时按实际调用需要添加。

| 边界 | 暴露能力 | 主干保留 |
| --- | --- | --- |
| CharacterFeatureApi | 角色能力快照、已解析契约、场景创建、头像资源访问、动画是否由 SC 驱动 | 选择事务、缓存字典、overlay、提供者加载。对应 `Core/SkinService.cs:202–238,3073,4655–4693,4834–4878` |
| CompanionFeatureApi | 随从选项/当前选择/随机、ApplyCompanionSelection、body owner 判据、预览资源和选择通知 | 解耦标记、独立最小事务、独立 Mod 适配及包恢复。对应 `Core/SkinService.cs:8838–8929,8986–9078`、`Core/SummonModCompat.cs:59–89,113–179` |
| 现有共享入口 | Focus/TryFocusRuntimeProviderBehaviorsOnGroups、BeginCreatureRuntimeScope、共享变换与存读档入口 | 单一场景范围、玩家上下文、配置存储。对应 `Core/SkinService.cs:1655–1724`、`Core/MultiplayerSkinSync.cs:518–594`、`Core/SkinService.CharacterSkinBundleRuns.cs:173–212` |

## 4. ShouldBeActive 的实际收益

### 4.1 角色

- 对持续安装角色皮肤的玩家，按“目录有可用角色换肤能力”判断，角色 Feature 会长期启用。可兑现的收益是无角色皮肤的安装组合可卸 16 个专属呈现补丁，以及模块级熔断和归属可维护性。第三方 DLL 的场景收窄已有独立实现。依据 `Catalog/SkinCatalog.cs:2342–2349`、`Core/SkinService.cs:1655–1711`、`Core/Features/FeatureRegistry.cs:79–88`。
- 第一版建议目录未就绪返回 null。目录就绪时从 IsCharacterAppearanceGroup 的非 base 选项构建轻量能力快照，同时纳入声明契约、纯资源、场景型和运行时选项。使用目录中已解析数据，避免只检查 IsRuntimeProvider 或文件夹。依据 `Core/Features/ISkinFeature.cs:11–12,26–29`、`Catalog/SkinCatalog.cs:2342–2349,2037–2105`。
- 第一版使用“可用能力”激活即可。若日后改成“当前选择”，须同时覆盖角色预览、本地选择、联机 available maps、随机/组合包及保留的变体状态，并在刷新对象前同步 Reevaluate。现有轮询间隔 2 秒，单靠轮询无法保证切肤入口立即有补丁。依据 `Core/SkinService.cs:68,164–175,7132–7157`、`Core/Features/FeatureBootstrap.cs:24,70–81`。
- IsInUse 首版可保守使用活 NRun、选人屏幕/模型预览、外观界面和执行中的重建。活 NRun 也覆盖仍需要缺头 VFX 守卫的节点，先保证生命周期。将来细分 live 对象计数才可能在局内卸载。依据 `Core/Features/ISkinFeature.cs:12,29`、`Core/Features/FeatureRegistry.cs:85–88`、`Ui/CharacterAppearanceRuntime.cs:2585–2606`、`Core/OptionalCharacterVfx.cs:9–35`。

### 4.2 随从

- 当前仅 OstyDisplayNameGuardPatch 可单独登记，原版选择也需要它，所以先保持兼容常驻有实际依据。后续若形成独立的随从呈现补丁集，再让该集合使用随从能力/选择快照启停。依据 `Core/CompanionDisplayNameGuard.cs:18–19,48–58,112–124`。
- 需要区分 Config 的 osty=base 与独立随从 Mod 正在拥有 body。ApplyCompanionSelection 选独立 Mod 时会把 osty 组设为 base，同时保存 CompanionImpostorSkinId，故仅判“osty 非 base”会误判。依据 `Core/SkinService.cs:8994–9010`、`Core/SummonModCompat.cs:59–89`。
- 对随从更有收益的现有路径是“仅随从选中时保留私有资源，排除角色级 DLL 行为”，以及按当前场景提供者范围执行。Godot 已挂载 PCK 保留在栈中，恢复通过校正包完成。依据 `Core/SkinService.cs:7031–7049,7164–7181`。

## 5. 迁移分步及验收

每步单独交付，下一步沿用已验收的边界。下表中的实机项目为后续执行项，本次勘察未运行游戏。

| 步骤 | 文件与工作 | 执行分工 | 正常使用实机验收与依据 |
| --- | --- | --- | --- |
| 1. 定角色能力及 facade | 新增 `Core/Features/Character/CharacterFeature.cs`、策略文件和需要的 CharacterFeatureApi；核心只加窄入口；`FeatureBootstrap.cs` 注册 | Claude/GPT 设计，Claude 持有 SkinService/Catalog/Loader 和注册表修改 | 无角色皮肤启动、有纯资源包启动、原皮与皮肤互切、选人入口始终可用。策略须覆盖 `Catalog/SkinCatalog.cs:2342–2349`，及时注册依据 `Core/Features/FeatureBootstrap.cs:28–36,70–81` |
| 2. 迁 16 个角色专属类 | `FrameworkSkinRuntimePatches.cs` 5 类、`ManagedCharacterAnimationBridge.cs` 2 类、`OptionalCharacterVfx.cs` 2 类、`ContextualSkinControls.cs` 7 类；按第 2 节“可按需”逐类加 FeaturePatch/匹配 Safe 键，保持方法体和优先级 | 肥鱼按明确清单和已定模板执行，Claude/GPT 复核安装列表与目标枚举 | 普通皮肤/框架皮肤/第三方角色切换，战斗入场、受伤/死亡动画，骨妹缺头场景，选人头像、局内头像和地图标记，退出重进。特别保留 `Core/FrameworkSkinRuntimePatches.cs:242–249,614–633` 的场景标记和目标去重 |
| 3. 处理共享归属 | `CharacterAppearanceRuntime.cs`、`CreatureIntentDepth.cs`、`ContextualSkinControls.cs` 的共享行保持主干；先明确怪物缩放/意图与全局外观编辑的 Safe 键和模块归属 | Claude/GPT 必须设计，肥鱼可在清单确定后机械改键/注解 | 角色与怪物分别缩放/移动、血条/意图/命中框、重置、SL；只装怪物皮肤也有效。依据 `Ui/CharacterAppearanceRuntime.cs:958–983,1988`、`Ui/CreatureIntentDepth.cs:40–50` |
| 4. 建随从 facade | `SkinService.cs:8812–9078`、`Catalog/SkinCatalog.cs:18–71,2317–2339,2533–2553`、`OstyImpostorCompat/Bridge`、`SummonModCompat`、`OstyModelPreviewControls.cs`；先搬调用边界及命名归属，保留现有最小事务 | Claude/GPT 处理核心接口，肥鱼整理调用清单/文档 | 主人 A + 随从 B，主人改 C 后随从保持 B，随从选原版、独立鸡煲、midori 帧动画，随机开局，其他角色隐藏预览但随从选择保留。依据 `Core/SkinService.cs:8994–9078`、`Ui/OstyModelPreviewControls.cs:24–31,78,135` |
| 5. 设计兼容安装生命周期 | `ManagedCharacterAssetRegistrationGuard`、`ManualCharacterVariantBridge`、`ManagedSlotVisibilityBridge`、`ProviderMenuGateCompat`、`VariantHotkeyPoller`、`ScopedSkinBehavior`、`FrameworkRegistryCooperation/Session`、`ManagedSkinModLoader`；只在能成对恢复现有手动安装记录时再迁 | Claude/GPT 必须设计。肥鱼可按最终安装/卸载对照表机械搬动 | 十字军场景变体、贞德设置型变体/双来源、快捷键、作者原生 UI、战损点击，战斗→商人→火堆→SL，联机同角色不同皮肤/随从及远端球。依据 `Core/ManualCharacterVariantBridge.cs:19–38,49–65`、`Core/ManagedSkinModLoader.cs:1090–1125,1527–1558`、`Core/MultiplayerSkinSync.cs:518–594` |

步骤 2 的准确数量为 FrameworkCombatVisual/Trail/CharacterEnergyColor/EnergyLayers/EntryAnimation 共 5 类，ManagedCharacterAnimation 两类，OptionalHead 两类，CharacterVisualResult 加六个头像/标记类共 7 类，总计 16。其余清单继续保留原安装路径，避免 Entry.PatchClass 与 Feature 重复安装。依据 `Core/Features/FeatureBootstrap.cs:48–56`、`Core/Features/FeatureRegistry.cs:171–175`、`Entry.cs:103`。

步骤 1 为能力策略补纯逻辑测试，至少覆盖目录未就绪、仅 base、纯资源角色包、框架/运行时包和仅随从包。步骤 4 为主人/随从独立选择及 base+独立 body owner 补针对性回归测试。构建与 LogicTests 显式传本机 GameAssemblyDir，构建 `-o` 输出临时目录。现有自动测试入口为 `tests/STS2SkinChanger.LogicTests/STS2SkinChanger.LogicTests.csproj`，策略语义来源为 `Catalog/SkinCatalog.cs:18–71,2317–2349`、`Core/SkinService.cs:8994–9026`。

## 6. 已知回归点

| 回归风险 | 迁移时保留的约束 | 来源 |
| --- | --- | --- |
| 把 EmptyNCreatureVisuals 误当失败，绕过正确场景工厂 | 保留原场景工厂、转换及 managed 标记，采用作者已确认的画面基准 | 作者报告 `D:\mod\作者仓库\STS2SkinChanger_问题与修复对照.md:69–70`、`Core/FrameworkSkinRuntimePatches.cs:180–185,242–249` |
| `::variant:` / `::source:` 使动画或战损补丁被收窄卸掉 | 沿用身份解析及两侧 NormalizeProviderIdentity | 作者报告 `:25–26,52,71`、`Core/ManagedSkinModLoader.cs:1090–1112` |
| 第三方前缀和 SC 重放各执行一次，造成重复位姿/动画 | 保留单一最终写入者、现有提供者回调筛选与恢复 | 作者报告 `:39–42,72`、`Ui/ContextualSkinControls.cs:3957–3962`、`Core/ManagedSkinModLoader.cs:1527–1558` |
| NRun 子节点 Ready 时机、单帧延迟恢复丢失 | 保留显式 AttachToRun 和现有 tick/重建链，按实际异步所有者处理 generation | 作者报告 `:73`、`Ui/CharacterAppearanceRuntime.cs:2593–2606` |
| 缺失资源进入原生 Spine 渲染导致黑块/崩溃 | 保留资源能力检查及现有失败回退，切肤路径验证含选择场景与战斗场景 | 作者报告 `:51,74`、`Core/FrameworkSkinRuntimePatches.cs:199–216` |
| 随从专用选项出现在主人下拉、预览泄漏到所有角色 | 保留声明级过滤和预览可见门控 | 作者报告 `:21–23`、`Catalog/SkinCatalog.cs:35–64`、`Ui/OstyModelPreviewControls.cs:28–31` |
| 随从包盖住主人、提权过的旧 PCK 留栈 | 保留 companion-only 低优先级挂载、全分组校正和角色级 DLL 排除 | `docs/issues/ISSUE_2026-11-10_osty_pack_leak_and_reticle_transform.md:11–41,87–105`、`Core/SkinService.cs:7031–7049,7164–7181` |
| 原版恢复取到 midori 壳，独立鸡煲仍显示猫 | 游戏原包 baseline 与通用 baseline 各走现有正确用途，保留 body owner 判据 | `docs/issues/ISSUE_2026-11-11_midori_osty_preview_and_impostor_override.md:17–18,66–86`、`Catalog/SkinCatalog.cs:4812` |
| 只支持 Spine，帧动画随从预览退回原版；含空格资源依赖丢失 | 保留场景型预览与完整依赖闭包 | 同上问题文档 `:17,88–107,120`、`Ui/OstyModelPreviewControls.cs:135` |
| 贞德设置型变体被拼成不存在的商人/火堆场景 | 保留 ProviderVariantOptions 与 ProviderSettingsVariantOptions 的不同语义 | `docs/issues/ISSUE_2026-11-07_variant_rest_site_crash.md:21–42`、`Ui/ProviderVariantPathPatch.cs:12–35` |
| 变体更新只改配置，预览/背景/模型仍旧色；双来源执行者混淆 | 保留资源重挂与家族缓存各自职责、行为执行者映射和手动作用域恢复 | `docs/issues/ISSUE_2026-11-09_joan_dual_pack_variant_isolation.md:466–484`、`Core/SkinService.cs:7199–7214`、`Core/ManualCharacterVariantBridge.cs:19–38` |
| 联机改队友影响本机，同角色玩家共享模型级选择 | 保留按 playerNetId 的完整选择映射、手动 fallback、PetOwner 与作用域恢复 | `docs/issues/ISSUE_2026-11-07_multiplayer_skin_independence.md:9–14,18–38`、`Core/MultiplayerSkinSync.cs:518–568` |
| 远端火堆/球拿到预加载的原版缓存 | 继续保留联机资源及缓存租约上下文，角色 Feature 只消费上下文 | `docs/issues/ISSUE_2026-11-07_remote_rest_site_vanilla.md:34–42`、`docs/issues/ISSUE_2026-11-07_remote_orb_skin.md:23–42` |
| 缩放后血条/意图/选择框错位，重置数值丢失 | 共享变换服务保持原基准、GlobalPosition、中心缩放和排队后应用 | `docs/issues/ISSUE_2026-11-10_osty_pack_leak_and_reticle_transform.md:43–72`、`Ui/CharacterAppearanceRuntime.cs:958–983,1988` |
| 外部骨架加载器与皮肤同时拥有骨架/图集 | 继续走外部所有权判据及现有兼容分支，覆盖角色/怪物/UI | `docs/issues/ISSUE_2026-11-07_custom_skeleton_loader_compat.md:13–33`、`Ui/ContextualSkinControls.cs:3275–3278,3304–3307` |
| 第三方 GenerateAnimator 重载导致整个 PatchAll 失败 | 保留参数/返回类型过滤和按真正方法体去重 | `Core/FrameworkSkinRuntimePatches.cs:614–633` |
| 独立奥斯提包因游戏方法改名而自卸全部补丁 | 加载前保留 OstyScaleToSize→ScaleAndPositionOsty 的版本兼容 | 作者报告 `:55`、`Core/OstyDefectImpostorCompat.cs:11–18,40–67` |

问题文档中的“待实机验证”“未完成”和回退历史仅用作回归场景来源。本次依据当前源码确定所有权，迁移后的画面状态由第 5 节逐步实测确认。回退或变更既有用户行为前应先确认作者意图，来源为作者报告第 75 行。

## 可选、不阻塞

- 以后若细分 IsInUse，可增加角色/随从活对象计数，现阶段先复用保守的活对局/预览判据，依据 `Core/Features/ISkinFeature.cs:12,29`。
- 手动安装桥将来可统一提供可释放安装句柄，先保留现有安装记录与安装时机，依据 `Core/ManualCharacterVariantBridge.cs:13–16,64–65`。
