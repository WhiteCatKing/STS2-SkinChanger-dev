# 上游修复记录（Gemini）

- **工作树**：`D:\mod\工作2\上游修复_1.0.35\STS2-SkinChanger_修改件`（分支 `fix/1.0.36-stability`，基于 `e23097a`）
- **日期**：2026-11-19
- **对应派单**：#189 / `task-gemini-dread-click-toggle-20261119.md`

---

## 恐惧模式选人背景点击不能切换两档（魅魔 / 贞德）

### 根因（反编译 `sts2.dll`、`succubusIroncladHpWear.dll`、`JoanofArcIroncladHpwear.dll` 与解析 `dread_screen.tscn` / `blitz_screen.tscn` 实证）

1. **包自身的点击热区与门控均未屏蔽恐惧模式**：
   - 魅魔 `BattleWear.SuccubusCharacterSelect.Attach(Control)` 在 `IRONCLAD_bg` 下挂 `SuccubusSkinPreviewClick`（FullRect、`MouseFilter = Stop`），其 `GuiInput` 回调只查 `SkinSelectionGate.MenuAllowed()`、`WearEnabled`、左键按下与 `background.IsVisibleInTree()`，不判断屏幕类型。
   - 贞德由受保护模块 `CharSelectStageCycle` 在 `AnimatedBg` 下挂 `SkinChangerCharSelectStageCatcher`（FullRect、`MouseFilter = Stop`），同时原包在 `IRONCLAD_bg` 下挂 `JoanofArcIroncladHpwearClickCatcher`，两者回调同样不判断屏幕类型。
2. **极速模式 (`NBlitzScreen` / `blitz_screen.tscn`) 无此遮挡**：
   - `AnimatedBg` 直接是 `BlitzScreen` 的子节点，与标准选人界面 `character_select_screen.tscn` 同构，上方无全屏可见的 `MouseFilter = Stop` 控件（`CharacterUnlockAnimationBackstop` 为 `visible = false`）。
3. **恐惧模式 (`NDreadScreen` / `dread_screen.tscn`) 的真实遮挡点**：
   - `AnimatedBg` 位于 `SubViewportContainer/SubViewport` 内部。`SubViewportContainer`（`stretch = true`、`mouse_target = false`）会把背景区域的鼠标点击映射并转发进子视口 `SubViewport`，但 `SubViewport` 内排在 `AnimatedBg` **之后（上方）**的两个纯视觉兄弟节点均未设置 `mouse_filter = 2`，在 Godot 4 中默认为 **`MouseFilter = Stop` (0)**：
     - **`%GrayScaler` (`ColorRect`)**：全屏 `anchors_preset = 15`，常驻 `visible = true`（仅做灰度后处理着色器），100% 拦下所有落入 `SubViewport` 的鼠标点击。
     - **`BigRotatingContainer` (`Control`)**（居中 300×300，常驻 `visible = true`——`NDreadScreen` 只切换其子节点 `%RotatingImage` 的 `Visible`，从不隐藏容器）及其子节点 **`%RotatingImage` (`TextureRect`)**：压在角色正中央 300×300 区域，同样会拦截点击。
   - 反汇编 `NDreadScreen._Ready` 与 `NDreadScreen.SetSelectedCharacterVisuals` 确认，游戏代码只操作 `%GrayScaler.Material` 着色器参数与 `%RotatingImage.Visible`，这两个节点不处理任何鼠标事件。

### 改动说明

仅修改切换器侧，不改第三方包、不改受保护模块 `ProtectedCharacters/`、不改版本号、未提交未部署：

1. **`STS2SkinChanger/Ui/ProviderModeSelectOverlayBridge.cs`**：
   - 新增 `SyncModeBackgroundClickPassthrough(Control screen, CharacterModel? character)` 及纯逻辑可测方法（`IsModeScreenTypeName`、`IsBackgroundClickCatcherName`、`IsBlockingVisualOverlayType`、`ShouldAllowOverlayClickPassthrough`）。
   - 仅当 `screen` 是模式界面且 `AnimatedBg` 的父节点是 `SubViewport` 时生效（标准选人界面与极速界面直接返回，零影响）。
   - 检查 `AnimatedBg` 内是否确实存在当前选中皮肤的活跃点击热区（`SkinChangerCharSelectStageCatcher` / `SuccubusSkinPreviewClick` / `JoanofArcIroncladHpwearClickCatcher` 等可见且非 `Ignore` 的热区控件）：
     - **存在活跃热区时**：将 `SubViewport` 内排在 `AnimatedBg` 之后的非交互视觉控件（`GrayScaler`、`BigRotatingContainer`、`RotatingImage`，显式排除 `BaseButton`/`LineEdit` 等交互控件）的原始 `MouseFilter` 存入节点元数据 `sc_mode_bg_orig_mouse_filter` 后设为 `MouseFilterEnum.Ignore`（不影响着色器渲染与旋转动画），并打印日志 `[选角背景] 已在模式界面放通 ... 背景点击（穿透 ...）`。
     - **切走至原版、其他无点击切换的皮肤或其他角色时**：按保存的元数据立即将上述节点的 `MouseFilter` 恢复原值，并打印恢复日志。
   - 在 `Attach` 与 `AttachRebuiltBackground` 的 `finally` 中调用 `SyncModeBackgroundClickPassthrough`，确保切回原版（`providerId == null`）也能触发恢复。
2. **`STS2SkinChanger/Ui/CharacterSelectModeBackgroundPatch.cs`**：
   - 在 `SetSelectedCharacterVisuals` Postfix 里、`ReapplySelectedBackground` 之后且 `ProviderModeSelectOverlayBridge.Attach` 之前补调 `CharSelectStageCycle.Attach(__instance, __0)`，避免恐惧模式连胜动画清空 `AnimatedBg` 后出现 `SkinChangerCharSelectStageCatcher` 短暂缺失导致放通判定延迟。
3. **`STS2SkinChanger/Ui/ContextualSkinControls.cs`**：
   - 在 `RebuildCharacterDisplay` 三条退出路径调用 `CharSelectStageCycle.Attach(screen, character)` 后同步调用 `ProviderModeSelectOverlayBridge.SyncModeBackgroundClickPassthrough(screen, character)`，保证在恐惧界面内通过下拉框热切皮肤（魅魔 ↔ 贞德 ↔ 原版/其他皮肤）时即时切换放通与恢复。
4. **`tests/STS2SkinChanger.RuntimeTests/CharacterSelectModeBackgroundTests.cs`**：
   - 新增 `VerifyDreadClickPassthroughPolicy()`：覆盖模式/标准界面门控、有无活跃热区的放通与恢复判定、点击热区命名识别、非交互视觉控件过滤（排除按钮等交互控件），以及 4 处调用链 IL 验证。

### 验证结果

- **测试版（v0.113.1）构建**（`-o %TEMP%\gemini-build-beta`）：0 错误、0 警告。
- **正式版（v0.111.1）构建**（`-o %TEMP%\gemini-build-stable`）：0 错误、0 警告。
- **LogicTests**（测试版 + 正式版）：全绿（`Skin Changer logic policy tests passed.`）。
- **完整无参数 RuntimeTests**（`SetErrorMode(3)`，测试版 + 正式版）：全绿，测试版打印 `Character select mode background patch passed.`，双端末行均为 `Workshop policy tests passed.`。
- 未启动或接触游戏进程；未触碰龙娘的 `HildeAncientDialogueCompat` 及本地化相关文件。

### 实测步骤（给云）

1. **恐惧模式 - 魅魔**：铁甲选「魅魔战士血量换装[两档]」，进入恐惧模式选人界面，待背景加载完成（日志应见 `[选角背景] 已在模式界面放通 IRONCLAD 背景点击（穿透 BigRotatingContainer、RotatingImage、GrayScaler）`）。点击角色身体中央及周边背景，确认能正常切换原版/改版两档，且顶部文字/底部按钮/轮换勾选项/返回与出发按钮均可正常点击。
2. **恐惧模式 - 贞德**：在恐惧模式下拉框切到「战士贞德 血量换装」（NSFW 三档）及其变体，点击角色中央与背景，确认可循环切换 1→2→3→1 档（日志出现 `选人界面点角色切战损`）。
3. **切走恢复**：在恐惧模式切回原版铁甲或无点击切换的其他铁甲皮肤，或切换到其他角色，确认日志出现 `[选角背景] 已在模式界面恢复 ... 遮罩点击拦截`，界面视觉与按钮正常、无残留点击响应；再切回魅魔/贞德确认仍可点切。
4. **回归检查**：核对极速模式与标准选人界面下魅魔、贞德的显示和点击切换均保持正常。

---

## 普通商店左侧出现巨大 A 大商人形象（CZN Hilde + MerchantFeminization）

- **对应派单**：#193

### 根因（反汇编 `MerchantFeminization.dll`、`CznCore.dll`、`sts2.dll` + 实测日志 `godot2026-11-19T18.52.46.log` 6395–6410 行）

1. **左边那个巨大形象是玩家自己的商店站立节点 `NMerchantCharacter`**：
   - 游戏本体 `sts2.dll` 中，真商人房间（`NMerchantRoom`）和假商人事件（`NFakeMerchant`）里的 NPC 商人按钮都使用 **`NMerchantButton`**；而 **`NMerchantCharacter`** 是普通商店左侧的玩家角色站立模型（如 `scenes/merchant/characters/fool_merchant.tscn`，其第 0 个子节点 `SpineSprite` 缩放为 `0.47`）。假商人事件 `NFakeMerchant` 甚至不使用 `NMerchantCharacter`。
2. **第三方包误判目标类（`MerchantFeminization.dll` 0.2.0）**：
   - 该包共有 4 个 Harmony 补丁：
     - `MerchantButtonPatch.Postfix`（`NMerchantButton._Ready`）：已通过 `InFakeMerchant`（检查祖先类型名含 `NFakeMerchant`）正确区分并替换真/假商人骨架；
     - `MerchantHandPatch.Postfix`（`NMerchantHand._Ready`）：替换货架手部骨架；
     - **`MerchantCharacterPatch.Postfix`（`NMerchantCharacter._Ready`）** 与 **`MerchantCharacterPatch.PlayAnimationPrefix`（`NMerchantCharacter.PlayAnimation`）**：作者误把 `NMerchantCharacter` 当成「假商人」（硬编码日志 `"[MerchantFeminization] 假商人骨架已替换"`），在普通商店里直接把玩家角色 `NMerchantCharacter` 的第 0 个子节点 `SpineSprite` 替换成 `res://MerchantFeminization/nsfw_fake/nsfw_fake.tres`（乘 `fake_scale = 1.0`）并强制播 `idle_loop`。
3. **为何在 CZN 角色（如 Hilde）+ A 大商人时铺满左半屏**：
   - `CznCore.Presentation.MerchantPresentationRuntime.Apply(NMerchantCharacter)` 为希尔德设置了 CZN 角色的较大 `SpineScale`（约 1.3+，近原版 `0.47` 的 3 倍），并排了延迟播放希尔德待机动画 `stop`。
   - 切换器原先只隔离了 `NMerchantButton`/`NMerchantHand` 等商人节点的 `_Ready` 补丁，把误挂在 `NMerchantCharacter` 上的 2 个补丁留在了全局 Harmony 上（日志 6434 行 `已停用未选中皮肤提供者 MerchantFeminization 的 2 个行为补丁`）。
   - 进入普通商店时，`MerchantCharacterPatch.Postfix` 先后把玩家希尔德的 `SpineSprite` 偷换成 `nsfw_fake.tres`（日志 6397 行 `假商人骨架已替换`），顶着 CZN 角色的大缩放铺满左半屏；随后 `CznCore` 尝试在其上播放 `stop` 动画失败（日志 6402 行 `Merchant SpineSprite missing animation: stop`）。其他商人皮肤（如 `Merchant2CuteII`）不 patch `NMerchantCharacter`，故无此问题。

### 改动说明

仅修改切换器侧，不改第三方包、不改版本号、未提交未部署：

1. **`STS2SkinChanger/Core/ManagedSkinModLoader.cs`**：
   - 新增 `IsMerchantOnlyProvider` / `IsMerchantOnlyProviderGroups` / `IsPlayerMerchantCharacterTarget` / `ShouldStripPlayerMerchantCharacterPatch`：判定「只服务商人/假商人分组（含非角色背景）且不拥有任何玩家角色/随从分组」的纯商人提供者。
   - 在激活补丁过滤 `IsManaged(ProviderPatch)` 中接入 `ShouldStripPlayerMerchantCharacterPatch(patch.Target, providerIsMerchantOnly)`：剥离纯商人提供者误挂在 `NMerchantCharacter` 上的全部 Harmony 补丁（`_Ready`、`PlayAnimation` 等），并输出日志 `[商人隔离] 已屏蔽纯商人提供者 ... 误挂在玩家商店形象 NMerchantCharacter 上的 N 个补丁`。
   - 保留其 `NMerchantButton._Ready`（真商人 + 假商人）与 `NMerchantHand._Ready`（手部）补丁不受影响；角色皮肤与同时含角色+商人分组的混合包也不受影响。
2. **`tests/STS2SkinChanger.RuntimeTests/MerchantPlayerCharacterIsolationTests.cs`** 及 **`Program.cs`**：
   - 新增 `MerchantPlayerCharacterIsolationTests`，验证纯商人/混合包/角色包的归类边界、`NMerchantCharacter` vs `NMerchantButton`/`NMerchantHand` 的剥离范围，以及 `ManagedSkinModLoader` 激活链路的调用 wiring。

### 验证结果

- **测试版（v0.113.1）构建**（`-o %TEMP%\gemini-build-beta`）：0 错误、0 警告。
- **正式版（v0.111.1）构建**（`-o %TEMP%\gemini-build-stable`）：0 错误、0 警告。
- **LogicTests**（测试版 + 正式版）：全绿。
- **完整无参数 RuntimeTests**（`SetErrorMode(3)`，测试版 + 正式版）：全绿，打印 `Merchant player character isolation passed.`，双端末行均为 `Workshop policy tests passed.`。

### 实测步骤（给云）

1. 愚者选 CZN Hilde，商人选 A 大商人（`MerchantFeminization`），普通模式进商店：
   - 确认日志出现 `[商人隔离] 已屏蔽纯商人提供者 A大商人 误挂在玩家商店形象 NMerchantCharacter 上的 2 个补丁`，不再出现 `[MerchantFeminization] 假商人骨架已替换` 与 `[CznCore] Merchant deferred loop animation failed: stop`；
   - 画面左侧正常显示希尔德商店形象（不再出现巨大 A 大商人），右侧正常显示缩放后的 A 大商人；
   - 打开商店货架确认手部骨架与购买正常。
2. 若遇到假商人事件（`NFakeMerchant`），确认右侧假商人按钮（走 `NMerchantButton` 的 `MerchantButtonPatch`）仍可正常显示 A 大商人假商人骨架。
3. 商店内热切换 A 大商人 ↔ 其他商人皮肤（如 `Merchant2CuteII`），确认切走再切回 A 大商人后左侧角色依然保持希尔德无污染。


---

## 三、#195/#196/#197/#198 AncientWaifus 0.3.7 遗物图标、时间线插图与动态立绘触摸反馈修复

### 1. 版本对照（0.3.6 vs 0.3.7）
静态对比 `3748235030.sts2test_disabled`（0.3.6）与 `3748235030`（0.3.7）的 DLL 与 PCK：
- DLL 差异仅位于 `AncientWaifus.Core.ConfigUIPatcher`（设置弹窗布局由 `CreateWaifuRow` 改为 `AddCard`），`WaifuAssetScanner`、`AtlasManager_GetSprite_Patch`、`AssetCache_*` 与 `GlobalTouchHook` 无实质变更。
- PCK 差异仅减少 1 个冗余原图 `res://waifu_assets/nonupeipe/packed/card_portraits/event/apotheosis.png`（219 → 218 文件）。
- 结论：本次三项问题均非 0.3.7 升级引入，而是此前未覆盖 `AncientWaifus` 真实的私有目录/独立贴图/元数据交互形态（`RelicOwnershipTests` 此前仅测了规范共享图集夹具）。

### 2. 三项根因定位（均已用源码、PCK/IL 反编译与实机日志确认）

#### （1）遗物图标与遗物百科大图均回落原版
1. **直接报错（实机 `godot.log` 实证）**：
   `[WARN] [SkinChanger] 隔离加载遗物图标失败，已保留当前图标 res://images/atlases/relic_atlas.sprites/touch_of_orobas.tres：遗物皮肤缺少其切片引用的图集。`（`biiig_hug.tres` 同理）。
   - `AncientWaifus.pck` 并未打包共享 `res://images/atlases/relic_atlas.png` / `relic_outline_atlas.png`，而是将 18 个切片（9 遗物 × 正常/描边）导出为二进制 `.res`（`AtlasTexture`，默认整图区域），每个切片分别指向独立的 256×256 贴图 `res://waifu_assets/<ancient>/images/relics/<relic>[_outline].png`。
   - `SkinService.LoadSelectedRelicBundle`（`Core/SkinService.cs`）只按 `SkinCatalog.IsRelicAtlasTexturePath` 查找 `relic[_outline]_atlas.png`，找不到即抛出 `遗物皮肤缺少其切片引用的图集`，被 `RelicSkinResources.Replace` 捕获后回落原版。
2. **遗物大图标（`RelicModel.BigIcon`）未打补丁**：
   - `NInspectRelicScreen.UpdateRelicDisplay`（遗物百科/点击查看详情）与 `NRelic.Reload(IconSize.Large)` 读取 `RelicModel.BigIcon`（`res://images/relics/<name>.png`），此前 `Ui/RelicSkinResources.cs` 仅补丁了 `RelicModel.Icon` 与 `RelicModel.IconOutline`。
3. **多先古包遗物归属未按先古隔离**：
   - `SkinCatalog.GetProviderRelicAssets`（`Catalog/SkinCatalog.cs`）将 `waifu_assets/<ancient>/...` 直接归一到规范路径并丢弃 `<ancient>` 段，导致只要任意一个先古选了 `AncientWaifus`，其它未选 `AncientWaifus` 的先古（如 `pael=PaelMod`、`tanx=AncientWaifus_Beta` 或原版 `neow`）的遗物都会被串台接管。
4. **启动日志 `0 套包含完整遗物图集` 与是否需重启**：
   - 启动阶段预热只遍历 7 个角色组，不包含先古组；遗物切片与大图标均在首次访问时懒加载，切皮时 `ClearRuntimeResourceCache` 自动失效缓存并恢复存量 `AtlasTexture`，**无需重启游戏**。

#### （2）时间线：缩略图原版、点开大图部分替换且未做隔离
- `AncientWaifus.pck` 的 `waifu_assets/timeline/` 包含 3 张 1620×1000 大图（`orobas_epoch.png`、`silent7_epoch.png`〔特兹卡塔拉“汤”〕、`placeholder/defect7_epoch.png`〔达弗“哦，亮闪闪！”〕）及 2 套整表图集（正式版 `epoch_atlas.png` 822×3236 与 Beta 版 `epoch_atlas_beta.png` 828×3788）。
- 游戏 `EpochModel.Portrait`（时间线面板 162×100 缩略图）经 `AtlasResourceLoader` → `AtlasManager.GetSprite("epoch_atlas", id)` 读取共享图集；`EpochModel.RealPortrait`（点开大图）经 `PreloadManager.Cache.GetTexture2D` 读取 `epoch_portraits/[placeholder/]<id>.png`。
- 此前 `BuildOverlay` 仅因 `orobas_epoch.png` 命中全局覆盖而挂载了大图（且会污染 `AssetCache`，切回原版不退），`silent7_epoch`/`defect7_epoch` 未接入，且 `epoch_atlas` 缩略图从未被替换。

#### （3）欧罗巴斯（orobas）与特兹卡塔拉（tezcatara）动态立绘无点击/触摸反馈
- `SkinCatalog.cs` L1066/L245 将 `HasInteractiveScenes` 限制在 `managedScriptCount > 0 && ContainsInteractiveScene(archive)`；而 `AncientWaifus` 的 `orobas.scn`/`tezcatara.scn` 不挂载 `.cs` 场景脚本（`场景脚本=0`），完全靠 `Touch_Box_*` 节点元数据（`ClickAnim`/`TargetBone`/`MaxDragRadius`/`Drag_Limit`/`DragAxisMode`/`IsSingleAnim`/`IsOneShot`/`IsAbsoluteDrag`/`Priority`/`Mix*`/`IntroDuration`）配合 DLL 中 `GlobalTouchHook` 驱动。
- 直接激活 `AncientWaifus.dll` 存在两重不可接受问题：① `ModStart.ModInit` 会调用 `WaifuAssetScanner.EnforceTakeOverPaths()` 全局劫持 `TakeOverPath`；② `GlobalTouchHook.IsOwnScene` 只认 `res://waifu_assets/` 或 `RedirectMap`，会直接跳过切换器隔离运行包路径 `res://sts2_skin_runtime/.../orobas.tscn`（记录 `다른 모드의 터치 박스라 건너뜀`）。

### 3. 修改内容
- **`STS2SkinChanger/Catalog/SkinCatalog.cs`**：
  - 保留 `res://waifu_assets/<owner>/...` 的先古归属（含 `tezcatara_waifu` → `tezcatara`），将 `_providerRelicAssets` 升级为 `ScopedRelicAsset`，同时索引遗物图集切片与独立大图标 `res://images/relics/<name>.png`；`FindSelectedRelicIconGroup`、`FindRelicIconOwnerGroup`、`GetProviderRelicSpritePaths(groupId, selected)`、`GetProviderRelicResourcePaths(groupId, selected)` 与 `TryResolveProviderAsset` 均按 `groupId` 隔离。
  - `BuildAliasedResourceOverlay` 与 `BuildIsolatedRelicResourceOverlay` 记录每个遗物切片到其真实贴图依赖的映射 `RelicSpriteTextures`，允许隔离遗物包包含 `IsRelicBigIconPath`。
  - 新增 `IsEpochPortraitPath` / `IsEpochAtlasPath`、`FindSelectedEpochPortrait`、`ResolveEpochPortraitOwnerGroupId`（`orobas_epoch` → `orobas`、`silent7_epoch` → `tezcatara`、`defect7_epoch` → `darv` 等，按各先古选择分别生效；若为角色包自带则优先归属对应角色）与 `BuildIsolatedEpochPortraitOverlay`；`BuildOverlay` 与 `ShouldMountProviderDependency` 不再向全局覆盖包挂载提供者的 `epoch_portraits` / `epoch_atlas*` / `images/relics/*.png`。
- **`STS2SkinChanger/Core/SkinService.cs`**：
  - `PreparedRuntimeOverlay` 透传 `RelicSpriteTextures`。
  - `LoadSelectedRelicBundle` 兼容两类切片：共享图集切片继续绑定 `normalAtlas`/`outlineAtlas`；独立 PNG 切片（`AncientWaifus` 形态）直接从私有别名加载 `Texture2D` 并包装为整图 `AtlasTexture`（同时把非描边贴图缓存在 `res://images/relics/<name>.png` 键下供 `BigIcon` 复用），避免无共享 `relic_atlas.png` 时抛错，也避免 Godot 反序列化二进制 `.res` 时找不到未挂载的 `res://waifu_assets/...`。
  - 新增 `GetRelicBigIconOverride` 与 `GetEpochPortraitOverride`（`thumbnail=true` 返回整图 `AtlasTexture`，与 `NEpochSlot` 162×100 及 `ImageHelper.GetAtlasUvRegion` 兼容；`thumbnail=false` 返回原图 `Texture2D`；按 `groupId` 缓存，切皮即时失效，**无需重启**）。
- **`STS2SkinChanger/Ui/RelicSkinResources.cs`**：
  - 新增 `RelicBigIconSkinPatch`（补丁 `RelicModel.BigIcon` getter，`Priority.Last`），由公开属性 `PackedIconPath` 派生 `res://images/relics/<name>.png`，同时避让框架遗物皮肤与已选角色独立贴图。
- **`STS2SkinChanger/Ui/TimelineEpochSkinResources.cs`（新增）**：
  - 补丁 `EpochModel.Portrait`（缩略图）与 `EpochModel.RealPortrait`（大图），按选中的先古走 `SkinService.GetEpochPortraitOverride`，未选中时保持原版。
- **`STS2SkinChanger/Ui/InteractiveTouchBoxBridge.cs`（新增）** + **`STS2SkinChanger/Entry.cs`**：
  - 在 `Entry.Initialize` 中安装 `Ui.InteractiveTouchBoxBridge.Install(tree)`。
  - 通过原生 `SceneTree.NodeAdded` + `SceneTree.ProcessFrame` 信号驱动隔离运行包（`res://sts2_skin_runtime/`）中带 `Touch_Box_*` 与交互元数据的 Spine 场景：支持点击/按住/单次/循环（`_Start`/`_Loop`/`_End`）、骨骼拖拽（4 种 `DragAxisMode`、`Drag_Limit` 矩形与 `MaxDragRadius` 半径、`IsAbsoluteDrag`）、重叠区域 `(Priority, ZIndex)` 命中与 `Intro` → `Idle` 入场动画。无需加载 `AncientWaifus.dll`，切回原版或其它皮肤时场景离树即自动清理，零残留。
- **`tests/STS2SkinChanger.RuntimeTests/AncientWaifusSupportTests.cs`（新增）** + **`tests/STS2SkinChanger.RuntimeTests/Program.cs`**：
  - 覆盖真实 `AncientWaifus` 形态的独立遗物切片 + `BigIcon` + 跨先古防串台、时间线三张 epoch 归属与全局覆盖包零泄漏、以及 `InteractiveTouchBoxBridge` 路径边界/轴向/裁剪/优先级策略，已挂入无参默认测试集与 `--test-relic-ownership`。

### 4. 编译与自动化验证结果（双分支均通过）
- Release 编译（Beta `data_sts2_windows_x86_64`，`-o %TEMP%\gemini-build-beta`）：**0 错误，0 警告**。
- Release 编译（正式版 `正式版_v0.111.1_20261117_2249_build26655341`，`-o %TEMP%\gemini-build-stable`）：**0 错误，0 警告**。
- `STS2SkinChanger.LogicTests`（Beta + 正式版，开启 Win32 `SetErrorMode(3)`）：均通过（`Skin Changer logic policy tests passed.`）。
- `STS2SkinChanger.RuntimeTests`（无参全量，Beta + 正式版，开启 Win32 `SetErrorMode(3)`）：均通过，含 `Relic ownership policy passed.`、`AncientWaifus support policy tests passed.`，以 `Workshop policy tests passed.` 结束。

### 5. 实机验证步骤（无需改排序，切换器排第一）
1. **遗物（百科 + 局内，无需重启）**：
   - 保持 `orobas`/`tezcatara`/`darv` 选 `AncientWaifus`，`pael` 选 `PaelMod`（或原版），`neow` 设为原版作为阴性对照。
   - 打开「遗物图鉴」查看 `触碰欧罗巴斯 (touch_of_orobas)`、`大大的拥抱 (biiig_hug)`、`符文金字塔 (runic_pyramid)`：小图标与点开的详情大图（`BigIcon`）均应为 `AncientWaifus` 样式；`帕埃尔的军团 (paels_legion)` 与 `涅奥` 的 `cursed_pearl`/`golden_pearl` 应保持原版（不串台）。
   - 局内控制台执行 `relic add TOUCH_OF_OROBAS` / `relic add BIIIG_HUG` / `relic add RUNIC_PYRAMID`，确认顶部遗物栏图标、描边与点击详情大图均为替换样式，且 `godot.log` 中不再出现 `遗物皮肤缺少其切片引用的图集`。
2. **时间线（缩略图 + 大图，无需重启）**：
   - 打开「时间线」，确认 `欧洛巴斯 (orobas_epoch)`、`静默猎手第 7 节点“汤”(silent7_epoch)`、`故障机器人第 7 节点“哦，亮闪闪！”(defect7_epoch)` 的**缩略图与点开的大图均为替换图**；其它节点缩略图排版正常无错位。
   - 将 `orobas` 切回原版后再进时间线（无需重启），确认 `orobas_epoch` 缩略图与大图立即恢复原版。
3. **动态立绘触摸反馈（图鉴预览 + 局内先古事件）**：
   - 在「其它图鉴」预览或局内遭遇 `欧罗巴斯 (orobas)` 与 `特兹卡塔拉 (tezcatara)`（均选 `AncientWaifus`），左键点击/按住拖拽头部、嘴部/法杖与视线区域，确认触发对应触摸动画与骨骼跟随拖拽；切回原版后无多余钩子或报错。


---

## 任务记录（#202 + #203）：InteractiveTouchBoxBridge 性能优化 + 实机遗物图鉴与触摸动画修复

### 1. 问题根因定位（均已以日志与静态 IL 实证确认）

1. **百科（遗物图鉴）仍显示原版，局内拿到手已换皮（#203-1）**：
   - 实机日志（`godot2026-11-19T20.15.25.log`、`20.19.55.log`）显示 `[CznStyleUI] CZN page host active for NCompendiumSubmenu`，当前测试环境启用了 `CznStyleUI`（`mods/_test_czn_3771060891/CznStyleUI.dll` v3.1.3）。
   - 静态 IL 分析 `CznStyleUI.GameModeSelection.RelicCollection.RelicCatalogProvider` 确认：`CznStyleUI` 用自建的 `CznRelicCollectionArchiveOverlay` 替换了原版 `NRelicCollection`，并通过 `ResolveCatalogIcon(RelicModel)` / `ResolveDetailIcon(RelicModel)` 调用 `ArchiveTextureResolver.LoadFirst`，优先对规范路径 `res://images/atlases/relic_atlas.sprites/<stem>.tres` 与 `res://images/relics/<stem>.png` 直接执行 `ResourceLoader.Load<Texture2D>`，仅在直接加载失败时才回退到 `model.Icon` / `model.BigIcon`。
   - 由于切换器为了隔离未选中皮肤包，在全局 VFS 上保持遗物规范路径为原版、并在 `RelicModel.Icon` / `IconOutline` / `BigIcon` 入口替换，因此 `CznStyleUI` 的直接加载绕过了三个 getter 补丁，直接读到原版贴图；而在局内 `NRelic` / `NInspectRelicScreen` 走的是游戏原生 `RelicModel.Icon` / `BigIcon`，故局内已正常生效。
2. **欧罗巴斯 / 特兹卡塔拉实机点击无触摸反馈，且存在全局 `NodeAdded` 与空帧骨骼查询开销（#203-2 + #202-1/2）**：
   - 实机日志确认 `ManagedAncientSceneAnimation.TryStart` 在图鉴预览与局内先古事件中均稳定触发（`已启动 orobas 的先古 Spine 动画：Intro -> Idle` / `已启动 tezcatara 的先古 Spine 动画：Intro -> Idle`）。
   - 但旧版 `InteractiveTouchBoxBridge` 通过全局 `SceneTree.NodeAdded` 监听并依赖 `IsManagedTouchScenePath(parent.SceneFilePath)`（要求前缀 `res://sts2_skin_runtime/`）；经由规范路径或 `.remap` 二进制 `.scn` 实例化的先古场景，`SceneFilePath` 为 `res://scenes/events/background_scenes/...` 或 `res://.godot/exported/...`，导致触摸区域零挂载。
   - 同时按 GPT `#201` 第 1、2 项建议：移除全局 `SceneTree.NodeAdded` 监听，并优化 `OnBeforeWorldTransformsChange` / `OnWorldTransformsChanged` 骨骼拖拽回调（未采用 `#201` 第 3、4 项）。

### 2. 代码改动

- **`STS2SkinChanger/Core/CznRelicCollectionCompat.cs`（新增）** + **`STS2SkinChanger/Entry.cs`**：
  - 参照 `PengoTarotFontCompat` 的无强依赖反射兼容模式（监听 `AppDomain.CurrentDomain.AssemblyLoad` 解决切换器排第一早于 `CznStyleUI` 加载的顺序问题），定位 `CznStyleUI.GameModeSelection.RelicCollection.RelicCatalogProvider.ResolveCatalogIcon(RelicModel)` 与 `ResolveDetailIcon(RelicModel)` 并挂入 `Priority.Last` Postfix；未安装 `CznStyleUI` 时零开销静默跳过。
  - 安装成功后输出单条日志 `[兼容] 已接入 CznStyleUI 遗物图鉴图标替换（2 个入口）。`。
- **`STS2SkinChanger/Ui/RelicSkinResources.cs`**：
  - `Replace` / `ReplaceBigIcon` 兼容可空 `ref Texture2D?`，新增 `ReplaceCatalogIcon` 与 `ReplaceDetailIcon` 供 `CznRelicCollectionCompat` 调用（同时兼顾 `FrameworkSkinRuntime.ResolveRelicVisual` 与 `SkinService` 隔离遗物缓存）。
- **`STS2SkinChanger/Ui/InteractiveTouchBoxBridge.cs`** + **`STS2SkinChanger/Core/Features/Ancient/AncientCompendium.cs`** + **`STS2SkinChanger/Entry.cs`**：
  - **#202-1 + #203-2**：彻底移除 `Entry.Initialize` 中的全局 `InteractiveTouchBoxBridge.Install(tree)` 与 `SceneTree.NodeAdded` 订阅；改为由 `ManagedAncientSceneAnimation.TryStart(groupId, sceneRoot)` 在确认当前先古为受管非原版皮肤并找到 `spineNode` 后，直接调用 `InteractiveTouchBoxBridge.AttachToScene(groupId, sceneRoot, spineNode)`（同时覆盖局内 `NAncientEventLayout.InitializeVisuals`、局内热切背景及先古图鉴预览）。
  - **零闲置开销**：仅当目标场景存在带交互元数据的 `Touch_Box_*` 节点时才注册并懒订阅 `SceneTree.ProcessFrame`；场景 `TreeExited` 或全部条目失效时立即清理状态并退订 `ProcessFrame`。`StartDefaultAnimation` 实际播放 `Intro` 时通过 `NotifyIntroStarted(sceneRoot)` 通知按 `IntroDuration` 阻断入场期误触，不重复播放 `Intro`。挂载成功时记录 `已为 {groupId} 挂载 {count} 个先古交互触摸区域。`。
  - **#202-2**：每个 `TouchBoxEntry` 注册时直接缓存所属 `SpriteTouchState`（含 `SpineSprite`、`HasDummy`、`BlockedUntilMsec`、`Entries` 与 `AppliedBones`），消除每次事件/回调的 `GetParentOrNull`/`GetNodeOrNull("SpineSprite")`；`OnBeforeWorldTransformsChange` 在无按住且 `DragOffset` 已归零时直接快速跳过，不调用 `GetSkeleton()`（松手回弹衰减到阈值 `BoneIdleEpsilonSq` 以下时立即归零），有拖拽时记录 `(BoundBone, OffsetX, OffsetY)` 到 `AppliedBones`；`OnWorldTransformsChanged` 直接按已记录的骨骼引用倒序恢复偏移，不再二次调用 `GetSkeleton()` / `FindBone()`。
- **`tests/STS2SkinChanger.RuntimeTests/AncientWaifusSupportTests.cs`**：
  - 补充断言：校验 `InteractiveTouchBoxBridge` 已无全局 `Install` 入口、`NeedsBoneUpdate` 空闲快速跳过、`ComputeNextDragOffset` 拖拽插值与松手归零收敛、`ResolveIntroDurationSeconds` 解析，以及 `CznRelicCollectionCompat.FindTargetMethods` 反射签名定位与缺省安全返回。

### 3. 编译与自动化验证结果（双分支全绿）

- Release 编译（Beta `data_sts2_windows_x86_64`，`-o %TEMP%\gemini-build-beta`）：**0 错误，0 警告**。
- Release 编译（正式版 `正式版_v0.111.1_20261117_2249_build26655341`，`-o %TEMP%\gemini-build-stable`）：**0 错误，0 警告**。
- `STS2SkinChanger.LogicTests`（Beta + 正式版，开启 Win32 `SetErrorMode(3)`）：均通过（`Skin Changer logic policy tests passed.`）。
- `STS2SkinChanger.RuntimeTests`（无参全量，Beta + 正式版，开启 Win32 `SetErrorMode(3)`）：均通过，以 `Workshop policy tests passed.` 结束。

### 4. 实机验证步骤

1. **百科（遗物图鉴）+ 局内遗物（含 `CznStyleUI` 启用环境，无需重启）**：
   - 启动后确认 `godot.log` 出现 `[兼容] 已接入 CznStyleUI 遗物图鉴图标替换（2 个入口）。`。
   - 打开「百科 → 遗物图鉴」，确认选了 `AncientWaifus` 的先古对应遗物（如 `触碰欧罗巴斯`、`大大的拥抱`、`符文金字塔`、`帕埃尔的军团` 等）在列表格子与点击后的详情大图中均显示替换图；将某个先古切回原版后重新打开遗物图鉴，确认其遗物同步恢复原版、其他先古不受影响。
2. **欧罗巴斯 / 特兹卡塔拉触摸动画与拖拽（先古图鉴预览 + 局内先古事件）**：
   - 在「其它图鉴」预览或局内遭遇欧罗巴斯 / 特兹卡塔拉，确认 `godot.log` 出现 `已为 orobas 挂载 3 个先古交互触摸区域。` / `已为 tezcatara 挂载 3 个先古交互触摸区域。`。
   - 等待约 2 秒入场动画结束后，左键点击/按住拖拽头部、嘴部/法杖、视线区域，确认触发对应触摸动作与骨骼跟随，松手平滑回弹；离开场景或切回原版后无报错、无残留。
