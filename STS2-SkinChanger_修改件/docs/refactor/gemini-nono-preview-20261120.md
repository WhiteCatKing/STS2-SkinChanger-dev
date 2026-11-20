# #249 NonoFoolSkin 选人小人预览显示原版愚者修复（Gemini）

- **工作树**：`D:\mod\工作2\上游修复_1.0.35\STS2-SkinChanger_修改件`（主工作区，基于当前工作区现状；未提交、未部署）
- **日期**：2026-11-20
- **改动文件**：仅修改 `STS2SkinChanger/Core/IndependentFoolSkinCompat.cs`，新增 `tests/STS2SkinChanger.RuntimeTests/NonoFoolPreviewTests.cs`，并在 `tests/STS2SkinChanger.RuntimeTests/IndependentFoolSkinTests.cs` 与 `Program.cs` 接入测试；未触碰 `ProtectedCharacters/*`、`FrameworkModelPreview.cs`、`FrameworkSkinRuntimePatches.cs`、`ContextualSkinControls.cs`、`ProviderModelDiscovery` 或第三方 Mod 文件。

---

## 一、根因实证（日志 `godot.log` 4543–4593 + `NonoFoolSkin.dll` 静态 PE/IL 反汇编）

1. **`NonoFoolSkin.CharacterVisualsPatch.OnCreateVisuals` 不做模型替换**：
   - `godot.log:4589` 显示切换器在选角预览期间确实重放了 `NonoFoolSkin.CharacterVisualsPatch.OnCreateVisuals`，但静态反汇编证实该函数仅调用 `ArtStore.Preload("idle_00.png")`，不对 `NCreatureVisuals` 做任何节点挂载或隐藏。
2. **真实模型替换位于 `NonoFoolSkin.CreaturePatches.OnCreatureReady(NCreature)`**：
   - 静态 IL 显示 `OnCreatureReady` 读取 `AnimDirector.Config` → 经 `ArtStore.Get("idle_00.png")`（或静态兜底 `character.png`）解码贴图 → 隐藏 `NCreatureVisuals.Body` → 调用 `Rig.Create(visuals, open, closed, layout)` 挂上 `NonoFoolSkin_Body`（`Sprite2D`）与 `NonoFoolSkin_Frames`（`AnimatedSprite2D`）→ 经 `AnimDirector.SetRig` / `AnimDirector.Play("idle")` 调用 `BuildFrames("idle", def)` 与 `Rig.ShowFrames(...)` 播放 73 帧循环待机动画。
3. **选角预览路径跳过 `NCreature._Ready` 且 `IndependentFoolSkinCompat.ApplyPreview` 未接入 `NonoFoolSkin`**：
   - `CharacterAppearanceRuntime.ReplaySelectedCreatureNodeReady`（`CharacterAppearanceRuntime.cs:2117-2120`）对 `FrameworkPreviewCreature` 直接早退（保护战斗绑定型包不因 `NCreature.UpdateBounds` 崩溃）；
   - `FrameworkModelPreview.TryRefresh`（`:74`）随后调用 `IndependentFoolSkinCompat.ApplyPreview(visuals, groupId)`，而该函数原先只处理了 `assembly.GetName().Name == "FoolSkinStatic"`，对 `"NonoFoolSkin"` 直接返回，导致预览里一直显示未隐藏的原版愚者 Spine 模型。

---

## 二、代码改动

1. **`STS2SkinChanger/Core/IndependentFoolSkinCompat.cs`**：
   - 将原 `ApplyPreview` 中的 `FoolSkinStatic` 逻辑无改动抽为 `ApplyFoolSkinStaticPreview`，并新增 `NonoFoolSkin` 分支 `ApplyNonoFoolPreview(Assembly assembly, NCreatureVisuals visuals)`：
     - 读取 `NonoFoolSkin.AnimDirector.Config`（若为空先调 `AnimDirector.Initialize()`）；
     - 按 `animations["idle"]` 的 `Prefix + "00.png"`（兜底 `File ?? "character.png"`）经 `ArtStore.Get` 取得首帧贴图；若贴图缺失则直接返回，不隐藏原版模型；
     - 调用 `Rig.Create(visuals, open, closed, layout)` 挂载 `NonoFoolSkin_Body` 与 `NonoFoolSkin_Frames`；
     - **零全局状态污染**：**不调用 `AnimDirector.SetRig` / `AnimDirector.Play`**（避免覆盖局内战斗实体的全局 `AnimDirector._rig` 或命中 120ms 防抖），并立即从静态字典 `Rig.Rigs` 中摘除刚创建的预览 `Rig`（按引用匹配键，零残留、不泄漏已释放的预览 `NCreatureVisuals`）；
     - 当 `idle.Mode == "frames"` 时直接调用 `AnimDirector.BuildFrames("idle", idleDef)` 与 `rig.ShowFrames(frames, speed, loop, targetHeight, returnTo)` 播放动态待机（`FrameworkModelPreview.MeasureCaptureSeed` 原生支持测量 `AnimatedSprite2D`）；帧图缺失或静态模式时按 `idle.Fallback`（如 `idle_static`）回落到 `rig.ShowStatic(staticOpen, true)`；
     - 最后通过 `GameBranchApi.CurrentBody(visuals)?.Hide()` 隐藏原版愚者模型（双版本兼容正式版 `GetCurrentBody()` 与测试版 `Body`）。
2. **`tests/STS2SkinChanger.RuntimeTests/NonoFoolPreviewTests.cs`（新增）** + **`IndependentFoolSkinTests.cs`** + **`Program.cs`**：
   - `VerifyRealPackageContract`：纯静态使用 `System.Reflection.Metadata.PEReader`（不加载第三方 DLL）+ `PckArchive.Open` 校验真实工坊包 `3841262975`（`NonoFoolSkin.dll` / `NonoFoolSkin.pck` / `animations.json`）中 `ApplyNonoFoolPreview` 依赖的全部类型、方法、字段及 `idle_00.png` 资源完整存在；同步接入 `IndependentFoolSkinTests.cs`。
   - `VerifyPreviewBehavior`：通过动态构造的同合同 `NonoFoolSkin` 程序集端到端执行 `IndependentFoolSkinCompat.ApplyPreview`，覆盖：① 帧动画模式（`BuildFrames` + `ShowFrames` + 隐藏原版 Body + `AnimDirector._rig` 不被覆盖 + `Rig.Rigs` 零残留）；② 静态回落模式（`idle_static` + `ShowStatic`）；③ `Config == null` 自动初始化；④ 贴图缺失时不隐藏原版 Body；⑤ 切到 `NailongFool` / 原版默认 / 非愚者角色时不触发 Nono 且零残留。

---

## 三、编译与自动化验证结果（正式版 + 测试版双端全绿）

- **正式版（v0.111.1，`D:\mod\游戏版本备份\正式版_v0.111.1_20261117_2249_build26655341\data_sts2_windows_x86_64`）**：
  - `STS2SkinChanger` Release 构建（`-o %TEMP%\gemini-nono-build-official`）：**0 警告，0 错误**。
  - `STS2SkinChanger.LogicTests`（`SetErrorMode(3)`）：通过（末行 `Skin Changer logic policy tests passed.`）。
  - `STS2SkinChanger.RuntimeTests` Release 构建：**0 警告，0 错误**；**完整无参 RuntimeTests** 通过，输出 `NonoFoolSkin preview contract & runtime isolation passed.`，末行 `Workshop policy tests passed.`（日志 `%TEMP%\gemini-nono-runtime-official.log`）。
- **测试版（v0.113.1，`C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64`）**：
  - `STS2SkinChanger` Release 构建（`-o %TEMP%\gemini-nono-build-beta`）：**0 警告，0 错误**。
  - `STS2SkinChanger.LogicTests`（`SetErrorMode(3)`）：通过（末行 `Skin Changer logic policy tests passed.`）。
  - `STS2SkinChanger.RuntimeTests` Release 构建：**0 警告，0 错误**；**完整无参 RuntimeTests** 通过，输出 `NonoFoolSkin preview contract & runtime isolation passed.`，末行 `Workshop policy tests passed.`（日志 `%TEMP%\gemini-nono-runtime-beta.log`）。
- 未触碰 `ProtectedCharacters` 或共享核心入口，未提交、未部署；需实机确认步骤见下节。

---

## 四、实机验证步骤

1. 打开测试版（v0.113.1），进入标准选人界面并选中 **愚者（FOOL）**。
2. 在皮肤下拉框选择 **愚者动画皮肤 / Nono Fool Skin**：
   - 确认选人界面右下/预览区域的**小人预览立即切换为 Nono 的动态待机动画**（不再显示原版愚者），且构图尺寸正常、无原版模型重叠；
   - 确认背景大图动态壁纸与底部角色头像仍正常显示。
3. 在同一界面依次切到 **Fool_Waifu**、**NailongFool**、**FoolSkinStatic**、**游戏默认**，再切回 **NonoFoolSkin**：
   - 确认切走后原版愚者或其他皮肤的小人预览均正常、无 Nono 节点或状态残留；切回 Nono 后小人动画预览稳定复现。
4. 用 Nono 进入一局战斗，确认局内战斗小人待机/出牌动画正常；局内或外观界面打开预览后再回到战斗，确认战斗小人的出牌/受击动画依然正常触发（`AnimDirector._rig` 未被预览劫持）。
