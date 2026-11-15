# 阶段 A 第 1 批：安全件骨架 + 写盘熔断接入（Claude，2026-11-14，基于 1.0.31.95）

状态：已在 1.0.31.95（作者补交的 .94/.95 已用 git am 合入，提交 64bcb72 / 01eca75）上重新构建（0 错误）、LogicTests 全绿（新增 `SafetyTests`）。**未实机测试、未部署、未改版本号、未 commit。**

## 新增 `STS2SkinChanger/Core/Safety/`（命名空间 `STS2SkinChanger.Core.Safety`）
| 文件 | 作用 | 依赖 Godot |
| --- | --- | --- |
| `SafetyLog.cs` | 安全件的日志出口（Entry 接到 `ModLog.Warn`），按 key 节流 30 s，补报省略次数 | 否 |
| `CircuitBreaker.cs` | 按功能 key 熔断：窗口内失败 ≥ N 次，或进入次数 > MaxCalls（循环触发）即跳闸；本会话停用，打一行 `[熔断]`，触发 `Tripped` 事件（留给阶段 C 的模块 Deactivate） | 否 |
| `Safe.cs` | `Safe.Run(feature, action)` / `Safe.Get(feature, func, fallback)`：异常止步，计入熔断，节流打 `[防崩溃]` | 否 |
| `LoopGuard.cs` | 同线程重入深度守卫（默认 3 层） | 否 |
| `DiskGuard.cs` | 写盘熔断：会话写入总量上限 6 GB（超了跳闸），剩余空间保底 2 GB（只拒绝这一次，不跳闸）；拒绝时抛 `DiskGuardException : IOException` | 否 |
| `NodeGuard.cs` | `IsAlive`（null / 已释放 / 释放队列中）、`IsAliveInTree`、`HasPath`、`LoadIfExists<T>` | 是 |

## 接入点（只改了这 4 处）
- `Pck/PckArchive.cs` `WriteEntries`：总大小算出来后先过 `DiskGuard.EnsureCanWrite`；写到一半失败就删掉残包再抛；成功后 `Account`。7 个 PCK 写入调用方（SkinService 6、OnlineSkinCache 1）都经过这里。
- `Core/SkinChangerStorage.cs` `WriteCache`：先用 `DiskGuard.CanWrite` 判断，不通过返回 false；写完按文件增长量记账，内层 PckArchive 已经记过的部分扣掉（线程内计数），不重复记。
- `Core/SkinChangerPaths.cs` `WriteCache`：被熔断拒绝时抛 `DiskGuardException`（IOException），原来的"正在退出"情况仍抛 `OperationCanceledException`。
- `Entry.cs`：`SafetyLog.Sink = ModLog.Warn`。

失败时的行为和"磁盘真写满"一样，走现有的 IOException 失败路径：这次换肤失败，外观保持不变。没有新增退路分支。

## 有意留到下一批的
- `Safe.Run` / `CircuitBreaker` / `LoopGuard` 还没接到任何业务代码上。要等肥鱼的 `risk-sites.md`（7 处 `while(true)`、92 处 retry），按清单逐个接。补丁体的批量包裹是肥鱼的任务 ④。
- 103 处不经过咽喉点的直接写盘（多数是配置和小文件）还没接 `DiskGuard`。
- 阈值（6 GB / 2 GB / 60 s 内 5 次）是保守猜测，实机跑一轮后再调。

## 请 GPT 复核的重点
1. `DiskGuard` 的线程内记账（`[ThreadStatic] _accountedOnThread`）放在 `WriteCache` 外层时，会不会少记或重复记？`SkinWorkshopService` 的 `Task.Run` 路径要特别看。
2. `PckArchive.WriteEntries` 删残包：调用方有没有在写失败后还依赖这个文件存在的？（`OverlayCacheStore.ReservePath` / `AfterWrite` 那条路径要看）
3. `CircuitBreaker` 跳闸不会自动恢复。对外观功能来说这个取舍是否合理？有没有哪个 key 应该允许恢复？
4. 6 GB 会话上限：结合 GENERAL.md 和报告里提到的写盘量（首装 630–830 MB、旧版每次切换 588 MB），正常使用会不会误触发？
5. `SafetyTests` 的覆盖有没有漏掉的情况。

结论写到 `docs/refactor/review-stageA-gpt.md`。

**范围限制（云 11-14 定）**：只看上面 5 条；只按正常使用场景判断（启动、选人、局内换肤、读档、联机）；不为正常使用之外的极端情况补测试或提改动，确实想到的写在文末「可选、不阻塞」一节，每条一行；一遍过，不要反复复核；每条结论给出一句话判断，再附上必要的行号。
