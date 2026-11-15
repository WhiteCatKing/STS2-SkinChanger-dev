# 皮肤包「外观变体」拆成独立皮肤选项（设置驱动型）

- 状态：**未解决**（1.0.26.70 已实现，待实机验证）
- 记录日期：2026-11-07
- 需求来源：作者本人（示例：`雪霁&森森改 贞德战士可战损版` / `JoanofArcIroncladHpwear`）

## 需求

很多皮肤包在**自己的设置**里提供多套外观变体（贞德：原色 / 黑贞德 / Saber 王庭蓝 / 霓虹审判官），
玩家通常不会去翻 mod 介绍，等于这些作者的劳动没人看到。要求像已兼容的 `CrusaderSkin`
（工坊 3801530111，走"同目录兄弟场景"）那样，**把每个变体变成下拉里的独立皮肤**，
命名 `<皮肤名>_<变体名>`，并且要通用：下次遇到同类包能自动识别。
另外：这种包的"本体"就是第一个变体（原色），所以下拉里**不应再保留没有 `_` 后缀的那一条**。

## 两种变体形态

| 形态 | 例子 | 现有实现 |
| --- | --- | --- |
| 每个变体一个场景 `scenes/creature_visuals/<组>_<变体>.tscn` | CrusaderSkin | `Catalog/ProviderVariantOptions.cs`（已有） |
| 一个场景 + 皮肤包设置里的调色板/样式值 | JoanofArcIroncladHpwear（RitsuLib 设置） | **本次新增** `Catalog/ProviderSettingsVariantOptions.cs` |

## 1.0.26.70 的关键行为

1. **原选项被"认领"为默认变体**：从设置类型的无参构造里读出默认值（贞德 `Appearance = 1`），
   若它对应某个标签，就把**原选项 id 改名成 `<皮肤名>_<该标签>`**（`战士贞德 血量换装_原色`），
   不再额外保留无后缀条目；存档里原来的选择 id 继续有效，且语义就是"那个默认变体"。
   读不出默认值时退化为"原选项 + 全部变体"（不会丢功能，只是多一条）。
2. **总共 N 条**，贞德 = 4 条：`_原色`、`_黑贞德`、`_Saber 王庭蓝`、`_霓虹审判官`。
3. **不叠两层**：已经由 `ProviderVariantOptions` 展开过兄弟场景变体的包（CrusaderSkin）会被跳过。

## 检测契约（元数据扫描，不加载程序集、不硬编码 mod 名）

每个环节都是"名字/能力"驱动，任何一步不满足就完全不碰这个包：

| 环节 | 支持的形式 |
| --- | --- |
| 设置对象 | 静态 `Current`/`Instance`/`Settings`/`Config`/`Configuration`/`State`/`Data`/`Runtime`/`Defaults` 的**属性、字段或无参静态方法**；兜底：任何返回该设置类型的静态属性 |
| 外观通道 | 设置对象上可写的 `int` 或 **enum** 属性；属性名按 PascalCase/下划线分词后与 appearance/variant/palette/outfit/costume/style/look/form/visual/skin/preset/color/theme/model **整词匹配**；带 version/count/size/hash/scale/offset/alpha/volume/priority… 等词的一律排除（避免 `VisualSelectionDefaultsVersion` 这类误判） |
| 变体标签 | 静态 `string[]` / `List<string>` / `IReadOnlyList<string>` / `IEnumerable<string>`（含属性背后的 `<Names>k__BackingField`），从 `.cctor` 的字符串字面量按该字段的 `stsfld` 归属读取；**或** enum 的成员名 |
| 标签可信性 | 名字含通道词（`AppearanceNames`）或"裸通用名"（`Names`/`Labels`/…）且该通道有 `Persist<通道>` 写入口；否则不认（`ReactionNames` 这类不会被误当变体名） |
| 写入口 | 静态 `Persist*` / `Set*` / `Save*` / `Apply*` / `Write*` / `Update*` 且类型为 `Action<int>`/`Func<int>` 风格；**没有也能工作**（直接写属性） |
| 默认变体 | 无参构造 IL 里 `ldc.i4.* ; stfld <通道>` 的常量；enum 则按枚举值定位 |
| 刷新 | 可选：任意类型上的静态 `Refresh*(SceneTree)` / `Apply*(SceneTree)`（贞德是 `Palette.RefreshAppearance`） |
| 应用时校值 | 0/1 基址按默认值推断，写入后用读回值校验，不匹配就换下一组候选（皮肤包 clamp 也能兜住） |

## 实测（离线）

- `--audit-settings-variants` 扫本机 **51 个 mod DLL**：只有 `JoanofArcIroncladHpwear` 命中，
  我们自己 DLL 的误报（`SkinConfig.VisualSelectionDefaultsVersion` + `ReactionNames`）已在 1.0.26.70 消除。
- 贞德契约：`runtime=WearSettingsRuntime.Current settings=WearSettings.Appearance
  persist=PersistAppearance labels=[原色|黑贞德|Saber 王庭蓝|霓虹审判官]`，默认值 1 → 原选项认领 `原色`。
- 目录展开后 ironclad 组正好 4 条：
  ```
  [JoanofArcIroncladHpwear]           战士贞德 血量换装_原色
  [JoanofArcIroncladHpwear::variant:1] 战士贞德 血量换装_黑贞德
  [JoanofArcIroncladHpwear::variant:2] 战士贞德 血量换装_Saber 王庭蓝
  [JoanofArcIroncladHpwear::variant:3] 战士贞德 血量换装_霓虹审判官
  ```
  （诊断脚本用 id 当名字；真实运行取 manifest 的 `name`。）

## 已知边界（不保证 100% 覆盖的情形）

- 标签只能在**运行时**算出（例如 getter 里拼出来），或标签是本地化 key 而不是显示名 → 不识别 / 会显示 key。
- 通道属性在**嵌套对象**里（`Current.Palette.Appearance`）→ 暂不支持；当前只处理一层。
- 一个包有**多个**外观通道时只取评分最高的一个。
- 设置对象只以实例形式存在（没有静态 `Current`/`Settings` 之类）→ 不识别。
- 下拉**预览**不预演变体配色（调色板是全局设置，预览时切换会闪），选中后才变。
- 变体设置是**本机全局**的：联机时不影响队友，队友也改不了你的变体。
- 只处理角色组（`res://scenes/creature_visuals/`），怪物/卡牌的设置型变体未纳入。

遇到新的同类包，可以直接跑
`RuntimeTests --audit-settings-variants <该包.dll>` 看能不能识别；识别不到时按上表对症补规则。

## 实机验证

1. 选人界面下拉：商人/铁甲战士组里应出现 5 条（原条目 + 4 个变体）。
2. 选 `战士贞德 血量换装_黑贞德` → 战斗里立绘变黑贞德配色；再选 `_霓虹审判官` → 变霓虹配色。
3. 重启游戏：选中的变体保持不变（每次外观重建都会重新应用）。
4. 日志搜 `已把` / `外观变体切到`：应出现"已把 战士贞德 血量换装 的 4 个外观变体注册为独立皮肤选项"
   与"已把 ...WearSettings 的外观变体切到「黑贞德」"。
