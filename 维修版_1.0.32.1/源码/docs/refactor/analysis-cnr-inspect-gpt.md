# CNR 图鉴大图分析 · GPT · 2026-11-14

基于 1.0.31.107 源码、三份 cmp106-107 日志及本机游戏 / Regent DLL 反编译。源码路径相对 `STS2SkinChanger/`，反编译文件在 `%TEMP%/sts2-gpt-cnr-{NInspectCardScreen,NCard,CardModel,RegentCardsAnimeRework}.cs`。

1. **根因是大图的最后一次赋图绕过了 Portrait 替换。** `UpdateCardDisplay` 克隆卡牌并做升级预览，先设置 Model、运行 UpdateVisuals，最后又执行 `SetPortraitTexture(cardModel2.BigPortrait)`。`BigPortrait` 在有独立 PNG 时直接取 `packed/card_portraits/<pool>/<id>.png`，只有缺图才回到 Portrait。CNR 的替换经 `.tres` / remap 和隔离提供者生效，游戏原包仍有同卡的独立 PNG，所以先换好的图又被原版覆盖。依据反编译 NInspect L372–395、CardModel L165–194、NCard L1255–1275/L1305–1317，以及 `Core/SkinService.cs` L3538–3552/L3644–3715。只读包索引也确认 CNR 的 `ricochet.tres.remap` 与包内 `Ricochet.png.import`，游戏原包另有规范小写 `ricochet.png.import`。

2. **“跟随分类”触发另一条最终刷新。** 检视 `ApplySelection → ApplyCardSelection` 后，CallDeferred 执行 `RefreshMatchingCards → RefreshCardSkin → Reload + UpdateVisuals`，最终按现有 Portrait 解析器设置 CNR。这次刷新结束后没有游戏大图那次 BigPortrait 覆写。重开重新执行 UpdateCardDisplay，于是重复覆盖。当前检视 Display Postfix 只 Sync 下拉。依据 `Ui/CardSkinControls.cs` L2341–2377/L2457–2474/L1784–1800/L2508–2513。

3. **Regent 有额外布局路径。** 该包 manifest 的 MeteorShower、SovereignBlade、TheSealedThrone 声明 Ancient 布局，呈现层会隐藏普通 Portrait、显示 AncientPortrait，原生 SetPortraitTexture 则按 Model.Rarity 返回值决定写哪个槽。在稀有度仍为普通卡时，可见先古 / 扩展槽可因此保留选中图。依据 `Ui/CardSkinControls.cs` L587–601/L646–655/L1114–1123、反编译 NCard L1305–1317。22.10.08 日志 L688 确有 Regent 的普通槽隐藏、先古槽显示记录，给定材料未标明云的 Regent 对照卡名，具体正常原因仍受此限制。Regent DLL 的检视补丁自身只改悬停提示，见其反编译 L1030–1053。

4. **最小修法是补检视界面的最终呈现。** 在 `CardInspectSkinDisplayPatch.Postfix` 完成游戏 UpdateCardDisplay、同步下拉后，取当前 `NCard.Model`，对有效受管非 base 选择复用 `ApplySelectedPortraitToNode`。它已按当前卡的呈现选择普通 / 先古 / 扩展槽，并尊重外部编辑器 Portrait 归属。随后沿用现有最终链的 `StatefulCardArtView.Apply` 与外部提供者同步，保持状态卡图和 Regent 的布局、租约规则，采用直接呈现即可。依据 `Ui/CardSkinControls.cs` L1095–1124/L2691–2700、`Core/SkinService.cs` L3224–3231、`Ui/StatefulCardArtView.cs` L39–53。需由 Claude 实现并实测 CNR 与对照包。

统一卡图清单可以统一纯资源包、DLL 导出的图片及状态定义，消除各包路径布局差异。
要整类避免这次覆盖，所有界面必须接入同一解析器，并覆盖 BigPortrait 和最终节点赋图入口。
清单还需声明普通 / 大图回退、卡牌身份与升级状态、单卡优先级、布局和外部编辑器归属。
配合隔离资源缓存及图鉴网格 / 大图一致性验证，才能让统一清单真正覆盖完整显示链。

## Claude 实现（1.0.31.108）
`CardInspectSkinDisplayPatch.Postfix` 在 Sync 之后调用 `CardInspectSkinControls.ReapplySelectedPortrait`：只在这张卡有受管卡图（`SkinService.HasManagedCardPortrait`）或带状态卡图时，按最终呈现链（`ApplySelectedPortraitToNode` → `StatefulCardArtView.Apply` → `ExternalCardVisualBridge.SynchronizeProvider`）补一次图；选原版时保留游戏大图。实测步骤：图鉴 → 猎人 → 打开大图应为 CNR 图，关掉重开仍是；储君、带状态卡图的大图不变。
