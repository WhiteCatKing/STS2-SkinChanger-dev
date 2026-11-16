# 三个愚者运行时皮肤无法识别

本地真实包：3838977271 Fool_Waifu 1.0.6，3838744026 NailongFool 0.2.7（奶蛙 Naiwa），3838976517 FoolSkinStatic 0.1.0（愚者单图皮肤）。工坊网页工具无法访问，身份、版本、打包和行为以本地清单、PCK 及 DLL 核对。

第一包使用 fool_waifu 私有路径 GDScript 自建骨架，非规范覆盖路径；第二包清单 has_pck=false，但 NailongFool.assets 是有效 Godot PCK，原代码自行 MountAssets 并用 GenerateAnimator 等补丁实现外观；第三包不带 PCK，FoolSkinStatic.Art.* 图片嵌入 DLL，以过程动画绘制。原探测不具备这三套独立呈现合同，无法生成可选项；不应泛化为遇到愚者类型引用就接管任意工具 Mod。

新增精确的清单身份 + DLL 程序集身份 + 实现类型 + 外观资源合同校验，仅给满足真实合同的包生成 fool 独立运行时选项。Normalize 读取同名 .assets 并验证 PCK 头，Build/Probe 一致处理；SkinService 描述符同步用于缓存戳。可选项进入既有 direct-character-runtime 激活/清理路径。

奶蛙只在被切换器激活时拦截原作者私有 MountAssets 方法，避免其高优先级全局挂载绕过选择隔离；资源由现有 coherent runtime pack 管线处理。不改作者文件。愚者单图皮肤预览只执行 EnsureCharacter/MakeSprite/idle/BlinkLoop(Sprite2D)，不调用战斗 Ready/攻击或死亡订阅。其他两包继续使用原有模型/动画回调。

--test-independent-fool-skins：三个实际包识别、DLL/游戏性/未知 ID 负例、实际 .assets 建目录、完整愚者选项、分别选择只激活一个、默认不激活任何包、真实奶蛙 MountAssets 拦截及单图预览方法合同通过。--test-legacy-defect 与 check-protected-characters.ps1 全通过；Release 零警告零错误。没有运行游戏，动态 GDScript、最终动画、声音与原作者设置交互未实机覆盖。
