# 交接文档：STS2 + SkinChanger 启动黑屏 / 资源加载死锁排查

> 写给接手的 agent。目标：**结合代码**（SkinChanger、游戏 sts2.dll、spine-godot、Godot ResourceLoader）找出死锁根因或可行规避。
> 截至 2026-11-15 02:50。所有结论均标注了证据强度；"推断"=未经代码验证。
> 用户目标：**找出为什么会卡（根因）**，不是找一个能玩的配置。

---

## 1. 现状摘要（先读这一节）

**现象**：开启 Gurio.SkinChanger 并选中较多 Spine 皮肤（角色/怪物）后，启动时预加载**永远不完成**：
- 卡在 `IntroLogo` / `MainMenuEssentials` → 黑屏；
- 或者主菜单能出来，但 `Common` 永远不完成 → 之后任何同步加载同一资源的操作（选角点角色、打开怪物图鉴、退出）把主线程一起拖死。

**每次卡死的固定形态（已确认，10 余次抓栈一致）**：
- **恰好 2 个 WorkerThreadPool 线程**，各自卡在一条 Spine 嵌套加载链里：`*_skel_data.tres → *.atlas（.spatlas）→ *.png`，原生栈为 `libspine_godot → Godot → SleepEx` 轮询；
- 主线程要么在帧循环空转，要么在同步 `ResourceLoader.Load` 里等待同一资源（条件变量）；
- 无托管锁被持有。

**每次卡住的具体资源都不同**：机器人皮、狐猎皮、仪式兽、红宝石劫掠者 + 旧日雕像、魅魔战士的商店/篝火……去掉一个包，卡住的就换成另一个包。**不是单包损坏**（已由排除实验证实）。

**与数量强相关**（SkinChanger `.99`）：

| SC 预加载排队数 | 结果 |
|---|---|
| 15 | 1/1 正常 |
| 21 | 3 正常 / 1 疑似卡 |
| 31 | 1/1 卡 |
| 48 | 2/2 卡 |

SC 关闭或不选外观皮肤时基本不卡。

**时序（日志行号可证）**：SkinChanger 的 `预加载排队 N 个皮肤场景` 出现在游戏 `Preloading 'IntroLogo'` **之前**，所以 SC 的线程化预加载与游戏自己的 IntroLogo / MainMenuEssentials / Common 预加载**并发进行**。

**SkinChanger 用到的加载 API**：从 DLL 字符串看，有 `LoadThreadedRequest`、`LoadThreadedGet`、`LoadResourcePack`、`AssetCache`、`GetScene`（未反编译确认调用方式）。

**最可能的方向（推断，待源码确认）**：Godot 4.5 ResourceLoader 的线程化加载里，两个任务互相等待对方正在加载的子资源（或同一 png/atlas 被两条链共享），并且等待方式是 WorkerThreadPool 上的阻塞等待，导致池内没有空闲线程去推进。SC 的大量 `LoadThreadedRequest` + 游戏预加载同时进行，使这种"环形等待"变得很容易出现。退出卡死时出现过**原版 Blur shader** 链（非 Spine），说明问题在 ResourceLoader 层而不在 Spine 本身。

---

## 2. 环境

| 项 | 值 |
|---|---|
| 游戏 | Slay the Spire 2 v0.113.0 public-beta，`D:\TOOL\steam\steamapps\common\Slay the Spire 2` |
| 引擎 | `MegaDot v4.5.1.m.14.mono.custom_build`（Godot 4.5.1 mono 魔改），D3D12，GTX 1650 Ti 4GB |
| 游戏托管代码 | `data_sts2_windows_x86_64\sts2.dll`（+ `sts2.xml`、`GodotSharp.dll`） |
| Spine 原生库 | `libspine_godot.windows.template_release.x86_64.dll`（游戏根目录，无符号） |
| SkinChanger | 创意工坊 `D:\TOOL\steam\steamapps\workshop\content\2868840\3787302680\Gurio.SkinChanger.dll`（1.0.31 beta）。**内部版本：卡死日志 25–29 都是 `1.0.31.84`；11-15 01:58 自动更新为 `1.0.31.99`，日志 30 及以后都是这个版本，死锁仍在**。同目录还有 `thunninoiSkinManager.dll` |
| 其他调用 `AssetCache`/`GetScene` 的 mod | RegentFX、figure_Saya（有 LoadAsset）、intentgraph2core、Rewind、QuickLink、SignatureLib、MFML、SkadaHelper、MoreSilentAnimations（DLL 字符串扫描）。`AssetCache.LoadAsset` / `GetScene` 被 Harmony 打了补丁（栈上是 `LoadAsset_Patch1` / `GetScene_Patch1`），**补丁来自哪个 mod 尚未确认**；RitsuLib 的 `ContentAssetOverridePatchHelper` 也在这条路径上 |
| 反编译工具 | 未安装 ILSpy/ilspycmd；本机有 `dotnet` SDK，可 `dotnet tool install -g ilspycmd` |
| 本地皮肤包 | `...\Slay the Spire 2\mods\<中文文件夹>\<id>.pck/.dll/.json`，如 `mods\雪霁&森森改 韩国机器人可战损版\defectHpWear.pck`、`mods\狐猎可战损版\silentKaguyaHpWear.pck`、`mods\白猫牌塔2怪物娘化整合包(完整版)\...`（怪物包，96 个子包） |
| Mod 启用表 | `%APPDATA%\SlayTheSpire2\steam\76561199106684372\settings.save` → `mod_settings.mod_list[{id,is_enabled,source}]` |
| SkinChanger 选择 | `%APPDATA%\SlayTheSpire2\Gurio.SkinChanger\skin_changer.json` → `Selections`（key=角色/怪物，value=皮肤包 id 或 `__base__`） |
| 日志 | `%APPDATA%\SlayTheSpire2\logs\godot.log`；轮转文件名 `godotYYYY-MM-DDTHH.MM.SS.log` 是**下一次启动**的时间，仅保留约 5 份 |

> 注意：本环境的工具输出会把四段式版本号显示成 `public-ip-xxxx`，引用版本前请逐字符核对（`[ord(c) for c in s]`）。

---

## 3. 症状与日志特征

| 类型 | 日志特征 |
|---|---|
| 启动黑屏（主问题） | 最后一行停在 `[INFO] Preloading 'IntroLogo' assets...` 或 `Preloading 'MainMenuEssentials' assets... count=5`，永远没有对应 `Complete`；SkinChanger 自己的预加载也不打 `预加载完成`。进程未崩溃，CPU 近 0 |
| 退出卡死 | `Steamworks shutdown succeeded!` 之后无 `FMOD Sound System: System released`；该局 `Preloading 'Common'` 从未 Complete（启动期就已死锁，退出时才暴露） |
| 选角界面实时切皮卡死 | 主线程同步 `GD.Load` 皮肤 spine data，同时 SkinChanger `预加载排队 N 个皮肤场景` |
| 联机开局卡死 | `Preloading 'characters=...' count=46` 不完成 → 被主机超时踢出 |

SkinChanger 相关日志行：`[SkinChanger] [性能] BuildBaselineDependencyOverlay ...`、`预加载排队 17 个皮肤场景（队列上限 48）`、`被 __base__ 接管，退回原版`。

---

## 4. 关键证据：每次卡死时线程在加载什么

抓取方式：`cdb -pv`（非侵入）抓原生栈；再扫描卡住线程栈内存中的 UTF-32 `res:` 字符串（见 §8）。

| 日志# / 时间 | 配置 | 卡住线程 → 正在加载的资源 | 资源来源 |
|---|---|---|---|
| 22 / 11-14 19:03 退出卡 | 34 mods | T2 `card_portrait_blur_material.tres → portrait_blur.gdshaderinc → Blur.gdshader → canvas_group_mask_blur.gdshader`；T5 `timeline_screen.tscn → … → epoch_slot.tscn::ShaderMaterial_blurx → Blur.gdshader` | **原版**资源（非 Spine！） |
| 24 / 19:40 联机卡 | 50 mods | T2 `rest_site_necrobinder_skel_data.tres → restsite_necrobinder.atlas → .png`；T3 `rest_site_osty_skel_data → restsite_osty.atlas → .png` | 皮肤包 Moe-Necrobinder / necrobinderHpWear |
| 25 / 20:03 | 48 mods | T7 `characterselect_defect_skel_data.tres → .atlas → .png`；T8 `defect_skel_data.tres → Defect.atlas → Defect.png` | defectHpWear.pck |
| 27 / 11-15 01:23 | 89 mods（怪物每种 1 个） | T6/T7 同上 defect；T6 栈上另有 ceremonial_beast、silent skel_data | defectHpWear 等 |
| 28 / 01:34 | 59 mods（11 个 Boss 皮 + 狐猎开） | T4 `res://silent/animations/rest_site/rest_site_silent_skel_data.tres` + `characterselect_defect` 链；T5 `defect_skel_data.tres → Defect.atlas → Defect.png` | silentKaguyaHpWear.pck + defectHpWear |
| 29 / 01:49 | 58 mods（11 Boss 皮，**狐猎关**） | T6 `ceremonial_beast_skel_data.tres → .atlas → .png`；T7 `ceremonial_beast_skel_data_hurt.tres → _hurt.atlas → _hurt.png`（两个 skel_data 的子资源 id 都是 `SpineAnimationMix_aujt4`） | 怪物包 `汐の雨い 半血爆衣 yss` |

原生栈形态（28/29 一致）：
```text
主线程:   NtDelayExecution ← SleepEx ← SlayTheSpire2 (帧循环，栈很浅)
工作线程: NtDelayExecution ← SleepEx ← SlayTheSpire2 ×7 ← libspine_godot(spine_godot_library_init 附近) ×8 ← SlayTheSpire2 ×17 ← thread_start
```
即：WorkerThreadPool 任务 → Godot 加载 skel_data → spine-godot 内部再调用 Godot 加载 atlas/png → 在 Godot 侧**睡眠轮询等待**某个永远不完成的加载。两个线程各自如此。无引擎符号，函数名未解析。

托管侧（dotnet-dump / dotnet-stack，11-14）：无托管锁被持有，所有 async 链在等下一帧/加载完成 → 死锁在原生 ResourceLoader 层。

`ERROR: Invalid Task ID at: wait_for_task_completion (core/object/worker_thread_pool.cpp:418)`：出现在 05/06/08/20/26/27/28/29 号日志里，但正常启动（06、20）也有，卡死（01/02/04/07…）也有没有的 → **弱相关**，仅供参考。

---

## 5. 统计与已排除项

| 条件 | 结果 |
|---|---|
| SkinChanger 关闭 | 启动全部正常（样本少，约 3 次），退出正常 |
| SC 开 + 关闭全部外观皮肤（只留卡图包） | 2/2 正常（日志 15/16） |
| SC 开 + 只加回 7 个角色皮 | 卡（日志 17） |
| SC 开 + 每角色 1 个战损皮、无怪物皮 | 多数正常，约 1/8 卡 |
| SC 开 + 选中大量怪物皮（overlay ≈450 MB，预加载队列 48） | 2/2 卡 |
| 11 个 Boss 皮 ± 狐猎 | 2/2 卡（28、29） |
| 有狐猎 Kaguya 包（11-14） | 6/7 卡 + 1 崩溃 |

**已排除 / 已否定**：
- MoreNecrobinderAnimations：关掉仍卡。
- CrossVersionCompat：在 IntroLogo 之前就已完成，未直接涉及。
- "Kaguya 包 id=`silent` 冲突"：否定。
- "某个怪物包损坏"：三次卡在三个不同资源上，单包损坏可能性低（推断）。
- "未选中的重复怪物包数量"：SC 只挂载**被选中**的皮肤；删未选中包后 overlay 451.7 → 449.5 MB，几乎不变。

**当前假设（推断，待代码验证）**：SC 挂载的 overlay pck 越大、同时排队的 Spine 场景越多，游戏原生预加载 + SC 预加载在 WorkerThreadPool 上并发加载 Spine 资源时，越容易在 Godot ResourceLoader 的嵌套/共享依赖加载中互相等待而死锁。退出卡死里出现的原版 Blur shader 嵌套加载说明**这类死锁不限于 Spine**，Spine 只是最常见的受害者。

---

## 6. 建议接手 agent 用代码核实的问题

1. **SkinChanger（反编译 `Gurio.SkinChanger.dll`，ILSpy/ilspycmd 均未安装）**
   - overlay pck 是何时、在哪个线程挂载的（`ProjectSettings.LoadResourcePack`？`BuildBaselineDependencyOverlay`）？挂载是否与游戏 `Preloading 'IntroLogo'` 并发？
   - 预加载队列（"队列上限 48"）用的是 `ResourceLoader.LoadThreadedRequest` 吗？`use_sub_threads` 参数是什么？有没有对同一路径在主线程同步 `Load` + 线程中异步加载并存？
   - 是否会对**原版路径**的资源做替换/重映射（`res://animations/characters/defect/...` 这类路径由皮肤包直接覆盖）？覆盖期间如果同一路径已在加载中会怎样？
   - `.84` → `.99` 的变更是否涉及预加载逻辑。
2. **sts2.dll 的 PreloadManager**（打 `Preloading '<group>' assets... count=` 的类）：用什么 API 并发加载、是否 sub-threads、等待方式。
3. **Godot 4.5.1 ResourceLoader / WorkerThreadPool**：嵌套 `ResourceLoader::load` 在工作线程中等待另一个尚未调度的加载任务时的已知死锁问题（关注 `_load_complete_inner`、`wait_for_task_completion`、`use_sub_threads`）。spine-godot 的 `SpineSkeletonDataResource` / `SpineAtlasResource` 加载 atlas/png 走的是 `ResourceLoader::load` 吗？
4. **`AssetCache.LoadAsset` / `GetScene` 的 Harmony 补丁是谁打的**（SkinChanger？RitsuLib？），补丁里有没有对"正在线程化加载中的路径"再发起同步 `ResourceLoader.Load`（这会让主线程加入等待，见日志 31/32）。
5. **为什么固定是"2 个工作线程"卡住**：Godot WorkerThreadPool 低优先级任务的线程上限是多少？ResourceLoader 线程化加载默认走低优先级池吗？如果上限是 2，那么两个任务各自在等一个排不上队的子加载，就会构成死锁。这一点最值得先查。
6. **`main_menu.tscn` 为什么会嵌套加载机器人 Spine**（日志 30 T7 栈）：皮肤包 / SC overlay 是否把选角或主菜单场景的依赖替换成了皮肤的 skel_data？
7. 可行规避的方向（待验证）：让 SC 的预加载在游戏 `Common Complete` 之后再串行执行；或降低并发（队列上限 / 不用 sub-threads）；或避免在工作线程里对已在加载中的路径再次同步等待。

**验证方法建议**：改完 SC 的配置或代码后，用 §9 当前这套"16 个怪物皮"（排队 31，1/1 卡）或 41 个怪物皮（排队 48，2/2 卡）做复现。判断标准是日志里 `Preloading 'Common' Complete` **和** SC 的 `预加载完成` 都出现，连续 3 次才算修好。

---

## 7. 另一个独立问题（与黑屏无关，别混在一起）

响木天音 MaidenSuccubus（WS 3811028356）开局没手牌 / 联机队友抽不了牌：`MaidenSuccubus.Core.Intents.PersonalNativeAdapters.RequiredPatchesInstalled()` 抛出 `Sequence contains no matching element`（`Enumerable.Single` 无匹配），调用方 `MaidenSuccubusCharacter.AfterPlayerTurnStartEarly`，回合循环因此中断。当时开着 CrossVersionCompat（它会在加载前重写所有 mod 程序集）。头号嫌疑是 CVC，**未证实**。用户目前已关闭 CVC 和 MaidenSuccubus。日志：`%TEMP%\sts2scan\nohand_195454.log`、`nocards_201452.log`。

---

## 8. 工具与操作约束

- **改 settings.save / skin_changer.json 只能在游戏关闭时**（游戏退出时会重写 settings.save）。文件是 **CRLF**，必须用 Python 按字节精确替换，**禁止 Git Bash `sed -i`**（会剥掉 CR）。SkinChanger 必须排在 `STS2-RitsuLib` 之后、皮肤包之前（当前 mod_list 第 2 位）。
- SkinChanger 在皮肤包被禁用时**会自己改写 `Selections`**（退回 `__base__` 或换成别的包）。恢复用户原选择用备份：`skin_changer.json.before_charskin_off`（最原始）、`.before_bisect`。
- 脚本（`%TEMP%\sts2scan\` = `C:\Users\ZuiMao\AppData\Local\Temp\sts2scan\`）：
  - `list.py`：列出启用 / 禁用 mod。
  - `toggle.py <label> <id> on|off`：单个开关，自动备份 `settings.save.before_<label>`。
  - `bisect_set.py <label> [pack ids...]`：只启用给定的怪物包（`monster_plan.json` 的 keep 列表内），其余关掉。
  - `res.cdb` + `decode.py`：扫描线程栈里的 `res://` 路径。
- **抓卡死现场**（游戏卡住时、结束进程前）：
  ```bash
  CDB="/c/Program Files/Windows Kits/10/Debuggers/x64/cdb.exe"
  "$CDB" -pv -p <PID> -c "~* kc 40; qd" > native_<PID>.txt          # 原生栈
  awk '/^ *[0-9]+  Id:/{t=$1} /libspine/{print t}' native_<PID>.txt | sort -u   # 找出卡在 spine 的线程号
  sed 's/~2s/~<A>s/; s/THREAD2/THREAD<A>/; s/~5s/~<B>s/; s/THREAD5/THREAD<B>/' res.cdb > res_now.cdb
  "$CDB" -pv -p <PID> -cf res_now.cdb > res_<PID>.txt
  python -I decode.py res_<PID>.txt                                  # 输出各线程上的 res:// 路径
  ```
  托管栈：`D:\TOOL\sts2_diag_tools\dotnet-stack.exe report -p <PID>`（`dotnet-dump collect` 在这个嵌入式运行时上会失败）。

---

## 9. 当前状态 / 正在进行的测试

- **当前配置（02:41 卡死的那套，可直接用来复现）**：64 个 mod 启用（列表显示 65，是因为 `voicemod` 有一条重复记录；DLL 在 `全角色娘化语音\君宝` 下有重复副本）。
  - SC 第 2 位（`STS2-RitsuLib` 之后）。
  - 怪物皮 16 个：11 个 Boss 皮，加上 `汐の雨い 半血爆衣 hm`、`汐の雨い 半血爆衣 xks`、`会画画的Small_day nsfw ynbb`、`会画画的Small_day sfw sxjc`、`Kiteseven nsfw csqzc`。
  - 角色皮：`defectHpWear`、`silentKaguyaHpWear`、`succubusIroncladHpWear`、`regentHpWear`。
- **快速切换到其它测过的配置**：
  - "11 Boss + 狐猎 + 机器人"（较稳）：`python -I bisect_set.py <label> from:before_bisect_sc99_add5`
  - 41 个怪物皮（必卡）：`python -I bisect_set.py <label> from:before_bisect_boss`
- 02:41 那局游戏进程（PID 20652）卡死时已抓栈，用户随后会结束它。
- `voicemod` 每次启动初始化失败：`Parameter "serializableRun" not found in method NRun::ShowGameOverScreen`，是游戏版本不兼容，与死锁无关。

---

### 9.1 SkinChanger `.99`（`1.0.31.99`）测试结果（02:00–02:45）

| 时间 | 配置 | 结果 |
|---|---|---|
| 02:00 | 5 个 Boss 皮，狐猎关，52 mods，SC 预加载排队 15 | 正常（日志 30c） |
| 02:11 | 11 个 Boss 皮 + 狐猎，59 mods，排队 21 | 日志在 SC 延后初始化中途中断，未到 IntroLogo，无退出记录 → 疑似卡死后被结束（未抓栈，日志 30a） |
| 02:13 / 02:17 | 同上 | 正常（日志 30b） |
| 02:20 | **89 mods（= 日志 27 的黑屏配置），排队 48** | 主菜单可见，但 `Common` 预加载 >2 分钟未完成，SC `预加载完成` 未出现 → **后台死锁**。T4 `characterselect_defect_skel_data → .atlas → .png`；T7 `main_menu.tscn → defect_skel_data.tres → Defect.atlas → Defect.png`，与 `.84` 的 25/27/28 **完全相同**（日志 30、diagnostics/30_*） |

结论：`.99` **没有修复**死锁，只是在皮肤少的时候卡的概率低一些（推断，样本少）。另外注意 T7 栈上有 `res://scenes/screens/main_menu.tscn`：主菜单场景加载时会嵌套加载机器人的 Spine 资源。

**02:24 同一会话彻底冻结**（Responding=False，CPU≈0）：用户在选角界面点机器人。主线程托管栈：
```text
NCharacterSelectButton.Select → NCharacterSelectScreen.SelectCharacter_Patch11
→ STS2RitsuLib CharacterSelectBackgroundRuntimeFactoryPatch.CreateControlFromCharacterSelectBgPath
→ ContentAssetOverridePatchHelper.ResolveScene/TryGetCachedResource
→ AssetCache.GetScene_Patch1 → AssetCache.GetAsset → AssetCache.LoadAsset_Patch1 → godot_icall (ResourceLoader.Load) → 原生 SleepConditionVariableSRW
```
主线程栈上的资源：`char_select_bg_defect.tscn → characterselect_defect_skel_data.tres`，**正是工作线程 T4 卡住的那个资源**。即：后台死锁一旦发生，任何之后同步加载同一资源的操作（这里是选角背景）都会把主线程一起拖死，这就是玩家感受到的"中途卡死"。证据：日志 31、`diagnostics/31_*`。

**02:30 关掉 defectHpWear（88 mods，其余同上）**：`Common` 仍然从未完成；打开怪物图鉴时冻结。主线程：`NCompendiumSubmenu.OpenBestiary → NBestiary.CreateEntries → SelectMonster_Patch2 → NBestiaryLayoutDefault.Setup → NCreature.Create → AssetCache.GetScene → LoadAsset_Patch1 → 原生等待`（资源 `creature.tscn`）。工作线程：T3 `brute_ruby_raider.tscn → brute_ruby_raider_skel_data.tres → .atlas → .png`（包 `anaertailin nsfw 桂冠 - 半卸甲 jlzbt`）；T4 `bygone_effigy_ske_hurt.tres → bygone_effigy_hurt.atlas → .png`（包 `汐の雨い 半血爆衣  jrdx`）。**去掉机器人皮后，卡住的对象换成了怪物皮** → 进一步支持"通用并发竞态，而非单包问题"。证据：日志 32、`diagnostics/32_*`。

**02:37 恢复"11 Boss + 狐猎 + 机器人"** → 正常（`Common` 10.6s 完成）。退出时引擎收尾仍卡住，但 `.99` 新增看门狗：`退出 12 秒仍未结束（存档已写完），强制结束进程`（日志 32b）。

**02:41 加到 16 个怪物皮（64 mods，SC 预加载排队 31）** → 卡在 `IntroLogo`。T5 `ironclad_merchant_skel_data.tres → ironclad_shop.atlas → .png`；T6 `rest_site_ironclad_skel_data.tres → restsite_ironclad.atlas → .png`（角色皮 `succubusIroncladHpWear`）。受害者又换成了战士皮（日志 33、`diagnostics/33_*`）。

`.99` 汇总：11 Boss 配置（排队 21）3 正常 / 1 疑似；16 怪物（排队 31）1/1 卡；41 怪物（排队 48）2/2 卡。卡死概率随 SC 预加载数量明显上升。

## 10. 文件索引

- 给作者的反馈文档：`SkinChanger_report_20261114\SkinChanger_启动卡死问题反馈.md`（第 00000 节最新，日志 23–33 尚未写进文档）。
- 日志：`SkinChanger_report_20261114\logs\01_…` ～ `33_…`（文件名即结论；`30a/30b/30c/32b` 是 `.99` 的正常或疑似样本）。
- 诊断：`SkinChanger_report_20261114\diagnostics\`（`18_/Q1_/Q2_/24_/25_/27_/28_/29_/30_/31_/32_/33_*`：原生栈、托管栈（31/32 有主线程托管栈）、res 扫描原始输出、解码后的资源路径）。
- 原始抓取数据：`D:\TOOL\sts2_diag_tools\native_<PID>.txt`、`res_<PID>*.txt`、`stacks_<PID>.txt`。
- 配置快照：`SkinChanger_report_20261114\config\`。
- 诊断工具与旧数据：`D:\TOOL\sts2_diag_tools\`（`native_*.txt`、`res_*.txt`、`stacks_*.txt`、`symcache\`、7.4 GB 完整 dump `sts2_hang_20484_full.dmp`，11-14 的一次死锁）。
