# 皮肤切换器（Gurio.SkinChanger）启动黑屏 / 资源加载死锁：根因分析与修复建议

> 反馈对象：皮肤切换器作者
> 反馈版本：工坊 3787302680，内部版本 `1.0.31.99`（源码提交 `2ad1534`）；对照 `1.0.31.84`（`264332c`）
> 日期：2026-11-15
> 证据强度标注：**【已验证】** = 源码/日志/实验直接证实；**【推断】** = 与证据一致但未直接证实；**【未知】** = 尚未定位

---

## 0. 结论速览

1. **卡死的直接原因是 SkinChanger 的后台预加载（`SkinPreloader`）把 Godot 线程池的"低优先级加载名额"占满了**，游戏自己的预加载（`IntroLogo` / `MainMenuEssentials` / `Common`）只能排队，永远轮不到 → 黑屏或主菜单可见但后台加载永不完成。【已验证：实验 + 线程栈 + 源码】
2. Godot 4.5.1 默认低优先级名额 = `线程数 × 0.3`，在 8 线程 CPU（i5-10300H）上**正好是 2**；每次卡死时**正好 2 个**工作线程卡在 Spine 资源加载里，其余线程空闲。【已验证】
3. 把名额从 2 提到 4（游戏目录放 `override.cfg`，`low_priority_thread_ratio=0.6`）后：**冷缓存启动 9/9 正常**（此前同类冷启动 13 次卡 9 次），包括预加载排满 48 个、146 个 Mod 的最重配置。【已验证】
4. `1.0.31.99` **没有修复**这个问题：新增的 `Paused` 只在选角重建时生效，`CancelAll` 只在退出时调用且取消不了引擎里已发出的加载。【已验证：源码 diff】
5. **建议修复**：预加载改为 `useSubThreads: false`、同一时间只在途 1 个、等游戏 `Common` 预加载完成后再开始、启动时只预加载与当前场景相关的分组。详见第 5 节。游戏官方自己就因为"并发后台加载会在共享子资源上竞态"而把 VFX 改成了逐个串行加载（第 3.6 节）。

---

## 1. 环境

| 项 | 值 |
|---|---|
| 游戏 | Slay the Spire 2 **v0.113.0 public-beta** |
| 引擎 | `MegaDot v4.5.1.m.14.mono.custom_build`（Godot 4.5.1 mono 魔改），D3D12 |
| 硬件 | Intel **i5-10300H（4 核 8 线程）**，GTX 1650 Ti 4GB |
| SkinChanger | 工坊 3787302680，`1.0.31.84`（日志 21–29）→ `1.0.31.99`（日志 30–46，01:58 自动更新） |
| Mod 数量 | 测试配置 52～148 个（详见第 4 节） |
| 前置 | `STS2-RitsuLib` 第 1 位，SkinChanger 第 2 位 |

---

## 2. 现象

| 类型 | 日志特征 |
|---|---|
| 启动黑屏（主问题） | 停在 `Preloading 'IntroLogo' assets... count=3` 或 `Preloading 'MainMenuEssentials' assets... count=5`，永无 `Complete`；SC 也不打 `预加载完成`。进程未崩，CPU≈0 |
| 后台死锁 | 主菜单可见，但 `Preloading 'Common'` 永不 `Complete`，SC `预加载完成` 不出现（日志 30、32） |
| 退出卡死 | `Steamworks shutdown succeeded!` 之后进程不退出（日志 22），`.99` 起被 12 秒强杀兜住 |
| 选角/图鉴冻结 | 主线程同步加载某资源时永久等待（日志 31、32），见 3.7 |

规律：**卡住的具体资源每次都不同**（机器人、狐猎、仪式兽、红宝石劫掠者、旧日雕像、铁甲商人/火堆……），去掉一个包就换另一个包被卡 → 不是某个皮肤包损坏。

---

## 3. 根因分析

### 3.1 卡住的线程在执行的是 SkinChanger 的预加载，不是游戏的预加载【已验证】

- 游戏的 `IntroLogo` 资源集只有 `NTransition.AssetPaths` + `NLogoAnimation.AssetPaths`（共 3 个，`AssetSets` 静态构造），**不含任何 Spine 角色/怪物资源**。
- 但卡死现场两个工作线程加载的都是 `defect_skel_data.tres`、`ceremonial_beast_skel_data.tres`、`ironclad_merchant_skel_data.tres`、`brute_ruby_raider_skel_data.tres` 等——都是**玩家选中的皮肤场景**，即 `SkinPreloader` 的队列内容。
- 日志时序（以日志 29 为例）：`[性能] 预加载排队 17 个皮肤场景` → SC 启动阶段结束 → `Preloading 'IntroLogo' assets...` → 永无 `Complete`，SC 也永无 `预加载完成`。**SC 预加载在游戏 IntroLogo 之前就已发出。**

### 3.2 SkinChanger 预加载的实现【已验证：`2ad1534` 源码】

`STS2SkinChanger/Core/SkinPreloader.cs`：

```csharp
private const int MaxQueuedPaths = 48;      // :22
private const double TickSeconds = 0.15;    // :23
private const int RequestsPerTick = 2;      // :24
...
var error = ResourceLoader.LoadThreadedRequest(path, "", useSubThreads: true);   // :187
```

- 每 0.15 秒发 2 个请求，**不限制在途数量**，最多 48 个；`useSubThreads: true` 会让每个场景的外部依赖再拆成子任务投进线程池。
- 调用点：`SkinService.cs:6677`，位于 `MountOverlayCore`（`:6334`）末尾，`SkinPreloader.Schedule(catalog, effectiveSelections, groups)`——`groups` 是**全部已挂载分组**（日志里 70～115 个），所以主菜单阶段会去加载机器人、铁甲商店/火堆、各种怪物的 Spine 资源（这也回答了"主菜单为什么会加载机器人皮肤资源"）。

对照游戏自己的预加载（反编译 `MegaCrit.Sts2.Core.Assets.AssetLoadingSession`）：

```csharp
ResourceLoader.LoadThreadedRequest(result, "", useSubThreads: false, ResourceLoader.CacheMode.Reuse)
```

**游戏用 `useSubThreads: false`，SkinChanger 用 `true`。**（该反编译来自 11-07 的 `sts2.dll`；当前 11-13 版本未重新反编译，但 3.6 引用的同类官方注释已在当前 `sts2.xml` 中核对存在。）

### 3.3 Godot 4.5.1 线程池的低优先级名额上限【已验证：Godot 4.5.1-stable 源码】

- `core/object/worker_thread_pool.cpp:830`：
  `max_low_priority_threads = CLAMP(p_thread_count * p_low_priority_task_ratio, 1, p_thread_count - 1);`
- `core/config/project_settings.cpp:329`：`threading/worker_pool/low_priority_thread_ratio` 默认 **0.3**；游戏 `.pck` 中搜不到任何 `threading/worker_pool/*` 覆盖项 → 用默认值。
- `main/main.cpp:1955-1957`：线程池在读取项目设置（含 `override.cfg`）之后初始化。
- `core/io/resource_loader.cpp:655`：后台加载通过 `add_native_task(&ResourceLoader::_run_load_task, load_task_ptr)` 投递，**未传高优先级** → 走低优先级队列。
- `worker_thread_pool.cpp:249`：低优先级任务超过上限时进入 `low_priority_task_queue` 排队，**空闲线程不会去拿**。

**8 线程 × 0.3 = 2.4 → 2 个名额。**这两个名额被 SC 的两个 Spine 加载占住后，所有其它后台加载（包括游戏 IntroLogo 那 3 个跟 Spine 毫无关系的资源）都只能排队。

说明：以上按 Godot 4.5.1 原版源码推算；`add_native_task` 的 `p_high_priority` 默认值未能从头文件直接核对（下载失败），MegaDot 是否改过公式也未知。但"每次正好 2 个线程卡住"以及"改比例后问题消失"（第 4 节）都与该推算一致。

### 3.4 线程栈证据【已验证：cdb 非侵入抓栈，日志 33】

`diagnostics/33_native_stacks_20652.txt`：

| 线程 | 状态 |
|---|---|
| t1、t2、t3、t4、t7、t8、t9（7 个，同一线程池形态） | **空闲**：`SleepConditionVariableSRW` |
| **t5、t6（正好 2 个）** | **卡住**：`ntdll!NtDelayExecution ← KERNELBASE!SleepEx ← SlayTheSpire2 ×N ← libspine_godot ← …` |
| t0 主线程 | 帧循环（后台 30 FPS 限帧的 Sleep） |

t5/t6 栈内存里扫出的资源路径（`33_卡住线程正在加载的资源.txt`）：

```text
THREAD5  ironclad_merchant_skel_data.tres → ironclad_shop.atlas → (.spatlas) → ironclad_shop.png
THREAD6  rest_site_ironclad_skel_data.tres → restsite_ironclad.atlas → (.spatlas) → restsite_ironclad.png
```

日志 25、27、28、29、30、32 的栈内存扫描结果同样是 **正好 2 个线程卡在"骨骼数据 → 图集 → 贴图"链的最深处（.png）**（`diagnostics\*_卡住线程正在加载的资源.txt`）。"其余池线程全部空闲"这一点是在日志 33 的完整栈上逐线程核对的，其它几次未逐线程核对。

### 3.5 为什么这两个加载不放手【未知，但不影响修复方向】

- Godot 4.5.1 自己的等待（`wait_for_task_completion` → `_wait_collaboratively`，`worker_thread_pool.cpp:498`）用的是条件变量，并且等待期间会**把排队中的低优先级任务提出来自己执行**（`_try_promote_low_priority_task`，`:329`）——如果走的是这条路，不会因名额耗尽而死锁。
- 但现场两个线程是在 `SleepEx` 里**睡眠轮询**（Windows `Sleep` 的实现），这种等待不会顺手处理别的任务，名额就一直被占着。
- 原版 4.5.1 在工作线程的资源/纹理加载路径上没有这种轮询（`delay_usec` 只出现在主线程 `load_threaded_get` 和关停清理里）。所以轮询点大概率在 **MegaDot 魔改代码或 spine-godot 插件**里，无符号无法定位。
- 另一个可疑点：栈上同一张贴图同时出现 `res://animations/...png` 和 `res:///animations/...png`（多一个斜杠）两种路径，是否导致同一文件以两个缓存键并发加载，未验证。

结论：**根因链 = 并发的 Spine 嵌套加载里存在一个非协作的轮询等待 × 低优先级名额只有 2 × SC 在游戏预加载之前并发发出大量 `useSubThreads` 请求。**前者是引擎/插件层问题，SC 无法修；后两者 SC 可以规避。

### 3.6 游戏官方对同类问题的处理方式（强烈佐证）【已验证：当前 `data_sts2_windows_x86_64/sts2.xml`，11-13 版本】

`AssetLoadingSession.ProcessVfxQueue` 的官方注释：

> Load VFX scenes one at a time via the async threaded loader. Many VFX scenes share ext_resources (e.g. `canvas_item_material_additive_shared.tres` is used by 162 scenes), and **loading them concurrently causes Godot's threaded loader to race on shared sub-resource paths.**

即 MegaCrit 自己已经发现"并发后台加载 + 共享子资源 = 竞态"，所以 VFX 改为**一次只加载一个**，普通资源也用 `useSubThreads: false`。SkinChanger 的预加载恰好是反过来的做法（并发 + sub-threads）。

### 3.7 相关现象的解释

- **退出卡死（日志 22）**：Godot 关停时 `ResourceLoader::clear_thread_load_tasks`（`resource_loader.cpp:1385-1404`）会每 1 ms 轮询，直到没有 `IN_PROGRESS` 的加载。启动期已卡死的加载永远不结束 → 进程不退出。`.99` 的 12 秒强杀只是兜底。
- **`.99` 的 `CancelAll` 无效**（`QuitCacheCleanup.cs:54` 调用）：它只清空 C# 侧的 `Pending/InFlight/Known` 集合；Godot **没有**取消后台加载的 API，已发出的请求照样在引擎里卡着；清掉 `InFlight` 后这些 token 再也不会被 `LoadThreadedGet` 取回。
- **`.99` 的 `Paused` 不覆盖启动**：只在 `ContextualSkinControls.RunRefresh`（`:1963`）期间置位，启动阶段不经过。
- **选角点机器人冻结（日志 31）**：主线程托管栈 `NCharacterSelectButton.Select → SelectCharacter_Patch11 → STS2RitsuLib CharacterSelectBackgroundRuntimeFactoryPatch.CreateControlFromCharacterSelectBgPath → ContentAssetOverridePatchHelper.ResolveScene → AssetCache.GetScene_Patch1 → AssetCache.GetAsset → AssetCache.LoadAsset_Patch1 → ResourceLoader.Load`，原生侧 `SleepConditionVariableSRW`。主线程同步要的 `characterselect_defect_skel_data.tres` 正被卡死的工作线程持有 → 主线程跟着冻结。
- **`AssetCache` 补丁归属**：SC 自己 Postfix 了 `AssetCache.GetScene` / `GetTexture2D`（`ContextualSkinControls.cs` 约 4051 行 `CachedSceneResultPatch` / `CachedTextureResultPatch`）；私有方法 `LoadAsset` 的补丁目标字符串在 137 个 Mod DLL 中只出现在 `STS2-RitsuLib.Runtime.dll`（UTF-16），大概率是 RitsuLib。【推断：字符串证据，未反编译确认】
- `ERROR: Invalid Task ID at: wait_for_task_completion (worker_thread_pool.cpp:418)`：SC 关闭时的正常启动里也出现过 → 弱相关，仅供参考。

---

## 4. 验证实验：提高低优先级名额

### 4.1 方法

在游戏根目录放 `override.cfg`（Godot 会读取 exe 同目录下的该文件，`project_settings.cpp:700`）：

```ini
[threading]
worker_pool/low_priority_thread_ratio=0.6
```

8 线程 × 0.6 = 4.8 → **4 个名额**。

- **生效验证**：临时在同一文件加 `[debug] settings/stdout/verbose_stdout=true`，进程命令行无任何参数（`"...\SlayTheSpire2.exe"`），日志仍出现大量 `Loading resource:` → **MegaDot 确实读取了 `override.cfg`**（日志 36）。验证后已删除该临时项。
- 注意：`WorkerThreadPool: N threads, M max low-priority.` 这行是 `print_verbose`，在文件日志挂上之前（`main.cpp:2145`）就打印了，**不会出现在 `godot.log` 里**，不能用它验证。

### 4.2 重要混杂因素：覆盖包缓存冷/热

统计发现：**几乎所有卡死都发生在"冷缓存"启动**（改配置后第一次启动，日志出现 `皮肤包或游戏版本有变化：已清空外观覆盖包缓存。`，SC 需要重写约 450 MB 覆盖包），热缓存启动本来就较少卡。因此对照实验**全部在冷缓存下进行**：每次启动前删除 `%APPDATA%\SlayTheSpire2\Gurio.SkinChanger\overlay-cache\` 下所有 `.pck` 及 `stamp.txt`，并确认日志出现"已清空外观覆盖包缓存"。

（冷启动更易卡的机制【推断】：多出的数秒写盘/冷读改变了时序，使 SC 预加载与游戏预加载的重叠更严重。）

### 4.3 结果

判定标准：同一局日志中同时出现 `Preloading 'Common' Complete` 与 SC `预加载完成：`。

**不加 override（日志 21–33，排除联机日志 24 与日志 30 同会话的 31）**

| 日志 | 缓存 | SC 预加载排队 | 结果 |
|---|---|---|---|
| 21 | 冷 | 6 | 正常 |
| 22 | 冷 | 8 | **卡**（Common 未完成，退出卡死） |
| 23 | 冷 | 8 | 正常 |
| 25 | 热 | 6 | **卡**（MainMenuEssentials） |
| 26 | 冷 | 48 | **卡**（MainMenuEssentials，146 个 Mod） |
| 27 | 冷 | 48 | **卡**（IntroLogo） |
| 28 | 冷 | 21 | **卡**（MainMenuEssentials） |
| 29 | 冷 | 17 | **卡**（IntroLogo） |
| 30 | 冷 | 48 | **卡**（主菜单可见，Common 未完成） |
| 30a | 冷 | 21 | **疑似卡**（日志中断，未到 IntroLogo） |
| 30b | 热 | 21 | 正常 |
| 30c | 冷 | 15 | 正常 |
| 32 | 冷 | 48 | **卡**（Common 未完成，随后图鉴冻结） |
| 32b | 冷 | 21 | 正常 |
| 33 | 冷 | 31 | **卡**（IntroLogo） |

→ **冷缓存 13 次，卡 9 次（约 69%）**。

**加 override（ratio 0.6），全部 `1.0.31.99`**

| 日志 | 配置 | 缓存 | 排队 | 结果 |
|---|---|---|---|---|
| 34–37 | 16 怪物皮（= 日志 33 配置） | 热 | 31 | 4/4 正常 |
| 38–40 | 16 怪物皮（= 日志 33 配置） | **冷** | 32 | **3/3 正常** |
| 41–43 | 41 怪物皮，89 Mod（= 日志 27/30 配置） | **冷** | **48** | **3/3 正常** |
| 44 | 全加，146 Mod（= 日志 26 配置） | **冷** | **48** | **1/1 正常** |
| 45–46 | 全加 + 贞德战士 + Moe-Necrobinder + necrobinderHpWear，148 Mod | **冷** | **48** | **2/2 正常** |

→ **冷缓存 9/9 正常**。按此前 69% 卡死率，连续 9 次都"碰巧不卡"的概率约 0.31⁹ ≈ 0.003%。

### 4.4 实验说明了什么、没说明什么

- 说明了：**"低优先级名额被占满"是卡死的必要环节**——只改名额这一个变量，卡死就消失了。
- 没说明：名额加到 4 只是让"同时卡住的加载数 ≥ 名额"更难发生，**轮询等待本身还在**；换更少线程的 CPU、更多并发，仍可能复现。这是规避，不是修复。
- `override.cfg` 只能由玩家放在游戏目录；**Mod 无法在运行时改**（线程池在 Mod 加载之前就初始化了）。

---

## 5. 修复建议（按优先级）

### P1：预加载串行化 + 关闭 sub-threads（改动最小，最关键）

```csharp
private const int RequestsPerTick = 1;
private const int MaxInFlight = 1;   // 新增：同一时间只在途 1 个

// Tick() 内：
CompleteFinishedRequests();
if (InFlight.Count >= MaxInFlight) { /* 等上一个完成 */ }
else if (Pending.Count > 0)
{
    var path = Pending.Dequeue();
    if (!ResourceLoader.HasCached(path))
    {
        var error = ResourceLoader.LoadThreadedRequest(path, "", useSubThreads: false);
        if (error == Error.Ok) InFlight.Add(path); else _failed++;
    }
    else _loaded++;
}
```

效果：SC 任何时刻最多占用 **1 个**低优先级名额，且不再派生子任务；在 8 线程机器上始终给游戏留至少 1 个名额。与游戏官方 VFX 串行加载的做法一致（3.6）。

### P2：等游戏自己的预加载完成后再开始

启动阶段不要在 `MountOverlayCore` 末尾立即开跑。建议门控条件：游戏 `Common` 会话完成（`Preloading 'Common' Complete`）后再开始派发，并带超时兜底（例如 60 秒后无论如何开始）。

实现思路（任选）：
- Harmony Postfix `AssetLoadingSession.Process`，读私有字段 `_name`（`AccessTools.FieldRefAccess`），当 `_name == "Common" && __instance.IsCompleted` 时置一个静态标志；`SkinPreloader.Tick` 在标志未置位前只 `EnsureTimer()` 返回（与现有 `Paused` 分支同样写法）。
- 注意 `PreloadManager.LoadCommonAndMainMenuAssets` 是 `async` 且内部未 `WaitForCompletion`，对它做 Postfix 只能拿到"开始"，拿不到"完成"。

### P3：启动时只预加载与当前场景相关的分组

现在启动时 `groups` = 全部已挂载分组（70～115 个），主菜单阶段就去加载所有角色的战斗/商店/火堆骨骼和所有怪物。建议启动只预加载**选角界面会用到的**（角色选角骨骼/背景），战斗/商店/火堆/怪物资源在**开局**或进入对应房间前再排队。这同时缩短冷启动时间。

### P4：退出与取消逻辑

- `CancelAll` 不要清 `InFlight`：清了也取消不了引擎里的加载，反而丢失 token。建议只停止派发新请求（清 `Pending`），已在途的保留。
- P1 落地后在途最多 1 个，退出时等待时间可控，12 秒强杀可保留作最后兜底。

### P5：给玩家的临时规避（作者修复前）

在 `SlayTheSpire2.exe` 同目录新建 `override.cfg`，内容见 4.1。删除该文件即恢复。可写入工坊说明/FAQ。

---

## 6. 作者如何复现

1. **让名额回到 2**：8 线程机器默认即可；线程更多的机器在游戏目录放 `override.cfg`，把 `worker_pool/low_priority_thread_ratio` 设为 `2 / 你的逻辑线程数`（例如 16 线程设 `0.13`）。
2. 选中较多 Spine 皮肤，使日志出现 `[性能] 预加载排队 31～48 个皮肤场景`。
3. **冷缓存**：启动前删除 `%APPDATA%\SlayTheSpire2\Gurio.SkinChanger\overlay-cache\` 下全部文件（含 `stamp.txt`）。
4. 启动，观察是否停在 `Preloading 'IntroLogo'` / `'MainMenuEssentials'`，或 `Common` 永不完成。卡住时用 `cdb -pv -p <PID> -c "~* kc 40; qd"` 抓栈，可见正好 N 个（= 名额数）线程在 `SleepEx ← … ← libspine_godot`。
5. **验收修复**：同配置冷缓存连续 ≥5 次，每次都出现 `Preloading 'Common' Complete` 与 `预加载完成：`。建议在**名额 = 2 且不放 `override.cfg`** 的条件下验收，避免被规避手段掩盖。

---

## 7. 尚未确认 / 次要问题

| 问题 | 状态 |
|---|---|
| 工作线程在 `SleepEx` 轮询的具体函数（MegaDot 还是 spine-godot） | 【未知】无引擎符号 |
| `res://` 与 `res:///` 双路径是否导致同一贴图并发重复加载 | 【未知】 |
| 运行中冻结（选角点机器人、打开怪物图鉴）在 P1/P2 后是否消失 | 未测；机制同源（主线程同步加载等被卡死线程持有的资源） |
| 骨妹偶发 `No loader found for resource: res://animations/characters/necrobinder/Necrobinder.atlas (expected type: SpineAtlasResource)`，导致 `necrobinder_skel_data.tres` / `necrobinder_merchant_skel_data.tres` 预加载失败 | 时有时无（日志 45 出现 2 次失败，46 为 0）；早在日志 06/08/10/12 就有，与 override 无关；玩家实测外观显示正常；**不是卡死原因**，疑似加载时机竞态，建议顺带排查 |
| 预加载与游戏 `MainMenuEssentials` 并发时后者变慢（日志 46：2,353 ms，平时 300～800 ms） | 资源竞争，非卡死；P1/P2 后应消失 |
| 启动失败率与排队数量的关系 | 趋势支持"越多越易卡"，但各档样本 1～4 次，阈值不可信 |

---

## 8. 附件索引（均在 `SkinChanger_report_20261114\`）

- `logs\21_…`～`logs\33_…`：未加 override 的对照日志（文件名即结论）
- `logs\34_…`～`logs\46_…`：加 override 的实验日志（文件名注明冷/热缓存与配置）
- `diagnostics\25_/27_/28_/29_/30_/32_/33_*`：卡死现场原生线程栈、栈内存 `res://` 路径扫描原始输出与解码结果
- `diagnostics\31_managed_stacks_31976.txt`：选角冻结时主线程托管栈
- `AGENT_HANDOFF_启动黑屏死锁排查交接.md`：完整排查过程与配置切换记录
- `SkinChanger_启动卡死问题反馈.md`：早期反馈（日志 01～22）

## 9. 引用的源码位置

| 位置 | 内容 |
|---|---|
| SC `Core/SkinPreloader.cs:22-24, :187` | 队列上限 48、0.15 s/2 个、`useSubThreads: true` |
| SC `Core/SkinService.cs:6334, :6677` | `MountOverlayCore` 末尾调用 `SkinPreloader.Schedule(…, groups)` |
| SC `Ui/ContextualSkinControls.cs:1963` | `RunRefresh` 内置 `Paused`（仅选角重建） |
| SC `Core/QuitCacheCleanup.cs:27-54` | 12 秒强杀、`CancelAll` |
| 游戏 `AssetLoadingSession`（`sts2.dll` / `sts2.xml`） | `useSubThreads: false`；VFX 逐个加载的官方注释 |
| Godot 4.5.1 `worker_thread_pool.cpp:249, :329, :498, :830` | 低优先级排队、提升、协作等待、名额公式 |
| Godot 4.5.1 `resource_loader.cpp:655, :1385-1404` | 后台加载以低优先级投递；关停时轮询等待在途加载 |
| Godot 4.5.1 `main.cpp:1955-1957, :2145`；`project_settings.cpp:329, :700` | 线程池初始化时机、文件日志挂载时机、默认比例 0.3、读取 `override.cfg` |
