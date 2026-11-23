# 切皮肤加速第 1 批（A 子阶段计时 + B 精准提供者补丁发现）交付记录

- **工作树 / 分支**：`D:\mod\工作2\上游修复_切换优化AB\STS2-SkinChanger_修改件`（分支 `perf/switch-ab-20261123`，基线 `ff27723`）
- **对应方案**：`D:\mod\工作2\docs\handoff\切换卡顿与可靠性讨论汇总_20261123.md` 第 3 节 A/B 项、第 7 节 B 项及约束清单（任务 `#329`）
- **边界确认**：未触碰 `SkinService.RuntimeSafety.cs`、`WithPreparedRuntimeResources` 及运行时包缓存路径（留给 E 项）；未执行 `git commit`、部署或启动游戏。

---

## 1. A 项：子阶段计时（零行为改动）

所有新增耗时输出沿用现有 `[性能]` 格式并在原行末尾追加 `键=值`，不破坏既有字段解析。

### 1.1 提权包预检 vs 激活原生提供者

- 位置：`STS2SkinChanger/Core/SkinService.cs`（`ApplyVisualSelection` 中 `MountSelectedLargeRuntimeProviderPacks(..., beforeMount)` 入口，约 L6861）。
- 改动：在 `beforeMount` 回调首行插入 `Mark("提权包预检");`。
- 效果：`[性能] 外观重建合计(...)` 明细中，原混在「激活原生提供者」里的提权包规划、过滤与 `CheckMount(A10)` 拆分为独立段「提权包预检」，「激活原生提供者」只统计 `RefreshSkippedProviders` + `ActivateSelectedProviders`。

### 1.2 隔离阶段拆分（扫描 / 卸载 / 泄漏校验）

- 位置：`STS2SkinChanger/Core/ManagedSkinModLoader.cs`（`ActivateProvider`）。
- 新增独立明细行（在 `PerfTrace.Enabled` 时输出）：
  ```text
  [性能] 皮肤代码隔离(<provider>) = <总ms> ms；路径=<路径标签>，方式=<精准|全量>，扫描=<ms>ms(目标=<N>/发现=<M>/去重=<D>)，卸载=<ms>ms(数=<U>)，校验=<ms>ms(目标=<V>)
  ```
- 原有行 `[性能] 皮肤代码激活耗时(<provider>)` 末尾追加字段：
  `路径=<...>、方式=<...>、扫描=<ms>ms(目标=<N>)、卸载=<ms>ms(数=<U>)、校验=<ms>ms(目标=<V>)`。

### 1.3 本地化拆分（读表合表 vs 同步回调）

- 位置：`STS2SkinChanger/Core/LocalizationPerfProbe.cs`（新增）与 `STS2SkinChanger/Core/SkinService.cs`（`RefreshLocalizationIfNeeded`）。
- 机制：`LocalizationTriggerPerfProbePatch` 以 `[HarmonyPatch(typeof(LocManager), "TriggerLocaleChange")]` 挂接 `Prefix` / `Finalizer`，仅在 `LocalizationPerfProbe.BeginScope()` 激活的 `[ThreadStatic]` 作用域内累计 `TriggerLocaleChange` 耗时，不修改参数、不改返回值、不改变执行顺序；`Prepare()` 确认目标方法存在才安装（未来游戏若改名则自动跳过，日志降级显示 `回调=未拆分`）。
- 日志格式：
  ```text
  [性能] 本地化(<lang>) = <总ms> ms；方式=<整套重载|缓存复用>，读表合表=<ms>ms，回调=<ms>ms，触发=<皮肤列表|无(默认)>，语言文件=<N>
  ```
  - 缓存复用走 `SetLanguageInternal` 时，`读表合表` 接近 `0 ms`，`回调` 直接呈现 `TriggerLocaleChange`（含 `TranslationServer.SetLocale`、订阅者回调与同步 `GC.Collect`）真实占比；
  - 整套重载走 `SetLanguage` 时，`读表合表` 对应 `LoadTablesFromPath` 与内部建表，`回调` 对应 `TriggerLocaleChange`。

---

## 2. B 项：精准提供者补丁发现（与全量语义严格等价）

核心实现位于新增文件 `STS2SkinChanger/Core/ProviderPatchIndex.cs`，并接入 `ManagedSkinModLoader.ActivateProvider`。

### 2.1 四条路径及辅助路径的发现范围与理由

| 路径枚举 | 触发条件 | 发现方式与目标集合 | 理由 |
| --- | --- | --- | --- |
| `FirstActivation` | 未跳过初始化器（含首次激活或未建立完整蓝图），且初始化器未抛异常 | **全量扫描**（`Harmony.GetAllPatchedMethods()` 全集），并**保留初始化前全量预扫** | 首次激活时提供者可能通过非声明式反射动态装入任意目标补丁，必须全量抓全并据此建蓝图；同时保留初始化前完整条目集合，以备异常回滚归属判定。 |
| `RepeatActivation` | `skippedInitializer == true` 且 `!isAdoptedGameAssembly` 且 `ProviderPatchIndex` 持有该程序集的完整蓝图（`HasCompleteCoverage == true`） | **精准查询**：`blueprint.AllKnownTargets ∪ (Harmony.GetAllPatchedMethods() ∩ IsManagedCandidateTarget)` | `Harmony.GetAllPatchedMethods()` 仅返回内存键数组开销极小，真正耗时的是对无关 Mod 目标逐个执行 `Harmony.GetPatchInfo`（触发 `PatchInfoSerialization.Deserialize`）。精准模式仅对蓝图已知目标及已打补丁的受管引擎候选目标调用 `GetPatchInfo`，且对每个命中目标仍按 `Callback.Module.Assembly` 全员过滤四类回调（Prefix/Postfix/Transpiler/Finalizer），与全量完全等价。 |
| `DynamicOrIncompleteFallback` | 未命完整蓝图（例如动态提供者、`isAdoptedGameAssembly == true`、无蓝图或上次异常回滚使蓝图覆盖失效） | **全量扫描** | 无法保证蓝图已覆盖所有动态目标时无条件退回全量，确保不漏检。 |
| `InitializerFailure` | 提供者初始化器抛异常（`initializerFailed == true`） | **全量扫描** + **逐条集合对比**（`DidInitializerInstallPatches`） | 初始化中途抛错可能仅装了一半补丁、替换了既有同程序集回调、或与他方补丁共存于同一目标。绝不能仅比较数量，必须将初始化前后按 `(Target, Callback, Kind, Owner)` 逐条计数比较，判定是否确实新增了补丁；异常进入 `catch` 回滚时同步调用 `InvalidateCoverage()` 撤销不完整蓝图。 |
| `LeakVerification` | `managedPatches.Length > 0` 后的卸载后置校验 | **仅检查本次真实卸载的受管目标**（`unpatchedManagedTargets`） | 未被本次卸载的目标无需重查；当 `managedPatches.Length == 0` 时直接跳过整段校验。 |
| `DeactivationFallback` | 停用前抓全量（如 `FoolWaifu` 停用） | **全量扫描** | 动态提供者停用保留原有全量语义，不做激进收缩。 |

### 2.2 蓝图记录与空卸载跳过

- **蓝图内容（`AssemblyPatchTargetRegistry.AllKnownTargets`）**：每次激活捕获并分类完毕后，记录该程序集的：
  1. 本次实际捕获到的 `behaviorPatches`、`managedPatches`、`presentationPatches` 目标；
  2. 静态解析到的程序集声明式 `[HarmonyPatch]` 目标（`ResolveDeclarativePatchTargets`，兼容类级与方法级特性，无法静态解析时安全忽略而不降级安全性）。
- **空卸载跳过**：重复激活命中蓝图时，未重新执行初始化器且当前未处于已安装状态的 `runEnvironmentPatches`（通过本轮 `capturedPatchKeys` 对照过滤）不再重复调用 `UnpatchProviderCallbacks`，消除无效卸载与重编译。
- **反射接口兼容**：`ManagedSkinModLoader` 内部嵌套的 `ProviderPatch`（7 参数构造函数）、`ProviderPatchKind` 与 `UnpatchProviderCallbacks` 签名保持原样，不破坏既有测试（`CardPortraitSessionTests`、`CreatureVisualLifecycleTests`、`HinaUiOstyRegressionTests`）的反射调用。

---

## 3. 改动文件清单

| 文件路径 | 变更类型 | 说明 |
| --- | --- | --- |
| `STS2SkinChanger/Core/ProviderPatchIndex.cs` | 新增 | 四路径提供者补丁发现、每程序集蓝图索引、声明式目标解析、受管目标谓词、初始化前后逐条增量判定 |
| `STS2SkinChanger/Core/LocalizationPerfProbe.cs` | 新增 | `LocManager.TriggerLocaleChange` 零行为改动计时探针 |
| `STS2SkinChanger/Core/ManagedSkinModLoader.cs` | 修改 | 接入 `ProviderPatchIndex` 四路径、空 `runEnvironmentPatches` 过滤、按卸载目标精准泄漏校验、隔离阶段细分计时 |
| `STS2SkinChanger/Core/SkinService.cs` | 修改 | 仅改动 2 处：`beforeMount` 入口 `Mark("提权包预检")` 与 `RefreshLocalizationIfNeeded` 细分计时 |
| `tests/STS2SkinChanger.RuntimeTests/ProviderPatchCaptureTests.cs` | 新增 | 四路径精准/全量等价性、重复项计数、他方回调与优先级顺序保持、失败场景 #1（同程序集既有+他方共存+中途抛错+同数量替换+回滚）、声明式目标、泄漏校验、本地化探针、150 混淆方法离线基准 |
| `tests/STS2SkinChanger.RuntimeTests/Program.cs` | 修改 | 注册 `--test-provider-patch-capture` 单跑入口与全量 `Gate.Run` 项 |
| `tests/required-fixes.json` | 修改 | 新增 `provider-patch-capture`（`since: "switch-ab"`，清单总数 36 -> 37） |
| `docs/agents/maintenance-rules.md` | 修改 | 「不许丢的修复」补充提供者补丁四路径精准发现与失败归属回滚条目 |
| `docs/refactor/perf-switch-ab-gemini.md` | 新增 | 本交付文档 |

---

## 4. 离线实测对照数据（`--test-provider-patch-capture`）

测试构造 150 个已挂第三方 Harmony 补丁的混淆方法（叠加运行期其他目标共 157 个已打补丁方法），对同一目标提供者分别运行 10 轮全量发现与重复激活精准发现并逐项断言结果完全一致：

| 分支 / 构建配置 | 全量发现（10 轮均值） | 精准发现（10 轮均值） | 目标数缩减 | 耗时倍率 | 等价性断言 |
| --- | --- | --- | --- | --- | --- |
| Stable (`Release`, v0.111.1) | `2.022 ms`（`157 targets`） | `0.051 ms`（`3 targets`） | `157 -> 3`（`-98.1%`） | **~39.6x** | 精确一致 |
| Beta (`ReleaseBeta`, v0.113.1) | `2.193 ms`（`157 targets`） | `0.052 ms`（`3 targets`） | `157 -> 3`（`-98.1%`） | **~42.2x** | 精确一致 |

---

## 5. 双分支构建、门禁与受保护检查结果

输出全部隔离至 `%TEMP%\sts2-gates-gemini-ab\`（未覆盖工作区或游戏目录）。

> 说明：本机 PowerShell 执行策略为 `Restricted`，按安全规则未绕过执行策略直接运行 `.ps1` 脚本，而是按 `tools/run-gates.ps1` 逐条执行完全等价的清单自检、双分支构建、`LogicTests`、无参全量 `RuntimeTests` 及全部 5 个无参 `SEPARATE` 项；请 Claude 审核时按需补跑 `tools/run-gates.ps1 -Out "$env:TEMP\sts2-gates-gemini-ab"` 复核。

| 检查项 | Stable (`Release`, v0.111.1) | Beta (`ReleaseBeta`, v0.113.1) | 备注 |
| --- | --- | --- | --- |
| `required-fixes.json` Logic 类自检 | PASS（37 项） | PASS（37 项） | 与 `run-gates.ps1` 同正则核验 |
| `ThunninoiSkinManagerCompat` + `STS2SkinChanger` 构建 | PASS（0 警告 / 0 错误） | PASS（0 警告 / 0 错误） | 输出至 `%TEMP%\sts2-gates-gemini-ab\<branch>\mod` |
| `LogicTests` + `RuntimeTests` 构建 | PASS（0 警告 / 0 错误） | PASS（0 警告 / 0 错误） | 输出至 `%TEMP%\sts2-gates-gemini-ab\<branch>\logic` / `runtime-tests` |
| `STS2SkinChanger.LogicTests` | PASS | PASS | `Skin Changer logic policy tests passed.` |
| `RuntimeTests --test-provider-patch-capture` | PASS | PASS | 全部断言 + 离线基准通过 |
| `RuntimeTests` 无参全量 | PASS | PASS | `total=37, PASS=35, FAIL=0, NOT RUN=0, SEPARATE=2, GAP=0`，尾行 `Workshop policy tests passed.` |
| `SEPARATE`：`--test-card-cache-reuse` | PASS | PASS | 独立运行通过 |
| `SEPARATE`：`--test-merchant-lifecycle` | PASS | PASS | 独立运行通过 |
| `SEPARATE`：`--test-mode-background` | PASS（旧版跳过 Dread） | PASS | 独立运行通过 |
| `SEPARATE`：`--test-protected-boundaries` | PASS | PASS | 独立运行通过 |
| `SEPARATE`：`--test-global-write-fallback` | PASS | PASS | 独立运行通过 |
| 受保护静态安全子集（`--test-runtime-safety` / `--test-preview-binding`） | PASS | PASS | 不加载第三方 Mod DLL |
| `check-protected-characters.ps1` 第三方 DLL 项（`--test-crusader-hooks` / `--test-protected-characters`） | **未执行（交由 Claude 复核）** | **未执行（交由 Claude 复核）** | `CrusaderHookTests.cs:32` 与 `JoanStagePaletteTests.cs:12` 调用 `Assembly.LoadFrom` 加载第三方 Mod DLL，按猫猫工作区只读红线（禁止 `Assembly.Load*` 第三方 Mod DLL）跳过，请 Claude 在审核阶段执行 `tools/check-protected-characters.ps1` |

---

## 6. 真机验证步骤（供云部署后复核）

部署构建产物进入游戏后，保持性能日志开启，依次在选人界面或局外连续切换带代码皮肤（例如 **DeepSeek**、**Joan / 贞德**、**WatcherHpWear**，每个皮肤切入 -> 切回原版 -> 再次切入，往返 2~3 轮），随后查看游戏日志中的 `[性能]` 行：

1. **核对 B 项隔离耗时与路径切换**：
   - 搜索 `[性能] 皮肤代码隔离(`：
     - **首次激活**某带代码皮肤：应出现 `路径=首次激活，方式=全量`，`扫描=<ms>ms(目标=<全部已打补丁方法数>/...)`；
     - **切回原版后再切回同一皮肤（重复激活）**：应切换为 `路径=重复激活，方式=精准`，`扫描` 的 `目标` 数与耗时应明显下降（只剩该皮肤蓝图目标 + 受管候选目标）；
     - 若 `managedPatches == 0`（无受管补丁卸载），`校验=0.0ms(目标=0)`。
   - 同时核对原有 `[性能] 皮肤代码激活耗时(` 仍包含旧字段并在末尾附带 `路径/方式/扫描/卸载/校验`。
2. **核对 A 项提权包预检拆分**：
   - 搜索 `[性能] 外观重建合计(`：明细中应分别出现 `提权包预检` 与 `激活原生提供者` 两段，可直观判断卡顿来自提权包规划/A10 检查还是原生提供者激活。
3. **核对 A 项本地化读表 vs 回调拆分**：
   - 搜索 `[性能] 本地化(`：
     - 首次切入带语言表的皮肤：应出现 `方式=整套重载`，同时展示 `读表合表=<ms>ms`、`回调=<ms>ms`、`触发=<皮肤id>`、`语言文件=<N>`；
     - 再次切回已缓存组合：应出现 `方式=缓存复用`，`读表合表` 接近 `0 ms`，剩余耗时全部归入 `回调`（即 `TriggerLocaleChange` 同步回调与 `GC.Collect`），为后续 D 项优化提供实测依据。
4. **行为回归抽查**：
   - 切入/切出 DeepSeek、Joan（含切档/战损）、WatcherHpWear 后，进战斗/商店/选人界面确认立绘、骨骼、对话、头像与原版回退均正常无报错。

---

## 7. 第二轮计时（`#331`：本地化回调/读表拆细 + 提权包预检与 `PackObservations` 命中拆细）

本轮**只加计时，不改任何业务行为**，基于同一工作树 `上游修复_切换优化AB`（分支 `perf/switch-ab-20261123`）追加完成，仍未触碰 `SkinService.RuntimeSafety.cs`、`WithPreparedRuntimeResources` 及运行时包缓存路径。

### 7.1 本地化回调 200 ms 与首次读表 4.3 s 拆细（`LocalizationPerfProbe.cs` + `SkinService.cs`）

- **`TriggerLocaleChange` 三段拆解（零行为改动）**：
  - 经静态 `PEReader` 核对，正式版（`v0.111.1`）与测试版（`v0.113.1`）的 `LocManager.TriggerLocaleChange` IL 完全一致，依次执行 `TranslationServer.SetLocale(string)`、遍历 `_localeChangeCallbacks` 调用 `LocManager.LocaleChangeCallback.Invoke()`、以及 `GC.Collect()`。
  - `LocalizationTriggerPerfProbePatch` 在保留原 `Prefix`/`Finalizer` 总计时的基础上新增严格校验的 `Transpiler`：仅当上述 3 处调用指令各精确命中 1 次时，才将其分别替换为静态薄代理 `InvokeSetLocale` / `InvokeSubscriber` / `InvokeGcCollect`（保留原指令全部 `labels`/`blocks`）；任一处未匹配则原样返回原始指令并退回总计时。
  - **严格零行为改动**：完全不修改 `_localeChangeCallbacks` 列表，不替换委托实例，不改变遍历顺序，代理内采用 `try { original(); } finally { record(); }` 原样传播所有异常；非计时作用域（`_activeScope == null`）直接透传原调用。
  - **订阅者归类与 Top10**：按 `(DeclaringType, AssemblyName)` 聚合（自动将编译器生成的 `<>c*` 闭包类型向上展开至外层声明类型），记录单次切换总调用数、类型数、订阅者总耗时，并按耗时降序输出前 10 名。
- **首次整套重载「读表合表」拆细（英语一遍 / 目标语言一遍 + 文件数）**：
  - 新增 `LocalizationLoadTablesPerfProbePatch`（挂在 `LocManager.LoadTablesFromPath`，参数按索引 `__0` 绑定防改名）与 `LocalizationLoadTableFilePerfProbePatch`（挂在 `LocManager.LoadTable(string)`），均带 `Prepare()` 防御。
  - `LoadTablesFromPath("zhs", ...)` 会递归调用 `LoadTablesFromPath("eng", ...)`；计时栈按父子帧扣除子调用耗时（`exclusive = total - childElapsed`），无重复叠加地分别统计每一轮语言加载的净耗时、读表 `LoadTable` 累计耗时以及文件数（含 `基=<res://localization/...>` 与 `模=<其余路径>`）。
- **新增日志格式（追加 `键=值`，最多多打 1 行）**：
  - 原 `[性能] 本地化(<lang>)` 行末尾追加：
    `SetLocale=<ms>ms，订阅者=<ms>ms(数=<N>/类=<M>)，GC=<ms>ms`；整套重载时再追加 `读表分段=eng:<ms>ms(文件=<N>/基=<B>/模=<M>/读表=<ms>ms)+<lang>:<ms>ms(...)`。
  - 当订阅者调用数 `> 0` 时，紧跟输出 1 行：
    `[性能] 本地化回调Top10(<lang>) 总数=<N>，类型数=<M>，合计=<ms>ms，Top10=<Type[Asm]>=<ms>ms(x<K>) | ...`

### 7.2 提权包预检 50–94 ms 拆细与 `PackObservations` 命中观测（`SkinService.cs` + `GlobalResourceWrites.cs`）

- **`GlobalResourceWrites.cs`（仅计时，零逻辑改动）**：
  - 新增 `[ThreadStatic]` 作用域 `GlobalResourceWrites.BeginPackAuditPerfScope()`，在 `ObservePack` 中记录 `Calls`、`CacheHits`、`CacheMisses`、`CacheEvictions`（是否触发 `PackCacheLimit=32` 驱逐）、`CacheResets`（是否因 `_cachedProtection` 变化整表清空）、`LastCacheSize`、`OpenElapsed`（`PckArchive.OpenForAudit`）、`ObserveElapsed`（`Observe` 扫描）与累计文件数 `TotalFiles`（`Observation` 私有记录补充 `ArchiveEntries` 默认字段，命中缓存时也能准确汇报文件数）。
- **`SkinService.cs`（仅计时，原方法签名完全不变）**：
  - 通过 `[ThreadStatic]` 透传计时结构，**保持 `PrepareHighPriorityProviderPack`（3 参）与 `MountSelectedLargeRuntimeProviderPacks`（4 参 `Action`）原有签名与无重载形态不变**，避免破坏 `GlobalWriteFallbackTests` 与 `LargeProviderOverlayTests` 的反射调用。
  - 将回调前全流程拆成 5 段并单行输出 `[性能] 提权包预检明细`（每次切换仅多 1 行）：
    1. `选包`（`7450-7502`：选中包数、拟提权数、仅随从数、已挂跳过数、已略跳过数、待检数、拦截数）；
    2. `安全过滤`（`PrepareHighPriorityProviderPack`：包数、免滤包数、开包 `OpenForAudit`、扫描 `InspectHighPriorityPack`（含 `MatchesGameEntry` SHA256 原版校验数与剔除数）、写包 `WriteFromArchives`、记账 `RecordStripped`、文件总数、已存在安全包缓存命中 vs 新写）；
    3. `CheckMount`（A10 审计：`PackObservations` 调用次/命中/未命中/驱逐/重置/缓存数/开包/审计/文件数）；
    4. `跳过重筛`（`SkippedRuntimeProviders` 复筛）；
    5. `刷新跳过`（`beforeMount` 内 `RefreshSkippedProviders` 耗时与是否触发重算），以及 `受检包=<前4个包名[+N]|无>`。

> 排查线索提示：实测时可重点对比 `安全过滤`（每次无缓存重开原包 + `InspectHighPriorityPack` 逐条校验）与 `CheckMount`（`PackObservations` 命中率、是否驱逐或重置）两段占比，并从 `受检包=` 直接定位纯资源皮肤切换时究竟是哪几个包进入了预检循环。

### 7.3 第二轮改动文件与双分支验证结果

- **本轮修改文件**：
  - `STS2SkinChanger/Core/LocalizationPerfProbe.cs`（扩展 `TriggerLocaleChange` Transpiler 三段计时、订阅者 Top10、`LoadTablesFromPath`/`LoadTable` 探针）
  - `STS2SkinChanger/Core/GlobalResourceWrites.cs`（仅加 `PackAuditPerfScope` 计时观测）
  - `STS2SkinChanger/Core/SkinService.cs`（`ReportLocalizationPerf` 追加拆细与 Top10；`PrepareHighPriorityProviderPack` + `MountSelectedLargeRuntimeProviderPacks` + `beforeMount` 追加单行 `[性能] 提权包预检明细`）
  - `tests/STS2SkinChanger.RuntimeTests/ProviderPatchCaptureTests.cs`（新增验证：三处本地化探针双分支完整挂接 + `IsTriggerBreakdownInstalled == true`、订阅者顺序与异常原样传播、闭包展开与 Top10、递归 `zhs->eng` 净耗时与文件数拆分、`PackAuditPerfScope` 未命中→命中验证）
- **双分支门禁复跑结果（输出 `%TEMP%\sts2-gates-gemini-ab`）**：
  - Stable (`Release`, v0.111.1)：4 工程 0 警告 0 错误，`LogicTests` PASS，`--test-provider-patch-capture` PASS（基准 `2.505 ms (159 targets)` -> `0.050 ms (3 targets)`），无参全量 `RuntimeTests` `total=37, PASS=35, FAIL=0, NOT RUN=0, SEPARATE=2, GAP=0`（尾行 `Workshop policy tests passed.`），全部 5 个无参 `SEPARATE` 项与 `--test-runtime-safety` / `--test-preview-binding` 全过。
  - Beta (`ReleaseBeta`, v0.113.1)：4 工程 0 警告 0 错误，`LogicTests` PASS，`--test-provider-patch-capture` PASS（基准 `2.314 ms (159 targets)` -> `0.056 ms (3 targets)`），无参全量 `RuntimeTests` `total=37, PASS=35, FAIL=0, NOT RUN=0, SEPARATE=2, GAP=0`（尾行 `Workshop policy tests passed.`），全部 5 个无参 `SEPARATE` 项与 `--test-runtime-safety` / `--test-preview-binding` 全过。
  - 照旧：`run-gates.ps1` 与涉及加载第三方 Mod DLL 的 `check-protected-characters.ps1` 留由 Claude 复跑；未提交、未部署。

### 7.4 第二轮真机测法（供云收集数据）

1. 进游戏后先触发一次**首次整套重载**（切入带语言表的皮肤，如 DeepSeek），摘取 `[性能] 本地化(zhs)` 中的 `读表分段=eng:...+zhs:...` 及紧随其后的 `[性能] 本地化回调Top10(zhs)`。
2. 连续在 **DeepSeek ↔ 其他故障机器人皮肤** 间往返切换 2~3 次，摘取 `[性能] 本地化(zhs)` 中的 `SetLocale / 订阅者 / GC` 与 `[性能] 本地化回调Top10(zhs)`，确认 200 ms 主体落在 `SetLocale`、哪几个订阅者类型/程序集、还是 `GC.Collect`。
3. 切一次**纯资源皮肤**与一次**带代码皮肤**，摘取 `[性能] 提权包预检明细`，重点看 `选包 / 安全过滤(开包/扫描/缓存命中) / CheckMount(命中/未命中/驱逐/重置) / 刷新跳过 / 受检包`。

## 提交前清理（Claude，11-23）

按云定的"不改游戏本身机制"，删除了 `LocalizationPerfProbe.cs` 整个文件。它在游戏 `LocManager` 上挂了三个计时补丁（`TriggerLocaleChange` 的 Prefix/Transpiler/Finalizer，以及 `LoadTablesFromPath`、`LoadTable`），其中 Transpiler 改写了游戏方法的指令。计时结论已经拿到（每次换语言约 200 ms，主要是游戏自身的 GC；首次整套重载 4.7 s 里读表约 45 ms），不再需要这些补丁。

- `SkinService.ReportLocalizationPerf` 只保留我们自己代码里测的总耗时、方式、触发皮肤和语言文件数。
- `ProviderPatchCaptureTests` 删除 `VerifyLocalizationPerfProbeAttachesCleanly`；`required-fixes.json` 的 `provider-patch-capture` 去掉 `LocalizationTriggerPerfProbePatch` 符号。
- 提权包预检和选包的分段计时（我们自己的代码）保留，后面查"选包 50–90 ms"要用。
