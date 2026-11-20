# #255 / #256 愚者卡图包全局挂载与共享图集导入产物防护修复（Gemini）

- **工作树**：`D:\mod\工作2\上游修复_1.0.35\STS2-SkinChanger_修改件`（主工作区；未提交、未部署）
- **日期**：2026-11-21
- **对照真实包与日志**：
  - `fool_card_beauty`（愚者卡面美化 v5.01：`fool_card_beauty.pck` + `fool_card_beauty.cardartpack.json`，无 DLL）
  - `CuteFool-Cards`（纯 PCK，无 DLL）
  - 白猫 1.0.37.2 日志：`C:\Users\yuinn\Desktop\愚者卡面美化_v5.01等2项文件\godot(4).log`
  - 云 1.0.36.104 实测日志：`C:\Users\yuinn\AppData\Roaming\SlayTheSpire2\logs\godot.log`

---

## 一、根因实证（含 #256 差异追查结论）

### 1. `fool_card_beauty.pck` 静态比对实证

- 与测试版 v0.113.1 `SlayTheSpire2.pck` 存在 **97 个精确重名路径，且全部 97 项 MD5 均与原版不同**：
  - 共享卡牌图集第 0 页：`res://.godot/imported/card_atlas_0.png-6b947739e98d17beac01a23ccc9a95cd.bptc.ctex`（原版 16,418,356 B `4651fa63` vs 本包 15,189,896 B `0e16f70c`；正式版 v0.111.1 与测试版 v0.113.1 路径完全同名）；
  - 95 张愚者测试版单卡导入载荷（如 `res://.godot/imported/all_is_well.png-73227dfce7ac1224cd4a4dd57fbfec57.bptc.ctex` 等）；
  - `res://.godot/global_script_class_cache.cfg`（原版 13,491 B 被本包 8 B 文件全局覆盖）。
- `SkinCatalog.ProbeSkinProviders` 先前未检查同目录 `*.cardartpack.json`，导致 `fool_card_beauty` 逃过隔离，游戏原加载器直接全局挂载了 `fool_card_beauty.pck`（白猫日志 L1065、云 00:13 日志 L1431）。

### 2. #256 核心问题：为何当前测试包（1.0.36.104）看起来正常，而白猫 1.0.37.2 选「游戏默认」会出改图？

**不是可靠机制兜底，而是两项与「已装其他卡包 / 仅覆盖愚者卡池」强耦合的间接效果暂时掩盖了全局污染：**

1. **白猫 1.0.37.2 为何只有愚者卡（尤其测试版单卡）在「游戏默认」下出改图**：
   - 愚者 95 张测试版卡的名义路径是 `res://images/atlases/card_atlas.sprites/fool/<card>.tres`，但测试版 `SlayTheSpire2.pck` 中实际资产在 `res://images/packed/card_portraits/fool/beta/<card>.png.import`；
   - 1.0.37.2 缺少 `SkinService.ResolveGameCardPortraitFallback`（本分支 `SkinService.cs:3684-3708`），选择 `fool/__base__` 时隔离回源失败，日志连续报 `卡牌皮肤资源不可用，已回退原版卡图：fool/__base__/res://images/atlases/card_atlas.sprites/fool/{foolish_shot,wah_touch,wah_sweep,wah_hammer,nock,recall,wah_cho}.tres；原因=找不到任何可隔离的卡牌资源`（`godot(4).log` L3703/3835/3881/3992/4084/29434/30549/30955/31172/32664）；
   - 回退游戏原生 `ResourceLoader` 读取规范路径后，直接命中 `fool_card_beauty.pck` 全局挂载进去的 95 张同名 `.godot/imported/<card>.ctex`；
   - 而 `strike_fool.tres` / `defend_fool.tres` 在本体包存在，走 `fool/__base__` 隔离加载（L30773/30816）未被击穿。
2. **云 00:13 日志（1.0.36.104）为何看似正常**：
   - `ResolveGameCardPortraitFallback` 把 `fool/__base__` 正确映射到 `res://images/packed/card_portraits/fool/beta/<card>.png`，并经 `BuildIsolatedCardResources(..., useSelectedProvider: false)` 从游戏本体 PCK 复制到私有别名加载，绕开了被污染的全局 VFS；
   - 云同时装有 `CuteFool-Cards` 和铁甲等卡包，`BuildCardOverlay`（云日志 L1998 挂载 1186 项）在恢复 `card_atlas.sprites/...` 基线时顺带把原版 `card_atlas_0` 重新盖回了全局。
3. **为何必须保留隔离原 PCK + 共享导入产物过滤（不能只补测试）**：
   - 若玩家**只装 `fool_card_beauty`**（未装 `CuteFool-Cards` 及其他共享 `card_atlas_0` 的 PCK 卡包），`BuildCardOverlay` 不会为无 PCK 卡包的职业回写 `card_atlas_0`，所有落在 `card_atlas_0` 上的非愚者职业卡都会直接读到 `fool_card_beauty.pck` 的 15.19 MB 改图；
   - `fool_card_beauty.pck` 在模组加载阶段（L1431）全局挂载，早于卡图覆盖包挂载（L1998，相隔 10+ 秒），期间或任何绕过 `ReplaceCardPortrait` 的第三方/原生读取（如检查图鉴、测试版卡图切换、其他 Mod 直接读规范路径）仍会读到全局污染的 `.ctex`；
   - 未隔离时其 8 B `res://.godot/global_script_class_cache.cfg` 仍会覆盖游戏本体的 13.5 KB 脚本类缓存；
   - 任何未来含同名共享图集/卡图导入产物的提供者包，若不经过安全包与覆盖过滤，仍会在提权/低优先级挂载时污染全局。

### 3. `CuteFool-Cards` 核实

- `CuteFool-Cards.pck`（298 项）与正式版/测试版 `SlayTheSpire2.pck` 在 `res://.godot/imported/` 下 **0 重名**，且已被 `ProbeSkinProviders` 正常隔离（白猫日志 L1058、云日志 L1430）；
- 当前代码（含 `#235`）下，`BuildIsolatedCardResources` 产出 `CanonicalDependencyPaths.Count == 0`，所有文件均位于 `res://sts2_skin_runtime/`，白猫 1.0.37.2 的问题确为 `#235` 前遗留。

---

## 二、代码改动

1. **`STS2SkinChanger/Catalog/SkinCatalog.cs`**：
   - **`ProbeSkinProviders`**：对非玩法 Mod 检查 `CardArtPackScanner.Describe(mod.RootPath)`；旁挂存在有效 `*.cardartpack.json` 时记入 `cardAssets` 并置 `hasResourceBackedCosmetics = true`，使 `ManagedSkinModLoader.TryManage` 拦截其原 `.pck` 不再全局挂载（`RegisterCardArtPacks` 仍会通过 `GetCatalogMods()` 接管该旁挂 JSON 卡包）。
   - **通用共享图集/卡图导入产物防护（参照 `#241`）**：新增 `SharedGameCardAtlasRegex`、`IsSharedGameAtlasPath`、`ProtectSharedGameImportedPayloadFile` 与统一入口 `ProtectGlobalCanonicalPackFile`（保留 `ProtectCanonicalSelectPortraitFile` 兼容既有测试）：
     - 惰性从当前 `_gameArchive` 提取全部 `res://images/atlases/*.{import,remap}`（覆盖 `card_atlas_0/1`、`relic_atlas`、`epoch_atlas`、`ui_atlas`、`compressed*_atlas` 等）与 `res://images/packed/card_portraits/**/*.{import,remap}` 的 `res://.godot/` 导入产物路径（零硬编码哈希，自动适配正式版与测试版）。
   - 拦截写入点：`BuildOverlay`、`ShouldMountProviderDependency`、`BuildProviderNamespaceOverlay`（`BuildCardOverlay` 使用）、`BuildRuntimeResourceOverlay` 与 `BuildAliasedResourceOverlay` 均接入 `ProtectGlobalCanonicalPackFile` / `IsSharedGameAtlasPath`。
2. **`STS2SkinChanger/Core/SkinService.cs`** + **`Core/SkinService.RuntimeSafety.cs`**：
   - `PrepareSelectPortraitSafePack`（被全包/按需运行时挂载共用，保留方法名兼容既有测试）改用 `catalog.ProtectGlobalCanonicalPackFile` 过滤共享图集与卡图导入载荷，按排除列表哈希缓存安全包；
   - `MountArchiveOverlay` 禁止包含受保护路径的覆盖直接复用原 PCK；
   - `BindRuntimeDependency` 对 `IsSharedGameAtlasPath` 跳过 `TakeOverPath`。
3. **`tests/STS2SkinChanger.RuntimeTests/FoolCardPackIsolationTests.cs`（新增）** + **`Program.cs`**：
   - 新增 `--test-fool-card-pack-isolation` 并接入默认无参 `RuntimeTests`；
   - **合成测试**：无标准 `card_portraits`/`card_atlas.sprites` 的纯 `.godot/imported/*` PCK + 旁挂 `*.cardartpack.json`，验证 `ProbeSkinProviders` 隔离、`ProtectSharedGameImportedPayloadFile` / `ProtectGlobalCanonicalPackFile`、`PrepareSelectPortraitSafePack` 剥离与 `BuildCardOverlay` 零泄露；
   - **真实双包测试**（`fool_card_beauty` + `CuteFool-Cards` + Beta `SlayTheSpire2.pck`）：验证两包均被隔离、真实 `fool_card_beauty.pck` 安全过滤后剔除 `card_atlas_0` 与愚者 `.ctex` 且保留私有卡图、选「游戏默认」时愚者卡字节级回源本体 `SlayTheSpire2.pck` 且非愚者卡（铁甲打击）保持原版、选中 `fool_card_beauty`（含 `strike_fool` 与 `all_is_well`）与 `CuteFool-Cards` 各自仅替换愚者卡且载荷独立无全局重定向、`A → B → 默认 → A` 切换无串图。

---

## 三、编译与自动化验证结果

全部改动文件行尾均核实为纯 LF（`CRLF=0`）：`SkinCatalog.cs`、`SkinService.cs`、`SkinService.RuntimeSafety.cs`、`FoolCardPackIsolationTests.cs`、`Program.cs`。

| 分支 | 主工程 Release 构建（`-o %TEMP%`） | `LogicTests` | 完整无参 `RuntimeTests` | 受保护角色实包回归（`check-protected-characters.ps1` 等效 5 条命令） |
| --- | --- | --- | --- | --- |
| 测试版（v0.113.1） | **0 警告 / 0 错误** | **通过**（`Skin Changer logic policy tests passed.`） | **通过**（含 `FoolCardPackIsolationTests`，尾行 `Workshop policy tests passed.`） | **通过**（`runtime-safety` / `protected-boundaries` / `preview-binding` / `crusader-hooks` / `protected-characters` 全绿） |
| 正式版（v0.111.1） | **0 警告 / 0 错误** | **通过**（`Skin Changer logic policy tests passed.`） | **通过**（尾行 `Workshop policy tests passed.`） | **通过**（5 项全绿，含十字军实包钩子与贞德 SFW/NSFW 实包） |

> 注：受系统 PowerShell 脚本执行策略限制未直接调用 `.ps1`，按 `tools/check-protected-characters.ps1` 逐条原样执行其全部 5 条 `dotnet run` 命令（参数为 `_maintainer_set\工坊_3801530111`、`本地_皮肤_战士_贞德战士sfw`、`本地_皮肤_战士_贞德战士nsfw`），双分支均退出码 0。

---

## 四、实机验证步骤

1. 保持启用 `mods\_test_新增_愚者卡图\` 下的两个包（`fool_card_beauty` 与 `CuteFool-Cards`），启动测试版（v0.113.1）：
   - 核对 `%APPDATA%\SlayTheSpire2\logs\godot.log`：**同时出现** `已隔离皮肤提供者 愚者卡面美化` 与 `已隔离皮肤提供者 CuteFool-Cards`，**不再出现** `Loading Godot PCK ...fool_card_beauty.pck`；
   - 核对日志仍出现 `已接管 1 个纯素材卡图包（愚者卡面美化）；覆盖 91 张卡牌`。
2. 打开卡牌总览，将愚者卡图设为 **游戏默认**：
   - 确认愚者所有卡牌（打击/防御及全部测试版愚者牌）以及铁甲/猎宝/故障等非愚者卡牌（尤其落在 `card_atlas_0` 上的卡）全为**游戏原版卡图**，无任何串图。
   - **对照项**：可临时只保留 `fool_card_beauty`（禁用其他卡包）再进卡牌总览，确认非愚者职业卡图与愚者「游戏默认」依然全为原版。
3. 依次切换愚者卡图：**`愚者卡面美化` (A) → `CuteFool-Cards` (B) → `游戏默认` → `愚者卡面美化` (A)**：
   - 选 A 仅愚者牌切为 `愚者卡面美化`，非愚者牌不变；
   - 选 B 愚者牌切为 `CuteFool-Cards`，无 A 残留；
   - 切回「游戏默认」愚者与非愚者牌全恢复原版；
   - 再切回 A 正常生效。
4. 正式版（v0.111.1）回归：进入卡牌总览确认原版卡牌及其他卡包显示正常。
