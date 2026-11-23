# 机制收敛第 1 项第一步：延迟任务「执行记录 + 崩溃面包屑」交付记录

- **工作树 / 分支**：`D:\mod\工作2\上游修复_机制1调度记录\STS2-SkinChanger_修改件`（分支 `perf/mech1-dispatch-records-20261123`，基线 `ff27723`）
- **方案依据**：`D:\mod\工作2\docs\handoff\切换卡顿与可靠性讨论汇总_20261123.md` 第 4 节第 1 项与第 7 节约束清单（任务 `#334` / `#336`）
- **第一步范围**：**只做记录、重入观察与崩溃面包屑，零执行时机改动**（不并入统一 Drain、不做帧预算、不去重合并、不截断重试、不改变异常传播）。
- **边界确认**：未触碰 `Core/ManagedSkinModLoader.cs`、`Core/SkinService.cs`、`Core/SkinService.RuntimeSafety.cs`、`Core/RuntimeLoadSafety.cs`、`Ui/ContextualSkinControls.cs`；未执行 `git commit`、部署或启动游戏。

---

## 1. 包装器 API（`STS2SkinChanger/Core/DeferredDispatch.cs`）

新增纯记录型静态入口 `STS2SkinChanger.Core.DeferredDispatch`：

| 入口 / 类型 | 用途与语义 |
| --- | --- |
| `DeferredDispatchRisk` | `Normal`（仅写内存环、按批/退出刷盘）与 `HighRisk`（原生闪退先例或高风险回调：执行前立即写盘 `ENTER`，完成后写盘 `DONE`/`FAIL`） |
| `CallDeferred(string taskName, object? owner, Action action, DeferredDispatchRisk risk = Normal)` | 在**原调用点**直接调用 `Callable.From(wrapped).CallDeferred()` 入队，保持与第三方 `CallDeferred` 的原始 FIFO 穿插顺序与 Critical 嵌套层级不变；线程安全，可从后台线程（如 `VanillaIntegritySelfCheck`）调用 |
| `WrapCallback(string taskName, object? owner, Action action, DeferredDispatchRisk risk = Normal, ulong? enqueueFrameOverride = null)` | 用于 `SceneTreeTimer.Timeout` 等非 `CallDeferred` 回调，不改变定时器触发时机 |
| `RunTracked(string taskName, object? owner, Action action, DeferredDispatchRisk risk = Normal, ulong? enqueueFrameOverride = null)` | 用于 `ProcessFrame` 轮询或循环体内单次受控步骤（如 `HalfHpCrabSkinPatch` 每 60 帧检查、`DeferredProviderActivator` 单个提供者激活） |
| `FlushBreadcrumbs()` | 把内存中未刷盘的记录按序号落盘；已接入 `CrashDiagnostics` 的正常退出、强制退出、进程退出（`AppDomain.ProcessExit`）与托管终止异常路径 |
| `SnapshotRecords()` | 线程安全读取内存环形缓冲快照（按时间/序号正序） |
| `ConfigureForTests(...)` / `ResetDefaultsForTests()` | 仅测试使用的队列/帧号/目录/告警/上限接缝，默认直接绑定 Godot `Callable.From(...).CallDeferred()`、`Engine.GetProcessFrames()`、`CrashDiagnostics.SessionDirectory` 与 `ModLog.Warn` |

### 记录字段与行为不变量

- **单条记录字段（`DeferredDispatchRecord`）**：`Sequence`、`TimestampUtc`、`Phase`（`ENTER` / `DONE` / `FAIL`）、`TaskName`、`Owner`（`GodotObject` 格式化为 `TypeName#InstanceId`，普通对象为 `TypeName@IdentityHash`，字符串直出，`null` 为 `global`）、`EnqueueFrame`、`ExecuteFrame`、`StreakCount`（同一 `(TaskName, Owner)` 连续重入次数）、`Result`（`running` / `ok` / `error:<Type>:<Message>`）、`ElapsedMs`、`Risk`。
- **异常语义完全不变**：回调体内 `catch (Exception) { ...; throw; }` 原样重抛，不吞异常、不二次包装、不额外刷错误日志；`finally` 中记录与刷盘异常内部吞掉，绝不掩盖原异常。
- **只观察、不干预的重入告警**：同一 `(TaskName, Owner)` 通过栈内自重入（回调执行期间再次入队自身）或连续帧（间隔 `<= 2` 帧）累加 `StreakCount`；当 `StreakCount > 50`（`DefaultReentrancyWarnThreshold = 50`）时，每 `(TaskName, Owner)` 仅发**一次**去重后的 `ModLog.Warn("[延迟调度] 任务连续重入超过阈值（只观察、不截断）...")`，任务照常继续执行。无延迟任务触发或正常单次任务开销接近零。

---

## 2. 面包屑文件位置、格式与上限

- **文件路径**：`<SkinChangerStorageRoot>/diagnostics/<session>/deferred-breadcrumbs.log`（位于 `CrashDiagnostics.SessionDirectory`，即 `_report` 会话目录，与 `session.json`、`managed-events.log`、`diagnostic.txt` 同级，受既有 `CrashDiagnostics.Prune` 会话清理与字节总预算统一管辖）。
- **单行格式**（UTF-8 无 BOM，纯 LF，便于人工和脚本检索）：
  ```text
  2026-11-23T14:50:12.3456789Z #12 [ENTER][HIGH] task=OstyModelPreview.FitModel owner=OstyModelPreviewSurface#12345 enq=420 run=421 streak=1 ms=0.00 res=running
  2026-11-23T14:50:12.3471234Z #13 [DONE][HIGH] task=OstyModelPreview.FitModel owner=OstyModelPreviewSurface#12345 enq=420 run=421 streak=1 ms=1.45 res=ok
  ```
- **分层落盘与上限**：
  - **内存环形缓冲**：默认上限 **256 条**（`DefaultRingBufferCapacity = 256`）。
  - **`HighRisk` 任务**：回调执行**前**先将前置未刷盘记录 + 本次 `ENTER` 行写入文件并 `Flush()`，执行结束（完成或异常）后再写入 `DONE`/`FAIL` 并 `Flush()`。原生崩溃瞬间最后一个正在执行的在途回调必然留在磁盘文件末尾。
  - **`Normal` 任务**：默认只写内存环（每次调用零磁盘 I/O）；每累计 **32 条**（`DefaultFlushEveryRecords = 32`）或遇到 `HighRisk` 刷盘、正常/强制退出、`AppDomain.ProcessExit`、托管终止异常时一并按序号落盘。
  - **文件大小上限**：默认 **64 KiB**（`DefaultMaxBreadcrumbFileBytes = 65536`）；超过上限时自动裁切，仅保留最新尾 **128 行**（`DefaultKeepTailLinesOnTrim = 128`，若单行较长则进一步裁剪至字节预算内）并通过临时文件原子替换。
  - **`diagnostic.txt` 附带条数**：存在 `deferred-breadcrumbs.log` 时，最多附带最后 **20 条**非空记录（`延迟任务面包屑（最近 N 条）：`）。

---

## 3. `STS2SkinChanger/Diagnostics/CrashWatch.ps1` 改动说明（#336 重点审阅项）

- **为什么要改 `CrashWatch.ps1`**：
  - 本项目的崩溃诊断分两条生成 `diagnostic.txt` 的路径：
    1. 托管进程内的 `CrashDiagnostics.RecordManaged("terminating", ...)`（只处理托管未处理异常）；
    2. 独立 Windows PowerShell 看门进程 `Diagnostics/CrashWatch.ps1`（内嵌资源），在游戏进程退出后汇总退出码、Windows 事件日志、dump 路径、托管异常日志并写出最终 `diagnostic.txt` 且追加回 `godot.log`。
  - **原生闪退（`SIGSEGV` / `EXCEPTION_ACCESS_VIOLATION` / Spine 原生崩）发生时，游戏进程当场终止，C# 侧的 `RecordManaged` 根本没有机会执行**——此时唯一会生成 `diagnostic.txt` 的就是 `CrashWatch.ps1`。如果不让看门进程读取 `_report\deferred-breadcrumbs.log`，高风险回调提前落盘的 `ENTER` 记录就无法出现在原生崩溃报告的 `diagnostic.txt` 中。
- **具体改了什么（共 2 处，合计 +11/-1 行，纯 LF 无 BOM）**：
  1. `CrashWatch.ps1:128-137`：在读取 `$managed`（`managed-events.log`）之后，检查 `$breadcrumbsPath = Join-Path $report 'deferred-breadcrumbs.log'` 是否存在；若存在，用 `[IO.File]::ReadAllLines($breadcrumbsPath, $utf8)` 过滤空行并 `Select-Object -Last 20`，拼出 `$breadcrumbSection = "\`n延迟任务面包屑（最近 N 条）：\`n..."`；读取包在 `try { ... } catch { }` 内，任何读取失败静默降级为空串，不影响原有崩溃诊断流程。
  2. `CrashWatch.ps1:173`：在 `$summary` 模板的 `托管异常：$managed` 后紧接拼接 `$breadcrumbSection`；当 `deferred-breadcrumbs.log` 不存在或为空时，`$breadcrumbSection` 为空串，输出文本与修改前**完全一致**。
- **静态验证**：已使用 PowerShell `System.Management.Automation.Language.Parser.ParseFile` 静态解析确认 `0` 语法错误，并确认无多余 BOM、纯 LF，且在 `DeferredDispatchTests.VerifyCrashWatchEmbedsBreadcrumbSupport` 中校验了内嵌资源包含 `deferred-breadcrumbs.log` 与 `Select-Object -Last 20`。（注：`--test-crash-diagnostics` 会拉起 `powershell.exe -ExecutionPolicy Bypass` 执行看门脚本，本机侧按安全红线未自行执行，请 Claude 审阅 `CrashWatch.ps1` 时补跑 `--test-crash-diagnostics`。）

---

## 4. 第一步迁移点清单（8 个文件，共 11 处调用点，含 6 项元数据）

每处调用点在源码上方均已标注对应 `[延迟调度]` 6 项注释；以下为汇总表（本步全部保持原执行时机与原重试/取消逻辑不变）：

| # | 文件与函数 | 任务名（归属） | 档位 / 风险 | 1. 原触发时机 | 2. 最早可执行帧 | 3. 顺序依赖 | 4. 能否合并 | 5. 重试上限 | 6. 取消条件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | `Core/Features/Ancient/AncientCompendium.cs` (`ScheduleDeferredAttach`) | `AncientCompendium.Attach` (`compendium`) | `CallDeferred` / `Normal` | `Attach` 见锚点按钮或宿主容器不可见时重试 | 入队帧末 idle / 下一帧 | 需等图鉴子界面 `_Ready` 且可见 | 可合并（`state.DeferredScheduled`） | 12 次 | `!IsInstanceValid(compendium)` 或挂载成功 |
| 2 | `Core/Features/Ancient/BestiaryEntranceSwitcher.cs` (`RememberReleased`) | `BestiaryEntranceSwitcher.ClearLastReleased` (`control`) | **Critical** `CallDeferred` / `Normal` | 按钮 `Released` 时立即排入清零 `_lastReleased` | **本帧末 idle（不可推迟）** | **Critical**：须在同帧 `TryRedirectBestiaryOpen` 后、下帧点击前清零，原位直排 | 不可合并 | 1 次 | `!ReferenceEquals(_lastReleased, control)` |
| 3 | `Core/Features/Ancient/BestiaryEntranceSwitcher.cs` (`ScheduleScan`) | `BestiaryEntranceSwitcher.ScanCompendium` (`compendium`) | `CallDeferred` / `Normal` | `ScanCompendium` 末尾继续扫描晚注入的怪物图鉴入口 | 入队帧末 / 下一帧 | 等待其他 UI Mod 晚注入按钮入树 | 可合并（`_scanScheduled`） | 16 次 | `!IsInstanceValid(compendium)` 或达上限 |
| 4 | `Ui/OstyModelPreviewControls.cs` (`ScheduleFit`) | `OstyModelPreview.FitModel` (`this`) | `CallDeferred` / **`HighRisk`** | 多帧等待 Spine 骨架建好取包围盒，每 6 次调 `update_skeleton(0f)` | 下一帧 | 预览节点入树后逐帧推进，涉及原生 Spine 调用先落盘 `ENTER` | 同实例单链串行 | 30 次（超限改保守默认缩放） | `!Alive() \|\| _spine == null \|\| !IsInstanceValid(_spine)` 或 `FitModel()` 成功 |
| 5 | `Core/VanillaIntegritySelfCheck.cs` (`ScheduleOnce`) | `VanillaIntegritySelfCheck.Begin` (`"global"`) | `CallDeferred` / `Normal` | 主菜单首次 `_Ready` 排到帧末抓纯内存快照 | 本帧末 idle | 主线程读取 `ModManager.Mods` 与内存覆盖映射 | 一次性（CAS 防重） | 1 次 | 执行完毕或异常跳过 |
| 6 | `Core/VanillaIntegritySelfCheck.cs` (`BeginOnMainThread` `Task.Run`) | `VanillaIntegritySelfCheck.Step` (`runtimeState`) | 跨线程 `CallDeferred` / `Normal` | 后台 `Task.Run` 完成磁盘 PCK 预扫后切回主线程 | 切回主线程后的下一 idle 帧 | **线程安全入队**；主线程访问 Godot VFS | 单次交接不可合并 | 由下游按项数分帧收敛 | 核验完毕或异常释放 |
| 7 | `Core/VanillaIntegritySelfCheck.cs` (`RuntimeConfirmRunner.StepOnMainThread`) | `VanillaIntegritySelfCheck.Step` (`this`) | 多帧 `CallDeferred` / `Normal` | 单帧 2.5 ms / 2 MiB 预算耗尽且仍有未检项 | 下一帧 | 主线程单链串行推进同一 `ConfirmSession` | 单链串行 | 按待检条目总数有界收敛 | `completed == true` 或异常释放 `_session` |
| 8 | `Ui/ScrollListRebuild.cs` (`PlaceAfterHeader` 外跳) | `ScrollListRebuild.QueueSort` (`scroll`) | **Critical** `CallDeferred` / `Normal` | 调整子节点顺序后第一跳执行 `QueueSort()` | 本帧末 idle | **Critical**：必须先完成本帧 `QueueSort()` 再排第二跳 | 不可跨跳合并（`GenerationMeta` 淘汰旧代） | 1 次 | `!IsInstanceValid(scroll)` 或 `GenerationMeta` 变化 |
| 9 | `Ui/ScrollListRebuild.cs` (`PlaceAfterHeader` 内跳) | `ScrollListRebuild.RestoreScroll` (`scroll`) | **Critical** `CallDeferred` / `Normal` | 第一跳 `QueueSort()` 后嵌套排入恢复 `ScrollVertical` | 第一跳之后的下一 idle 步 | **Critical**：必须严格晚于第一跳，否则 Godot 将滚动值夹零 | 不可与第一跳合并 | 1 次 | `!IsInstanceValid(scroll)` 或 `GenerationMeta` 变化 |
| 10 | `Ui/EmperorCrabCompat/HalfHpCrabSkinPatch.cs` (`StartInstallRetry`) | `HalfHpCrabSkinPatch.InstallRetry` (`"EmperorCrabCompat"`) | `ProcessFrame` / `Normal` | 皮肤行为 DLL 未加载时订阅 `ProcessFrame`，每 60 帧检查一次 | 订阅后第 60 帧（每 60 帧一次） | 主线程检查已加载程序集并安装 Harmony 前缀 | 单例重试（`_retryTree != null`） | 300 次（18000 帧 / 约 5 分钟） | `_halfHpPrefixInstalled \|\| _halfHpPrefixMissing \|\| _retryFrames >= 18000` 或异常 `StopInstallRetry()` |
| 11 | `Core/DeferredProviderActivator.cs` (`EnsureTimer` + `Tick`) & `Core/SkinPreloader.cs` (`EnsureTimer`) | `DeferredProviderActivator.Tick` (`"global"`, `Normal`) + `DeferredProviderActivator.Activate` (`providerId`, **`HighRisk`**)；`SkinPreloader.Tick` (`"global"`, `Normal`) | `CreateTimer` / `Normal` + **`HighRisk`** | 启动延后 DLL 初始化（0.12 s 周期）与后台资源预加载（0.15 s 周期） | 定时器到期帧（0.12 s / 0.15 s） | 主线程单定时器串行；`DeferredProviderActivator.Activate` 调第三方 DLL 初始化器，标记 `HighRisk` 先写盘 `ENTER` | `_timerRunning` 单定时器串行 | 有界队列（预加载上限 48 项、启动门禁 60 s；单项失败不重试） | 队列清空、`Cancel()` / `Reset()` / `_cancelled` |

---

## 5. 排除项与暂未迁移项说明

- **`Ui/ContextualSkinControls.cs`（`ApplyProviderVariantLabels`）**：按 `#334` 明确排除——白猫正在本地修改该函数（小红帽无限重试修法 `allowRetry: false`），避免合并冲突，待上游合入后再迁移。
- **未在第一步范围的其余 2 类 Critical**（`Core/FrameworkModelPreview.cs:87`、`Core/Features/Merchant/MerchantRuntimeAppearance.cs:269,494`）以及第 2 批（手写去重刷新）与第 3 批（一次性布局/弹窗/退出）：按步骤规划留在后续批次，本步不改动。
- **并发工作树隔离文件**：`Core/ManagedSkinModLoader.cs`、`Core/SkinService.cs`、`Core/SkinService.RuntimeSafety.cs`、`Core/RuntimeLoadSafety.cs` 均零改动。

---

## 6. 测试覆盖与双分支门禁结果

新增 `tests/STS2SkinChanger.RuntimeTests/DeferredDispatchTests.cs`，已注册 `--test-deferred-dispatch`、全量 `Gate.Run("DeferredDispatchTests", ...)`、`tests/required-fixes.json`（`deferred-dispatch-records`，清单总数 37 项）及 `docs/agents/maintenance-rules.md`。

### 单元测试项（`DeferredDispatchTests`）

1. **穿插顺序 + Critical 嵌套两跳**：`OursA -> ThirdParty -> OursB` 严格保序；`OuterHop -> ThirdPartyBetweenHops -> InnerHop` 绝不把第二跳提前吞入第一轮。
2. **跨帧推进（龙娘失败场景 3）**：第 3 帧才就绪的模型在第 1/2 帧穿插第三方回调，验证首帧未耗光、第三方顺序未反转、第 3 帧精确成功，且 `EnqueueFrame` / `ExecuteFrame` / `StreakCount` 准确。
3. **异常语义一致**：回调抛出的异常实例原样传播，不吞、不包装、不触发额外告警，记录标为 `FAIL`。
4. **面包屑与上限**：普通任务未达阈值不写盘；`HighRisk` 任务在回调体执行前即把前置记录 + `ENTER` 落盘（模拟回调中途致命异常）；完成后补写 `FAIL`/`DONE`；验证内存环上限、文件字节裁切上限保留尾部新记录、以及 `CrashDiagnostics.FormatRecentBreadcrumbsSection` 最多取最后 20 条。
5. **连续重入告警**：默认阈值 50；超阈值只告警一次（含任务名、归属、次数）且不截断执行。
6. **后台线程安全入队**：8 个工作线程并发入队 128 个任务零丢失。
7. **内嵌 `CrashWatch.ps1` 检查**：校验内嵌看门脚本包含 `deferred-breadcrumbs.log` 与 `Select-Object -Last 20`。

### 双分支构建与测试结果（产物隔离至 `%TEMP%\sts2-gates-gemini-mech1`）

| 检查项 | Stable (`Release`, v0.111.1) | Beta (`ReleaseBeta`, v0.113.1) | 备注 |
| --- | --- | --- | --- |
| `required-fixes.json` Logic 类自检 | PASS（37 项） | PASS（37 项） | — |
| 4 工程构建（Compat / Mod / LogicTests / RuntimeTests） | PASS（0 警告 / 0 错误） | PASS（0 警告 / 0 错误） | — |
| `LogicTests` | PASS | PASS | — |
| `RuntimeTests --test-deferred-dispatch` | PASS | PASS | 全部 7 组断言通过 |
| `RuntimeTests` 无参全量 | PASS | PASS | `total=37, PASS=35, FAIL=0, NOT RUN=0, SEPARATE=2, GAP=0`，尾行 `Workshop policy tests passed.` |
| 全部 5 个无参 `SEPARATE` 项 + `--test-runtime-safety` / `--test-preview-binding` | PASS | PASS | 逐项通过 |
| `run-gates.ps1` / `check-protected-characters.ps1` / `--test-crash-diagnostics` | 交由 Claude 复跑 | 交由 Claude 复跑 | 按本机红线不绕过 PS 执行策略且不加载第三方 Mod DLL |

---

## 7. 给云的实机验证方法

1. 部署构建产物后正常启动游戏进入主菜单，等待几秒（触发 `VanillaIntegritySelfCheck`、`DeferredProviderActivator`、`SkinPreloader`）。
2. 在游戏内依次做几项常规操作：
   - 进入选人界面切骨妹（触发 `OstyModelPreview.FitModel` 高风险 Spine 预览拟合，以及皮肤列表 `ScrollListRebuild` 两跳滚回）；
   - 打开图鉴、切换怪物图鉴/先古图鉴入口（触发 `AncientCompendium.Attach`、`BestiaryEntranceSwitcher.ScanCompendium` 与点击释放 `ClearLastReleased`）；
   - 正常游玩一小会并正常退出游戏。
3. 检查游戏日志与诊断目录：
   - 在 `godot.log` 中搜索 `[延迟调度]`：正常游玩应无异常报错或无端重入告警（若真遇到小红帽类无限循环则会只出现一次带任务名/归属/次数的 `WARN`）；
   - 打开本次会话目录 `<SkinChanger存储目录>\diagnostics\<本次session>\`，确认生成了 `deferred-breadcrumbs.log`（大小 `<= 64 KiB`），内容中能看到 `OstyModelPreview.FitModel` / `DeferredProviderActivator.Activate` 的 `[ENTER][HIGH]` 与 `[DONE][HIGH]` 成对记录，以及正常退出时刷盘的常规记录；
   - 核对画面与交互：怪物图鉴点击切换、滚动列表位置恢复、奥斯提预览缩放、主菜单启动流畅度均与之前完全一致。
