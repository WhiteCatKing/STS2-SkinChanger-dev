# 常驻补丁清单（DSH，2026-11-15）

1. 这些补丁不进 `FeaturePatch`，**不要改成按需安装**：它们服务角色、随从、怪物和多个界面共用的生命周期。
2. 改方法体之前，先看「碰了会坏什么」那一列；空白表示勘察第 6 节没有对应到具体回归点。
3. 新加的常驻补丁，在这里登记一行，并在类（或安装方法）上方贴同样格式的标签：`// 常驻·类型：谁在用 —— 干什么。详见 docs/refactor/resident-patches.md`。

共 **45** 处（入口 16、共享 24、兼容 5）。行号是标签所在行；源码位置相对 `STS2SkinChanger/`。

| 类 | 文件:行号 | 类型 | 谁在用 | 干什么 | 熔断键 | 为什么必须常驻 | 碰了会坏什么 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `GameCommonPreloadCompletePatch` | `Core/SkinPreloader.cs:327` | 入口 | 游戏启动预加载 | 观察 Common 完成，放行皮肤预加载；识别失败时放行 | —（内部 catch 降级） | Common 属于全局加载会话，不随任一皮肤模块停用 | 过早派发会和游戏启动加载竞争；未放行则由 60 秒兜底接续 |
| `CharSelectTakeoverSuppressor.Install` | `Core/CharSelectTakeoverSuppressor.cs:54` | 入口 | 选人界面 | 抑制第三方选人接管并接收独立随从选择通知 | — | 第三方接管和随从通知都在选人阶段 |  |
| `FrameworkRegistryCooperation.Bind` | `Core/FrameworkRegistryCooperation.cs:35` | 入口 | 作者原生选择器 | 让作者原生选择器和我们的选择器双向衔接，含预览和联机头像 | — | 原生选择器和 SC 选择器必须一起工作 | 变体更新只改配置，预览/背景/模型仍旧色；双来源执行者混淆 |
| `ManagedSlotVisibilityBridge.BindPreview` | `Core/ManagedSlotVisibilityBridge.cs:26` | 入口 | 选择器预览 | 观察选中提供者的部件开关并应用到独立实例 | — | 预览要用提供者自己的部件开关 |  |
| `ManualCharacterVariantBridge.BindScreen` | `Core/ManualCharacterVariantBridge.cs:52` | 入口 | 选人界面 | 观察提供者自己的变体 Toggle 并保存主动选择 | — | 按玩家渲染的作用域要靠它恢复临时设置 | 变体更新只改配置，预览/背景/模型仍旧色；双来源执行者混淆 |
| `CharacterSkinBundleNewRunBindingPatch` | `Core/SkinService.CharacterSkinBundleRuns.cs:173` | 入口 | 选人界面开局 | 记录本局的随机角色与组合包预设 | — | 开局还涉及别的分类的预设，不能只按角色包判断 |  |
| `CharacterSkinBundleSavedRunPatch` | `Core/SkinService.CharacterSkinBundleRuns.cs:187` | 入口 | 读档 | 恢复旧存档里的组合包记录 | — | 当前目录没有角色皮肤时，旧存档也可能带着记录 |  |
| `CharacterSkinBundleRunSavePatch` | `Core/SkinService.CharacterSkinBundleRuns.cs:208` | 入口 | 存档 | 把本局的组合包预设写进存档 | — | 与新建、读档两类成套，缺一个就对不上 |  |
| `PackHotkeySuppression.Suppress` | `Core/VariantHotkeyPoller.cs:301` | 入口 | 快捷键 | 抑制提供者自己的快捷键，避免和我们的重复执行 | — | 和 VariantHotkeyPoller 配对，缺一半会双执行 |  |
| `CharSelectStageCyclePatch` | `Ui/CharSelectStageCycle.cs:726` | 入口 | 选人界面 | 挂战损/阶段切换控件 | `patch:character` | 选择界面的阶段预览入口 |  |
| `CharacterAppearancePauseMenuPatch` | `Ui/CharacterAppearanceScreen.cs:2801` | 入口 | 局内外观界面 | 在暂停菜单里挂外观面板入口 | `patch:character` | 面板同时编辑角色、怪物和随从 |  |
| `RandomCharacterLobbySkinPatch` | `Ui/ContextualSkinControls.cs:3809` | 入口 | 选人界面 / 大厅 | 返回大厅时恢复随机选择的基准 | — | 大厅往返还靠它保持随机选择 |  |
| `CharacterSelectionSkinPatch` | `Ui/ContextualSkinControls.cs:3824` | 入口 | 选人界面 | 捕获布局并把皮肤选择器挂上去 | `patch:character` | 所有角色外观选择的入口 |  |
| `SingleplayerEmbarkSkinSelectorPatch` | `Ui/ContextualSkinControls.cs:3835` | 入口 | 单人开局 | 开局时应用组合包、处理随机角色并收起选择器 | — | 开局钩子，缺了组合包不生效 |  |
| `MultiplayerEmbarkSkinSelectorPatch` | `Ui/ContextualSkinControls.cs:3866` | 入口 | 多人开局 | 按本地大厅角色应用组合包 | — | 多人开局各自按本地角色应用 |  |
| `CharacterSkinBundleRunCleanupPatch` | `Ui/ContextualSkinControls.cs:3897` | 入口 | 退出对局 | 退出时恢复此前的卡牌、怪物等预设 | — | 和新建、保存、读档三类成套 |  |
| `CreatureVisualParentBridge.Register` | `Core/CreatureVisualParentBridge.cs:20` | 共享 | 角色 / 怪物 / 随从 | 转译第三方对 NCreature 父节点的查询 | — | 变换包装要跨三类生物，还要保留自有 Harmony 的生命周期 |  |
| `FrameworkRegistrySession.Install` | `Core/FrameworkRegistrySession.cs:144` | 共享 | 角色 registry | 接 registry 的读写与 Resolve（能力/药水/遗物/球） | — | 球、药水、遗物等契约都走它 |  |
| `FrameworkCardEnergyColorPatch` | `Core/FrameworkSkinRuntimePatches.cs:359` | 共享 | 卡池能量描边 | 用角色契约值画卡池能量描边 | — | 和卡牌呈现共用同一个契约值 |  |
| `FrameworkOrbDarkenedColorPatch` | `Core/FrameworkSkinRuntimePatches.cs:464` | 共享 | 充能球 / 联机球作用域 | 用角色契约的 Orb 描述驱动球颜色 | — | 和充能球呈现、联机球作用域共用 |  |
| `FrameworkRelicPackedIconPatch` | `Core/FrameworkSkinRuntimePatches.cs:490` | 共享 | 遗物 UI | 把角色契约提供的遗物图标接进遗物 UI | — | 和遗物 UI 共用同一份图标 |  |
| `FrameworkRelicOutlinePatch` | `Core/FrameworkSkinRuntimePatches.cs:524` | 共享 | 遗物 UI | 把角色契约提供的遗物描边接进遗物 UI | — | 和遗物 UI 共用同一份描边 |  |
| `ManagedCharacterAssetRegistrationGuard.EnsureFrameworkPatchesInstalled` | `Core/ManagedCharacterAssetRegistrationGuard.cs:67` | 共享 | 角色契约 / UI | 在框架角色资源注册入口装守卫补丁 | — | 角色选择会同时控制战斗、商人、火堆、UI 路径 |  |
| `ManagedSkinModLoader.EnsureDirectCharacterSkinPathRouter` | `Core/ManagedSkinModLoader.cs:568` | 共享 | 角色运行时提供者 | 按角色选择分发 Spine 外观路径 | — | 随提供者程序集注册常驻，避免同包里未被选中的角色被污染 |  |
| `ScopedSkinBehavior.Install` | `Core/ScopedSkinBehavior.cs:54` | 共享 | 角色 / 玩家 | 处理提供者 AppliesTo 的玩家作用域兼容 | — | 玩家作用域和安装记录由主干持有 |  |
| `InRunCharacterAppearanceRuntimePatch` | `Ui/CharacterAppearanceRuntime.cs:2585` | 共享 | 角色 / 随从 / 怪物 / 局内界面 | 挂局内外观运行时节点并接联机同步 | — | 联机同步和提供者范围都由它建立，怪物和局内界面也依赖 | NRun 子节点 Ready 时机、单帧延迟恢复丢失 |
| `TopBarRoomIconSkinTexturePatch` | `Ui/CharacterAppearanceRuntime.cs:2611` | 共享 | Boss / 房间图标 | 接管顶栏房间图标的贴图加载 | — | 和怪物地图呈现共用同一个压缩贴图入口 |  |
| `BossMapPointSkinPresentationPatch` | `Ui/CharacterAppearanceRuntime.cs:2633` | 共享 | 怪物 / Boss 提供者 | 地图 Boss 点的皮肤呈现 | — | 目前主要服务怪物提供者回调，归属以后再理 |  |
| `CharacterAppearanceCreatureReadyPatch` | `Ui/CharacterAppearanceRuntime.cs:2643` | 共享 | 角色 / 随从 / 怪物 | 生物就绪时记录视觉缩放基准并套变换外壳 | — | 三类生物共用同一套基准与变换包装 | 缩放后血条/意图/选择框错位，重置数值丢失 |
| `CharacterAppearancePlayerLayoutPatch` | `Ui/CharacterAppearanceRuntime.cs:2652` | 共享 | 角色与随从 | 记录原生排布快照 | — | 角色和随从的排布共用一份快照 |  |
| `CombatRoomRuntimeProviderScopePatch` | `Ui/CharacterAppearanceRuntime.cs:2661` | 共享 | 角色 / 宠物 / 怪物 | 进战斗房时扫描双方并建运行时范围 | — | 扫描覆盖 Allies 和 Enemies 两侧 |  |
| `CombatRoomRuntimeProviderScopeExitPatch` | `Ui/CharacterAppearanceRuntime.cs:2671` | 共享 | 角色 / 宠物 / 怪物 | 离开战斗房时收尾同一份范围 | — | 和上一项共用场景生命周期，必须成对 |  |
| `CharacterAppearanceBoundsPatch` | `Ui/CharacterAppearanceRuntime.cs:2680` | 共享 | 角色 / 随从 / 怪物 | 按变换更新命中框，含纯怪物缩放分支 | — | 命中框跟着共享变换走，含怪物缩放 | 缩放后血条/意图/选择框错位，重置数值丢失 |
| `CharacterAppearanceOrbPositionPatch` | `Ui/CharacterAppearanceRuntime.cs:2694` | 共享 | 角色呈现 / 本体外观编辑 | 跟随生物设置充能球位置 | — | 角色呈现和本体外观编辑都用它，先随变换服务保留 |  |
| `MonsterVisualScalePatch` | `Ui/ContextualSkinControls.cs:4055` | 共享 | 怪物 / 随从 | 按缩放分组记录恢复怪物缩放；熔断键是 patch:character 属于历史原因 | `patch:character` | 角色功能停用后，怪物和随从仍要恢复各自节点的皮肤缩放 | 缩放后血条/意图/选择框错位，重置数值丢失 |
| `CachedSceneResultPatch` | `Ui/ContextualSkinControls.cs:4063` | 共享 | 任意分组 | 按资源路径查任意分组的场景缓存 | — | 按路径查表，服务所有分组 | 外部骨架加载器与皮肤同时拥有骨架/图集 |
| `CachedTextureResultPatch` | `Ui/ContextualSkinControls.cs:4072` | 共享 | 任意分组 | 按资源路径查任意分组的贴图缓存 | — | 同上，还会写事件模块用到的元数据 | 外部骨架加载器与皮肤同时拥有骨架/图集 |
| `CreatureIntentDepthPatch` | `Ui/CreatureIntentDepth.cs:151` | 共享 | 怪物 / 有意图的随从 | 调整意图图标的显示层级 | `patch:character` | 主体只处理 IsMonster，但随从也有意图 |  |
| `ProviderMerchantVariantPathPatch` | `Ui/ProviderVariantPathPatch.cs:6` | 共享 | 角色变体 / 商人房 | 解析角色变体在商人房用的场景路径 | — | 和商人场景、预加载共用同一套路径 | 贞德设置型变体被拼成不存在的商人/火堆场景 |
| `ProviderRestSiteVariantPathPatch` | `Ui/ProviderVariantPathPatch.cs:26` | 共享 | 角色变体 / 火堆 | 解析角色变体在火堆用的场景路径 | — | 和联机房间的资源作用域共用 | 贞德设置型变体被拼成不存在的商人/火堆场景 |
| `OstyDisplayNameGuardPatch` | `Core/CompanionDisplayNameGuard.cs:128` | 兼容 | 随从名字 | 把随从名字恢复成游戏原名（奥斯提） | — | 选非 midori 或原版时也要恢复名字，保留 HarmonyAfter 和 Last |  |
| `FrameworkNativeUiPatch.Install` | `Core/FrameworkNativeUiPatch.cs:11` | 兼容 | 角色框架选人 UI | 修 debug 查询并给框架 AddEntry 加角色选择门控 | — | 和角色入场动画共用同一段 UI 逻辑 |  |
| `OstyDefectImpostorCompat.Install` | `Core/OstyDefectImpostorCompat.cs:35` | 兼容 | 独立奥斯提皮肤包 | 为第三方补丁提供旧方法名兼容 | — | 必须在提供者加载前装好，否则对方 PatchAll 失败会自卸全部补丁 | 独立奥斯提包因游戏方法改名而自卸全部补丁 |
| `ProviderMenuGateCompat.PatchGate` | `Core/ProviderMenuGateCompat.cs:77` | 兼容 | 角色变体 / 战损 | 给提供者的菜单门控打补丁 | — | 提供者的选择门控要按变体和战损分别处理 |  |
| `ProviderMenuGateCompat.PatchOwnSelectionEntryPoints` | `Core/ProviderMenuGateCompat.cs:122` | 兼容 | 角色变体 / 战损 | 给提供者自己的选择入口打补丁 | — | 同上，入口不止一处 |  |

## 说明

- 「熔断键」— 表示这个补丁体没有包 `Safe.Run`，异常会按原来的方式处理（多数是 `ref __result` 之类的形态，包不了 lambda）。
- `MonsterVisualScalePatch` 的熔断键是 `patch:character` 属于历史原因（它服务怪物和随从，键是早期定的），改动时别被键名误导。
- 手动安装的桥（没有 `[HarmonyPatch]` 特性）标签贴在负责安装的方法上方，清单里写成 `类.方法`。
- `CharacterAppearanceOrbPositionPatch` 勘察第 2.2 节没有直接标常驻，按其「先随常驻变换服务保留，另行拆分」记为共享。
- `ManagedSkinModLoader.EnsureDirectCharacterSkinPathRouter` 原勘察只写「由主干加载器持有」；GPT 复核（#74）确认它按角色选择分发 Spine 外观路径，已补为共享常驻并入表。
- 勘察第 91 行列出的全局 core 补丁（Mod 加载、本地化、模组列表等）按任务书不在本批范围。
