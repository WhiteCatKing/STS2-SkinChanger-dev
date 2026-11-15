# 1.0.27.82 重建记录（从「工作备份」恢复，回退到文件损毁前一步）

- 日期：2026-11-07
- 背景：编辑 `Ui/ContextualSkinControls.cs` 时写文件操作把换行合并导致源码损坏；
  为恢复，先用反编译产物顶替，但恢复出的代码与其后两处改动把选人界面的
  皮肤下拉与预览小人弄没了。现按作者要求，从 `C:\Users\Administrator\Desktop\工作备份`
  重新整理，做到"文件损毁的上一步"为止。

## 基线

`工作备份\STS2-SkinChanger_修改件`（上一次创意工坊更新时的整棵树，手写源码）。

## 重新应用的内容（按版本顺序）

| 版本 | 内容 | 涉及文件 |
| --- | --- | --- |
| 1.0.27.73 | 内容 Mod（如「晓夫九」）不再被当皮肤提供者隔离 | `Core/ProviderGameplayContentPolicy.cs`、`Catalog/SkinCatalog.cs` |
| 1.0.27.74 | Pengo 塔罗：CJK 标签改用语言字体 | `Core/PengoTarotFontCompat.cs`、`Entry.cs` |
| 1.0.27.75 | 变化特效前预加载卡图 | `Ui/CardTransformPreload.cs` |
| 1.0.27.76 | 卡面节点排到 `%CardVfxContainer` 之下 + 光效 ZIndex | `Ui/CardSkinControls.cs`、`Ui/CardTransformPreload.cs` |
| 1.0.27.77 | 框架皮肤注册补做 + 注册/契约丢弃诊断 | `Core/FrameworkCompatibilityLayer.cs`、`Catalog/SkinCatalog.cs` |
| 1.0.27.78 | 变体场景是否存在的校验（修火堆 `ironclad_3_rest_site` 崩溃） | `Ui/ContextualSkinControls.cs` |
| 1.0.27.79 | 联机远端火堆/商人场景取皮肤资源 | `Core/RemoteRoomSceneLease.cs`、`Core/MultiplayerRoomAppearancePatches.cs` |
| 1.0.27.80 | 联机远端充能球本体 | `Core/MultiplayerRoomAppearancePatches.cs`、`Core/SkinService.cs` |
| 1.0.27.81 | 充能球图标 + 提供者规范路径通用覆盖 | `Catalog/SkinCatalog.cs`、`Core/RemoteRoomSceneLease.cs` |
| 1.0.27.82 | CZN 系皮肤选人背景/大图重建后重新铺回 | `Ui/FrameworkSelectPresentationBridge.cs`、`Ui/ContextualSkinControls.cs` |

`Ui/ContextualSkinControls.cs` 用备份里的**手写版本**，只加了 1.0.27.78 的
`VariantSceneAvailable` 守卫和 1.0.27.82 的框架重新应用调用；不再包含任何反编译产物。
`Ui/MonsterSkinPriorityControls.cs` 与备份一致（未改）。

## 明确**未**包含（属于损坏发生时的下一步）

这两项是 1.0.27.83 的工作，按"只做到损毁前一步"的要求没有并入：

1. `ShouldDeferCharacterIconResourceToExternalRuntime` 对"非资源型运行时提供者"放宽
   —— 用于让 CZN 系皮肤的**选人栏头像 / 顶栏头像**生效；
2. `RemoveStaleProviderCharacterSelectControls` 对运行时提供者跳过清理
   —— 用于保留 CZN 皮肤的**背景切换工具条**（静态立绘/动态立绘/直播大厅）。

## 此后：在干净树上重做那两项（2026-11-07 深夜）

恢复完成后，在 1.0.27.82 的干净树上把上面两项**一项一次构建**重做：

| 版本 | 内容 | 涉及文件 |
| --- | --- | --- |
| 1.0.27.83 | `ShouldDeferCharacterIconResourceToExternalRuntime` 对非资源型运行时提供者放宽（CZN 头像） | `Core/SkinService.cs` |
| 1.0.27.84 | `RemoveStaleProviderCharacterSelectControls` 对运行时提供者跳过清理（CZN 背景切换工具条） | `Ui/ContextualSkinControls.cs` |

详见 `ISSUE_2026-11-07_czn_select_icons_and_toolbar.md`。

## 当前状态

- 当前版本 `1.0.27.84`，已部署到桌面测试包与游戏 `mods` 测试包；
- LogicTests 通过；RuntimeTests 仅剩历史遗留失败 `OtherCompendiumUiTests`；
- 证据保留：`Ui/ContextualSkinControls.cs.broken_20261107`、
  `STS2_Work\partials_backup_20261107\`、`STS2_Work\recover_82\`（反编译产物，仅参考）；
- 本次改文件前留了 `.bak_20261107b` 备份（`Core/SkinService.cs`、`Ui/ContextualSkinControls.cs`）。
