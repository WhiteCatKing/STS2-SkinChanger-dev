# 正式版与测试版同一产物兼容

正式版用户日志：v0.107.1，预览失败 Method not found: Godot.Node2D NCreatureVisuals.get_Body()。正式版没有 Body 属性，仅有 GetCurrentBody()；此前仅在 beta 编译，没有执行最低版本构建和同产物验证，IndependentFoolSkinCompat 的直接 getter 引用使进入预览路径的任意角色都失败。

修复：GameBranchApi.CurrentBody 按实际属性/方法解析；商人/先古预览的 RunState.CreateForTest 按本端原生可选参数构造，保留玩家与 seed；事件 AddDetailsTo 按能力调用；动画 SetAnimation 不再编译绑定旧版返回值。MLM Mod 单程序集/程序集列表字段以反射统一枚举。瓦库 ConfigureBackgroundScene 改为可选目标，原有彩蛋资源存在性门槛保留。三款愚者皮肤只有实际游戏资源存在 fool 角色才进入探测和目录，不尝试把第三方 beta 专属内容移植到正式版。

验证环境：正式程序集 MVID 97f10687-c306-4798-ab75-8b9f23f34dfb（当前游戏 v0.107.1），测试程序集 MVID 9d417b39-1d40-4fc7-b993-28acdc29e151（此前 v0.113.1 测试快照）。生产以正式版引用 Release 编译；测试版 ReleaseBeta 单独目录构建。tools/check-dual-branch.ps1 向两个测试目录复制同一生产 DLL，并核对 SHA-256；这避免仅各自重编译导致的虚假双端支持。

两端同一生产 SHA-256 41D64B19A4FC64B752217088C2DC6B47767E11D35A78CDFE1D160955DDDB74F3：接口审计转换 0、未解析 0（16 项泛型引用由审计工具标为未验证）；预览相关方法 JIT 预编译通过。--test-game-branch、--test-character-select-modes（正式版只检查存在的标准界面）、--test-runtime-safety、--test-protected-boundaries、--test-preview-binding、--test-merchant-lifecycle、实际鸡煲 DLL 回归均通过。旧版没有新模式及新商店派生类型，测试按实际类型存在性覆盖，原断言用于实际存在的入口。

正式版 check-protected-characters.ps1 全通过；同一生产 DLL 在测试版运行器中通过实际 Crusader 钩子和真实贞德/Crusader 13 变体、24 次调色切换回归（资源基线使用当前正式版 PCK，共有角色资源）。正式版另通过真实 MLM 一键排序/早期错误回归、无愚者时三个 beta 专属包排除回归。双端构建零警告零错误。

没有启动或控制游戏；最终画面、网络会话与全部第三方 Mod 组合不属于本次离线证明。第三方仅测试版模组仍受其自身游戏内容及依赖要求限制。
