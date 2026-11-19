# 交接：魅魔战士在恐惧 / 极速模式选人背景（DSH → GPT，2026-11-19）

Claude 在 #180 把这项转给你。云 12:50 实测：**#179 的部署仍不生效，叠加层挂上了但看不见**；贞德本来就正常（说明另有覆盖路径或它的包视觉更"自给自足"）。下面是这条线的全部结论、改动、已验证与未验证点，以及我建议你优先查的方向。

## 1. 背景与根因（已确认）

- 目标包：`succubusIroncladHpWear`（`mods\_maintainer_set\本地_皮肤_战士_a大魅魔战士可战损版`，只读）。PCK 里选人资产是 `res://succubus_ironclad_hpwear/ironclad/cs_stage1.*`、`cs_stage2.*`（两档）。
- 反编译确认：`BattleWear.SuccubusCharacterSelect` 是
  ```csharp
  [HarmonyPatch(typeof(NCharacterSelectScreen), "SelectCharacter")]
  private static void Postfix(NCharacterSelectScreen __instance, object[] __args)
  ```
  Postfix 里取 `NCharacterSelectScreen._bgContainer`（`Cache.HarmonyField`），再 `GetNodeOrNull<Control>(<角色>_bg)`，然后 `CallDeferred(() => Attach(background))`；`Attach(Control background)` 在 `<角色>_bg` 下找 Spine、给 `skeleton_data_res` 换成 `cs_stage*`，并挂一个 `Control`（名字 `SuccubusSkinPreviewClick`）当点击切换热区。
- 因此，**模式界面（`NDreadScreen` / `NBlitzScreen`）从不调用标准界面的 `SelectCharacter`，这个 Postfix 永远不会跑**；同时这个包没有规范背景场景（`char_select_bg_ironclad.tscn`），我们通用的「背景重做」按设计会跳过它。两条原因叠加 → 原版背景。
- 这个包的 `Attach(Control)` 是**与屏幕类型无关**的：只依赖 `<角色>_bg` 这个 Control。这就是我选择用反射调用它、而不是重放 Postfix 的原因（Postfix 的第一个参数是 `NCharacterSelectScreen`，把 `NDreadScreen` 传进去会 `ArgumentException`，用未初始化对象又会在 `GetNodeOrNull` 时炸）。

## 2. 我做的事（都在 `D:\mod\工作2\上游修复_1.0.35`，未提交、未改版本号）

1. **`Ui/CharacterSelectModeBackgroundPatch.cs`（新增）**
   `Postfix(Control __instance, CharacterModel __0)`：先 `ContextualSkinControls.ReapplySelectedBackground`，再 `ProviderModeSelectOverlayBridge.Attach`。
2. **`Ui/CharacterSelectModePatches.cs`**
   `Install` 里新增 `BindBackgroundRefresh(type, "SetSelectedCharacterVisuals")`：按类型反射给模式屏幕的这个私有方法挂 `Priority.Last` 的 Postfix（没有这个方法的版本自动跳过，例如 `NBlitzScreen`）。
3. **`Ui/ProviderModeSelectOverlayBridge.cs`（新增）**
   只在非标准选人界面生效：`SkinService.GetSelectedCreatureRuntimeProvider(groupId)` 拿到当前选中提供者 → 遍历 `AppDomain` 程序集，用 `ManagedSkinModLoader.IsProviderAssemblyFor(providerId, assembly)` 过滤 → 反射找 `static Attach(Control)` 并传入模式界面的 `<角色>_bg`。未选中时提供者 id 为空直接跳过；包内部的 `SkinSelectionGate.MenuAllowed()` 与「已有 catcher 不重复挂」是第二道保险。
4. **`Ui/ContextualSkinControls.cs`**
   - 新增 `ReapplySelectedBackground(screen, character)`（第 2253 行）：仅重做背景（用 `SkinService.WithRuntimeResources` 从隔离依赖里实例化 `char_select_bg_<char>`，再走既有 `ReplaceCharacterBackground`；包里没有规范场景就什么都不换）。
   - `ShowCharacter` 末尾（第 285 行附近）：非标准界面补一次 `ProviderModeSelectOverlayBridge.Attach`（覆盖没有 `SetSelectedCharacterVisuals` 的极速界面）。
   - 修好 `DescribeCharacterSelectBackground`（第 2316 行）：它原来算了一堆 `AnimatedBg` 子节点状态却**从不输出**，现在会打 `[选角背景] 分组/角色：子节点/可见…`。
5. **测试** `tests/.../CharacterSelectModeBackgroundTests.cs`（`--test-mode-background`）：断言 `NDreadScreen.SetSelectedCharacterVisuals(CharacterModel)` 存在、签名正确、`CharacterSelectModePatches.Install` 确实把 Postfix 挂上去。
6. 文档：`docs/refactor/fix-upstream-1.0.35-dsh.md` 的「二、极速/恐惧选人背景」+「补充：魅魔战士…」两节。

## 3. 已验证 / 未验证

**已验证（离线）**
- beta（v0.113.1）与官方 v0.111.1 引用：主工程 0 错误 0 警告。
- `--test-mode-background`、`--test-merchant-lifecycle` 通过。
- 完整无参数 RuntimeTests 绿色（末行 `Workshop policy tests passed.`）；LogicTests 全绿。
- 云 12:50 日志里能看到我的桥确实执行了：`[选角背景] 已为 succubusIroncladHpWear 在模式界面重放提供者的选人叠加层（SuccubusCharacterSelect.Attach）`，且包自己在 3087–3106 行加载了 `cs_stage1`、挂了 wear safety poller、打印了 "click background to toggle modified skin"。

**未验证 / 已知失败**
- **画面仍看不见**（云实测）。`IRONCLAD_bg 子=3`（3084/3114）说明背景容器里有东西，但叠加层不可见。
- 极速（`NBlitzScreen`）没有实测证据；贞德包是否也走 `Attach(Control)` 未确认（若没有同形方法，我的桥对它不生效，但云说贞德本来就正常，说明它有别的路径）。
- 切走皮肤后的残留清理由包的 `SkinSelectionGate`/自身逻辑负责，未实测。

## 4. 我建议你优先查的（按可能性排序）

1. **尺寸/位置/锚点**：`Attach` 里挂的点击热区与 Spine 都挂在 `<角色>_bg` 下；Dread 的背景是 `_bgContainer`（`%AnimatedBg`）里再套 `<角色>_bg`，而 `NCharacterSelectScreen` 的层级/尺寸可能不同。请打印 `IRONCLAD_bg` 的 `Size`、`GlobalPosition`、`Scale`、`ClipContents`，以及新挂节点的 `GetRect()`/`Visible`/`ZIndex`。
2. **被模式界面自己的层盖住**：Dread 有 `_grayscaleBg`（`%GrayScaler` 的 ShaderMaterial）、`_chainVisualization`、`_rotatingImage` 等。确认叠加层在兄弟节点中的位置（`GetIndex()`）与这些层的 `ZIndex`；必要时在切换器里把它 `MoveToFront`（只动我们自己挂的那棵子树，不动包资源）。
3. **`Attach` 之后背景被换新**：Dread 连胜动画/`SetSelectedCharacterVisuals` 会清空 `_bgContainer`。我的 Postfix 挂在它的末尾，理论上重挂；但 `Attach` 内部拿到的 `background` 是**旧实例**的引用风险要排除——请确认日志里 `Attach` 的次数与 `_bgContainer` 子节点替换次数是否对得上（可以给 `ProviderModeSelectOverlayBridge` 加一行 `background.GetInstanceId()` 与 `AnimatedBg.GetChild(0).GetInstanceId()` 对比）。
4. **`SkinSelectionGate.MenuAllowed()` 的门控**：它可能只在标准界面的某些状态下返回 true；若 `Attach` 早退，日志里不会有 `click background…`，但 12:50 日志有，说明确实进了 `Attach` 的后半段——所以优先查 1/2。
5. **包的两档资源是否真的应用到 Spine**：`cs_stage1` 加载了，但 `skeleton_data_res` 是否成功替换、Spine 是否有有效贴图/尺寸，需要看包自己的日志（它打包了自己的日志前缀）。

## 5. 约束（照任务书）

- 不改第三方包文件；修在切换器。
- 标准模式与「未选中该皮肤」不能受影响：我的桥只在 `screen.GetType() != typeof(NCharacterSelectScreen)` 且 `GetSelectedCreatureRuntimeProvider` 非空时执行；你的实现请保持这个边界。
- 不 commit、不改版本号；两种引用 0/0、LogicTests、完整 RuntimeTests 全绿；结论和实测步骤回 `--to=Claude`。

## 6. 我这边可以直接配合的

- 需要我加临时诊断（打印叠加层 Rect/ZIndex、`background` 实例 id、`MenuAllowed` 的反射结果），说一声，10 分钟内给。
- 如果你发现我的桥方向不对（例如 Dread 的 `<角色>_bg` 不是包要找的那个节点），直接改或告诉我，我不再动同一批文件避免撞车。
