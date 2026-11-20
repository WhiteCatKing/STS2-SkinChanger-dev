# #233 / #234 卡图隔离和回弹回退

基线 `fix/1.0.36-stability` · `a1f2a99`

## 结论

- **回弹的同名路径由切换器生成的卡图运行包写入。** `godot(4).log` 15751 行正在批量加载 `misc/Pornographic beautification of all cards v2.2 sp`，15752 行从其 `189_card_provider` 读取导出的回弹 AtlasTexture，15773、15947 和 16022 行显示相同 UID 和依赖又出现在游戏规范路径。旧的 `BuildAliasedResourceOverlay` 同时写私有重定向与规范路径的 `.remap/.import`，Godot 的追加挂载会保留这一规范重定向。两个游戏原包均没有 `event/rebound.tres`，只有 `images/packed/card_portraits/event/beta/rebound.png.import` 及实际贴图。点「游戏默认」时，旧代码仍请求缺失的 atlas，失败后保留游戏调用已经从污染路径取到的结果。RTR 三包中负责卡图的 3748201671 没有回弹。
- **回响形态的 NOPE 像素来源待确认。** 7437 和 7470 行能确认两个导出的 Echo Form AtlasTexture 发生 UID 回退，分别指向 `MegaCrit.Sts2.Core.Models.Cards.EchoForm_card_art.png` 与 `echo_form.png`。Godot 遇到未注册的 UID 会改读文本路径，单独这条警告无法证明纹理读失败，也不能确定屏幕上的 NOPE 是哪一层。只读解出了本机 RTR 0.2.0 的 `echoform.png`、故障机器人卡框和原版回响大图，均无 NOPE。RTR 随包配置的 `EchoForm` 开启自定义边框，图像替换开关列表没有该卡。当前机器缺少日志中该同名美化包的同版本 PCK 和原截图，无法核对它的最终像素。 [Godot 二进制资源依赖加载](https://github.com/godotengine/godot/blob/4.5/core/io/resource_format_binary.cpp#L1013-L1037)

## 修复与验证

- 卡图运行 PCK 只发布会话内的私有路径。已导出的二进制卡图重写依赖路径和内部偏移，清理旧 UID，避免绕回同名规范路径。按归档中的实际拼写生成别名，因此文件里的小写引用也可使用大写导入入口。撤掉了切换卡包时重新挂载旧规范重定向的逻辑。其他资源的挂载逻辑仍按各自入口处理。
- 游戏卡的默认 AtlasTexture 缺失时，从目录查同卡的普通 PNG、Beta AtlasTexture 和 Beta PNG。回弹据此从游戏资源恢复原始 Beta 图。卡包匹配仍用原来的卡牌身份，故仍可选完整回弹卡图。正常换皮和默认回退没有改写第三方文件。
- 新回归以 PCK 模拟两个卡包共用回弹规范路径，覆盖原始和压缩二进制、UID、依赖大小写、偏移重写、各自纹理和回到游戏原图。v0.111.1 与 v0.113.1 的主工程和测试项目均为 0 警告、0 错误。两端 LogicTests、完整 RuntimeTests、内存守卫、保护边界、预览绑定、十字军 hooks、十字军和两版贞德实包全部通过。日志 `%TEMP%/sts2-gpt-233/{official,beta}.log`。当前为待复测工作区，尚未提交或部署。

## 联机以外的实机验证

1. 重启一次测试游戏，清掉旧版本在当前进程中挂载的路径。打开卡牌总览和详情。回弹依次选美化包、游戏默认、另一套包，再切回，核对默认是游戏原图且其他卡未一起换图。
2. 回响形态分别选游戏默认、RTR、美化包，核对总览和详情。RTR 默认配置应保留其自定义边框，回响的卡图开关按配置生效。如果仍出现 NOPE，留当前选中的包、界面截图和同段日志，用于定位叠加层。
3. 对照日志，加载私有回弹时不应再把该美化包的 UID 警告带到原版 `res://images/atlases/card_atlas.sprites/event/rebound.tres`。
