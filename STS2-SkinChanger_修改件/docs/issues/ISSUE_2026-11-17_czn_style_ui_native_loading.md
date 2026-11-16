# CznStyleUI 原生加载边界

工坊 3771060891，本地版本 3.1.4。玩家日志明确出现“已隔离皮肤提供者 CZN Style UI：视觉组=0, 卡图=23”，且原 PCK 未全局挂载。这会阻止整体 UI 模组按作者原意加载；不是角色皮肤之间的选择冲突。

在提供者探测及目录构建两个入口，按清单 ResourceNamespaceId 精确排除 CznStyleUI，保留游戏原加载器对整包 DLL/PCK 的所有权。独立 CZN 角色皮肤继续使用既有兼容桥；不改写第三方文件或配置。

验证：--test-czn-style-ui 使用真实游戏 PCK 和真实 CznStyleUI 包，包含重复安装身份，确认无提供者探测结果、无卡图及角色选项；check-protected-characters.ps1 全部通过；Release 零警告零错误。

旧的完整 --test-czn-compatibility 测试因本地缺少 3799519206 Tiphera 包停在既有断言，没有删改断言。新建独立入口覆盖本次真实 UI 包，不依赖未安装的其他 Mod。没有运行游戏，未验证最终界面画面。
