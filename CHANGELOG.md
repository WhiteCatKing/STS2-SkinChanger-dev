### 1.0.31.45 —— 奥斯提预览框三处诊断日志（不改行为）

1.0.37~1.0.43 连续几版都没能让预览框稳定隐藏，先取证：在 `ShowCharacter`、
`RebuildCharacterDisplay`、`OstyModelPreviewControls.Refresh` 各打一行 `[奥斯提诊断]`，
记录进入顺序与当时的隐藏标记、面板存在/可见状态。
### 1.0.31.44 —— 回退奥斯提预览框改动，修复贞德战损切换抖动

作者实测 1.0.39~1.0.43 期间：①奥斯提预览框始终无法稳定隐藏（判定路径没被触发）；
②贞德战损切换出现"混乱画面→默认色→变体色"的循环抖动。
为避免继续扩大影响，把 `Ui\ContextualSkinControls.cs` 与 `Ui\OstyModelPreviewControls.cs`
整体退回 1.0.31.36 的状态（奥斯提只保留"非主人页面收起下拉"这一层，贞德恢复原样）。
### 1.0.31.43 —— 随从预览框的判定挪到角色切换入口

前几版把判定放在 `RebuildCharacterDisplay`，实测切角色时那条路径并不总是跑，于是状态陈旧。

这一版把 `CompanionPreviewAllowed` 的计算放进 `ShowCharacter(screen, character)`——
游戏的 `SelectCharacter` 每次点击都会进来——不是骨妹就当场收起预览框。
### 1.0.31.42 —— 奥斯提预览框改单一真源判定

作者实测：框出现后切角色仍会重现，说明 screen 上的标记被多条路径互相覆盖（`PopulateOsty` 会写、`RebuildCharacterDisplay` 也会写）。

这一版不再用 screen 标记做判定，改成 `ContextualSkinControls.CompanionPreviewAllowed` 单一静态真源：
每次 `RebuildCharacterDisplay` 按当前选中角色重算；`OstyModelPreviewControls.Refresh` 只看它，
不是骨妹就 `Hide` 并 return，任何"显示"调用都过不去。
### 1.0.31.41 —— 修奥斯提预览框"切角色后又冒出来"

1.0.40 里 `PopulateOsty` 每次都会无条件 `screen.SetMeta(CompanionHiddenMeta, false)`，
切角色时它跟着跑，把 1.0.39 设好的「隐藏」标记又擦成 false，于是随后的 `Refresh` 照样 `ShowOsty`。

这一版删掉那行无条件复位，改为：只有归属不符（非骨妹）时才置 true 并收起；骨妹页面由
`RebuildCharacterDisplay` 置 false。
### 1.0.31.40 —— 切角色时把奥斯提预览框按"刷新"处理

作者实测：框存在时只要切一下皮肤就消失了 —— 说明隐藏逻辑本身对，只是「切角色」这条路径没跑到它。

这一版在判定为隐藏时：①递归找出所有 `STS2OstyModelPreview` 节点逐个设为不可见（切角色时面板可能换了父节点）；
②再调一次 `OstyModelPreviewControls.Refresh(screen)`，让包内状态按同一套"刷新"逻辑复位。
### 1.0.31.39 —— 非骨妹页面立刻收起奥斯提预览框

1.0.37 只把"该不该显示"记在 screen 上，真正收起还是等 `Refresh`；而切角色时 `Refresh` 不一定被调用，
于是「选过骨妹再切别的角色」框会一直留着。

这一版在 `RebuildCharacterDisplay` 里判定为隐藏时直接 `Hide(screen)` 并把
`InfoPanel/STS2OstyModelPreview` 设为不可见——切换的瞬间就收掉，符合作者要求的四条：
选其他角色不显示、只有骨妹显示、每次切换都检测、销毁后回到"不显示"状态。
### 1.0.31.38 —— 退出耗时诊断（只加日志）

在 `Core\QuitExitCleanup.cs` 追加 `QuitTimingDiagnostics`：`NGame.Quit` 进入/返回各打一行，
返回时带耗时。用来区分「退出卡在我们自己的清理里」还是「日志写完后卡在引擎侧收尾」。
### 1.0.31.37 —— 奥斯提预览框与骨妹强绑定（作者要求）

1.0.31.36 的标记只在 `PopulateOsty` 里写，切到别的角色时那条路径不一定重跑，所以选过骨妹后再切别的角色，框又回来了。

这一版把判定挪到 `RebuildCharacterDisplay(screen, character, groupId)` 开头：每次重建角色展示时都按**当前选中的分组**
重写 `sts2_skin_companion_hidden`——只有 `groupId` 等于随从主人（骨妹）时才为 false。
`OstyModelPreviewControls.Refresh` 继续读这个标记，非骨妹页面直接 `Hide` 且不再 `ShowOsty`。
只作用于选人界面，局内不碰（局内其他角色仍可取到奥斯提）。
### 1.0.31.36 —— 随从预览框按页面归属彻底隐藏（作者要求）

1.0.31.35 只收了一次面板，但 `OstyModelPreviewControls.Refresh` 会无条件 `ShowOsty()`，
而且面板实际挂在 `InfoPanel/STS2OstyModelPreview` 下（1.0.31.35 查错了父路径）。

这一版：隐藏时在 screen 上打 `sts2_skin_companion_hidden` 标记，`Refresh` 开头读到就
`Hide` + 不再 `ShowOsty`；回到骨妹页面时标记被清掉，预览框照常出现。
### 1.0.31.35 —— 非骨妹页面连随从预览框一起隐藏（作者要求）

1.0.31.34 只隐藏了下拉，右侧的随从预览框还在。这一版在同一个归属判定里补上
`OstyModelPreviewControls.Hide(screen)`，并按 `OstyModelPreviewControls.PanelName`
（`STS2OstyModelPreview`）隐藏预览面板本身。
### 1.0.31.34 —— 随从下拉只在骨妹页面显示（作者要求）

作者要求：除骨妹（死灵之缚）之外的所有角色都不显示随从/手部下拉。

`Catalog\SkinCatalog.cs` 新增 `public string? CompanionOwnerGroup(string companionGroupId)`
（随从分组 → 主人分组，`CompanionGroups` 里 osty → necrobinder）。
`Ui\ContextualSkinControls.cs` 的 `PopulateOsty` 增加归属判定：选择器所在分组（`GroupMeta`）
不等于随从主人的分组时，直接把下拉隐藏并清空 `OstyGroupMeta`。
### 1.0.31.33 —— 主人页面不再列出随从专用皮肤（玩家反馈修复）

玩家在战士页选中「随从分组 osty」条目后整局卡死（1.0.31.32 已加兜底，这一版治因）。

`Catalog\SkinCatalog.cs` 新增 `public bool IsCompanionOnlyProvider(string providerId)`：不看当前选择，
只看声明——某个 provider 是否**只**被随从分组的选项引用。旧的
`IsCompanionOnlyProviderSelection(providerId, selections)` 要求选择已经落在随从分组里，
玩家在主人页选择时恒为 false，所以排除表一直为空。

`Ui\ContextualSkinControls.cs` 的选项循环（`:907`）加过滤：当前页面不是随从分组时，
跳过只服务随从分组的条目。
### 1.0.31.32 —— 随从分组不再占用主人选人槽位 + 整包重建兜底（玩家反馈修复）

玩家在战士页选中随从专用条目后整局卡死：
`NSpineAutoPlayer's parent's skeleton data must have exactly 1 animation. This has 6.`
栈为 `WithRuntimeResourcesForSelection` ← `RebuildMountedFullRuntimeCharacterDisplay` ← `RebuildCharacterDisplay`。

`Ui\ContextualSkinControls.cs:1991` 的整包运行时重建分支改为：
1. 随从分组（`SkinService.Catalog.IsCompanionGroup`）不再走整包重建，改走普通资源挂载；
2. 整段 try/catch 兜底，异常打 `[骨骼防护]` 后退回资源挂载，不再 `return` 卡死整局。
### 1.0.31.31 —— 不再摘除「我们没接管」的 provider 呈现补丁（玩家反馈修复）

`Core\VisualPatchGuard.cs` 摘补丁的判据原来是「程序集不是激活中的 provider」。对**排在本 Mod 前面、
由游戏原生加载**的皮肤包（玩家实测 Merchant2CuteII 排第 2、我们排第 51），`IsProviderAssemblyActive`
恒为 false，于是它的 8 个呈现补丁（含 `NMerchantButton._Ready`、`NMerchantCharacter.PlayAnimation`）
被我们摘掉，商人回原版。

现在加一道前提：只有**由本 Mod 加载/接管**的程序集（`ManagedSkinModLoader.IsManagedProviderAssembly`）
才可能被摘。没接管的包一律保留原样。
### 1.0.31.30 —— 外观覆盖包跨会话缓存命中（性能）

修复「每次启动都重写 587.8 MB 外观覆盖包」。原因是覆盖包签名里用了归档的**物理路径**，
而运行期生成的缓存包路径包含每会话的目录名与递增编号，同一个覆盖包每次启动都被算成新内容。

签名改用稳定归档标识：文件名去掉数字后的骨架 + 归档文件字节数 + 该资源字节数。
会话号变化不再影响命中；包内容变了仍然会失效并重写。

效果预期：`覆盖包[visual] 写盘 587.8 MB = 321 ms` 变为 `命中磁盘缓存`（<1 ms），
启动的「写包+挂载」337 ms 与 588 MB 磁盘写入一起消失。
### 1.0.31.29 —— 修「读档后商人仍显示旧皮肤」

读档进入商店时，房间在皮肤包资源挂载之前就 `_Ready` 完，之后再没有人回来补这一次刷新；只靠事件（`_Ready` / `SetCurrentRoom`）都不可靠，日志里连一行 `[商人]` 都没有。

改成在一直运行的每帧 tick（`MultiplayerSkinSync.Tick`）里看住当前 `NMerchantRoom`：每个房间实例只处理一次，最多重试 240 帧（约 4 秒），成功打 `[商人] 房间就绪后已套用当前选择：<选项>`，换房间或换选择自动重新开始跟踪。不依赖任何事件是否触发。
### 1.0.31.17 —— 一个补丁类装不上，不再拖垮整包行为补丁

`DefectSkinMod`（鸡煲娘化）的补丁类里有一个方法声明了 `characterModel` 参数，而 v0.113.0 把
`NCharacterSelectScreen.SelectCharacter` 改成只接收按钮（`SelectCharacter(NCharacterSelectButton)`），
Harmony 绑不上这个参数就抛异常；我们此前用的是整程序集 `PatchAll`，一处失败整包回滚，
于是它的局内换装、背景绘制补丁一起失效，只剩走规范路径的头像还能换。

改为 `PatchAllTolerant`：逐个补丁类走 `CreateClassProcessor(type).Patch()`，装不上的类跳过并记警告，
其余照常生效，最后汇总一行「共 N 个补丁类无法应用（游戏接口变更），其余 M 个已生效」。
### 1.0.31.16 —— 「这个包带不带规范背景」改用包自身 PCK 判定

1.0.31.15 的检查用 `ReadProviderFile`，而它会在「包没有该文件」时命中游戏本体和我方缓存覆盖包里的
同名文件 —— 而 `res://scenes/screens/char_select/char_select_bg_defect.tscn` 这类规范场景在游戏本体里
**每个角色都有**，于是判定恒为真，仍然去替换背景，鸡煲背景依旧退回原版。

新增 `SkinCatalog.ProviderShipsFile(groupId, optionId, relativePath)`：只在「不是游戏本体
`SlayTheSpire2.pck`、也不是 `cache\` 下的覆盖包」的档案里找到该路径时才算「包自带」。背景守卫改用它。
### 1.0.31.15 —— 不自带选人背景场景的皮肤包，不再用原版背景覆盖

`DefectSkinMod`（鸡煲娘化）、`succubusIroncladHpWear`（魅魔战士）这类包没有
`res://scenes/screens/char_select/char_select_bg_<角色>.tscn`，它们用自己的补丁把背景画上去
（`DefectAnimatedBg` / `AnimatedBg` 之下）。此前重建选角展示时会走「按规范路径取场景」的流程，
资源解析在覆盖包里查不到该路径就退回游戏原版场景，于是原版背景被装上去，包自己画的那套被冲掉，
表现为「选人界面只剩下方头像不一样」。

现在重建前先问一句 `ProviderShipsCanonicalBackground`：该选择是否真的带规范背景场景。
不带就跳过 `TakeOverCharacterSelectSceneCache` 与 `ReplaceCharacterBackground`，只刷新图标、
重放呈现步骤并保留包自己的背景，同时打一行
`[选角背景] 分组=… 没有规范背景场景，保留皮肤包自己的背景。`。
带规范背景的包（贞德、A 类战损等）行为完全不变。

同时把 `[选角背景]` 诊断也接到这条分支上，便于确认背景容器里到底还剩哪些节点。
### 1.0.31.14 —— 加「选角背景」诊断日志（不改任何行为）

`DefectSkinMod`（鸡煲娘化）的选人背景不由规范路径提供，而是它自己的 DLL 补丁在选人界面塞一个
`DefectAnimatedBg`（场景 `res://Scenes/DefectSelectBg.tscn`）到 `AnimatedBg` / `BgContainer` 之下；
我们的重建如果把这个节点整段换掉，背景就会退回原版。这一版只记录现状：重建之后打印
`AnimatedBg` 与 `BgContainer` 的每个子节点名、类型、`可见`、`在树可见`、子节点数，据此判断
「节点被我们删了」还是「节点还在但被隐藏」。
### 1.0.31.13 —— 加「选角点击」诊断日志（不改任何行为）

1.0.31.12 让骨妹包重新量到背景并成功挂上点击接收器（日志 `char-select toggle attached (necrobinder, stage 1/3, silenced 0 skin handler(s))`），
但点击仍无反应。这一版只在重挂之后把接收器节点本身的情况打出来，用来区分「点击没送到」还是「送到了被它自己的条件挡回」：
节点路径、尺寸、位置、自身可见性、是否在树中可见、`MouseFilter`、父节点可见性，都写进 `[选角点击]` 一行。
`DBWCharSelectClickCatcher` 若尺寸为 0 或父节点不可见，就说明点击根本不会落在它身上。
### 1.0.31.12 —— 让血量换装族皮肤包在选人界面重新量取背景

necrobinderHpWear 这一族包给选人背景挂「点击切战损」时，会先量当前背景 SpineSprite 的图集页尺寸，
只有等于自己预设尺寸才接管；量测发生在包初始化那一刻，而我们的皮肤资源包要等用户选中后才挂载，
于是它们量到原版图集、判定「没装对应皮肤」并永久放弃（日志 `atlas page 9132x5386 != 4757x4757`）。

新增 `Core\CharSelectPackRebindCompat.cs`：每次重建选角展示后，按当前选择的提供者程序集名找到它的
`TryReattachFromScreen` 并再调用一次，让它重新量一次背景；带 700ms 节流，包内部仍用自己的
`SkinSelectionGate.MenuAllowed()` 决定是否接管，未选中的包不会响应。
### 1.0.31.11 —— 加「选角闸门」诊断日志（不改任何行为）

血量换装那一族皮肤包（魅魔战士、骨妹「明前奶绿」等）在选人界面自己挂 `NCharacterSelectScreen.SelectCharacter` postfix 做点击切战损，但被它们自己的三道闸门挡住时**一行日志都不打**。这一版只加观测：

- `Core\SkinService.cs` 的 `GetSelectedRuntimeProvider(groupId)` 在返回值变化时打 `[选角闸门] 分组=… 选择=… 是运行包=… 返回=…`（皮肤包反射调这个方法判断「当前选中的是不是我」，返回 null 或名字不匹配它们就整个关掉）。
- 新增 `Core\DriverGateProbe.cs`：选角小模型刷新后，反射扫描所有 `*HpWear*` 程序集里的 `BattleWear.SkinSelectionGate.MenuAllowed()` 与 `BattleWear.WearActivation.PeekActiveDriver()`，把实测布尔值打进日志；同一分组只打一次。
- 副作用：无（只读探测 + 一次 Info 日志）。诊断完成后整个 `DriverGateProbe.cs` 与这两行日志应当删除。
### 1.0.31.10 —— 撤销 1.0.31.9（贞德变体方案暂缓）

1.0.31.9 让变体 id 回退到基础 option 查包根，实际效果是把贞德的「变体 + 战损」两条链搅在一起、点击切换更乱。整段回退，`SkinCatalog` 回到 1.0.31.8 状态。

贞德这类「一个主体既有变体又有战损」的皮肤暂不处理，先解包其他单一形态的战损皮肤，弄清它们自己的运行原理。
### 1.0.31.9 —— 修「战损变体的选人背景/头像全都一样」（真因：变体 id 查不到包根）

设置变体的选项 id 是 `<mod>::variant:<key>`，它的资产列表里只有配色文件、没有 `s1_*`，于是 `GetAssetRootDirectory` 返回 null，`ReadProviderFile` 也拿不到包内文件——`VariantPortraits` 因此读不到 `portrait_palettes/<变体>/s1_select_icon.png`，四个变体全退回规范图。

改法：`SkinCatalog` 新增 `StripVariantSuffix` 与 `FindOption`，`ReadProviderFile` / `GetAssetRootDirectory` 在按变体 id 查不到时回退到基础 option id（并不改动任何选中值）。

贞德包里确认存在 `portrait_palettes/1..3/s1_{run,select}_icon.png`，索引 0 是包自己的默认外观、本来就没有目录。
### 1.0.31.8 —— 修「战损变体的背景/头像全都一样」

1.0.31.3 为了让战损档能读到 `s2_/s3_*_data.tres`，把 `ResolvePlan` 里的 `directory` 直接改成了包根目录。`directory` 同时被拿去拼 `portrait_palettes/<变体>/s<阶段>_select_icon.png` 与配色路径，于是四个变体的选人背景、选人头像全退回到同一张默认图——**同一个变量被两件事共用**，是这次倒退的根因。

改法（做减法，不加新条件）：

- `StagePlan` 拆出 `DataDirectory`（只用于找 `s<阶段>_<角色>_data.tres`），`Directory` 恢复为「当前骨架所在目录」，供 `portrait_palettes`/配色路径使用；
- 删除 1.0.31.7 的「包自命名背景兜底」（一次都没触发，日志里背景路径始终是规范路径），`SkinCatalog`/`ContextualSkinControls` 回到 1.0.31.5 的干净版本。

战损档点击仍从包根读数据文件，变体区分恢复。### 1.0.31.7 —— 选人背景：规范路径优先 + 包自命名兜底（排除别名）

1.0.31.4 的兜底把贞德弄坏了：它选到了 `res://custom/…` 这类只在挂载期有效的别名路径，实例化失败后背景直接没了。

这一版改成两条硬约束：

- 只有该皮肤包**不带**规范 `char_select_bg_<角色>.tscn` 时才启用兜底；
- 兜底时**拒绝** `res://custom/` 与 `res://sts2_skin_runtime/` 开头的别名路径。

同时把上一版写错的诊断字段（`包内自带背景` 恒为 False，因为把包根目录拼在了路径前面）换成直接输出最终选中的背景路径。
### 1.0.31.6 —— 皮肤缓存总量兜底 + 选角诊断日志

玩家磁盘被 %APPDATA%\SlayTheSpire2\Gurio.SkinChanger\cache 撑到 63 GB：缓存按「一次游戏会话一个目录」存放，清理只在正常退出时执行，崩溃/卡死/强退会整份留下，越积越多（单目录 1–4 GB）。

- Core\SkinChangerStorage.cs：新增 TrimCacheBudget(long maxBytes)，按最后写入时间从最旧开始整目录删除，直到总量回落到预算内；正在使用的会话目录永不删除。
- Core\SkinService.cs：启动清理后追加一次 TrimCacheBudget(8 GB)，并打出删除数量、释放量、剩余量。
- Ui\ContextualSkinControls.cs：RebuildCharacterDisplay 增加一行 [选角诊断]，输出分组、当前选择、规范背景路径、以及该皮肤包是否自带这份背景资源。纯日志。
### 1.0.31.5 —— 撤销 1.0.31.4 的选人背景识别放宽

1.0.31.4 把选人背景改成「先找包里的 `*selectbg*` 场景、否则退回规范路径」，实测把贞德的选人背景也弄丢了（回退后恢复），问题也没真正解决，因此整段撤销，回到 1.0.31.3 的选人背景逻辑。

- `Catalog\SkinCatalog.cs`：删除 `FindCharacterSelectBackgroundScene`。
- `Ui\ContextualSkinControls.cs`：`RebuildCharacterDisplay` 恢复只按 `res://scenes/screens/char_select/char_select_bg_<角色>.tscn` 取背景。

结论记录：故障机器人（DefectSkinMod）的背景用的是它自己命名的 `res://Scenes/DefectSelectBg.tscn`，靠「放宽文件名匹配」这条路走不通，需要按包注册（provider contract）显式声明背景场景，另开改动。
### 1.0.31.4 —— 选人界面背景识别放宽（支持包自己的场景命名）

v0.113.0 之后，故障机器人（DefectSkinMod）这类把选人背景放在自己命名下（es://Scenes/DefectSelectBg.tscn）的皮肤包，选人界面一直被换回原版背景：我们只认游戏的规范路径 es://scenes/screens/char_select/char_select_bg_<角色>.tscn。

- Catalog\SkinCatalog.cs：新增 FindCharacterSelectBackgroundScene(groupId, optionId, characterId)，优先用规范名，找不到时在该选项自己的资源里挑 *selectbg* / char_select_bg_* 的 .tscn（同名多份时取路径最短的一个）。
- Ui\ContextualSkinControls.cs：RebuildCharacterDisplay 里背景路径先问上面这个方法，取不到再退回规范路径；后续的资源收集、整包快照重建都沿用同一个路径。

贞德（JoanOfArcIroncladHpwear）两份资源本来就在规范路径上，走的是第一个分支，行为不变，可作回归基准。魅魔战士（succubusIroncladHpWear）包里根本没有选人背景场景，这一版不会让它凭空多出背景。
### 1.0.31.3 —— 修「商人皮肤刷新报『独立皮肤资源不是场景』」
根因：v0.113.0 把商店房间场景从 `res://scenes/rooms/merchant_room.tscn` 拆成 `res://scenes/rooms/standard_merchant_room.tscn` 与 `res://scenes/rooms/blitz_merchant_room.tscn`，旧路径在新版游戏包里已不存在。本 Mod 的 `MerchantRoomScenePath` 仍是旧路径，运行时 overlay 里取不到 PackedScene，于是在 `SkinService.InstantiateRuntimeScene` 抛 `InvalidOperationException: 独立皮肤资源不是场景：res://scenes/rooms/merchant_room.tscn`，面板里切换商人皮肤一律失败。
- `Ui\MerchantRuntimeAppearance.cs`：新增 `StandardMerchantRoomScenePath` / `BlitzMerchantRoomScenePath` 与 `ResolveMerchantRoomScenePath(NMerchantRoom?)`（按活体房间类型名选场景，`NBlitzMerchantRoom` 走 blitz，其余走 standard）；`MerchantRoomScenePath` 保留为 standard 默认值，供预览预热与无房间实例的场景使用。
- `TryCreateCurrentMerchantRoom` 改用 `ResolveMerchantRoomScenePath(preloadCreatedRoom)`，主路径与兜底路径都用同一个场景。

### 1.0.31.2 —— 修「商人皮肤无法切换」
根因：v0.113.0 把商店房间拆成 `NStandardMerchantRoom` / `NBlitzMerchantRoom`，`NMerchantRoom.MerchantButton` 变成 **abstract 属性**，真正的 `<MerchantButton>k__BackingField` 只存在于具体子类。本 Mod 仍按 `typeof(NMerchantRoom)` 取该字段 → 反射得到 null，`TryRefreshMerchant` 直接以 `merchant visual fields unavailable` 退出，面板里点商人皮肤不生效。同时两个子类都覆写 `_Ready()` 并在第一行调 `base._Ready()`，挂在基类上的作用域捕获早于子类建好自己的 `%MerchantButton` / `%Inventory`。
- `Ui\MerchantRuntimeAppearance.cs`：新增 `ResolveMerchantButtonField(object)`，按实例真实类型（含基类链）缓存解析 `<MerchantButton>k__BackingField` 或 `_merchantButton`；预览、刷新、回滚、预览原生引用四处改用它，不再依赖基类的静态字段。
- 新增 `MerchantRoomConcreteReadyPatch`：同时挂 `NStandardMerchantRoom._Ready` 与 `NBlitzMerchantRoom._Ready` 的 `Priority.Last` postfix，等具体房间完成自身初始化后再跑一次 `ScopePartTakeover`。

### 1.0.31.1 —— 让「撑在地上的鸡煲」继续可用（奥斯提皮肤包兼容层）
根因：工坊 `3820801911`（`OstyDefectImpostor`）把全部补丁塞进一次 `Harmony.PatchAll()`，只要有一个目标解析不到，它的 catch 就 `UnpatchAll()` 把自己整个停掉。v0.113.0 把 `NCreature.OstyScaleToSize(float)` 改名成 `ScaleAndPositionOsty(float, double)`，它仍按旧名打补丁 → 日志 `[OstyDefectImpostor] Patch unavailable; original visuals retained`，皮肤选项随之消失。
- `Core\OstyDefectImpostorCompat.cs`（新增）：在 `AccessTools.Method(Type, string, Type[], Type[])` 上挂 postfix，只有当查找结果为 null、且请求的正是 `NCreature.OstyScaleToSize` 时，才返回改名后的 `NCreature.ScaleAndPositionOsty`；其它任何查找原样放行。
- 定位该重载必须用 `typeof(AccessTools).GetMethod(..., [typeof(Type), typeof(string), typeof(Type[]), typeof(Type[])], null)`：`AccessTools` 有两个同名 `Method` 重载，用 `AccessTools.Method(typeof(AccessTools), ...)` 去查会二义性命中失败、静默返回 null。
- 类型判定按 `FullName + 程序集名` 比较，不用 `typeof(NCreature)` 引用比较：每个模组各有 AssemblyLoadContext，同一个游戏类型在不同 ALC 里是不同的 `Type` 实例，引用比较永远不相等、重定向不会触发。
- `Entry.cs`：在 `PengoTarotFontCompat.Install(harmony)` 之后调用 `OstyDefectImpostorCompat.Install(harmony)`，早于任何第三方模组的初始化器。

### 1.0.31.0 —— 适配游戏 v0.113.0（修「启动即致命错误」）
根因：v0.113.0 删除了 `MegaCrit.Sts2.Core.Models.SingletonModel`。本 Mod 挂在 `OneTimeInitialization.ExecuteEssential` 上的补丁体直接引用该类型，补丁方法 JIT 时抛 `TypeLoadException`，游戏一启动就进「致命错误」画面（日志栈 `OneTimeInitialization.ExecuteEssential_Patch3` ← `NGame.GameStartup`）。
- `Core\ProviderModelDiscovery.cs`：改用 `typeof(ModelDb).Assembly.GetType("MegaCrit.Sts2.Core.Models.SingletonModel", throwOnError:false)` 反射取类型；缺类型时只打一次日志并跳过「给皮肤包补注册单例模型」，改由 `ModelDb` 自身类型发现负责。补丁体不再持有该类型的编译期令牌。
- `Ui\StatefulCardCosmeticPatches.cs`：删除已消失的 `MegaCrit.Sts2.Core.Models.Singleton` 命名空间引用。
- `Ui\MerchantRuntimeAppearance.cs`：`NMerchantRoom.Create` 已删除 → 房间补丁改挂 `NMerchantRoom._Ready`（「丢弃启动预加载房间并按选择重建」随静态工厂入口一起退役，已留注释；玩家外观仍由同文件 `MerchantRoomPlayerAppearancePatch` 负责）。`MerchantInventory.AddRelicEntry` 已删除 → 假商人预览改用 `MerchantInventory.CreateForNormalMerchant(previewPlayer)`。
- `Ui\CharSelectStageCycle.cs`、`Ui\ContextualSkinControls.cs`：`NCharacterSelectScreen.SelectCharacter` 只剩 `NCharacterSelectButton` 一个参数 → 改从 `charSelectButton.Character` 取角色（游戏内部本来就这么取），补丁类恢复安装。
- `Core\MultiplayerSkinSync.cs`：`StartRunLobby.ChangeCharacter` 形参改名 `isRotatingDread`（语义为轮转恐惧），该值只用于日志措辞。
- 启动期健壮性：`Entry.GetLoadableTypes` 提为 `internal`，`ModelTypeCompatibility` 枚举 GameAssembly 类型改走它；`EssentialInitializationPatch` 的前置/后置步骤各自 try/catch 并打 Error 日志。第三方包引用已删除类型时，异常不再顺着启动流程把玩家挡在「致命错误」外。
- 新增 `Core\OstyDefectImpostorCompat.cs`：工坊 `3820801911`（撑在地上的鸡煲）仍按旧名 `NCreature.OstyScaleToSize` 打补丁，失败即 `UnpatchAll` 自己。这里给 `AccessTools.Method` 挂后置，把旧名请求重定向到 v0.113.0 的 `ScaleAndPositionOsty`；取重载用精确反射，避免同名重载二义性返回 null。
- 战损档：`Ui\CharSelectStageCycle.cs` 的 `ResolvePlan` 改用皮肤包根目录（`SkinCatalog.GetAssetRootDirectory`）拼 `s<stage>_<group>_data.tres`，修掉「点角色切战损」每次都读到 0 个资源的问题。
- 按作者要求，撤销此前为工坊 `3747527467` 做的延迟注册/作用域门闩等兼容改动（7 个文件回到 .30.200 状态），只保留本次版本适配。
- 验证：构建 0 错误（仅 3 个既有 CS8601）、LogicTests 末行 `Skin Changer logic policy tests passed.`、启动进主菜单显示 v0.113.0、日志无「补丁类安装失败」、贞德变体与战损档点击切换恢复（作者实测确认）。
---

## 2026-11-12

### 1.0.30.201 —— 撤销 .200 的完整性放宽（当时被误判）
- 背景：作者复测后报「奥斯提=Orchis 时骨妹本体也被换成 Orchis」，据此把启动完整性判定改回严格版 `IsFullRuntimeProviderFullySelected`。
- 真相（同日晚确认）：串皮与这条判定无关，是工坊 `3759170855`（STS2-OrchisNecrobinderSkinFix）被 Steam 重新下载并启用，它声明依赖 Orchis，触发"被依赖则交回原加载器"的安全阀，Orchis 因此全局 `PatchAll`。
- 代价：严格判定会让「骨妹与奥斯提不同款」的组合每次启动都把角色皮肤重置成原版。DLL 前缀 `FBB05DC56986`。

### 1.0.30.200 —— 精准修「退出重进变原皮」
- 根因：启动清理把随从分组的选择算进"完整运行时提供者是否完整"，`necrobinder=Orchis` + `osty=别的` 时 Orchis 被判不完整 → 卸载补丁 + 重置选择（日志 `已将 … 的 1 个不完整联动选择恢复为游戏原版`）。
- 改法：判定改用 `Catalog.IsFullRuntimeProviderSelectedIgnoringCompanions(providerId, Config.Selections)`（随从分组有自己的挂载事务，不参与角色提供者的完整性判定）；只动这一行判定。
- 验证：重启后配置保持、无「不完整联动选择」；骨妹 = HpWear、奥斯提 = Orchis，无串皮。DLL 前缀 `25D454CC2125`（后来按同一逻辑重编为 `0251B42C98E7`）。

### 1.0.30.199 —— 启动完整性判定（第一版）/ 选角图标缺失预检
- 就是把 .200 的判定改动 + `LoadCharacterSelectIcon` 的 `ResourceLoader.Exists` 预检一起做的那版；预检用于修 `char_select_random_character.png` 每帧报错拖到界面无响应。DLL 前缀 `007E9809680B`。

### 1.0.30.198 —— Orchis 本体头顶蓝火（局内）修好
- 节点：`Visuals/HeadBoneNode/SteppedFireMix_dark`（原版 `necrobinder.tscn` 就有；`NNecrobinderVfx` 只负责显隐）。
- 根因：`SkinService.ApplySelectedVisualPostfix` 的 `allowed` 要求"完整运行时提供者必须全组选中"，局内 osty 分给「撑在地上的鸡煲」时 Orchis 被判不完整，骨妹分组的视觉后处理整体不回放。
- 改法：① `allowed` 只看本组选择；② `TryRebuildCreatureVisuals` 在挂载后重放一次 `ApplySelectedCreatureVisualPostfix`（热重建实例不在游戏创建路径上）。
- 验证：`后置重放前后 HeadBoneNode：False → False`（修前 `True → True`），截图蓝火消失。DLL 前缀 `74E84AAE0273`。

### 1.0.30.197 —— 奥斯提坐标被 Orchis 摆位补丁带走
- 反编译 Orchis：`NCreature.OstyScaleToSize` 的 Prefix 会把宠物根节点 tween 到主人身旁，`Creature.CreateVisuals` 后置还会加 `OstyCombatPositionOffset`。
- 改法：`MonsterRuntimePresentationPolicy` 增加 `MonsterVisualCallback.OstyPositioning`；`ManagedSkinModLoader.ProviderOwnsOstyPet(providerId)` 判定；`GetSelectedCreatureProviderIds` 在宠物组自带提供者时不再重放主人的 `NCreature._Ready` 行为。
- 结论：静止 18 s 根节点恒定，结束回合波动 ≤0.7 px——此前的"漂移"是切换皮肤后重建身体的滑入缓动。DLL 前缀 `ADF4844F0F8C`。

### 1.0.30.196 —— 三项小修
- `RefreshCompanionGroup`：osty 选「游戏默认」时先 `RebuildSummonBodyForImpostor` 再返回，显示真正的原版身体。
- 局内伙伴下拉去掉「随机皮肤」项。
- 回退 .195 的「整包已在栈顶就跳过提权」——它会让分层覆盖层把整包反压下去，Orchis 的 VFX 变回原版。DLL 前缀 `B55A65605044`。

### 1.0.30.195 —— 名字守卫 + 联机重建去重
- `CompanionDisplayNameGuard`：`LocString.GetFormattedText` 的 Postfix，只处理 `OSTY.name`；osty 选 midori 时保留「坚果」，否则从游戏原包 `res://localization/<lang>/monsters.json` 读回「奥斯提」（第三方改的是本地化表原文，`GetRawText` 拿不到原名）。
- `MultiplayerSkinSync.ReconcileRemoteAppearances` 按选择签名去重（`LastAppliedSelectionSignatures`）。DLL 前缀 `1C71092D424F`。

### 1.0.30.194 —— 修「鸡煲显示原皮」
- 根因：.193 的别名兜底自己 `Instantiate` 原版身体，绕过了 `OstyDefectImpostor` 挂在 `MonsterModel.CreateVisuals` 上的 Postfix。
- 改法：别名场景只用来 `PreloadManager.Cache.SetAsset` 登记到规范路径（`ForceCanonicalSceneFromAlias`），身体仍走 `CreateVisuals`，独立 Mod 才能接管。DLL 前缀 `EE0E7DD58D83`。

### 1.0.30.193 —— 别名兜底（当时以为修好了「猫 → 鸡煲」）
- `EnsureGameSceneOnTop` + `ProbeSceneHasSpine` + 私有别名路径 `res://__skin_changer_base/{hash}/{file}.tscn`；包名每次递增，避免同名 `LoadResourcePack` 不重新压栈。DLL 前缀 `3AE9F0004A8F`。

### 1.0.30.192 —— 第三个根因：按路径的资源缓存
- 结论：`AssetCache`（`PreloadManager.Cache`）按原样路径字符串长期持有，`ResourceLoader.CacheMode.Reuse` 同理；猫形态一旦进缓存，后面压包栈/写覆盖包都没用。
- 改法基础：`EnsureGameSceneOnTop` 写 `base_scene_{hash}_{seq}.pck` + `CacheMode.Replace` + `SetAsset`；`ProbeSceneHasSpine` 判断规范路径是否已真是原版 Spine。

---

## 2026-11-11

### 1.0.30.191 —— 校正包装配顺序：选中项覆盖把提权包的猫壳写回
- 复现规律：进游戏直接选「撑在地上的鸡煲」正常；先选猫再切鸡煲显示猫；先选别的再切鸡煲正常。
- 改法：`LargeProviderMountPlan` 带 `PromotedProviderIds`；选中项覆盖之后，对"属提权包、属游戏原包、且归属分组当前没选这个提供者"的路径用 `BuildGameBaselineOverlay` 写回（日志 `[提权] 已按游戏原包还原 N 条其它分组的规范路径`）；缓存刷新条件改为 `overlayPlanRebuilt || mountedDeltaFiles > 0`。DLL 前缀 `8F72FC9168DF`。

### 1.0.30.190 —— 场景缓存是第二根因（没修好）
- 新增 `RefreshCachedCreatureVisualScenes`：对覆盖层里的 `/creature_visuals/*.tscn` 用 `CacheMode.Ignore` 重读并 `SetAsset`。
- 结果：缓存确实刷了，但刷到的还是猫（选中项覆盖写回），留给 .191。DLL 前缀 `A1B93E01E4E6`。

### 1.0.30.189 —— midori 的奥斯提：预览 + 抢占鸡煲（第一轮）
- 依赖闭包允许空格：`SkinCatalog.EmbeddedResourcePathRegex` 不再把空格当终止符，`catimage/猫召唤 2/**` 回到闭包（600 → 648 文件），修掉 `No loader found resource: …猫召唤 2_00000.png`。
- 预览框支持场景型奥斯提：`Ui/OstyModelPreviewControls.cs` 增加 `TryLoadSceneForCurrentSelection / TryShowScene / MeasureSceneBounds / FitScene`。
- 校正包只按游戏原包还原提权路径（`BuildGameBaselineOverlay` 限定 `_baselineIndexes[0]`）。DLL 前缀 `6D61E87DCB60`。

### 1.0.30.188 —— 上述三项的开发版（问题单第 18 节起点）
- 与 .189 同一批改动，.189 为对外交付版本。

### 1.0.30.187 —— 收尾清理
- 删掉临时性能探针与调试残留，保留 `[外观诊断]` 一族日志与文档。四个部署位置一致，DLL 前缀 `64FAA0383C70`。

### 1.0.30.186 —— 每切一次写 630 MB → 分层增量
- 覆盖包改成增量层（`SkinService.MountOverlayDelta`）：换肤 49–171 ms、多数情况 0 写盘，修掉连续切换时的闪退/磁盘打满。

### 1.0.30.185 —— 随从换肤比例收尾 + 两条新发现
- 9.5-2 结案：代码改了，但"换随从皮肤偏小"在当前包里无法复现。
- 新发现（未修）：局内把奥斯提切到 `midori_black` 会把场景加载炸掉。

### 1.0.30.184 —— 9.5 遗留收尾
- Yuzuki 包作废、随从换肤不再沿用旧基准缩放。

### 1.0.30.183 —— 全量重建复测
- 冷启动 2.1 s / 热缓存 106–135 ms / 换一个提供者 15 ms ⇒ 结论：热缓存有效，不做"分组产物缓存"。

### 1.0.30.182 —— 拆分批量激活
- 启动时把"立即激活"与"延后激活"拆开（`DeferredProviderActivator`），启动阻塞 6.8 s → 3.45 s。

### 1.0.30.179 / .180 / .181 —— 性能探针、预加载、依赖闭包缓存
- `.179`–`.181`：`Core/PerfTrace.cs` 性能探针、`Core/SkinPreloader.cs` 预加载、依赖闭包缓存。

### 1.0.30.178 —— 覆盖包跨会话缓存
- 新增 `Core/OverlayCacheStore.cs`，覆盖包命中缓存时不再重新组装。

### 1.0.30.177 —— 随从兼容通用化 + 加载性能 + 写盘兜底
- 新增 `Core/SummonModCompat.cs`：随从皮肤只分"资源型 / 运行时型"两种情况，统一处理。
- 加载性能优化；磁盘满时不再崩溃（写盘兜底）。

### 1.0.30.176 —— 两个包的奥斯提部分补完
- 改 `Catalog/SkinCatalog.cs`：补齐场景依赖闭包与"随从私有场景映射"，让只提供私有路径皮肤的包（Yuzuki 等）也能被 osty 分组识别。
- 工具修复：`CatalogInspect --visual-overlay` 统计口径。DLL 前缀 `DD1FA37F1A53`。

### 1.0.30.175 —— 一批"外观显示≠实际"的修复
| 问题 | 修法 |
| --- | --- |
| 骨妹被强制变成 Orchis | 环境侧禁用 `3759170855`；代码侧 `GetActiveRuntimeProviders` 排除"只在随从分组被选中"的提供者 |
| 面板数值与实际模型不符、重置没反应 | 重置时一并删 `GroupTransformMemoryKey`（`__group_default__`） |
| 换肤后面板绑定旧节点 | `CharacterAppearanceScreen.RefreshTargetCreatureReference()` |
| 面板被手牌遮挡 | 面板移入 `CanvasLayer(105)` |
| 局内奥斯提下拉项比选人界面少 | 补 `OstyImpostorBridge.Options` 与"随机皮肤" |
| 局内切奥斯提无效 | 改走 `ApplyCompanionSelection`；独立 Mod 皮肤不重建；重建后 `ReapplyCompanionImpostorSkin()` |
| 战斗中奥斯提点不到 | 「下一个目标 ▶」按钮 `SelectNextAppearanceTarget` |
| 奥斯提 Orchis 皮肤带原版火焰 | `ApplySelectedVisualPostfix` 对随从分组只看本组选择 |
| 「撑在地上的鸡煲」不显示 | `OstyImpostorCompat.OwnsSummonBody()` 两条重建路径提前 return |
| 局内 Orchis → 鸡煲不即时 | `RebuildSummonBodyForImpostor()` + `OstyImpostorBridge.RefreshLiveSummons()` |
| 选角预览里 Orchis 本体带原版火焰 | `FrameworkModelPreview` 补 `ApplySelectedCreatureVisualPostfix` |

---

## 2026-11-10

### 1.0.30.167 —— 深夜：Orchis 全局补丁真因 + 随从视觉后处理
- `OrchisNecrobinderSkinMod.dll` 在 `Init` 里 `PatchAll()`，`Creature.CreateVisuals` 后置无条件换骨；它能绕过隔离是因为 `3759170855` 的 manifest **声明依赖它**（安全阀"被依赖则交回原加载器"）→ 禁用该 Fix Mod 后日志恢复 `已隔离皮肤提供者 Orchis…原 PCK 未全局挂载`。
- `ApplySelectedVisualPostfix` 对随从分组不再要求"全部分组选中"。
- 重建召唤物后必须再 `OstyImpostorBridge.ApplyQuiet` 一次（`ReapplyCompanionImpostorSkin`），否则精灵被重建覆盖。
- 遗留：随从组选「游戏默认」时基线资源数=0。

### 1.0.30.161 —— 「重置没反应」真凶 + 随从局内可换肤
- `SetCharacterCombatTransform` 写非默认值时同时写 `__group_default__`，重置只删 `options[optionId]` ⇒ 面板显示 100% 而模型还是旧值；改为默认值分支一并删组级记忆。
- 新增 `SkinService.CanSelectCompanionSkin`（解耦后允许随从换肤），选择走 `ApplySelection("osty", …)`。

### 1.0.30.159 —— 骨妹本体被换回旧包 + 滑条不动 + 邪教徒纹理
- `BuildOverlay` 恢复无条件基线回写；新增 `RequestFullOverlayReassert`（多分组提供者被替换/降级为随从专用时，下次重建自动扩成全部分组，日志 `外观一致性保护：…`）。
- `CharacterAppearanceScreen.RefreshTargetCreatureReference()` 重新绑定活节点。
- 新增 `Core/ProviderRuntimeCacheReset.cs`：选择变化时清空提供者程序集里的静态缓存（名字含 cache/applied/hurt）。
- 新增 `[外观诊断]` 日志族。

### 1.0.30.158 —— 随从包泄漏 + 外观面板缩放/重置
- 新增 `SkinCatalog.IsCompanionOnlyProviderSelection`：只被随从分组选中的提供者做 `replaceFiles:false` 低优先级挂载、永不置顶，`BuildOverlay` 跳过它的基线回写。
- 选择框改成绕矩形中心按 `size` 缩放；`OnTransformChanged` 排队期间先存值、`OnQueuedSelectionFinished` 再应用；设置面板移入 `CanvasLayer(105)`。

### 1.0.30.157 —— 双贞德包并存 + 外观变体 + 点角色切战损
- 两个贞德包（0.3.5-S / 0.3.6）可并存：`ProviderMenuGateCompat` 放行同契约变体的门控，`VariantHotkeyPoller` 以 0.05 s 轮询接管快捷键。
- 外观变体改为"设置型变体"；局内外观同步；已知缺口：选人头像不随战损档变化。

### 1.0.30.138 —— 回退一个有副作用的优化
- 某个省 ~1 ms 的优化会导致选人背景/预览/局内模型颜色不更新，已回退。

### 1.0.30.125 / .124
- `.125` 局内左上角头像随变体（a/b）变化，实机通过。
- `.124` 爆衣：b 在 16/80 时进入第 2 档，实机通过。

---

## 2026-11-09

### 1.0.30.107 —— 战士贞德双包（0.3.5-S / 0.3.6）兼容
- 区分"外观差分快换"（包自己设置，默认 `=`）与"点角色切形态"（`CharSelectCycle`，仅 b 有），避免把后者当成前者。
- a 的快捷键本来被包自己的 `SkinSelectionGate` 关掉；用 `ProviderMenuGateCompat` 放行，`CharSelectCycle.Attach/OnInput` 前缀保证点击轮换只属于包自己。
- `Core/VariantHotkeyPoller.cs` 接管快捷键（UI 会吃掉 `_input`），只在该包的变体家族里前进一格。

### 1.0.30.102 —— 骨妹四档血量换装（necrobinderHpWear）修复 + 奥斯提独立下拉
- 骨妹四档：根因是在"运行中的" SpineSprite 上换骨架，引擎要断上千条连接（实测 1182/2554/3487）→ 原生崩溃。改为 `pristine.Duplicate()` 预建独立精灵、进树前设好骨架、换档只切 `Visible`；新增 `BattleWear/SpineWrappers.cs` 用原生 `Variant` 调用即时释放。
- 皮肤切换器：新增「奥斯提」独立下拉 + 右侧预览框，把角色皮肤与奥斯提皮肤解耦；修选人头像串图、Orchis 背景动画不播。
- 把"拦截第三方在选人界面接管外观"的补丁提取成通用件。

---

## 2026-11-07

### 1.0.27.88 —— 垫层按相机缩放反向补偿
- 帝皇蟹房 `GetCameraScaling()=0.75`，canvas 0 的垫层只盖 75%；改为"垫层树里没有 `CanvasLayer` 时才按 `1 / GetCameraScaling()` 反向缩放"，有 CanvasLayer 的包不动。其它缩放 0.85/0.9 的 Boss 一并修好。

### 1.0.27.87 —— 别抽图层，整块垫在本体之后
- `.86` 只替换 `Layer_XX` 槽位导致帝皇蟹上半身被背景盖住（CZN 战场在 `CanvasLayer -1`，天空 z_index 2400+）。
- 改为：保留选中战场的完整节点（含它自己的 CanvasLayer），插到遭遇背景第 0 位，隐藏遭遇自己的 `Layer_XX` 布景，保留本体与 `Foreground`。

### 1.0.27.86 —— 尝试只换图层（失败，被 .87 取代）

### 1.0.27.85 —— 帝皇蟹 Boss 房卡死
- 根因：「背景即本体」的 Boss（`EncounterModel.HasCustomBackground`）被背景功能当成风景 `QueueFree()`，`Crusher.AfterAddedToRoom` 撞上已释放对象，战斗初始化中断。
- 修法：遭遇自带背景一律不替换（`EncounterOwnsBackground` / `CurrentEncounter`，三处入口统一走 `CanApplyActBackground`）；`CombatCrabSkinPatch` 写钳子引用前校验节点有效性。

### 1.0.27.84 / .83 —— 两项在干净树上重做
- `.84` `RemoveStaleProviderCharacterSelectControls` 增加 `character` 参数，运行时提供者跳过"清理 CharacterSelectOptionsPanel"（CZN 背景切换工具条）。
- `.83` `ShouldDeferCharacterIconResourceToExternalRuntime` 去掉 `GetRuntimeImagePath != null` 要求，非资源型运行时提供者一律让位（CZN 选人栏/顶栏头像）。

### 1.0.27.82 / .78
- `.82` `FrameworkSelectPresentationBridge.Reapply(...)` 调用；`.78` `VariantSceneAvailable` 守卫；两者是后来重做 .83/.84 的干净基线。

---

## 怎么回退

```powershell
cd 'D:\Storage\Desktop\工作2'

git log --oneline                     # 找版本对应的提交
git show <哈希> --stat                # 看那次动了哪些文件
git revert <哈希>                     # 撤销某次提交（推荐，历史不断）
git checkout <哈希> -- <文件路径>      # 只把某个文件恢复到旧版本
git checkout v1.0.30.200              # 回到某个标签看代码（只读，改之前先回 main）
```

回退代码之后必须：重建 → `LogicTests` 通过 → 复制到 4 处部署目录 → 重启游戏确认 `godot.log` 里的 `内测版本` 与提交相符。
2026-11-12 之前的版本没有 git 标签，只有本文件与交接文档的记录；那几天要"回退"只能改代码或改用 `D:\工作2026.11.10`、`D:\工作2026.11.11` 里的历史安装包。
















