# 事件模块迁移（Claude，阶段 C 第 2 批，基线 1.0.31.105）

## 进模块（`[FeaturePatch("event")]`，没装事件皮肤时整块不装）
`Core/EventSkinRuntime.cs` 里的四个类：
- `EventSkinResourceAvailabilityPatch`：`EventModel.HasVfx` / `HasPhobiaModePortrait` 的 getter。
- `EventSkinTextPatch`：`LocTable.GetRawText`。游戏每次取文字都会走这个前缀，是这次按需激活收益最大的一处。
- `EventSkinVfxPatch`：`EventModel.CreateVfx`。
- `EventSkinPortraitTrackingPatch`：`NEventLayout.SetPortrait`。

## 留在主干
`Ui/EventCompendiumPreview.cs` 的 `EventPreviewLifecyclePatch` 不进模块。它是图鉴预览的生命周期保护，所有事件都能预览，没装事件皮肤也要在。

## 判定
- `ShouldBeActive`（`EventFeaturePolicy`，纯函数，LogicTests 7 条断言）：任何 `event:` 分组或 `fake_merchant_monster` 分组里有非默认选项就激活；目录没就绪时返回 null。假商人事件的皮肤挂在 `fake_merchant_monster` 上，见 `EventSkinPolicy.GroupId`。
- `IsInUse`：`NEventRoom.Instance` 在场景树里，或图鉴里最近一次打开的事件预览还在（`EventCompendiumPreview.HasLivePreview`，用弱引用记录，不遍历整棵树）。
- `OnActivated` / `OnDeactivating`：都留空。模块只在目录就绪、事件包增减时才会变化，在用时注册表不会卸；已经打开的事件也不重建，因为重新 `SetEvent` 会重置选项和奖励。

## 其他
- 没有延迟回调需要加代次：事件路径里没有 `CallDeferred` 或计时器。
- `Safe.Run` 沿用第 1 批的结果：`PortraitTracking` 已经包过；另外三个是带 `ref __result` 的前缀，按规则不包。
- 没有搬 GameApi：只有 `NEventLayout._event` 一个反射字段，留到阶段 B 统一处理。

## 待实机
- 装了事件皮肤时：进事件，立绘、特效和文字都要是所选皮肤；图鉴预览正常。
- 日志里要出现 `[模块] 已注册「event」：4 个补丁类`。
