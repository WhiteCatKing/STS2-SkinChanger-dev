# 十字军与贞德保护区

用户明确要求保护这两款皮肤及其所有变体，避免修其他问题时反复回归。

- 不因其他功能的修复改写这里的行为；共享能力仍服务其他皮肤，保持原命名空间及调用接口。
- 确需修改时，阅读 README 的边界，并运行 tools/check-protected-characters.ps1；失败必须解决，不能删除或放宽原断言来交付。
- 外部共享入口的改动同样须回归：ProviderModelDiscovery、FrameworkSkinRuntimePatches、CharacterAppearanceRuntime、ContextualSkinControls、SkinService 的资源键/加载/清理。
- 保留作者卡牌动画、设置、血量阶段、头像、快捷键、点击切形态和双包区分；不能只保留静态模型。现有停用标志不是启用授权，不要顺手翻转。
- 不直接修改第三方 DLL/PCK，不控制游戏验证。离线结果与实际画面分开报告。
