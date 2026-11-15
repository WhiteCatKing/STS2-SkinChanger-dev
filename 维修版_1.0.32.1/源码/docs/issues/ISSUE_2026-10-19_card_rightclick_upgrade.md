# 无法右键卡牌查看升级

- 状态：**未解决**
- 来源：Steam 工坊讨论区，2026-10-19，裤裆藏龙「任务达人」
- 链接：https://steamcommunity.com/workshop/filedetails/discussion/3787302680/569300630414379140/
- 收录：2026-11-03

## 现象

不管是游戏内还是百科（卡牌库）里，都不能右键点卡牌查看升级预览。发帖时 0 回复。

## 影响

原版右键查看升级是常用操作；被吞掉后玩家无法确认升级后的数值/卡面。

## 初步判断

最可能是 SkinChanger 给卡牌节点加的覆盖控件/来源指示条拦截了右键：

- `STS2SkinChanger/Ui/CardSkinControls.cs` 会在卡牌右侧加来源指示 `VBoxContainer`/`Panel`，`MouseFilter = Stop`；
- 卡牌本体上还可能有透明覆盖层（`MouseFilter = Stop`）用于拖动/选择；
- 右键在别处被用于“随机排除”“拖动重置”等，需确认是否有全局右键处理。

## 下一步

1. 用干净存档 + 只开 SkinChanger 复现，确认是否与本 Mod 相关。
2. 检查挂到 `NCard`/卡牌库条目上的所有 `Control`，把纯装饰层改为 `MouseFilter = Pass/Ignore`，或在 `_GuiInput` 里把右键原样放行。
3. 回归：游戏内手牌、抽牌堆/弃牌堆、卡牌库、升级预览都要能右键。

## 关联

- 未见于 `WORKSHOP_FEEDBACK_2026-08-30.md` / `COMPLETED_FIXED.md`。
