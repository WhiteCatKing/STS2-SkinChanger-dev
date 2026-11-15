# ④ 第 2 批：补丁体包 `Safe.Run`（character / core / osty / workshop / compat）（DSH，2026-11-14）

状态：**已包裹 20 处、跳过 71 处**；构建 **0 错误**；LogicTests **通过**；未实机、未部署、**未 commit**、未改版本号。

做法、规则、交付格式全部照第 1 批（`saferun-batch1.md`），脚本用同一份 `tools/saferun_wrap.py --batch 2`：
`void Prefix/Postfix` → `Safe.Run("patch:<模块>", () => { 原方法体 })`；`bool Prefix` → `return Safe.Get(…, true)`；顶部补 `using STS2SkinChanger.Core.Safety;`。GPT 复核第 1 批时指出的「收尾/还原/解绑/释放/ExitTree/对象池/回合清理一律不包」照做，8 条 `SKIP_OVERRIDES` 继续生效。

## 范围与三个口径说明

- 任务书范围是 `[HarmonyPatch]` **特性补丁类**。`compat:*`（EmperorCrab / PengoTarotFont / OstyDefectImpostor / CustomSkeletonLoader）在 `patches.csv` 里**全是手动安装点**（`harmony.Patch(...)`，共 10 条，`方式=手动`），没有特性类 → 本批 **0 处**，key `patch:compat` 没被用上。
- `osty` 只有 1 个特性类（`OstyDisplayNameGuardPatch`），签名带 `ref __result` → 按 ref/out 跳过，本批 osty **0 处**。
- **启动期补丁一律跳过**：本批命中 4 处——`NativeFrameworkAssemblyLoadPatch`（`AssemblyLoadContext.LoadFromAssemblyPath`）、`LegacyManifestReadCompatibilityPatch` 的 Prefix 与 Finalizer（`ModManifest.ReadFromStream`）、`ProviderSettingsConsolePatch`（`DevConsole` 构造）。另外 `Entry.cs` / `ManagedSkinModLoader.cs` 里还有 7 处启动期补丁（`OneTimeInitialization`、`ModManager.TryLoadMod`、`ModModel.Init`、`ModManager.GetModdedLocTables` 等），因为这两个文件在「不碰」清单里，标成「不碰的文件（启动期补丁）」。
- **不碰的文件**：4 个文件共 30 处（`CharacterAppearanceRuntime.cs` 12、`ManagedSkinModLoader.cs` 9、`SkinService.CharacterSkinBundleRuns.cs` 5、`Entry.cs` 4）。`Core/Features/` 本批没有命中。

## 自检

`tools/verify_saferun.py --batch 2`（对 `git show HEAD` 的原方法体去空白逐字比对）：

```
逐处比对：20 处
内容不一致：0
bool Prefix 数量：2，fallback 全 true：True
```


## 改了什么

共 **20** 处，涉及 **14** 个文件、18 个补丁类。
包裹方式：`void Prefix/Postfix` → `Safe.Run(key, () => { 原方法体 })`；`bool Prefix` → `return Safe.Get(key, () => { 原方法体 }, true)`（出错放行原方法）。

| 模块 key | 处数 |
|---|---|
| `patch:character` | 9 |
| `patch:core` | 10 |
| `patch:workshop` | 1 |

按文件：

| 文件 | 处数 |
|---|---|
| `STS2SkinChanger/Ui/ContextualSkinControls.cs` | 3 |
| `STS2SkinChanger/Ui/LoadOrderWarning.cs` | 3 |
| `STS2SkinChanger/Core/ManagedCharacterAnimationBridge.cs` | 2 |
| `STS2SkinChanger/Core/QuitTimingDiagnostics.cs` | 2 |
| `STS2SkinChanger/Core/FrameworkSkinRuntimePatches.cs` | 1 |
| `STS2SkinChanger/Core/OptionalCharacterVfx.cs` | 1 |
| `STS2SkinChanger/Core/SkinWorkshopService.cs` | 1 |
| `STS2SkinChanger/Ui/CharSelectStageCycle.cs` | 1 |
| `STS2SkinChanger/Ui/CharacterAppearanceScreen.cs` | 1 |
| `STS2SkinChanger/Ui/CreatureIntentDepth.cs` | 1 |
| `STS2SkinChanger/Ui/ManagedModListNamePatch.cs` | 1 |
| `STS2SkinChanger/Ui/ModThemeEditor.cs` | 1 |
| `STS2SkinChanger/Ui/PauseMenuEntryToggles.cs` | 1 |
| `STS2SkinChanger/Ui/StatefulCombatEffects.cs` | 1 |

### 明细

| 位置 | 补丁类 | 方法 | 返回 | key |
|---|---|---|---|---|
| `STS2SkinChanger/Core/FrameworkSkinRuntimePatches.cs:398` | FrameworkEnergyLayersPatch | Postfix | void | `patch:core` |
| `STS2SkinChanger/Core/ManagedCharacterAnimationBridge.cs:225` | ManagedCharacterAnimationReadyPatch | Postfix | void | `patch:character` |
| `STS2SkinChanger/Core/ManagedCharacterAnimationBridge.cs:234` | ManagedCharacterAnimationTriggerPatch | Postfix | void | `patch:character` |
| `STS2SkinChanger/Core/OptionalCharacterVfx.cs:35` | NecrobinderOptionalHeadAnimationPatch | Prefix | bool | `patch:character` |
| `STS2SkinChanger/Core/QuitTimingDiagnostics.cs:25` | QuitTimingDiagnostics | Postfix | void | `patch:core` |
| `STS2SkinChanger/Core/QuitTimingDiagnostics.cs:15` | QuitTimingDiagnostics | Prefix | void | `patch:core` |
| `STS2SkinChanger/Core/SkinWorkshopService.cs:314` | WorkshopRuntimeNoticePatch | Prefix | bool | `patch:workshop` |
| `STS2SkinChanger/Ui/CharSelectStageCycle.cs:731` | CharSelectStageCyclePatch | Postfix | void | `patch:character` |
| `STS2SkinChanger/Ui/CharacterAppearanceScreen.cs:2811` | CharacterAppearancePauseMenuPatch | Postfix | void | `patch:character` |
| `STS2SkinChanger/Ui/ContextualSkinControls.cs:3822` | CharacterSelectionSkinPatch | Postfix | void | `patch:character` |
| `STS2SkinChanger/Ui/ContextualSkinControls.cs:3819` | CharacterSelectionSkinPatch | Prefix | void | `patch:character` |
| `STS2SkinChanger/Ui/ContextualSkinControls.cs:4044` | MonsterVisualScalePatch | Postfix | void | `patch:character` |
| `STS2SkinChanger/Ui/CreatureIntentDepth.cs:156` | CreatureIntentDepthPatch | Postfix | void | `patch:character` |
| `STS2SkinChanger/Ui/LoadOrderWarning.cs:419` | LoadOrderWarningModalClosedPatch | Postfix | void | `patch:core` |
| `STS2SkinChanger/Ui/LoadOrderWarning.cs:407` | LoadOrderWarningModalReadyPatch | Postfix | void | `patch:core` |
| `STS2SkinChanger/Ui/LoadOrderWarning.cs:413` | MainMenuLoadOrderWarningPatch | Postfix | void | `patch:core` |
| `STS2SkinChanger/Ui/ManagedModListNamePatch.cs:14` | ManagedModListNamePatch | Postfix | void | `patch:core` |
| `STS2SkinChanger/Ui/ModThemeEditor.cs:396` | ModThemeEditorReadyPatch | Postfix | void | `patch:core` |
| `STS2SkinChanger/Ui/PauseMenuEntryToggles.cs:320` | PauseMenuEntryTogglesPatch | Postfix | void | `patch:core` |
| `STS2SkinChanger/Ui/StatefulCombatEffects.cs:143` | StatefulExhaustInitializePatch | Postfix | void | `patch:core` |

## 跳过清单（71 处）

| 位置 | 补丁类 | 方法 | 原因 |
|---|---|---|---|
| `STS2SkinChanger/Core/CombatHudResourceLifecycle.cs:92` | CombatUiExitCleanupPatch | Finalizer | Transpiler/Finalizer/Prepare/TargetMethod |
| `STS2SkinChanger/Core/CompanionDisplayNameGuard.cs:138` | OstyDisplayNameGuardPatch | Postfix | ref/out 参数 |
| `STS2SkinChanger/Core/FrameworkSkinRuntimePatches.cs:193` | FrameworkCombatVisualPatch | Prefix | ref/out 参数 |
| `STS2SkinChanger/Core/FrameworkSkinRuntimePatches.cs:259` | FrameworkCharacterTrailPatch | Prefix | ref/out 参数 |
| `STS2SkinChanger/Core/FrameworkSkinRuntimePatches.cs:331` | FrameworkCharacterEnergyColorPatch | TargetMethods | Transpiler/Finalizer/Prepare/TargetMethod |
| `STS2SkinChanger/Core/FrameworkSkinRuntimePatches.cs:341` | FrameworkCharacterEnergyColorPatch | Prefix | ref/out 参数 |
| `STS2SkinChanger/Core/FrameworkSkinRuntimePatches.cs:359` | FrameworkCardEnergyColorPatch | TargetMethods | Transpiler/Finalizer/Prepare/TargetMethod |
| `STS2SkinChanger/Core/FrameworkSkinRuntimePatches.cs:369` | FrameworkCardEnergyColorPatch | Prefix | ref/out 参数 |
| `STS2SkinChanger/Core/FrameworkSkinRuntimePatches.cs:462` | FrameworkOrbDarkenedColorPatch | TargetMethods | Transpiler/Finalizer/Prepare/TargetMethod |
| `STS2SkinChanger/Core/FrameworkSkinRuntimePatches.cs:470` | FrameworkOrbDarkenedColorPatch | Prefix | ref/out 参数 |
| `STS2SkinChanger/Core/FrameworkSkinRuntimePatches.cs:487` | FrameworkRelicPackedIconPatch | TargetMethods | Transpiler/Finalizer/Prepare/TargetMethod |
| `STS2SkinChanger/Core/FrameworkSkinRuntimePatches.cs:491` | FrameworkRelicPackedIconPatch | Prefix | ref/out 参数 |
| `STS2SkinChanger/Core/FrameworkSkinRuntimePatches.cs:520` | FrameworkRelicOutlinePatch | TargetMethods | Transpiler/Finalizer/Prepare/TargetMethod |
| `STS2SkinChanger/Core/FrameworkSkinRuntimePatches.cs:524` | FrameworkRelicOutlinePatch | Prefix | ref/out 参数 |
| `STS2SkinChanger/Core/FrameworkSkinRuntimePatches.cs:614` | FrameworkEntryAnimationPatch | TargetMethods | Transpiler/Finalizer/Prepare/TargetMethod |
| `STS2SkinChanger/Core/FrameworkSkinRuntimePatches.cs:637` | FrameworkEntryAnimationPatch | Postfix | 已有 try |
| `STS2SkinChanger/Core/ManagedSkinModLoader.cs:4858` | GodotScriptPathRegistrationCompatibilityPatch | Prepare | 不碰的文件 |
| `STS2SkinChanger/Core/ManagedSkinModLoader.cs:4860` | GodotScriptPathRegistrationCompatibilityPatch | TargetMethod | 不碰的文件 |
| `STS2SkinChanger/Core/ManagedSkinModLoader.cs:4864` | GodotScriptPathRegistrationCompatibilityPatch | Prefix | 不碰的文件 |
| `STS2SkinChanger/Core/ManagedSkinModLoader.cs:4904` | GodotScriptRegistrationSessionGuard | Prepare | 不碰的文件 |
| `STS2SkinChanger/Core/ManagedSkinModLoader.cs:4906` | GodotScriptRegistrationSessionGuard | TargetMethod | 不碰的文件 |
| `STS2SkinChanger/Core/ManagedSkinModLoader.cs:4910` | GodotScriptRegistrationSessionGuard | Finalizer | 不碰的文件 |
| `STS2SkinChanger/Core/ManagedSkinModLoader.cs:4942` | ManagedSkinModLoadPatch | TargetMethod | 不碰的文件（启动期补丁） |
| `STS2SkinChanger/Core/ManagedSkinModLoader.cs:4945` | ManagedSkinModLoadPatch | Prefix | 不碰的文件（启动期补丁） |
| `STS2SkinChanger/Core/ManagedSkinModLoader.cs:4947` | ManagedSkinModLoadPatch | Postfix | 不碰的文件（启动期补丁） |
| `STS2SkinChanger/Core/NativeFrameworkAssemblyLoader.cs:105` | NativeFrameworkAssemblyLoadPatch | Prefix | 启动期 |
| `STS2SkinChanger/Core/OptionalCharacterVfx.cs:12` | NecrobinderOptionalHeadReadyPatch | Transpiler | Transpiler/Finalizer/Prepare/TargetMethod |
| `STS2SkinChanger/Core/ProviderManifestCompatibility.cs:112` | LegacyManifestReadCompatibilityPatch | Prefix | 启动期 |
| `STS2SkinChanger/Core/ProviderManifestCompatibility.cs:118` | LegacyManifestReadCompatibilityPatch | Finalizer | 启动期 |
| `STS2SkinChanger/Core/ProviderSettingsControls.cs:52` | ProviderSettingsConsolePatch | Postfix | 启动期 |
| `STS2SkinChanger/Core/QuitCacheCleanup.cs:23` | QuitCacheCleanup | Prefix | 收尾/还原 |
| `STS2SkinChanger/Core/SkinService.CharacterSkinBundleRuns.cs:176` | CharacterSkinBundleNewRunBindingPatch | TargetMethods | 不碰的文件 |
| `STS2SkinChanger/Core/SkinService.CharacterSkinBundleRuns.cs:179` | CharacterSkinBundleNewRunBindingPatch | Postfix | 不碰的文件 |
| `STS2SkinChanger/Core/SkinService.CharacterSkinBundleRuns.cs:189` | CharacterSkinBundleSavedRunPatch | TargetMethods | 不碰的文件 |
| `STS2SkinChanger/Core/SkinService.CharacterSkinBundleRuns.cs:193` | CharacterSkinBundleSavedRunPatch | Prefix | 不碰的文件 |
| `STS2SkinChanger/Core/SkinService.CharacterSkinBundleRuns.cs:209` | CharacterSkinBundleRunSavePatch | Prefix | 不碰的文件 |
| `STS2SkinChanger/Core/SkinWorkshopService.cs:312` | WorkshopRuntimeNoticePatch | TargetMethod | Transpiler/Finalizer/Prepare/TargetMethod |
| `STS2SkinChanger/Core/SkinWorkshopService.cs:313` | WorkshopRuntimeNoticePatch | Prepare | Transpiler/Finalizer/Prepare/TargetMethod |
| `STS2SkinChanger/Entry.cs:218` | CosmeticLocalizationOwnershipPatch | Postfix | 不碰的文件（启动期补丁） |
| `STS2SkinChanger/Entry.cs:392` | DuplicateModelInitCompatibilityPatch | TargetMethod | 不碰的文件（启动期补丁） |
| `STS2SkinChanger/Entry.cs:399` | DuplicateModelInitCompatibilityPatch | Prefix | 不碰的文件（启动期补丁） |
| `STS2SkinChanger/Entry.cs:422` | DuplicateModelInitCompatibilityPatch | Postfix | 不碰的文件（启动期补丁） |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:2590` | InRunCharacterAppearanceRuntimePatch | Prefix | 不碰的文件 |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:2593` | InRunCharacterAppearanceRuntimePatch | Postfix | 不碰的文件 |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:2614` | TopBarRoomIconSkinTexturePatch | Transpiler | 不碰的文件 |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:2636` | BossMapPointSkinPresentationPatch | Postfix | 不碰的文件 |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:2644` | CharacterAppearanceCreatureReadyPatch | Postfix | 不碰的文件 |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:2652` | CharacterAppearancePlayerLayoutPatch | Prefix | 不碰的文件 |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:2661` | CombatRoomRuntimeProviderScopePatch | Prefix | 不碰的文件 |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:2670` | CombatRoomRuntimeProviderScopeExitPatch | Postfix | 不碰的文件 |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:2676` | CharacterAppearanceBoundsPatch | TargetMethod | 不碰的文件 |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:2682` | CharacterAppearanceBoundsPatch | Postfix | 不碰的文件 |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:2689` | CharacterAppearanceOrbPositionPatch | TargetMethod | 不碰的文件 |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:2693` | CharacterAppearanceOrbPositionPatch | Postfix | 不碰的文件 |
| `STS2SkinChanger/Ui/CharacterAppearanceScreen.cs:2804` | CharacterAppearancePauseMenuPatch | TargetMethods | Transpiler/Finalizer/Prepare/TargetMethod |
| `STS2SkinChanger/Ui/ContextualSkinControls.cs:3805` | RandomCharacterLobbySkinPatch | Prefix | 已有 try |
| `STS2SkinChanger/Ui/ContextualSkinControls.cs:3828` | SingleplayerEmbarkSkinSelectorPatch | Prefix | 已有 try（catch 外尾部还有 HideCharacterSelector 收尾） |
| `STS2SkinChanger/Ui/ContextualSkinControls.cs:3858` | MultiplayerEmbarkSkinSelectorPatch | Prefix | 已有 try（catch 外尾部还有 HideCharacterSelector 收尾） |
| `STS2SkinChanger/Ui/ContextualSkinControls.cs:3889` | CharacterSkinBundleRunCleanupPatch | Prefix | 收尾/还原 |
| `STS2SkinChanger/Ui/ContextualSkinControls.cs:3944` | CharacterVisualResultPatch | Postfix | ref/out 参数 |
| `STS2SkinChanger/Ui/ContextualSkinControls.cs:4051` | CachedSceneResultPatch | Postfix | ref/out 参数 |
| `STS2SkinChanger/Ui/ContextualSkinControls.cs:4059` | CachedTextureResultPatch | Postfix | ref/out 参数 |
| `STS2SkinChanger/Ui/ContextualSkinControls.cs:4067` | CharacterSelectIconResultPatch | Postfix | ref/out 参数 |
| `STS2SkinChanger/Ui/ContextualSkinControls.cs:4075` | CharacterSelectLockedIconResultPatch | Postfix | ref/out 参数 |
| `STS2SkinChanger/Ui/ContextualSkinControls.cs:4083` | CharacterIconResultPatch | Postfix | ref/out 参数 |
| `STS2SkinChanger/Ui/ContextualSkinControls.cs:4091` | CharacterIconTextureResultPatch | Postfix | ref/out 参数 |
| `STS2SkinChanger/Ui/ContextualSkinControls.cs:4103` | CharacterIconOutlineTextureResultPatch | Postfix | ref/out 参数 |
| `STS2SkinChanger/Ui/ContextualSkinControls.cs:4115` | CharacterMapMarkerResultPatch | Postfix | ref/out 参数 |
| `STS2SkinChanger/Ui/PauseMenuEntryToggles.cs:313` | PauseMenuEntryTogglesPatch | TargetMethods | Transpiler/Finalizer/Prepare/TargetMethod |
| `STS2SkinChanger/Ui/ProviderVariantPathPatch.cs:16` | ProviderMerchantVariantPathPatch | Prefix | ref/out 参数 |
| `STS2SkinChanger/Ui/ProviderVariantPathPatch.cs:29` | ProviderRestSiteVariantPathPatch | Prefix | ref/out 参数 |

### 跳过原因统计

| 原因 | 处数 |
|---|---|
| 不碰的文件 | 23 |
| ref/out 参数 | 19 |
| Transpiler/Finalizer/Prepare/TargetMethod | 12 |
| 不碰的文件（启动期补丁） | 7 |
| 已有 try | 4 |
| 启动期 | 4 |
| 收尾/还原 | 2 |

> 口径：两条「启程」Prefix（`SingleplayerEmbarkSkinSelectorPatch`、`MultiplayerEmbarkSkinSelectorPatch`）的业务体已有 try/catch，catch 之外尾部还调 `HideCharacterSelector` 收起选择器，属于收尾，故不包。`LoadOrderWarningModalClosedPatch` 虽名含 Closed，函数体是 `TryShow`（弹窗清空后继续处理待展示提示），没有截住原生 Clear，故照常包裹。


## 跳过原因汇总

| 原因 | 处数 |
| --- | --- |
| 不碰的文件 | 23 |
| ref/out 参数 | 19 |
| Transpiler/Finalizer/Prepare/TargetMethod | 12 |
| 不碰的文件（启动期补丁） | 7 |
| 已有 try | 4 |
| 启动期 | 4 |
| 收尾/还原 | 2 |

## 构建与测试

- 构建（`D:\mod\工作2\STS2-SkinChanger_修改件`，输出到临时目录）：**0 个错误**，8 个警告（与第 1 批同一批警告，都在未改动行上）。
- LogicTests：末行 `Skin Changer logic policy tests passed.`
- 第 1 批回归：脚本改了解析器（表达式体里含多行 lambda 的截断问题），重跑 `--batch 1` 仍是 **包 39 / 跳过 51**，`verify_saferun.py --batch 1` 仍 39/0/true，无回归。

## 取舍与遗留

- key：`patch:core` 10、`patch:character` 9、`patch:workshop` 1。
- `core` 的 10 处包裹里没有任何启动期补丁（`QuitTimingDiagnostics` 挂在 `NGame.Quit`，不是启动流程；`LoadOrderWarning` / `ModThemeEditor` / `ManagedModListName` / `PauseMenuEntryToggles` 都是菜单 UI）。
- **19 处 ref/out**（多数 `ref string __result`）仍裸奔，和第 1 批同样的遗留。
- 12 处 Transpiler/Prepare/TargetMethod(s)、4 处已有 try、2 处收尾（`QuitCacheCleanup.Prefix`、`CharacterSkinBundleRunCleanupPatch.Prefix`）按任务书跳过。

## 复核提示（给 GPT）

1. `core` 的 10 处包裹：`FrameworkEnergyLayersPatch.Postfix`、`QuitTimingDiagnostics.Prefix/Postfix`、`WorkshopRuntimeNoticePatch.Prefix`（workshop）、`MonsterVisualScalePatch.Postfix`、`LoadOrderWarningModalReadyPatch.Postfix`、`MainMenuLoadOrderWarningPatch.Postfix`、`LoadOrderWarningModalClosedPatch.Postfix`、`ManagedModListNamePatch.Postfix`、`ModThemeEditorReadyPatch.Postfix`、`PauseMenuEntryTogglesPatch.Postfix`、`StatefulExhaustInitializePatch.Postfix`——都不是启动期（不碰 `OneTimeInitialization` / 装载 / 清单 / `DevConsole` / `NGame._Ready`）。
2. 复跑：`cd docs\refactor; python tools\verify_saferun.py --batch 2`。
3. 跳过清单 71 条，重点看「不碰的文件」30 条里的文件边界，和「启动期」4 条。
