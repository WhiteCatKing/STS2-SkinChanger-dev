# 未点击角色的选人头像未按保存皮肤显示

内部版本：1.0.34.4。公开工坊版本仍为 1.0.34，本次不上传。

## 现象与原因

工坊 3838976517（FoolSkinStatic）和 3838977271（Fool_Waifu）保存了愚者皮肤，进入选人界面时愚者头像却是原版，点击愚者后才变正确。前者通过 NCharacterSelectButton.Init 的 postfix 修改 Icon 贴图；后者通过 CharacterModel 的选人头像 getter 返回私有图片。现有 RefreshAllCharacterButtonIcons 虽遍历所有角色，但这些代码不属于当前聚焦角色的活动 runtime，所以重刷仍取到原版值；按钮 Init 的呈现回放也只发生于点击后的当前角色。

## 修复

- 新 CharacterStripPortraits 按每个角色保存的 visual runtime 选择读取头像，不改变当前角色、联机大厅或活动 provider 集合。
- 通过既有 inspection 入口取得所选程序集，不调用初始化器、不 PatchAll。缓存 Harmony 声明，只识别两个选人头像 getter 的 texture-result postfix 和按钮 Init 的 postfix；支持类型/方法声明合并与显式 TargetMethods 合同，不按这两个 Mod 的 ID 写专用分支。
- 原生 getter 在活动 runtime 下继续工作；未活动时在所选资源作用域内定向回放 getter，Init 仅绑定当前按钮和当前角色。不会调用 SelectCharacter 或重建背景、战斗场景。
- 有自己规范头像文件的皮肤继续使用原资源读取优先级，避免代码头像覆盖所选资源/设置变体。未解锁按钮跳过仅提供普通头像的 Init 回调，专用 locked getter 仍保留。
- 同步 Icon / IconAdd 两层。即使当前浏览的是没有皮肤分组的内容角色，也刷新其余角色头像。一名角色失败继续由原有逐角色异常隔离处理，不阻断后续按钮。

## 离线验证

- Release 正式版引用构建零警告、零错误；同一生产 DLL 通过 stable v0.107.1 / beta v0.113.1 的 check-dual-branch.ps1，含 ABI、三模式入口、运行时安全、预览、商人和旧鸡煲回归。
- 新 --test-character-strip-portraits：两份真实 DLL 的声明在未启动其 initializer 前可识别；beta 拦截真实作者图片读取，重复 400 次普通/锁定 getter 派发、200 次真实 Init 回调参数绑定；校验列表刷新入口接入且不切换焦点/初始化包。正式版缺少 Fool 类型，只验证声明识别，实际 Fool 方法执行由 beta 覆盖。
- --test-mode-presentation-parity 通过，包含真实 CznCore 多角色事件所有权、三模式、头像双层合同。
- check-protected-characters.ps1 完整通过，实际十字军/两份贞德共 13 变体和 24 次战损调色回归保留。
- 未启动/控制游戏，最终渲染未实机验证。通用支持范围是上述头像合同；未宣称未知第三方未来所有私有头像机制都已验证。

生产 DLL SHA-256：2D8CF0FF44524F473A28B1AA52419F2F5A67AE2E0701659AC3463A43B0DFF4BD。
