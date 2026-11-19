# 上游 1.0.35 修复交付：诊断限额 / 外部 WER 工具 / MLM 计划原子保存（DSH，2026-11-17）

任务书：`docs/refactor/task-fix-upstream-1.0.35-20261117.md` 第 3、4、5 项。仓库 `D:\mod\工作2\上游修复_1.0.35`，分支 `fix/1.0.35-stability`，基线 `4dbeae7`。
**未 commit、未 push、未改版本号**；改动文件均为 LF。

## 第 3 项：内置诊断限额（`Core/CrashDiagnostics.cs`、`Diagnostics/CrashWatch.ps1`）

改动：
- 诊断目录从「只按份数裁剪已完成的会话」改成 **按总字节 512 MB + 保留 14 天 + 份数** 三条一起裁剪（`CrashDiagnostics.Prune`）。
- **没完成的旧会话也参与裁剪**：正常退出 3 份、异常结束 8 份、未完成 4 份，分别计数；正在观察的会话（传入 `keepDirectory`）永远不删。
- 年龄按目录里**最新文件时间**算（不是创建时间），避免一次长会话被误判；越界目录、非 GUID 目录、reparse point 一律跳过。
- 总量超预算时从最旧的会话开始删，直到降到预算以内；单步删除失败只影响这一份。
- **转储生成加超时**：`CrashWatch.ps1` 的 `ScCrashDump.Save` 改成后台线程写、`Join(20 s)` 超时就删掉半截文件并写 `dump-error.txt`；超时秒数走 `session.json` 的 `DumpTimeoutSeconds`（默认 20）。

## 第 4 项：外部 WER 工具（`tools/Sts2CrashWatch/`）

改动（`WerDumpConfig.cs` 重写，`DumpWriter.cs`、`Program.cs` 配套）：
- 默认改成 **小转储**：`DumpType 2 → 1`、`DumpCount 10 → 2`。
- `DumpWriter` 写卡死转储时也只保留线程栈（`MiniDumpWithThreadInfo`），不再写完整内存；同样带 20 秒超时（后台线程 + `ManualResetEventSlim`），超时删半截文件返回 null。
- **注册表恢复修好**：开启前把用户原有 `LocalDumps\SlayTheSpire2.exe` 的全部值（String / ExpandString / DWord / QWord）连同「父键、子键是否存在」备份成 JSON 写进 `__watch_backup`；退出时还原原值、删掉本次新加的值，只删除自己创建的键（父键原本不存在且已空才删），不碰别的程序的 LocalDumps 项。
- **强退残留**：下次启动 `Enable()` 会先按备份还原再重新开启（之前是发现标记就当成用户设置，永久改掉）。
- 新增离线自测 `--selftest-wer`（`WerDumpSelfTest.cs`）：用一次性注册表路径验证「原有设置被还原、原本不存在的键被删掉、强退残留能被恢复」，跑完删测试键，不碰真实 `LocalDumps`。

补充说明：该工具不在 Mod 分发包里、Entry 也不启动它（与 GPT #106 的结论一致），只有用户手动运行 `Sts2CrashWatch` 时才会改注册表；现在这个改动才真正安全。

## 第 5 项：MLM 启动计划原子保存（`Core/MlmLaunchPlanSync.cs`）

- `WritePlan` 里原来的 `File.WriteAllText` 换成 `ReplaceFileAtomically`：同目录 `.<guid>.tmp` → `Flush(flushToDisk: true)` → `File.Move(overwrite: true)`。
- 失败（目标不可替换、磁盘满等）时清掉临时文件并抛出，**原 `mod_launch_last_plan.json` 保持不变**；`SyncFromSettings` 原有的每文件 try/catch 会记录日志，不会半截覆盖。

## 验证

- 主工程 `dotnet build -c Release`（GameAssemblyDir=正式版）：**0 错误、0 警告**。
- `Sts2CrashWatch` 工具：`dotnet build -c Release` 0 错误；`--selftest-wer` **通过**；`--simulate-hang` 生成的 dump 为 **0.2 MB**（改动前是完整内存转储）。
- `CrashWatch.ps1`：UTF-8 语法检查通过；`CrashDiagnosticsTests` 里真实拉起 observer 的集成用例全过。
- `STS2SkinChanger.LogicTests`：全绿（末行 `Skin Changer logic policy tests passed.`）。
- `STS2SkinChanger.RuntimeTests`：**整体跑不通**，卡在 `BaselinePartTakeoverTests`（「只覆盖商人房间手部的风格包必须按部件接管处理」）。已在 `4dbeae7` 干净 worktree 上复现，**是上游 1.0.35 自带失败，不是本次改动引入**。我本次相关的两组（`CrashDiagnosticsTests` 含新增裁剪用例、`MlmLaunchPlanSyncTests`）单独跑全过：
  - 新增裁剪用例：陈旧未完成会话按 14 天被删、正在观察的会话保留、总量预算从最旧开始删、成功/失败的原子替换与临时文件清理。

## 涉及文件

```
STS2-SkinChanger_修改件/STS2SkinChanger/Core/CrashDiagnostics.cs        (+125/-…)  诊断限额 + 超时
STS2-SkinChanger_修改件/STS2SkinChanger/Core/MlmLaunchPlanSync.cs       (+38)      原子保存
STS2-SkinChanger_修改件/STS2SkinChanger/Diagnostics/CrashWatch.ps1      (+23)      转储超时
STS2-SkinChanger_修改件/tests/STS2SkinChanger.RuntimeTests/CrashDiagnosticsTests.cs (+54) 裁剪用例
STS2-SkinChanger_修改件/tests/STS2SkinChanger.RuntimeTests/MlmLaunchPlanSyncTests.cs (新增)
STS2-SkinChanger_修改件/tests/STS2SkinChanger.RuntimeTests/Program.cs   (+1)       接上 MLM 用例
tools/Sts2CrashWatch/WerDumpConfig.cs                                   (重写)     备份/还原
tools/Sts2CrashWatch/WerDumpSelfTest.cs                                 (新增)     注册表自测
tools/Sts2CrashWatch/DumpWriter.cs                                      (+75/-…)   小转储 + 超时
tools/Sts2CrashWatch/Program.cs                                         (+5)       --selftest-wer
```

## 留给 Claude / 云

- `BaselinePartTakeoverTests` 在基线上就红，需要上游自己修，不在本单范围；如果合并前要全绿，得单独处理。
- 诊断目录 512 MB / 14 天是我按任务书建议值定的；如要更紧，改 `CrashDiagnostics` 顶部三个常量即可。
- 实机验证：诊断裁剪与 WER 注册表行为都能离线覆盖，但「游戏真崩时 observer 落盘」仍需一次实机（云端版本没动这部分主流程）。

## #12 一键给所有英雄设商人皮肤（2026-11-18）

**背景**：商人皮肤在切换器里属于**怪物分组**，所以「管理皮肤包 → 怪物 · Mod 优先级」那一行列出的就是 Merchant2CuteII、商人"阿塔"这些包，每个英雄的皮肤包各存一份（启用 + 顺序），英雄多了要逐个勾。

**改动**
- `Core/MonsterSettingsCopyPolicy.cs`（新增）：纯策略。`Capture` 取当前皮肤包的 `MonsterMode` + `MonsterModPriority`（启用状态与顺序，逐条克隆）；`Apply` 写进目标；`ApplyToAll` 批量并返回条数。只碰怪物字段，角色皮肤、卡牌区、预设名一律不动。
- `Core/SkinService.CharacterSkinBundles.cs`：新增 `CopyMonsterSettingsToAllCharacterBundles(sourceGroupId, source)`。遍历 `Catalog.Groups` 里所有角色分组（含 Mod 英雄），把每个英雄**已存在**的皮肤包逐个覆盖怪物设置；源皮肤包本身跳过。**没有皮肤包的英雄直接跳过**——不替玩家凭空新建 `[P]` 皮肤包（避免一次性给所有英雄塞一个默认包）；这一点按「照现有代码习惯」选了「跳过」，写在方法注释里。
- `Ui/CharacterSkinBundleControls.ContentModes.cs`：怪物内容区（模式下拉旁、创意工坊按钮旁）新增「把怪物设置应用到所有英雄」按钮；两段式确认，第一次点击变成「再次点击确认：这会覆盖其他英雄的怪物设置」并染成警告色，第二次才执行；执行后状态栏显示复制到了几个皮肤包（或有「其他英雄还没有皮肤包」）。
- `Core/ModLocalization.MonsterCopy.cs`（新增）：按钮/确认/成功/无目标 4 条文案 × 15 种语言（eng/zhs/zht/deu/esp/spa/fra/ita/jpn/kor/pol/ptb/rus/tha/tur）。

**测试**：`tests/STS2SkinChanger.LogicTests/MonsterSettingsCopyPolicyTests.cs`（已接进 LogicTests）：三个英雄的皮肤包，复制后目标英雄的怪物模式/条数/顺序/启用状态与源一致；卡牌区（模式、优先级、预设名）、角色皮肤、包名不变；复制的列表互相独立（改一个不影响源/另一个）；空序列返回 0。

**验证**：主工程 0 错误 0 警告；LogicTests 全绿（含新组 `Monster settings copy policy passed`）；完整 RuntimeTests 跑完（SetErrorMode(3) 启动），末行 `Workshop policy tests passed.`。未改版本号、未 commit。UI 位置与两段式确认需要实机看一眼（离线只验证了策略与编译）。
## 一、局内热切换商人后商人点不了（2026-11-19）

**根因**：热切换重建 `NMerchantButton` 时，`InstantiateMerchantButton` 在节点**还没进树**的时候调 `ReplayNodeReadyBeforeTree`，手动重放了商人包的 `_Ready` **前缀**。Merchant2CuteII 的 `MerchantButtonPatch.Prefix` 里 `MerchantVisualMotion.Apply` 会调 `GetPath()`，节点不在树里必然抛 `Cannot get path of node as it is not in a scene tree`，前缀只执行了一半：它已经挂上去的覆盖层留在了按钮上，于是按钮被自己的视觉层挡住、点不开。
同一次 `_Ready` 的这套前缀本来就会在按钮进树时由 `MerchantButtonProviderReadyBridgePatch`（Prefix Priority.First）再重放一遍——也就是同一前缀被跑两次，一次失败留下垃圾、一次成功，这才有「能看不能点」。

**改动**（`Core/Features/Merchant/MerchantRuntimeAppearance.cs`）：
- 删掉 `InstantiateMerchantButton` 里的 `ReplayNodeReadyBeforeTree(...)` 调用与这个只剩调用点的方法。前缀改由既有的**树内**桥（`MerchantButtonProviderReadyBridgePatch`）在原生 `_Ready` 之前重放——顺序同样是「原生初始化前」，但节点已经在树里，`GetPath()` 正常，且只跑一次。
- 手部 / 库存的 `ReplaySelectedNodeReadyBehavior` 自己就是把节点挂进树之后才重放的，未改。

**验证**：主工程 beta 引用 0/0、官方 v0.111.1 引用 0/0；`--test-merchant-lifecycle` 通过（200 个商店/火堆场景身份、初始化先于建节点、离场提供者隔离）；完整无参数 RuntimeTests 绿色（末行 `Workshop policy tests passed.`）。

**A 大商人没有白色描边**：`MerchantFeminization.pck` 只有自己的 Spine（`nsfw.tres/.spatlas/.spjson/.ctex` + `nsfw_fake.*`）、一只手的素材和 `merchant_room/top/shop_merchant_top.tres` 替换，**没有任何描边资源/网格**；它用自己的骨架替换商人本体，原版描边网格自然套不上去。切换器侧热切换只是复用上一颗按钮的 `MerchantSelectionReticle`（`ReplaceMerchantButtonCore` 里的 `CopyRequiredUniqueNode`），没有删除或禁用描边。判定：**包本身的问题**，不改代码。
## 二、极速（Blitz）/ 恐惧（Dread）模式选人背景退回原版（2026-11-19）

**根因（已确认的链路）**：两种模式的钩子本来就生效——`CharacterSelectModePatches` 把 `CharacterSelectionSkinPatch` 绑到了 `NBlitzScreen` / `NDreadScreen.SelectCharacter`，玩家日志里能看到 `NDreadScreen.SelectCharacter_Patch1 → ShowCharacter` 和「已从隔离依赖路径重建…选角展示」，也就是背景替换执行过。
问题出在**替换之后**：反编译确认这两套界面还有绕过 `SelectCharacter` 直接重写 `_bgContainer` 的入口——`NDreadScreen.SetSelectedCharacterVisuals(CharacterModel)`（Dread 连胜动画收尾会直接调它，见 `PlayDreadWinStreakResultAnimation` 的 finally）。它清空 `_bgContainer` 后用 `PreloadManager.Cache.GetScene(characterModel.CharacterSelectBg)` 实例化**原版**背景，没有经过我们的 Patch，于是画面退回原版。

**改动**：
- `Ui/CharacterSelectModeBackgroundPatch.cs`（新增）：`Postfix(Control __instance, CharacterModel __0)`，只调用「重做背景」这一步。
- `Ui/ContextualSkinControls.cs`：新增 `ReapplySelectedBackground(screen, character)`——取当前分组、用 `SkinService.WithRuntimeResources` 把选人背景场景从当前选择的隔离依赖里实例化，再走既有的 `ReplaceCharacterBackground`；包里没有规范背景场景时什么都不换（保留它自己画的层）。顺带修好 `DescribeCharacterSelectBackground`：它算了一堆 `AnimatedBg` 子节点状态却**从来没输出**，现在会打一行 `[选角背景] 分组/角色：…`，下一轮实测能直接看到容器里是谁。
- `Ui/CharacterSelectModePatches.cs`：给模式屏幕的 `SetSelectedCharacterVisuals` 动态挂 `Priority.Last` 的 Postfix（按类型反射，缺这个方法的版本自动跳过）。
- `tests/.../CharacterSelectModeBackgroundTests.cs`（新增，`--test-mode-background`）：验证 `NDreadScreen.SetSelectedCharacterVisuals` 存在且签名正确、`CharacterSelectModePatches.Install` 确实把 Postfix 挂上去。

**已知未覆盖**：`NBlitzScreen` 里只有 `SelectCharacter` 与随机角色开局的 `DoRandomCharacterChange` 会写 `_bgContainer`，没有单独的 SetSelectedCharacterVisuals 入口；如果极速模式实测仍退回原版，请把新打的 `[选角背景]` 那行发我，就能定位是哪个入口在覆盖。

**验证**：beta 引用 0 错误 0 警告；官方 v0.111.1 引用 0 错误 0 警告；`--test-mode-background`、`--test-merchant-lifecycle` 通过；LogicTests 全绿。
完整 RuntimeTests 目前停在 `FoolWaifuIdentityTests`（龙娘 11:51 正在改的 Fool_Waifu 兼容，未提交），失败点与本次改动无关；等他这轮收尾后我再复跑。
### 补充：魅魔战士这类「DLL 自己画选人叠加层」的包在恐惧/极速模式下的背景（2026-11-19 第二轮）

**根因（反编译确认）**：`succubusIroncladHpWear` 的 `BattleWear.SuccubusCharacterSelect` 是 `[HarmonyPatch(typeof(NCharacterSelectScreen), "SelectCharacter")]`，Postfix 里取 `_bgContainer` 的 `<角色>_bg` 子节点，再 `CallDeferred(Attach(background))` 挂自己的 Spine 叠加层。恐惧/极速是 `NDreadScreen`/`NBlitzScreen`，**从不调用** `NCharacterSelectScreen.SelectCharacter`，所以叠加层根本不会挂；而这类包又没有规范背景场景，上一轮的 `ReapplySelectedBackground` 按设计跳过——两层原因叠在一起就是「背景仍是原版」。

**改动**：
- `Ui/ProviderModeSelectOverlayBridge.cs`（新增）：只在模式界面（非 `NCharacterSelectScreen`）上，取当前选中提供者（`SkinService.GetSelectedCreatureRuntimeProvider`）的**程序集**（`ManagedSkinModLoader.IsProviderAssemblyFor`），反射调用其 `Attach(Control background)`（包自己的、与屏幕类型无关的叠加层入口），把模式界面的 `<角色>_bg` 传进去。包内部 `SkinSelectionGate.MenuAllowed()` 与「已有 catcher 就不重复挂」的判断保留，所以未选中不会挂、重复调用可重入。
- `Ui/ContextualSkinControls.cs`：`ShowCharacter` 末尾对非标准界面补一次（覆盖没有 `SetSelectedCharacterVisuals` 的 `NBlitzScreen`）。
- `Ui/CharacterSelectModeBackgroundPatch.cs`：Dread 的 `SetSelectedCharacterVisuals` 之后也补一次（连胜动画会清空背景容器，需要重新挂）。
- 日志：成功会打 `[选角背景] 已为 <provider> 在模式界面重放提供者的选人叠加层（类型.Attach）`。

**验证**：beta 与官方 v0.111.1 引用均 0 错误 0 警告；`--test-mode-background`、`--test-merchant-lifecycle` 通过；**完整无参数 RuntimeTests 绿色**（末行 `Workshop policy tests passed.`）；LogicTests 全绿。
**未覆盖**：贞德战士 `JoanOfArcIroncladHpwear` 若没有同形的 `Attach(Control)` 方法，这条桥不会生效；实测时请一并看一眼，日志里没有那行就是没挂上。