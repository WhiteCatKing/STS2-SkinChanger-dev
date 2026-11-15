# 联机：选人界面的显示方式与局内解耦（队友皮肤互相独立）

- 状态：**未解决**（1.0.26.67 已实现，待实机验证）
- 记录日期：2026-11-07
- 需求来源：作者本人（用 `联机_一键双开.bat` 双开实测）

## 作者要求

1. 选人界面那个「队友显示…」设置只保证**联机选人界面**的效果，不要一直影响整局。
2. 进局后队友的皮肤**按选人界面的设置**决定，但只是开局那一刻的结果，不是持续绑定。
3. 队友局内换皮肤，不能影响选了「2. 队友显示我和队友都有的皮肤」的其他人。
4. 队友皮肤与自己完全独立：局内可以随意把某个朋友改成别的皮肤，自己不受影响。
5. 手动给自己或给别人换过皮肤后，除非再次手动换他的，否则**任何情况（含存档/读档 SL）都不改变显示**。
6. 除联机选人界面外，队友怎么换皮肤都不影响自己这里的显示。

## 原实现为什么会串

- `ResolveRemoteVisualSelection` / `TryGetPlayerSelectionMap` 每次都**实时读当前显示方式**：
  - `Mine` → 返回 `SkinService.Config.GetSelection(groupId)`（本机当前选择），所以本机局内换皮肤会把队友一起换掉；
  - `Shared` → 每次队友发通告就 `TryMakeSelectionAvailable` 重建队友外观，所以队友换皮肤自己这边立刻跟着变。
- 局内「外观」界面对**别的玩家**的改动被 `CanEditSkinForCreature` 拦住（对方皮肤可用时返回 false），
  于是走回**持久化选择**路径 `SkinService.ApplySelection` → 改的其实是自己，这正是「给队友换皮肤自己变了」。
- 局内的显示方式开关虽然只在选人界面出现，但值每次都被重新读取，等于一直绑定。

## 1.0.26.67 的做法

`Core/MultiplayerSkinSync.cs`：

- **开局快照**：`AttachToRun` 进入新对局时 `CaptureRunSnapshot()` 记录当时的显示方式
  `_runModeSnapshot` 与本机选择表 `_runSelectionSnapshot`；之后整局都用
  `EffectiveRemoteMode()` / `RemoteFallbackSelections()`，不再读实时设置。
- **解析不再实时绑定**：`ResolveRemoteVisualSelection` 与 `TryGetPlayerSelectionMap` 统一为
  「手动指定 → 快照映射 → 模式回退（原皮 / 我开局时的皮肤）」，三种模式只是回退规则不同。
- **局内冻结队友皮肤**：`FreezeInRunRemoteSkin`——对局里已经认识到的玩家，其通告只更新
  模型参数（缩放/偏移），**不更新皮肤**；只有本局还没见过的玩家（中途加入）才整份采用作为初始快照。
- **队友皮肤本地化**：`CanEditSkinForCreature` 对远端玩家恒为 true；`UsesLocalFallbackControls`
  只要「联机 + 远端玩家」就允许用本地选择器；手动改动走 `TrySetLocalFallbackSkin`
  （只写 `ManualSelections` / `AvailableSelections`，不碰自己的持久化选择）。
- **模型参数仍按老规则**：`CanEditTransformForCreature` / `TryGetSyncedTransform` 只在
  「共享模式」或「本机手动给该玩家指定过皮肤」时应用对方的参数，避免原皮/我的皮肤被对方参数摆错位置。
- **本机换皮肤不再重建队友**：`OnLocalCharacterSelectionChanged` 只在**选人界面**（`!_inRun`）
  且模式为 `Mine` 时重算远端外观。
- **模式切换清理只作用于选人界面**：`RequestFullRemoteRefresh` 里丢弃远端映射的条件加了 `!_inRun`。

## 保留没动的

- 通告（发送自己的皮肤）仍然一直发送，只是对面在局内不再应用。
- 选人界面里三种模式的实时效果不变（共享模式仍会跟着队友换）。
- SL：`ManualSelections` 不随 `DetachFromRun` 清除，只有开始新的联机选人回合
  （`ResetRoundStateForLobby`）才清，所以手动指定能活过存档/读档。

## 测试

- `tests/STS2SkinChanger.RuntimeTests/MultiplayerAppearanceIdentityTests.cs`：
  - 断言改为「远端玩家的皮肤本机可以改」（原断言要求锁定）；
  - 新增：把显示方式快照到 `Shared`、开局后收到队友「原皮」通告，本机仍显示对方原皮肤 `pea`；
    随后 `TrySetLocalFallbackSkin` 手动改成 `__base__` 立即生效并覆盖对方通告。
- LogicTests 全通过；RuntimeTests 只剩改动前就存在的 `OtherCompendiumUiTests` 失败（与本改动无关）。

## 实机验证步骤

1. 双开，选「2. 队友显示双方都有的皮肤」，进局。
2. 队友局内换皮肤：自己这边**不应**变化。
3. 自己在局内给队友换一个皮肤：自己**不变**；队友那边（他选 2 的话）也不受我这边影响。
4. 存档→读档：刚才手动指定的队友皮肤保持。
5. 选「3. 队友显示我用的皮肤」再开一局：队友显示**开局时**我的皮肤；局内我换皮肤，队友不变。
6. 在选人界面切换这三种模式：选人界面本身仍立即生效。
