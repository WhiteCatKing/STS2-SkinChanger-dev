# #276 对局清理后卡图已释放

基线 `fix/1.0.36-stability` 的 `6a8a9f6`（1.0.36.105）。报告使用的 1.0.37.2 与该基线的 `CardPortraitSessionCache` 和退出清理代码相同。

## 定位

- 事故日志第 795–796 行记载 `AI_silent_NSFW_MOD` 已被原生加载，第 2403 行有加载顺序警告。第 9938–9941 行联机断线回主菜单，第 10198 行重新进入同一局，第 10408 行首次出现 `Godot.AtlasTexture` 已释放异常。异常点是 `project_export_mod.PortraitPatch.RestoreDefaultState → TextureRect.SetTexture`，栈还出现在抽牌、打开牌库和 `NCardPlayQueue`。另一份脱敏副本第 16224 行是故障的防御牌，说明该回调也触及非猎人卡。
- 切换器旧版在 `RunManager.CleanUp` 后清会话缓存，对已传给卡牌的 `Source` 和 `View` 调用 `Dispose()`。在 Beta 夹具里用旧实现复现了同一条已释放异常。第三方全局回调保存的是切换器曾返回的同一托管包装，退局后还会使用它。换新版后，同样的跨局缓存、第二张卡节点的 `_EnterTree` 与 `UpdateVisuals` 均成功。
- 游戏第 9960 行的 `Unloading 4 missed cache assets` 经 IL 核对，会对游戏缓存移出的对象安排帧末释放。切换器的隔离卡图加载走 `Ignore` 或 `IgnoreDeep`，其根贴图未登记到游戏 `AssetCache`。第 9966 行天音的 `run-cleanup` 经本机实装 DLL 核对，`ReleaseAll → DropWarm` 清除的是天音自身热缓存引用，`Dispose` 调用属于枚举器。日志没有对象 ID、四条资源路径或第三方字典内容，无法反向确定玩家现场那张贴图的实际来源。已确认切换器的清理方式足以独立复现异常。

## 修复

- `CardPortraitSessionCache` 清空请求、会话、失败项和驻留预算时允许外部继续使用已发布的托管包装。以弱键记录已接受的源图和图集视图。贴图对托管代码不可达后，由配套终结器只把托管引用放进队列。场景树的 `ProcessFrame` 在游戏线程调用 `Dispose`，每帧最多 32 个。被拒绝且尚未交给外部的贴图当场释放。沿用 512 MiB 活跃缓存预算、LRU 和隔离包复用。由第三方长期持有的贴图会随其生命周期延长。
- `ManagedSkinModLoader` 仅在识别到 `AI_silent_NSFW_MOD` 自身有可接管卡图时，把其 `project_export_mod.PortraitPatch` 上挂到 `NCard._EnterTree` 和 `NCard.UpdateVisuals` 的两个已知卡面后置归入接管。加载顺序若已让它原生初始化，这条识别规则无法拦截已装的补丁，跨局贴图修复仍生效。报告材料没有该包的 DLL，规则按报告清单、调用栈和代表性 Harmony 夹具验证，实包待复测。

## 验证

- 正式版 v0.111.1（Release）和测试版 v0.113.1（ReleaseBeta）均 0 警告、0 错误。LogicTests、完整无参 RuntimeTests、受保护角色回归脚本均通过。
- `CardPortraitSessionTests` 现覆盖第三方跨局缓存的 AtlasTexture、清理后建卡和刷新、会话重载、不可达后在游戏线程释放、预算拒绝立即释放和 LRU 继续可用。旧行为复现日志在 `%TEMP%/sts2-gpt-276/compare/repro.log`，双端回归日志在同目录上层的 `official-final.log`、`beta-final.log`。
- 未提交、未部署。玩家原组合与旧奖励闪退仍需实机回归。

## 实机步骤

1. 测试版装报告同款 AI_silent 1.0.3，先保留报告的原生先加载顺序并重启。选其猎人卡图，可用其他职业单机。进局抓牌并打开牌库，让第三方记住部分原始贴图。
2. 回主菜单后继续同一局，先打开牌库，再进战斗抽牌和出牌，确认手牌布局、出牌动画与奖励选择正常。查日志有无 `RestoreDefaultState → TextureRect.SetTexture` 已释放异常。条件允许时再用联机断线重进确认队友飞牌。
3. 调整到切换器先于该卡包加载后重启，确认托管选项仍显示、猎人卡图可切换，重复上述步骤。
