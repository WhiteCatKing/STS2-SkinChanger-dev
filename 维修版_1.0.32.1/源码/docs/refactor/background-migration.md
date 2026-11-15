# 战斗背景模块迁移 · GPT · 2026-11-14

基于 1.0.31.105 工作区完成勘察与迁移，交 Claude 复核。

- 新增 `BackgroundFeature` / `BackgroundFeaturePolicy`，Id 为 `background`。`ShouldBeActive` 只读 Catalog 就绪状态和 `_backgrounds` 的 ID 快照，未就绪返回 null，存在有效背景返回 true，过滤 base、跟随分类、模组默认及空项。跨地区的游戏战场也算可用选项，判定沿用“有选项即启用”。六项纯策略用例已加入 LogicTests。
- `IsInUse` 使用 `NCombatRoom.Instance`，检查有效背景的 `sc_background_applied` / `sc_encounter_backdrop_option` 标记。背景界面通过开启、关闭时维护的弱引用与可见状态判断。活房间背景暂不可读或查询异常时返回 true。查询只访问单例、标记及界面跟踪状态。
- `OnActivated` 调用现有 `RefreshCurrentActIfNeeded` 的受控入口，沿用当前战斗刷新规则。`OnDeactivating` 留空，模块等待活背景和界面结束后才卸补丁，当前选择切换的还原仍由既有路径处理。
- 标记 `BackgroundSkinRoomPatch`、`BackgroundSkinCreatePatch`。保留 `FakeMerchantCombatArtPatch` 在主干，它在假商人遭遇创建前挂载事件帐篷美术，默认背景选择也依赖它。`BackgroundAppearancePauseMenuPatch` 保留为常驻选择入口。`SkinPopupBackgroundMutePatch` 归属独立 UI 功能。
- 沿用已有 `Safe.Run("patch:background", …)`，Create Postfix 带 `ref __result`，保持原业务 try/catch。~~三个同步目录枚举循环 `ListLayersInDirectory`、`ListLayerPaths`、`ListGameBackgroundTitles` 在进入时记录 Generation，每轮核对 IsCurrent，过期则结束目录枚举并返回空结果。~~（Claude 复核时已去掉，见房间 #50：这三个是同步的有限循环，不属于延迟回调；背景选项列表就是从它们建出来的，中途返回空表会把列表弄丢。）
- `CombatBackgroundApi` 纯搬迁四项游戏反射缓存，覆盖 NRun 状态字段及旧名回退、NCombatRoom.Background、EncounterModel.HasCustomBackground、NCombatRoom._visuals。同时搬迁预览 RNG 构造与 Chaotic 回退、暂停菜单两个补丁目标查找。`ReadCustomBackgroundPath`、`PrepCustomBackgroundAssets` 的动态遭遇预览探测保留原处，提供者 Profiles / Battlefield 等第三方探测也保留。

Claude 需在 `FeatureBootstrap.CreateFeatures()` 添加以下注册行，再做模块接入实测。

```csharp
yield return new Background.BackgroundFeature();
```

验证通过。主项目 Release 构建 0 错误、9 个警告，警告均在本次修改文件之外。完整 LogicTests 含事件和新增背景用例通过，末行为 `Skin Changer logic policy tests passed.`。共享测试 Program.cs / csproj 中的事件追加已保留，差异空白检查通过。

构建和测试输出在 `C:\Users\yuinn\AppData\Local\Temp\sts2-background-migration-cf43acc88e4947d89d5ea6eee3c21ee3`，日志为 `mod-build.log`、`logic-results.log`。实机验证由 Claude 按任务书安排。
