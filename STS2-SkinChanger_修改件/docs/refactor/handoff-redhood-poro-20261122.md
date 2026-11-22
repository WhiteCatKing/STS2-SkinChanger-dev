# 交接：小红帽（RedHood）两包兼容 + 魄罗独立皮肤（1.0.38.1 → 1.0.38.12）

交接对象：接手皮肤切换器、继续处理"小红帽与魄罗 · 亡灵契约师外观"（工坊 `3841828104`，提供者 id `RedHoodNecrobinder`）与"小红帽 · 战士外观"（`3841824454`，`RedHoodIronclad`）兼容问题的人。

产品目标（维护者原话，2026-11-22）：**骨妹本体可以选择任何皮肤，奥斯提也可以选择任何皮肤，两者是彻底独立拆开的个体**；魄罗的两套外观就叫「魄罗 · 原皮」「魄罗 · 女仆」。

## 一、这两个包的特殊之处

- 整包外观：自带 `has_pck` + `has_dll`，`src/Mod.cs` 自己打了一套 Harmony 补丁（选人界面、战斗视觉、背景、奥斯提等），不经过我们的"资源替换"框架。包的 `min_game_version` 是 `0.113.1`。
- 背景与立绘：`CharacterSelectBg` getter 在 `src/Mod.cs:535`（`internal static class SelectBackground`）返回 `Mod.SelectScene = res://<Id>/portraits/selection.tscn`；该场景本身**只有一块全屏深色 ColorRect**，立绘靠它自己的 `Decorate` 在运行时把 `RedHoodPortrait` 加进去。
- 变体（原皮/女仆）：`src/Mod.cs:48 public static bool Maid { get; private set; }`，唯一入口 `src/Mod.cs:311 public static void SetAppearance(bool maid, bool effects, bool audio, bool save = true)`。没有并列场景、没有 `res://scenes/creature_visuals/` 资源——所以现有的 `ProviderVariantOptions` / `ProviderSettingsVariantOptions` 两条变体通道都认不出它。
- 魄罗：`src/NativeVisuals.cs` 里 `CharacterId(bool pet) => pet ? Mod.Maid ? "redhood_maid_bot01" : "redhood_bot01" : …`，即**魄罗形态从角色变体布尔值推导**；骨架经 `res://<Id>/scripts/spine_factory.gd`（`data_for(id)`，从 `spine42/<id>/` 组 `SpineSkeletonDataResource`）取得。`spine42/` 下与本任务相关的目录：`redhood_bot01`（原皮魄罗）、`redhood_maid_bot01`（女仆魄罗）；另有 `fx_death`/`fx_skill_start`/`portrait_*` 等非随从目录。

## 二、我们改了什么（按版本）

| 版本 | 提交 | 内容 |
| --- | --- | --- |
| 1.0.38.1 | `f2bf4464` | 认得两个"自绘角色整包"：`Catalog/DirectCharacterRuntimeTargetScanner.cs` 扫描 DLL 里的目标角色字段，`Core/DirectCharacterRuntimeTargetPolicy.cs` 判定，`IsTargetCharacterField` 放宽 + `DeclaresDirectCharacterRuntimeTarget`；新增 LogicTests 用例 |
| 1.0.38.2–.38.5 | `bbbfc974`/`d3e6f155`/`41f5f0d4` | 选角背景跟随提供者声明（`ReplaceProviderDeclaredBackground` / `TryResolveProviderDeclaredBackground`），`.38.5` 提到通用分支最前 |
| 1.0.38.6 | `d949f404` | 变体接入我方下拉：新增 `ManualCharacterState.AppliesArgument`、`FindArgumentApplier`（找静态 bool 执行器）、`Core/ManualCharacterVariantBridge.cs` 执行/绑定/双向同步 |
| 1.0.38.7/.38.8 | `6a8b8446`/`d2c3d622` | 标签改成读提供者自己菜单的措辞（原皮/女仆），`VariantWord` 只取变体词，按 `providerId|语言` 缓存到 `user://skin_changer_variant_labels.txt` |
| 1.0.38.9 | `75013c04` | 局内皮肤面板也套同一套标签：`Ui/CharacterAppearanceScreen.cs:794 PopulateSkinDropdown()` 调 `ContextualSkinControls.ApplyProviderVariantLabels` |
| 1.0.38.10 | `0475b934` | 魄罗第一版（方向错误，已废弃）：按"提供者程序集能力"识别，结果换掉了角色本体，且必须选中小红帽皮肤才出现选项 |
| 1.0.38.11 | `3275398e` | 重写 `Core/PoroVariantBridge.cs`：只认游戏结构（`PetOwner != null && Monster is Osty`），只换随从骨骼，预览面板接 `TryLoadSkeleton` |
| 1.0.38.12 | `1eda36ac` | 探测从"磁盘解包目录"改为"`res://` 虚拟文件系统"（`Godot.FileAccess.FileExists` + `DirAccess`），两项恢复常驻 |

关键文件：`Core/PoroVariantBridge.cs`（探测 + 应用 + `Creature.CreateVisuals` Postfix）、`Ui/OstyModelPreviewControls.cs:83`、`Core/SkinConfig.cs`（`CompanionProviderVariantId`）、`Core/SkinService.cs`（`ApplyCompanionSelection` 的 `poro:` 分支）、`Ui/OstySkinChoices.cs`、`Ui/CharacterAppearanceScreen.cs`、`Ui/ContextualSkinControls.cs`。

## 三、踩过的坑（不要再踩）

1. **资源不在磁盘上。** 工坊包的资源在 `.pck` 里，只有被选中/挂载过的包才有解包目录。任何"探测包里有什么"的代码都必须走 `res://<提供者>/…`（PCK 启动即挂载），不能用 `Directory.GetDirectories`。1.0.38.11 就是因为这个把选项全弄丢了。
2. **不要用"程序集是否可调用"当判据。** 提供者的 DLL 只有在我们"激活"它时才可用；魄罗这条线只依赖它的**资源**，所以一律走包内 `spine_factory.gd`，与 DLL 是否激活无关。
3. **别碰本体。** 作者给本体和随从打了同一个 `redhood_mod` meta，按 meta 认会连角色一起换掉。判据只能是 `PetOwner` + 怪物类型。
4. **背景替换≠画面正确。** 替换后日志会打 `[选角背景] … 子=1`，但 `DescribeCharacterSelectBackground` 不打印子节点名字，无法区分"原版背景"和"替换后的空壳"；两个方案都要看画面。
5. **构建陷阱。** `dotnet build tests\STS2SkinChanger.RuntimeTests -p:GameAssemblyDir=<测试版>` 会顺带重编产品项目，把 `STS2SkinChanger\bin\Release\Gurio.SkinChanger.dll` 覆盖成"引用测试版"的产物；部署前必须用 `-p:GameAssemblyDir='C:\sts2refs\stable'` 重编并核对 SHA-256。

## 四、当前未解决（2026-11-22 14:53 实测）

**现象**：奥斯提下拉已出现「魄罗 · 原皮 / 魄罗 · 女仆」（维护者截图确认），但**选了不换肤**——选人界面的奥斯提预览仍是原版奥斯提，游戏内也没变。

**日志证据**（`C:\Users\Administrator\AppData\Roaming\SlayTheSpire2\logs\godot.log`，372,326 B / 2,666 行）：

- `{2219} [SkinChanger] 魄罗外观：RedHoodNecrobinder 提供 2 档随从外观（redhood_bot01/redhood_maid_bot01）。` —— 探测已成功。
- `{2264}/{2290}/{2312}/{2333}/{2426}/{2471}/{2516}/{2561}/{2582} 奥斯提皮肤已切换到 poro:base|poro:maid；此后与角色皮肤互不影响。` —— 选择已写入配置。
- `{2246} 奥斯提预览：加载 res://animations/monsters/osty/osty_skel_data.tres，类型=SpineSkeletonDataResource。`、`{2247} 奥斯提预览：骨架包围盒=(846.1461, 782.9721)，缩放=0.2769，位置=(138.63345, 281.62952)。` —— **预览仍走原版骨架**，说明 `PoroVariantBridge.TryLoadSkeleton` 没生效（或没被调用）。
- 全程没有任何 `魄罗外观：取骨架失败` 警告 → 更像是"没进这条分支"而不是"取骨架抛异常"。

**优先验证的三个假设**（按成本排序）：

1. `Ui/OstyModelPreviewControls.cs:83` 的 `PoroVariantBridge.CurrentOptionId` 在预览刷新时为 null（配置写入与预览刷新顺序问题）。
2. `TryLoadSkeleton` 里 `discovery.Factory.Call("data_for", skinId)` 返回的不是 `Resource`（GDScript 返回值可能是 `Variant`/`RefCounted` 包装），`AsGodotObject() is not Resource` 直接 false。
3. 应用到随从的分支没跑到：`ApplyOnCreated`/`RefreshLiveSkins` 的 `PetOwner`/`Monster` 判定与真实战斗实体不一致（例如战斗里 `PetOwner` 为空、或用的是 `Osty` 之外的模型）。

**下一步建议**：在 `TryLoadSkeleton`（discovery 是否 null、返回对象类型、是否抛异常）与 `ApplyTo`（找到的 spine 节点名、data 类型、`set_skeleton_data_res` 调用结果）各加一行 `[DEBUG-poro]` 日志，重新部署让维护者切一次，按日志定分支；两处都覆盖到再删日志。

## 五、还没做的

- 休息场景的魄罗：作者用自建 actor（`src/Mod.cs:242` 的 `PoroRestSkin`，`poro.Name="PoroRestSkin"`）摆放，不走 `Creature.CreateVisuals`，本次未覆盖。
- 选人界面背景：替换是执行了的，但提供者的 `Decorate` 注入没跑，画面仍是深色底（`Decorate`/`RedHoodPortrait` 在日志里命中 0 次）。
- 冷启动偶发"背景大图与预览小人/选项对不上"（维护者 2026-11-22 报，不稳定，只在退出重进时出现）。

## 六、验证与交付

- 每次交付的门禁：`-p:GameAssemblyDir='C:\sts2refs\stable'` Release 构建 0 警告 0 错误 → LogicTests 末行 `Skin Changer logic policy tests passed.` → 完整 RuntimeTests 116 行 `EXIT=0` → 两处 mod 目录部署并回读 SHA-256。
- 1.0.38.12 产物：3,769,856 B / `22B02D0ABCC82F49`。
- 实机步骤（交给维护者）：骨妹选任意皮肤 → 奥斯提下拉选「魄罗 · 女仆」→ 看右侧奥斯提预览是否变成女仆魄罗、骨妹小人不动 → 进战斗看场上随从 → 再看休息点。
