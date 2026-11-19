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


---

## 任务记录（#205）：恐惧 / 极速模式选人背景战损点击切换通用桥接（骨妹 / 战士 / 储君及同框架皮肤）

### 1. 静态反编译定位（只读 PE/IL 分析，未加载第三方 DLL）

对维护者对照库 `mods/_maintainer_set` 下三个目标包及同生态战损包做静态 IL 分析，确认它们在标准选人界面（`NCharacterSelectScreen`）的挂接与点击切档机制如下：

1. **三包共用框架（`necrobinderHpWear.dll`、`ironcladHpWear.dll`、`regentHpWear.dll`；同框架亦见 `silentKaguyaHpWear.dll`、`defectHpWear.dll`）**：
   - **标准界面入口**：均通过 `HarmonyPatch(typeof(NCharacterSelectScreen), "SelectCharacter")`（`NecrobinderCharSelectToggle` / `IroncladCharSelectToggle` / `RegentCharSelectToggle`；`regentHpWear` 另补丁 `NRegentCharacterSelectBg._Ready`）调用 **`BattleWear.CharSelectCore.Attach(Control bg, CsProfile p)`**（**双参数**，而非单参数 `Attach(Control)`）。
   - **角色 Profile 存放形态**：
     - `necrobinderHpWear`：静态字段 `CharSelectCore.Necrobinder`（`CsProfile.Name == "necrobinder"`），带 `CharSelectCore.NotifySelectedCharacter(string)`；
     - `ironcladHpWear`：静态字段 `CharSelectCore.Ironclad`（同类型同时存在 `Regent` / `Defect` / `Ironclad` 三个字段，必须按当前角色精确匹配）；
     - `regentHpWear`：静态方法 `CharSelectCore.ProfileFor(string)` + 静态字段 `CharSelectCore.Regent`（亦含 `Defect` / `Ironclad`），带 `CharSelectCore.NotifySelectedCharacter(string)`。
   - **点击热区与轮询门槛**：
     - `InstallClickCatcher` 在 `<character>_bg` 下创建全屏 `Control` 节点 **`DBWCharSelectClickCatcher`**（连接 `GuiInput` 并 `MoveChild` 到最顶层），同时挂载 `Timer` 节点 `DBWInputPoller` 通过 `bg.GetViewport().GuiGetHoveredControl()` 与 `DBWCharSelectClickCatcher` 比对触发 `HandleClick` / `HandleWearClick`；
     - `necrobinderHpWear` 与 `regentHpWear` 的 `IsOwnedProfileActive(p)` 会校验静态字段 `_selectedCharacterId` 是否等于当前角色名；若在恐惧/极速界面从未调用 `NotifySelectedCharacter(groupId)`，切过角色后点击会被静默丢弃；
     - 若此前在骨架尚未替换时误触发过 `Attach` 导致 `CsProfile.Dead = true`，后续 `Attach` 会直接 `return`，需按包内 `TryAttachProfile` 相同约定在重挂前复位 `Dead = false`。
   - **其他战损形态对照**：
     - 魅魔 `succubusIroncladHpWear`：`BattleWear.SuccubusCharacterSelect.Attach(Control)` + 热区 `SuccubusSkinPreviewClick`；
     - 贞德 `JoanofArcIroncladHpwear`：`JoanofArcIroncladHpwear.CharSelectCycle.Attach(Control)` + 热区 `JoanofArcIroncladHpwearClickCatcher`；
     - 观者 `WatcherHpWear`：`WatcherHpWear.SelectionCycle.AttachBackground(Control)` + 热区 `WatcherHpWearClick`；
     - 弩手 `CrossbowFool`：由 `CrossbowFoolSelectCompat` 挂载热区 `CrossbowFoolClick`。

2. **原实现在恐惧 / 极速模式失效的四处断口**：
   - `ProviderModeSelectOverlayBridge.AttachRebuiltBackground` 写死了 `ironclad` + `succubusIroncladHpWear`，重建背景（`ReplaceCharacterBackground`）后其他所有角色/提供者均未重挂；
   - `ProviderModeSelectOverlayBridge.Attach` 只查找单参数 `Attach(Control)`，完全漏掉了双参数 `BattleWear.CharSelectCore.Attach(Control, CsProfile)` 与 `AttachBackground(Control)`，且仅查询 `GetSelectedCreatureRuntimeProvider`（未回退 `GetSelectedRuntimeProvider`）；
   - `CharSelectPackRebindCompat.AfterRebuild` 仅匹配参数为 `NCharacterSelectScreen` 的 `TryReattachFromScreen`，恐惧（`NDreadScreen`）/ 极速（`NBlitzScreen`）直接跳过；
   - `ContextualSkinControls.RebuildCharacterDisplay` 在模式界面热切皮肤后只调用了 `SyncModeBackgroundClickPassthrough`，对未重建场景的纯叠加层提供者未补调 `ProviderModeSelectOverlayBridge.Attach`。

### 2. 代码改动

- **`STS2SkinChanger/Ui/ProviderModeSelectOverlayBridge.cs`**：
  - 移除 `AttachRebuiltBackground` 对 `ironclad` 与魅魔的硬编码；将 `AttachRebuiltBackground` 与 `Attach` 统一走通用挂接内核 `AttachCore`，并均在入口强校验 `IsModeScreenTypeName(screen.GetType().FullName)`（严格限定 `NBlitzScreen` / `NDreadScreen`，**标准模式 `NCharacterSelectScreen` 与未选中任何运行时皮肤时完全不干预**）。
  - 提供者解析统一为 `SkinService.GetSelectedCreatureRuntimeProvider(groupId) ?? SkinService.GetSelectedRuntimeProvider(groupId)`，并合并 `ManagedSkinModLoader.GetActiveProviderAssembly(providerId)` 与 `AppDomain` 匹配程序集。
  - 新增通用反射发现与调用机制 `DiscoverBackgroundAttachBindings` / `TryResolveProfile` / `ResetProfileDeadFlag` / `InvokeBinding`（零硬编码包名与角色名）：
    - 自动识别单参数 `Attach(Control)` / `AttachBackground(Control)`（覆盖魅魔、贞德、观者等）；
    - 自动识别双参数 `Attach(Control, TProfile)`（覆盖骨妹/战士/储君/狐猎/鸡煲等 `BattleWear.CharSelectCore` 全族）：优先调用同类型 `ProfileFor(string)`，否则按当前 `characterId` 精确匹配静态字段名或实例 `Name` 字段/属性，绝不跨角色串挂；调用前自动同步同类型 `NotifySelectedCharacter(string)` 并复位 `profile.Dead = false`。
  - 在已挂接的新背景节点上写入 `sc_mode_bg_attached_provider` 元数据，同一背景实例且热区存活时避免 `AttachRebuiltBackground` + `Attach` 连续重复重挂（同时仍同步 `NotifySelectedCharacter` 与点击放通）。
  - 扩展 `IsBackgroundClickCatcherName` 识别 `DBWCharSelectClickCatcher`（已由 `ClickCatcher` 命中）、`WearClick`（如 `WatcherHpWearClick`）与 `CrossbowFoolClick`，在恐惧模式 `SubViewport` 下统一放通 `GrayScaler` / `BigRotatingContainer` 视觉遮罩，切走或切回原版时自动恢复原 `MouseFilter`。
- **`STS2SkinChanger/Ui/ContextualSkinControls.cs`**：
  - 在 `RebuildCharacterDisplay` 三条返回路径的 `SyncModeBackgroundClickPassthrough` 前补调 `ProviderModeSelectOverlayBridge.Attach(screen, character)`（对标准模式零操作，确保模式界面内热切皮肤后背景交互立即可用）。
- **`tests/STS2SkinChanger.RuntimeTests/CharacterSelectModeBackgroundTests.cs`**：
  - 新增 `VerifyGenericProviderAttachDiscovery`：覆盖双参数 `Attach(Control, Profile)`（单字段 + `NotifySelectedCharacter`、多角色字段精确匹配、`ProfileFor(string)` + `Dead` 复位、无关角色阴性对照）、单参数 `Attach(Control)` / `AttachBackground(Control)`、热区名（含 `DBWCharSelectClickCatcher` / `WatcherHpWearClick` / `CrossbowFoolClick`）以及 `Attach`/`AttachRebuiltBackground` 对 `IsModeScreenTypeName` 的标准模式保护。

### 3. 编译与自动化验证结果（双分支全绿）

- Release 编译（Beta `data_sts2_windows_x86_64`，`-o %TEMP%\gemini-build-beta`）：**0 错误，0 警告**。
- Release 编译（正式版 `正式版_v0.111.1_20261117_2249_build26655341`，`-o %TEMP%\gemini-build-stable`）：**0 错误，0 警告**。
- `STS2SkinChanger.LogicTests`（Beta + 正式版，开启 Win32 `SetErrorMode(3)`）：均通过（`Skin Changer logic policy tests passed.`）。
- `STS2SkinChanger.RuntimeTests`（无参全量，Beta + 正式版，开启 Win32 `SetErrorMode(3)`）：均通过，含 `Character select mode background patch passed.`（Beta）、`AncientWaifus support policy tests passed.` 与 `Workshop policy tests passed.`。

### 4. 实机验证步骤（三个角色各测恐惧、极速，含对照项）

1. **准备工作**：保持测试包已启用（`necrobinderHpWear`、`ironcladHpWear`、`regentHpWear`，可同时保留魅魔/贞德对照）；分别给三个角色选上对应战损皮：
   - 骨妹（Necrobinder）→ `necrobinderHpWear`（a大骨妹可战损版）
   - 战士（Ironclad）→ `ironcladHpWear`（a大驼鹿战士可战损版）
   - 储君（Regent）→ `regentHpWear`（韩国大额头储君可战损版）
2. **极速模式（Blitz）逐角色实测**：
   - 进入「极速模式」选人界面，依次点选 **骨妹 → 战士 → 储君**，并在每个角色界面**左键连续点击人物背景空白处**：
     - 确认日志出现 `[选角背景] 已为 <providerId> 在模式界面挂接选人背景交互（CharSelectCore.Attach，背景=<角色>_bg，...）`；
     - 确认三个角色每次点击背景都能按档位循环切换战损立绘（与标准模式一致），点击按钮/信息面板区域不误触。
   - 在极速界面内将战士热切为 **魅魔** / **贞德**，再切回 **a大驼鹿战士可战损版**，确认每次热切后点背景都能立即切换；切回 **原版** 后点背景无反应且无报错。
3. **恐惧模式（Dread）逐角色实测**：
   - 进入「恐惧模式」选人界面，依次点选 **骨妹 → 战士 → 储君**，并在每个角色界面**左键连续点击人物背景**：
     - 确认日志同时出现挂接日志与 `[选角背景] 已在模式界面放通 <角色> 背景点击（穿透 GrayScaler、BigRotatingContainer...）`；
     - 确认三个角色在恐惧模式（含 `SubViewport` 灰阶/旋转视觉层下）点击背景均能正常切战损档位；
     - 在恐惧界面内将任意角色切回 **原版**（或不带点选热区的普通皮肤），确认日志出现 `已在模式界面恢复 <角色> 遮罩点击拦截`，且不残留其他角色的点击响应。
4. **标准模式回归**：
   - 返回「标准模式」选人界面，依次确认骨妹 / 战士 / 储君 / 魅魔 / 贞德点背景切战损与原先完全一致、无重复挂接日志或异常。


---

## 任务记录（#207）：猎宝「狐猎Kaguya血量换装」（`silentKaguyaHpWear`）点击切战损排查与修复

### 1. 三项待查问题结论（均以日志、`skin_changer.json` 与静态 PE/IL 实证确认）

1. **① `silentKaguyaHpWear` 与 `KaguyaSilentMSGKSkin` 的关系**：
   - 两者是**完全独立的两个 Mod**（独立 manifest、独立 DLL、独立 PCK、命名空间分别为 `BattleWear.*` 与 `SilentSkinMod.*`，无任何互相依赖或跨包调用）。
   - `KaguyaSilentMSGKSkin`（森森自用魔改）不含任何 Harmony 补丁，也不含战损切换，仅包含头部点击脚本 `KaguyaSilentHeadPressInteractor`。
   - `silentKaguyaHpWear`（狐猎Kaguya血量换装，68 MiB 完整独立 PCK）基于前者美术场景制作了 7 档血量换装，并在 `BattleWear.CharSelectCore` + `CharSelectHeadInteract` 里自己接管换装与眨眼/骷髅头切换；其 `.scn` 残留了 `KaguyaSilentHeadPressInteractor.cs` 引用，因此日志会出现良性的 `Cannot instantiate C# script`，不影响自身逻辑。
   - **并非皮肤合并/组合**：`skin_changer.json` 中 `"CharacterSkinCompositions": []`、`"silent": "silentKaguyaHpWear"`；截图里第二个下拉框显示 `KaguyaSilentMSG…` 仅是旁边未启用的「皮肤合并」（`STS2CharacterSkinMergeControl`）默认展示的候选列表首项。

2. **② 为何 22:09 `godot.log` L2503 出现 `已停用未选中皮肤提供者 silentKaguyaHpWear 的 29 个行为补丁`**：
   - 该行发生在选人界面初始聚焦于 **`FOOL`（`CrossbowFool`）** 期间：切换器正常的选角预览收窄（`已按选角预览将皮肤代码行为收窄到 1 个可见外观组`）会暂时停用所有非当前预览角色（`defectHpWear`、`necrobinderHpWear`、`silentKaguyaHpWear`、`ironcladHpWear`、`regentHpWear`、`WatcherHpWear`）的行为补丁。
   - 切到猎宝 `SILENT` 时（实证见 `godot2026-11-19T22.07.10.log` L2555–2560、L3194–3197、L3368–3371），`silentKaguyaHpWear` 均被正确识别并重新激活（`已按当前选择启用 狐猎Kaguya血量换装 的完整视觉会话 ... 保留 29 个原作者动画/场景行为补丁`），**并未被误判为未选中**。

3. **③ 真正的根因（标准模式原本就坏；极速/恐惧已由 `#206` 挂上，但需注意该包窄热区设计）**：
   - **确定的 Bug（标准模式 `NCharacterSelectScreen` 完全无法挂接）**：
     - 在 Beta v0.113.x 上，`silentKaguyaHpWear` 自身的 `SilentCharSelectToggle` 因期望已删除的 `CharacterModel` 参数而**安装失败**（`[ERROR] [silentKaguyaHpWear] patch failed for SilentCharSelectToggle: Patching exception in method NCharacterSelectScreen::SelectCharacter(NCharacterSelectButton charSelectButton)`）。
     - 切换器在 `STS2SkinChanger/ProtectedCharacters/CharSelectPackRebindCompat.cs` 第 45 行原本有专门针对狐猎的重挂兜底，但写成了**小写 `GetNodeOrNull<Control>("silent_bg")`**；而游戏与 `ContextualSkinControls.ReplaceCharacterBackground` 创建的节点名是**大写 `"SILENT_bg"`**（`character.Id.Entry + "_bg"`，日志 `22.07.10.log` L2623 / L2731 实证为 `SILENT_bg[Control] 可见=True 在树可见=True 子=6`，L2738 实证为 `hover=SILENT_bg`）。Godot 的 `GetNodeOrNull` 区分大小写，导致该兜底永远返回 `null`，标准模式从未挂上 `DBWCharSelectClickCatcher` 与 `DBWInputPoller`。
   - **极速（Blitz）/ 恐惧（Dread）模式实证与热区说明**：
     - `22.07.10.log` 实证 `#206` 的通用桥接在极速（L3198–3238）与恐惧（L3357–3405）中**均已成功挂接** `silentKaguyaHpWear`（`[silentKaguyaHpWear] char-select toggle attached (silent, stage 1/7)`，`SILENT_bg` 子节点由 6 增至 10，恐惧模式遮罩亦已放通）。
     - **但静态 IL 显示该包自身的换装点击热区非常窄**：与 `ironcladHpWear`（全屏）、`necrobinderHpWear`（`0.22–0.78 × 0.05–0.68`）、`regentHpWear`（`0.42–0.88 × 0.30–0.80`）不同，`silentKaguyaHpWear.CharSelectCore.InstallClickCatcher` 把 `DBWCharSelectClickCatcher` 锚定在 **`x: 38%–62%, y: 12%–52%`**（仅中央偏上躯干区域），且 `Press_Head`（点/长按眨眼闭眼，L3699 有命中日志 `char-select head: click blink.`）与 `DBWSkullHit`（点面具切骷髅）盖在头部上方。点人物两侧背景或下半身不会命中 `DBWCharSelectClickCatcher`，点头部只触发眨眼/骷髅，**只有点人物胸口/躯干中部（头部下方、`x: 38%–62%, y: 12%–52%` 内）才会切战损档位（`1/7 → 7/7`）**。

### 2. 代码改动（未触碰 `Core/Features/Merchant/*`）

- **`STS2SkinChanger/ProtectedCharacters/CharSelectPackRebindCompat.cs`**：
  - 新增大小写无关背景查找 `FindCharacterBackground(Control screen, string? groupId)` 与 `IsCharacterBackgroundNodeName`（先查大写 `SILENT_bg`、再查小写 `silent_bg`、最后大小写无关遍历子节点），修复标准模式下 `GetNodeOrNull("silent_bg")` 恒返 `null` 导致狐猎无法重挂的缺陷。
  - 若当前背景已挂有 `DBWCharSelectClickCatcher`（例如极速/恐惧已由 `ProviderModeSelectOverlayBridge` 挂接）则直接跳过，避免重复挂载；并在 `ReattachFoxBackground` 调用前通过 `ProviderModeSelectOverlayBridge.ResetProfileDeadFlag` 复位 `Silent.Dead`、重挂成功时输出单行诊断日志 `[选角重挂] 已为狐猎（silent）重挂选人背景交互（背景=SILENT_bg）。`。
- **`STS2SkinChanger/Ui/ProviderModeSelectOverlayBridge.cs`**：
  - `Attach` 与 `HasActiveBackgroundClickCatcher` 同步改用大小写无关的 `FindCharacterBackground` 兜底，避免任何混合大小写 `<character>_bg` 节点漏匹配。
- **`tests/STS2SkinChanger.RuntimeTests/CharacterSelectModeBackgroundTests.cs`**：
  - 新增 `#207` 回归断言：验证 `CharSelectPackRebindCompat.IsCharacterBackgroundNodeName` 同时匹配 `SILENT_bg` / `silent_bg` 且排除无关节点，并验证 `AfterRebuild` 调用 `FindCharacterBackground`。

### 3. 编译与自动化验证结果

- Release 编译（Beta `data_sts2_windows_x86_64`，`-o %TEMP%\gemini-build-beta`）：**0 错误，0 警告**。
- Release 编译（正式版 `正式版_v0.111.1_20261117_2249_build26655341`，`-o %TEMP%\gemini-build-stable`）：**0 错误，0 警告**。
- `STS2SkinChanger.LogicTests`（Beta + 正式版，开启 `SetErrorMode(3)`）：均通过（`Skin Changer logic policy tests passed.`）。
- `STS2SkinChanger.RuntimeTests`（无参全量，Beta + 正式版，开启 `SetErrorMode(3)`）：均通过（含 `Character select mode background patch passed.`、`AncientWaifus support policy tests passed.`、`Workshop policy tests passed.`）。
- 保护区离线回归（Beta，未加载任何第三方 DLL）：`--test-protected-boundaries`、`--test-runtime-safety`、`--test-preview-binding` 全部通过（注：含实包 `Assembly.LoadFrom` 的 `--test-crusader-hooks` / `--test-protected-characters` 按安全红线留交 Claude 侧执行）。

### 4. 实机验证步骤

1. **标准模式（`NCharacterSelectScreen`）**：
   - 给猎宝选中「狐猎Kaguya血量换装」（`silentKaguyaHpWear`），确认 `godot.log` 出现 `[选角重挂] 已为狐猎（silent）重挂选人背景交互（背景=SILENT_bg）。` 与 `[silentKaguyaHpWear] char-select toggle attached (silent, stage 1/7, ...)`，且 `SILENT_bg` 子节点为 10。
   - **左键点击人物中央偏上躯干区域（头部下方，`x: 38%–62%, y: 12%–52%` 内）**，确认日志输出 `char-select (silent): stage → ... (Kaguya path).` 且战损档位 1/7→7/7 正常循环；点头部确认仍能眨眼/长按闭眼/切骷髅。
2. **极速模式（Blitz）与恐惧模式（Dread）**：
   - 分别进入极速、恐惧并选猎宝（`silentKaguyaHpWear`），确认日志出现挂接日志（无重复双挂），点击人物胸口/躯干区域确认 7 档战损正常切换；在恐惧模式下切回原版猎宝确认恢复遮罩拦截且无残留。


---

## 任务记录（#211）：极速 / 恐惧选人背景双挂与跨界面 `DBWInputPoller` 竞态修复

### 1. 根因实证（22:31 `godot.log` + `silentKaguyaHpWear.dll` 静态 IL）

1. **为什么会出现「要点好几下才切，有时卡住点不了」（核心根因：跨界面残留 `DBWInputPoller` 抢占静态单例 `CsProfile.WasPressed`）**：
   - 游戏 `NSubmenuStack` 退出选人界面（标准 `NCharacterSelectScreen` / 极速 `NBlitzScreen` / 恐惧 `NDreadScreen`）时**不会释放界面节点**，仅将其 `Visible = false` 留在 `SceneTree` 中。
   - 静态 IL 确认 `silentKaguyaHpWear`（及同框架 `ironcladHpWear`、`defectHpWear`）的 `CharSelectCore.<InstallClickPoller>b__1` **未检查 `bg.IsVisibleInTree()`**，直接读取全局 `Input.IsMouseButtonPressed(Left)` 并改写**静态单例字段 `CsProfile.Silent.WasPressed`**（`justPressed = pressed && !p.WasPressed; p.WasPressed = pressed;`）。
   - 当玩家依次访问标准 → 极速 → 恐惧后，最多有 **3 个 `SILENT_bg` 下的 `DBWInputPoller` 同时在树中 60Hz 轮询**。先触发的隐藏界面轮询器抢先把 `CsProfile.Silent.WasPressed` 置为 `true`（随后因自身 `DBWCharSelectClickCatcher` 不可见而丢弃点击），紧接着当前可见界面的轮询器读到 `WasPressed == true` 判定非上升沿、把真实点击静默吞掉；只有在鼠标按下恰好落在两个定时器间隙时才偶发切档。
2. **为什么模式界面每次选人都挂两次（L3452→3482、L4030→4032、L4077→4115、L4188→4218）**：
   - `ContextualSkinControls.ShowCharacter` 在 `ScheduleCharacterRefresh`（推迟到帧末 `RebuildCharacterDisplay` 重建新 `<character>_bg`）之后，同帧立刻同步调用了 `ProviderModeSelectOverlayBridge.Attach(screen, character)`，先挂在了即将被销毁的初始背景实例上；
   - 紧接着 `RebuildCharacterDisplay` → `ReplaceCharacterBackground` 销毁旧背景、创建最终 `<character>_bg`，再次触发 `AttachRebuiltBackground` 挂第二次；骨妹 / 战士 / 储君 / 鸡煲 / 观者 / 狐猎在极速与恐惧里都存在这一同帧双挂现象，且旧背景在 `QueueFree` 生效前其 `DBWInputPoller` 未被同步停止。

### 2. 代码改动

- **`STS2SkinChanger/Ui/ProviderModeSelectOverlayBridge.cs`**：
  - **模式界面单次合并挂接（覆盖骨妹 / 战士 / 储君 / 狐猎 / 鸡煲 / 魅魔 / 贞德 / 观者）**：`Attach` 与 `AttachRebuiltBackground` 改为基于每界面代次计数器（`PendingAttachGenerationMeta`）的 `ScheduleDeferredAttach`（`CallDeferred` 合并），仅当 `ShouldRunDeferredAttach(scheduledGen, currentGen, screenValid, screenVisible)` 为真时在**最终存活的 `<character>_bg` 上执行一次 `AttachCore`**；已挂接且热区存活时跳过重复挂接，标准模式（`!IsModeScreenTypeName`）完全不干预。
  - **跨界面输入轮询器互斥暂停/恢复**：新增 `RegisterScreenLifecycle(Control screen)` 与 `SyncExclusiveScreenPollers(Control activeScreen)`，监听标准/极速/恐惧三个选人界面的 `VisibilityChanged`：任一界面隐藏时立即 `Stop()` 其 `AnimatedBg` 下所有提供者点击轮询器（`IsProviderInputPollerName`，匹配 `DBWInputPoller` / `*InputPoller*` / `*ClickPoller*` / `*HpwearPoller*`，排除 `*Safety*`），返回可见界面时再恢复；任意界面激活时同时确保其余已追踪隐藏界面的轮询器处于停止态。
  - **旧背景同步清理与单例输入状态复位**：新增 `StopBackgroundPollers(Node root, bool queueFree)` 在旧背景被替换前同步 `Stop()` + `QueueFree()` 轮询器；新增 `ResetProfileInputState(profile)` 在调用双参数 `Attach` 前同步复位 `Dead`、`WasPressed`、`IgnoreClickUntilRelease`，彻底消除卡死残留。
- **`STS2SkinChanger/Ui/ContextualSkinControls.cs`**：
  - `ShowCharacter` 入口调用 `ProviderModeSelectOverlayBridge.RegisterScreenLifecycle(screen)` 与 `SyncExclusiveScreenPollers(screen)`（统一纳管标准、极速、恐惧三套界面）。
  - `ReplaceCharacterBackground` 移除旧背景子节点前先同步调用 `ProviderModeSelectOverlayBridge.StopBackgroundPollers(child, queueFree: true)`。
- **`STS2SkinChanger/ProtectedCharacters/CharSelectPackRebindCompat.cs`**：
  - 狐猎标准模式兜底跳过模式界面（交由 `ProviderModeSelectOverlayBridge` 独占合并挂接，杜绝双挂），并在标准模式重挂前调用 `RegisterScreenLifecycle`、`SyncExclusiveScreenPollers` 与 `ResetProfileInputState`。
- **`tests/STS2SkinChanger.RuntimeTests/CharacterSelectModeBackgroundTests.cs`**：
  - 新增 `#211` 回归断言：`ShouldRunDeferredAttach` 代次合并与可见性门控、`IsProviderInputPollerName` 精准匹配与防误伤、`ResetProfileInputState` 同步复位 `Dead`/`WasPressed`/`IgnoreClickUntilRelease`，以及 IL 校验 `ReplaceCharacterBackground` 在 `RemoveChildSafely` 之前先调用 `StopBackgroundPollers`。

### 3. 编译与自动化验证结果（双分支全绿）

- Release 编译（Beta `data_sts2_windows_x86_64`，`-o %TEMP%\gemini-build-beta`）：**0 错误，0 警告**。
- Release 编译（正式版 `正式版_v0.111.1_20261117_2249_build26655341`，`-o %TEMP%\gemini-build-stable`）：**0 错误，0 警告**。
- `STS2SkinChanger.LogicTests`（Beta + 正式版，开启 `SetErrorMode(3)`）：均通过（`Skin Changer logic policy tests passed.`）。
- `STS2SkinChanger.RuntimeTests`（无参全量，Beta + 正式版，开启 `SetErrorMode(3)`）：均通过（含 `Character select mode background patch passed.`、`AncientWaifus support policy tests passed.`、`Workshop policy tests passed.`）。
- 保护区离线回归（Beta）：`--test-protected-boundaries`、`--test-runtime-safety`、`--test-preview-binding` 全部通过。

### 4. 实机验证步骤

1. **三模式连续切换防竞态实测（狐猎重点 + 骨妹 / 战士 / 储君 / 魅魔 / 贞德对照）**：
   - 依次进入 **标准模式 → 返回 → 极速模式 → 返回 → 恐惧模式 → 返回 → 极速/标准模式**，在每个模式下选中狐猎（`silentKaguyaHpWear`）并连续左键点击**胸口/躯干中部**：
     - 确认极速、恐惧每次选人日志中**只出现 1 次** `char-select toggle attached` 与 1 次 `[选角背景] 已为 silentKaguyaHpWear 在模式界面挂接选人背景交互`（不再出现两次不同实例号）；
     - 确认每次点击都稳定单次切换 1 档战损（`1→2→3/5→...`），不再需要连点多下或卡死，且切换到其他角色（如愚者）后不再触发狐猎隐藏界面的 `char-select head: click blink.`。
2. **其他战损皮肤单挂确认**：
   - 在极速与恐惧中依次点选骨妹（`necrobinderHpWear`）、战士（`ironcladHpWear`）、储君（`regentHpWear`）、魅魔、贞德，确认日志每次选人仅挂接 1 次、点击切档顺畅，标准模式行为不受影响。


---

## #213：奥契丝文本替换按骨妹当前皮肤门控（不越权覆盖其他皮肤）

### 1. 静态 IL 根因定位

对游戏目录下只读分析 `mods\_test_orchis_本体\OrchisNecrobinderSkinMod.dll` 与 `mods\_test_orchis_修复包\STS2-OrchisNecrobinderSkinFix.dll`（静态 `PEReader` 扫描，未加载第三方 DLL）：

1. **文本覆盖的实际来源（`OrchisNecrobinderSkinMod.dll`）**：
   - `OrchisNecrobinderSkinMod.Scripts.LocStringGetRawTextPatch`（`[HarmonyPatch(typeof(LocString), "GetRawText")]`）：
     - `Prefix` 调用 `TextReplacePatch.TryGetCharacterText(LocString, out string)`，直接覆写 `NECROBINDER.title`（`奥契丝`）、`NECROBINDER.description`（`因为有心，所以悲伤……`）等角色词条；
     - `Postfix` 调用 `TextReplacePatch.ShouldReplace(LocString)`，为真时执行 `ReplaceNames`（`亡灵契约师→奥契丝`、`奥斯提→洛伊德`、`缚魂命匣→破旧玩偶`、`无界命匣→精致玩偶`）。
   - `OrchisNecrobinderSkinMod.Scripts.LocStringGetFormattedTextPatch`（`[HarmonyPatch(typeof(LocString), "GetFormattedText")]`）：
     - `Postfix` 同样调用 `TextReplacePatch.ShouldReplace(LocString)` 并执行 `ReplaceNames`。
2. **修复包（`STS2-OrchisNecrobinderSkinFix.dll`）与现有兼容层缺口**：
   - 修复包本身不注入文本，其 `STS2OrchisNecrobinderSkinFix.Patches.OrchisFeatureGatePatch` 动态补丁上述同一对方法（`ShouldReplace` 挂 Postfix `TextShouldReplacePostfix`，`TryGetCharacterText` 挂 Prefix `CharacterTextPrefix`），仅按其 RitsuLib 开关（默认全开）放行。
   - SkinChanger 的 `OrchisSkinFixCompat` 原先仅门控了视觉/房间补丁（`InstallFixCallbacks`、`InstallProviderCallbacks`），漏掉了 `OrchisNecrobinderSkinMod.Scripts.TextReplacePatch`；且原 `InstallLoaded()` 在未安装修复包时会提前返回。结果只要 `OrchisNecrobinderSkinMod.dll` 原生加载，即使骨妹当前选的是 `necrobinderSkin_NSFW.Ver.2.0` 或原版，选人界面（以及 `ContextualSkinControls.RestoreCharacterInfoText` 实时刷新时调用的 `LocString.GetFormattedText()`）仍被强制替换成奥契丝名称、简介与初始遗物名。

### 2. 代码改动

- **`STS2SkinChanger/Core/OrchisSkinFixCompat.cs`**：
  - 新增 `InstallProviderTextCallbacks(Assembly)`（L85–91），使用 `Priority.First` Prefix 门控：
    - `OrchisNecrobinderSkinMod.Scripts.TextReplacePatch.ShouldReplace(LocString)` → `AllowTextReplace(ref bool __result)`（L133–138）
    - `OrchisNecrobinderSkinMod.Scripts.TextReplacePatch.TryGetCharacterText(LocString, out string)` → `AllowCharacterTextReplace(ref string __1, ref bool __result)`（L140–146）
  - 统一经 `AllowsText() => Allows(Body)`（L148）判定：仅当骨妹（`necrobinder`）当前实际选中 `OrchisNecrobinderSkinMod` 时放行（此时继续透传给 `STS2-OrchisNecrobinderSkinFix` 的 RitsuLib 开关和原方法）；骨妹选择原版、其他皮肤或仅奥斯提选奥契丝时直接返回 `false`（且将 `out text` 置为 `string.Empty`）。
  - 调整 `InstallLoaded()`（L55–68）：只要 `OrchisNecrobinderSkinMod` 程序集已加载即安装文本门控，无论 `STS2-OrchisNecrobinderSkinFix` 是否同时存在；原有 `InstallFixCallbacks` / `InstallProviderCallbacks` 条件保持不变。
  - 核对实时热切链路：`ContextualSkinControls.RestoreCharacterInfoText`（L2331–2375）在每次换肤重建选人显示时已重新求值 `CharacterSelectTitle`、`CharacterSelectDesc` 以及初始遗物的 `Title` / `Description`，因此门控生效后选人界面切换皮肤即可实时恢复或切换文本，无需额外侵入 UI 层。
- **`tests/STS2SkinChanger.RuntimeTests/OrchisRoomCallbackTests.cs`**：
  - 新增合成 `OrchisNecrobinderSkinMod.Scripts.TextReplacePatch` 与模拟修复包开关（`SimulateFixTextShouldReplacePostfix` / `SimulateFixCharacterTextPrefix`），通过真实 Harmony detour 验证：原版拦截、其他皮肤（`necrobinderSkin_NSFW.Ver.2.0`）拦截、仅奥斯提选奥契丝拦截、骨妹选奥契丝放行、奥契丝选中下仍尊重修复包自身开关关闭、以及热切回其他皮肤后立即恢复拦截。

### 3. 编译与自动化验证结果（双分支全绿）

- Release 编译（Beta `data_sts2_windows_x86_64`，`-o %TEMP%\gemini-build-beta`）：**0 错误，0 警告**。
- Release 编译（正式版 `正式版_v0.111.1_20261117_2249_build26655341`，`-o %TEMP%\gemini-build-stable`）：**0 错误，0 警告**。
- `STS2SkinChanger.LogicTests`（Beta + 正式版，开启 `SetErrorMode(3)`）：均通过（`Skin Changer logic policy tests passed.`）。
- `STS2SkinChanger.RuntimeTests`（无参全量，Beta + 正式版，开启 `SetErrorMode(3)`）：均通过（含 `Orchis room & text callbacks passed: real detours on synthetic merchant, rest-site, and text-replace methods.` 与 `Workshop policy tests passed.`）。
- 保护区离线回归（Beta）：`--test-protected-boundaries`、`--test-runtime-safety`、`--test-preview-binding` 全部通过（涉及加载第三方 DLL 的工坊专项留交 Claude 端执行）。

### 4. 实机验证步骤

1. 保持骨妹选中 `necrobinderSkin_NSFW.Ver.2.0`（或原版 `__base__`），进入选人界面点选骨妹：
   - 确认左上角色名与简介显示为原版（或所选皮肤自身文本，如「亡灵契约师」与原版简介，初始遗物为「缚魂命匣」），不再显示「奥契丝 / 因为有心，所以悲伤……」。
2. 在选人界面通过换肤器把骨妹切到 `OrchisNecrobinderSkinMod`：
   - 确认实时热切为「奥契丝」与奥契丝简介（「因为有心，所以悲伤……」），无需重启或退回主菜单。
3. 在同一界面把骨妹切回 `necrobinderSkin_NSFW.Ver.2.0` 或原版（也可额外测试「骨妹选其他皮肤 + 奥斯提选奥契丝」）：
   - 确认角色名、简介与初始遗物文本立即恢复为非奥契丝文本。

> 可选观察项（未在本次改动物料范围内）：静态扫描显示 `OrchisNecrobinderSkinMod.dll` 还包含 `RelicIconPatch.TryGetReplacementIcon` 与 `CharacterSelectIconPathPatch` / `CharacterSelectLockedIconPathPatch`；如后续实机发现非奥契丝皮肤下初始遗物图标或选人头像按钮也发生串联，可沿同一 `OrchisSkinFixCompat` 机制补挂门控。
