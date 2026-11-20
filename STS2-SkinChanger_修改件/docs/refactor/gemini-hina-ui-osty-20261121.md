# #258 三项回归问题排查与修复交接（Gemini）

- **工作树**：`D:\mod\工作2\上游修复_1.0.35\STS2-SkinChanger_修改件`（主工作区；未提交、未部署）
- **日期**：2026-11-21
- **日志依据**：`C:\Users\yuinn\AppData\Roaming\SlayTheSpire2\logs\godot.log`（01:05，8168 行）

---

## 一、空崎日奈（`SorasakiMana`，工坊 `3839527876`）选中后原版卡图被识别成日奈卡图

### 1. 根因实证（确认为补丁隔离遗漏，非有意联动）

- `godot.log:1348` 明确记录 `空崎日奈替换愚者` 自带 `卡图=110`，其卡图资源已由 `SkinCatalog` 独立接管并在卡牌总览里作为 `SorasakiMana` 卡图选项提供（`godot.log:4171`：`已批量隔离 fool/SorasakiMana 的 3 个卡牌资源`）；日志同句也写明设计原则为「资源替换与卡牌呈现仍由皮肤切换器自身接管」。
- 静态检查 `SorasakiMana.dll` 证实，该包带有 `FoolPortraitPatch`（`SorasakiMana_Portrait`），直接对 `CardModel.PortraitPath` 安装了 Harmony 补丁，返回 `res://SorasakiMana/images/cards/...`。
- 当玩家把**愚者角色皮肤**切为 `SorasakiMana` 时，`ManagedSkinModLoader.ActivateProvider`（`godot.log:3232`）按 `IsManagedResourceOwnershipPatch` 剥离了 4 个资源/选角呈现补丁，但 `IsManagedResourceOwnershipPatch`（`ManagedSkinModLoader.cs:1774-1778`）仅覆盖 `CharacterModel` 与 `MonsterModel`，**漏掉了 `CardModel`**，导致 `FoolPortraitPatch` 留在 56 个行为补丁中保持生效。
- 结果：当卡牌总览里愚者卡图设为「游戏默认」（`__base__`）时，`SkinService.ResolveCardPortraitRequest` → `FrameworkCardVisualGuard.TryGetBaselinePortraitPath(card)` 调用 `card.PortraitPath` 读到了被劫持的 `res://SorasakiMana/images/cards/fool/strike_fool.png`；隔离回源本体 `SlayTheSpire2.pck` 失败（`godot.log` L3422/3456/3485/3497/3511/3534/3572/3587/3646 连续警告 `卡牌皮肤资源不可用，已回退原版卡图：fool/__base__/res://SorasakiMana/images/cards/fool/*.png；原因=找不到任何可隔离的卡牌资源`），回退原生加载后直接命中了随完整视觉会话挂载的 `SorasakiMana` 卡图。切换其他愚者皮肤时该 56 个补丁被卸载（L3690），手动切回原版卡图后恢复正常。

### 2. 修复内容

- **`STS2SkinChanger/Core/ManagedSkinModLoader.cs`**：
  - 在 `IsManagedResourceOwnershipPatch` 中纳入 `typeof(CardModel).IsAssignableFrom(declaringType)` 且目标方法为 `IsManagedCardPortraitMemberName`（`get_PortraitPath`、`get_Portrait`、`get_BetaPortraitPath`、`get_PackedPortraitPath`、`get_HasPortrait`、`GetPortraitPath`）的补丁；
  - 仅剥离 `CardModel` 卡图路径/贴图补丁，保留 `SorasakiMana` 的 `CardPoolModel` 卡框配色（`CardFrameColorPatch`）、出牌/生牌 VFX、语音、角色动画等其余全部行为补丁；
  - 选中「游戏默认」时愚者卡保持游戏原版，选中 `SorasakiMana` 卡图选项时仍由皮肤切换器隔离通道提供日奈 110 张卡图。

---

## 二、卡牌总览「有皮肤卡牌」筛选框与皮肤优先级按钮被左下角返回箭头遮挡

### 1. 根因实证（正式版与测试版 `card_library.tscn` 实测几何一致）

- 静态解包正式版 v0.111.1 与测试版 v0.113.1 的 `res://scenes/screens/card_library/card_library.tscn` 与 `res://scenes/ui/back_button.tscn` 证实两版本布局一致：
  - `BackButton` 锚定于画面左下角（`anchor_top=1, anchor_bottom=1, offset_left=-40, offset_right=160, offset_top=-354, offset_bottom=-244`，视觉阴影延伸至 `right=218, bottom=-205`）；
  - `Sidebar/MarginContainer/BottomVBox` 锚定在底边（`margin_bottom=16`），原版 3 个勾选框（`MultiplayerCards`、`Stats`、`Upgrades`，各 42 px）已占 `[-150, -16]`，距离返回箭头阴影仅剩 ~55 px；
  - `CardSkinControls.Attach` 先前把 `STS2CardSkinSelector`（40 px）和 `STS2SkinnedCardsOnly`（42 px）直接塞进 `BottomVBox` 顶部，将 `BottomVBox` 顶高至约 `-240..-260`，正好撞进返回箭头区域；
  - 左侧 `TopVBox` 在测试版新增愚者卡池（3 行）及装入 RitsuLib 模组角色后也会向下逼近返回箭头，同样不宜追加行。

### 2. 修复内容

- **`STS2SkinChanger/Core/Features/Card/CardSkinControls.cs`**：
  - 将 `STS2CardSkinSelector`（`皮肤优先级 (N)` + `预设`）与 `STS2SkinnedCardsOnly`（`有皮肤卡牌`）移出 `BottomVBox`，放入新建的顶部横向工具栏 `STS2CardSkinToolbar`（`HBoxContainer`，置于弹窗遮罩之下）；
  - 新增纯计算方法 `ComputeLibraryToolbarLayout(...)`，按游戏实际节点（`Sidebar.OffsetRight`、`CardGrid.OffsetLeft/OffsetTop/OffsetRight`、`CardGrid/ScrollContainer.OffsetLeft/OffsetRight`、`Sidebar/MarginContainer` 的 `margin_top`）动态相对定位（无硬编码屏幕像素）：
    - 左缘对齐 `CardGrid/ScrollContainer` 左内边距（正式/测试版为 `288 + 50 = 338`，完全避开 `Sidebar` 与 `BackButton`）；
    - 右缘对齐 `ScrollContainer` 右内边距（`-150`，避开右侧 `Scrollbar`）；
    - 顶部对齐 `SearchBar` 顶边距（`16`，高 `44`），并将 `CardGrid.OffsetTop` 同步下移至工具栏下方（`+8` 间距），不遮挡首行卡牌；
  - 统一 `ShowForFilter`、`SyncToSelectedFilter`、`UpdateLibrarySourceIndicators`、`ResetAvailabilityFilter` 经 `FindToolbarSelector` / `FindAvailabilityFilter` 查找节点。

---

## 三、骨妹切皮肤弹「应用角色外观失败：OSTY: independent summon mod owns this body」

### 1. 根因实证

- 当独立召唤 Mod（`OstyDefectImpostor`）接管奥斯提身体时，`CharacterAppearanceRuntime.TryRebuildCreatureVisuals`（`L1662-1666`）按原设计跳过重建奥斯提节点并返回 `"independent summon mod owns this body"`，避免删掉独立 Mod 挂载的骨架；
- 骨妹（`necrobinder`）切皮肤时，`ApplyRunAppearanceRefresh` 会顺带检查其宠物的 `ownerAffected` 分支，汇总出 `"OSTY: independent summon mod owns this body"`；此时骨妹本身已成功切换，`RequestSelection` 返回 `State = AppearanceSelectionRequestState.Applied` 并记录 `ModLog.Warn("皮肤选择已保存，但部分实战外观将在下次创建时生效：...")`（`godot.log` L6743/6780/6833/7030/7069）；
- 但 `CharacterAppearanceScreen.SetAppliedStatus(string? liveRefreshError)`（`L2341-2352`）只要 `liveRefreshError` 非空，就无条件拼接了 `ModText.AppearanceFailed`（`"应用角色外观失败"`）并标红警告，把已成功生效的切肤错报成失败。

### 2. 修复内容（未改动 `CharacterAppearanceRuntime.cs` 中奥斯提归位与门控逻辑）

- **`STS2SkinChanger/Core/ModLocalization.cs`**：
  - 新增 `ModText.AppearanceSavedNextTime`，补齐全部 15 种语言（`eng`/`zhs`/`zht`/`deu`/`esp`/`fra`/`ita`/`jpn`/`kor`/`pol`/`ptb`/`rus`/`spa`/`tha`/`tur`，简体中文：`"已保存；部分外观将在下次创建时生效"`）。
- **`STS2SkinChanger/Ui/CharacterAppearanceScreen.cs`**：
  - 新增 `FormatAppliedStatus(string? liveRefreshError)` 并更新 `SetAppliedStatus`：
    - 当全部延迟刷新项均仅为独立召唤 Mod 接管提示（`independent summon mod owns this body`）时，返回 `null`（不弹误导性警告，日志仍保留原记录）；
    - 当 `State == Applied` 但存在其他延迟生效项（如死亡动画期间）时，展示本地化的 `ModText.AppearanceSavedNextTime`（非失败样式），不再报「应用角色外观失败」；
    - 真正失败（`AppearanceSelectionRequestState.Failed`）仍保留 `ModText.AppearanceFailed`。

---

## 四、新增回归测试与双分支验证结果

- **新增测试**：`tests/STS2SkinChanger.RuntimeTests/HinaUiOstyRegressionTests.cs`（`--test-hina-ui-osty`，已接入默认无参 `RuntimeTests`）：
  1. 校验 `IsManagedResourceOwnershipPatch` 拦截 `CardModel.get_PortraitPath` / `get_Portrait` 且不误杀 `CardPoolModel.DeckEntryCardColor`；实挂 Harmony 后置补丁复现基线路径被劫持为 `res://SorasakiMana/...`，剥离后验证游戏默认恢复为原版路径；
  2. 校验 `CardSkinControls.ComputeLibraryToolbarLayout` 在 `1920×1080` 与 `1280×720` 下均不与 `BackButton`（含阴影）、`Sidebar`、`Scrollbar` 或首行卡牌重叠；
  3. 校验 `CharacterAppearanceScreen.FormatAppliedStatus` 对 `OSTY: independent summon mod owns this body` 返回空（不弹失败）、对其他延迟项返回全部 15 种语言的 `ModText.AppearanceSavedNextTime` 且与 `AppearanceFailed` 区分。
- **全部改动文件行尾**：均核实为纯 LF（`CRLF=0`）：`ManagedSkinModLoader.cs`、`CardSkinControls.cs`、`ModLocalization.cs`、`CharacterAppearanceScreen.cs`、`HinaUiOstyRegressionTests.cs`、`Program.cs`。
- **双分支验证结果**：

| 分支 | 主工程 Release 构建（`-o %TEMP%`） | `LogicTests` | 完整无参 `RuntimeTests` | 受保护角色实包回归（`check-protected-characters.ps1` 等效 5 条命令） |
| --- | --- | --- | --- | --- |
| 测试版（v0.113.1） | **0 警告 / 0 错误** | **通过** | **通过**（含 `HinaUiOstyRegressionTests`、`OrchisRoomCallbackTests`、`CreatureVisualLifecycleTests`，尾行 `Workshop policy tests passed.`） | **通过**（5 项全绿，含十字军实包钩子与贞德 SFW/NSFW 实包） |
| 正式版（v0.111.1） | **0 警告 / 0 错误** | **通过** | **通过**（尾行 `Workshop policy tests passed.`） | **通过**（5 项全绿） |

---

## 五、实机验证步骤

1. **空崎日奈卡图隔离（测试版 v0.113.1）**：
   - 选愚者角色皮肤为「空崎日奈替换愚者」，并在卡牌总览将愚者卡图设为「游戏默认」；
   - 核对 `godot.log`：日奈启用日志中接管的资源入口数增加（不再出现 `fool/__base__/res://SorasakiMana/images/cards/...` 报错），卡牌总览及战斗中愚者卡均显示**游戏原版卡图**；
   - 在卡牌总览将愚者卡图切为日奈卡图包时，正常显示日奈卡图，且日奈的语音、卡框配色与战斗特效仍正常。
2. **卡牌总览筛选条布局（正式版 v0.111.1 + 测试版 v0.113.1）**：
   - 打开卡牌总览，确认「皮肤优先级 (N)」「预设」按钮与「有皮肤卡牌」勾选框位于卡牌区顶部横向工具栏内，与左下角返回箭头、左侧栏、滚动条及首行卡牌均无重叠；
   - 勾选/取消「有皮肤卡牌」及点击「皮肤优先级」「预设」弹窗功能正常。
3. **骨妹 + 独立奥斯提 Mod 切换提示（测试版 / 正式版）**：
   - 启用 `OstyDefectImpostor` 并让奥斯提选用该独立 Mod 皮肤，在战斗中多次切换骨妹皮肤；
   - 确认骨妹皮肤正常切换、奥斯提外观与站位保持正常，界面**不再弹出**「应用角色外观失败：OSTY: independent summon mod owns this body」。
