# 2026-11-17：1.0.33.5 极速/恐惧选人界面同步

## 原因与官方结构核对
- 当前游戏的 NBlitzScreen、NDreadScreen 与 NCharacterSelectScreen 是三个独立 NSubmenu 实现，均实现 ICharacterSelectButtonDelegate，并非标准选人界面的子类。
- 原控件、预览、选择事件及联机绑定仅接受标准界面，模式界面只显示原生背景和角色头像，没有皮肤入口。
- 实际 dread_screen.tscn 没有 InfoPanel；AnimatedBg 位于 SubViewportContainer/SubViewport 内。不能依靠标准界面的固定背景路径，更不能重建成标准模式而丢失恐惧模式的灰度 Shader/规则。

## 实现
- CharacterSelectScreenAccess 统一严格识别三个原生界面，读取各自大厅/背景/文字字段，新增模式通过运行时发现，旧版本缺少类型时跳过。
- 原标准模式皮肤下拉、随机/包/合并编辑、模型预览、骨妹专属奥斯提预览、异步加载/取消、头像刷新及作者变体/战损输入共用 Control 界面入口，配置仍按同一角色皮肤分组保存。
- 极速/恐惧的 SelectCharacter、OnSubmenuOpened、AfterInitialized、Process、单人/多人开局入口挂接原有逻辑；不运行标准模式的游戏初始化，不修改官方模式/大厅状态。
- 恐惧只增加透明的外观控件锚点，不复制标准角色说明面板；所有背景加载、位置恢复、战损点击定位均读取实际背景宿主，保留其子视口和灰度材质。
- 作者只接受 NCharacterSelectScreen 的标准模式回调不强行传入其他模式，避免参数不匹配；共用资源、手动变体和保护区战损路径照常工作。需要作者独有且写死标准界面的复杂额外面板，仍取决于作者对新模式的支持。

## 验证
- 新 --test-character-select-modes：当前真实三种界面类型、各自大厅/背景字段身份、共享选择/模型/奥斯提接口、极速/恐惧选角/开局/联机入口真实 Harmony 安装与普通 Control 排除。
- 托管离线测试跳过 Dread 原生 StringName 静态初始化，避免无 Godot 运行时调用 native ABI；不以此代替画面验证。
- 十字军/贞德真实包全变体、战损调色页、运行时缓存/退出保护、预览绑定保护完整通过；新商人身份/生命周期及瓦库散图专项通过。
- Release 0 警告 0 错误。未控制游戏实机验证，控件实际布局和作者专属扩展的效果属于离线覆盖限制。

内测 1.0.33.5，公开版继续 1.0.33。同步四处安装产物，提交推送 Git，不上传工坊。
