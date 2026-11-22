# 优化阶段 1：门禁清单与一键验收框架（Gemini）

- **工作树**：`D:\mod\工作2\上游修复_优化阶段1`（分支 `perf/phase1-gates-20261121`，基于 `748f16d`；未提交、未推送、未部署）
- **日期**：2026-11-21
- **参照文档**（只读，未修改文档工作树）：`D:\mod\工作2\上游修复_维护规矩\STS2-SkinChanger_修改件\docs\agents\maintenance-rules.md`（至 `1.0.36.106`）、`maintenance-workflow.md`、`optimization-plan.md`

---

## 一、改动文件清单

全部文件均核实为纯 LF（`CRLF=0`），未触碰 `Core/SkinService.cs`、`Catalog/SkinCatalog.cs`、`Core/CardPortraitSessionCache.cs`、`Core/ManagedSkinModLoader.cs`：

1. `STS2-SkinChanger_修改件/tests/required-fixes.json`（新增）：将 `maintenance-rules.md`「不许丢的修复」全部 **31 条**（基线 1.0.35 8 条 + 1.0.36.102 6 条 + 1.0.36.103/.104 6 条 + 1.0.36.105 8 条 + 1.0.36.106 3 条）逐条建档，每项含 `id`、`title`、`since`、`symbols`、`tests`（含 `suite`、`name`、`args`、可选 `requiresExternalPack`）。
2. `STS2-SkinChanger_修改件/tests/STS2SkinChanger.RuntimeTests/RequiredFixesGate.cs`（新增）：实现 `Gate.Run` / `Gate.RunAsync`（不改变顺序与遇错即停语义）、定位并读取 `tests/required-fixes.json`，以及结束时（成功或失败均打印）按项输出 `PASS` / `FAIL` / `NOT RUN` / `SEPARATE` / `GAP` 与汇总行。
3. `STS2-SkinChanger_修改件/tests/STS2SkinChanger.RuntimeTests/RequiredFixesManifestTests.cs`（新增）：门禁清单自检测试（已接入无参完整套件并提供 `--test-required-fixes-manifest`），校验 JSON 解析、`id` 唯一、必填字段、`suite == "Runtime"` 类型存在于测试程序集、非空 `args` 在 `Program.cs` 中被识别、无参条目通过 `Gate.Run`/`Gate.RunAsync` 调用。
4. `STS2-SkinChanger_修改件/tests/STS2SkinChanger.RuntimeTests/Program.cs`（修改）：为无参完整套件各测试组包上 `Gate.Run` / `Gate.RunAsync`，在 `finally` 中打印门禁清单报告；存在失败或未运行项时设置非零退出码；全通过时最后一行保持输出 `Workshop policy tests passed.`。
5. `STS2-SkinChanger_修改件/tools/run-gates.ps1`（新增）：一键双分支门禁脚本（开头调用 Win32 `SetErrorMode(3)`；支持 `-StableRef`、`-BetaRef`、`-Out` 全路径参数；对 `suite == "Logic"` 条目检查 `tests/STS2SkinChanger.LogicTests/*.cs` 源码类定义；正式版 `-c Release` 与测试版 `-c ReleaseBeta` 依次跑主工程构建 → `LogicTests` → 无参 `RuntimeTests`（输出至 `-Out` 各自子目录）→ 无需外部包的 `SEPARATE` 单项；最后打印双分支合并清单表、外部真包单项提示与总耗时）。

---

## 二、门禁清单统计与缺口说明

- **清单总数**：**31 项**
- **无参 `RuntimeTests` 单独运行时统计**：
  - `PASS`：**30 项**
  - `SEPARATE`：**1 项**（`merchant-hot-switch-ready-order`，仅由 `--test-merchant-lifecycle` 覆盖）
  - `FAIL`：**0 项**
  - `NOT RUN`：**0 项**
  - `GAP`：**0 项**
- **合并双分支完整门禁统计（含 `LogicTests` 与无需外部包的 3 条 `SEPARATE` 单项）**：
  - `PASS`：**29 项**（全绿）
  - `PASS+EXT`：**2 项**（离线核心用例均已 PASS，另挂了需要外部真实包的单项，由脚本列为「需要真实包，另跑」）：
    - `ancient-waifus-support`：离线 `AncientWaifusSupportTests` 已 PASS；另挂 `CznCompatibilityTests (--test-czn-style-ui)` 需真实包路径
    - `independent-fool-skin-nono-preview`：离线 `NonoFoolPreviewTests` 已 PASS；另挂 `IndependentFoolSkinTests (--test-independent-fool-skins)` 需真实包路径
  - **独立运行的无需外部包 `SEPARATE` 单项（3 条，双分支均 PASS）**：`--test-merchant-lifecycle`、`--test-mode-background`、`--test-protected-boundaries`
  - **GAP 列表**：**无（0 项）**
    - 注（供拍板确认）：`multiplayer-lobby-change-character-param`（1.0.35 第 8 条「不绑定 `ChangeCharacter` 第三个参数名」）当前绑定 `MultiplayerAppearanceIdentityTests`（会实装 `MultiplayerLobbyCharacterChangedPatch` 到 `StartRunLobby.ChangeCharacter`，双分支运行会自然捕获参数名漂移），未单独写静态反射参数名断言；如希望要求更严格的静态断言，可在后续补一条。

---

## 三、验证结果与耗时

- **`tools/run-gates.ps1` 语法检查**：PowerShell AST 解析（`[Parser]::ParseFile`）**0 错误**。
  - 说明：本机 Windows PowerShell 默认执行策略全作用域为 `Undefined`（`Restricted`），未绕过策略直接调用 `.ps1`；已按脚本同等完整流程逐项实跑双分支全部命令，建议在允许执行本地脚本的终端中再直接运行一次 `tools/run-gates.ps1` 终验。
- **双分支端到端验证总耗时**：**43.3 秒**

| 分支 | 配置 | 主工程构建（`-o %TEMP%`） | `LogicTests` | 无参 `RuntimeTests`（含门禁自检） | `SEPARATE` 3 条无外部包单项 |
| --- | --- | --- | --- | --- | --- |
| Stable（v0.111.1） | `Release` | **0 警告 / 0 错误** | **通过** | **通过**（31 项：30 PASS / 1 SEPARATE / 0 FAIL / 0 GAP；末行 `Workshop policy tests passed.`） | **3/3 通过** |
| Beta（v0.113.1） | `ReleaseBeta` | **0 警告 / 0 错误** | **通过** | **通过**（31 项：30 PASS / 1 SEPARATE / 0 FAIL / 0 GAP；末行 `Workshop policy tests passed.`） | **3/3 通过** |

---

## 四、尚未做的事项

- **UI 护栏**（阶段 1 下一轮任务，按 `#283` 指示本轮不做）。
- **直接执行 `.ps1` 文件**：受本机默认 `Restricted` 执行策略限制，本次已做 AST 解析验证 + 同等命令全量双分支实跑，待云/Claude 在允许执行本地脚本的环境直接点跑一次 `tools/run-gates.ps1`。
- **需要真实包的单项**（`tools/check-protected-characters.ps1`、`--test-czn-style-ui`、`--test-independent-fool-skins`）：按 `#283` 要求不在 `run-gates.ps1` 内强跑，已在脚本末尾显式提示。

---

## 五、第二轮：UI 护栏 + A/B 模板 + 无头冒烟可行性 + 门禁增量（2026-11-22）

- **基线进展**：工作树已由 Claude 快进至 `92ab5f6`（内测 `1.0.38.12`），第一轮改动保留。未提交、未推送、未部署，未触碰 `Core/SkinService.cs`、`Catalog/SkinCatalog.cs`、`Core/CardPortraitSessionCache.cs`、`Core/ManagedSkinModLoader.cs`、`Core/PoroVariantBridge.cs`、`CardPipelineExceptionBoundary`、`CombatAppearanceBoundary` 及白猫两份交接文档（`handoff-*-20261122.md`）。

### 5.1 改动文件清单（第二轮）

- **新增**：
  - `STS2SkinChanger/Ui/UiInjectionGuard.cs`
  - `tests/STS2SkinChanger.RuntimeTests/UiInjectionGuardTests.cs`
  - `docs/agents/ab-record-template.md`
- **修改**：
  - 12 个 UI 注入补丁文件（完整清单见 5.2 节）
  - `tests/STS2SkinChanger.RuntimeTests/EntryPatchIsolationTests.cs`（补充 `VakuuVanillaArtPatch.Prepare()` 离线分支验证）
  - `tests/STS2SkinChanger.RuntimeTests/WorkshopTests.cs` & `tests/STS2SkinChanger.RuntimeTests/Program.cs`（接入 `UiInjectionGuardTests`，并去除 `WorkshopTests.Run()` 重复打印，无参全套仅在最后一行输出 `Workshop policy tests passed.`；`--test-workshop` 单独调用仍保留该输出）
  - `tests/required-fixes.json`（31 → **33 项**，新增 `stable-vakuu-compendium-entry`、`ui-injection-guard`）
  - `tools/run-gates.ps1`（默认路径自动探测 + 开头打印实际引用目录 + `EXTERNAL` 汇总计数分支；Claude 已补回 UTF-8 BOM）
  - `docs/agents/maintenance-rules.md`（末尾补 `stable-vakuu-compendium-entry` 与 `ui-injection-guard` 两条）
  - `docs/agents/maintenance-workflow.md`（第 4 步与核对清单从旧「116 行」改为「末行 `Workshop policy tests passed.` + `GATE VERDICT: PASSED`」，标注一键入口）

> **编码注意（UTF-8 BOM）**：`tools/run-gates.ps1` 内含中文默认路径与提示文本，**后续修改必须保存为 UTF-8 带 BOM + LF**（Windows PowerShell 5.1 读取无 BOM 的 UTF-8 脚本会把中文路径读乱并导致 `GetFullPath` 报「路径中有非法字符」；本轮已由 Claude 补回 UTF-8 BOM 并保持 LF）。其余 `.cs` / `.json` / `.md` 保持纯 LF 无 BOM。

---

### 5.2 E. UI 注入点异常隔离完整列表

- **隔离机制（`STS2SkinChanger/Ui/UiInjectionGuard.cs`）**：
  - `Run(string point, Action action)` / `Run<T>(string point, Func<T> action, T fallback)`：捕获全部异常并吞掉（绝不外抛截断同条 Harmony 链上其他 Mod 的回调，例如 RitsuLib 图鉴头像同步）；同 `point` 仅首次输出 `[界面注入] <point> 失败: <异常类型>: <消息>` + 完整异常堆栈，后续仅累加每点位计数。
  - `RunWhenInTree(Node? node, string point, Action action)`：节点已入树时经 `Run` 立即执行；未入树时挂载一次性 `TreeEntered` 回调，入树后先退订再经 `Run` 执行。

#### 已套用 Guard 的全部 UI 注入点（12 个文件 / 22 个补丁类 / 27 个 Harmony 钩子 / 28 个 Guard 点位）

| # | 文件 | 补丁类（`STS2SkinChanger.Ui`） | 目标游戏方法 | Guard 点位 ID |
|---|---|---|---|---|
| 1 | `Ui/BackgroundAppearanceScreen.cs` | `BackgroundAppearancePauseMenuPatch` | `NPauseMenu._Ready`、`NPauseMenu.OnSubmenuOpened` | `BackgroundAppearancePauseMenuPatch.Postfix` |
| 2 | `Ui/CacheWriteNotice.cs` | `CacheWriteNoticeReadyPatch` | `NMainMenu._Ready` | `CacheWriteNoticeReadyPatch.Postfix` |
| 3 | `Ui/CharacterAppearanceScreen.cs` | `CharacterAppearancePauseMenuPatch` | `NPauseMenu._Ready`、`NPauseMenu.OnSubmenuOpened` | `CharacterAppearancePauseMenuPatch.Postfix` |
| 4 | `Ui/ContextualSkinControls.cs` | `CharacterSelectionSkinPatch` | `NCharacterSelectScreen.SelectCharacter`（含 `CharacterSelectModePatches` 复用模式） | `CharacterSelectionSkinPatch.Prefix`、`CharacterSelectionSkinPatch.Postfix` |
| 5 | `Ui/ContextualSkinControls.cs` | `BestiarySelectionSkinPatch` | `NBestiary.SelectMonster` | `BestiarySelectionSkinPatch.Postfix` |
| 6 | `Ui/ContextualSkinControls.cs` | `MultiplayerPlayerStateIconScopePatch` | `NMultiplayerPlayerState._Ready` | `MultiplayerPlayerStateIconScopePatch.Prefix`、`MultiplayerPlayerStateIconScopePatch.Postfix` |
| 7 | `Ui/ContextualSkinControls.cs` | `RemoteLobbyPlayerIconScopePatch` | `NRemoteLobbyPlayer._Ready` | `RemoteLobbyPlayerIconScopePatch.Prefix`、`RemoteLobbyPlayerIconScopePatch.Postfix` |
| 8 | `Ui/ContextualSkinControls.cs` | `RemoteLobbyPlayerVisualRefreshScopePatch` | `NRemoteLobbyPlayer.RefreshVisuals` | `RemoteLobbyPlayerVisualRefreshScopePatch.Prefix`、`RemoteLobbyPlayerVisualRefreshScopePatch.Postfix` |
| 9 | `Ui/EventCompendiumPreview.cs` | `EventPreviewLifecyclePatch` | `NEventLayout._EnterTree` / `_ExitTree`、`NEventOptionButton._Ready` / `OnRelease` | `EventPreviewLifecyclePatch.Prefix`（回退 `true`） |
| 10 | `Ui/LoadOrderWarning.cs` | `LoadOrderWarningModalReadyPatch` | `NModalContainer._Ready` | `LoadOrderWarningModalReadyPatch.Postfix` |
| 11 | `Ui/LoadOrderWarning.cs` | `MainMenuLoadOrderWarningPatch` | `NMainMenu._Ready` | `MainMenuLoadOrderWarningPatch.Postfix` |
| 12 | `Ui/LoadOrderWarning.cs` | `LoadOrderWarningModalClosedPatch` | `NModalContainer.Clear` | `LoadOrderWarningModalClosedPatch.Postfix` |
| 13 | `Ui/ManagedModListNamePatch.cs` | `ManagedModListNamePatch`（保留 `Priority.First`） | `NModMenuRow._Ready` | `ManagedModListNamePatch.Postfix` |
| 14 | `Ui/ModThemeEditor.cs` | `ModThemeEditorReadyPatch` | `NMainMenu._Ready` | `ModThemeEditorReadyPatch.Postfix`、`ModThemeEditorReadyPatch.Deferred` |
| 15 | `Ui/PauseMenuEntryToggles.cs` | `PauseMenuEntryTogglesPatch` | `NPauseMenu._Ready`、`NPauseMenu.OnSubmenuOpened` | `PauseMenuEntryTogglesPatch.Postfix` |
| 16 | `Core/Features/Card/CardSkinControls.cs` | `CardLibrarySkinReadyPatch` | `NCardLibrary._Ready` | `CardLibrarySkinReadyPatch.Postfix` |
| 17 | `Core/Features/Card/CardSkinControls.cs` | `CardInspectSkinReadyPatch` | `NInspectCardScreen._Ready` | `CardInspectSkinReadyPatch.Postfix` |
| 18 | `Core/Features/Card/CardSkinControls.cs` | `CardLibrarySkinOpenedPatch` | `NCardLibrary.OnSubmenuOpened` | `CardLibrarySkinOpenedPatch.Prefix`、`CardLibrarySkinOpenedPatch.Postfix` |
| 19 | `Core/Features/Card/CardSkinControls.cs` | `CardLibraryPoolSkinPatch` | `NCardLibrary.UpdateCardPoolFilter` | `CardLibraryPoolSkinPatch.Postfix` |
| 20 | `Core/Features/Ancient/BestiaryEntranceSwitcher.cs` | `BestiaryEntranceReadyPatch` | `NCompendiumSubmenu._Ready` | `BestiaryEntranceReadyPatch.Postfix` |
| 21 | `Core/Features/Ancient/BestiaryEntranceSwitcher.cs` | `BestiaryEntranceOpenedPatch` | `NCompendiumSubmenu.OnSubmenuOpened` | `BestiaryEntranceOpenedPatch.Postfix` |
| 22 | `Core/Features/Monster/MonsterSkinPriorityControls.cs` | `BestiaryInitialSkinNamePatch` | `NBestiary.OnSubmenuOpened` | `BestiaryInitialSkinNamePatch.Postfix` |

#### 显式排除的非 UI 注入生命周期补丁（15 类，均不套用 `UiInjectionGuard`）

- `CardTreeReentrySkinPatch`（`NCard._EnterTree`，归 `CardPipelineExceptionBoundary` 管辖，按规矩切换器自身异常照常抛出，且 `AppearanceResourceBoundaryTests` 直接检验其 IL 调用）；
- `CharacterAppearanceCreatureReadyPatch`、`InRunCharacterAppearanceRuntimePatch`、`BossMapPointSkinPresentationPatch`（战斗内模型/地图boss表现，受 `CombatAppearanceBoundary` 降级规则保护）；
- 商人/火堆场景作用域与预览抑制 10 类（`MerchantRoomCreateAppearancePatch`、`MerchantRoomPlayerAppearancePatch`、`MerchantRoomPreviewEnterTreePatch`、`FakeMerchantAppearancePatch`、`FakeMerchantPreviewEnterTreePatch`、`RestSitePlayerAppearancePatch`、`MerchantButtonProviderReadyBridgePatch`、`MerchantInventoryProviderReadyBridgePatch`、`MerchantHandProviderReadyBridgePatch`、`MerchantPreviewProceedReadyPatch`，均为场景数据/抑制前缀，非 UI 注入）；
- `RandomCharacterLobbySkinPatch`（非 UI 注入的随机皮肤恢复，自带 `try/catch` 且 `RandomCharacterSkinTests` 直接检验 `Prefix` 内部调用顺序）。

---

### 5.3 完整性检查机制、已知缺口与需实机确认项

- **检查机制（`tests/STS2SkinChanger.RuntimeTests/UiInjectionGuardTests.cs`）**：
  1. **行为与链式隔离验证**：验证抛出异常的 `action` 不外抛，日志精确按 `[界面注入] <point> 失败: <异常类型>: <消息>` + 完整堆栈输出且同一 `point` 只记一次、后续仅累计次数；用两个 Harmony Postfix 实际挂到虚设目标方法上模拟前一个抛错、确认下游后置回调照常执行；离线校验 `RunWhenInTree` 空节点防护及 IL 调用了 `Node.IsInsideTree`、`add_TreeEntered`、`remove_TreeEntered` 与 `UiInjectionGuard.Run`。
  2. **完整性检查**：用反射扫描 `STS2SkinChanger.Ui*` 命名空间内所有带 `[HarmonyPatch]` 的类，汇总特性元数据与 `TargetMethod` / `TargetMethods` 解析出的目标方法名，筛选全部以 `_Ready` / `_EnterTree` / `OnSubmenuOpened` 为目标的补丁类，扣除上述 15 类显式排除表（同时校验排除表无陈旧失效项）后合并 5 类显式审计界面刷新/清理补丁，读取每个 `Prefix` / `Postfix` 钩子的原始 IL，断言**必须调用** `UiInjectionGuard.Run` 或 `UiInjectionGuard.RunWhenInTree`（当前锁死 **22 类 / 27 钩子**）。任何新增的 UI 生命周期补丁未接入 guard 或未显式排除时，该测试直接变红。
- **已知缺口（4 项）**：
  1. 手动调用 `harmony.Patch(...)` 而未挂载 `[HarmonyPatch]` 特性的类不在反射类型扫描范围内（注：`CharacterSelectModePatches` 复用的 `CharacterSelectionSkinPatch` 已受扫描与 guard 覆盖）。
  2. 声明在 `STS2SkinChanger.Ui*` 以外命名空间的生命周期补丁不在扫描范围内。
  3. 除 `_Ready` / `_EnterTree` / `OnSubmenuOpened` 及 5 个显式审计类以外的自定义界面初始化方法名，需手动加入审计清单或命名规范统一。
  4. IL 校验只验证钩子方法内存在 `UiInjectionGuard.Run` / `RunWhenInTree` 调用，不能阻止开发者在同一钩子方法中把额外逻辑写在委托之外。
- **需实机确认项**：
  1. **`UiInjectionGuard.RunWhenInTree` 真节点时序**：离线 .NET 测试宿主无 Godot 原生运行时，无法实例化真实 `Godot.Node`；其未入树挂一次性 `TreeEntered` 回调并退订的实际行为需实机确认。
  2. **22 处 UI 注入点的实机界面表现**：需在游戏内走一遍主菜单（主题调节按钮、缓存空间提示）、Mod 列表（`[SC]` 标记）、暂停菜单（外观/背景入口与开关）、选角（单人/多人/恐惧/极速）、卡牌总览/详情、怪物/先古/事件图鉴及联机大厅头像，确认无新异常且 `[界面注入]` 日志无误报。

---

### 5.4 F. A/B 构建对照模板

- 已创建 [`docs/agents/ab-record-template.md`](../agents/ab-record-template.md)：
  - **模板部分**：覆盖基本信息、构建标识对比表（内测版本号 + `Gurio.SkinChanger.dll` SHA-256 前 12 位 + 启动/补丁计数等日志特征行）、从 `settings.save` 导出已启用 Mod ID 与加载顺序命令、共用操作步骤、A/B 观测对照表及归属结论。
  - **实填示例**：已用 `#259` 卡牌总览工具栏消失与空崎日奈图鉴头像回退案例完整填写对照表（构建 A = `1.0.36.104`「接管 4 个，保留 56 个」vs 构建 B = `1.0.36.105`「接管 5 个，保留 55 个」及 `godot2026-11-21T01.53.12.log` 堆栈对比）。

---

### 5.5 G. 真实场景树无头 Godot 冒烟测试可行性结论（仅静态调查，未写代码、未启动游戏）

- **一句话结论**：**底层可行但受限于发布版二进制、存档隔离风险与约 3 GB 全量冷启动耦合，实现与维护成本约 2–5 人天，短期不建议并入日常门禁**；若后续仍有高频 `_Ready` 回归，可先做 30 分钟实机预研（验证 `override.cfg` 存档隔离 + `--headless --audio-driver Dummy` 能否顺利跑到 `NMainMenu`）再决定是否单建可选慢速脚本。
- **所需运行环境与文件**：
  - 完整游戏安装目录，非仅托管程序集引用：`SlayTheSpire2.exe`（894.7 MB，静态特征为 Godot `4.5.1.m.14.mono.custom_build` `template_release`，`tools_enabled=False`，包含 `--headless` / `--script` / `--main-pack` / `--quit` 参数）、`SlayTheSpire2.pck`（2.15 GB）、`data_sts2_windows_x86_64\`（.NET 9.0.7 + `sts2.dll` / `GodotSharp.dll` / `Steamworks.NET.dll` / `Sentry*.dll`）、原生 GDExtension（`libGodotFmod` + `fmod*.dll`、`libsentry` + `crashpad_handler.exe`、`libspine_godot`），以及待测 `Gurio.SkinChanger.dll` + 驱动用测试脚本/微型测试 Mod。
- **玩家存档与配置污染风险（高）**：
  - 包内 `res://project.binary` 启用 `application/config/use_custom_user_dir = true`，`custom_user_dir_name = SlayTheSpire2`；`SlayTheSpire2.exe` 未编译 `--user-data-dir` 参数且直接调用 Windows `SHGetKnownFolderPath`（无视环境变量或需改 `override.cfg`）；游戏侧 `SaveManager` / `GodotFileIo` 与切换器 `SkinChangerPaths.cs` 均直接读写 `OS.GetUserDataDir()`。
  - 直接无参数/常规 `--headless` 运行会触碰玩家真实 `%APPDATA%\SlayTheSpire2\`（存档、`settings.save`、触发 `godot*.log` 归档轮转、皮肤配置与 PCK 缓存写入），并同时加载玩家真实 `mods/` 与工坊订阅。若要安全隔离，需搭建独立沙盒工作目录并用 `override.cfg` 覆写 `custom_user_dir_name`（发布版 `template_release` 是否放行 `override.cfg` 覆盖该键仍需实机实测，且仍会在 `%APPDATA%` 创建临时目录）。
- **主要阻塞点**：
  1. **启动链强耦合**：`sts2.dll` 虽包含 `--headless` / `IsHeadless` / `ScenarioRunner`，但直接 `--script` 跳过 `NGame.GameStartup` 与 `OneTimeInitialization` 会使 `ModelDb`、`LocManager`、`PreloadManager`、`SaveManager` 处于未初始化状态，直接实例化 `NMainMenu` / `NPauseMenu` / `NCardLibrary` 会第一时间空引用；走完整 `game.tscn` 启动链则必须冷加载 2.15 GB `SlayTheSpire2.pck` 并经过 `NGame.InitializePlatform`（Steam 初始化路径）。
  2. **Autoload 兼容风险**：`project.binary` 配有 `SentryBootstrap0`（`Sentry.Godot`）与 `FmodManager` 两个全局 Autoload，在 `--headless` 无渲染/无音频设备环境是否稳定降级或需加 `--audio-driver Dummy` 未经实机验证。
  3. **双分支环境体量**：与目前几十秒级纯托管 `RuntimeTests` 不同，跑真实场景树需要正式版与测试版各自约 3 GB 的完整游戏本体与独立沙盒，CI/本地磁盘与耗时显著增大。
  4. **无编辑器测试宿主**：`template_release` 不含编辑器与 GUT 等测试运行器，必须自写专用测试驱动 Mod 挂接 `NGame.GameStartupComplete` 自动切菜单、触发断言并以退出码反馈结果。
- **大致工作量估算**：
  - 仅验证单分支 `override.cfg` 隔离 + `--headless` 拉起 `NMainMenu` 最小原型：**2–3 人天**；
  - 覆盖主菜单/暂停菜单/卡牌总览三场景并兼容双分支、稳妥清理沙盒：**4–5 人天**（且随游戏启动流程改版存在持续维护成本）。

---

### 5.6 Vakuu 门禁补项、Script 默认路径及验证汇总

- **新增第 32、33 项门禁**：
  - `stable-vakuu-compendium-entry`（`1.0.37.4`，`60eb38f`）：在 `EntryPatchIsolationTests` 中验证 `VakuuVanillaArtPatch.Prepare()` 随目标方法存在性返回 `true/false`，并实调 `FeatureBootstrap.InstallGroups` 确认正式版跳过该类时不导致 `ancient` 模块回滚（`BestiaryEntranceReadyPatch` 正常挂上 `NCompendiumSubmenu._Ready`）；同时保留 `VakuuVanillaArtTests`（`--test-vakuu-vanilla`，`requiresExternalPack: true`）。
  - `ui-injection-guard`（`1.0.38.12`）：接入无参全套 `UiInjectionGuardTests`。
- **`tools/run-gates.ps1` 默认路径探测**：
  - 未传 `-StableRef` / `-BetaRef` 时按顺序取首个含 `sts2.dll` 的目录：正式版 `C:\sts2refs\stable` → `D:\mod\游戏版本备份\正式版_v0.111.1_20261117_2249_build26655341\data_sts2_windows_x86_64`；测试版 `D:\steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64` → `C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64`；找不到直接报错并提示参数用法，脚本开头输出实际使用路径。
- **验收结论（Claude 实跑 `powershell -ExecutionPolicy Bypass -File tools/run-gates.ps1`）**：
  - 两种引用（正式版 `Release` + 测试版 `ReleaseBeta`）构建 0 警告 0 错误，**33 项门禁汇总：`PASS 30 + PASS+EXT 3`，`FAIL 0 / NOT RUN 0 / GAP 0`，结论 `PASSED`**；无参完整 `RuntimeTests` 末行仅出现一次 `Workshop policy tests passed.`。
- **顺带发现的既有问题（本轮遵循边界原则未越界修改，留待另开单评估）**：
  - 独立条目 `--test-game-branch`（`GameBranchCompatibilityTests.cs:19`）在正式版与测试版均报 `ProviderAssemblyCompatibility.PrepareForCurrentGame` 有 **3 项未解析成员**：`Core/HildeAncientDialogueCompat.cs`（L66、L87）使用 `AncientDialogueSet` 的 `init` 属性写入器产生 `System.Void modreq(System.Runtime.CompilerServices.IsExternalInit)`（`set_AgnosticDialogues` / `set_CharacterDialogues` / `set_FirstVisitEverDialogue`），兼容校验未剥离 `modreq` 导致既有用例在到达后续断言前抛出。本轮未修改 `ProviderAssemblyCompatibility.cs` 与 `HildeAncientDialogueCompat.cs`。
