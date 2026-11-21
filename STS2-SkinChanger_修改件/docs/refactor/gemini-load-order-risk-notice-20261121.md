# #277 提前加载危险提供者风险提醒与 LoadOrderManager 识别（Gemini）

- **工作树**：`D:\mod\工作2\上游修复_加载顺序提醒\STS2-SkinChanger_修改件`（分支 `feat/load-order-risk-notice`，基于 `6a8a9f6`；未提交、未部署；主工作区未触碰）
- **日期**：2026-11-21

---

## 一、实现概要（仅提醒，不改加载行为）

1. **新增 `STS2SkinChanger/Ui/LoadOrderRiskDetector.cs`**（`namespace STS2SkinChanger.Ui.LoadOrderWarning`）：
   - **危险提供者判定**（仅扫描 `ManagedSkinModLoader.SkinProvidersBeforeSelf`，即本次已提前被游戏原生加载的皮肤提供者）：
     - **原因 a（`LoadOrderRiskReasons.CardPatch`）**：通过 `Harmony.GetAllPatchedMethods()` + `Harmony.GetPatchInfo(...)`，按加载程序集（`Mod.assembly` 或已加载 `AppDomain` 中按 `.dll` 规范路径匹配）或补丁 `owner == manifest.id` 检查是否给 `NCard` 卡面显示方法（`_EnterTree`、`_Ready`、`UpdateVisuals`、`Reload`、`UpdateVisualsFromModel`）或 `CardModel` 卡图 getter（复用 `ManagedSkinModLoader.IsManagedCardPortraitMemberName`）安装了 Harmony 补丁；
     - **原因 b（`LoadOrderRiskReasons.SharedGameAsset`）**：读取该提供者 `.pck` 路径表，复用 `#257` 的 `SkinCatalog.ProtectGlobalCanonicalPackFile`（`catalog` 未就绪时回退 `SkinCatalog.IsSharedGameAtlasPath`）判断是否包含与游戏本体共享图集或原版卡图导入产物重名的路径。
   - **结构化日志**：检测到危险提供者时固定输出 `ModLog.Warn`：
     `[加载顺序] 提前加载的危险提供者: 包名(原因a/b) …`（单命中标 `原因a` 或 `原因b`，双命中标 `原因a/b`）。
   - **外部排序管理器识别**：`IsExternalLoadOrderManagerLoaded` 严格按 `mod.state == ModLoadState.Loaded` 且 `manifest.id == "LoadOrderManager"`（忽略大小写）判断，绝不按显示名猜测。
   - **弹窗策略矩阵（`ResolvePromptPlan`）**：
     - 存在危险提供者时：**每次启动必弹**（不再受「排序未变化」或「旧快照迁移」静默跳过），白话列出风险包名、可能后果（联机手牌消失、出牌卡左上角、或选「游戏默认」仍显示别的包的卡图）及处理步骤；
     - 检测到 `LoadOrderManager` 已加载时：**不做自动移动**、**不显示「优先并重启」**，仅显示单个「知道了」按钮（`YesButton` 设 `Acknowledge`、`NoButton.Hide()`），提示改在 Load Order Manager 中把皮肤切换器拖到这些皮肤包前面后重启；
     - 无危险提供者且未加载 `LoadOrderManager` 时：**完全保持原有行为**（含首次安装自动移动与重启提示、排序未变不弹、排序变更弹询问框）。
2. **更新 `STS2SkinChanger/Ui/LoadOrderWarning.cs`**：
   - `Schedule()` 接入 `LoadOrderRiskDetector.DetectDangerousProviders`、`IsExternalLoadOrderManagerLoaded` 与 `ResolvePromptPlan`；
   - `ShowWarning(...)` 支持单按钮「知道了」模式（与 `MultiplayerSkinFailureDialog` 同一模式），首次安装自动移动流程原样保留（仅在加载 `LoadOrderManager` 时抑制自动移动）。
3. **更新 `STS2SkinChanger/Core/ModLocalization.cs`**：
   - 新增 4 个键并补齐全部 15 种语言（`eng`、`zhs`、`zht`、`deu`、`esp`、`spa`、`fra`、`ita`、`jpn`、`kor`、`pol`、`ptb`、`rus`、`tha`、`tur`）：
     - `LoadOrderRiskMessage`
     - `LoadOrderRiskWithManagerMessage`
     - `LoadOrderWithManagerMessage`
     - `LoadOrderRiskFirstInstallMessage`
4. **新增回归测试 `tests/STS2SkinChanger.RuntimeTests/LoadOrderRiskNoticeTests.cs`**（`--test-load-order-risk-notice`，已接入无参全套 `Program.cs`）：
   - 覆盖原因 a（真实 Harmony 补丁挂到 `NCard._EnterTree` 与 `CardModel.PortraitPath`，按程序集和 `owner` 均检出；无关目标不误报）；
   - 覆盖原因 b（合成 PCK 含 `card_atlas_0` / 愚者单卡 `.godot/imported/*.ctex` 载荷命中，私有路径不误报）及结构化日志格式；
   - 覆盖 `LoadOrderManager` 按 `manifest.id` 与已加载状态判定（不按名称猜测、未加载不误判）；
   - 覆盖有无 `LoadOrderManager`、有无危险包、首次安装与排序未变/变更的完整策略矩阵与按钮/文案选择；
   - 覆盖 4 条新文案全部 15 种语言非空与 `{0}` 占位符校验。

---

## 二、编译与自动化验证结果

全部改动文件行尾均核实为纯 LF（`CRLF=0`）：`LoadOrderRiskDetector.cs`、`LoadOrderWarning.cs`、`ModLocalization.cs`、`LoadOrderRiskNoticeTests.cs`、`Program.cs`。

| 分支 | 主工程 Release 构建（`-o %TEMP%`） | `LogicTests` | 完整无参 `RuntimeTests` |
| --- | --- | --- | --- |
| 测试版（v0.113.1） | **0 警告 / 0 错误** | **通过** | **通过**（含 `LoadOrderSafetyTests` 与 `LoadOrderRiskNoticeTests`，尾行 `Workshop policy tests passed.`） |
| 正式版（v0.111.1） | **0 警告 / 0 错误** | **通过** | **通过**（含 `LoadOrderSafetyTests` 与 `LoadOrderRiskNoticeTests`，尾行 `Workshop policy tests passed.`） |

---

## 三、实机验证步骤

1. **危险提供者每次启动提醒（无 `LoadOrderManager`）**：
   - 在 Mod 顺序中把 `AI_silent_NSFW_MOD`（原因 a）或 `fool_card_beauty`（原因 b）拖到 `皮肤切换器-Skin Changer` 前面，重启游戏进入主菜单；
   - 确认日志出现 `[加载顺序] 提前加载的危险提供者: ...`；
   - 确认弹出风险提示框，白话说明后果与处理方式，提供「重启/优先并重启」与「先不了」；点「先不了」后直接退出再重进，确认**下一次启动仍会再次提醒**。
2. **装有 `LoadOrderManager`（工坊 `3747605109`）场景**：
   - 启用 `LoadOrderManager` 且保持皮肤切换器排在皮肤包后面，启动进入主菜单；
   - 确认日志未执行自动移动；弹窗提示改为「检测到已加载 Load Order Manager，请在 Load Order Manager 里把皮肤切换器拖到这些皮肤包前面后重启游戏」，且**只显示「知道了」一个按钮**（无「优先并重启」）。
3. **无危险包回归**：
   - 把皮肤切换器排在所有危险包前面，仅把一个不含卡面补丁和共享图集载荷的普通皮肤包留在前面且排序不变，重启进入主菜单；
   - 确认日志无危险提供者记录、主菜单不弹出风险框，保持既有静默行为。
