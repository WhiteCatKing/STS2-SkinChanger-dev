# 卡图主体迁移 · GPT · 2026-11-14

基于 1.0.31.106 工作区完成，交 Claude 复核。

- 新增 `CardFeature` / `CardFeaturePolicy`，Id 为 `card`。只读合并 `Catalog.CardGroups[*].Options` 与 `PckCardOptions`，覆盖配置卡组、模型分类后的单卡与卡图包、角色包附带卡图。有效非 base 选项含普通图、先古图、资源、呈现、表面设置或带状态卡图任一内容即启用，目录未就绪返回 null。纯策略六项测试已接入 LogicTests。
- `IsInUse` 在有活 NRun、卡牌图鉴或检视界面可见时返回 true，异常返回 true。界面用 Attach 时保存的弱引用、有效性和可见状态查询。两个生命周期回调留空，现有完整刷新会重建卡牌，遵循任务书由原选择刷新路径处理。
- **标记 11 类**。`CardLibraryInitialPortraitPreloadPatch`、`CardLibraryRowPortraitPreloadPatch`、`CardRewardPortraitPreloadPatch`、`CardPortraitResultPatch`、`CardBuiltInOverlayPatch`、`LocalCardPlayQueuePortraitPatch`、`CardPlayQueueRebindPortraitPatch`、`TinyCardSelectedSurfacePatch`，以及 `CardTransformPreload.cs` 的三个变形预加载补丁。
- **保留 14 类在主干**。选择入口及筛选联动六类为 `CardLibrarySkinReadyPatch`、`CardLibrarySkinOpenedPatch`、`CardLibraryPoolSkinPatch`、`CardLibraryAvailabilityFilterPatch`、`CardInspectSkinReadyPatch`、`CardInspectSkinDisplayPatch`。共享生命周期七类为 `CardLayoutResetPatch`、`CardLayoutBaselineCapturePatch`、`CardLayoutFinalPatch`、`CardPreviewModeCapturePatch`、`CardLayoutModelRebindPatch`、`CardLayoutPoolReleasePatch`、`CardTreeReentrySkinPatch`，它们配合带状态卡图、外部编辑器及租约清理，Reload 三类保持整体在位。`TinyCardAnonymousSurfacePatch` 继续清除无卡牌身份节点的旧绑定。
- 三处真实延迟执行补代次，为预设 `QueuePresetChange`、优先级 `QueuePriorityChange`、检视单卡 `ApplySelection` 的 CallDeferred。排队时记录 Generation，回调先检查 IsCurrent。同步枚举沿用原逻辑。
- 沿用现有 Safe 包裹。三个变形预加载补丁已有业务 try/catch，带 ref 的肖像返回值和筛选、重置与回池等清理入口沿用原处理。
- `CardApi` 收拢清单全部 15 处游戏反射，合并为 12 个成员，含 Reload、UpdateVisuals、两种回池、UpdateFilter、FilterCards 精确重载、Columns、CalculateRowsNeeded、奖励 RefreshOptions、内置遮罩字段、高亮材质字段、TinyCard banner 字段。原调用与目标方法顺序沿用。

Claude 需在 `FeatureBootstrap.CreateFeatures()` 添加注册行，再安排正常场景实测。

```csharp
yield return new Card.CardFeature();
```

验证通过。Release 构建 0 错误、9 个已有警告，完整 LogicTests 含 stateful-card 与新增 card 用例全绿，末行为 `Skin Changer logic policy tests passed.`。差异空白检查通过，共享测试文件中的既有模块行已保留。

输出及日志在 `C:\Users\yuinn\AppData\Local\Temp\sts2-card-migration-342fd57dd9984a22b214040cdcda7c7e` 的 `mod/`、`logic/`、`mod-build.log`、`logic-results.log`。

## Claude 复核（1.0.31.107）
去掉了 `QueuePresetChange`、`QueuePriorityChange`、检视单卡 `ApplySelection` 三处的代次检查。这三个延迟回调执行的是玩家自己的选择（写配置），不是模块往节点上挂的东西；代次一变就丢掉，会悄悄吞掉玩家的选择。代次检查只用于模块自己改节点的回调。其余照收。
