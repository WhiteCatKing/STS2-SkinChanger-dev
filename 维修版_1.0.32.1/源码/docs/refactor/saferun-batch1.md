# ④ 第 1 批：补丁体包 `Safe.Run`（DSH / 肥鱼，2026-11-14）

状态：**已包裹 39 处、跳过 51 处**；构建 **0 错误**；LogicTests **通过**；未实机、未部署、**未 commit**、未改版本号。

> **复核结果**：GPT 2026-11-14 **通过**（房间 #27，`review-saferun-batch1-gpt.md`）；整改后定向核对确认 8 处收尾外壳已移除、方法与 HEAD 一致、脚本显式跳过规则已同步，自检 39/0/true。

> **复核整改（GPT #25 / `review-saferun-batch1-gpt.md`）**：首版包了 47 处，GPT 抽查发现其中 **8 处是收尾/还原/解绑**（退出场景、图鉴关闭、卡牌复用、回合切换时恢复原状），按任务书「熔断后收尾仍要执行」必须移出包裹。已把这 8 处改为显式跳过，现为包 39 / 跳过 51；8 条清单与理由见文末「复核整改」。

基线：工作区 = 1.0.31.95（`01eca75`）+ Claude 未提交的阶段 A 改动；补丁清单取自 `docs/refactor/patches.csv`（按 `092ec98` 生成，这边一律按**类名**匹配，不受行号漂移影响）。
范围：`patches.csv` 里 `建议模块` ∈ {`card`, `background`, `ancient`, `event`, `monster`} 的 `[HarmonyPatch]` 特性类，共 **73 个类 / 18 个文件**。
任务书「不碰的文件」（`SkinService*`、`MultiplayerSkinSync`、`CharacterAppearanceRuntime`、`ManagedSkinModLoader`、`SkinCatalog`、`MerchantRuntimeAppearance`、`Core\Safety`、`Entry`）**没有命中任何五个模块的补丁类**，所以这一类跳过为 0。

## 怎么改的

脚本 `tools/saferun_wrap.py`（`--dry-run` 出决策、`--apply` 写回），只加壳，补丁逻辑一行不改：

- `void Prefix/Postfix` → `Safe.Run("patch:<模块>", () => { 原方法体 });`
- `bool Prefix` → `return Safe.Get("patch:<模块>", () => { 原方法体 }, true);`（出错返回 `true`，让游戏原方法照常执行）
- 每个被改文件顶部补 `using STS2SkinChanger.Core.Safety;`
- 表达式体方法包成 `=> Safe.Run("patch:<模块>", () => 原表达式);`，签名原样保留
- lambda 里原有的 `return;` 保持不变

自检 `tools/verify_saferun.py`：把 `git show HEAD` 里的原方法体和包裹后 lambda 内层做「去掉全部空白后逐字比较」——

```
逐处比对：39 处
内容不一致：0
bool Prefix 数量：5，fallback 全 true：True
```


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
| `STS2SkinChanger/Ui/CardSkinControls.cs:2607` | CardBuiltInOverlayPatch | Postfix | void | `patch:card` |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2506` | CardInspectSkinDisplayPatch | Postfix | void | `patch:card` |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2499` | CardInspectSkinReadyPatch | Postfix | void | `patch:card` |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2659` | CardLayoutBaselineCapturePatch | Postfix | void | `patch:card` |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2682` | CardLayoutFinalPatch | Postfix | void | `patch:card` |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2672` | CardLayoutFinalPatch | Prefix | void | `patch:card` |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2548` | CardLibraryInitialPortraitPreloadPatch | Postfix | void | `patch:card` |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2535` | CardLibraryPoolSkinPatch | Postfix | void | `patch:card` |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2573` | CardLibraryRowPortraitPreloadPatch | Prefix | void | `patch:card` |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2514` | CardLibrarySkinOpenedPatch | Postfix | void | `patch:card` |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2512` | CardLibrarySkinOpenedPatch | Prefix | void | `patch:card` |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2493` | CardLibrarySkinReadyPatch | Postfix | void | `patch:card` |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2719` | CardPlayQueueRebindPortraitPatch | Postfix | void | `patch:card` |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2600` | CardPreviewModeCapturePatch | Prefix | void | `patch:card` |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2585` | CardRewardPortraitPreloadPatch | Prefix | void | `patch:card` |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2705` | CardTreeReentrySkinPatch | Postfix | void | `patch:card` |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2712` | LocalCardPlayQueuePortraitPatch | Postfix | void | `patch:card` |
| `STS2SkinChanger/Ui/CardSurfaceView.cs:90` | TinyCardSelectedSurfacePatch | Postfix | void | `patch:card` |
| `STS2SkinChanger/Ui/ContextualSkinControls.cs:3909` | BestiarySelectionSkinPatch | Postfix | void | `patch:monster` |
| `STS2SkinChanger/Ui/ContextualSkinControls.cs:3998` | MonsterVisualResultPatch | Prefix | void | `patch:monster` |
| `STS2SkinChanger/Ui/ContextualSkinControls.cs:4190` | SkinPopupBackgroundMutePatch | Prefix | bool | `patch:background` |
| `STS2SkinChanger/Ui/EventCompendiumPreview.cs:293` | EventPreviewLifecyclePatch | Prefix | bool | `patch:event` |
| `STS2SkinChanger/Ui/MonsterSkinPriorityControls.cs:816` | BestiaryInitialSkinNamePatch | Postfix | void | `patch:monster` |
| `STS2SkinChanger/Ui/StatefulCardCosmeticPatches.cs:97` | StatefulCardPlayCosmeticPatch | Prefix | void | `patch:card` |
| `STS2SkinChanger/Ui/StatefulCardCosmeticPatches.cs:124` | StatefulDamageCosmeticPatch | Postfix | void | `patch:card` |
| `STS2SkinChanger/Ui/StatefulCardCosmeticPatches.cs:44` | StatefulRewardGlowPatch | Prefix | bool | `patch:card` |
| `STS2SkinChanger/Ui/StatefulCombatEffects.cs:153` | StatefulExhaustHidePatch | Prefix | bool | `patch:card` |

## 跳过清单（51 处）

| 位置 | 补丁类 | 方法 | 原因 |
|---|---|---|---|
| `STS2SkinChanger/Core/BackgroundSkinService.cs:3821` | BackgroundSkinCreatePatch | Postfix | ref/out 参数 |
| `STS2SkinChanger/Core/EventSkinRuntime.cs:114` | EventSkinResourceAvailabilityPatch | TargetMethods | Transpiler/Finalizer/Prepare/TargetMethod |
| `STS2SkinChanger/Core/EventSkinRuntime.cs:119` | EventSkinResourceAvailabilityPatch | Prefix | ref/out 参数 |
| `STS2SkinChanger/Core/EventSkinRuntime.cs:135` | EventSkinTextPatch | Prefix | ref/out 参数 |
| `STS2SkinChanger/Core/EventSkinRuntime.cs:148` | EventSkinVfxPatch | Prefix | ref/out 参数 |
| `STS2SkinChanger/Core/FrameworkSkinRuntimePatches.cs:275` | FrameworkCardFramePatch | Prefix | ref/out 参数 |
| `STS2SkinChanger/Core/FrameworkSkinRuntimePatches.cs:295` | FrameworkCardEnergyIconPatch | Prefix | ref/out 参数 |
| `STS2SkinChanger/Core/FrameworkSkinRuntimePatches.cs:312` | FrameworkEnergyIconHelperPatch | Prefix | ref/out 参数 |
| `STS2SkinChanger/Core/FrameworkSkinRuntimePatches.cs:423` | FrameworkOrbIconPatch | Prefix | ref/out 参数 |
| `STS2SkinChanger/Core/FrameworkSkinRuntimePatches.cs:441` | FrameworkOrbSpritePatch | Prefix | ref/out 参数 |
| `STS2SkinChanger/Core/FrameworkSkinRuntimePatches.cs:532` | FrameworkRelicIconTexturePatch | Prefix | ref/out 参数 |
| `STS2SkinChanger/Core/FrameworkSkinRuntimePatches.cs:549` | FrameworkRelicOutlineTexturePatch | Prefix | ref/out 参数 |
| `STS2SkinChanger/Core/FrameworkSkinRuntimePatches.cs:566` | FrameworkRelicBigIconTexturePatch | Prefix | ref/out 参数 |
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
| `STS2SkinChanger/Ui/CardSkinControls.cs:2579` | CardRewardPortraitPreloadPatch | TargetMethod | Transpiler/Finalizer/Prepare/TargetMethod |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2592` | CardPortraitResultPatch | Postfix | ref/out 参数 |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2622` | CardLayoutModelRebindPatch | Prefix | 收尾：换模型前释放旧呈现 |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2631` | CardLayoutPoolReleasePatch | TargetMethods | Transpiler/Finalizer/Prepare/TargetMethod |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2638` | CardLayoutPoolReleasePatch | Prefix | 收尾：对象池回收时释放呈现 |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2644` | CardLayoutResetPatch | TargetMethod | Transpiler/Finalizer/Prepare/TargetMethod |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2648` | CardLayoutResetPatch | Prefix | 收尾/还原 |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2655` | CardLayoutBaselineCapturePatch | TargetMethod | Transpiler/Finalizer/Prepare/TargetMethod |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2665` | CardLayoutFinalPatch | TargetMethods | Transpiler/Finalizer/Prepare/TargetMethod |
| `STS2SkinChanger/Ui/CardSurfaceView.cs:97` | TinyCardAnonymousSurfacePatch | Prefix | 收尾：清除 tiny icon 的旧卡牌关联 |
| `STS2SkinChanger/Ui/CardTransformPreload.cs:30` | TransformPreviewPortraitPreloadPatch | Prefix | 已有 try |
| `STS2SkinChanger/Ui/CardTransformPreload.cs:69` | CardTransformVfxPortraitPreloadPatch | Prefix | 已有 try |
| `STS2SkinChanger/Ui/CardTransformPreload.cs:91` | CardTransformShineVfxPortraitPreloadPatch | Prefix | 已有 try |
| `STS2SkinChanger/Ui/CardTransformPreload.cs:124` | CardTransformShineVfxPortraitPreloadPatch | Postfix | 已有 try |
| `STS2SkinChanger/Ui/ContextualSkinControls.cs:3926` | BestiaryRuntimeProviderScopePatch | Postfix | 收尾：图鉴关闭后恢复/清空范围 |
| `STS2SkinChanger/Ui/ContextualSkinControls.cs:4013` | MonsterVisualResultPatch | Postfix | ref/out 参数 |
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


## 复核整改（GPT #25 提出的 8 处）

首版把下面 8 处也包了。它们的方法名/函数体不带"restore/cleanup"字样，但语义是收尾：**功能熔断后这些动作仍必须执行**，被 `Safe` 挡住会残留旧呈现、旧关联或没恢复的范围。现已全部移出包裹（`saferun_wrap.py` 的 `SKIP_OVERRIDES` 写死了这 8 条，重跑不会再把它们包回去）：

| 方法 | 语义 |
| --- | --- |
| `Ui/AncientCompendium.cs` `AncientRuntimeProviderScopeExitPatch.Postfix` | 先古场景 `_ExitTree` 后恢复行为范围 |
| `Ui/ContextualSkinControls.cs` `BestiaryRuntimeProviderScopePatch.Postfix` | 图鉴关闭后恢复/清空范围 |
| `Ui/CardSkinControls.cs` `CardLayoutModelRebindPatch.Prefix` | 换模型前释放旧呈现 |
| `Ui/CardSkinControls.cs` `CardLayoutPoolReleasePatch.Prefix` | 对象池回收时释放呈现 |
| `Ui/StatefulCardArtView.cs` `StatefulCardTreeExitPatch.Prefix` | `_ExitTree` 释放 lease/节点 |
| `Ui/StatefulCardCosmeticPatches.cs` `StatefulCharacterSceneExitPatch.Prefix` | 退出选人时复位手指位置 |
| `Ui/CardSurfaceView.cs` `TinyCardAnonymousSurfacePatch.Prefix` | 清除 tiny icon 的旧卡牌关联 |
| `Ui/StatefulCardCosmeticPatches.cs` `StatefulSideTurnCosmeticPatch.Postfix` | 侧回合开始清旧效果 |

整改后重跑：包裹 39、跳过 51（`ref/out` 24、`Transpiler/Prepare/TargetMethod(s)` 12、已有 try 5、收尾类 10）、改动 13 个文件；构建 0 错误、LogicTests 通过、自检 39/0/true。

## 构建与测试

- 构建（`D:\mod\工作2\STS2-SkinChanger_修改件`，输出到临时目录）：
  `dotnet build STS2SkinChanger/STS2SkinChanger.csproj -c Release -p:GameAssemblyDir="C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64" -o <临时目录>`
  → **0 个错误**，8 个警告。8 个警告的所在行**都不在本次改动的范围内**（`CardTransformPreload.cs:78` 文件本轮没改；`ContextualSkinControls.cs:2119/2247/3329` 在我改的 3800 行之后；`ProviderMenuGateCompat.cs:44`、`VariantHotkeyBridge.cs:50`、`MerchantRuntimeAppearance.cs:908` 都是 HEAD 已有）。
- LogicTests：`dotnet run --project tests/STS2SkinChanger.LogicTests/STS2SkinChanger.LogicTests.csproj -c Release -p:GameAssemblyDir=<同上>` → 末行 `Skin Changer logic policy tests passed.`

## 取舍与遗留

- 熔断 key 按模块：`patch:card` / `patch:background` / `patch:ancient` / `patch:event` / `patch:monster`，用 `CircuitBreaker` 默认策略（60 秒内失败满 5 次即本会话停用该模块补丁）。
- 包住的方法异常不再顺着 Harmony 冒到游戏本体；`bool Prefix` 出错时放行原方法（返回 `true`）。
- **24 处 `ref`/`out` 参数**（多数是 `ref string __result`）没法用 lambda 捕获，按任务书跳过，仍然裸奔——要防崩得另想办法（方法内 try 或改成返回值），不在本批。
- 12 处 `Transpiler`/`Prepare`/`TargetMethod(s)`、5 处已有 try、10 处收尾/还原（含上面 8 处）按任务书跳过。

## 复核提示（给 GPT）

1. 看改动：`git diff -- <文件>`（13 个文件见上面「按文件」表）。
2. 复跑自检：`cd docs\refactor; python tools\verify_saferun.py`（自动对 HEAD 版比对，输出 39/0/true）。
3. 重点确认 8 处收尾已不在包裹里：这 8 个方法在 `git diff` 里应无改动。
4. `bool Prefix` 共 5 处，fallback 都是 `true`。
