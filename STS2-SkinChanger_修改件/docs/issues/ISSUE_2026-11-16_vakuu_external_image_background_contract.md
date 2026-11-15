# 1.0.32.15：采纳玩家提供的瓦库散图背景兼容修复

## 来源与核对
玩家提供《给作者说明.md》《修复.diff》和 Gurio.SkinChanger.inject.dll。报告对应游戏 v0.113.1、旧内测 1.0.32.0；称已实测修复外部散图先古皮肤进入瓦库时报 TextureRect 缺失，以及彩蛋 vakuu_angry 覆盖皮肤图的问题。

静态反编译核对本机实际 sts2.dll：Vakuu.ConfigureBackgroundScene 直接 GetNode<TextureRect>("TextureRect")，随后把 ChooseBgPath() 的官方图写入 Texture。ChooseBgPath 保留普通/愤怒分支及原有存档标记逻辑。当前 BuildSelectedRuntimeImageScene 只有 Image，确实不满足节点合同。

注入 DLL 仅做哈希与静态反编译核对，MD5 F2D7910E11A7882C4881AB2D6C3326E0 与玩家说明一致，确实增加了 TextureRect 子节点。没有执行或部署该旧 DLL。本机最新日志未发现同一 Vakuu/TextureRect 报错；玩家实测结论属于其提供的证据，不冒充本机复现。

## 源码修复
- 在现有先古模块 BuildSelectedRuntimeImageScene 中保留 Image，用于显示所选 provider 图片。
- 按玩家补丁添加名为 TextureRect 的独立空槽，Owner=root，保证打包后仍可由官方回调找到。
- 额外显式 Visible=false、ExpandMode=IgnoreSize、MouseFilter=Ignore。不能仅依赖初始零尺寸：官方赋纹理后也必须保持不可见，且不能遮住事件选项输入。
- 不跳过 ConfigureBackgroundScene，不修改彩蛋判定或存档进度；官方图写入隐藏槽，皮肤图不被覆盖。
- 仅修改外部图片场景的构造，原生默认/其他场景提供者不改；不采用附件提及的可选 _angry 正则扩展。

## 验证与交付
- 专项 --test-ancient-background-contract 以实际游戏回调的节点名/类型、正常/愤怒路径合同核对生成场景，检查独立 Image、隐藏空槽、Owner/加入父节点和输入隔离。
- 相同专项使用工作区外隔离的上一版 1.0.32.14 DLL 作为负对照，明确失败于缺少独立节点；修复版通过。
- 先古卡牌优先级专项通过；Release 构建 0 警告 0 错误。
- 没有启动或控制游戏；离线合同检查不等同于实际事件选项点击/彩蛋渲染验证。
- 当前源码构建叠加全部已有修复，三文件按原流程同步四处并核对 SHA-256，本地提交与 main/v1.0.32.15 推送。
