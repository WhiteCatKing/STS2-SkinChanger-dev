# 优化阶段 2：原版完整性自检（Gemini）

- **工作树**：`D:\mod\工作2\上游修复_优化阶段2_完整性自检`（分支 `perf/phase2-integrity-20261122`，基于 `9bec0e9`；未提交、未推送、未部署）
- **日期**：2026-11-22
- **禁改红线确认**：未触碰 `Core/SkinService.cs`、`Core/CardPortraitSessionCache.cs`、`Catalog/SkinCatalog.cs`、`Core/ManagedSkinModLoader.cs`、`Core/PoroVariantBridge.cs`。

---

## 一、改动文件清单与放置位置

全部改动文件均为纯 LF 无 BOM：

1. `STS2SkinChanger/Core/VanillaIntegritySelfCheck.cs`（新增）：
   - `VanillaIntegritySelfCheck`：纯函数抽查与判定核心（`IsSampledRootPath`、`SelectSampledPaths`、`Evaluate`、`ScanArchives`、`FormatLogLines`）及运行时两阶段只读调度器（`ScheduleOnce`、`BeginOnMainThread`、`PrepareScanFromDisk`、`CompleteOnMainThread`、`SnapshotNativeModSources`、`IsModIsolatedBySkinChanger`）。
   - `VanillaIntegrityMainMenuReadyPatch`：挂接 `NMainMenu._Ready` 后置（归属 `STS2SkinChanger.Core`，由 `Entry.InstallPatchClasses` 自动安装，不改动 `Entry.cs`，亦不干扰 `STS2SkinChanger.Ui*` 的 `UiInjectionGuardTests` 计数）。
2. `tests/STS2SkinChanger.RuntimeTests/VanillaIntegritySelfCheckTests.cs`（新增）：离线单测套件（已接入无参全套并提供 `--test-vanilla-integrity`）。
3. `tests/STS2SkinChanger.RuntimeTests/Program.cs`（最小接线）：注册 `--test-vanilla-integrity` 单项并在无参完整套件中增加 `Gate.Run("VanillaIntegritySelfCheckTests", ...)`。
4. `tests/required-fixes.json`（最小接线）：新增第 **34 项** `vanilla-integrity-self-check`（`"since": "phase2"`）。

---

## 二、触发时机与线程分工

- **触发时机**：主菜单 `NMainMenu._Ready` 首次执行后（此时所有 Mod 与 `ManagedSkinModLoader` 均已完成启动期加载），通过 `Interlocked.CompareExchange` 保证整局会话**只调度一次**，再用 `Callable.From(BeginOnMainThread).CallDeferred()` 延后一帧启动，避免与同帧 `_Ready` 回调争抢。
- **线程分工（目标主线程仅数毫秒）**：
  1. **主线程快照（零磁盘 I/O）**：调用 `GamePackLocator.Resolve(...)` 定位 `SlayTheSpire2.pck`，并从 `ModManager.Mods` 按加载顺序拷贝所有 `ModLoadState.Loaded` 且含 `.pck` 的第三方 Mod 元信息与隔离标记（`SnapshotNativeModSources`）。
  2. **后台线程只读 PCK 扫描（`Task.Run`）**：只读打开游戏主包 `SlayTheSpire2.pck`（`PckArchive.Open`，`FileShare.Read | FileShare.Delete`）提取抽查集合；再逐个只读打开未隔离的原生加载 Mod PCK，仅针对抽查集合做 `O(N)` 存在性查表与便宜元数据比对，仅对命中冲突的路径提前计算游戏侧与 Mod 侧 SHA-256，完成后立即释放全部 PCK 文件句柄。
  3. **主线程核对与记日志（`CallDeferred` 回切）**：若后台零冲突，直接打汇总行（主线程零 VFS 读取）；若存在嫌疑路径，仅对这少数冲突路径调用只读 `Godot.FileAccess.FileExists` + `Godot.FileAccess.GetFileAsBytes`（不实例化场景/贴图、不写任何文件、不加载或执行任何第三方 DLL），比对 SHA-256 给出 `确认` 或 `嫌疑`，最后按约定格式记日志。全程外层捕获异常，自检自身报错只打 `ModLog.Warn`，绝不影响游戏运行。

---

## 三、抽查范围与判定规则

### 3.1 抽查三类原版关键资源（动态从当前游戏 PCK 推导，不写死版本）

1. **共享图集页**：直接复用 `SkinCatalog.IsSharedGameAtlasPath(path)`（涵盖 `card_atlas(_N)`、`relic(_outline)_atlas`、`epoch_atlas(_beta)` 的 `.png`/`.tpsheet` 及 `.import`/`.remap`）。
2. **选人头像规范入口**：直接复用 `SkinCatalog.IsCanonicalCharacterSelectPortrait(path)`（`res://images/packed/character_select/char_select_<角色>(_locked)?.(png|tres)(.import|.remap)?`）。
3. **先古模板**：同时覆盖正式版与测试版候选路径（`res://images/atlases/compressed.sprites/card_template/ancient_card_*.tres`、`res://images/atlases/compressed_atlas.sprites/ancient_*.tres`、`res://images/compressed_ui/ancient_*.png(.import)`）。
- **两跳依赖展开**：从 `.tres` / `.import` / `.remap` 文本中解析 `res://...` 路径并最多展开两跳，自动把 `.tres → compressed(_0|_atlas).png.import → res://.godot/imported/*.ctex` 以及 `.import → res://.godot/imported/*.ctex` 纳入抽查集。
- **真实游戏主包抽查规模（只读实测）**：
  - 正式版 `v0.111.1` `SlayTheSpire2.pck`（15,891 项）→ 抽查 **54 项**（图集 12、选角头像 36、先古模板及依赖 6）；
  - 测试版 `v0.113.1` `SlayTheSpire2.pck`（17,369 项）→ 抽查 **70 项**（图集 12、选角头像 44、先古模板及依赖 14）。

### 3.2 托管包过滤与两阶段判定

- **托管包过滤**：被 `ManagedSkinModLoader.TryManage` 拦截隔离的皮肤包（位于 `ManagedModInstances` 中，或热注册工坊资源包；若反射不可用则回退为 `ProviderRoots` 减去 `PartTakeoverRoots` 与 `SkinProvidersBeforeSelf`）**直接跳过不报**；排在切换器之前提前被游戏原生加载的包（`SkinProvidersBeforeSelf`）、部分接管且原生挂载的包（`PartTakeoverRoots`）以及其他普通原生加载 Mod 均纳入检查。
- **第一阶段（便宜元数据比对）**：在未隔离的原生 Mod PCK 中查找与抽查集合同名的规范路径，依次比较：
  1. 文件大小（`Size`）；
  2. `.import`/`.remap`/`.tres` 指向签名（解析出的规范 `res://...` 目标集合）；
  3. PCK 头内 16 字节 MD5（仅当双方均为非全零有效 MD5 时判定相等；若任一方 MD5 为全零，不贸然判相等，转入第二阶段实际字节 SHA-256 核对）。
  - 大小、指向与非全零 MD5 全部一致视为原版未改动副本，不误报；否则进入嫌疑列表。
- **第二阶段（哈希与运行态确认，仅对有嫌疑的路径执行）**：
  - 分别计算游戏主包原版字节 SHA-256 与各嫌疑 Mod PCK 字节 SHA-256（若实际字节与原版一致则剔除误报）。
  - 通过 `readLiveBytes`（运行时为主线程 `Godot.FileAccess.GetFileAsBytes`）读取运行态 VFS 实际字节并算 SHA-256：
    - 若无法读取运行态字节，或运行态字节仍与原版一致 → 所有冲突候选维持 **`嫌疑`**；
    - 若运行态字节异于原版且命中某嫌疑包的 SHA-256（多个包字节相同时取加载顺序靠后者，匹配 Godot `LoadResourcePack` 后加载覆盖语义）→ 该生效包标为 **`确认`**，其余同路径不同内容候选保持 **`嫌疑`**。
- **日志格式**：
  - 冲突明细（每条一行）：`[完整性自检] 嫌疑|确认 <路径> ← <包名>(<Mod ID>)`
  - 末尾汇总（始终输出；无冲突时**仅输出此行**）：`[完整性自检] 检查 N 项，嫌疑 X，确认 Y，耗时 Z ms`

---

## 四、离线测试覆盖与验证结果

### 4.1 离线测试（`VanillaIntegritySelfCheckTests`）

1. **无冲突**：无关路径 + 原版字节完全一致副本不触发报警；验证 `FormatLogLines` 仅输出 1 行汇总。
2. **只有嫌疑**：覆盖「无运行态读取」「运行态仍为原版字节」「零 MD5 同尺寸被篡改（命中嫌疑）vs 零 MD5 同字节原样拷贝（不误报）」三种场景，核对明细与汇总格式。
3. **确认归因**：双原生包同路径冲突且后加载包 B 生效，验证 B 标 `确认`、A 标 `嫌疑` 且计数与日志行精确一致。
4. **托管包过滤**：验证 `IsManagedBySkinChanger: true` 的包即使含冲突路径也不报；混合场景下仅报原生加载包。
5. **双分支先古模板**：验证正式版旧路径（`compressed.sprites/card_template/` → `compressed.png.import` → `.ctex`）与测试版新路径（`compressed_atlas.sprites/` → `compressed_atlas.png.import` → `.ctex`）均可两跳展开并确认。
6. **真实 `PckArchive` 磁盘端到端**：在临时目录用 `PckArchive.Write` 生成真实 `game.pck` / `bad_native.pck` / `managed_skin.pck`，调 `ScanArchives` 验证仅抽查关键类别（无关场景不纳入）、确认冲突原生包、跳过托管包。
7. **主菜单补丁挂接**：双分支验证 `VanillaIntegrityMainMenuReadyPatch` 可干净挂接 `NMainMenu._Ready`。

### 4.2 双分支构建与门禁实测（总耗时 53.5 秒）

> 本机 PowerShell 默认执行策略为 `Restricted`（不绕过策略）；已按 `tools/run-gates.ps1` 同等流程逐项在双分支实跑全部构建与测试命令，并保留 `tools/run-gates.ps1` 原样未改（仍为 UTF-8 带 BOM + LF），建议 Claude 在允许本地脚本的环境用 `powershell -ExecutionPolicy Bypass -File STS2-SkinChanger_修改件/tools/run-gates.ps1` 终验一次。

| 分支 | 配置 | 主工程构建（`-o %TEMP%`） | `LogicTests` | 无参 `RuntimeTests`（34 项） | `SEPARATE` 3 条无外部包单项 |
|---|---|---|---|---|---|
| Stable（v0.111.1） | `Release` | **0 警告 / 0 错误** | **通过** | **通过**（34 项：33 PASS / 1 SEPARATE / 0 FAIL / 0 GAP；末行唯一一次 `Workshop policy tests passed.`） | **3/3 通过** |
| Beta（v0.113.1） | `ReleaseBeta` | **0 警告 / 0 错误** | **通过** | **通过**（34 项：33 PASS / 1 SEPARATE / 0 FAIL / 0 GAP；末行唯一一次 `Workshop policy tests passed.`） | **3/3 通过** |

- **合并双分支 34 项清单统计**：`PASS 31 + PASS+EXT 3`，`FAIL 0 / NOT RUN 0 / GAP 0`（新增第 34 项 `vanilla-integrity-self-check` 双分支均为 `PASS`）。

---

## 五、需实机确认的事项

离线测试无法模拟真实游戏内的 `ModManager.Mods` 完整启动态与 Godot VFS 文件句柄，建议实机确认以下 3 项：

1. **正常加载顺序（切换器排首位、无污染原生包）**：进入主菜单等待 1 秒后查 `godot.log`，确认只出现一条 `[完整性自检] 检查 <54 或 70> 项，嫌疑 0，确认 0，耗时 <Z> ms`，且主菜单打开无可见卡顿。
2. **制造冲突对照（例如把含原版共享图集或选角头像的皮肤包故意排在切换器前面，或启用一个原生覆盖共享图集的包）**：进入主菜单后查 `godot.log`，确认出现 `[完整性自检] 确认 <路径> ← <包名>(<Mod ID>)`（准确点名该包），且汇总行 `确认 >= 1`。
3. **托管包免报对照**：将同一个带共享图集的皮肤包移回切换器后面重启，确认日志回到仅 1 条汇总行（`嫌疑 0，确认 0`），证明被切换器隔离托管的包不会误报。

---

## 六、复核后修改（响应 GPT `#297` / Claude `#298`）

针对交叉复核提出的两项问题，在 `STS2SkinChanger/Core/VanillaIntegritySelfCheck.cs` 与 `tests/STS2SkinChanger.RuntimeTests/VanillaIntegritySelfCheckTests.cs` 完成以下整改（零改动禁改文件，未触碰 `SkinService.cs`）：

### 6.1 消除主线程磁盘 I/O 与单帧大文件卡顿

1. **主线程彻底零磁盘 I/O**：
   - `BeginOnMainThread()` 现在只做纯内存快照（`OS.GetExecutablePath()`、`GameCommandLine.Arguments()`、`ModManager.Mods` 内存字段及 `SkinService` 已挂载覆盖路径），不再在主线程调用 `GamePackLocator.Resolve(...)`、`SkinPackagePaths.Resolve(...)` 或 `File.Exists(...)`。
   - `GamePackLocator.Resolve(...)` 与原生 Mod `<id>.pck` 的 `File.Exists(...)` 全部移至 `Task.Run` 后台线程执行。
2. **主线程 VFS 核对改为「先查长度 + 分帧分块 + 硬预算封顶」**：
   - 引入 `IVanillaIntegrityVfsReader` / `IVanillaIntegrityVfsHandle` 与可测状态机 `VanillaIntegritySelfCheck.ConfirmSession`，运行时由 `GodotVfsReader`（`Godot.FileAccess.Open` + `GetLength()` + `Seek()` + `GetBuffer()`）按帧打开并在帧末立即 `Dispose()`（帧间零常驻文件句柄），配合 `IncrementalHash.CreateHash(HashAlgorithmName.SHA256)` 跨帧增量哈希。
   - 默认预算（`VanillaIntegritySelfCheck.DefaultBudget`）：
     - `DefaultMaxConfirmFileBytes = 32 MiB`（单文件长度超限直接跳过不读，保持 `嫌疑`）；
     - `DefaultMaxTotalConfirmBytes = 64 MiB`（本次自检累计 VFS 确认总量超限后，后续超额文件跳过不读，保持 `嫌疑`）；
     - `DefaultPerFrameByteBudget = 2 MiB`（单帧读取上限）；
     - `DefaultChunkBytes = 256 KiB`（单次 `GetBuffer` 分块粒度）；
     - `DefaultPerFrameTimeBudgetMs = 2.5 ms`（每读完一个 256 KiB 分块即核对本帧耗时，触及上限立即 `CallDeferred()` 让出至下一帧）。
   - 若文件在分帧读取期间长度发生变化或读失败，立即释放哈希状态并保守维持 `嫌疑`。
3. **汇总行新增主线程累计耗时与帧数**：
   - 格式更新为：`[完整性自检] 检查 N 项，嫌疑 X，确认 Y，耗时 Z ms，主线程 M ms（F 帧）`（无冲突时仍仅输出这一行汇总）。
4. **最坏情况主线程单帧耗时离线实测与估算**：
   - 针对真实测试版 `SlayTheSpire2.pck` 中最大共享图集页 `card_atlas_0`（`16,418,356 B` ≈ 15.66 MiB），按同等 `2 MiB/帧 + 256 KiB/块 + 2.5 ms/帧` 切分做 3 轮只读实测：**8 帧**完成，总计 `10.13–10.38 ms`，单帧均值 **`1.27–1.30 ms`**，单帧峰值 **`1.38–1.57 ms`**。
   - 叠加 Godot `FileAccess` C# 封送开销及极端冷盘抖动：
     - **零冲突常态**：2 帧，主线程累计 **< 1 ms**；
     - **命中大文件（如 15.7 MiB 图集页）热缓存**：约 8 帧（双图集页 30.4 MiB 约 16 帧），单帧 **~1.5–2.5 ms**；
     - **冷盘/IO 抖动最坏单帧**：每 256 KiB 分块触发一次 `2.5 ms` 软墙检查，最多多跑完当前这一个 256 KiB 分块即让出，单帧最坏 **~2.5–3.5 ms**（不会再出现单帧 16–30 MiB 阻塞）。

### 6.2 严格按原生加载规则过滤候选 + 排除切换器自挂载覆盖

1. **严格匹配游戏原生加载器规则（`ResolveNativeModSources`）**：
   - 仅接纳 `mod.state == ModLoadState.Loaded`、`mod.manifest?.hasPck == true`、非自 Mod、未被切换器托管，且磁盘存在精确路径 `Path.Combine(mod.path, modId + ".pck")` 的包；彻底移除 `SkinPackagePaths.Resolve` 的回退逻辑，杜绝 `has_pck=false` 残留 `.pck` 或非 `<id>.pck` 文件抢占 `确认` 归因。
2. **排除切换器自挂载覆盖与不确定来源（不改 `SkinService.cs`）**：
   - 通过只读反射绑定 `SkinService.Sync`、`SkinService.MountedOverlayOwners` 与 `SkinService.RuntimeCanonicalDependencyPaths`（在 `BeginOnMainThread` 做启动快照，且每帧确认前对当前嫌疑路径做 `O(K)` 增量补查，防自检期间新换肤导致竞态；`Monitor.TryEnter(sync, 2 ms)` 避免阻塞主线程）。
   - 凡命中切换器自挂载路径、反射字段缺失或锁争用导致自挂载记录不完整、VFS 不可读/超预算/中途变更者，**一律保守降级为 `嫌疑`，绝不误报 `确认`**。

### 6.3 新增回归测试与复验结果

- 在 `VanillaIntegritySelfCheckTests` 新增并跑通以下回归用例：
  1. `VerifyNativeModSourceRulesExcludeInactiveHasPckFalseAndLegacyFallback`（复现 GPT 的 A/B 场景：A 为 `Loaded + has_pck=true + mod.a.pck`，后加载的 B 为 `Loaded + has_pck=false` 带同字节残留 `mod.b.pck`，C 为 `has_pck=true` 仅含旧命名 `legacy_stem.pck`，D 为未加载态；断言仅 A 进入候选并获 `确认`，B/C/D 均被剔除）。
  2. `VerifySelfOverlayExclusionDowngradesToSuspect`（命中 `selfOverlayPaths` 时降级为 `嫌疑`、未命中路径仍正常 `确认`；`selfOverlayIncomplete=true` 时零 VFS 读取且全部降级 `嫌疑`）。
  3. `VerifyBudgetedMultiFrameConfirmationAndOverBudgetFallback`（验证分帧字节预算、单文件/累计总预算超限跳过不读并维持 `嫌疑`、中途出现自挂载立即中止并降级 `嫌疑`、以及 `PerFrameTimeBudgetMs` 命中后每帧仅读 1 个分块的时间截断）。
  4. `VerifyMainMenuPatchAndSelfOverlayHooksBindCleanly`（双分支验证 `NMainMenu._Ready` 补丁及 `SkinService` 三个只读反射字段均可正常绑定）。
- **双分支门禁复验（输出目录 `%TEMP%\sts2-gates-gemini`，总耗时 52.0 秒）**：
  - Stable（`Release`，v0.111.1）：主工程 / `LogicTests` / `RuntimeTests` 均 **0 警告 / 0 错误**，`LogicTests`、`--test-vanilla-integrity`、无参全套 `RuntimeTests`（末行唯一 `Workshop policy tests passed.`）及 3 条无外部包 `SEPARATE` 单项全部通过。
  - Beta（`ReleaseBeta`，v0.113.1）：同上全部 **0 警告 / 0 错误**、全部通过。
  - 34 项清单统计：`PASS 31 + PASS+EXT 3`，`FAIL 0 / NOT RUN 0 / GAP 0`（本地 PowerShell 策略为 `Restricted`，已按 `tools/run-gates.ps1` 同等流程跑完，待 Claude 在允许执行脚本的环境跑 `run-gates.ps1 -Out $env:TEMP\sts2-gates-gemini` 终验）。

