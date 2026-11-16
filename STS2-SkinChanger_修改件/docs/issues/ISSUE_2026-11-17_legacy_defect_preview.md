# 鸡煲娘化小预览仍显示原皮

用户确认 1.0.33.12 局内模型正常、选角背景正常，仅小预览原版。FrameworkModelPreview 虽调用 ReplaySelectedCreatureNodeReady，但该入口明确拒绝 FrameworkPreviewCreature，避免其他皮肤调用 UpdateBounds 时访问菜单中不存在的战斗节点。鸡煲娘化实际在 NCreature._Ready 中添加 DefectMecha AnimatedSprite2D，因此该保护同时跳过了其身体替换。

保留全局保护，仅为激活且选中的 DefectSkinMod 提供预览专用 ApplyPreviewBody：使用原 CharacterScene 创建预览拥有的独立 Node2D，保留作者 2.5 倍缩放与 Idle 动画，隐藏原生 %Visuals。放在共享预览模型创建与视野采样之前，模型随预览根节点释放；不注册作者全局 ActiveSprites、不创建方向雷达、不重放 NCreature 战斗 Ready。

扩展 --test-legacy-defect：验证真实提供者 CharacterScene 来源、无关程序集排除、共享预览入口实际接入、选中和激活门槛，以及没有调用战斗 Ready/UpdateBounds。原预载和参数适配断言保留。check-protected-characters.ps1 全通过；Release 零警告零错误。未控制游戏，最终动画和取景未实机验证。
