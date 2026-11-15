# 角色模块迁移（DSH，阶段 C 第 5 批，第 1、2 步）

基线 `cd62cce`（1.0.31.109）。照怪物模块模板做，只做勘察第 5 步的第 1、2 步；随从、共享类、手动安装桥未动。

## 标了哪 16 个类（各加一行 `[FeaturePatch("character")]`）

| 文件 | 类 | Safe key 变化 |
| --- | --- | --- |
| `Core/FrameworkSkinRuntimePatches.cs` | `FrameworkCombatVisualPatch`、`FrameworkCharacterTrailPatch`、`FrameworkCharacterEnergyColorPatch`、`FrameworkEnergyLayersPatch`、`FrameworkEntryAnimationPatch` | `FrameworkEnergyLayersPatch` 原来是 `patch:core`，改成 `patch:character`；其余原样 |
| `Core/ManagedCharacterAnimationBridge.cs` | `ManagedCharacterAnimationReadyPatch`、`ManagedCharacterAnimationTriggerPatch` | 原来就是 `patch:character` |
| `Core/OptionalCharacterVfx.cs` | `NecrobinderOptionalHeadReadyPatch`、`NecrobinderOptionalHeadAnimationPatch` | 后者原来就是 `patch:character` |
| `Ui/ContextualSkinControls.cs` | `CharacterVisualResultPatch`、`CharacterSelectIconResultPatch`、`CharacterSelectLockedIconResultPatch`、`CharacterIconResultPatch`、`CharacterIconTextureResultPatch`、`CharacterIconOutlineTextureResultPatch`、`CharacterMapMarkerResultPatch` | 这 7 个原本没包 `Safe.Run`，按要求**没有新包** |

- 16 个类都是独立的 `[HarmonyPatch]` 类，没有一个是"由别处手动安装"的，所以清单没有缺口。
- 清单外的类一律没标：`MonsterVisualScalePatch`（key 是 character 但服务怪物）、`CharacterAppearance*`、`CharacterSkinBundle*`、`*Embark*`、`*Lobby*`、`ProviderMerchant/RestSiteVariantPathPatch`、`FrameworkCardEnergyColorPatch`、`FrameworkOrbDarkenedColorPatch`、`FrameworkRelic*`，以及所有手动安装桥。

## ShouldBeActive

`SkinCatalog.IsCharacterAppearanceGroup`（`Catalog/SkinCatalog.cs:2342`）或 `IsCompanionGroup`（`:33`）为真的分组里，只要有一个选项 id 不等于 `SkinCatalog.BaseOptionId` 就启用；`SkinService.Catalog == null` 返回 null；其余返回 false。只看目录、不看当前选择（勘察 4.1：按选择判断会被 2 秒轮询拖慢，切皮肤那一刻补丁可能还没装上）。

## IsInUse

**直接返回 true（保守）**。局内角色/随从外观随时重建，本来就在用；选人界面没有现成的"开着"信号——`NCharacterSelectScreen` 没有单例（`CharSelectStageCycle`、`ContextualSkinControls` 里也没有记录当前界面的字段，全仓库 0 处 `CurrentScreen`/`ActiveSubmenu`），遍历场景树又被任务书禁掉。所以不新加跟踪，一律当作在用。副作用：模块装上后本会话不会因为"不需要"被摘（只在没有角色/随从皮肤时才"不需要"，此时摘不摘都不影响外观）。

## 要 Claude 加的一行（`Core/Features/FeatureBootstrap.cs` 的 `CreateFeatures()`）

```csharp
yield return new Character.CharacterFeature();
```

## 改动文件

- 新建：`Core/Features/Character/CharacterFeature.cs`、`CharacterFeaturePolicy.cs`
- 修改：上面 4 个源码文件（只加特性一行 + 1 处 key）
- 测试（只追加）：`tests/.../CharacterFeaturePolicyTests.cs` 新建；`Program.cs` 加一行 `Run()`；`csproj` 加一条 `Compile Include`
- 登记表已登记，完工后已删

## 验证

- 构建 **0 错误**，9 个警告全是既有的，新代码无新警告。
- LogicTests 全绿，新增输出 `Character feature policy tests passed.`，末行 `Skin Changer logic policy tests passed.`
- 策略测试 6 条：目录未就绪 / 空目录 / 角色组只有原版 / 角色组有皮肤 / 只有随从组有皮肤 → true / 非角色非随从不触发。
- 未实机、未 commit、未切分支、未 checkout。

## 可选、不阻塞

- `IsInUse` 恒为 true，等于角色模块装上后整局不摘；等以后有便宜的"选人界面开着"信号（比如给 `NCharacterSelectScreen` 的 `OnSubmenuOpened/Closed` 记一个弱引用）再收紧。
- 勘察第 3–5 步（共享归属、随从、手动安装桥）按要求没做。
