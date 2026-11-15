# 骨手位置修复与缩小效果排查

GPT，2026-11-15。基于 `bdb7ca9` 加 `.111` 已实测的工作区改动，交 Claude 审核和实测。

- 本次代码增量仅在 `Ui/CharacterAppearanceRuntime.cs` 的 RefreshPlayerAndPetLayout。通用排布、锚点刷新后，对本地玩家的 Osty 同步写入 `owner.Position + NCreature.GetOstyOffsetFromPlayer(Entity)`，复用游戏现有的主人 Hitbox/骨手 MaxHp 公式（当前第 1594–1600 行）。同步写入让“排布后”日志立即采到最终位置。用户保存的 wrapper 偏移继续正常应用，`[骨手诊断]` 保留。
- 日志依据为 `logs_111/godot2026-11-15T20.38.07.log:1156–1162,1189–1190`，游戏公式给出 `(-374.3333,125)`，旧通用排布给出 `(-571,210)`。骨手位置现在统一采用召唤时的游戏公式，MaxHp 或主人 Hitbox 变化时自然随公式变化。
- Release 构建 0 错误、9 个既有警告，LogicTests 全绿。构建目录为 `C:\Users\yuinn\AppData\Local\Temp\sts2-gpt-osty-layout-5a0dbb66636846abad59539187735f77`，显式使用本机 GameAssemblyDir。当前画面等待云实测。

## 给云的骨手实测步骤

1. 在外观面板将骨手 OffsetX/OffsetY 重置为 0，固定同一主人皮肤，原版与 Moe 骨手互切，比较排布后坐标和画面。
2. 过一回合，比较召唤 Tween 完成后的坐标，确认位置遵循同一公式。
3. 保存读档，确认位置一致，再换主人皮肤检查其 Hitbox 变化后的骨手位置。

## 缩小效果只读结论

**已确认切换器热重建会丢失游戏缩小倍率。** 游戏 ShrinkPower.AfterApplied 调用 `NCreature.ScaleTo(0.5,0.75)`，把 `_tempScale` 设为 0.5，Tween 将 `NCreature.Visuals.Scale` 写成 `_tempScale × Visuals.DefaultScale`。当前 TryRebuildCreatureVisuals 将 `runtimeScale` 固定为 One，再将新 visuals.Scale 写成 `newBaseScale × runtimeScale`（第 1795、1847 行），已有 `_tempScale` 没有乘回去。日志第 1269–1270 行主人 Scale 为 0.5，第 1271–1301 行局内切到四档包并重建后已为 1，首个自动 `wear stage 1->2` 在第 1331 行。

战损包 `BattleWear.ApplyCombatStage` 直接替换内部 SpineSprite 的 `skeleton_data_res`，也可借同层 CanvasGroup/ghost 做过渡，保留同一个 NCreatureVisuals。该阶段方法没有直接重建 visuals 根节点。包首次 TickCombat 另会调用 `NCreature.SetScaleAndHue(VisualScale,0)`，游戏此方法也直接重写根 Scale/DefaultScale，构成包自身的初始化覆盖风险。纯自动档位切换单独引起的丢失仍需一次“缩小后只受伤、全程不手动换皮肤”的对照。包自身这部分按任务约定不修。

MonsterVisualScalePatch 通过 MonsterScaleGroupMeta 门控，标记由 Monster 路径设置，玩家通常直接返回。玩家外观编辑写 wrapper.Scale，当前日志 wrapper 为 1。现有证据将切换器缺口定位到热重建的 runtimeScale 恢复。

后续修法建议由 Claude/GPT 在 TryRebuildCreatureVisuals 保留游戏 `_tempScale`，在新皮肤基准与提供者初始化完成后合成游戏临时倍率，继续使用 wrapper 承载用户外观缩放。验证需同时覆盖缩小应用、手动热换肤和缩小移除。自动战损的独立对照若也重置根 Scale，再交包作者处理其初始化/重置缩放行为。

反编译材料位于 `C:\Users\yuinn\AppData\Local\Temp\sts2-gpt-three-issues-20261115`。定位为 `game-shrink/...ShrinkPower.decompiled.cs:49–77`、`game-creature/...NCreature.decompiled.cs:1186–1208`、`game-visuals/...NCreatureVisuals.decompiled.cs:333–336`、`necro/necrobinderHpWear.decompiled.cs:969–978,1115–1208,8132–8207,8465–8485`。切换器门控位于 `Ui/ContextualSkinControls.cs:3056–3074,3173–3182`，wrapper 设置位于 `Ui/CharacterAppearanceRuntime.cs:768–777`。

## 可选、不阻塞

- 骨手实测通过后按后续任务删除临时坐标诊断。
