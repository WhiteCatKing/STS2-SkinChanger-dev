# STS2SkinChanger 路径与全局覆写风险普查报告（Gemini）

- **审查范围**：`D:\mod\工作2\上游修复_1.0.35\STS2-SkinChanger_修改件\STS2SkinChanger\`（只读普查，未改动任何源码与测试，未构建）
- **基线快照**：分支 `fix/1.0.36-stability`（HEAD `a1f2a99` + 工作区含 GPT `#235` / `#239` 与 Claude `NonoFoolSkin` 未提交改动；行号以 2026-11-20 22:48 工作区为准）
- **对照 PCK**：
  - **正式版（v0.111.1）**：`D:\mod\游戏版本备份\正式版_v0.111.1_20261117_2249_build26655341\SlayTheSpire2.pck`（15,891 项）
  - **测试版（v0.113.1）**：`C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.pck`（17,369 项）
- **判定口径**：原文件、`.import` 或 `.remap` 任一存在即计为「有」；同步覆盖 `ImageHelper.GetImagePath`、`SceneHelper.GetScenePath`、`CanonicalImagePath`、`CanonicalScenePath` 等拼接出来的规范路径。

---

## A. 写死的 `res://` 路径与拼接前缀普查

### A.1 核心发现（高优先级：确定的跨版本断口 / 缺口）

1. **确定的问题 — 正式版 v0.111.1 商人房间预热与解析指向不存在的测试版场景**：
   - `Core/Features/Merchant/MerchantRuntimeAppearance.cs:44, 48, 50-53, 561, 1861-1863`：`MerchantRoomScenePath` 硬编码为 `res://scenes/rooms/standard_merchant_room.tscn`，且 `ResolveMerchantRoomScenePath(NMerchantRoom?)` 未判断 `MerchantVariantScenes.HasBlitz`。正式版中该文件不存在（仅 `res://scenes/rooms/merchant_room.tscn` 存在）。结果：`MerchantRoomPrewarmPatch.Postfix`（`:1860-1863`）在正式版每次打开地图时都对不存在路径执行预热（失效）；`TryCreateCurrentMerchantRoom`（`:561`，当前无活调用方但仍为 `internal` 方法）在正式版同样会解析到不存在路径。同文件 `:1702` 的 `GetMerchantScenePath` 已做了 `HasBlitz` 判断，但这三处未走它。
2. **确定的问题 — `MerchantAppearancePathRegex` 漏掉了正式版 `merchant_room.tscn`**：
   - `Catalog/SkinCatalog.cs:9894`：正则写成 `res://scenes/rooms/(?:(?:blitz_)?merchant_button|blitz_merchant_room|standard_merchant_room)\.tscn$`，丢弃了 `merchant_room.tscn`。在正式版（或修改 `merchant_room.tscn` 的旧版商人包）中，该路径无法被归类为商人外观资源。另：测试版新增了 `res://scenes/rooms/blitz_merchant.tscn`，目前也未在正则中（待确认是否会被极速商人包替换）。
3. **可能有风险 — 其它图鉴普通商人兜底路径仍写死测试版已删除的 `merchant_room.tscn`**：
   - `Core/Features/Ancient/AncientCompendium.cs:1805`：`GetOtherEntries` 为普通商人条目的 `ScenePath` 写死 `"res://scenes/rooms/merchant_room.tscn"`。正常情况下 `InstantiateOtherPreviewScene`（`:2474`）会优先尝试 `"res://scenes/rooms/merchant_button.tscn"`（双端均存在），但如果该路径失败回退到 `entry.ScenePath`，测试版会因 `merchant_room.tscn` 已删除而直接失败（应改用 `MerchantRuntimeAppearance.GetMerchantScenePath()`）。
4. **已有兜底但存在边角风险 — 先古卡模板旧路径（`#239` 相关）**：
   - `Core/Features/Card/CardSkinControls.cs:45` 与 `:910` 仍硬编码正式版旧路径 `compressed.sprites/card_template/ancient_card_border.tres` 与 `ancient_card_text_bg_{type}.tres`（测试版已删除并迁至 `compressed_atlas.sprites/ancient_card_border.tres` 与 `ancient_text_bg_{type}.tres`）。`#239` 在 `SkinService.LoadCardPresentationResource`（`:3354`）经 `SkinCatalog.ResolveCardPresentationBaselinePath`（`:3040-3058`）做了运行时双端转译，当前实机安全；但：① 若 `Catalog == null` 或将来有调用绕过 `LoadCardPresentationResource` 会回退到旧路径出 NOPE；② `SkinService.cs:3348-3350` 在转译前先拿旧路径查选中提供者，如果未来有纯测试版卡包只覆写了 `compressed_atlas.sprites/ancient_text_bg_*.tres` 而没提供旧路径，提供者侧会漏命中并回落游戏原图。

---

### A.2 完整核对表（只在一边存在的与两边都没有的排在最前面）

#### 一、仅在一边 PCK 存在（版本迁移 / 分支差异，按风险由高到低）

| 路径 | 源码位置（文件:行） | 正式版有/无 | 测试版有/无 | 测试版里疑似的新位置（按文件名搜） | 影响（缺了会怎样） |
| --- | --- | --- | --- | --- | --- |
| `res://scenes/rooms/standard_merchant_room.tscn` | `Core/Features/Merchant/MerchantRuntimeAppearance.cs:44,48,53,561,1863` | **无** | 有 | （测试版拆出；正式版对应 `res://scenes/rooms/merchant_room.tscn`） | **正式版受影响**：`MerchantRoomPrewarmPatch`（`:1863`）在正式版每次打开地图都预热不存在场景；`ResolveMerchantRoomScenePath`（`:53`）未判 `HasBlitz`（`GetMerchantScenePath` `:1702` 已判） |
| `res://scenes/rooms/merchant_room.tscn` | `Core/Features/Ancient/AncientCompendium.cs:1805`<br>`Core/Features/Merchant/MerchantRuntimeAppearance.cs:1702` | 有 | **无** | `res://scenes/rooms/standard_merchant_room.tscn`<br>`res://scenes/rooms/blitz_merchant_room.tscn` | `MerchantRuntimeAppearance.cs:1702` 已受 `HasBlitz` 保护；但 `AncientCompendium.cs:1805` 在测试版作为 `merchant_button.tscn` 后的第二兜底，若前者加载失败会直接报错 |
| 正则 `res://scenes/rooms/(?:(?:blitz_)?merchant_button\|blitz_merchant_room\|standard_merchant_room)\.tscn$` | `Catalog/SkinCatalog.cs:9894` (`MerchantAppearancePathRegex`) | 部分无（`blitz_*`/`standard_*` 无；且漏匹配正式版 `merchant_room.tscn`） | 有（另新增 `blitz_merchant.tscn` 未含） | 测试版另有 `res://scenes/rooms/blitz_merchant.tscn` | **正式版受影响**：只改写 `res://scenes/rooms/merchant_room.tscn` 的商人皮肤包无法被 `MerchantAppearancePathRegex` 识别为商人外观；测试版 `blitz_merchant.tscn` 归属待确认 |
| `res://images/atlases/compressed.sprites/card_template/ancient_card_border.tres` | `Core/Features/Card/CardSkinControls.cs:45`<br>`Catalog/SkinCatalog.cs:3029,3034` | 有 | **无** | `res://images/atlases/compressed_atlas.sprites/ancient_card_border.tres` | 缺了会由游戏返回 `missing_power.png`（NOPE）。`#239` 已在 `SkinService.LoadCardPresentationResource` 经 `ResolveCardPresentationBaselinePath` 转译；但 `Catalog==null` 或纯测试版卡包只给新路径时会漏命中 |
| `res://images/atlases/compressed.sprites/card_template/ancient_card_text_bg_{type}.tres` (`attack`/`skill`/`power`) | `Core/Features/Card/CardSkinControls.cs:910`<br>`Catalog/SkinCatalog.cs:3029,3035-3037` | 有（3 项） | **无** | `res://images/atlases/compressed_atlas.sprites/ancient_text_bg_{attack,skill,power}.tres`（文件名也去掉了 `card_`） | 同上：直载旧路径会显示大号 NOPE 压在卡面说明区；`#239` 已对基础兜底转译，纯测试版卡包只给新路径时提供者查询仍会漏命中 |
| `res://images/atlases/compressed_atlas.sprites/`（含上述 4 项） | `Catalog/SkinCatalog.cs:3031,3034-3037` | **无** | 有（56 项） | — | 仅由 `ResolveCardPresentationBaselinePath` 配合 `_gameArchive.Contains` 按实包探测选用，正式版不会误载 |
| `res://scenes/rooms/blitz_merchant_room.tscn` | `Core/Features/Merchant/MerchantRuntimeAppearance.cs:45,1701`<br>`Core/Features/Ancient/AncientCompendium.cs:1814` | **无** | 有 | — | 均有 `MerchantVariantScenes.HasBlitz` 或 `NBlitzMerchantRoom` 运行时类型门控，正式版不触发 |
| `res://animations/backgrounds/blitz_merchant_room/top/blitz_merchant_top_skel_data.tres` | `Core/MerchantVariantScenes.cs:13` | **无** | 有 | — | 仅在 `blitz == true`（`HasBlitz` 成立且为极速商店）时检查，正式版不触发 |
| `res://images/ancients/vakuu_angry.png` | `Core/VakuuVanillaArt.cs:13` | **无** | 有（`.import`） | — | `SkinCatalog.Vakuu.cs:10` 先用 `ResolveBaseline` 校验存在性，不存在时不注册彩蛋选项；`VakuuVanillaArtPatch` 亦在正式版跳过 |
| `res://scenes/creature_visuals/byrdine_monster.tscn` | `Ui/OtherCreatureCatalog.cs:97` | **无** | 有 | — | 由 `RequiredModelType = "...ByrdineMonster"` 经 `OtherCreatureCatalog.Available` 门控，正式版隐藏条目 |
| `res://scenes/creature_visuals/wuppy.tscn` | `Ui/OtherCreatureCatalog.cs:102` | **无** | 有 | — | 由 `RequiredModelType = "...Wuppy"` 经 `OtherCreatureCatalog.Available` 门控，正式版隐藏条目 |
| `res://scenes/vfx/monsters/byrdine_overlay_vfx.tscn` | `Ui/ByrdineAttackPreview.cs:14` | **无** | 有 | — | 仅从异鸟成鸟预览触发，间接由 `ByrdineMonster` 存在性门控；正式版不可达 |

#### 二、两边 PCK 均不存在（区分「第三方 Mod / 扩展约定 / 切换器虚拟路径」与「需留意的非标准路径」）

| 路径 | 源码位置（文件:行） | 正式版有/无 | 测试版有/无 | 测试版里疑似的新位置（按文件名搜） | 影响（缺了会怎样） |
| --- | --- | --- | --- | --- | --- |
| `res://images/events/fake_merchant.png` / 前缀 `res://images/events/fake_merchant` | `Core/FakeMerchantEventArtPolicy.cs:24`<br>`Catalog/SkinCatalog.Events.cs:147` | 无 | 无 | 原版无此图（假商人用 `scenes/events/custom/fake_merchant.tscn` + `animations/backgrounds/fake_merchant_room/bottom/`） | **无游戏缺口**：用于识别第三方事件立绘包（如 NSFW 化原版事件）自绘的假商人事件图；不会当作原版必选图直接加载 |
| `res://images/ancients/{id}_{character\|character_sleeping\|character_mask\|background_cover}.*` | `Catalog/SkinCatalog.cs:9934` (`AncientLayerImageRegex`) | 无 | 无 | 原版 `images/ancients/` 仅含 `*_placeholder.png`、`under_construction.png`、`vakuu_angry.png` | **无游戏缺口**：仅用于识别先古皮肤包的分层静态图片，无直接加载 |
| `res://images/atlases/epoch_atlas_beta.{png,tpsheet}` | `Catalog/SkinCatalog.cs:9970` (`EpochAtlasRegex`) | 无 | 无 | 原版仅 `res://images/atlases/epoch_atlas.{png.import,tpsheet}` | **无游戏缺口**：用于识别并拦截 `AncientWaifus` 私有时间线图集（归一后路径），防止全局泄漏 |
| `res://animations/events/`、`res://audio/events/` | `Core/EventSkinPolicy.cs:14` | 无 | 无 | 原版无此目录（事件资源在 `images/events/`、`scenes/events/`、`scenes/vfx/events/`、`images/packed/vfx/event/`） | **无游戏缺口**：仅作为事件皮肤归属扫描的防御性前缀白名单 |
| `res://scenes/creature_visuals/templates/`、`res://scenes/merchant/characters/templates/`、`res://scenes/rest_site/characters/templates/`、`res://scenes/ui/character_icons/templates/` | `Catalog/SkinCatalog.cs:9077,9090,9103,10000-10013` | 无 | 无 | 原版模板仅在 `scenes/vfx/templates/` | **无游戏缺口**：用于识别第三方 Mod 的模板目录约定 |
| `res://custom/...`、`res://scenes/char_select/`、`res://scenes/character_select/`、`res://backgrounds/` | `Catalog/SkinCatalog.cs:9045,9116-9117,9994-10010,10695` | 无 | 无 | 原版选角在 `scenes/screens/char_select/`，背景在 `scenes/backgrounds/` | **无游戏缺口**：仅用于识别第三方皮肤包的常见非标目录约定 |
| `res://OrchisNecrobinderSkinMod/animations/rest_site/necrobinder/{rest_site_necrobinder,lloyd_relax}_skel_data.tres` | `Core/OrchisSkinFixCompat.cs:30-31` | 无 | 无 | 第三方奥契丝包私有路径（原版路径无前缀） | **无游戏缺口**：专用于奥契丝修复包火堆路径识别与门控 |
| `res://animations/monsters/kaiser_crab/kaiser_crab_hurt.atlas`、`kaiser_crab_skeleton_data_hurt.tres` | `Ui/EmperorCrabCompat/HalfHpCrabSkinPatch.cs:34-35` | 无 | 无 | 第三方帝皇蟹爆衣包（`erowatcher`）私有资源 | **无游戏缺口**：仅在命中 `erowatcher` 半血爆衣补丁时从皮肤包查询 |
| `res://osty_defect/display_select.tres` | `Ui/OstyModelPreviewControls.cs:26` | 无 | 无 | 第三方奥斯提皮肤包私有资源 | **无游戏缺口**：图鉴预检/预览兼容条目，未装该包不加载 |
| `res://generated/assets/card_art/MegaCrit.Sts2.Core.Models.Cards.EscapePlan_card_art.png` | `Catalog/CardLayoutVariantPolicy.cs:14` | 无 | 无 | 第三方卡面美化包路径特征 | **无游戏缺口**：仅用于策略探测，不直接加载 |
| `res://animations/customs/merchant/`、`res://animations/customs/merchant2cute_ii/blitz_merchant/`、`res://waifu_assets/...` | `Catalog/SkinCatalog.cs:9892-9893,9991-9994` | 无 | 无 | 第三方商人包 / `AncientWaifus` 私有路径 | **无游戏缺口**：仅作正则归属识别 |
| `res://sts2_skin_runtime/...`、`res://__skin_changer_base/...` | `Catalog/SkinCatalog.cs:6154,6370`、`Core/SkinService.cs:8815,8868`、`Core/SkinService.RuntimeSafety.cs:123` 等 | 无 | 无 | 切换器自建运行包虚拟别名根 | **无游戏缺口**：切换器私有虚拟路径命名空间 |
| `res://project.godot`、`res://export_presets.cfg`、`res://.godot/exported/`、`res:///` | `Catalog/SkinCatalog.cs:4434-4443,5826,9650`、`Catalog/SkinCatalog.SpinePreflight.cs:40` | 无 | 无 | 导出包中工程文件为 `res://project.binary`（双端均有） | **无游戏缺口**：用于过滤编辑器残留/规范化多余斜杠 |

#### 三、正式版与测试版均存在（确认完好，附清单供复核）

| 路径 / 模板 / 前缀 | 源码位置（文件:行） | 正式版有/无 | 测试版有/无 | 备注 / 潜在留意点 |
| --- | --- | --- | --- | --- |
| `res://images/atlases/ui_atlas.sprites/card/ancient_banner.tres` | `Core/Features/Card/CardSkinControls.cs:47` | 有 | 有 | 先古卡顶部横幅（`ui_atlas.sprites/card/` 双端均为 20 项，未搬迁） |
| `res://materials/cards/banners/card_banner_ancient_mat.tres` | `Core/Features/Card/CardSkinControls.cs:49` | 有 | 有 | 先古卡横幅材质 |
| `res://scenes/cards/card_canvas_group_mask_material.tres`<br>`res://scenes/cards/card_canvas_group_mask_blur_material.tres` | `Core/Features/Card/CardSkinControls.cs:640-641,684-685` | 有 | 有 | 卡图遮罩材质 |
| `res://scenes/screens/card_library/card_library_tickbox.tscn` | `Core/Features/Card/CardSkinControls.cs:115` | 有 | 有 | 卡牌图鉴复选框；若游戏重命名将导致图鉴过滤勾选框构建失败 |
| `res://scenes/screens/main_menu/compendium_bottom_button.tscn` | `Core/Features/Ancient/AncientCompendium.cs:98`<br>`Core/Features/Ancient/BestiaryEntranceSwitcher.cs:226` | 有 | 有 | `SceneHelper.GetScenePath` 拼接；图鉴入口按钮模板 |
| `res://scenes/ui/back_button.tscn` | `Ui/BackgroundAppearanceScreen.cs:22`<br>`Ui/CharacterAppearanceScreen.cs:37`<br>`Core/Features/Ancient/AncientCompendium.cs:868` | 有 | 有 | 返回按钮场景；缺了会抛 `InvalidOperationException` |
| `res://scenes/pause_menu/pause_menu_button.tscn` | `Ui/BackgroundAppearanceScreen.cs:286`<br>`Ui/CharacterAppearanceScreen.cs:2708` | 有 | 有 | 暂停菜单按钮场景；缺了会抛 `InvalidOperationException` |
| `res://themes/kreon_bold_glyph_space_one.tres` | `Ui/ContextualSkinControls.cs:60` | 有 | 有 | UI 字体资源 |
| `res://scenes/events/event_option_button.tscn` | `Ui/EventCompendiumPreview.cs:250` | 有 | 有 | 事件图鉴选项按钮预览 |
| `res://scenes/rooms/merchant_button.tscn` | `Core/Features/Ancient/AncientCompendium.cs:2474` | 有 | 有 | 商人独立按钮预览场景 |
| `res://scenes/merchant/merchant_inventory.tscn` | `Core/Features/Ancient/AncientCompendium.cs:1101`<br>`Core/Features/Merchant/MerchantRuntimeAppearance.cs:55` | 有 | 有 | 商店货架场景 |
| `res://scenes/events/custom/fake_merchant.tscn`<br>`res://scenes/events/custom/fake_merchant_button.tscn`<br>`res://scenes/events/custom/fake_merchant_inventory.tscn` | `Core/Features/Ancient/AncientCompendium.cs:1100,1811,2476`<br>`Core/Features/Merchant/MerchantRuntimeAppearance.cs:54` | 有 | 有 | 假商人事件/按钮/货架场景（注：源码注释误写 `fake_merchant_button.tscn` 仅 beta 有，实测正式版 v0.111.1 已有） |
| `res://animations/backgrounds/merchant_room/top/shop_merchant_top.tres`<br>`res://animations/backgrounds/merchant_room/hand/merchant_hand_skel_data.tres` | `Catalog/SkinCatalog.PrivateMerchant.cs:16`<br>`Core/PrivateMerchantResourceIdentity.cs:15-16` | 有 | 有 | 普通商人主骨架与手部骨架 |
| `res://animations/backgrounds/fake_merchant_room/bottom/`<br>`res://scenes/backgrounds/fake_merchant_event_encounter/fake_merchant_event_encounter_background.tscn` | `Catalog/SkinCatalog.Events.cs:146`<br>`Core/FakeMerchantEventArtPolicy.cs:21,27,29` | 有 | 有 | 假商人帐篷底层与遭遇背景 |
| `res://scenes/creature_visuals/{byrdpip,paels_legion,osty,kaiser_crab_boss_setup}.tscn`<br>`res://animations/monsters/osty/osty_skel_data.tres`<br>`res://scenes/backgrounds/kaiser_crab_boss/kaiser_crab_boss_background.tscn` | `Ui/OtherCreatureCatalog.cs:47,63,87`<br>`Ui/OstyModelPreviewControls.cs:24-25`<br>`Ui/EmperorCrabCompat/KaiserCrabSkinRefreshPatch.cs:34`<br>`Ui/EmperorCrabCompat/CombatCrabSkinPatch.cs:33` | 有 | 有 | 随从与帝皇蟹核心场景/骨架 |
| `res://images/ancients/vakuu_placeholder.png` | `Core/VakuuVanillaArt.cs:12` | 有 | 有 | 瓦库默认占位图 |
| `res://scenes/orbs/orb_visuals/{id}.tscn`、`res://images/orbs/` | `Core/MultiplayerRoomAppearancePatches.cs:119-120,160` | 有（5 球 / 6 图） | 有（5 球 / 6 图） | 联机房间充能球模型与图标 |
| `res://scenes/creature_visuals/{id}.tscn`<br>`res://scenes/merchant/characters/{id}_merchant.tscn`<br>`res://scenes/rest_site/characters/{id}_rest_site.tscn`<br>`res://scenes/screens/char_select/char_select_bg_{id}.tscn`<br>`res://scenes/ui/character_icons/{id}_icon.tscn`<br>`res://scenes/combat/energy_counters/{id}_energy_counter.tscn`<br>`res://scenes/vfx/card_trail_{id}.tscn`<br>`res://animations/characters/{id}/{id}_skel_data.tres`<br>`res://materials/transitions/{id}_transition_mat.tres` | `Catalog/ManagedCharacterAssetReplacementScanner.cs:497-514`<br>`Catalog/SkinCatalog.cs:1177,8739-8753`<br>`Core/SkinService.cs:8331`<br>`Ui/ContextualSkinControls.cs:123,135-136,2039,2160,2270,2941,3131,3199,3471,4072`<br>`Core/MultiplayerSkinSync.cs:557` 等 | 有 | 有 | 角色全套规范场景/动画/材质模板（测试版因新增愚者 `fool` 数量 +1 或 +2） |
| `res://images/packed/character_select/char_select_{id}[_locked].png`<br>`res://images/ui/top_panel/character_icon_{id}[_outline].png`<br>`res://images/packed/map/icons/map_marker_{id}.png`<br>`res://images/ui/hands/multiplayer_hand_{id}_{paper,point,rock,scissors}.png`<br>`res://images/packed/map/ancients/ancient_node_{id}[_outline].png`<br>`res://images/ui/run_history/{id}[_outline].png` | `Core/CharacterStripPortraits.cs:92`<br>`Core/SkinService.cs:8332-8333,8600,8936`<br>`Ui/ContextualSkinControls.cs:2042-2043,3425,3818,3826,4229,4241`<br>`Core/Features/Ancient/AncientCompendium.cs:3447,3460,3473,3486`<br>`Catalog/SkinCatalog.cs:8743-8753` | 有 | 有 | 选角头像、顶栏头像、地图图标、联机手势、先古地图/历史图标（注：其中 `char_select_*.png`、`character_icon_*.png` 等的全局重定向风险详见 **B 节**） |
| `res://images/atlases/card_atlas.sprites/`<br>`res://images/packed/card_portraits/`<br>`res://images/atlases/relic[_outline]_atlas.{png,sprites/}`<br>`res://images/relics/`<br>`res://images/timeline/epoch_portraits/[placeholder/]`<br>`res://images/atlases/epoch_atlas.{png,tpsheet}` | `Core/SkinService.cs:3692-3693`<br>`Catalog/SkinCatalog.cs:9950-9972`<br>`Ui/RelicSkinResources.cs:125,154-155` | 有 | 有 | 卡牌图集/卡面、遗物图集/大图、时间线插图/图集 |
| `res://images/events/[{id}[_phobia_mode].png]`<br>`res://images/packed/vfx/event/`<br>`res://scenes/events/background_scenes/`<br>`res://scenes/events/custom/`<br>`res://scenes/vfx/events/{id}_vfx.tscn`<br>`res://scenes/backgrounds/{title}/{title}_background.tscn`<br>`res://scenes/backgrounds/{title}/layers/{name}` | `Core/EventSkinPolicy.cs:12-14`<br>`Core/Features/Event/EventSkinRuntime.cs:59,147-148,176`<br>`Ui/EventCompendiumPreview.cs:152`<br>`Core/Features/Background/BackgroundSkinService.cs:2058,2107,2130,2798,3618` | 有 | 有 | 事件立绘/VFX/先古背景与战斗背景图层 |
| `res://localization/{candidate}/monsters.json`<br>`res://.godot/{imported/,global_script_class_cache.cfg,uid_cache.bin}`<br>`res://project.binary` | `Core/CompanionDisplayNameGuard.cs:83`<br>`Catalog/SkinCatalog.Workshop.cs:29`<br>`Catalog/SkinCatalog.cs:4433,4441-4442` | 有 | 有 | 本地化与 Godot 导出元数据 |

---

## B. 全局改写原版路径的写入点普查

> 参照标杆：`#235`（`Catalog/SkinCatalog.cs:6062-6128` `BuildIsolatedCardResources` + `CardBinaryResource.Rewrite`）将卡图隔离包改为纯私有路径 + 重写二进制依赖/清 UID，零规范路径写入；同时本次工作区已包含 GPT 正在推进的 `#241` 未提交改动（`IsCanonicalCharacterSelectPortrait` / `ProtectCanonicalSelectPortraitFile` / `PrepareSelectPortraitSafePack`，专门保护 `char_select_*.png`）。以下按**实际工作区当前状态**逐一盘点所有仍会向全局 VFS 或全局资源缓存写入原版规范路径的位置。

### B.1 汇总表（按风险由高到低）

| # | 风险等级 | 函数与位置（文件:行） | 写入/挂载方式与触发时机 | 会不会被其他角色或 Mod 读到 | 收窄建议 |
| --- | --- | --- | --- | --- | --- |
| **B1** | **高（确定的绕过漏洞）** | `ProtectedCharacters/CharSelectStageCycle.cs:357-386`（`EnsureProviderPackMounted`，核心行 `:376`） | 选人界面切战损档位（如贞德 s2/s3）时，直接对原始 `.pck` 调用 **`ProjectSettings.LoadResourcePack(full, replaceFiles: true)`** | **会（100% 绕过 `#241` 防护）**。完全绕开 `PrepareSelectPortraitSafePack`、`MountedLargeRuntimeProviderPacks`、`MountedOverlayOwners` 与 `MountOverlayCore` 校正层；哪怕 `#241` 已从常规提权包剔除 `char_select_ironclad.png.import`，只要点一下贞德切战损，未过滤的原始 PCK 就会被直接压回全局栈顶，瞬间重新污染 `char_select_ironclad.png` 等原版规范路径且无后续校正恢复 | 改为 `replaceFiles: false`（仅开放包内私有战损路径），或统一经 `SkinService.PrepareSelectPortraitSafePack` / 私有运行别名加载 `s2`/`s3` 骨架，禁止裸调 `ProjectSettings.LoadResourcePack(full, replaceFiles: true)` |
| **B2** | **高（主覆盖层面广）** | `Catalog/SkinCatalog.cs:3080-3383`（`BuildOverlay`）+ `Core/SkinService.cs:6579-6805`（`MountOverlayCore` → `MountOverlayDelta` `:7749` `replaceFiles: true`） | 每次皮肤切换、进局/换场景时，将增量覆盖包以 `replaceFiles: true` 挂到全局 VFS 栈顶。<br>① `:3222-3277`：除先古背景、选角头像（`#241`）、遗物、时间线外，其余分组资产一律写回原版规范路径；<br>② `:3201-3220, 3315-3362` → `CollectSelectedProviderOverlayDependenciesCore`（`:4308-4380`）+ `ShouldMountProviderDependency`（`:4063-4091`）：选中提供者的依赖闭包（及全选中大包除工程控制文件外的**全部文件**）只要不在 `selectableProviderFiles` 且未命中 4 条排除项，全部默认写入全局覆盖包 | **会**。① 第三方角色 Mod 若像响木天音一样借用原版角色的 `images/ui/top_panel/character_icon_<id>.png`、`images/packed/map/icons/map_marker_<id>.png`、`scenes/ui/character_icons/<id>_icon.tscn`、`images/ui/hands/multiplayer_hand_<id>_*.png`、`materials/transitions/<id>_transition_mat.tres`、`scenes/combat/energy_counters/<id>_energy_counter.tscn`、`scenes/vfx/card_trail_<id>.tscn` 或 `scenes/creature_visuals/<id>.tscn`，会直接读到被全局改写的皮肤资源；<br>② 皮肤包夹带的共享原版路径（如 `scenes/orbs/orb_visuals/*`、`images/orbs/*`、通用 VFX/Shader/字体/UI）若未被登记为可选分组文件，会经 `ShouldMountProviderDependency` 直接泄露到全局 | ① 优先将已有按模型读取入口的角色 UI 图标族（`character_icon_*`、`map_marker_*`、`*_icon.tscn`、`multiplayer_hand_*`、`ancient_node_*`、`run_history/*`）参照 `#241` 移出 `BuildOverlay` 全局写盘，改走按模型私有别名；<br>② 将 `ShouldMountProviderDependency` 从「排除黑名单」改为「规范目录白名单」：原版命名空间（`res://images/`、`res://scenes/`、`res://materials/`、`res://themes/`）下非当前分组显式拥有的文件默认禁止并入全局覆盖包 |
| **B3** | **高（隔离包的规范桥接残留）** | `Catalog/SkinCatalog.cs:6158-6286`（`BuildAliasedResourceOverlay`）+ `SkinCatalog.cs:5355-5474`（`BuildRuntimeResourceOverlay`）+ `Core/SkinService.cs:6086-6225`（`WithPreparedRuntimeResources`，`:6105` `replaceFiles: true`） | 除卡图（`#235` `isolateCardDependencies: true`）、遗物（`:5508-5510` 事后剔除）、时间线（`:3929-3931` 事后剔除）外，所有走 `BuildRuntimeResourceOverlay` 的独立运行包都会：<br>① `:6218-6226` 为 `.tscn` 写 `sourcePath + ".remap"` 指向私有别名；<br>② `:6278-6285` 为每个带 `.remap`/`.import` 的资源写规范路径 `sourcePath + ".remap"/".import"` 指向私有载荷（仅排除 `IsCanonicalCharacterSelectPortrait` 与 `FakeMerchantEventArtPolicy`）；<br>③ `:5446-5473` 当 `includeProviderDependencies == true` 时还把 `CollectSelectedProviderDependencies` 找到的依赖直接以原始规范路径写进同一 PCK | **会**。`WithPreparedRuntimeResources` 的 `finally`（`SkinService.cs:6209-6216`）仅对 `prepared.RestoreGroups` 调用 `MountOverlay` 恢复——而 `RestoreGroups` 只包含「**其他目录分组**」，**不恢复**当前 `groupId` 自己的规范路径、共享原版路径或非目录的第三方内容 Mod 借用路径。因此这些规范 `.remap`/`.import` 桥接在作用域结束后仍滞留在 Godot 包栈顶；后续其他角色/Mod 读到同一规范路径即串图（联机队友作用域未触及 `RestoreGroups` 时亦然） | ① 将 `#235` 的纯私有模式（泛化 `isolateCardDependencies` 为 `suppressCanonicalRedirects` + `CardBinaryResource` 依赖重写）逐步推广到单图/UI/事件图/简单 `.tres` 资源，直接停止发射规范桥接；<br>② 对因二进制 `.scn` 内部硬编码路径而暂时必须发射桥接的复杂 Spine 场景，在 `finally` 中除 `RestoreGroups` 外，对非常驻选中或联机临时作用域写出的 `CanonicalDependencyPaths` 显式压回原版基线 |
| **B4** | **高（整包提权窗口与非分组路径穿透）** | `Core/SkinService.cs:7149-7299`（`MountSelectedLargeRuntimeProviderPacks`，`:7270` `replaceFiles: true`）<br>`Core/SkinService.cs:7085-7113`（`MountContextualFullRuntimeProviderPacks`，`:7105` `replaceFiles: true`）<br>辅助：`SkinService.cs:7061-7083`（`PrepareSelectPortraitSafePack`） | 选中完整运行时大包（或 ≥ 阈值大包，非仅随从）时，直接 `LoadResourcePack(PrepareSelectPortraitSafePack(...), replaceFiles: true)` 整包提权到栈顶；联机加载本地未全选中的队友大包时亦于 `:7105` 整包 `replaceFiles: true` 提权 | **会**。当前 `PrepareSelectPortraitSafePack` **只剥离了 `char_select_*.png` 及其 `.godot/imported/` 载荷**，包内其余全部原版规范路径（顶栏头像、地图图标、充能球、遗物/时间线/卡图、通用 UI/VFX、甚至对其他第三方 Mod 路径的同名文件）都会先被压到栈顶。虽随后 `MountOverlayCore` 用 `BuildGameBaselineOverlay`（`:6682`）与 `foreignPromotedPaths`（`:6736-6772`）做校正，但：① 第三方 Mod 规范路径不在 `_gameArchive` 中，不会被 `BuildGameBaselineOverlay` 还原；② `foreignPromotedPaths` 跳过 `FindGroupIdForResourcePath == null` 的共享原版路径，而 `selectedOverlay`（`:6726`）会把非分组路径覆盖回来；③ 联机 `:7105` 结束后的恢复受限于 `restoreGroups` | 将 `PrepareSelectPortraitSafePack` 升级为通用的规范路径剥离器（如 `PrepareCanonicalSafePack`）：提权前即统一剔除卡图、遗物、时间线、选角头像及所有改由私有路径加载的规范路径（含其 `.godot/imported/` 载荷）；同时收紧 `MountOverlayCore:6726` 不允许把非当前分组拥有的原版路径从 `selectedOverlay` 写回 |
| **B5** | **中（内存级全局缓存接管 `TakeOverPath` / `SetAsset`）** | `Core/SkinService.RuntimeSafety.cs:91-117`（`BindRuntimeDependency` → `TakeOverRuntimeCanonicalPath` `:116`）<br>`Core/SkinService.cs:6143, 6179, 6189-6199, 6227-6253`（`BeginCanonicalRuntimeResourceOwnership`）<br>`Ui/ContextualSkinControls.cs:2653-2662`（`TakeOverCharacterSelectSceneCache`） | ① `BindRuntimeDependency` 对每个绑定依赖调用 `resource.TakeOverPath(canonical)`（`:95` 仅排除 `IsCanonicalCharacterSelectPortrait`）；非联机场景下 `SkinService.cs:6196-6199` 主动把本组条目移出恢复列表，`:6252` 返回 `NoopDisposable`，导致接管**在作用域结束后永久保留**；<br>② `TakeOverCharacterSelectSceneCache` 对 `char_select_bg_<id>.tscn` 同时执行 `scene.TakeOverPath(scenePath)` 和 `PreloadManager.Cache.SetAsset(scenePath, scene)` | **会（绕过 VFS 保护）**。即使 VFS 已将某原版规范路径保持为原版，只要内存级 `ResourceLoader` 缓存或 `PreloadManager.Cache` 已被 `TakeOverPath` / `SetAsset` 接管，其他角色或 Mod 以 `CacheMode.Reuse` / `GetAsset` 读取该规范路径时仍会直接拿到皮肤对象（例如若有自定义角色借用原版角色的 `char_select_bg_<id>.tscn` 或通用骨架/贴图） | ① `BindRuntimeDependency` 与 VFS 层共用同一份「禁止全局接管规范路径」谓词清单，不局限于 `char_select_*.png`；<br>② 除已证实依赖规范 `ResourcePath` 的第三方脚本（如贞德调色板/战损处理器）外，作用域结束即通过 `CanonicalRuntimeResourceOwnershipScope.Dispose` 归还规范路径 |
| **B6** | **低（受控专用路径）** | `Catalog/SkinCatalog.Events.cs:205-255`（`AddFakeMerchantEventPresentation` / `ScanFakeMerchantEventPresentation`）<br>`Core/SkinService.cs:5168-5195, 6894-6908`（`MountFakeMerchantEventArt` → `MountOverlayDelta(..., "fake_merchant_event", ...)`） | 只要安装了包含 `res://animations/backgrounds/fake_merchant_room/bottom/` 或 `res://images/events/fake_merchant*` 的事件立绘包，就在每次 `MountOverlayCore` 末尾与进假商人事件前以 `replaceFiles: true` 全局挂载 | 不会串到角色或普通商人（路径严格限定在假商人帐篷底层与假商人事件图）；仅当存在多个此类事件包或需要独立切回原版假商人帐篷时才互相覆盖 | 当前范围已足够窄，维持现状即可（若后续需要原版开关，可纳入普通事件分组） |
| **B7** | **低（`replaceFiles: false` 低优先级挂载的“空位首占”边角）** | `Core/SkinService.cs:7137`（`EnsureScopedRuntimeProviderResourcesMounted`）<br>`Core/SkinService.cs:7230`（`MountSelectedLargeRuntimeProviderPacks` 仅随从分支）<br>`Core/Features/Background/BackgroundSkinService.cs:2216`（`TryInstantiatePackBackground`） | 以 `replaceFiles: false` 挂载原始 PCK 以便解析私有资源路径。前两处经过 `PrepareSelectPortraitSafePack`；**`BackgroundSkinService.cs:2216` 未经任何过滤**且裸调 `ProjectSettings.LoadResourcePack` | 不会覆盖游戏原包已存在的规范路径；但若某规范路径在游戏原包中**不存在**（如 `#235` 中的 `event/rebound.tres`、测试版删掉的旧路径、或晚加载第三方 Mod 的路径），低优先级整包里的同名文件仍会首占该规范路径并被全局读到 | 对 `BackgroundSkinService.cs:2216` 改为只打包/挂载该背景包的背景目录或经安全过滤包装；必要时对 `replaceFiles: false` 挂载也剥离游戏规范目录前缀（只保留包私有命名空间） |

---

### B.2 已确认安全的对照点（无需收窄，可作参照）

1. **卡图隔离（`#235` 标杆）**：`Catalog/SkinCatalog.cs:6062-6128`（`BuildIsolatedCardResources`）传 `isolateCardDependencies: true` 并用 `CardBinaryResource.Rewrite` 改写二进制内部依赖和清除 UID；`BuildCardOverlay`（`:4093-4180`）将全部卡牌规范路径保持在 `ResolveBaseline`，仅挂载卡包私有命名空间。**无全局规范路径污染。**
2. **遗物与时间线隔离**：`Catalog/SkinCatalog.cs:5479-5520`（`BuildIsolatedRelicResourceOverlay`）与 `:3900-3938`（`BuildIsolatedEpochPortraitOverlay`）均主动剔除了 `overlay.CanonicalDependencyPaths`，且 `BuildOverlay`（`:3252-3255`）与 `ShouldMountProviderDependency`（`:4072-4073`）已阻止它们写进全局覆盖包。
3. **原版恢复辅助**：`Core/SkinService.cs:8691-8751`（`EnsureGameSceneOnTop`）与 `:8796-8930`（`LoadGameSceneViaAlias` / `InstantiateGameCreatureVisuals`）只向规范路径写回游戏原包字节或挂到 `res://__skin_changer_base/` 私有别名，不引入皮肤污染。

---

### 可选、不阻塞（后续按优先级整理）

1. **优先立刻顺手修（成本极低、收益明确）**：
   - 修 `CharSelectStageCycle.cs:376` 裸调 `ProjectSettings.LoadResourcePack(full, replaceFiles: true)` 绕过 `#241` `PrepareSelectPortraitSafePack` 的漏洞（否则点贞德切战损会击穿天音修复）；
   - 修 `MerchantRuntimeAppearance.cs:48,50-53,1863`（正式版预热与解析走 `GetMerchantScenePath`）、`AncientCompendium.cs:1805`（普通商人兜底路径走 `GetMerchantScenePath`）与 `SkinCatalog.cs:9894`（`MerchantAppearancePathRegex` 补回 `merchant_room.tscn`）。
2. **中期架构收窄（防下一批“天音/回弹类”串图）**：
   - 将 `character_icon_*`、`map_marker_*`、`*_icon.tscn`、`multiplayer_hand_*` 等角色附属 UI 规范路径按 `#241` 方式迁出 `BuildOverlay` 全局写入；
   - 将 `PrepareSelectPortraitSafePack` 升级为通用规范路径过滤整包，并把 `ShouldMountProviderDependency` 改为对原版命名空间默认拒绝。
3. **跨版本防御**：
   - 在 `CardSkinControls.cs` / `SkinService.LoadCardPresentationResource` 中考虑纯测试版卡包只覆写 `compressed_atlas.sprites/ancient_text_bg_*.tres` 时的双向提供者查询；
   - 测试版新增的 `res://scenes/rooms/blitz_merchant.tscn`（待确认用途）在出现对应极速商人包时决定是否纳入 `MerchantAppearancePathRegex`。

