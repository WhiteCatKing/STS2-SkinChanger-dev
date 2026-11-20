# #236 卡图上的 NOPE

## 定位

NOPE 来自游戏原包的 `res://images/powers/missing_power.png`。从测试版 PCK 读出该路径的 S3TC 导入贴图并解码，实际像素是透明底大字 NOPE，尺寸 256×256。游戏程序集的 `AtlasResourceLoader._Load` 在找不到图集条目及专用回退时调用 `GetMissingTexture`。缺失的 `compressed` 图集归入通用分支，使用 `missing_power.png`。`card_atlas` 分支才使用 `res://images/packed/card_portraits/beta.png`，解出的那张图写的是 BETA。

- **剑圣已定位到可见节点。** 22:06 的 `godot.log` 3817–3824 行显示，RTR 全卡图已成功送到 `%Frame`。同一张卡的 `%AncientTextBg` 可见，加载的是 256×256 的 `CompressedTexture2D`，请求路径为 `res://images/atlases/compressed.sprites/card_template/ancient_card_text_bg_power.tres`。该贴图随节点的黑色、0.66 透明度绘制，和暗色字样一致。2984、3066、3068 行也记录了三类背景的缺失警告。测试版的实际文件在 `compressed_atlas.sprites/ancient_text_bg_*.tres`，正式版仍在旧位置。
- **逃脱计划有同源入口。** 仓库对 `SilentCardAnimerRework` 的这张卡有展开画面和先古布局，使用同一旧路径的先古边框。日志 3224 行记录了该边框缺失，随后 3253–3254 行显示逃脱计划的游戏图和 `ATA_Silent` 图都可加载。日志没有逃脱计划出现字样那一帧的节点快照，仍需复测两个实际选项。详情页的 `Asset not cached` 指向原版大图，和 NOPE 的来源是两条加载路径。

## 修复与验证

`SkinService.LoadCardPresentationResource` 保留选中包显式提供的资源优先级。回退到游戏资源时，`SkinCatalog` 从实际加载的游戏 PCK 确认先古边框及攻击、技能、能力背景的路径。两个路径都缺失时结束该回退，避免让图集加载器返回 NOPE。该兼容层只覆盖已核对的四项，沿用 #235 的卡图隔离。

正式版 v0.111.1 和测试版 v0.113.1 均为 0 警告、0 错误。两端 LogicTests、完整 RuntimeTests、内存守卫、保护边界、预览绑定、十字军 hooks 和十字军、两版贞德实包回归均通过。新增测试拦截实际贴图加载入口，断言旧版用旧路径、新版用新路径、两边都缺失时无加载。日志在 `%TEMP%/sts2-gpt-236/{official,beta}.log`，解码验证图在同目录的 `missing-power.png`。当前改动留在工作区，尚未提交或部署。

## 实机复测

1. 重启测试版，选 RTR，打开故障机器人的回响形态、超临界态和储君的剑圣、虚空形态。在总览和详情查看，确认完整卡图和说明背景仍在，NOPE 消失。
2. 对逃脱计划依次选 `SilentCardAnimerRework` 的两种布局、`ATA_Silent` 和「游戏默认」，来回切换并打开详情。核对各自的画面、边框和说明，并确认新版日志不再请求旧版的这四条图集路径。
3. 正式版快速对照一张 RTR 全卡画面，确认旧路径仍能绘制说明背景。


## #237 和 #238 的补充核对

云已实测确认回弹在全卡美化包与「游戏默认」之间来回切换正常。

只读核对 RTR 0.2.0。其 93 项 PCK 中有故障机器人和储君卡图的私有 PNG 导入条目和 CTEX，例如 `echoform.png`、`swordsage.png`，也有两组边框。作者的 `CardCustomBorderPatch.Postfix` 读取 `card_custom_border_enabled`，向 Frame 写整卡图，设置 AncientTextBg，背景路径正是正式版的旧 `compressed.sprites/card_template/ancient_card_text_bg_*`，并另建自己的边框节点。在本次剑圣的复现中，切换器的 `ProviderCardBehaviorScanner` 已把这些启用项转成 `UseFullFrameArt` 定义，`SkinService.GetCardPresentation` 按单卡最终选中的包读取该定义。快照在 `Reload/managed` 阶段已经显示 RTR 完整卡图和错误的游戏背景，现有证据定位的是切换器重现这套布局的路径。

全卡美化包 2.2.1 的 `FramePatch` 和 `CustomFrameRuntime` 确实会应用帧、背景等 UI；`CardUiModeSpoofPatch` 根据卡类型和界面调用环境选布局。其 `FramePatch.AncientResource` 已有测试版和正式版两套路径的存在检查。日志将这两个卡图包都登记为已隔离，卡面节点的证据来自切换器选中布局。可另行用活动 Harmony 补丁清单核对全局回调，现有证据已足以修正这次 NOPE 的准确绘制入口。
