# 战损热切换修复与骨手诊断

GPT，2026-11-15，基于 `bdb7ca9 / 1.0.31.110`，交 Claude 审核、递增版本和实测。

- `Core/ManagedSkinModLoader.cs` 在提供者激活完成、NRun 活着且 IsNodeReady 时补初始化。只接受 NRun._Ready 的 Postfix，回调完整类型为 `BattleWear.BattleWearTimerPatch`、方法为 `Postfix`、静态 void、唯一参数为 NRun。判据放在新建的 `Core/BattleWearTimerPolicy.cs`，依据本轮反编译确认的视觉轮询 Timer 合约。
- 复用 `ReplaySelectedNodeReadyPostfixes` 的参数构造、回调调用与节点增量追踪，增加可选回调过滤，原调用者使用默认值。新建 Timer 纳入对应 NRun 的 NodeReadyMutation，出现 `[战损] 已为 <providerId> 补建对局计时器`。
- 提供者停用和 NRun.TreeExiting 配对清理，解绑退出回调，恢复该次节点增量。恢复时 Timer 先 Stop，再移出父节点并 QueueFree，再启用沿用原包的同名节点幂等检查。新补初始化和诊断使用常驻 Core 键 `patch:core`。
- `Ui/CharacterAppearanceRuntime.cs` 加临时只读诊断。在热切换排布前后记录，新增 OstyLayoutDiagnosticPatch 在 ScaleAndPositionOsty 调用前记录，并监听游戏 `_scaleTween.Finished` 读取完成后状态。日志以 `[骨手诊断]` 开头，每个战斗房实例最多 30 条，包含双方 Position/GlobalPosition/Bounds、主人 Hitbox、MaxHp、双方选项、wrapper Position/Scale、Visuals.Scale/DefaultScale 和发起位置。代码标明“临时诊断，骨手修复后删除”。
- 测试新增 `tests/STS2SkinChanger.LogicTests/BattleWearTimerPolicyTests.cs`，Program 与 csproj 各追加一行。九条断言覆盖已验证回调和其他类型、目标、方法、返回/静态约束及参数签名的排除。Release 构建 0 错误、9 个既有警告，LogicTests 全绿。

构建目录为 `C:\Users\yuinn\AppData\Local\Temp\sts2-gpt-battlewear-osty-9e1e4044f7f94f11a492b43780a2543e`，其中 `Gurio.SkinChanger.dll` 同时包含两项，构建显式传入本机 GameAssemblyDir。本次验证为编译与离线逻辑测试，画面与 Timer 的游戏内行为待实测。

## 给云的战损实测步骤

1. 用其他骨妹皮肤进入战斗，局内切到四档战损包，日志应出现 `[战损]` 补建提示和原包 `wear timer attached.`。
2. 受伤跨过设置中的一条阈值，等待受伤动画结束，检查换装。
3. 切走、切回并保存读档，确认换装继续生效，同一提供者的 Timer 始终只有一个。

## 给云的骨手采样步骤

1. 同一局中固定主人皮肤，原版骨手与 Moe 骨手互切，保留 `[骨手诊断]` 的热切换前后记录。
2. 结束回合，等新回合的骨手召唤 Tween 完成，比较对应的调用前/完成后记录和画面。
3. 保存读档，再换另一种主人皮肤重做一个回合，保留双方 Hitbox/Bounds 与 wrapper 的差异。

## 可选、不阻塞

- 将来支持其他战损 Timer 合约时，先核对其回调职责与签名，再扩展判据。
