# 《杀戮尖塔 2》皮肤切换器（Skin Changer）接手分析报告

- 报告日期：2026-11-02
- 分析对象（旧版，游戏内已安装）：`D:\steam\steamapps\workshop\content\2868840\3787302680\Gurio.SkinChanger.dll`
- 分析对象（新版，你提供的 Git 源码）：`D:\STS2-SkinChanger`
- 游戏日志：`C:\Users\Administrator\AppData\Roaming\SlayTheSpire2\logs\godot.log`
- 游戏本体：`D:\steam\steamapps\common\Slay the Spire 2`，当前为公开测试版 **v0.111.0**（`release_info.json` 的 commit `41cef1ea`）
- 源文件保护：全部分析、构建、测试均在临时副本 `D:\STS2_Work\src` 中进行；原目录 `D:\STS2-SkinChanger` 的 501 个文件在分析前后 SHA-256 完全一致，未改动任何一个字节。

---

## 0. 结论摘要

1. **这个 Mod 不是“替换几张图”的简单包**。它在游戏启动早期接管 Mod 加载：把皮肤 Mod 的 PCK/DLL 先隔离起来，只读取元数据建立“皮肤目录”，玩家选中某个皮肤时再动态生成一个 Godot 资源覆盖包（PCK），用 `ProjectSettings.LoadResourcePack(..., replaceFiles: true)` 挂到资源栈顶；切换时重建覆盖包，从而做到不重启即时换肤。完整外观包还会临时加载作者 DLL、执行其视觉初始化器、按需启用 Harmony 行为补丁，切走后卸载。
2. **新版不是重写，而是 1.0.19.0 → 1.0.19.11 的连续修复**。反编译对比显示公开类型只多了 2 个，25 个源码文件有文本差异，其中约 16 个是实质改动，集中在：逐类安装补丁、共享骨骼怪物分组、先古优先级、商店资源预读、局内外观入口恢复、加载顺序尊重、覆盖包重建防重入、诊断日志。
3. **在 v0.111.0 上可以正确构建和通过全部离线测试**：`dotnet build -c Release` 0 警告 0 错误；LogicTests 全部通过；RuntimeTests 默认全套 92 行输出、退出码 0；旧版日志中没有 Skin Changer 的异常或报错。
4. **但有两处新逻辑值得在接手后优先复核**（详见第 5 节）：
   - **F35 加载顺序尊重策略的“记忆”可能永远不会被写入**：首次自动排序后保存的是空列表，玩家下次手动上移仍会被再次压回，修复可能实际无效。
   - **先古皮肤优先级在第一次打开“其它图鉴 → 先古”时会自动把每个先古切换成该组第一个可用皮肤**，并写入配置；这不是用户主动选择，属于行为风险。
5. **未验证项**：本机只有 v0.111.0，无法对正式版 v0.107.1 做编译/运行验证；也没有启动游戏做实机画面测试。发布正式版前必须在 v0.107.1 程序集上重新构建并跑测试。

---

## 1. 分析对象、版本与方法

### 1.1 版本对照

| 项目 | 旧版（创意工坊已安装） | 新版（Git 源码） |
|---|---|---|
| 公开版本 `version` | 1.0.19 | 1.0.19 |
| 四段内测版本 `internal_test_version` | 1.0.19.0 | 1.0.19.11 |
| 程序集版本 | 1.0.19.0 | 1.0.19.11 |
| DLL 大小 | 2,914,304 字节 | 构建产物 2,938,880 字节 |
| DLL SHA-256 | `67ADECB61C33D9140DC903FF8EAF86208832405F42A7E94FF79CB508EF42F7AD` | `199DE99DFC8771D1B2FEAF677112FBC9E1440DF9EFEBB577FBC6358738CDB709` |
| 随包兼容 DLL | `thunninoiSkinManager.dll` 17,408 字节 | 16,896 字节（API 完全一致，仅编译器重建） |
| 文件时间 | 2026-09-18 | 2026-11-01（文档更新至 11-02） |

游戏要求 DLL 文件名与 Mod ID 一致，所以发布物必须是 `Gurio.SkinChanger.dll`；清单是 `SkinChanger.json`，兼容层是 `thunninoiSkinManager.dll`，共三件。

### 1.2 使用的方法

- 用 ILSpy 9.0 把旧 DLL 和新构建 DLL 各自反编译成 C# 工程，逐文件、逐方法对比。
- 用自写的 `System.Reflection.Metadata` 工具导出两个程序集的类型/方法签名，做集合差异。
- 在源码副本上构建，并运行项目自带的两套离线测试。
- 阅读 README、AGENTS、PRODUCT、`docs/COMPLETED_FIXED.md`、`docs/WORKSHOP_FEEDBACK_2026-08-30.md`、`docs/FEATURE_REQUESTS_PENDING.md` 及相关设计文档。
- 检查游戏日志中的 Mod 初始化、皮肤提供者识别、警告、异常与退出清理。

---

## 2. 旧版 Mod 的运行原理（完整链路）

### 2.1 游戏侧加载机制

《杀戮尖塔 2》的 Mod 加载器会：

1. 递归扫描 `mods/` 和 Steam 创意工坊目录，读取每个 `*.json` 清单；
2. 检查 `id`、`version`、`min_game_version`、依赖；
3. 用 `AssemblyLoadContext.LoadFromAssemblyPath` 加载 `has_dll=true` 的 DLL；
4. 调用带 `[ModInitializer]` 的静态方法；
5. 如果 `has_pck=true`，把 PCK 挂到 Godot 资源系统。

皮肤切换器的关键点是：**它不希望游戏把每个皮肤 Mod 的 PCK/DLL 直接全局挂载**，否则多个皮肤会互相覆盖、未选中的皮肤也会执行代码。因此它把自己放在皮肤 Mod 之前加载，先“接管”这些 Mod。

### 2.2 入口：`Entry.Initialize`

新版入口 `STS2SkinChanger/Entry.cs` 的顺序：

```csharp
ModLog.Info($"内测版本 {InternalTestVersion}；程序集=...");
ModPrivateDependencyResolver.Install(ModLog.Info);   // 让皮肤 Mod 能加载自己包内的私有依赖
SkinChangerPaths.Initialize();                        // %APPDATA%/SlayTheSpire2/Gurio.SkinChanger
FrameworkCompatibilityLayer.Initialize();             // RitsuLib/BaseLib 等框架兼容
ManagedSkinModLoader.Initialize();                    // 扫描并隔离皮肤提供者
InstallPatchClasses(new Harmony(ModId), GetLoadableTypes(assembly)); // 逐类装 Harmony 补丁
ProviderManifestCompatibility.AcknowledgeParsedManifests(ModManager.Mods);
```

旧版这里是 `harmony.PatchAll()`：**任何一个补丁类失败，整批回滚，整个 Mod 的代码接管失效**。新版改为逐类 `CreateClassProcessor(type).Patch()`，失败只跳过该类并单独 `Unpatch()`，这是 1.0.19.11 的核心改动之一。

入口还包含几个关键 Harmony 补丁：

- `EssentialInitializationPatch`：在游戏 `OneTimeInitialization.ExecuteEssential` 前后再次清理未选中提供者的视觉补丁，并初始化卡片分组、角色预览、外部卡牌桥、社区投稿刷新。
- `CosmeticLocalizationOwnershipPatch`：过滤已卸载皮肤的角色本地化表，避免角色名/说明串皮。
- `DuplicateModelTypeCompatibilityPatch` / `DuplicateModelInitCompatibilityPatch`：处理旧玩法 Mod 与游戏原版模型 ID 冲突，保留原版，避免 `ModelDb` 启动崩溃。
- `MerchantRoomPrewarmPatch`（新版新增）：地图打开时预读商店房间覆盖包。
- 大量 UI/角色/卡牌补丁分布在 `Ui/`、`Core/` 中。

### 2.3 隔离与识别：`ManagedSkinModLoader`

启动后它做四件事：

1. **枚举 Mod**：读取 `ModManager.Mods`、每个 Mod 的目录、清单、依赖、PCK、DLL、资源文件。
2. **判断是不是“皮肤提供者”**：不是看名字，而是看证据：
   - 有 PCK，且其中包含角色/怪物/卡牌/先古/商人/事件资源；
   - DLL 补丁了角色外观创建入口、卡牌呈现、怪物场景等；
   - 纯素材卡图包（`*.cardartpack.json`）；
   - 整目录都是散装卡面的提供者目录；
   - 框架（RitsuLib/BaseLib 等）的内容注册声明；
   - 必须 `affects_gameplay=false`，玩法 Mod 不会被当皮肤。
3. **隔离**：未选中的提供者，其 PCK 不全局挂载、DLL 初始化器不执行、全局视觉补丁不生效；只静态读取 PCK 索引和 DLL 元数据/IL 来建立目录。对于“完整外观包”，在选中期间才临时加载其 DLL、执行其视觉初始化器、启用其行为补丁，切走后精确卸载。
4. **建立基线**：游戏原版 + 玩法 Mod 的资源作为“原版”基线；选中“游戏默认”时用基线资源压回。

日志中能看到它的工作痕迹：

```
[SkinChanger] 托管加载模式已识别 136 个皮肤提供者；PCK 资源由本 Mod 按选择接管；
运行时私有依赖会在首次选中时低优先级挂载，DLL 行为补丁仅在选中期间启用。
[SkinChanger] 已隔离皮肤提供者 XXX：视觉组=1, 卡图=0, 卡牌呈现=0, 独立图片=0, 场景脚本=0, 交互场景=否；
原 PCK 未全局挂载；只有选中该皮肤时，才会按需调用与模型 CreateVisuals 明确绑定的视觉后处理……
```

### 2.4 皮肤目录：`SkinCatalog`

`SkinCatalog` 是“资源 → 分组 → 选项”的数据库：

- **分组（SkinGroup）**：角色（`ironclad`、`silent`…）、卡牌分类（角色牌、无色、先古、其他）、怪物（按怪物 ID）、先古（`darv`、`neow`…）、商人、事件（`event:xxx`）。
- **选项（SkinOption）**：一个具体皮肤来源，带资源集合、提供者 ID、显示名、是否合并皮肤、是否完整运行时提供者等。
- **原版选项**：`__base__`，不指向任何 Mod，表示回退游戏原版/玩法基线。
- **来源身份**：提供者 ID、工坊 ID、GitHub 仓库合成 ID 等，用于多人同步和缓存。
- **资源归类**：解析 PCK 路径，例如：
  - `res://animations/characters/<角色>/...`
  - `res://images/packed/character_select/...`
  - `res://card_portraits/<角色>/...`
  - `res://animations/monsters/<怪物>/...`
  - `res://scenes/events/background_scenes/<先古>.tscn`
  - 商人房间、商店背景、手部动画等。
- **声明式呈现**：读取 PCK 里的卡框/卡面/布局 JSON（如 `frame_replacements.json`），不执行作者代码。
- **共享骨骼目录**：新版新增 `SharedAnimationModelIds`，把 `cultists`、`decimillipede`、`kaiser_crab`、`bowlbug` 这些“多个怪物共用一套骨骼目录”的组映射到具体模型 ID，解决 1.0.19.4 的入口缺失问题。

### 2.5 选择与挂载：`SkinService`

`SkinService` 是运行时总服务，负责：

1. **保存选择**：`Config.Selections[组ID] = 选项ID`，写入 `skin_changer.json`。
2. **构建事务**：`Catalog.BuildVisualSelectionTransaction(groupId, optionId, selections)` 计算“选这个皮肤会同时影响哪些组”，例如完整包同时拥有角色、宠物、多个怪物，必须一起切换/一起回退。
3. **重建覆盖包**：`MountOverlay(groups)`：
   - 从基线 PCK 中取未被覆盖的资源；
   - 从选中提供者 PCK 中取要覆盖的资源；
   - 处理资源重映射、私有路径、依赖、图集、脚本路径；
   - 生成一个临时 PCK（`cache/<会话>/...`）；
   - `ProjectSettings.LoadResourcePack(path, replaceFiles: true)` 挂到 Godot 资源栈顶；
   - 记录挂载状态，切走时挂“修正包”把原版资源压回。
4. **刷新界面**：角色预览、选角大图、头像、卡牌、怪物、商店、事件文本等。
5. **皮肤包（[P]）**：保存“角色皮肤 + 各卡牌分类预设 + 各怪物地区预设”的组合；开局只应用本局，退出恢复全局。
6. **随机皮肤**：选角列表末尾的“随机皮肤”，开局从原皮/可见皮肤/合并皮肤/有效皮肤包中抽取；多人只为本机抽。
7. **多人同步**：自定义网络消息，按玩家发送/接收皮肤、头像、缩放位移；对方未安装的皮肤回退本机原皮，不下载远端资源。
8. **先古/事件优先级**（新版扩展）：按分类统一启用/排序来源，单个对象可“跟随分类”或手动指定。

覆盖包的核心不是“改游戏文件”，而是**运行时资源栈替换**。所以：
- 不修改任何已安装 Mod 的 PCK/DLL/清单；
- 切回原版时挂修正包，而不是删文件；
- 退出时清理 `cache/`，被进程占用的包留到下次启动清理。

### 2.6 配置与持久化

配置目录：`%APPDATA%\SlayTheSpire2\Gurio.SkinChanger\`（Godot 的 `user://`）。

| 文件 | 作用 |
|---|---|
| `skin_changer.json` | 皮肤选择、优先级、预设、皮肤包、外观参数、先古优先级、被尊重的加载顺序等 |
| `skin_changer_theme.json` | 当前界面主题 |
| `skin_changer_theme_presets.json` | 主题预设 |
| `skin_changer_bundle_runs/` | 皮肤包对局的独立外观记录 |
| `skin_changer_bundle_run_restore.json` | 皮肤包恢复点 |
| `github_skins.json` | GitHub 皮肤列表缓存 |
| `github_installs.json` | GitHub 安装台账（目录、版本、sha256） |
| `cache/<会话>/` | 运行时生成的覆盖包、工坊图片、社区清单等 |

旧版配置会自动迁移；冲突旧文件放 `migration-backup/`。

### 2.7 界面入口

- **选角界面**：角色皮肤下拉、合并皮肤、管理皮肤包、单卡选择器、模型预览、拖动柄。
- **暂停菜单 → 外观**：先点击画面中的角色/怪物/先古/同伴/商人，再打开设置面板；可切皮肤、调模型缩放/位移、血条/意图参数。
- **卡牌总览**：按角色/颜色切换卡牌皮肤；单卡放大后可在卡牌顶部单独选卡图/卡框。
- **怪物图鉴**：按怪物切皮肤、按地区保存怪物皮肤预设。
- **其它图鉴**：先古、商人（含假商人）、生物、事件；新版在这里加了“先古皮肤优先级”面板和“显示局内外观入口”开关。
- **创意工坊面板**：内置清单 + 社区投稿 + Steam 订阅/取消订阅/下载进度；来源可切 Steam / GitHub。
- **主题编辑器**：颜色、透明度、模糊、圆角、边框、文字描边、阴影、预设码。

### 2.8 兼容层

- **旧皮肤管理器兼容**：随包 `thunninoiSkinManager.dll` 提供同名接口/数据类/注册表；原管理器启用时由游戏加载它，SC 不抢同名接口，也不卸载它的显示补丁。
- **框架兼容**：RitsuLib/BaseLib 等框架的皮肤契约、卡牌视觉、场景工厂；框架已原生处理的呈现不再叠加 SC 后备实现。
- **跨游戏版本**：对声明了更高 `min_game_version` 的皮肤，管理器把 DLL 整份 JIT 编译一遍（不执行、不打补丁），全部方法可编译才接管，并把已知旧接口调用改写到新接口；任何一处编译失败就保持游戏原本的拒绝，日志写明阻塞方法。
- **私有依赖**：只从请求方自己的包目录解析，不顶替游戏自带同名程序集。
- **重复模型 ID**：保留游戏原版，避免旧玩法 Mod 让 `ModelDb` 启动崩溃。
- **部件接管**：对“整体风格/UI 包顺带覆盖的单个部件”，仍由游戏原加载器完整挂载作者的 DLL/PCK，只把可还原的部件做成选项；未选中时作者的分组画面补丁停用，选中时恢复。

### 2.9 退出清理

主菜单确认退出后：
1. 先在后台清理 `cache/`；
2. 再执行游戏原本的设置保存与退出；
3. 取消退出不清理；
4. 本次仍被占用的 PCK 留在原地并写日志，下次启动清理；
5. 没有独立清理程序，不依赖 Windows 计划任务，不改脚本执行策略。

---

## 3. 新版相对旧版改了什么

### 3.1 二进制/源码差异规模

- 旧 DLL 反编译出 682 个 `.cs` 文件；新 DLL 684 个（多 2 个真实类型：`MerchantRoomPrewarmPatch`、`LoadOrderRespectPolicy`；其余差异是编译器生成的正则类型哈希）。
- 程序集公开类型：934 → 936（含 57 个正则生成类型）；去掉生成类型后是 877 → 879。
- 方法签名差异约 45 处，绝大多数是编译器生成的 lambda/local function 编号变化；真实新增方法集中在 `SkinService`、`AncientCompendiumScreen`、`Entry`、`LoadOrderWarningController`。
- 逐文件文本对比：25 个文件有差异，其中约 16 个是实质改动；其余是版本号、正则生成哈希、`const` 可见性、编译器空值转换等。
- 兼容层 `thunninoiSkinManager.dll` 的公开 API 与旧版完全一致（70 行类型/方法导出逐行相同），只是重新编译导致体积差 512 字节。

### 3.2 按版本列出的更新（来自 `docs/COMPLETED_FIXED.md`，并与代码差异核对）

| 版本 | 更新 | 代码证据 | 核对结果 |
|---|---|---|---|
| 1.0.19.3 | 先古/商人皮肤互踩（根因一：登记被前缀过滤清空） | `SkinCatalog` 新增“分组被接管则回退原版”的日志；`SkinService` 选择事务 | 文档记录；反编译层面只看到日志差异，核心逻辑可能早已存在或 diff 不可见 |
| 1.0.19.4 | 共享骨骼目录怪物换肤入口缺失（邪教徒/千足虫/帝皇蟹/碗虫） | `SkinCatalog.SharedAnimationModelIds` + `ContextualSkinControls.GroupMatchesSharedAnimationLayout`；运行时测试 `Shared monster groups passed` | 已确认 |
| 1.0.19.5 | 框架类型发现不容忍程序集拆分加载失败 | `Entry.GetLoadableTypes`、`FrameworkCardVisualGuard.TryGetType`、`FrameworkCreatureSceneFactory` 改用容错查找 | 已确认 |
| 1.0.19.6 | 进商店卡顿/黑屏：地图打开时预读商店房间资源包 | 新类型 `MerchantRoomPrewarmPatch`（`NMapScreen.Open` Postfix）+ `SkinService.PrewarmRuntimeSceneOverlay` + 耗时日志 | 已确认 |
| 1.0.19.7 | 局内暂停菜单外观入口被右键隐藏后找不回 | 其它图鉴新增“显示局内外观入口”CheckButton；`ModText.InRunAppearanceEntryToggle`；15 语言文本 | 已确认 |
| 1.0.19.9 | 自动排序每次启动覆盖玩家手动排序 | 新类型 `LoadOrderRespectPolicy`；`SkinConfig.RespectedLoadOrderProviders`；`LoadOrderWarningController` 只移动“未手动上移”的提供者 | 代码已确认，但存在“记忆永不写入”的逻辑问题，见 5.1 |
| 1.0.19.10 | 选人界面切“游戏默认”卡死/闪退（覆盖包重建同线程重入 → 递归写包 → 栈溢出） | `SkinService.MountOverlay` 增加 `[ThreadStatic]` 深度/待重建集合，重入合并到最外层，最多补 8 轮 | 已确认 |
| 1.0.19.11 | 测试版与 baselib/RitsuLib 同开启动级故障（单补丁类失败放大成全 Mod 停用） | `Entry.InstallPatchClasses` 逐类安装；失败类单独 `Unpatch()` 并记录；`GetLoadableTypes` 容忍部分类型加载失败 | 已确认 |

### 3.3 文档未单列、但代码中新增的功能

1. **先古皮肤优先级（分类）**
   - 配置新增 `AncientSkinPriorities`（复用事件优先级的设置类，但组 ID 不带 `event:` 前缀）。
   - 常量 `all_ancients`、`__ancient_category__`。
   - `SkinService.InitializeAncientSkinCategories()` / `GetAncientPriorityOptions()` / `SetAncientPriorityOptionEnabled()` / `MoveAncientPriority()` / `FollowAncientCategoryPriority()`。
   - 其它图鉴“先古”分类新增“皮肤优先级”按钮、启用勾选、覆盖数/总数、上下排序、关闭。
   - 单个先古下拉新增“跟随分类”，与手动选择双向同步。

2. **“全部事件”全局优先级**
   - `EventRegionPolicy.Global = "__global__"`。
   - 事件地区下拉最上方新增“全部事件”；`__global__` 在分组字典中排在最前，`BuildEventPrioritySelectionUpdates` 用 `TryAdd` 让全局池成为跟随分类事件的统一来源。
   - 15 语言补齐“全部事件”。

3. **商店房间预读**
   - 地图打开即准备当前商人/商人房间的覆盖包，进入商店时只做挂载/实例化。
   - 超过 30ms 会写日志；失败不阻断，进房间仍走原路径。

4. **性能/诊断日志**
   - 选角预览重建入口日志（F29 取证）。
   - 卡牌总览同步准备耗时（超过 100ms）。
   - 单张卡图首次加载超过 30ms。
   - 随机皮肤候选数量日志。
   - 覆盖包重建“开始/完成”成对日志。

5. **随机皮肤候选去重**
   - `FilterRandomCharacterSkinCandidates(...).Where(RandomCharacterSkinPolicy.IsCandidate).Distinct(...)`，避免同一来源重复进入随机池。

6. **加载顺序提示**
   - 自动排序成功后，提示语改为“移到未手动上移的皮肤 Mod 之前”。
   - 手动保留的提供者不再提示重启。

### 3.4 没有变的部分

- 皮肤目录的总体识别规则、覆盖包机制、运行时资源栈替换、配置目录与文件格式（新增字段向后兼容）。
- 工坊/GitHub 的总体流程、SCM 投稿码、多人协议、主题系统、皮肤包/预设。
- 兼容层 API 完全不变。
- 公开版本号仍是 1.0.19（只有四段内测号到 .11）。

---

## 4. 构建、测试与运行验证

### 4.1 构建

在本机 v0.111.0 程序集上：

```powershell
dotnet build STS2SkinChanger/STS2SkinChanger.csproj -c Release `
  -p:GameAssemblyDir="D:/steam/steamapps/common/Slay the Spire 2/data_sts2_windows_x86_64"
```

结果：

```
ThunninoiSkinManagerCompat -> .../thunninoiSkinManager.dll
STS2SkinChanger -> .../Gurio.SkinChanger.dll
已成功生成。0 个警告，0 个错误。用时 9.18 秒
```

产物：`Gurio.SkinChanger.dll`、`SkinChanger.json`、`thunninoiSkinManager.dll`（另有 deps/runtimeconfig/pdb，是构建副产物，不必部署）。

### 4.2 逻辑测试

```powershell
dotnet run --project tests/STS2SkinChanger.LogicTests -c Release -p:GameAssemblyDir=...
```

全部通过，覆盖：GitHub 来源身份/策略/发布/安装台账/客户端/筛选/文本、工坊选项选择、皮肤优先级显示、页面池、Godot 脚本路径注册、私有依赖、游戏包定位、退出拦截。

### 4.3 运行时测试

```powershell
dotnet run --project tests/STS2SkinChanger.RuntimeTests -c Release -p:GameAssemblyDir=...
```

默认全套通过，退出码 0。包含（摘录）：

- 基线部件接管、商人形象作用域、存储生命周期、退出清理/边界
- 生物视觉父级、提供者工具栏、手动角色差分、外观边界、意图布局
- 托管立绘映射、随机大厅事务、随机角色皮肤、随机排除、预览取景
- 框架选择器循环、框架模型预览、内置预览、动作复活
- HUD 生命周期、私有依赖解析、主题预设、加载顺序安全
- **Entry 补丁逐类安装隔离测试**（坏类跳过、好类保留、失败按类报告）
- 卡牌优先级性能、卡牌呈现生命周期、导出卡面、状态卡图
- GitHub 文件夹交换、工坊轮播/元数据/订阅筛选/投稿完整性
- **共享怪物分组**（cultists/decimillipede/kaiser_crab/bowlbug）

### 4.4 旧版游戏日志

- 日志共 2,479 行，Skin Changer 正常初始化：识别 136 个提供者、旧依赖清单迁移、代码补丁加载、退出缓存清理 16 个文件。
- 没有 `[SkinChanger]` 的异常/失败；只有一些**与 SC 无关**的警告：
  - 多个“找不到皮肤提供者程序集”的警告，来自白猫整合包里 DLL 缺失的条目，SC 只是如实报告；
  - `LulukaSignaturePack` 缺少前置 `SignatureLib,Luluka`，是另一个 Mod 的依赖问题；
  - Godot 退出时的 RID/资源泄漏是引擎常见现象，不是 SC 抛出的异常。
- 结论：旧版在本次会话中运行正常，未崩溃。

### 4.5 能否正确运行的判断

- **v0.111.0（本机测试版）**：构建、逻辑测试、运行时测试全部通过；代码路径与旧版一致，新增逻辑有测试覆盖的部分通过。可以认为“能正确运行”，但未做实机画面验证。
- **v0.107.1（正式版）**：本机没有该版本程序集，无法验证编译。新版使用了 `NMapScreen.Open`、`ModelDb.AllAncients` 等 API；这些在正式版大概率存在，但必须在正式版程序集上重新构建、跑 `Release` 与 `ReleaseBeta` 两套测试后才能发布。
- **未覆盖**：先古优先级面板的实际交互、局内外观入口开关、商店预读的实际卡顿改善、右键排除随机皮肤的画面表现。

---

## 5. 代码审查发现（按严重度）

### 5.1 【高】F35 加载顺序的“尊重列表”可能永远不会被写入

相关代码：

- `STS2SkinChanger/Ui/LoadOrderWarning.cs:38-59`
- `STS2SkinChanger/Ui/LoadOrderRespectPolicy.cs:8-28`
- `STS2SkinChanger/Core/SkinConfig.cs:148`

调用链：

```csharp
var (deliberate, actionable) = LoadOrderRespectPolicy.Resolve(
    providersAboveSelf.Select(...),
    SkinService.GetRespectedLoadOrderProviders());

if (actionable.Count == 0) { ... return; }

MoveSelfBeforeSkinProviders([.. actionable]);
SkinService.SetRespectedLoadOrderProviders(deliberate);   // ← 保存的是 deliberate
```

`Resolve` 的定义是：

- `remembered` = 配置里记录的“玩家手动保留”ID；
- `deliberate` = 既在 `providersAboveSelf` 又在 `remembered` 里的 ID；
- `actionable` = 在上面但不在 `remembered` 里的 ID。

`RespectedLoadOrderProviders` 的**唯一写入点**就是上面这一行，写入的是 `deliberate`。首次运行时配置为空 → `remembered` 为空 → `deliberate` 为空 → 保存空列表。之后玩家如果把某个皮肤 Mod 手动移到 SC 之上，下次启动它仍然“不在 remembered 里” → 被归为 `actionable` → 再次被压回。也就是说：

> 这个“记忆”没有种子，永远学不到第一个“玩家手动上移”的提供者，F35 想修的场景可能仍然复现。

合理的修复方向：成功移动后把 `actionable`（本次被自动移动的提供者）记入 `RespectedLoadOrderProviders`，这样下次玩家再把其中任何一个移到上面，就会被识别为“刻意”；新安装的提供者仍然不在列表里，会继续被自动排序。或者单独记录“玩家手动排序事件”，不要用自动排序结果反推。

现有测试 `LoadOrderSafetyTests` 只直接调用 `Resolve` 并传入一个现成的 `remembered` 列表，没有覆盖“第一次运行 → 写入 → 第二次运行”的持久化闭环，所以测试通过但问题可能仍在。

**影响**：玩家手动排序仍可能每次启动被覆盖，商人娘化等依赖“皮肤 Mod 先于 SC 加载”的场景仍会失效。  
**建议**：接手后先补一个两会话回归测试，再决定是否改为保存 `actionable`。

### 5.2 【高】第一次打开“其它图鉴 → 先古”会自动套用每个先古的第一个皮肤

相关代码：

- `STS2SkinChanger/Ui/AncientCompendium.cs:651`：`_Ready()` 调用 `SkinService.InitializeAncientSkinCategories()`。
- `STS2SkinChanger/Core/SkinService.AncientPriorities.cs:11-36`：登记 `all_ancients` 分组。
- 同文件 `172-199`：`BuildAncientPrioritySelectionUpdates()`。

流程：

1. 首次打开其它图鉴时，`Register` 把当前目录里的先古组登记到 `RegionGroups["all_ancients"]`。
2. `Register` 只把“配置里已有非原版选择”的先古加入 `ManualGroups`；选择是 `__base__`（原版）的先古仍然是“跟随分类”。
3. 此时 `Priorities["all_ancients"]` 为空，`GetAncientPriorityEntries()` 会把该分类下**所有选项**按目录顺序补进列表，且默认全部启用。
4. `BuildAncientPrioritySelectionUpdates()` 对每个“跟随分类”的先古执行 `EventSkinPriorityPolicy.Resolve(...)`，返回**该组第一个可用选项**。
5. 代码直接把结果写进 `Config.Selections`，然后 `MountOverlay(affected)` 并 `Config.Save`。

结果：玩家只是打开图鉴看一眼，所有原本用原版皮肤的先古就会被换成各自第一个皮肤，并且写入配置、立即挂载。这不是用户主动选择，属于明显的行为副作用。

**建议**：
- 首次登记时只建立分组，不执行选择应用；或
- 把 `__base__` 也视为“手动”，只有用户显式点“跟随分类”才让它参与分类解析；或
- 在 `InitializeAncientSkinCategories` 中跳过 `BuildAncientPrioritySelectionUpdates`，仅保存空优先级。

### 5.3 【中】先古优先级的应用路径绕过了事件优先级的“事务 + 完整运行时提供者”保护

事件版本 `SkinService.EventPriorities.cs:152-180` 做了两件先古版本没有做的事：

1. 对“完整运行时提供者”（一个 DLL 同时拥有多个组）做一致性校验，避免只改一个组导致半接管；
2. 用 `catalog.BuildVisualSelectionTransaction(id, selected, working)` 计算连带修改，并拒绝修改非事件组。

先古版本 `SkinService.AncientPriorities.cs:182-194` 只是：

```csharp
var desired = pools.ToDictionary(...Resolve...);
var working = new Dictionary<string, string>(Config.Selections, ...);
foreach (var (id, selected) in desired)
    if (!working.GetValueOrDefault(id, "__base__").Equals(selected, ...))
        working[id] = selected;
```

直接改选择，不检查提供者是否拥有其它组，也不处理依赖/联动。对普通“单先古场景”皮肤没问题；对“整套先古包/完整运行时提供者”可能出现只切了一个先古、其它组未同步的状态。事件路径已经解决过同类问题，建议先古路径复用同一套逻辑。

### 5.4 【中】商店预读在主线程持锁同步构建，首次可能反而造成地图打开卡顿

相关代码：`SkinService.PrewarmRuntimeSceneOverlay`（`SkinService.cs:1443` 附近）、`MerchantRoomPrewarmPatch`（`MerchantRuntimeAppearance.cs:1517`）。

`NMapScreen.Open` 的 Postfix 直接调用预读，而预读内部 `lock (Sync)`，并同步执行 `GetOrPrepareRuntimeOverlay(...)`（可能读 PCK、生成覆盖包、写临时文件）。首次选中某个商人皮肤时，这个耗时可能达到数百毫秒，正好卡在“打开地图”这个操作上。注释说“cheap no-op after the first attempt”，但第一次并不便宜。

**建议**：把预读放到后台线程/延迟一帧；或先只做轻量索引，不生成包；或在预读前后加更细的耗时日志，确认实际开销。现有代码只在 ≥30ms 时记一条日志，不会阻断，但玩家可能感到一次性卡顿。

### 5.5 【低】`MountOverlay` 的重入队列在异常时可能残留

`MountOverlay` 用 `[ThreadStatic] _mountOverlayDepth` 和 `_pendingRemountGroups` 合并重入；`finally` 只递减深度，不清空 pending。如果 `MountOverlayCore` 抛异常，调用方 catch 后，pending 集合可能留到下一次无关的重建里被补执行，造成一次多余挂载。影响较小，但建议在异常路径清空或记录。

### 5.6 【低】`ModText` 枚举插入导致后续数值整体偏移

`ModText.InRunAppearanceEntryToggle` 插在枚举中间，后面所有成员的整数值 +1。`ModLocalization` 内部的 switch 已经同步改了 `text - 82` 之类的范围，代码内没有问题；但如果未来有外部代码/存档/日志按数值持久化 `ModText`，会出现错位。当前搜索未发现按数值持久化的地方，属于潜在风险。

### 5.7 【信息】公开版本仍是 1.0.19

`SkinChanger.json` 的 `version` 还是 1.0.19，只有 `internal_test_version` 是 1.0.19.11。如果直接把这套源码构建后上传工坊，Steam 的版本号不会变，玩家/更新检查可能认为没有更新。按项目约定，上传工坊时才提升第三段公开版本，并同步 `csproj`、`Entry`、`SkinChanger.json`、`workshop/workshop.json`。

### 5.8 【信息】正式版 v0.107.1 未验证

本机只有 v0.111.0。`Directory.Build.props` 默认指向 `D:/Programs/Steam/.../v0.107.1`，但该路径不存在，构建时必须显式传 `GameAssemblyDir`。发布前必须在正式版程序集上构建 `Release`，并在测试版程序集上构建 `ReleaseBeta`，两套测试都跑。

---

## 6. 接手建议

### 6.1 推荐阅读顺序

1. `README.md`（功能全貌）
2. `STS2SkinChanger/Entry.cs`（启动链路）
3. `Ui/LoadOrderWarning.cs`（为什么需要排在皮肤 Mod 前面）
4. `Core/ManagedSkinModLoader.cs`（识别与隔离，最大的一份）
5. `Catalog/SkinCatalog.cs`（资源如何归类成分组/选项）
6. `Core/SkinService.cs`（选择 → 事务 → 覆盖包 → 挂载）
7. `Core/SkinConfig.cs` + `SkinChangerStorage.cs`（配置）
8. `Ui/` 下各界面（选角、其它图鉴、卡牌、工坊、主题）
9. `Core/FrameworkCompatibilityLayer.cs`、`ProviderAssemblyCompatibility*.cs`（兼容层）
10. `Core/MultiplayerSkinSync.cs`（多人协议）

### 6.2 构建与部署

```powershell
# 正式版（需要 v0.107.1 程序集目录）
dotnet build STS2SkinChanger/STS2SkinChanger.csproj -c Release `
  -p:GameAssemblyDir="<v0.107.1 程序集目录>"

# 测试版（独立配置，避免覆盖正式版产物）
dotnet build STS2SkinChanger/STS2SkinChanger.csproj -c ReleaseBeta -p:Optimize=true `
  -p:GameAssemblyDir="D:/steam/steamapps/common/Slay the Spire 2/data_sts2_windows_x86_64"

# 测试
dotnet run --project tests/STS2SkinChanger.LogicTests -c Release -p:GameAssemblyDir=...
dotnet run --project tests/STS2SkinChanger.RuntimeTests -c Release -p:GameAssemblyDir=...
```

部署三件：`Gurio.SkinChanger.dll`、`SkinChanger.json`、`thunninoiSkinManager.dll`。建议放到 `mods/SkinChanger/` 做本地测试；确认无误后再替换工坊内容或上传。

### 6.3 发布前检查清单

- [ ] 关闭游戏后再替换 DLL，避免文件占用。
- [ ] 备份 `%APPDATA%\SlayTheSpire2\Gurio.SkinChanger\`。
- [ ] 启动日志出现 `内测版本 1.0.19.11`。
- [ ] 日志中补丁加载没有“失败被跳过”（除非确实有可选框架缺失）。
- [ ] 136 个提供者能正常识别；未选中皮肤不报错。
- [ ] 角色/卡牌/怪物/先古/商人/事件各切一次，再切回原版。
- [ ] 多人：双方都装 SC 时能同步；一方缺皮肤时回退不崩。
- [ ] 退出后 `cache/` 清理；被占用的包下次启动能删。
- [ ] 上传工坊前 boost 公开版本号并同步四个位置。

### 6.4 优先修复建议

1. 先补 F35 的两会话回归测试，确认 `RespectedLoadOrderProviders` 的实际行为，再决定保存 `deliberate` 还是 `actionable`。
2. 给“先古优先级首次登记”加保护：首次打开不自动换肤；补一个“空优先级 + 全原版选择”的测试。
3. 把先古优先级的事务路径对齐事件优先级（`BuildVisualSelectionTransaction` + 完整运行时提供者校验）。
4. 观察商店预读的实际耗时；如首次地图打开明显卡顿，改后台/延迟。

---

## 7. 附录

### 7.1 主要改动文件（反编译对比）

实质改动：

- `STS2SkinChanger/Entry.cs`
- `STS2SkinChanger/Core/SkinService.cs`（+ 先古优先级分卷）
- `STS2SkinChanger/Core/SkinConfig.cs`
- `STS2SkinChanger/Core/EventRegionPolicy.cs`
- `STS2SkinChanger/Core/EventSkinPrioritySettings.cs`（在 `EventSkinPriorityPolicy.cs` 中）
- `STS2SkinChanger/Core/FrameworkCardVisualGuard.cs`
- `STS2SkinChanger/Core/FrameworkCreatureSceneFactory.cs`
- `STS2SkinChanger/Core/ModLocalization.cs`
- `STS2SkinChanger/Catalog/SkinCatalog.cs`
- `STS2SkinChanger/Ui/AncientCompendiumScreen.cs`（在 `AncientCompendium.cs` 中）
- `STS2SkinChanger/Ui/ContextualSkinControls.cs`
- `STS2SkinChanger/Ui/LoadOrderWarning/LoadOrderWarningController.cs`
- `STS2SkinChanger/Ui/MerchantRuntimeAppearance.cs`（const 可见性 + 新补丁类）
- 新文件 `STS2SkinChanger/Ui/LoadOrderRespectPolicy.cs`（类型在 `LoadOrderWarning` 命名空间）
- 新类型 `MerchantRoomPrewarmPatch`（定义在 `MerchantRuntimeAppearance.cs` 末尾）

仅版本/编译器差异：

- `Properties/AssemblyInfo.cs`
- `Core/GitHubSkinService.cs`
- `Core/SkinWorkshopService.cs`
- `Core/ManagedSkinModLoader.cs`（空值转换）
- `Catalog/CardArtPackScanPolicy.cs`、`Core/OnlineSkinCache.cs` 等（正则生成哈希）
- `Core/ModText.cs`（枚举插入）

### 7.2 关键日志标记

| 日志 | 含义 |
|---|---|
| `内测版本 1.0.19.11；程序集=...` | 入口版本 |
| `代码补丁已加载。等待游戏资源初始化。` | 所有补丁类安装成功 |
| `代码补丁已加载（N 个类失败被跳过：...）` | 有补丁类失败，需看具体类名 |
| `托管加载模式已识别 N 个皮肤提供者` | 目录构建完成 |
| `已隔离皮肤提供者 ...` | 某个提供者被隔离，不全局挂载 |
| `开始重建外观覆盖：N 个分组` | 覆盖包重建开始 |
| `已挂载...` / `耗时=...` | 挂载完成与性能 |
| `已自动将皮肤切换器...移到未手动上移的皮肤 Mod 之前` | 自动排序发生 |
| `已尊重玩家手动排序...` | 本次不排序（如果列表真能记住） |
| `退出缓存清理：已删除 N 个文件` | 退出清理 |

### 7.3 文件统计

- 原项目：501 个文件，32 个子目录（不含根）。
- 中文副本：501 个原文件 + 33 个 `_目录说明.txt`（根 + 32 个子目录）。
- 副本校验：501 个文件逐字节 SHA-256 与源文件一致，0 处差异。
- 源项目校验：分析前后 501 个文件 SHA-256 完全一致，未被修改。

### 7.4 中文副本位置

`D:\STS2皮肤切换器_中文副本`

- 目录名、文件名已翻译为中文；
- 文件内容保持原样（代码、JSON、Markdown、图片均未改）；
- 每个目录（含根）都有 `_目录说明.txt`，解释同目录文件/子目录的用途；
- 副本不能直接用于构建：目录名和 `.csproj` 引用路径已改变（这是“阅读副本”，不是发布副本）。需要构建时请使用原项目 `D:\STS2-SkinChanger`。

---

*本报告基于 2026-11-02 的源码与日志；未启动游戏做实机画面验证，正式版 v0.107.1 的编译/运行需在对应程序集上另行确认。*
