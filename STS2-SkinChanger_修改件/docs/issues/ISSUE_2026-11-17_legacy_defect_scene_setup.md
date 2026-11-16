# 鸡煲娘化仅头像生效

实际提供者 DefectSkinMod，目录 皮肤/鸡煲/杂鱼鸡煲娘化/DefectSkinMod，版本 1.0。用户日志记录原初始化器失败：SelectCharacter 已不含 characterModel 参数；随后的“失败前已装入作者补丁”分支保留了部分补丁。

反编译确认 Entry.Init 顺序是 Harmony.PatchAll -> DefectSkinGlobals.Preload。PatchAll 在旧选角 Postfix 处失败，导致 CharacterScene/SelectBgScene 始终为空。已注册的战斗、商店、休息点回调依赖这些字段，因而失效；头像来自可独立替换的资源，仍能生效。场景分别为 res://Scenes/DefectCharacter.tscn 和 DefectSelectBg.tscn，使用 AnimatedSprite2D 动画而非游戏规范骨架路径。

修复仅匹配真实 DefectSkinMod 程序集和旧回调合同，在既有提供者激活作用域内检测并执行缺失的 Preload，不重跑 PatchAll。将未安装的旧选角 Postfix 补入既有 CharacterPresentationPatches，利用既有参数适配绑定当前 screen/button/CharacterModel，并沿用选择隔离和呈现变动恢复。保留原有战斗动作、死亡、商店及休息点回调；不修改第三方 DLL/PCK。

--test-legacy-defect 使用真实 DLL：拦截其实际 Preload，验证两字段补齐和重复调用不重载；确认新版游戏确实没有 characterModel 参数，现有回放参数构建器正确传入选中的屏幕/按钮/模型。check-protected-characters.ps1 和 --test-merchant-lifecycle 通过；Release 零警告零错误。没有运行游戏，资源加载和最终动画渲染仍有实机验证限制。
