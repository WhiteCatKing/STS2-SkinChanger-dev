# 重构清点（DSH / 肥鱼，2026-11-14）

对应任务书 `task-dsh-inventory-20261114.md` 的 ①②③。基线 **`092ec98`（1.0.31.93）**，只读源码、不构建、不 commit。
扫描范围：`STS2SkinChanger/` + `compat/` 共 **337** 个 `.cs`（不含 `tests/`、`tools/`、`bin/`、`obj/`）。

> 扫的过程中 Claude 已经开始阶段 A。脚本对**被改过/新增的文件一律读 `092ec98` 的版本**（`git show`），
> 所以这份清单是「开工前状态」。每次运行会打印它读了哪几个文件的 HEAD 版本。

## 复跑

```powershell
cd 'D:\mod\工作2\STS2-SkinChanger_修改件\docs\refactor'
python tools\inventory_scan.py
```

末行打印统计并做**回读校验**（每条记录的 `文件:行号` 必须真的含有对应语法），当前 200+44+80+450 条全部通过。
类型表（判定目标归属）是离线文件，重新生成：

```powershell
& dotnet D:\mod\工作2\STS2_Work\dump\bin\Release\net9.0\dump.dll `
    'C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll' $env:TEMP\sts2_api.txt
# 取 TYPE 行 → tools\game-types-sts2-v0.113.0.txt（6378 个类型定义）
# GodotSharp.dll 同理 → tools\godot-types-v4.5.1.txt（4808 个）
```

## 产物

| 文件 | 内容 |
| --- | --- |
| `patches.csv` | ① 244 行：特性 200 行（来自 181 个 `[HarmonyPatch]` 类，多目标拆行）+ 手动安装 44 行 |
| `patches-summary.md` | ① 按模块/归属/目标类型统计 + 同一方法 ≥3 个补丁的高危点 |
| `risk-sites.md` | ② 三张表：写盘 80、无限循环 7 + 每帧钩子 9、重试 34 块 |
| `disk_writes.csv` / `loops.csv` / `frame_hooks.csv` / `retry.csv` | ② 的机读版 |
| `reflection.csv` | ③ 450 行，含查找方式、目标类型/成员、归属、失败时行为 |
| `reflection-summary.md` | ③ 归属/失败行为统计 + 游戏目标且无兜底的行 + 重复查找合并候选 |

CSV 均为 UTF-8 **带 BOM**，**表头用任务书里的中文列名**（Excel 直接打开不乱码）。
早前那版脚本产出的 `patches_manual.csv`、`reflection_targets.csv`、`reflection_unresolved.csv` 已删除，内容并入上面几张表。

## 口径（复跑/复核时按这个对）

**① 补丁**

- 特性补丁：`[HarmonyPatch]` 特性 + 同文件里的补丁类。类里多目标（`TargetMethod(s)`）会拆成多行；`TargetMethods()` 全类型扫描的记 `(动态)`。
- 手动补丁：非注释行上的 `.Patch(` / `PatchAll(` / `CreateClassProcessor(...).Patch()`；`Unpatch*` 不算补丁安装点。
- `mode`：`attribute` / `manual` / `patchall`（本工程当前没有 `PatchAll()`）。
- `owner`：目标类型在 `sts2.dll` 里 → `game`；在 GodotSharp 里 → `godot`；我们自己的类型 / BCL / 工具库 → `self`；provider 包 → `thirdparty`；`(动态)`、`(运行时解析)` → `unknown`。
- `module`：按 文件名 → 目标类型 → 目录 判定；拿不准才 `unknown`（本次 0 条）。

**② 风险点**

- 写盘只收任务书列的 API；`new FileStream(..., FileAccess.Read)` 是只读 IL/元数据扫描，**已剔除**（16 处），`资源/配置 .Save()` 不在这张表里（另有 48 处，文里注明）。
- `Directory.CreateDirectory` 收进来，但「写的是什么」标成「目录创建（不写数据）」，占 26 行；真正写数据的是 54 行。
- 「上限」优先取同语句附近 ±6 行里的 `Max*` / `Budget` / `GB/MB/KB`；没有就看文件里有没有；都没有标 ⚠。
- 无限循环的「退出条件」靠循环体里有没有 `break/return` 判断，只是提示。
- 每帧钩子 = `_Process/_PhysicsProcess/_Notification` + `tree.ProcessFrame += handler`，再看钩子体（或它调用的同级方法，最多两层）里有没有挂载/重建/写盘。

**③ 反射**

- 命中：`AccessTools.*`、`GetType("…")`、`GetMethod(` / `GetProperty(` / `GetField(`、`TypeByName(`。**不含**裸 `.Invoke()`（委托调用 180 处，噪声太大，不列为反射点）。
- 行内注释先剥掉；跨行调用向前取最多 8 行。
- 「失败时行为」是静态启发式：往后 7 行有 `throw` → 抛异常；有日志 → 日志；有 null 判断/回退 → 返回null静默；都没有 → 未处理。**未处理 ≠ 会崩**。
- 「目标类型/成员」取不到时记 `(变量/动态)` 或 `变量:xxx`（只知道接收者变量名）。

## 主要结论

**① 补丁**

- 181 个补丁类、244 行；按模块：`core` 57、`card` 52、`character` 41、`multiplayer` 32、`merchant` 17、`monster` 10、`event` 9、`ancient` 8、`compat:EmperorCrab` 7、`background` 6，其余 ≤1。
- 单文件最重：`ContextualSkinControls.cs` 23、`CardSkinControls.cs` 20、`FrameworkSkinRuntimePatches.cs` 17、`MultiplayerSkinSync.cs` 14、`MerchantRuntimeAppearance.cs` 13。
- 同一游戏方法被挂 **≥3** 个补丁的高危点：`NCreature._Ready`、`NPauseMenu._Ready`、`NPauseMenu.OnSubmenuOpened`、`NCard.Reload`（各 3）。
- 阶段 C 不能只搬特性类：`ManagedSkinModLoader` 会在运行时按 provider 声明动态 `Patch`（`patches.csv` 里记为手动）。

**② 风险**

- 写盘 80 处：目录创建 26、其他/未分类 15、缓存 12、日志 10、覆盖包 PCK 8、配置 7、下载 2。**咽喉点只有 11 处**（`SkinChangerPaths.WriteCache` 8、`PckArchive.Write*` 2、`Storage.WriteCache` 1），`DiskGuard` 先接这三处就覆盖绝大部分。
- 文件集中度：`SkinService.cs` 9、`SkinChangerStorage.cs` 7、`DebugSnapshot.cs` 6。
- 无限循环 7 处，全部有 `break/return`（背景列表 3、`SkinService` 2、事件优先级 1、`StartupWatchdog` 1）。
- 每帧钩子 9 处，确认会触发重建/挂载的 2 处：`MultiplayerSkinSync._Process`（→ Tick → Refresh*）、`CharacterAppearanceRuntime._Process`（→ ApplySelectionNow）；其余 7 处列出直接调用供人工判断。
- 重试 34 块，**28 块没有可识别的数值上限**；只有 `HalfHpCrabSkinPatch.StartInstallRetry`(18000 帧)、`OstyModelPreviewControls.ScheduleFit`(30)、`BestiaryEntranceSwitcher.ScheduleScan`(16)、`AncientCompendium.ScheduleDeferredAttach`(12) 等少数带数字。

**③ 反射**

- 归属：`game` 173、`unknown` 220（其中完全动态 134、只知变量名 77）、`self` 54、`godot` 3。
- 目标是游戏本体、失败时未处理或抛异常：**135 行**（阶段 B 优先收拢，清单见 `reflection-summary.md`）。
- 用得最多的查找方式：`AccessTools.Method` 101、`AccessTools.Field` 91、`GetMethod` 89、`GetProperty` 71、`GetType("…")` 36。
- 重复查找（≥2 次，合并候选）在 `reflection-summary.md` 末尾。

## 与重构计划草案数字的差异

| 项 | 草案 | 本次 | 差异原因 |
| --- | --- | --- | --- |
| `[HarmonyPatch]` | 186 处 | 181 个类 | 186 是 grep 命中行数，5 行是注释/文档注释里的举例 |
| 手动 Patch | 约 65 处 | 44 行 | 65 复现不出来；主工程非注释 `.Patch(` 正好 44（含 2 处 `CreateClassProcessor`） |
| 直接写盘 | 约 44 处 | 80 处 | 草案只数 `File.Write*/Copy/Create`（本次 38 处）；本次把 `Directory.CreateDirectory`、`File.Delete/Move`、`PckArchive` 也算进表，并剔除了只读 FileStream |
| retry | 约 76 处 | 34 块 / 92 行提及 | 草案按「标识符出现次数」计，本次按「同一方法聚成一个逻辑块」计 |
| 反射 | 503 处 | 450 行 | 草案含 `Invoke`（180 处委托调用）；本次剔除并补上 `GetProperty/GetField` |

## 已知盲区

1. 动态目标：`TargetMethods()` 全类型扫描、`AccessTools.Method(变量, …)` 只能记到「有这么一个补丁」，记不到最终打到谁。
2. 每帧钩子两层解析会漏掉「三跳以上才重建」的链路；`ProcessFrame +=` 的 lambda 体只按方法名回溯。
3. 无限循环只看字面 `while (true)` / `for(;;)`；`while (条件)` 的无界重试归到第 3 张表。
4. 「上限」是附近数字，可能是无关常量，标了 ⚠ 的才是确定没有。
5. 反射「失败时行为」只看 7 行窗口，跨方法的兜底看不到。

## 阶段 C 模块进度

以 `Core/Features/` 下的源码为准（2026-11-14，基线 1.0.31.106）。补丁类 = 标了 `[FeaturePatch("<id>")]`、由注册表按需装卸的类。

| 模块 id | 补丁类（个数和类名） | 什么时候启用（ShouldBeActive） | 什么时候算在用（IsInUse） | 留在主干的相关补丁 | 交件文档 | 实测版本 |
| --- | --- | --- | --- | --- | --- | --- |
| `merchant` | 4 个：`MerchantRoomPrewarmPatch`、`MerchantButtonProviderReadyBridgePatch`、`MerchantInventoryProviderReadyBridgePatch`、`MerchantHandProviderReadyBridgePatch` | `merchant` 或 `fake_merchant_monster` 分组里有非默认选项（装了商人皮肤）就启用；目录没就绪时先不判断 | 有活的 `NMerchantRoom`，或场景树里找得到商人房间、假商人、商人预览根节点，就先不卸（会遍历一次场景树） | 预览玩法保护、房间 scope 与玩家外观混合的 9 个类，以及具体房间 `_Ready`（仍由 Entry 手动安装） | `merchant-migration-step1.md` | 1.0.31.97 起 |
| `event` | 4 个：`EventSkinResourceAvailabilityPatch`、`EventSkinTextPatch`、`EventSkinVfxPatch`、`EventSkinPortraitTrackingPatch` | 任何 `event:` 分组或 `fake_merchant_monster` 分组里有非默认选项才启用 | `NEventRoom` 在场景树里，或图鉴里上次打开的事件预览还开着（弱引用记录，不遍历树） | 图鉴预览生命周期 `EventPreviewLifecyclePatch`（所有事件都能预览，没装事件皮肤也要在） | `event-migration.md` | 1.0.31.106 |
| `background` | 2 个：`BackgroundSkinRoomPatch`、`BackgroundSkinCreatePatch` | 背景目录里存在非「游戏默认」「跟随分类」「模组默认」的有效选项才启用；目录没就绪时先不判断 | 背景界面开着，或当前战斗房间挂着有效背景标记；查询异常时按在用处理 | `FakeMerchantCombatArtPatch`（假商人遭遇的帐篷美术，默认背景也依赖它）、`BackgroundAppearancePauseMenuPatch`（常驻入口）、`SkinPopupBackgroundMutePatch`（独立 UI 功能） | `background-migration.md` | 1.0.31.106 |

**进行中**：`card`（GPT，`CardSkinControls` / `CardSurfaceView` / `CardTransformPreload`）迁移中；`stateful-card`（Claude，`StatefulCard*`）迁移中。
