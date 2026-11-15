# 阶段 A 第 2 批：把熔断接到失控风险点（Claude，2026-11-14，基于 1.0.31.95）

状态：构建 0 错误，LogicTests 全绿（补了阈值用例）。**未实机、未部署、未改版本号、未 commit。**

## 接了哪些（依据肥鱼的 `risk-sites.md`）
| 位置 | 风险 | 接法 |
| --- | --- | --- |
| `SkinService.MountOverlay`（外观覆盖重建总入口） | 每帧或每秒反复请求重建，导致反复写包、反复挂载 | 最外层进入时调用 `CircuitBreaker.TryEnter("mount-overlay")`：60 秒内超过 120 次就熔断，本会话外观停在当前状态。同线程重入照旧走原来的合并逻辑。 |
| `MultiplayerSkinSync.Tick`（单机也会每帧跑） | 6 个子任务串在一起：变体快捷键 ×2、商人读档补刷、外观排队、意图层级、联机同步。任何一个每帧抛异常，都会冒进 Godot `_Process` | 每个子任务各自包一层 `Safe.Run(tick:*)`。60 秒内失败满 20 次只停用这一个子任务，其余照常。联机部分抽成 `TickMultiplayer`，逻辑没改。 |
| `CharacterAppearanceRuntimeNode._Process` | 外观排队每帧驱动 | `Safe.Get(appearance-queue, …, false)`：出错时按"没有待处理"处理，并停掉这个节点的每帧处理。 |

阈值集中在 `Core/Safety/SafetyKeys.cs`，Entry 启动时调用 `ConfigureDefaults()`。

## 核对过、不需要改的
- 7 处 `while (true)`：3 处 `BackgroundSkinService` 是目录遍历，遍历完就 break；`StartupWatchdog` 是后台看门狗，启动完成后 return；`SkinService:9346` 和 `EventPriorities:154` 每轮都会往排除集合里至少加 1 个提供者，否则就 break，提供者数量有限，一定会结束。
- `risk-sites.md` 里标 ⚠ 的 28 处重试，大多是字段声明或者计数归零那一行（扫描时被当成"重试"记了进来），本身不是循环。真正会反复触发的是联机外观重试（`_appearanceRetryCooldown`）和商人读档补刷（已有 240 帧上限），这两处都走 `MultiplayerSkinSync.Tick`，这一批已经包上。

## 交给肥鱼的
④ 第 1 批：card / background / ancient / event / monster 这五个模块的补丁体包 `Safe.Run`，任务书 `task-dsh-saferun-batch1-20261114.md`。

## 复核
和肥鱼的 ④ 合在一起，交给 GPT 复核一次（省一轮）。本批只请她看两点：①`Tick` 拆分后，原有的提前 `return` 语义是否不变；②`mount-overlay` 熔断后直接 return，会不会让调用方卡在"排队中"的状态。
