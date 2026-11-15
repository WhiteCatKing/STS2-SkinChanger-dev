## 改了什么

共 **39** 处，涉及 **13** 个文件、37 个补丁类。
包裹方式：`void Prefix/Postfix` → `Safe.Run(key, () => { 原方法体 })`；`bool Prefix` → `return Safe.Get(key, () => { 原方法体 }, true)`（出错放行原方法）。

| 模块 key | 处数 |
|---|---|
| `patch:ancient` | 2 |
| `patch:background` | 4 |
| `patch:card` | 23 |
| `patch:event` | 2 |
| `patch:monster` | 8 |

按文件：

| 文件 | 处数 |
|---|---|
| `STS2SkinChanger/Ui/CardSkinControls.cs` | 17 |
| `STS2SkinChanger/Ui/BestiaryEntranceSwitcher.cs` | 5 |
| `STS2SkinChanger/Ui/ContextualSkinControls.cs` | 3 |
| `STS2SkinChanger/Ui/StatefulCardCosmeticPatches.cs` | 3 |
| `STS2SkinChanger/Core/BackgroundSkinService.cs` | 2 |
| `STS2SkinChanger/Ui/AncientCompendium.cs` | 2 |
| `STS2SkinChanger/Core/EventSkinRuntime.cs` | 1 |
| `STS2SkinChanger/Core/StatefulCardArtRuntime.cs` | 1 |
| `STS2SkinChanger/Ui/BackgroundAppearanceScreen.cs` | 1 |
| `STS2SkinChanger/Ui/CardSurfaceView.cs` | 1 |
| `STS2SkinChanger/Ui/EventCompendiumPreview.cs` | 1 |
| `STS2SkinChanger/Ui/MonsterSkinPriorityControls.cs` | 1 |
| `STS2SkinChanger/Ui/StatefulCombatEffects.cs` | 1 |

### 明细

| 位置 | 补丁类 | 方法 | 返回 | key |
|---|---|---|---|---|
| `STS2SkinChanger/Core/BackgroundSkinService.cs:3788` | BackgroundSkinRoomPatch | Postfix | void | `patch:background` |
| `STS2SkinChanger/Core/BackgroundSkinService.cs:3801` | FakeMerchantCombatArtPatch | Prefix | void | `patch:background` |
| `STS2SkinChanger/Core/EventSkinRuntime.cs:169` | EventSkinPortraitTrackingPatch | Postfix | void | `patch:event` |
| `STS2SkinChanger/Core/StatefulCardArtRuntime.cs:245` | StatefulCardLocalizationPatch | Postfix | void | `patch:card` |
| `STS2SkinChanger/Ui/AncientCompendium.cs:3288` | AncientSceneResultPatch | Prefix | void | `patch:ancient` |
| `STS2SkinChanger/Ui/AncientCompendium.cs:3281` | ManagedAncientStaticBackgroundWindowPatch | Postfix | void | `patch:ancient` |
| `STS2SkinChanger/Ui/BackgroundAppearanceScreen.cs:389` | BackgroundAppearancePauseMenuPatch | Postfix | void | `patch:background` |
| `STS2SkinChanger/Ui/BestiaryEntranceSwitcher.cs:384` | BestiaryEntranceForceClickPatch | Prefix | void | `patch:monster` |
| `STS2SkinChanger/Ui/BestiaryEntranceSwitcher.cs:405` | BestiaryEntranceOpenedPatch | Postfix | void | `patch:monster` |
| `STS2SkinChanger/Ui/BestiaryEntranceSwitcher.cs:391` | BestiaryEntrancePushPatch | Prefix | bool | `patch:monster` |
| `STS2SkinChanger/Ui/BestiaryEntranceSwitcher.cs:398` | BestiaryEntranceReadyPatch | Postfix | void | `patch:monster` |
| `STS2SkinChanger/Ui/BestiaryEntranceSwitcher.cs:378` | BestiaryEntranceReleasePatch | Prefix | void | `patch:monster` |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2608` | CardBuiltInOverlayPatch | Postfix | void | `patch:card` |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2506` | CardInspectSkinDisplayPatch | Postfix | void | `patch:card` |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2499` | CardInspectSkinReadyPatch | Postfix | void | `patch:card` |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2660` | CardLayoutBaselineCapturePatch | Postfix | void | `patch:card` |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2683` | CardLayoutFinalPatch | Postfix | void | `patch:card` |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2673` | CardLayoutFinalPatch | Prefix | void | `patch:card` |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2548` | CardLibraryInitialPortraitPreloadPatch | Postfix | void | `patch:card` |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2535` | CardLibraryPoolSkinPatch | Postfix | void | `patch:card` |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2573` | CardLibraryRowPortraitPreloadPatch | Prefix | void | `patch:card` |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2514` | CardLibrarySkinOpenedPatch | Postfix | void | `patch:card` |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2512` | CardLibrarySkinOpenedPatch | Prefix | void | `patch:card` |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2493` | CardLibrarySkinReadyPatch | Postfix | void | `patch:card` |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2720` | CardPlayQueueRebindPortraitPatch | Postfix | void | `patch:card` |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2601` | CardPreviewModeCapturePatch | Prefix | void | `patch:card` |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2586` | CardRewardPortraitPreloadPatch | Prefix | void | `patch:card` |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2706` | CardTreeReentrySkinPatch | Postfix | void | `patch:card` |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2713` | LocalCardPlayQueuePortraitPatch | Postfix | void | `patch:card` |
| `STS2SkinChanger/Ui/CardSurfaceView.cs:90` | TinyCardSelectedSurfacePatch | Postfix | void | `patch:card` |
| `STS2SkinChanger/Ui/ContextualSkinControls.cs:3907` | BestiarySelectionSkinPatch | Postfix | void | `patch:monster` |
| `STS2SkinChanger/Ui/ContextualSkinControls.cs:3996` | MonsterVisualResultPatch | Prefix | void | `patch:monster` |
| `STS2SkinChanger/Ui/ContextualSkinControls.cs:4187` | SkinPopupBackgroundMutePatch | Prefix | bool | `patch:background` |
| `STS2SkinChanger/Ui/EventCompendiumPreview.cs:293` | EventPreviewLifecyclePatch | Prefix | bool | `patch:event` |
| `STS2SkinChanger/Ui/MonsterSkinPriorityControls.cs:816` | BestiaryInitialSkinNamePatch | Postfix | void | `patch:monster` |
| `STS2SkinChanger/Ui/StatefulCardCosmeticPatches.cs:97` | StatefulCardPlayCosmeticPatch | Prefix | void | `patch:card` |
| `STS2SkinChanger/Ui/StatefulCardCosmeticPatches.cs:124` | StatefulDamageCosmeticPatch | Postfix | void | `patch:card` |
| `STS2SkinChanger/Ui/StatefulCardCosmeticPatches.cs:44` | StatefulRewardGlowPatch | Prefix | bool | `patch:card` |
| `STS2SkinChanger/Ui/StatefulCombatEffects.cs:156` | StatefulExhaustHidePatch | Prefix | bool | `patch:card` |

## 跳过清单（51 处）

| 位置 | 补丁类 | 方法 | 原因 |
|---|---|---|---|
| `STS2SkinChanger/Core/BackgroundSkinService.cs:3821` | BackgroundSkinCreatePatch | Postfix | ref/out 参数 |
| `STS2SkinChanger/Core/EventSkinRuntime.cs:114` | EventSkinResourceAvailabilityPatch | TargetMethods | Transpiler/Finalizer/Prepare/TargetMethod |
| `STS2SkinChanger/Core/EventSkinRuntime.cs:119` | EventSkinResourceAvailabilityPatch | Prefix | ref/out 参数 |
| `STS2SkinChanger/Core/EventSkinRuntime.cs:135` | EventSkinTextPatch | Prefix | ref/out 参数 |
| `STS2SkinChanger/Core/EventSkinRuntime.cs:148` | EventSkinVfxPatch | Prefix | ref/out 参数 |
| `STS2SkinChanger/Core/FrameworkSkinRuntimePatches.cs:276` | FrameworkCardFramePatch | Prefix | ref/out 参数 |
| `STS2SkinChanger/Core/FrameworkSkinRuntimePatches.cs:296` | FrameworkCardEnergyIconPatch | Prefix | ref/out 参数 |
| `STS2SkinChanger/Core/FrameworkSkinRuntimePatches.cs:313` | FrameworkEnergyIconHelperPatch | Prefix | ref/out 参数 |
| `STS2SkinChanger/Core/FrameworkSkinRuntimePatches.cs:427` | FrameworkOrbIconPatch | Prefix | ref/out 参数 |
| `STS2SkinChanger/Core/FrameworkSkinRuntimePatches.cs:445` | FrameworkOrbSpritePatch | Prefix | ref/out 参数 |
| `STS2SkinChanger/Core/FrameworkSkinRuntimePatches.cs:536` | FrameworkRelicIconTexturePatch | Prefix | ref/out 参数 |
| `STS2SkinChanger/Core/FrameworkSkinRuntimePatches.cs:553` | FrameworkRelicOutlineTexturePatch | Prefix | ref/out 参数 |
| `STS2SkinChanger/Core/FrameworkSkinRuntimePatches.cs:570` | FrameworkRelicBigIconTexturePatch | Prefix | ref/out 参数 |
| `STS2SkinChanger/Ui/AncientCompendium.cs:3253` | ManagedAncientSceneAnimationPatch | Postfix | 已有 try |
| `STS2SkinChanger/Ui/AncientCompendium.cs:3308` | AncientSceneResultPatch | Postfix | ref/out 参数 |
| `STS2SkinChanger/Ui/AncientCompendium.cs:3345` | AncientRuntimeProviderScopeExitPatch | Postfix | 收尾：先古场景 _ExitTree 后恢复行为范围 |
| `STS2SkinChanger/Ui/AncientCompendium.cs:3353` | AncientMapIconResultPatch | Postfix | ref/out 参数 |
| `STS2SkinChanger/Ui/AncientCompendium.cs:3365` | AncientMapIconOutlineResultPatch | Postfix | ref/out 参数 |
| `STS2SkinChanger/Ui/AncientCompendium.cs:3377` | AncientRunHistoryIconResultPatch | Postfix | ref/out 参数 |
| `STS2SkinChanger/Ui/AncientCompendium.cs:3389` | AncientRunHistoryIconOutlineResultPatch | Postfix | ref/out 参数 |
| `STS2SkinChanger/Ui/BackgroundAppearanceScreen.cs:382` | BackgroundAppearancePauseMenuPatch | TargetMethods | Transpiler/Finalizer/Prepare/TargetMethod |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2520` | CardLibraryAvailabilityFilterPatch | TargetMethod | Transpiler/Finalizer/Prepare/TargetMethod |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2526` | CardLibraryAvailabilityFilterPatch | Prefix | ref/out 参数 |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2544` | CardLibraryInitialPortraitPreloadPatch | TargetMethod | Transpiler/Finalizer/Prepare/TargetMethod |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2580` | CardRewardPortraitPreloadPatch | TargetMethod | Transpiler/Finalizer/Prepare/TargetMethod |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2593` | CardPortraitResultPatch | Postfix | ref/out 参数 |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2623` | CardLayoutModelRebindPatch | Prefix | 收尾：换模型前释放旧呈现 |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2632` | CardLayoutPoolReleasePatch | TargetMethods | Transpiler/Finalizer/Prepare/TargetMethod |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2639` | CardLayoutPoolReleasePatch | Prefix | 收尾：对象池回收时释放呈现 |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2645` | CardLayoutResetPatch | TargetMethod | Transpiler/Finalizer/Prepare/TargetMethod |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2649` | CardLayoutResetPatch | Prefix | 收尾/还原 |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2656` | CardLayoutBaselineCapturePatch | TargetMethod | Transpiler/Finalizer/Prepare/TargetMethod |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2666` | CardLayoutFinalPatch | TargetMethods | Transpiler/Finalizer/Prepare/TargetMethod |
| `STS2SkinChanger/Ui/CardSurfaceView.cs:97` | TinyCardAnonymousSurfacePatch | Prefix | 收尾：清除 tiny icon 的旧卡牌关联 |
| `STS2SkinChanger/Ui/CardTransformPreload.cs:30` | TransformPreviewPortraitPreloadPatch | Prefix | 已有 try |
| `STS2SkinChanger/Ui/CardTransformPreload.cs:69` | CardTransformVfxPortraitPreloadPatch | Prefix | 已有 try |
| `STS2SkinChanger/Ui/CardTransformPreload.cs:91` | CardTransformShineVfxPortraitPreloadPatch | Prefix | 已有 try |
| `STS2SkinChanger/Ui/CardTransformPreload.cs:124` | CardTransformShineVfxPortraitPreloadPatch | Postfix | 已有 try |
| `STS2SkinChanger/Ui/ContextualSkinControls.cs:3924` | BestiaryRuntimeProviderScopePatch | Postfix | 收尾：图鉴关闭后恢复/清空范围 |
| `STS2SkinChanger/Ui/ContextualSkinControls.cs:4011` | MonsterVisualResultPatch | Postfix | ref/out 参数 |
| `STS2SkinChanger/Ui/DecimillipedeBestiarySupport.cs:19` | DecimillipedeBestiarySetupCleanupPatch | Prefix | 收尾/还原 |
| `STS2SkinChanger/Ui/EventCompendiumPreview.cs:286` | EventPreviewLifecyclePatch | TargetMethods | Transpiler/Finalizer/Prepare/TargetMethod |
| `STS2SkinChanger/Ui/RelicSkinResources.cs:46` | RelicIconSkinPatch | Postfix | ref/out 参数 |
| `STS2SkinChanger/Ui/RelicSkinResources.cs:54` | RelicIconOutlineSkinPatch | Postfix | ref/out 参数 |
| `STS2SkinChanger/Ui/StatefulCardArtView.cs:292` | StatefulCardTreeExitPatch | Prefix | 收尾：_ExitTree 释放 lease/节点 |
| `STS2SkinChanger/Ui/StatefulCardCosmeticPatches.cs:21` | StatefulCardTooltipPatch | TargetMethods | Transpiler/Finalizer/Prepare/TargetMethod |
| `STS2SkinChanger/Ui/StatefulCardCosmeticPatches.cs:26` | StatefulCardTooltipPatch | Postfix | ref/out 参数 |
| `STS2SkinChanger/Ui/StatefulCardCosmeticPatches.cs:57` | StatefulInfectionOverlayPatch | Postfix | ref/out 参数 |
| `STS2SkinChanger/Ui/StatefulCardCosmeticPatches.cs:76` | StatefulCharacterSceneExitPatch | Prefix | 收尾：退出选人时复位手指位置 |
| `STS2SkinChanger/Ui/StatefulCardCosmeticPatches.cs:91` | StatefulCardPlayCosmeticPatch | TargetMethods | Transpiler/Finalizer/Prepare/TargetMethod |
| `STS2SkinChanger/Ui/StatefulCardCosmeticPatches.cs:149` | StatefulSideTurnCosmeticPatch | Postfix | 收尾：侧回合开始清旧效果 |

### 跳过原因统计

| 原因 | 处数 |
|---|---|
| ref/out 参数 | 24 |
| Transpiler/Finalizer/Prepare/TargetMethod | 12 |
| 已有 try | 5 |
| 收尾/还原 | 2 |
| 收尾：先古场景 _ExitTree 后恢复行为范围 | 1 |
| 收尾：换模型前释放旧呈现 | 1 |
| 收尾：对象池回收时释放呈现 | 1 |
| 收尾：清除 tiny icon 的旧卡牌关联 | 1 |
| 收尾：图鉴关闭后恢复/清空范围 | 1 |
| 收尾：_ExitTree 释放 lease/节点 | 1 |
| 收尾：退出选人时复位手指位置 | 1 |
| 收尾：侧回合开始清旧效果 | 1 |

> 本批没有命中「不碰的文件」的补丁类（`SkinService*`、`MultiplayerSkinSync`、`CharacterAppearanceRuntime`、`ManagedSkinModLoader`、`SkinCatalog`、`MerchantRuntimeAppearance`、`Core\Safety`、`Entry` 都不在五个模块的清单里）。
