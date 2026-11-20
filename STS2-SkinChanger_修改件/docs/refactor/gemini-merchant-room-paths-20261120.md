# #246 商人房间场景路径按实际游戏 PCK 解析修复（Gemini）

- **工作树**：`D:\mod\工作2\上游修复_商人路径\STS2-SkinChanger_修改件`（分支 `fix/merchant-room-paths`，基于 `a1f2a99`）
- **日期**：2026-11-20
- **文件归属**：仅改动 `Core/Features/Merchant/MerchantRuntimeAppearance.cs`、`Core/Features/Ancient/AncientCompendium.cs`、`tests/STS2SkinChanger.RuntimeTests/MerchantRoomPathTests.cs` 与 `tests/STS2SkinChanger.RuntimeTests/Program.cs`；未触碰 `SkinService*`、`SkinCatalog*`、`ProtectedCharacters/*`；未改动主工作区 `上游修复_1.0.35`；未提交、未部署。

---

## 一、根因

正式版（v0.111.1）游戏包仅包含共享商人房间场景 `res://scenes/rooms/merchant_room.tscn`；测试版（v0.113.x）将其拆分为 `res://scenes/rooms/standard_merchant_room.tscn` 与 `res://scenes/rooms/blitz_merchant_room.tscn` 并删除了 `merchant_room.tscn`。切换器原有三处路径写死：

1. `STS2SkinChanger/Core/Features/Merchant/MerchantRuntimeAppearance.cs`：
   - `MerchantRoomScenePath` 常量写死为 `StandardMerchantRoomScenePath`（`res://scenes/rooms/standard_merchant_room.tscn`）；
   - `MerchantRoomPrewarmPatch.Postfix`（打开地图时预热）直接传 `MerchantRoomScenePath`，导致正式版每次打开地图都在预热不存在的测试版场景；
   - `ResolveMerchantRoomScenePath(NMerchantRoom? room)` 同样无视正式版直接返回 `StandardMerchantRoomScenePath`；
   - `GetMerchantScenePath` 仅靠程序集反射 `MerchantVariantScenes.HasBlitz` 区分分支，未先核对实际加载的游戏 PCK。
2. `STS2SkinChanger/Core/Features/Ancient/AncientCompendium.cs`：
   - `GetOtherEntries` 为普通商人条目的 `ScenePath` 写死了测试版已删除的 `"res://scenes/rooms/merchant_room.tscn"`，当 `merchant_button.tscn` 失败回落到 `entry.ScenePath` 时在测试版会直接失败。
3. `STS2SkinChanger/Catalog/SkinCatalog.cs`（`MerchantAppearancePathRegex`，归龙娘，本次未修改，见下文给 Claude 合入的建议正则）：
   - 仅匹配 `(?:blitz_)?merchant_button|blitz_merchant_room|standard_merchant_room`，漏掉了正式版 `merchant_room.tscn`。

---

## 二、代码改动

1. **`STS2SkinChanger/Core/Features/Merchant/MerchantRuntimeAppearance.cs`**：
   - 新增常量 `LegacyMerchantRoomScenePath = "res://scenes/rooms/merchant_room.tscn"`；
   - 将 `MerchantRoomScenePath` 改为只读属性 `=> GetMerchantScenePath()`，`ResolveMerchantRoomScenePath(NMerchantRoom? room)` 改为直接委托 `GetMerchantScenePath(GroupId, room?.GetType())`；
   - `GetMerchantScenePath` 统一走纯可测的 `ResolveMerchantScenePath(groupId, roomType, hasGameScene, hasBlitzBranch)`：
     - 假商人固定返回 `res://scenes/events/custom/fake_merchant.tscn`；
     - 已初始化 `SkinService.Catalog` 时，直接按当前加载的游戏 PCK 探测（`catalog.IsBaseGameResource(path) || catalog.IsBaseGameResource(path + ".remap")`）：极速商店房间存在 `blitz_merchant_room.tscn` 时返回之；普通商店优先选用 PCK 中实际存在的 `standard_merchant_room.tscn` 或 `merchant_room.tscn`（与编译引用分支解耦）；
     - 仅在 `Catalog == null`（离线/启动早期）或最小测试包不含任一路径时，回落到 `hasBlitzBranch ? StandardMerchantRoomScenePath : LegacyMerchantRoomScenePath`；
   - `MerchantRoomPrewarmPatch.Postfix` 改为显式调用 `MerchantRuntimeAppearance.GetMerchantScenePath()`。
   - **保留的既有修复**：未触碰商人热切换（进树后重放前缀、只传递按钮视觉状态不传 `MouseFilter`/`FocusMode`）、纯商人提供者 `NMerchantCharacter` 补丁剥离与既有 `NMerchantRoom`/`NStandardMerchantRoom`/`NBlitzMerchantRoom` 生命周期补丁。
2. **`STS2SkinChanger/Core/Features/Ancient/AncientCompendium.cs`**：
   - `GetOtherEntries` 中 `"merchant"` 条目的 `ScenePath` 由写死的 `"res://scenes/rooms/merchant_room.tscn"` 改为 `MerchantRuntimeAppearance.GetMerchantScenePath()`。
3. **`tests/STS2SkinChanger.RuntimeTests/MerchantRoomPathTests.cs`（新增）** + **`tests/STS2SkinChanger.RuntimeTests/Program.cs`**：
   - 接入无参全量 `RuntimeTests`；
   - 直接打开本机真实正式版 `SlayTheSpire2.pck`（v0.111.1）与真实测试版 `SlayTheSpire2.pck`（v0.113.1），验证不论 `hasBlitzBranch` 为真或假，解析出的普通/极速/假商人场景均真实存在于对应游戏 PCK 中；
   - 通过真实 `SkinCatalog.Build` + `SkinService.Catalog` 端到端验证正式版布局、测试版布局与 `.tscn.remap` 导出布局下 `MerchantRoomScenePath` / `GetMerchantScenePath` / `ResolveMerchantRoomScenePath` 全部正确命中；
   - 通过 IL 校验 `MerchantRoomPrewarmPatch.Postfix`、`ResolveMerchantRoomScenePath` 与 `AncientCompendiumScreen.GetOtherEntries` 均接入 `GetMerchantScenePath` 且不再写死单分支路径。

---

## 三、给 Claude 合入 `Catalog/SkinCatalog.cs` 的建议正则（本次未改 `SkinCatalog.cs`）

原正则（`STS2SkinChanger/Catalog/SkinCatalog.cs` 约 L9812–9819 / 主工作区 L9890–9897）：

```csharp
    [GeneratedRegex(
        "^(?:res://animations/backgrounds/(?:blitz_)?merchant_room/|" +
        "res://animations/customs/merchant/|" +
        "res://animations/customs/merchant2cute_ii/blitz_merchant/|" +
        "res://scenes/rooms/(?:(?:blitz_)?merchant_button|blitz_merchant_room|standard_merchant_room)\\.tscn$|" +
        "res://scenes/merchant/(?!characters/)[^/]+\\.tscn$)",
        RegexOptions.IgnoreCase)]
    private static partial Regex MerchantAppearancePathRegex();
```

建议改为（补回正式版 `merchant_room.tscn`）：

```csharp
    [GeneratedRegex(
        "^(?:res://animations/backgrounds/(?:blitz_)?merchant_room/|" +
        "res://animations/customs/merchant/|" +
        "res://animations/customs/merchant2cute_ii/blitz_merchant/|" +
        "res://scenes/rooms/(?:(?:blitz_)?merchant_button|(?:blitz_|standard_)?merchant_room)\\.tscn$|" +
        "res://scenes/merchant/(?!characters/)[^/]+\\.tscn$)",
        RegexOptions.IgnoreCase)]
    private static partial Regex MerchantAppearancePathRegex();
```

（注：测试版还新增了 `res://scenes/rooms/blitz_merchant.tscn`，目前是否会被极速商人包替换待确认；若后续遇到替换该路径的极速商人包，可在括号内追加 `|blitz_merchant`。）

---

## 四、编译与自动化验证结果（正式版 + 测试版双端全绿）

- **正式版（v0.111.1，`D:\mod\游戏版本备份\正式版_v0.111.1_20261117_2249_build26655341\data_sts2_windows_x86_64`）**：
  - `STS2SkinChanger` Release 构建（`-o %TEMP%\gemini-merchant-build-official`）：**0 警告，0 错误**。
  - `STS2SkinChanger.LogicTests`（`SetErrorMode(3)`）：通过（末行 `Skin Changer logic policy tests passed.`）。
  - `STS2SkinChanger.RuntimeTests` Release 构建：**0 警告，0 错误**；`--test-merchant-lifecycle` 通过（120 次热切换与输入隔离通过）；**完整无参 RuntimeTests** 通过，输出 `Merchant player character isolation passed.`、`Merchant room path resolution passed.`，末行 `Workshop policy tests passed.`（日志 `%TEMP%\gemini-merchant-runtime-official.log`）。
- **测试版（v0.113.1，`C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64`）**：
  - `STS2SkinChanger` Release 构建（`-o %TEMP%\gemini-merchant-build-beta`）：**0 警告，0 错误**。
  - `STS2SkinChanger.LogicTests`（`SetErrorMode(3)`）：通过（末行 `Skin Changer logic policy tests passed.`）。
  - `STS2SkinChanger.RuntimeTests` Release 构建：**0 警告，0 错误**；`--test-merchant-lifecycle` 通过；**完整无参 RuntimeTests** 通过，输出 `Merchant player character isolation passed.`、`Merchant room path resolution passed.`，末行 `Workshop policy tests passed.`（日志 `%TEMP%\gemini-merchant-runtime-beta.log`）。
- 未触碰主工作区 `上游修复_1.0.35`，未提交、未部署。

### 尚待实机确认项
- 实机进游戏测试尚未执行，需由云/Claude 在正式版与测试版各进一次商店复测（见下节）。

---

## 五、实机验证步骤（正式版 + 测试版）

1. **正式版（v0.111.1）**：
   - 选一套商人皮肤（如 `Merchant2CuteII` / `ATA_Merchant` / `A大商人`）与默认对照各测一次。
   - 开一局并打开地图（触发 `MerchantRoomPrewarmPatch`），核对 `godot.log` 不再出现对 `res://scenes/rooms/standard_merchant_room.tscn` 的预热失败或找不到资源警告。
   - 进入商店，确认商人形象、可点击打开货架与手部显示正常；商店内热切换不同商人皮肤并切回游戏默认，确认无点击失效、无左侧玩家模型污染。
   - 打开「其它图鉴 → 商人」，确认预览加载与热切换正常。
2. **测试版（v0.113.1）**：
   - 分别在标准模式与极速模式打开地图并进入商店，核对 `godot.log` 预热与进店均走 `standard_merchant_room.tscn` / `blitz_merchant_room.tscn`，无 `merchant_room.tscn` 报错。
   - 商店内热切换不同商人皮肤及切回游戏默认，确认商人点击与货架交互正常。
   - 打开「其它图鉴 → 商人 / 极速商人」，确认预览加载与热切换正常。
