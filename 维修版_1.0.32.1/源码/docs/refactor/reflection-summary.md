# 反射分类汇总（③）

基线 `092ec98 (1.0.31.93)`。明细见 `reflection.csv`（共 450 行）。
失败时行为是**静态启发式**：看查找语句之后 7 行里有没有 `throw` / 日志调用 / null 判断，都没有就记「未处理」。它不等于运行时会崩，只是「代码里没显式兜底」。

归属口径：`game` = 目标类型在 `sts2.dll`（v0.113.0，6378 个类型定义）里；`godot` = GodotSharp；`self` = 我们自己的类型 / BCL / 工具库；`thirdparty` = provider 包；`unknown` = **220** 行，其中 134 行目标完全动态、77 行只知道变量名（如 `变量:type`），静态判不出。

## 按归属

| 归属 | 行数 |
|---|---|
| unknown | 220 |
| game | 173 |
| self | 54 |
| godot | 3 |

## 按失败时行为

| 行为 | 行数 |
|---|---|
| 未处理 | 289 |
| 返回null静默 | 119 |
| 抛异常 | 33 |
| 日志 | 9 |

## 目标是游戏本体、失败时「未处理」或「抛异常」（135 行，阶段 B 优先收拢）

| 位置 | API | 目标 | 失败行为 | 备注 |
|---|---|---|---|---|
| `STS2SkinChanger/Core/BackgroundSkinService.cs:42` | AccessTools.Field | NRun._state | 未处理 | 查找后无显式兜底；所在方法 BackgroundOption |
| `STS2SkinChanger/Core/BackgroundSkinService.cs:44` | AccessTools.Property | NCombatRoom | 未处理 | 查找后无显式兜底；所在方法 BackgroundOption |
| `STS2SkinChanger/Core/BackgroundSkinService.cs:46` | AccessTools.Property | EncounterModel | 未处理 | 查找后无显式兜底；所在方法 BackgroundOption |
| `STS2SkinChanger/Core/BackgroundSkinService.cs:49` | AccessTools.Field | NCombatRoom._visuals | 未处理 | 查找后无显式兜底；所在方法 BackgroundOption |
| `STS2SkinChanger/Core/CombatHudResourceLifecycle.cs:42` | AccessTools.Field | NCombatUi._state | 抛异常 | 后续有 throw；所在方法 MissingMethodException |
| `STS2SkinChanger/Core/EventSkinRuntime.cs:17` | AccessTools.Field | NEventLayout._event | 未处理 | 查找后无显式兜底；所在方法 (未知) |
| `STS2SkinChanger/Core/EventSkinRuntime.cs:115` | AccessTools.PropertyGetter | EventModel.HasVfx | 未处理 | 查找后无显式兜底；所在方法 TargetMethods |
| `STS2SkinChanger/Core/EventSkinRuntime.cs:116` | AccessTools.PropertyGetter | EventModel.HasPhobiaModePortrait | 未处理 | 查找后无显式兜底；所在方法 TargetMethods |
| `STS2SkinChanger/Core/FrameworkPreviewCreature.cs:15` | AccessTools.PropertySetter | NCreature.Entity | 未处理 | 查找后无显式兜底；所在方法 Initialize |
| `STS2SkinChanger/Core/FrameworkPreviewCreature.cs:16` | AccessTools.PropertySetter | NCreature.Visuals | 未处理 | 查找后无显式兜底；所在方法 Initialize |
| `STS2SkinChanger/Core/FrameworkSkinRuntimePatches.cs:386` | AccessTools.Field | NEnergyCounter._player | 未处理 | 查找后无显式兜底；所在方法 Prefix |
| `STS2SkinChanger/Core/FrameworkSkinRuntimePatches.cs:604` | AccessTools.Field | CreatureAnimator._currentState | 未处理 | 查找后无显式兜底；所在方法 Apply |
| `STS2SkinChanger/Core/ManagedSkinModLoader.cs:28` | AccessTools.Method | ModManager.InvokeOnModDetected | 未处理 | 查找后无显式兜底；所在方法 (未知) |
| `STS2SkinChanger/Core/ManagedSkinModLoader.cs:30` | AccessTools.Field | ModManager.OnModDetected | 未处理 | 查找后无显式兜底；所在方法 (未知) |
| `STS2SkinChanger/Core/ManagedSkinModLoader.cs:32` | AccessTools.Field | ModManager._gameVersion | 未处理 | 查找后无显式兜底；所在方法 (未知) |
| `STS2SkinChanger/Core/ManagedSkinModLoader.cs:34` | AccessTools.Field | ModManager._circularDependencies | 未处理 | 查找后无显式兜底；所在方法 (未知) |
| `STS2SkinChanger/Core/ManagedSkinModLoader.cs:36` | AccessTools.Field | ModManager._settings | 未处理 | 查找后无显式兜底；所在方法 (未知) |
| `STS2SkinChanger/Core/ManagedSkinModLoader.cs:4925` | AccessTools.Method | ModManager.TryLoadMod | 未处理 | 查找后无显式兜底；所在方法 TargetMethod |
| `STS2SkinChanger/Core/MultiplayerRoomAppearancePatches.cs:26` | AccessTools.Method | NMerchantRoom.AfterRoomIsLoaded | 抛异常 | 后续有 throw；所在方法 TargetMethod |
| `STS2SkinChanger/Core/MultiplayerRoomAppearancePatches.cs:257` | AccessTools.Method | NHandImage.SetTextureToFightMove | 抛异常 | 后续有 throw；所在方法 TargetMethod |
| `STS2SkinChanger/Core/MultiplayerSkinSync.cs:2781` | AccessTools.Method | InitialGameInfoMessage.Serialize | 未处理 | 查找后无显式兜底；所在方法 TargetMethods |
| `STS2SkinChanger/Core/MultiplayerSkinSync.cs:2782` | AccessTools.Method | ClientLobbyJoinRequestMessage.Serialize | 未处理 | 查找后无显式兜底；所在方法 TargetMethods |
| `STS2SkinChanger/Core/MultiplayerSkinSync.cs:2783` | AccessTools.Method | ClientLoadJoinRequestMessage.Serialize | 未处理 | 查找后无显式兜底；所在方法 TargetMethods |
| `STS2SkinChanger/Core/MultiplayerSkinSync.cs:2784` | AccessTools.Method | ClientRejoinRequestMessage.Serialize | 未处理 | 查找后无显式兜底；所在方法 TargetMethods |
| `STS2SkinChanger/Core/MultiplayerSkinSync.cs:2796` | AccessTools.Method | NetHostGameService.OnPacketReceived | 未处理 | 查找后无显式兜底；所在方法 TargetMethods |
| `STS2SkinChanger/Core/MultiplayerSkinSync.cs:2797` | AccessTools.Method | NetClientGameService.OnPacketReceived | 未处理 | 查找后无显式兜底；所在方法 TargetMethods |
| `STS2SkinChanger/Core/MultiplayerSkinSync.cs:2820` | AccessTools.Method | NetHostGameService.StartSteamHost | 未处理 | 查找后无显式兜底；所在方法 TargetMethods |
| `STS2SkinChanger/Core/MultiplayerSkinSync.cs:2821` | AccessTools.Method | NetHostGameService.StartENetHost | 未处理 | 查找后无显式兜底；所在方法 TargetMethods |
| `STS2SkinChanger/Core/ProviderModelDiscovery.cs:27` | GetField | ReflectionHelper._modTypes | 未处理 | 查找后无显式兜底；所在方法 (未知) |
| `STS2SkinChanger/Core/ProviderSettingsControls.cs:11` | AccessTools.Field | DevConsole._commands | 未处理 | 查找后无显式兜底；所在方法 (未知) |
| `STS2SkinChanger/Core/SkinService.CharacterSkinBundleRuns.cs:58` | AccessTools.Field | RunManager._startTime | 未处理 | 查找后无显式兜底；所在方法 BindNewCharacterSkinBundleRun |
| `STS2SkinChanger/Core/SkinService.CharacterSkinBundleRuns.cs:190` | AccessTools.Method | RunManager.SetUpSavedSingleplayer | 未处理 | 查找后无显式兜底；所在方法 TargetMethods |
| `STS2SkinChanger/Core/SkinService.CharacterSkinBundleRuns.cs:191` | AccessTools.Method | RunManager.SetUpSavedMultiplayer | 未处理 | 查找后无显式兜底；所在方法 TargetMethods |
| `STS2SkinChanger/Core/SkinService.cs:120` | AccessTools.Field | LocManager._tables | 未处理 | 查找后无显式兜底；所在方法 (未知) |
| `STS2SkinChanger/Core/SkinService.cs:122` | AccessTools.Method | LocManager.SetLanguageInternal | 未处理 | 查找后无显式兜底；所在方法 (未知) |
| `STS2SkinChanger/Entry.cs:234` | GetProperty | ModelDb.All | 未处理 | 查找后无显式兜底；所在方法 Postfix |
| `STS2SkinChanger/Entry.cs:238` | GetField | ModelDb._contentById | 未处理 | 查找后无显式兜底；所在方法 Postfix |
| `STS2SkinChanger/Entry.cs:383` | AccessTools.Method | ModelDb.Init | 抛异常 | 后续有 throw；所在方法 TargetMethod |
| `STS2SkinChanger/Ui/AncientCompendium.cs:43` | AccessTools.Field | NSubmenu._stack | 未处理 | 查找后无显式兜底；所在方法 (未知) |
| `STS2SkinChanger/Ui/AncientCompendium.cs:3075` | AccessTools.Field | NAncientEventLayout._ancientEvent | 未处理 | 查找后无显式兜底；所在方法 DescendantsAndSelf |
| `STS2SkinChanger/Ui/BackgroundAppearanceScreen.cs:383` | AccessTools.Method | NPauseMenu._Ready | 未处理 | 查找后无显式兜底；所在方法 TargetMethods |
| `STS2SkinChanger/Ui/BackgroundAppearanceScreen.cs:384` | AccessTools.Method | NPauseMenu.OnSubmenuOpened | 未处理 | 查找后无显式兜底；所在方法 TargetMethods |
| `STS2SkinChanger/Ui/CardSkinControls.cs:49` | AccessTools.Method | NCard.Reload | 未处理 | 查找后无显式兜底；所在方法 (未知) |
| `STS2SkinChanger/Ui/CardSkinControls.cs:51` | AccessTools.Method | NCardLibrary.UpdateFilter | 未处理 | 查找后无显式兜底；所在方法 (未知) |
| `STS2SkinChanger/Ui/CardSkinControls.cs:57` | AccessTools.Field | NCard._cardOverlay | 未处理 | 查找后无显式兜底；所在方法 (未知) |
| `STS2SkinChanger/Ui/CardSkinControls.cs:59` | AccessTools.Field | NCardHighlight._shaderMaterial | 未处理 | 查找后无显式兜底；所在方法 (未知) |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2524` | AccessTools.Method | NCardLibraryGrid.FilterCards | 未处理 | 查找后无显式兜底；所在方法 TargetMethod |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2546` | AccessTools.PropertyGetter | NCardGrid.Columns | 未处理 | 查找后无显式兜底；所在方法 Postfix |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2549` | AccessTools.Method | NCardGrid.CalculateRowsNeeded | 未处理 | 查找后无显式兜底；所在方法 TargetMethod |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2586` | AccessTools.Method | NCardRewardSelectionScreen.RefreshOptions | 未处理 | 查找后无显式兜底；所在方法 TargetMethod |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2638` | AccessTools.Method | NCard.OnFreedToPool | 未处理 | 查找后无显式兜底；所在方法 TargetMethods |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2639` | AccessTools.Method | NCard.OnReturnedFromPool | 未处理 | 查找后无显式兜底；所在方法 TargetMethods |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2650` | AccessTools.Method | NCard.Reload | 未处理 | 查找后无显式兜底；所在方法 TargetMethod |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2661` | AccessTools.Method | NCard.Reload | 未处理 | 查找后无显式兜底；所在方法 TargetMethod |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2673` | AccessTools.Method | NCard.Reload | 未处理 | 查找后无显式兜底；所在方法 TargetMethods |
| `STS2SkinChanger/Ui/CardSkinControls.cs:2674` | AccessTools.Method | NCard.UpdateVisuals | 未处理 | 查找后无显式兜底；所在方法 TargetMethods |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:80` | AccessTools.Field | NCreature.<Visuals>k__BackingField | 未处理 | 查找后无显式兜底；所在方法 CreatureAppearanceBinding |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:82` | AccessTools.Field | NCombatRoom.<Background>k__BackingField | 未处理 | 查找后无显式兜底；所在方法 CreatureAppearanceBinding |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:84` | AccessTools.Field | NCreature._spineAnimator | 未处理 | 查找后无显式兜底；所在方法 CreatureAppearanceBinding |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:86` | AccessTools.Field | NCreature._selectionReticle | 未处理 | 查找后无显式兜底；所在方法 CreatureAppearanceBinding |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:88` | AccessTools.Field | NSelectionReticle._currentTween | 未处理 | 查找后无显式兜底；所在方法 CreatureAppearanceBinding |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:90` | AccessTools.Field | NCreature._stateDisplay | 未处理 | 查找后无显式兜底；所在方法 CreatureAppearanceBinding |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:92` | AccessTools.Field | NCreature._tempScale | 未处理 | 查找后无显式兜底；所在方法 CreatureAppearanceBinding |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:94` | AccessTools.Field | NRun._state | 未处理 | 查找后无显式兜底；所在方法 CreatureAppearanceBinding |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:96` | AccessTools.Field | NRun._roomContainer | 未处理 | 查找后无显式兜底；所在方法 CreatureAppearanceBinding |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:98` | AccessTools.Field | NMapScreen._marker | 未处理 | 查找后无显式兜底；所在方法 CreatureAppearanceBinding |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:100` | AccessTools.Field | NBossMapPoint._usesSpine | 未处理 | 查找后无显式兜底；所在方法 CreatureAppearanceBinding |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:102` | AccessTools.Field | NBossMapPoint._animController | 未处理 | 查找后无显式兜底；所在方法 CreatureAppearanceBinding |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:104` | AccessTools.Field | NBossMapPoint._material | 未处理 | 查找后无显式兜底；所在方法 CreatureAppearanceBinding |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:106` | AccessTools.Field | NBossMapPoint._placeholderImage | 未处理 | 查找后无显式兜底；所在方法 CreatureAppearanceBinding |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:108` | AccessTools.Field | NBossMapPoint._placeholderOutline | 未处理 | 查找后无显式兜底；所在方法 CreatureAppearanceBinding |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:110` | AccessTools.Method | NBossMapPoint.RefreshColorInstantly | 未处理 | 查找后无显式兜底；所在方法 CreatureAppearanceBinding |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:112` | AccessTools.Method | NTopBarRoomIcon.UpdateIcon | 未处理 | 查找后无显式兜底；所在方法 CreatureAppearanceBinding |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:114` | AccessTools.Field | NOrbManager._orbs | 未处理 | 查找后无显式兜底；所在方法 CreatureAppearanceBinding |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:116` | AccessTools.Field | NCombatUi._energyCounter | 未处理 | 查找后无显式兜底；所在方法 CreatureAppearanceBinding |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:118` | AccessTools.Field | NCombatUi._starCounter | 未处理 | 查找后无显式兜底；所在方法 CreatureAppearanceBinding |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:120` | AccessTools.Method | NCreature.ConnectSpineAnimatorSignals | 未处理 | 查找后无显式兜底；所在方法 (未知) |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:122` | AccessTools.Method | NCreature.UpdateBounds | 未处理 | 查找后无显式兜底；所在方法 (未知) |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:124` | AccessTools.Method | NCreature.UpdatePhobiaMode | 未处理 | 查找后无显式兜底；所在方法 (未知) |
| `STS2SkinChanger/Ui/CharacterAppearanceRuntime.cs:126` | AccessTools.Method | NCreature.SetOrbManagerPosition | 未处理 | 查找后无显式兜底；所在方法 (未知) |

（只列前 80 行，完整看 `reflection.csv`。）

## 同一成员被多处反射查找（≥2 次，合并候选：19 组）

| 目标类型 | 成员 | 次数 |
|---|---|---|
| NCard | Reload | 5 |
| ModManager | _gameVersion | 4 |
| 变量:definitionType | ReadAssembly | 3 |
| 变量:profileType | Target | 3 |
| 变量:PropertyType | MonsterId | 3 |
| 变量:opCodesType | Call | 3 |
| ModelDb | EventEncounters | 3 |
| NPauseMenu | _Ready | 3 |
| NPauseMenu | OnSubmenuOpened | 3 |
| NRun | _state | 2 |
| 变量:stateType | GetCurrent | 2 |
| LocManager | _tables | 2 |
| 变量:type | All | 2 |
| NCreature | UpdateBounds | 2 |
| NCreature | SetOrbManagerPosition | 2 |
| NBestiary | _selectedEntry | 2 |
| 变量:skinServiceType | GetOrLoadRuntimeScene | 2 |
| NFakeMerchant | <MerchantButton>k__BackingField | 2 |
| NCombatCardPile | _localPlayer | 2 |