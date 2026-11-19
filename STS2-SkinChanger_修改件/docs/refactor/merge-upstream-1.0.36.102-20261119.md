# 合并上游 fix/1.0.36-stability 第二轮（1.0.36.102 → 1.0.37.0）

日期：2026-11-19
合并提交：`e707667`，parents `aff2d38c`（本地 1.0.37.0）+ `938899e`（维护者 1.0.36.102）
分支：`main`（本地），未 push、未发工坊

## 改了什么

本次只做合并与版本号统一，没有修改上游代码逻辑：

- `git merge --no-commit --no-ff origin/fix/1.0.36-stability`（上游在本轮新增 `938899e`），合计 28 个文件、+4028 / −121。
- 四处版本串冲突全部按本地版本号解决，保留 `1.0.37.0` / `1.0.37`：
  - `STS2SkinChanger/Entry.cs`（`InternalTestVersion`）
  - `STS2SkinChanger/STS2SkinChanger.csproj`（`Version` / `AssemblyVersion` / `FileVersion` / `InformationalVersion`）
  - `STS2SkinChanger/SkinChanger.json`（`version` / `internal_test_version`）
  - `workshop/workshop.json`（`version` / `internalTestVersion`；`featureUpdate` 与 `changeNote` 仍为 1.0.36 文案，留发布时统一改）
- 上游 `938899e` 引入的六类修复原样并入：
  - `Core/FoolWaifuRuntimeCompat.cs`：未选中 Fool_Waifu 时拦下它把切换器停掉的补丁装回来，修愚者皮肤头像串包与商店 Hilde 缩小。
  - `Catalog/SkinCatalog.CharacterDialogue.cs` + `Core/HildeAncientDialogueCompat.cs`：角色专属先古对话随皮肤选择，Hilde 的 `AncientDialogueSet` 只在选中时生成。
  - `Core/Features/Merchant/MerchantRuntimeAppearance.cs`：第三方 `_Ready` 前缀改为入树后重放、只跑一次，修商人热切换后点不动。
  - `Core/ManagedSkinModLoader.cs`：纯商人提供者不得改写玩家 `NMerchantCharacter` 形象，保留 `NMerchantButton` / `NMerchantHand`。
  - `Ui/CharacterSelectModeBackgroundPatch.cs` + `Ui/ProviderModeSelectOverlayBridge.cs`：恐惧 / 极速模式选人背景重做，叠加层在新背景入树后重挂。
  - `Ui/RelicSkinResources.cs`、`Ui/TimelineEpochSkinResources.cs`、`Core/CznRelicCollectionCompat.cs`、`Ui/InteractiveTouchBoxBridge.cs`：AncientWaifus 遗物 / 时间线 / 触摸立绘支持。
  - 新增 5 个测试：`FoolWaifuIdentityTests`、`CharacterDialogueLocalizationTests`、`CharacterSelectModeBackgroundTests`、`MerchantPlayerCharacterIsolationTests`、`AncientWaifusSupportTests`。

## 为什么这么解冲突

四个冲突文件里只有版本号行不同，逻辑完全一致；本地已经把版本号统一到 1.0.37.0（上一轮 `f0a03ba` 决定），因此冲突一律取本地值，避免把上游的 `1.0.36.102` 写回清单造成「同一版本两个号」。

## 怎么验证

1. `dotnet build STS2SkinChanger/STS2SkinChanger.csproj -c Release -p:GameAssemblyDir='C:\sts2refs\stable'` → 0 警告 0 错误，`bin/Release/Gurio.SkinChanger.dll` 3,651,072 B。
2. `dotnet build STS2SkinChanger/STS2SkinChanger.csproj -c ReleaseBeta -p:GameAssemblyDir='D:\steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64'` → 0 警告 0 错误，`bin/ReleaseBeta/Gurio.SkinChanger.dll` 3,851,264 B，`FileVersion 1.0.37.0`、`ProductVersion 1.0.37.0+aff2d38c…`。
3. LogicTests → 末行 `Skin Changer logic policy tests passed.`。
4. 完整无参数 RuntimeTests（先 `SetErrorMode(3)`）→ 退出码 0，末行 `Workshop policy tests passed.`。
5. 特征补丁类计数：合并前 `aff2d38` = 89、上游 `938899e` = 89、合并后 = 89，无丢失。
6. 第一轮八条「不许丢的修复」对应文件全部在位（`CacheWriteBudget`、`CardPortraitSessionCache`、`SkinCatalog.SpinePreflight`、`OrchisSkinFixCompat`、`MonsterSettingsCopyPolicy`、`CacheWriteNoticeState`、`ModLocalization.CacheWrite`、`ImportedTexturePathPolicy`、`MultiplayerSkinSync`、`ProtectedCharacters/CharSelectTakeoverSuppressor`）。

## 剩下的

- 未实机验证：愚者皮肤头像、商店 Hilde 尺寸、商人热切换点击、恐惧 / 极速选人背景、AncientWaifus 遗物与时间线立绘，需作者进游戏确认。
- 未 push、未发工坊（等实机测过）。
- `workshop/workshop.json` 的 `featureUpdate` / `changeNote` 与 `localizations.json` 15 语言版本串仍是 1.0.36，发布时统一改。
