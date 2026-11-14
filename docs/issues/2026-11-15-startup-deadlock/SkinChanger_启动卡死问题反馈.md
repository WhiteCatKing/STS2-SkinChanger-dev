# SkinChanger 卡死问题反馈(启动黑屏 / 切换皮肤死锁 / 退出卡死)

> 反馈日期:2026-11-14
> 涉及 Mod:皮肤切换器-Skin Changer(`Gurio.SkinChanger`,Steam 创意工坊 `3787302680`)

## 00000. 第五轮:切换皮肤时死锁、点"退出"时卡死(2026-11-14 19:10 更新,最新)

> 第四轮之后(14:46)我订阅了 **Mod Launch Manager**(创意工坊 `3788556097`),后来发现它每次启动都会用自己存的旧方案(`restored canonical launch plan`)覆盖游戏里的 Mod 开关。日志 01~17 里没有它的运行记录,所以前几轮不受影响。这一轮开始前它已经被禁用,下面每次的 Mod 组合都和日志开头的 `New sorting order` 一致。
>
> - 这一轮 SkinChanger 的版本是 manifest `version = 1.0.31`、`internal_test_version = 1.0.31.84`。日志原文:`[SkinChanger] 内测版本 1.0.31.84；程序集=Gurio.SkinChanger 1.0.31.84；路径=D:\TOOL\steam\steamapps\workshop\content\2868840\3787302680\Gurio.SkinChanger.dll`(日志 19~22 都是这个版本)。
> - 这一轮**没有再出现启动黑屏**:每次启动都能正常进主菜单。新出现的是下面两种卡死,性质和启动黑屏一样,都是进程死锁:`Responding=False`,CPU 几乎不增长,日志停止写入。

### 1) 每次启动的情况

所有启动都开着 RitsuLib、BaseLib、ModConfig 等基础 Mod。卡图包的开关见"配置"一列。

| 序号 | 启动时间(PID) | 配置 | 我做了什么 | 结果 | 材料 |
|---|---|---|---|---|---|
| — | 17:40、17:46 | SkinChanger **开**,12~13 个 Mod(只有基础 Mod 和 2 个卡图包,没有角色皮肤包) | 进主菜单后退出 | ✅ 2 次都正常退出 | 原始日志已被轮换删除 |
| **18** | 17:58(13964) | SkinChanger **开**,20 个 Mod,含 7 个角色皮肤包(`ironcladHpWear`、`succubusIroncladHpWear`、`JoanofArcIroncladHpwear`、`necrobinderHpWear`、`Moe-Necrobinder`、`regentHpWear`、`defectHpWear`) | 在选角界面把战士从 `ironcladHpWear` 切到 `JoanofArcIroncladHpwear::variant:2` | ❌ **切换的瞬间死锁** | 日志节选 18、`diagnostics/18_*` |
| — | 18:05(21692) | 同 18 | 切了 4 次贞德(原版、variant 1、variant 3),还切了魅魔、储君、Moe-Necro、机器人,**都没卡**;玩了约 8 分钟后点"退出" | ❌ **退出卡死**,这一局 `Common` 预加载**有完成** | `diagnostics/Q1_*`(原始日志已被游戏轮换删除) |
| — | 18:17 | 同 18 | 具体操作没有记录 | ✅ 正常退出 | 原始日志已被轮换删除 |
| — | 18:22(22504) | SkinChanger **开**,34 个 Mod(在 18 的基础上加了 14 个卡图包) | 进主菜单后马上进选角,玩了约 8 分钟后点"退出" | ❌ **退出卡死**,这一局 `Common` 预加载**没有完成** | `diagnostics/Q2_*`(原始日志已被轮换删除) |
| **19** | 18:39 | SkinChanger **关**,33 个 Mod(其余同上一行) | 进选角,约 1 分钟后退出 | ✅ 正常退出 | 日志 19 |
| **20** | 18:51 | 同 19 | 同 19 | ✅ 正常退出 | 日志 20 |
| **21** | 18:59(29948) | SkinChanger **开**,31 个 Mod,**每个角色只留 1 个战损皮**(战士 `succubusIroncladHpWear`、骨妹 `necrobinderHpWear`、储君 `regentHpWear`、机器人 `defectHpWear`;猎人原版) | 没切皮肤,约 1 分钟后退出 | ✅ 正常退出 | 日志 21 |
| **22** | 19:03(26348) | SkinChanger **开**,34 个 Mod(在 21 的基础上加回 `ironcladHpWear`、`JoanofArcIroncladHpwear`、`Moe-Necrobinder`,选择不变) | 没切皮肤,约 1 分钟后退出 | ❌ **退出卡死**,`Common` 预加载**没有完成** | 日志 22、`config/mods_list_第五轮_34个Mod.md` |

- **汇总**:这一轮 SkinChanger **开着**时,退出 7 次有 3 次卡死,3 次都出现在装了角色皮肤包之后;另外还有 1 次是切换皮肤时卡死。SkinChanger **关掉**时,退出 2 次都正常。不过关掉的那 2 次都只玩了 1 分钟左右,样本太少,不能下结论。
- 日志 18 是我当时读取日志的节选:原日志第 880~1195 行,每行截到 260 个字符。完整日志已经被游戏轮换删除了。

### 2) 切换皮肤时死锁(日志 18)

日志最后几行:

```text
[INFO] [SkinChanger] 开始应用皮肤选择：ironclad → JoanofArcIroncladHpwear::variant:2。
[INFO] [SkinChanger] 分组 ironclad 的皮肤 ironcladHpWear 被 JoanofArcIroncladHpwear 接管，退回原版。
...
[INFO] [SkinChanger] [性能] 预加载排队 4 个皮肤场景（队列上限 48）。
...
[INFO] [SkinChanger] 开始重建选角预览：ironclad。
[INFO] [SkinChanger] 已刷新 IRONCLAD 的角色列表头像。
ERROR: No loader found for resource: res://images/packed/character_select/char_select_random_character.png (expected type: unknown)
[INFO] [JoanofArcIroncladHpwear] char-select: click catcher + poller installed.     ← 之后再没有输出
```

用 `dotnet-stack` 抓到的主线程(`diagnostics/18_dotnet-stack_切换贞德皮肤死锁_PID13964.txt`;其他托管线程都在线程池里空闲等待):

```text
Thread (0x5EC0):
  [Native Frames]
  GodotSharp!Godot.ResourceLoader.Load(class System.String,class System.String,value class CacheMode)
  GodotSharp!Godot.GD.Load(class System.String)
  JoanofArcIroncladHpwear!JoanofArcIroncladHpwear.SpineSwap.LoadStageData(class System.String,int32)
  JoanofArcIroncladHpwear!JoanofArcIroncladHpwear.CharSelectCycle.GetStageData(int32)
  JoanofArcIroncladHpwear!JoanofArcIroncladHpwear.CharSelectCycle.Apply(...)
  JoanofArcIroncladHpwear!JoanofArcIroncladHpwear.CharSelectCycle.Attach(class Godot.Control)
  JoanofArcIroncladHpwear!JoanofArcIroncladHpwear.CharSelectCycle.OnCharacterSelected(...)
  JoanofArcIroncladHpwear!JoanofArcIroncladHpwear.CharSelectPatch.Postfix(...)
  System.Reflection.MethodBase.Invoke(...)
  Gurio.SkinChanger!STS2SkinChanger.Core.ManagedSkinModLoader.ReplaySelectedCharacterPresentation(...)
  Gurio.SkinChanger!STS2SkinChanger.Ui.ContextualSkinControls.ReplaySelectedCharacterPresentation(...)
  Gurio.SkinChanger!STS2SkinChanger.Ui.ContextualSkinControls+<>c__DisplayClass114_0.<RebuildCharacterDisplay>b__0(...)
  Gurio.SkinChanger!STS2SkinChanger.Core.SkinService.WithRuntimeResourcesForSelection(...)
  Gurio.SkinChanger!STS2SkinChanger.Core.SkinService.WithRuntimeResources(...)
  Gurio.SkinChanger!STS2SkinChanger.Ui.ContextualSkinControls.RebuildCharacterDisplay(...)
  Gurio.SkinChanger!STS2SkinChanger.Ui.ContextualSkinControls.RunRefresh(class System.Action)
  Gurio.SkinChanger!STS2SkinChanger.Ui.ContextualSkinControls+<>c__DisplayClass104_0.<ApplyDropdownSelectionNow>b__1()
```

- SkinChanger 在 `ReplaySelectedCharacterPresentation` 里通过反射调用了贞德包的 `CharSelectPatch.Postfix`。贞德包在**主线程**上用 `GD.Load` 同步加载自己的 Spine 骨骼数据(DLL 里的路径是 `res://joanofarcironcladhpwear/…_data.tres`),这个调用进了 Godot 原生代码后就没再返回。
- 就在几行之前,SkinChanger 刚把 4 个皮肤场景放进后台预加载队列。这和第 000 节抓到的 `MoreNecrobinderAnimations` 是**同一种模式**:主线程同步 `ResourceLoader.Load`,同时 SkinChanger 的后台加载正在进行。
- 下一局(18:05)切换了 4 次贞德都没卡,所以这是偶发的竞争条件,不是每次都会出现。

### 3) 点"退出"时卡死(Q1、Q2、日志 22)

**卡在退出的哪一步**:正常退出时(日志 19~21),日志结尾是:

```text
[INFO] Steamworks: shutting down...
[INFO] Steamworks shutdown succeeded!
FMOD Sound System: System released
WARNING: 1050 RIDs of type "CanvasItem" were leaked.
...
ERROR: 302 resources still in use at exit (run with --verbose for details).
```

卡死时(日志 22,以及 18:05、18:22 两局)日志停在 `Steamworks shutdown succeeded!`,后面没有 `FMOD Sound System: System released`。存档、`settings.save` 都已经写完了,SkinChanger 自己的 `[退出诊断] NGame.Quit 返回` 也已经输出。

**托管线程(Q1,18:05 那局)**:`dotnet-stack` 只看到 5 个线程池空闲线程,主线程上已经没有任何 C# 代码在执行。

**原生线程(Q2,18:22 那局,PID 22504)**:我用 `cdb -pv`(非侵入挂载,只读)抓了全部 54 个线程(`diagnostics/Q2_cdb原生线程栈_*.txt`)。游戏 exe 没有符号,只能看到地址:

- **0 号主线程**:`SlayTheSpire2.exe` 内部循环调用 `KERNELBASE!SleepEx`,也就是在轮询等待什么东西完成。
- **2 号、5 号线程**(Godot WorkerThreadPool 线程):各自停在很深的嵌套资源加载里,同一组地址反复递归,最顶上也是 `SleepEx` 轮询。
- FMOD 自己的线程都在正常的 sleep/等待里,不像是 FMOD 卡住了。

我从 2 号、5 号线程的栈上扫描指向 UTF-32 `res:` 字符串的指针,把它们正在加载的资源读了出来(`diagnostics/Q2_栈上资源路径_解码结果_PID22504.txt`,原始输出在同目录):

| 线程 | 栈上的资源(从外层到内层) |
|---|---|
| 2 号 | `res://scenes/cards/card_portrait_blur_material.tres` → `res://shaders/blur/portrait_blur.gdshaderinc` → `res://shaders/blur/Blur.gdshader` → `res://scenes/cards/card_canvas_group_mask_material.tres` → `res://shaders/blur/canvas_group_mask_blur.gdshader` |
| 5 号 | `res://scenes/timeline_screen/timeline_screen.tscn` → `epoch_inspect_screen.tscn` → `unlock_info.tscn` → `epoch_slot.tscn` → `epoch_slot.tscn::ShaderMaterial_blurx` → `res://shaders/blur/Blur.gdshader`(栈更外层还有 `portrait_blur.gdshaderinc`、`canvas_group_mask_blur.gdshader`,可能是残留的旧值) |

- 两个线程都卡在同一组**原版**模糊着色器上(`Blur.gdshader`、`portrait_blur.gdshaderinc`、`canvas_group_mask_blur.gdshader`),看起来是互相等对方加载完。这些都是游戏原版资源,不是哪个 Mod 的(`card_portrait_blur_material.tres` 和 `timeline_screen` 下的场景路径直接写在游戏的 `sts2.dll` 里)。
- 这一局 `Preloading 'Common' assets... count=727 vfx=96` 一直没有输出 `Complete`。所以我推测:**`Common` 预加载在启动时就已经死锁了。它在后台跑,玩的时候察觉不到,到退出时引擎要等后台加载任务结束,才一直卡住。**
- 但 18:05 那局(Q1)`Common` 是加载完成了的,也照样退出卡死,那次没抓原生栈。所以"`Common` 没完成"不是每次退出卡死的共同点,可能有别的后台加载也卡住了。

**一个时间上的规律(只有 5 局能核对,仅供参考)**:

| 这一局 | SkinChanger | 进选角界面时 `Common` 还在加载 | `Common` 完成 | 退出 |
|---|---|---|---|---|
| 18:22(Q2) | 开 | 是 | 否 | ❌ 卡死 |
| 18:39(19) | 关 | 否 | 是 | ✅ |
| 18:51(20) | 关 | 否 | 是 | ✅ |
| 18:59(21) | 开 | 是 | 是 | ✅ |
| 19:03(22) | 开 | 是 | 否 | ❌ 卡死 |

SkinChanger 开着、并且在 `Common` 预加载还没结束时进了选角界面,有 2/3 卡死。这时 SkinChanger 会挂载覆盖包(`覆盖包[visual] LoadResourcePack`)、同步加载选角资源(`LoadCharacterSelectIcon`)、重建 DLL 皮肤的选角展示,正好和游戏的多线程预加载重叠。

### 4) 推测(没有被证实,只供作者参考)

1. 启动黑屏、切换皮肤死锁、退出卡死,很可能都是**同一类问题:Godot 资源加载器在多线程加载和主线程同步加载、或挂载资源包同时发生时互相等待**。只是撞上的时机和资源不同。
2. SkinChanger 可能会增加这种重叠:游戏后台还在多线程预加载时,它在主线程上 `ResourceLoader.Load`、挂载覆盖包(`LoadResourcePack`)、通过反射触发皮肤 DLL 的同步加载(比如贞德包的 `SpineSwap.LoadStageData`)。如果能在 `Common` 等游戏预加载完成之后再做这些事,或者避免在后台加载进行时挂载资源包,也许能避开。
3. 退出卡死那局的死锁链里**全是原版资源**,也可能是这个 beta 版引擎本身就有的问题,SkinChanger 只是让它更容易出现。我没有在完全不装 Mod 的情况下测过,不能排除。

### 5) 顺便发现的其他报错(不一定和卡死有关)

- 每次进选角界面,SkinChanger 的 `SkinService.LoadCharacterSelectIcon` 都会尝试加载 `res://images/packed/character_select/char_select_random_character.png`,然后报 `No loader found for resource` / `Error loading resource`(一局里出现十几次)。看起来 v0.113 里这个图片已经不在这个路径了。
- 选 `Moe-Necrobinder` 时,SkinChanger 生成的运行时场景 `res://sts2_skin_runtime/<会话>/…/scenes/creature_visuals/necrobinder.tscn` 有两处 `Parse Error: [ext_resource] referenced non-existent resource`(第 307、325 行)。

## 0000. 第四轮:逐步关闭皮肤包(2026-11-14 14:40 更新)

| 序号     | 条件(SkinChanger 一直开启)                                                                                                                                                             | SkinChanger 识别的皮肤提供者            | 结果                                                                                            |
| ------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------- | --------------------------------------------------------------------------------------------- |
| 13     | 只关闭 `MoreNecrobinderAnimations`                                                                                                                                                  | 129 个                           | ❌ 卡在 `IntroLogo`                                                                              |
| 14     | 再关闭 7 个角色外观皮肤包(`JoanofArcIroncladHpwear`、`ironcladHpWear`、`succubusIroncladHpWear`、`regentHpWear`、`defectHpWear`、`Moe-Necrobinder`、`necrobinderHpWear`),SkinChanger 里的角色选择全部改回原版 | 约 120 个                         | ❌ 卡在 `MainMenuEssentials`                                                                     |
| **15** | **再关闭全部 103 个带外观的皮肤包**(怪物、事件、商人、涅奥、先古等),只留 17 个纯卡图包,共启用 55 个 Mod                                                                                                                 | **17 个**(`托管加载模式已识别 17 个皮肤提供者`) | ✅ **正常**:`IntroLogo` 41ms 完成,`MainMenuEssentials` 2513ms 完成,`Common` 10899ms 完成;在主菜单点"退出"正常退出 |

- 第 15 次 SkinChanger 的覆盖包只有 `写盘 0.2 MB（69 项）`;之前卡死时是 445~477MB、1234~1334 项。这次也没有出现 `预加载排队 48 个皮肤场景`。
- **结论**:卡死跟 SkinChanger **要接管的外观皮肤数量**有关,或者跟这 103 个包里的某一批有关。只留 SkinChanger 和卡图包时能正常启动(1 次)。第 0 节的"ID 撞名"、第 000 节的 `MoreNecrobinderAnimations` 都**不是**原因;第 000 节抓到的主线程位置,只是卡死那一刻碰巧停在那里。
| **16** | 同 15(另外多了工坊的 `CrossVersionCompat`),共 56 个 Mod | 17 个 | ✅ 正常,`Common` 4273ms 完成,点"退出"正常退出 |
| **17** | 在 16 的基础上**只加回 7 个角色外观皮肤包**,并恢复原来的角色选择;怪物、事件等 103 个包仍然关闭,共 61 个 Mod | 24 个(`覆盖包[visual] 写盘 115.0 MB（403 项）`、`预加载排队 26 个皮肤场景`) | ❌ 卡在 `IntroLogo`,4 分钟以上没有完成,进程还在响应,CPU 很低 |

- **第 16、17 次对比**:关掉所有外观皮肤包后,只要加回这 7 个**角色**外观皮肤包,就会卡死。所以跟怪物皮肤的数量无关,问题出在 SkinChanger 接管角色外观皮肤的时候。
- 第 14 次关掉 7 个角色包、但留着怪物包时也卡死了。说明怪物包里可能还有别的触发因素,也可能是两边都会触发。
- 下一步:7 个角色包逐个或分组加回来,定位是哪个包、或者哪个角色(结果待补充)。

## 000. 第三轮:抓到主线程卡住的位置(已被第 0000 节修正)(2026-11-14 14:00 更新)

- SkinChanger 更新到新的内测构建后(日志 11、12 里的 `内测版本` 一行),狐猎包也已经删掉,照样卡死:
  - 日志 11:卡在 `IntroLogo`。
  - 日志 12:进了主菜单后,加载 `Common` 时死锁。
- **日志 12 死锁时,用 `dotnet-stack` 抓到了游戏主线程的调用栈**(`diagnostics/12_dotnet-stack_report_主线程0x23C0.txt`)。用 PowerShell 按线程创建时间排序,确认 `0x23C0` 是进程里最早创建的线程,也就是主线程:

```text
Thread (0x23C0):   ← 主线程
  GodotSharp!Godot.NativeCalls.godot_icall_3_1180(...)
  GodotSharp!Godot.ResourceLoader.Load(class System.String,class System.String,value class CacheMode)
  MoreNecrobinderAnimations!MoreNecrobinderAnimations.PresentationPreloader.<Warm>g__Actor|8_0(...)
  MoreNecrobinderAnimations!MoreNecrobinderAnimations.PresentationPreloader+<Warm>d__8.MoveNext()
  ...
  GodotSharp!Godot.SignalAwaiter.SignalCallback(...)
```

- 卡住前日志的最后一条记录:`WARNING: res://scenes/creature_visuals/necrobinder.tscn:5 - ext_resource, invalid UID ...`,它的 C# 调用栈同样来自 `MoreNecrobinderAnimations.PresentationPreloader.Warm`。这条之后,`MaidenSuccubus` 的诊断日志每 30 秒报一次 `asset session waiting: Common; lastProcessAgoMs=...`,数值一路涨到 36 万毫秒以上,说明**主循环 6 分钟都没跑过一帧**。
- 也就是说:**主线程卡在了 `More Necrobinder Animations`(工坊 `3816316338`,0.1.27,DLL 是 11-13 21:52 跟游戏一起更新的)在主线程上同步调用 `ResourceLoader.Load` 加载骨妹场景 `necrobinder.tscn` 的地方,一直没有返回。**
- **推测**(没有被证实):
  - 当时游戏的 `Common` 预加载和 SkinChanger 的皮肤场景预加载(`预加载排队 48 个皮肤场景`)都在用 Godot 的多线程加载。
  - 这时在主线程上同步加载同一个、或者有依赖关系的角色场景,可能触发 Godot 资源加载器的同步和多线程互等问题。
  - 这能解释为什么问题是随机的、为什么禁用 SkinChanger 就没事、为什么皮肤包越多越容易出现。
- 第 10 次死锁的转储里,主线程没有托管调用栈,当时 `PresentationPreloader.Warm` 正在等待信号。所以**不能说每次卡死都卡在同一个地方**,但两次都出现了这个 Mod 的预热代码。
- **下一步测试**:保持 SkinChanger 开启,只禁用 `MoreNecrobinderAnimations`,连续启动几次看看(结果待补充)。

## 00. 第二轮测试结论(2026-11-14 下午更新,请优先看这一节,它修正了下面第 0 节)

**第 0 节"ID 叫 `silent` 的 Mod 是触发条件"的结论不成立。** 后来又测了 4 次:

| 序号 | 条件 | SkinChanger 准备的角色资源包 | 结果 |
|---|---|---|---|
| 07 | 新装猎人 Kaguya 皮肤包 `silentKaguyaHpWear`(ID **不**撞名),猎人选原版 | 5 个 | ❌ 卡在 `IntroLogo` |
| 08 | 同上,但猎人**选中** `silentKaguyaHpWear` | 5 个 | ⚠️ 进了主菜单,加载 `Common` 时**闪退**。日志直接中断;没有 dump,Windows 事件日志里也没有崩溃或显卡驱动重置记录。进主菜单时显存 `VRAM=3.7GB`,显卡只有 4GB |
| 09 | 同 08 | 5 个 | ❌ 卡在 `IntroLogo` |
| 10 | **删掉**猎人 Kaguya 包(条件和成功的 06 一样) | 4 个 | ❌ `MainMenuEssentials` 加载完成后**进程彻底死锁**:`Responding=False`,5 秒内 CPU 只用了 0.02 秒,日志停止写入 |

**汇总所有 SkinChanger 开启的启动**:
- 装了猎人 Kaguya 外观包:7 次里 6 次卡死、1 次闪退,0 次正常。
- 没装:1 次正常(06)、1 次死锁(10)。
- SkinChanger 禁用时只测过 1 次,正常(03)。

**结论修正**:
- 这是一个**随机出现**的卡死或死锁。装了猎人 Kaguya 外观包后几乎每次都会出现,但没装也会出现。
- 判断依据:每次卡死时 SkinChanger 都显示 `准备 5 个角色的选角资源包`,而且自己的皮肤预加载一直没跑完。唯一正常的 06 是 `准备 4 个角色`,并且出现了 `[性能] 预加载完成：成功 46，失败 2`。
- 我这边不能 100% 排除其他 Mod 的影响,因为 SkinChanger 禁用的情况只测了 1 次。

**第 10 次死锁的进程转储分析**(用微软官方 `dotnet-dump` 分析完整内存转储;原始输出见 `diagnostics/`):
- `syncblk`:**没有任何托管锁被持有**。线程池全局队列和各线程本地队列都是空的。
- 主线程(OSID `0x572c`)上**没有托管调用栈**,说明卡在 Godot 引擎的原生代码里。另外 9 个曾经执行过托管代码的引擎工作线程也一样。
- `dumpasync`:所有没完成的 async 任务都在等**下一帧**。包括游戏的 `NGame.LaunchMainMenu → NTransition.FadeIn → NodeUtil.AwaitProcessFrame`、`SteamInitializer.RunCallbacksAsync`、`MoreNecrobinderAnimations.PresentationPreloader.Warm`(等一个 `Godot.SignalAwaiter`)。主线程卡在原生代码里,下一帧永远不会到来。
- 主线程上最后一次抛出的异常是 `System.BadImageFormatException: Bad method token.`,没有调用栈,看起来是某次反射扫描时被吞掉的,不确定和卡死有没有关系。
- **推测**:更像是 Godot 的资源加载,也就是 `ResourceLoader` 的多线程加载,在原生层互相等待了。SkinChanger 启动时挂载了大量覆盖包,还会排队预加载皮肤场景,可能让这种情况更容易发生。要确认需要原生调用栈加引擎符号,我这边做不到。
- 转储文件有 7.4GB,而且包含进程的完整内存(可能有账号会话等隐私数据),所以没有附上。如果作者需要,可以再商量怎么提供。

## 0. 找到触发条件(第一轮结论,已被第 00 节修正)

**删掉一个 Mod ID 叫 `silent` 的猎人皮肤 Mod 之后,SkinChanger 开着、其他设置都不变,游戏能正常进主菜单(日志 06)。**

- 被删掉的 Mod:`狐猎爆衣·Kaguya融合·全斗篷`,版本 0.3.0,放在本地 `mods\Kaguya全斗篷\` 目录(`silent.dll` + `silent.pck` + `mod_manifest.json`)。
  - **它的 manifest 里 `id` 写的是 `silent`**,跟原版猎人角色的 ID 完全一样。
  - 它没有声明 `min_game_version`(日志 05 第 1907 行:`Mod silent does not declare min game version`)。
- 在 SkinChanger 的配置里,`Selections` 用 `"silent": "silent"` 表示猎人用原版皮肤。
  - 4 次卡死的日志里**都出现了** `[SkinChanger] 已直接复用完整外观资源包 silent.pck，后续仅生成资源校正包。`(01 第 1507 行、02 第 1475 行、04 第 2670 行、05 第 2729 行)。
  - 第 05 次已经把战士、储君、机器人都换回默认皮肤,这一行还是在。
  - 正常的那次(日志 06)**没有这一行**。
- 所以我的判断是:**SkinChanger 把"原版猎人皮肤"的 `silent` 和这个 ID 也叫 `silent` 的 Mod 搞混了**,把它的整个 `silent.pck` 当成猎人原版外观资源包挂了上去,结果游戏的 `IntroLogo` / `MainMenuEssentials` 预加载一直完成不了。
  - 这也能解释作者说的"不是所有人都进不去":只有装了 ID 跟角色 ID 同名的 Mod 的人才会触发。
  - 这个 Mod 在游戏 v0.111 + 旧版 SkinChanger 下是能正常用的,问题是在 v0.113 + 新版 SkinChanger 后才出现。
- 具体是怎么导致预加载卡死的,我这边没法确认,因为 DLL 做了混淆,读不了代码。
- 目前删掉 Mod 后只验证了 **1 次**正常启动;卡死那边是 4/4 必现。

**给作者的建议**:
1. SkinChanger 判断"原版皮肤"时,不要只看 Mod ID 是不是等于角色 ID(`silent` / `ironclad` / `regent` / `defect` / `necrobinder` 等)。
2. 遇到 Mod ID 跟角色 ID 撞名时,给个警告,或者自动区分开。

**下面第 1~6 节是找到这个原因之前写的排查记录,保留作参考。以下几条推测已经被日志 06 推翻:**
- "预加载 48 个皮肤场景和游戏预加载互相等待":日志 06 里同样有 `预加载排队 48 个皮肤场景（队列上限 48）`(第 1553 行),但游戏能正常进。
- "覆盖包里的主菜单资源有问题":日志 06 生成的覆盖包里同样有 56 个主菜单、Logo 相关路径,但游戏能正常进。
- "`[选角闸门]` 刷屏导致卡死":日志 06 里照样刷了 1.2 万多行,但游戏能正常进。刷屏本身不是原因,不过还是建议降低这条日志的输出频率。
- "机械硬盘慢导致卡死":可能性很低。日志 06 用的是同一台电脑、同一块硬盘,能正常进。

## 1. 结论摘要(找到触发条件之前写的,参考第 0 节)

- 游戏升级到 **v0.113.0** 后,只要启用当前版本的 SkinChanger,**游戏启动后就会一直黑屏,进不了主菜单**:
  - 游戏自己的预加载 `Preloading 'IntroLogo'` 或 `Preloading 'MainMenuEssentials'` 开始后**一直不结束**,最长一次等了十几分钟也没有进展。
  - 同时日志里 `[SkinChanger] [选角闸门]` 一直在刷屏,一次启动就有 1.6 万~4 万行。
- **只禁用 SkinChanger,其他 Mod 不动,游戏能正常进主菜单**。前后共 5 次启动(4 次开启、1 次禁用),结果都一样。
- 下面这些因素已经排除(详见第 4 节):
  - 血量换装皮肤的选择:3 个角色换回默认皮肤后照样卡死。
  - Mod 加载顺序:SkinChanger 排第 2 位和第 165 位都会卡死。
  - 工坊和本地重复安装的同 ID 皮肤包:删掉重复的工坊包后照样卡死。
- 我的推测(**没有被证实**,仅供参考):SkinChanger 启动时会做几件后台工作,包括延后初始化 21 个皮肤 DLL、预加载 48 个皮肤场景(正好达到队列上限 48)、每次重新生成约 450~500MB 的覆盖包。这些工作可能和游戏自己的 `PreloadManager` 预加载抢资源或互相等待,导致游戏的预加载一直完成不了。我的游戏和 Mod 都装在**机械硬盘**上,读写速度慢,可能就是"只有部分设备会出问题"的原因(详见第 6 节)。

## 2. 环境信息

### 2.1 硬件

| 项目 | 值 |
|---|---|
| 机型 | 笔记本,日志里的 Device Model 是 `82AV` |
| CPU | Intel Core i5-10300H @ 2.50GHz(4 核 8 线程,日志 `Processor Count: 8`) |
| 内存 | 15.9 GB(启动时可用物理内存约 3.6 GB;卡死时游戏进程占用约 2.2~2.5 GB) |
| 独显 | NVIDIA GeForce GTX 1650 Ti,驱动 `32.0.16.2114` |
| 核显 | Intel UHD Graphics,驱动 `31.0.101.2125` |
| 其他显示适配器 | 系统里装有 `GameViewer Virtual Display Adapter`(虚拟显示器,游戏没有用它) |
| 屏幕 | 1920×1080 @ 60Hz,缩放 DPI 120 |
| C 盘 | SAMSUNG MZVLB256HBHQ(**NVMe SSD**,238 GB,剩余约 28 GB)。`%APPDATA%`(SkinChanger 的配置和覆盖包缓存)在这里 |
| D 盘 | TOSHIBA MQ04ABF100(**SATA 机械硬盘 HDD**,1 TB,剩余约 44 GB)。**游戏本体、创意工坊 Mod、本地 `mods` 目录都在这里** |

### 2.2 软件

| 项目 | 值 |
|---|---|
| 操作系统 | Windows 10 Pro 22H2(`10.0.19045`),语言 `zh_CN` |
| 游戏版本 | `v0.113.0`(commit `16ac0fe3`,2026-11-12,Steam `public-beta` 分支) |
| 引擎 | MegaDot `4.5.1-m.14 (custom_build)`,mono |
| 渲染 | D3D12,Forward+,设备 0 = GTX 1650 Ti |
| 显示设置 | 全屏、`vsync = adaptive`、`fps_limit = 60`、`msaa = 2`、后台限帧开启(完整内容见 `config/settings_display.json`) |
| 安装路径 | `D:\TOOL\steam\steamapps\common\Slay the Spire 2` |
| Mod 数量 | 已加载 165~168 个(本地 `mods` 目录 + 创意工坊),完整列表和加载顺序见 `config/mods_list.md` |

### 2.3 SkinChanger 版本

| 项目 | 值 |
|---|---|
| 出问题的版本 | manifest `version = 1.0.31`,`internal_test_version = 1.0.31.29`;启动日志原文:`[SkinChanger] 内测版本 1.0.31.29；程序集=Gurio.SkinChanger 1.0.31.29` |
| 更新时间 | 2026-11-13 21:52~21:53,和游戏 v0.113.0 一起被 Steam 自动更新 |
| 上一个能正常用的版本 | 日志显示为 `内测版本 1.0.29.100`(当时游戏版本是 v0.111.0)。那份日志已经被游戏自动轮换删除,没法附上,只保留了当时查到的数据(见第 3 节) |

## 3. 现象与时间线

### 3.1 现象

从 Steam 启动后窗口一直黑屏,进不了主菜单。游戏进程没有崩溃,窗口焦点切换的日志(`MuteInBackground: FocusIn/FocusOut`)还在正常输出,说明主线程还在运行,只是游戏的资源预加载一直不完成。同时 `[SkinChanger] [选角闸门]` 一直刷屏,一直刷到进程被结束为止。

日志里的刷屏行长这样(每轮 3 行,分别对应储君、机器人、战士):

```text
[INFO] [SkinChanger] [选角闸门] 分组=regent 选择=regentHpWear 是运行包=True 返回=regentHpWear
[INFO] [SkinChanger] [选角闸门] 分组=defect 选择=defectHpWear 是运行包=True 返回=defectHpWear
[INFO] [SkinChanger] [选角闸门] 分组=ironclad 选择=JoanofArcIroncladHpwear::source:3833061799::variant:2 是运行包=True 返回=JoanofArcIroncladHpwear::source:3833061799
```

3 个角色都换回默认皮肤后,日志变成下面这样,但还是一样刷屏:

```text
[INFO] [SkinChanger] [选角闸门] 分组=ironclad 选择=ironclad 是运行包=False 返回=<null>
[INFO] [SkinChanger] [选角闸门] 分组=regent 选择=regent 是运行包=False 返回=<null>
[INFO] [SkinChanger] [选角闸门] 分组=defect 选择=defect 是运行包=False 返回=<null>
```

我统计了相邻两次焦点切换之间的刷屏行数:窗口在前台时明显比在后台时多(前台 60 帧,后台限制在 30 帧)。所以这段日志看起来是**每一帧或每隔几帧调用一次**,而不是只在选角界面调用。

### 3.2 每次启动的情况

日志在 `logs/` 目录下,文件名里写了序号、结果和启动时间(当地时间 UTC+8)。下表中的行号就是对应日志文件里的行号。

| 序号 | 启动时间 | SkinChanger | SkinChanger 加载位置 | 结果 | 卡在哪里(行号) | `[选角闸门]` 行数 | 怎么结束的 |
|---|---|---|---|---|---|---|---|
| (旧) | 11-13 更新前 | `1.0.29.100` | 未记录 | ✅ 正常,游戏版本 v0.111.0 | `IntroLogo` 801ms 完成,`MainMenuEssentials` 256ms 完成 | 0 | 在主菜单点"退出" |
| 01 | 11-14 00:46 | 新内测版 | 第 2 位 | ❌ 卡死 | `IntroLogo` 开始(L4531)后再也没完成 | 16659 | 玩家直接关窗口 |
| 02 | 11-14 01:07 | 新内测版 | 第 2 位 | ❌ 卡死 | `IntroLogo` 开始(L3391)后再也没完成 | 40794 | 卡了很久(十几分钟)没有进展,`taskkill /F` 强制结束 |
| 03 | 11-14 01:24 | **禁用** | — | ✅ 正常 | `IntroLogo` 118ms 完成(L4293),`MainMenuEssentials` 2171ms 完成(L4299) | 0 | 在主菜单点"退出"(L4590) |
| 04 | 11-14 02:21 | 新内测版 | 第 165 位 | ❌ 卡死 | `IntroLogo` 307ms 完成(L4464),**`MainMenuEssentials` 开始(L4469)后再也没完成** | 26089 | `taskkill /F` 强制结束 |
| 05 | 11-14 02:30 | 新内测版,**战士/储君/机器人都改成默认皮肤** | 第 165 位 | ❌ 卡死 | `IntroLogo` 开始(L4542)后再也没完成 | 24953 | `taskkill /F` 强制结束 |
| **06** | 11-14 02:5x | 新内测版,原来的皮肤选择,**删掉了 `Kaguya全斗篷`(id=`silent`)** | 第 2 位 | ✅ **正常** | `IntroLogo` 157ms 完成(L3461),`MainMenuEssentials` 2367ms 完成(L4012),`Common` 12994ms 完成(L5971) | 12971(复制日志时游戏还在运行) | 复制日志时游戏还在运行 |

补充说明:

- 第 04、05 次 SkinChanger 排到了第 165 位,原因是排查时在 `settings.save` 里把它禁用后又重新启用,游戏就把它重新排到了后面。日志里有这条警告:`托管加载模式仅能拦截排在本 Mod 后面的皮肤提供者…`(04 的 L2439,05 的 L2495)。第 01、02 次它排在第 2 位,也一样卡死,所以加载顺序不是原因。
- 第 01 次启动时,工坊和本地 `mods` 目录各装了一份同 ID 的血量换装包(`necrobinderHpWear`、`JoanofArcIroncladHpwear`、`ironcladHpWear`)。第 02 次启动前删掉了工坊里重复的那份,还是卡死。
- 第 04 次是唯一一次 `IntroLogo` 完成、改成卡在 `MainMenuEssentials` 的,每次卡住的位置不完全一样。

## 4. 排查过程

| 步骤 | 做了什么 | 结果 | 结论 |
|---|---|---|---|
| 1 | 对比游戏更新前(v0.111.0)和更新后(v0.113.0)的日志 | 更新前 `IntroLogo` 801ms 完成;更新后一直不完成 | 问题从游戏和 Mod 自动更新之后开始出现 |
| 2 | 查所有 Mod 的 pck 有没有覆盖开场 Logo 和主菜单资源(`animations/ui/logo`、`logo_animation`、`scenes/screens/main_menu/*.tscn` 等) | 151 个 pck 里没有一个命中 | 不是某个 Mod 直接替换了 Logo 场景 |
| 3 | 查哪些 Mod 的 DLL 引用了游戏的预加载类(`PreloadManager`、`AssetCache`、`LoadAssetSets`、`AssetLoadingSession`) | SkinChanger、BaseLib、RitsuLib、MaidenSuccubus、figure_Saya 等都有引用;其中 MaidenSuccubus 和 figure_Saya 都声明支持 0.113 | 没法只靠这一步锁定是谁 |
| 4 | 对比 SkinChanger 新旧版本的日志量 | 包含 `SkinChanger` 的日志行:旧版整个会话 821 行;新版在日志 02 那次会话运行途中统计时已有 29716 行,其中 `[选角闸门]` 占 29406 行 | SkinChanger 新版有明显的高频循环 |
| 5 | **只禁用 SkinChanger**(在 `settings.save` 里设 `is_enabled=false`) | ✅ 正常进入主菜单(日志 03) | 问题和 SkinChanger 强相关 |
| 6 | 重新启用 SkinChanger | ❌ 又卡死(日志 04,这次卡在 `MainMenuEssentials`) | 可以稳定复现 |
| 7 | 保持 SkinChanger 开启,只把 `skin_changer.json` 里刷屏的 3 个角色改回默认皮肤(`ironclad` / `regent` / `defect`) | ❌ 照样卡死,刷屏照样有(日志 05) | 不是这几个血量换装皮肤造成的,是 SkinChanger 本身 |
| 8 | 检查 SkinChanger 的覆盖包缓存 `overlay-cache` | 每次启动都会重新生成(日志里有"皮肤包或游戏版本有变化:已清空外观覆盖包缓存"),所以手动清缓存没有意义 | 不是旧缓存残留造成的 |
| 9 | 对比"所有卡死会话都有、正常会话没有"的 WARN/ERROR | 只有 SkinChanger 自己的警告:`检测到重复 Mod 模型 ID CARD.FUSION / CARD.LULLABY / CARD.SOUL_IMPACT / POWER.LULLABY_POWER`、`背景自检失败 maiden_succubus_act_maiden_succubus_fourth_act` | 没有其他 Mod 的错误跟卡死对得上 |

## 5. 日志中的关键证据

### 5.1 SkinChanger 启动阶段的后台工作(每次卡死都有)

以日志 04 为例:

```text
L2660 [INFO] [SkinChanger] 皮肤包或游戏版本有变化：已清空外观覆盖包缓存。
L2697 [WARN] [SkinChanger] [性能] 覆盖包[visual] 写盘 477.2 MB = 6242.5 ms（1334 项）
L2700 [INFO] [SkinChanger] [性能] 启动阶段延后初始化 21 个皮肤提供者的 DLL：先让主菜单出来，再在后台逐个完成。
L2702 [INFO] [SkinChanger] [性能] 预加载排队 48 个皮肤场景（队列上限 48）。
L2704 [WARN] [SkinChanger] [性能] 外观重建合计 7328 ms ...
L4247 [WARN] [SkinChanger] [性能] 覆盖包[cards] 写盘 46.6 MB = 2783.3 ms（955 项）
L4283 [INFO] [SkinChanger] [性能] 延后初始化完成：成功 0、跳过 21、失败 0 ...
L4433 [INFO] Preloading 'IntroLogo' assets... count=3 vfx=0
L4464 [INFO] Preloading 'IntroLogo' Complete: assets=3 time_elapsed=307ms
L4469 [INFO] Preloading 'MainMenuEssentials' assets... count=5 vfx=0
            ← 之后再也没有 'MainMenuEssentials' Complete,只剩 [选角闸门] 刷屏和焦点切换日志
```

- 4 次卡死的会话里**都有**"预加载排队 48 个皮肤场景(队列上限 48)",但**一次也没看到这个队列的完成日志**。
- 第 01、02 次的"延后初始化完成"是`成功 15、跳过 6`;第 04、05 次是`成功 0、跳过 21`。不管哪种都会卡死。

### 5.2 覆盖包内容

`%APPDATA%\SlayTheSpire2\Gurio.SkinChanger\overlay-cache\` 里那个约 500MB 的覆盖包(文件清单见 `config/overlay-cache_listing.txt`)里,包含了大量游戏本体的界面资源路径,例如:

```text
res://animations/backgrounds/mainmenu/logo/main_menu_logo_skel_data.tres
res://animations/backgrounds/mainmenu/top/main_menu_top_skel_data.tres
res://animations/ui/logo/logo_megacrit_animate_skel_data.tres
res://images/packed/main_menu/*.png、res://images/ui/main_menu/*.png
res://addons/sentry/user_feedback/logo.svg
```

这些可能是 SkinChanger 为了切回原版皮肤而保存的"基线"资源(日志里有 `BuildBaselineDependencyOverlay` / `基线依赖覆盖`)。我没办法确认这些资源是不是和 v0.113.0 的原版资源一致,**请作者确认一下**:在 v0.113.0 下挂载这些主菜单和 Logo 资源,会不会影响游戏自己的 `IntroLogo` / `MainMenuEssentials` 预加载。

### 5.3 游戏更新后其他 Mod 的报错(和卡死无关,仅供参考)

下面这些在禁用 SkinChanger、能正常进游戏的会话(日志 03)里同样存在:

- `VoiceModFramework`:`Parameter "serializableRun" not found in method NRun::ShowGameOverScreen`
- `IzumiKonata`:`does not include a build for STS2 v0.113.0`
- `LustTravel2`:关键补丁 `lusttravel2_character_select_bgm` 失败,已经自动回滚
- `Merchant2CuteII`:`Cannot open file 'res://animations/backgrounds/fake_merchant_room/top/fake_merchant_top.tres.tres'`

## 6. 推测(没有被证实,只供作者参考)

1. **可能的原因:后台任务和游戏预加载抢资源或互相等待。** 游戏的 `PreloadManager` 预加载 `IntroLogo` 或 `MainMenuEssentials` 时,SkinChanger 正在后台排队预加载 48 个皮肤场景、延后初始化 21 个皮肤 DLL,同时挂载几百 MB 的覆盖包。如果两边的资源加载在线程池或主线程上互相等待,或者游戏要等的某个资源正被覆盖包替换或占用,游戏的预加载就会一直完成不了。
   - 支持这个推测的现象:每次卡住的位置不固定,有时是 `IntroLogo`,有时是 `MainMenuEssentials`;旧版 SkinChanger 不会这样;只有部分用户会遇到。
2. **为什么只有部分设备会出问题:** 我的游戏和 Mod 在**机械硬盘(D 盘)**上,覆盖包在 NVMe(C 盘)上;CPU 是 4 核 8 线程;启动时可用内存只有约 3.6 GB;同时加载了 160 多个 Mod、129~131 个皮肤提供者。磁盘慢、线程少、皮肤多,都会改变加载时序,让问题更容易出现。
3. **`[选角闸门]` 的高频调用**,看起来是每帧或每隔几帧调用一次。它本身不一定是卡死的原因,但它说明 SkinChanger 有一个每帧都在跑的检查,可能在等待某个永远不会满足的条件。

## 7. 附件清单

```text
SkinChanger_report_20261114/
├─ SkinChanger_启动卡死问题反馈.md        ← 本文档
├─ logs/
│  ├─ 01_卡死_IntroLogo_SkinChanger开启_00-46启动.log
│  ├─ 02_卡死_IntroLogo_SkinChanger开启_01-07启动.log
│  ├─ 03_正常_SkinChanger禁用_01-24启动.log
│  ├─ 04_卡死_MainMenuEssentials_SkinChanger开启_02-21启动.log
│  ├─ 05_卡死_IntroLogo_SkinChanger开启_三角色改默认皮肤_02-30启动.log
│  ├─ 06_正常_SkinChanger开启_删除Kaguya全斗篷(id=silent)后_02-5x启动.log   ← 复制时游戏还在运行,是截至当时的日志
│  ├─ 07_卡死_IntroLogo_SkinChanger开启_新装狐猎可战损版(silentKaguyaHpWear)_猎人选原版_03-0x启动.log
│  ├─ 08_闪退_进主菜单后加载Common时_猎人选silentKaguyaHpWear_03-1x启动.log
│  ├─ 09_卡死_IntroLogo_猎人选silentKaguyaHpWear_13-13启动.log
│  └─ 10_死锁_MainMenuEssentials完成后_已删狐猎包_13-21启动.log
├─ diagnostics/                       ← 第 10 次死锁时抓的进程分析(dotnet-dump / dotnet-stack 原始输出)
│  ├─ 10_dump分析_clrthreads_syncblk_clrstack.txt
│  ├─ 10_dump分析_dumpasync_tpq.txt
│  └─ 10_dotnet-stack_report.txt
└─ config/
   ├─ skin_changer.json              ← SkinChanger 配置(卡死时使用的原始皮肤选择)
   ├─ SkinChanger_manifest.json      ← 当前安装的 SkinChanger manifest
   ├─ overlay-cache_listing.txt      ← 覆盖包缓存的文件清单
   ├─ mods_list.md                   ← 168 个 Mod 的加载顺序、版本和来源
   ├─ settings_mod_list.json         ← 游戏 settings.save 里的 mod_settings(注意:导出时 SkinChanger 已经被禁用)
   └─ settings_display.json          ← 显示相关设置
```

第三~五轮新增的文件(上面的树里没有列出):

```text
logs/
├─ 11_卡死_IntroLogo_SkinChanger新内测版_已删狐猎包_13-43启动.log
├─ 12_死锁_主线程卡在MoreNecrobinderAnimations同步加载_13-54启动.log
├─ 13_卡死_IntroLogo_已禁用MoreNecrobinderAnimations_14-17启动.log
├─ 14_卡死_MainMenuEssentials_已禁用7个角色皮肤_14-28启动.log
├─ 15_正常_只留SkinChanger和17个卡图包_关闭全部外观皮肤_14-3x启动.log
├─ 16_正常_同15条件第二次启动.log
├─ 17_卡死_IntroLogo_加回7个角色皮肤_怪物皮肤仍关闭_14-43启动.log
├─ 18_死锁_选角切换到贞德皮肤_SkinChanger开启_20个Mod_17-58启动_日志节选.txt
├─ 19_正常退出_SkinChanger禁用_33个Mod_18-39启动.log
├─ 20_正常退出_SkinChanger禁用_33个Mod_18-51启动.log
├─ 21_正常退出_SkinChanger开启_每个角色只留1个战损皮_31个Mod_18-59启动.log
└─ 22_退出卡死_SkinChanger开启_加回3个角色皮肤_34个Mod_19-03启动.log
diagnostics/
├─ 12_dotnet-stack_report_主线程0x23C0.txt
├─ 18_dotnet-stack_切换贞德皮肤死锁_PID13964.txt
├─ Q1_dotnet-stack_退出卡死_18-05启动_PID21692.txt
├─ Q2_cdb原生线程栈_退出卡死_18-22启动_PID22504.txt
├─ Q2_cdb栈上资源路径扫描原始输出_PID22504.txt
└─ Q2_栈上资源路径_解码结果_PID22504.txt
config/
└─ mods_list_第五轮_34个Mod.md
```

日志都是游戏原始的 `godot.log`,只改了文件名,内容没有任何改动。原始位置在 `%APPDATA%\SlayTheSpire2\logs\`。 例外:日志 18 是节选(见第 00000 节说明)。

## 8. 可以配合的测试

如果需要更多信息,我可以配合做这些测试:

- 用 `launch_vulkan.bat` 或 `launch_opengl.bat` 换渲染后端启动,看是否还会卡死
- 把游戏或 Mod 移到 SSD 上再测
- 减少启用的皮肤包数量再测
- 换成作者提供的调试版或开了详细日志的版本再测
