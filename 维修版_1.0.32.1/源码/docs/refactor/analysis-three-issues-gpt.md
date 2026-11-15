# 三个角色老问题的只读排查

GPT，2026-11-15。基线 `9fdf6fc / 1.0.31.110`。按任务书顺序检查现有日志、切换器源码、第三方 PCK 和反编译结果。结论包括一个已离线复现的资源缺口、一个已定位的热切换初始化缺口，以及骨手位置的已确认触发链和待补坐标。

源码路径相对 `STS2SkinChanger/`。行号取本轮读取位置，DSH 同时添加的常驻注释可能使后续行号移动，附带的方法名可用于定位。

## 取证材料

- 日志根目录为 `D:\mod\工作2\_test_backup_20261114\logs_110`。魅魔使用 `godot2026-11-15T01.53.36.log`，下文简称闪退日志。骨手与战损使用 `godot.log`，下文简称战斗日志。
- 魅魔包位于 `C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\mods\测试\魅魔战士血量换装`，检查 `succubusIroncladHpWear.dll/.pck`。
- 骨手包位于游戏 `mods\MOD\森森自用 韩国大扔扔骨妹NSFW`，manifest id 为 `Moe-Necrobinder`，`has_dll=false`，包内是纯资源替换。
- 四档战损包位于游戏 `mods\测试\雪霁&森森&Kira改 a骨妹可战损版`，检查 `necrobinderHpWear.dll`。
- 反编译和离线资源包保存在 `C:\Users\yuinn\AppData\Local\Temp\sts2-gpt-three-issues-20261115`。下文的反编译路径均相对此临时目录，资源读取使用现有 PckInspect，目录重建使用现有 CatalogInspect。

## 1. 魅魔战士选角闪退

### 根因与证据

已确认选角隔离资源包漏掉 Spine 图集的纹理页。魅魔 PCK 只有 30 个文件，选角纹理实际位于 `.godot/imported/characterselect_ironclad.png-1b865d1e0a90d3a7fe38379cdb48c8d3.s3tc.ctex`，PCK 没有自己的 `.png.import`。当前游戏同一逻辑 PNG 的 `.import` 指向同 stem/hash 的 `.bptc.ctex`。PckResourceIndex 通过共享 importedToSource 表按完整导入路径归属资源，因此这个 `.s3tc.ctex` 未进入该包的逻辑 PNG 资产。其 atlas/skeleton 的导入名称仍匹配，所以新 atlas 被隔离并加载，纹理页却未进入相同别名目录。依据 `Catalog/SkinCatalog.cs` 的 `PckResourceIndex.Build:9905–9926`、`AddRemap:9989–10019`。

选角 atlas 的 `.spatlas` JSON 包含 `atlas_data`，第一页名是 `characterselect_ironclad.png`，尺寸 `3802 × 3802`。JSON 的 source_path 被改写到 `res://sts2_skin_runtime/.../animations/character_select/ironclad/characterselect_ironclad.atlas`，相对页名会从同一隔离目录解析。GetSiblingAtlasTextureAssets 只从当前索引已知资产及 PNG/import/remap 候选取页，无法把孤立 `.s3tc.ctex` 补成 PNG 资产。依据 `Catalog/SkinCatalog.cs:4039–4098,5162–5171,5762–5774`。

闪退日志第 `21231–21386` 行连续出现该隔离路径 PNG 的 `No loader found`、`Error loading resource`、`Can't load texture`，原生位置为 `GodotSpineTextureLoader::load / SpineAtlasResource.cpp:165`。栈来自 `WithRuntimeResourcesForSelection → RebuildMountedFullRuntimeCharacterDisplay`。随后仍记录选角重建完成、小模型刷新成功和覆盖层挂载完成。当前 `Core/SkinService.cs:6037–6057` 只根据 ResourceLoader 的顶层返回值判断，Spine 资源对象内的纹理页加载失败可能仍留有非 null 资源，后续 `Ui/ContextualSkinControls.cs:2183–2208` 会继续实例化背景。

本次用与实际选角入口一致的三个根资源生成隔离包，得到 **14 个逻辑资源、27 个文件**，与日志中的计数一致，生成包中没有任何 `characterselect_ironclad.png` 的逻辑重定向或 payload。输出为临时目录中的 `succubus-select-exact-roots.pck`。这复现了缺页前提。原生进程最终退出的具体指令仍需 crash dump 才能确认。

### SuccubusCharacterSelect 安装失败

包的 Postfix 签名仍为 `Postfix(NCharacterSelectScreen __instance, CharacterModel characterModel)`，当前游戏 `SelectCharacter` 只传 `NCharacterSelectButton charSelectButton`。Harmony 无法绑定 characterModel 参数，初始化器逐类 catch 后继续安装其他类。此 Postfix 的职责是给选角背景挂点击战损切换控件。它的安装失败解释点击控件缺失，现有资源缺页链路则发生在切换器自己的重建流程中。依据 `succubus/succubusIroncladHpWear.decompiled.cs:2712–2745,2781–2787,5993–6031`、闪退日志 `:494–506`，以及当前切换器适配按钮签名的 `Ui/ContextualSkinControls.cs` 中 CharacterSelectionSkinPatch。

### 推荐修法

推荐由切换器修复同源导入纹理归属，并在原生加载前校验 Spine 依赖闭包。

1. 在 `Catalog/SkinCatalog.cs` 的 PckResourceIndex 构建中，给“只带 imported payload 的包”补同源映射。以源文件名、hash 和导入资源类型识别 `.s3tc.ctex` / `.bptc.ctex` 对应同一逻辑 PNG，优先使用选中包的实际 payload，再生成指向实际文件的 import/remap。现有 MatchImportedFile 可在已有资产内按类型找唯一 payload，但前提是 payload 先进入正确资产，当前缺口在这一步之前。依据 `:9096–9109,9921–9926`。
2. 在 BuildRuntimeResourceOverlay 完成依赖图后、LoadResourcePack/ResourceLoader.Load 前解析 `.atlas` 或 `.spatlas` 的实际页名，校验每一页在本次别名目录有可解析源文件或重定向，且重定向目标已准备。失败时终止这次选角重建并提示缺失资源，回到游戏原包外观。应覆盖首次选择提交与启动恢复选择两条入口，调用点包括 `Core/SkinService.cs:1908–2007,6010–6042` 和 `Ui/ContextualSkinControls.cs:2172–2208`。
3. 校验失败的回退要使用明确的游戏原包 baseline 和现有选择事务，避免把半加载资源存入 RuntimeResourceBundles。当前跳过 null 后缓存其余资源的行为不足以检测 Spine 内部缺页，依据 `Core/SkinService.cs:6033–6057`。

切换器可以在挂载前发现这次的依赖缺口。不能仅凭一个 Harmony 类安装失败就判整包不可用，资源兼容判定应给出具体缺失页/目标路径。SuccubusCharacterSelect 的参数适配可另设兼容回调，从按钮取得 Character，并复用原有选择重放边界。

**分工为 Claude 或 GPT。** 涉及目录索引、导入格式、资源事务及原生加载前回退，需要设计后再实现。

### 实测验证

1. 先离线重建同样三个根资源，确认选角 PNG 别名和它指向的选中包 payload 均存在。
2. 实机从原版切到魅魔，观察选角背景与小模型。资源检查故意失败时，应显示缺失页提示并回到原版。
3. 退出重进验证保存选择的启动路径，日志中应消除该 Spine PNG 的 Can't load texture，并正常完成一次选角。

## 2. Moe-Necrobinder 骨手跑远

### 根因状态

**异常距离的最终数值根因未定，位置重写链路已定位。** Moe-Necrobinder 是纯 PCK，osty.tscn 的根位置为 `(1,4)`，Spine 子节点位置 `(-19,-23)`、缩放 `(0.23,0.23)`，Bounds 宽 `232`。包本身没有回合监听或位置修正 DLL。切换器在热切换后重跑通用玩家/宠物排布，游戏在召唤增长时又按主人位置和 Hitbox 计算另一套 Osty 位置，保存的皮肤偏移也会继续叠在外观 wrapper 上。日志缺少两次写入前后的实体绝对坐标，因此尚不能把“很远”的具体数值全部归到某一个量。

### 新回合在哪一步重置位置

游戏起始遗物 BoundPhylactery 在 `AfterEnergyResetLate` 中从第二回合起召唤 1 点骨手生命，走 `SummonPet → OstyCmd.Summon → ScaleAndPositionOsty(osty.MaxHp,0.75)`。后者为本地玩家 Tween 写入 **NCreature.Position**，目标公式如下。

```text
owner.Position
+ (owner.Hitbox.Size.X / 2, 0)
+ Lerp((150,-75), (250,-75), Clamp(osty.MaxHp / 150, 0, 1))
```

依据反编译 `game-phylactery/MegaCrit.Sts2.Core.Models.Relics.BoundPhylactery.decompiled.cs:22–42`、`game-ostycmd/MegaCrit.Sts2.Core.Commands.OstyCmd.decompiled.cs:35–86`、`game-creature/MegaCrit.Sts2.Core.Nodes.Combat.NCreature.decompiled.cs:1212–1234,1287–1290`、`game-osty/MegaCrit.Sts2.Core.Models.Monsters.Osty.decompiled.cs:21–25`。

换肤后走的是 `CharacterAppearanceRuntime.RefreshPlayerAndPetLayout`，它取全部玩家及有 PetOwner 的节点，调用 `NCombatRoom.PositionPlayersAndPets`。该方法对 pets 写通用横向排布坐标，公式含 `targetXPos`、宠物数量及 **pet.Visuals.Bounds.Size.X**。因此“重新点一次皮肤回正，下一回合再次跑远”与两条不同排布公式交替执行相符。依据 `Ui/CharacterAppearanceRuntime.cs:1263–1265,1502–1504,1553–1581`、`game-room/MegaCrit.Sts2.Core.Nodes.Rooms.NCombatRoom.decompiled.cs:562–642`。

### 读档后记录了什么基准

CapturePlayerAndPetLayout 保存房间弱引用、相机 scaling 和 fullyCenterPlayers，CaptureVisualBaseline 保存 Scale/DefaultScale。它们均没有保存角色或随从的绝对位置。热重建把新 visuals.Position 设为 Zero，保留配置 wrapper 偏移。读档/重新创建宠物时，游戏还会以 duration=0 调用 ScaleAndPositionOsty，立即采用上述主人 Hitbox/MaxHp 公式。依据 `Ui/CharacterAppearanceRuntime.cs:1540–1551,1670–1672,1815–1835,2022–2033`、`game-room/...NCombatRoom.decompiled.cs:755–772`。

战斗日志 `:1437,1461,1527,1599,1621` 明确显示 osty 存储值及 wrapper 为 **scale=1、off=(192,-50)**。选择 base 后第 `1599` 行仍应用这个偏移，选择 Moe 后第 `1621` 行继续应用同值。换回原皮会保留按 osty 组保存的变换，这可以解释原皮也偏的那部分现象。日志里的 visuals.Scale=1 是 visuals 根节点，PCK 的 `(0.23,0.23)` 在其内部 Spine 节点。

### 两个最可能方向与需补日志

| 方向 | 已有依据 | 最小需补日志 |
| --- | --- | --- |
| 热切换通用宠物排布与召唤专用排布采用不同基准 | 两个方法的公式、输入宽度及调用时机均不同 | 热切换排布前后，以及 ScaleAndPositionOsty 调用前/完成后，记录主人/骨手 Position、GlobalPosition、owner.Hitbox.Size、双方 Visuals.Bounds.Size、骨手 MaxHp |
| 保存的 `(192,-50)` 外观补偿叠在已改变的实体基准上，混用主人皮肤又改变 Hitbox 输入 | 日志确认同一偏移跨 base/Moe 持续，GetOstyOffsetFromPlayer 使用主人 Hitbox；SC 的 CorrectBounds 会更新 Hitbox | 同一条记录附当前主人/随从选项、wrapper.Position/Scale、Visuals.Scale/DefaultScale，并记录是谁发起排布 |

### 推荐修法

先在 `Ui/CharacterAppearanceRuntime.cs` 增加上述一次换肤、一次召唤的有限诊断，取得数值后统一本地 Osty 的排布入口。优先让热切换也复用游戏的 Osty 专用公式，将本地 Osty 从普通 pets 的通用排布写入中分开处理，随后应用已有 wrapper 变换。保留用户保存的偏移值，诊断时用面板重置做对照。若坐标证明主要是 owner.Hitbox 异常，应修共享 Bounds 基准计算，再验证专用排布。涉及方法为 `RefreshPlayerAndPetLayout`、`RefreshCreatureAnchors`、`CorrectBoundsForVisualTransforms`。

**分工为 Claude 或 GPT。** 实机坐标判读与共享外观变换相关，需要先确定具体数值来源。

### 实测验证

1. 固定主人皮肤，记录当前偏移后用面板重置随从变换，从原版切到 Moe，采集一次排布坐标。
2. 结束回合，在起始遗物召唤前后采集同一组坐标，确认发生跳变的写入与目标公式。
3. 保存读档，再换另一种主人皮肤重复一回合，对照主人 Hitbox 和骨手实际坐标。

## 3. 战斗中切四档战损后不换装

### 根因与初始化差异

已定位热切换漏建战损轮询 Timer 的生命周期缺口。这个四档包通过 `BattleWearTimerPatch.Postfix(NRun)` 在 NRun.GlobalUi 下创建 `BattleWearTimer_<ModId>`，每 **0.12 秒**调用 BattleWear.Tick。TickCombat 在轮询中解析当前玩家、血量和战损档位，首次为 NCreature 实例建立 CombatStates 并捕获 CombatSnapshot。热切换时原 NRun._Ready 已经过了，加载器只重新装回该 Postfix，当前 live creature 重放限于 NCreature._Ready 与 NCombatRoom.AddCreature，没有补调 NRun 上的 Timer 初始化。读档创建新 NRun 时，已激活的包能正常跑它的 _Ready Postfix，Timer、CombatStates 和血量档位更新链由此建立。依据 `necro/necrobinderHpWear.decompiled.cs:809–835,866–958,1002–1041,2272–2302`、`Core/ManagedSkinModLoader.cs:2250–2290,3129–3147`、`Ui/CharacterAppearanceRuntime.cs:1724–1729,2041–2053`。

战斗日志显示包先在选人阶段启用，`:1261–1266` 换成另一个骨妹皮肤并卸下其 23 个行为补丁。`:1699–1710` 在战斗中切回四档包，加载器跳过重复 ModEntry.Initialize，重新装回 23 个行为补丁。随后 `:1729–1747` 有每回合的 AttackGate 日志，整份日志没有该包 `wear timer attached.`，也没有本次战斗的战损 stage 应用日志。AttackGate 的回合事件补丁和 0.12 秒战损轮询是各自的入口，前者活动不代表后者存在。

`ModEntry.Initialize` 自己负责安装补丁与 Runtime.Bootstrap，Timer 的创建放在 NRun._Ready Postfix。OnActivationChanged 清空 CombatStates、缓存和 fuse，并尝试重新挂选角控件，它也没有创建这个 Timer。依据 `necro/necrobinderHpWear.decompiled.cs:708–725,2272–2302,5781–5797`。本轮证据支持漏建 Timer，实机仍需确认 GlobalUi 下节点是否缺失，以及是否能稳定触发一次 Tick。

### 推荐修法

在 `Core/ManagedSkinModLoader.cs` 的已选提供者激活完成后增加窄的“当前对局视觉运行时补初始化”步骤。针对已确认的 BattleWearTimerPatch 这类纯视觉 Timer 回调，在 NRun 已就绪时补调一次原包 Postfix，复用现有参数构造和节点增量追踪机制。原包 Postfix 已以 `BattleWearTimer_<ModId>` 是否存在做幂等检查，初次 Tick 会自行创建 CombatStates。

现有 `ReplaySelectedNodeReadyPostfixes(providerId,node)` 的内部调用器会从 runtime.Patches 与 NodeReadyPresentationPatches 合并筛选 `_Ready` Postfix，因此可以复用这部分机制，但新增入口应限定已验证的 Timer 回调。停用提供者/退出对局时配对处理新增 Timer 的停止、移除和再次启用。通用节点暂停逻辑按 node.GetType().Assembly 识别提供者，Godot.Timer 的类型属于 Godot 程序集，需把新增 Timer 的所有权纳入现有节点增量追踪。依据 `Core/ManagedSkinModLoader.cs:1575–1657,2382–2425,2463–2503`。

**分工为 Claude 或 GPT。** 需要设计提供者激活与对局 Timer 的配对生命周期，肥鱼可在接口和明确清单定好后做机械改动。

### 实测验证

1. 用别的骨妹皮肤进入战斗，切到四档包，确认 GlobalUi 下只有一个对应 Timer，出现一次补初始化及首次 Tick 日志。
2. 受伤跨过一条设置中的战损阈值，等待返回可换骨架的动画状态，确认受伤阶段自动变化。
3. 切走再切回并保存读档，确认轮询次数与 Timer 数量稳定，热切换和读档呈现一致。

## 本轮验证与交付边界

使用 PckInspect 读取两个第三方资源包，反编译两个第三方 DLL 与五个游戏类型，并用 CatalogInspect 离线复现魅魔的 27 文件隔离包缺页。所有反编译/实验输出在上述临时目录，主源码保持只读。本次排查未启动游戏，骨手具体距离和修复后的实际画面等待所列实测。

并行收到的常驻标签任务已按两点复核，六处修正后在房间 #76 放行。最终 44 项标签/清单对应 44 行注释新增、0 删除、0 行非注释新增，该批构建与测试采用 DSH 报告的通过结果。

## 可选、不阻塞

- 后续可对其他只有 imported payload 的旧包抽查一次同源纹理格式漂移，沿用本次缺页检测。
- 若需要精确定位魅魔最终原生崩溃指令，保留对应的 Windows crash dump 与游戏构建版本。
