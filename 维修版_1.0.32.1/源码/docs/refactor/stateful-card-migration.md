# 带状态卡图模块迁移（Claude，阶段 C 第 3 批，基线 1.0.31.106）

## 进模块（`[FeaturePatch("stateful-card")]`，11 个类）
装了带状态卡图的包才装这些补丁，没装时整块不装。

| 文件 | 补丁类 | 目标方法 |
| --- | --- | --- |
| `Core/StatefulCardArtRuntime.cs` | `StatefulCardLocalizationPatch` | `LocManager.SetLanguage` |
| `Ui/StatefulCardCosmeticPatches.cs` | `StatefulCardTooltipPatch` | `CardModel.HoverTips` / `ExtraHoverTips` |
| 同上 | `StatefulRewardGlowPatch` | `NCard.ActivateRewardScreenGlow` |
| 同上 | `StatefulInfectionOverlayPatch` | `CardModel.CreateOverlay` |
| 同上 | `StatefulCharacterSceneExitPatch` | `NCharacterSelectScreen.BeginRun`（会遍历整个选角界面） |
| 同上 | `StatefulCardPlayCosmeticPatch` | `Neurosurge.OnPlay` / `Clash.OnPlay` |
| 同上 | `StatefulDamageCosmeticPatch` | `AbstractModel.AfterDamageGiven`（每次伤害结算、每个模型都会走） |
| 同上 | `StatefulSideTurnCosmeticPatch` | `AbstractModel.AfterSideTurnStart` |
| `Ui/StatefulCardArtView.cs` | `StatefulCardTreeExitPatch` | `NCard._ExitTree` |
| `Ui/StatefulCombatEffects.cs` | `StatefulExhaustInitializePatch` | `NExhaustPileButton.Initialize`（每场战斗往消耗堆按钮挂一个逐帧节点） |
| 同上 | `StatefulExhaustHidePatch` | `NCombatCardPile.AnimOut` |

不归这个模块管的：提供者自己的 `NCardAwareCardsPatch.get_Node` 前缀。它在第一次为某个包建立运行时时才装（`StatefulCardArtRuntime.Runtime` 构造），本来就是按需的。

## 判定
- `ShouldBeActive`（`StatefulCardFeaturePolicy`，LogicTests 4 条断言）：`SkinCatalog.PckCardOptions` 里有任何选项带 `StatefulArt` 合同就激活；目录没就绪时返回 null。
- `IsInUse`：`NRun.Instance` 在场景树里，也就是在局内。这是保守判断：战斗特效、消耗堆图标、手牌的状态图都只在局内出现。
- `OnActivated` / `OnDeactivating`：都留空。模块只在包增减时才变化，在局内不会切换。

## 其他
- 熔断 key：这几个文件里原来用的 `patch:card` / `patch:core` 改成了 `patch:stateful-card`。这样这块出错只会熔断它自己，不会连带卸掉卡图主体模块（`card`，GPT 在迁）。
- 和 `card` 模块的交界：卡图主体的 `NCard.Reload` 系列、`_EnterTree`、回池等补丁会直接调用 `StatefulCardArtView`，GPT 把它们留在主干，所以带状态卡图的显示不受这个模块装卸影响。
- 没有延迟回调需要加代次。
