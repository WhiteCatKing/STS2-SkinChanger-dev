# 先古、怪物模块迁移（DSH，阶段 C 第 4 批）

基线 `c40292d`（1.0.31.108，分支 `refactor/stageA-C-20261114`）。照事件模块模板做，未改 `FeatureBootstrap.cs`、未动 `Core/Safety`、`SkinService`、`SkinCatalog`、`ManagedSkinModLoader`、`Entry.cs`。

## 先古 `ancient`

- **标了 8 个**（`Ui/AncientCompendium.cs`，`[FeaturePatch("ancient")]` 加在各自 `[HarmonyPatch]` 前，`:3262` 起）：
  `ManagedAncientSceneAnimationPatch`、`ManagedAncientStaticBackgroundWindowPatch`、`AncientSceneResultPatch`、`AncientRuntimeProviderScopeExitPatch`、`AncientMapIconResultPatch`、`AncientMapIconOutlineResultPatch`、`AncientRunHistoryIconResultPatch`、`AncientRunHistoryIconOutlineResultPatch`。
- `AncientRuntimeProviderScopeExitPatch` **也标了**：它做的是离开先古场景时把行为范围收回对局角色。模块只在"先古房间和预览都不在用"时才会被卸，所以它该执行时一定在位；反过来摘掉它才会留下没人回收的范围。
- `ShouldBeActive`：先古分组 = `SkinCatalog.KnownAncientIds`（`Catalog/SkinCatalog.cs:9176`，8 个先古 id 就是分组 id），组内有非空且不等于 `__base__` 的选项就 true；目录没就绪返回 null。
- `IsInUse`：`NEventRoom.Instance` 在场景树里，或先古图鉴预览还开着。预览状态用新增的弱引用 `AncientCompendiumScreen.HasLivePreview`（`Ui/AncientCompendium.cs:552`，在 `_otherPreviewInstance` 赋值处 `:1947`、`:2360` 记录），不遍历树。
- `OnActivated` / `OnDeactivating` 留空：模块只在目录就绪、先古包增减时变化，在用时注册表不会卸；已经打开的先古事件不重建（重新 `SetEvent` 会重置选项和奖励）。
- Safe key：现有两处已是 `patch:ancient`，保持；另外 6 个类原本没包，按要求不新包。

## 怪物 `monster`

- **只标 `MonsterVisualResultPatch`**（`Ui/ContextualSkinControls.cs:4000`，目标 `MonsterModel.CreateVisuals`），key 沿用 `patch:monster`。
- **留主干、不标**：`BestiaryEntranceReleasePatch`、`BestiaryEntranceForceClickPatch`、`BestiaryEntrancePushPatch`、`BestiaryEntranceReadyPatch`、`BestiaryEntranceOpenedPatch`、`BestiarySelectionSkinPatch`、`BestiaryRuntimeProviderScopePatch`、`BestiaryInitialSkinNamePatch`、`DecimillipedeBestiarySetupCleanupPatch`（图鉴入口、下拉、范围，都是选择入口）。
- `ShouldBeActive`：怪物组按三个信号任一命中来认，组里有非 `__base__` 选项才激活；排除 `merchant`、`fake_merchant_monster`（归商人模块）。
  1. **`Config.MonsterSkinCategoryGroups.Values` 的已登记分组快照**——普通 PCK 骨骼/贴图皮肤走这一条（它们的 `ManagedMonsterScene` / `RuntimeMonsterVisualMode` 都是 null，见 `SkinCatalog.cs:5972–5980`）；
  2. `ManagedMonsterScene` 或 `RuntimeMonsterVisualMode` 非空（DLL 型皮肤）；
  3. 选项资源路径以 `res://animations/monsters/` 开头——目录自己就是用 `MonsterPathRegex`（`SkinCatalog.cs:8774–8778`）这样认组的，加这一条是因为①的快照要等分类登记后才有内容，全新安装时可能是空的，只看①会在"只装普通 PCK 怪物皮肤"时误判 false。
- `IsInUse`：`NRun.Instance` 在树里（保守：局内随时会创建怪物），或怪物图鉴开着。图鉴状态用弱引用 `ContextualSkinControls.HasOpenBestiary`（`:34`，在 `EnsureMonsterSelector`（`:598`）建立下拉时记录），不遍历树。

## 要 Claude 加的两行（`Core/Features/FeatureBootstrap.cs` 的 `CreateFeatures()`）

```csharp
yield return new Ancient.AncientFeature();
yield return new Monster.MonsterFeature();
```

## 改动文件

- 新建：`Core/Features/Ancient/AncientFeature.cs`、`AncientFeaturePolicy.cs`、`Core/Features/Monster/MonsterFeature.cs`、`MonsterFeaturePolicy.cs`
- 修改：`Ui/AncientCompendium.cs`（8 个特性 + 预览弱引用）、`Ui/ContextualSkinControls.cs`（1 个特性 + 图鉴弱引用）
- 测试（只追加）：`tests/.../AncientFeaturePolicyTests.cs`、`MonsterFeaturePolicyTests.cs` 新建；`Program.cs` 加两行 `Run()`；`csproj` 加两条 `Compile Include`

## 验证

- 构建 **0 错误**；9 个警告全是既有的（`SkinCatalog:6281`、`ProviderMenuGateCompat:44`、`VariantHotkeyBridge:50`、`CardTransformPreload:81`、`ContextualSkinControls:2127/2255/3337`、`MerchantRuntimeAppearance:1004`），新代码无新警告。
- LogicTests 全绿，新增输出 `Ancient feature policy tests passed.` 与 `Monster feature policy tests passed.`，末行 `Skin Changer logic policy tests passed.`
- 回归断言（GPT #60）：已登记的普通 PCK 怪物分组、两项运行时元数据都为 null 时仍然激活；另有"运行时元数据命中即激活""商人/假商人即使登记过也不触发""角色分组不触发"。
- 未实机、未 commit、未切分支、未 checkout 任何文件。

## 可选、不阻塞

- `HasOpenBestiary` 只在怪物下拉建立时记录；若图鉴打开但下拉没建立，只剩 `NRun.Instance` 判断（模块只在怪物包增减时变化，影响面很小）。
- 先古预览只记录最近一个"其它预览"实例，同屏反复切换时旧实例的弱引用会被覆盖，属预期。
