# 局内换怪物皮肤「选了不变 + 选项变灰 + SL 不恢复」

- 状态：**未解决**（1.0.26.68 已改动主要成因，待实机复现验证）
- 记录日期：2026-11-07
- 需求来源：作者本人（联机/单机都出现过；当次日志未保存）

## 现象

1. 局内「外观」里给怪物选了新皮肤，怪物外观不变。
2. 该选项随后变灰、点不动。
3. 存档/读档也不恢复，重启游戏才恢复。

## 根因（代码定位）

**主因：局内切换被排进静态队列后可能永远排不出来。**

- `Ui/CharacterAppearanceRuntime.cs` `RequestSelection`：`CanApplySelectionNow()` 为 false 时
  只把请求塞进 `_pendingSelection` 并 `Wake()` 运行时节点，**不写配置、不挂载资源**。
- `CanApplySelectionNow()` 在整场战斗里可以恒为 false：动作执行器繁忙
  （`RunManager.Instance.ActionExecutor.IsRunning`），或任意一只怪
  `DeathAnimationTask is { IsCompleted:false }`（`:1074-1075` 的判断）。
- 排空完全依赖 `CharacterAppearanceRuntimeNode._Process`（`NRun._Ready` 时挂上）。
  节点没挂上 / `Wake()` 找不到节点 → 请求永久残留。
- 于是：配置没写 → 世界里的怪不变；但下拉显示用 `GetRequestedOption`
  （`CharacterAppearanceScreen.cs:746-748`）优先展示"待应用选项" → 看起来已经选中了。
- 变灰：`RefreshStatusForCurrentContext`（`CharacterAppearanceScreen.cs:1915-1930`）
  只要存在 pending 就 `SetTransformControlsEnabled(false)`，而它会
  `_skinDropdown.Disabled = true`（`:1958`）→ 整个下拉变灰、点不动。
- SL 无效：`_pendingSelection` 是进程内静态，配置从未写入；SL 只重建场景，
  不会补做那次挂载，所以读档后依然是原皮（重启进程才清掉）。

**次要成因（同类"静默失败"，本次只加了日志）：**

- `Core/SkinService.cs` `MountOverlay` 的"重入合并"在连续 8 轮后
  `leftover.Clear()` 静默丢弃挂载请求，而 `ApplySelection` 已返回 true：
  配置是新皮肤、Godot 的 `res://` 映射还是旧的，直到下次重建。
- `ApplyMonsterCategoryPriorityToSelections` 会把「跟随分类」分组的选择改写成
  优先级选项；分组未被登记为手动时，玩家手选可能被悄悄覆盖。
- `RuntimeMonsterVisualModeBridge.FailedBindings` 让某 provider 的视觉模式
  一次失败后本进程内不再重试。

## 1.0.26.68 的改动

1. `Ui/CharacterAppearanceRuntime.cs`
   - `PendingSelection` / `PendingMonsterRefresh` 记录 `StartedMsec`；
   - 新增 `PendingSelectionTimeoutMsec = 2500` 与 `PendingWaitedTooLong()`；
   - `ProcessPendingSelection`：排队超过 2.5 秒就**强制应用**并清空 pending，
     同时 `ModLog.Warn("外观选择排队过久，已强制应用：…")`。
     这样即使战斗一直"忙"或死亡动画任务残留，选择也一定会落地、下拉一定会解锁。
2. `Ui/CharacterAppearanceScreen.cs` `RefreshStatusForCurrentContext`
   - 排队时不再禁用控件，只显示"排队中"提示：即使排队卡住，玩家仍能改选其它皮肤自救。
3. `Core/SkinService.cs` `MountOverlay`
   - 8 轮后放弃余下请求时改为 `ModLog.Error`，并写明"这些分组要等下一次外观重建才生效"，
     以后再出现同类问题时日志里能直接看到。

## 未做（记录在案的后续可选项）

- 把"重入放弃"改成返回失败让 `ApplySelection` 回滚/重试；
- `ApplyMonsterCategoryPriorityToSelections` 保留仍有效的玩家手选；
- `RuntimeMonsterVisualModeBridge` 失败后允许重试。

## 实机验证

1. 局内进入战斗（让敌人行动/死亡动画期间）换怪物皮肤：
   - 正常应在 1 帧内生效；最坏 2.5 秒内强制生效，日志出现"排队过久，已强制应用"。
2. 连续快速切换多个怪物皮肤：下拉保持可点，不会变灰。
3. 换完立刻存档→读档：怪物仍是新皮肤。
4. 若仍复现：日志里搜 `外观选择排队过久`、`放弃余下请求`、`重入重建`。
