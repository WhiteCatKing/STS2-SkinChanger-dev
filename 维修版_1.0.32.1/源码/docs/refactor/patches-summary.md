# 补丁清单汇总（①）

基线 `092ec98 (1.0.31.93)`；扫描范围 `STS2SkinChanger/` + `compat/`（不含 tests/tools/bin/obj）。明细见 `patches.csv`。

- `[HarmonyPatch]` 补丁类：**181** 个
- 明细：特性 **200** 行 + 手动安装 **44** 行

## 按模块

| 模块 | 行数 |
|---|---|
| core | 57 |
| card | 52 |
| character | 41 |
| multiplayer | 32 |
| merchant | 17 |
| monster | 10 |
| event | 9 |
| ancient | 8 |
| compat:EmperorCrab | 7 |
| background | 6 |
| osty | 1 |
| workshop | 1 |
| compat:CustomSkeletonLoader | 1 |
| compat:OstyDefectImpostor | 1 |
| compat:PengoTarotFont | 1 |

口径：模块按**文件名 → 目标类型 → 目录**的顺序判定（兼容层按目录归 `compat:<包名>`）；`core` 含框架/启动/存档/PCK/装载等非具体功能的补丁。


## 按目标归属

| 归属 | 行数 |
|---|---|
| game | 191 |
| self | 35 |
| unknown | 18 |

口径：`game` = 目标类型在 `sts2.dll` v0.113.0 里；`self` = 我们自己的类型 / BCL；`thirdparty` = provider 包；`unknown` = **18** 行（8 动态目标 + 10 `(运行时解析)`）。


## 按目标类型 Top 30

| 目标类型 | 补丁数 |
|---|---|
| NCard | 12 |
| CharacterModel | 11 |
| NCharacterSelectScreen | 8 |
| NCreature | 7 |
| RunManager | 6 |
| NPauseMenu | 6 |
| RelicModel | 5 |
| NMerchantRoom | 5 |
| FrameworkRegistryCooperation | 5 |
| NCombatRoom | 4 |
| EventModel | 4 |
| NFakeMerchant | 4 |
| OrbModel | 4 |
| AncientEventModel | 4 |
| CardModel | 4 |
| HalfHpCrabSkinPatch | 4 |
| NEventLayout | 3 |
| NHandImage | 3 |
| NetHostGameService | 3 |
| NGame | 3 |
| NCardLibrary | 3 |
| NBestiary | 3 |
| ProviderMenuGateCompat | 3 |
| EncounterModel | 2 |
| CardPoolModel | 2 |
| ModManager | 2 |
| NRestSiteCharacter | 2 |
| StartRunLobby | 2 |
| NRun | 2 |
| MessageTypes | 2 |

## 同一游戏方法挂了 ≥3 个补丁（双份执行高风险点）

| 目标类型 | 方法 | 补丁数 |
|---|---|---|
| NCreature | _Ready | 3 |
| NPauseMenu | _Ready | 3 |
| NPauseMenu | OnSubmenuOpened | 3 |
| NCard | Reload | 3 |