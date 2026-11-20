# Skin Changer 开发约定

动手前先读两份：
- [docs/agents/maintenance-rules.md](docs/agents/maintenance-rules.md)：不许丢的修复、改完必须做的双引用构建与测试、需要停下来先问的产品取舍、交接格式、排查第三方 Mod 的经验。
- [docs/agents/maintenance-workflow.md](docs/agents/maintenance-workflow.md)：一个问题从报上来到交付的六个步骤和每步的通过标准，末尾有交件前核对清单，每次交件逐项核对。

## 当前环境

- 主环境为 Windows 原生 PowerShell。仓库位于 `C:/Users/gurio/Documents/ChatGPT/STS2-SkinChanger`。
- 使用 Windows 的 `dotnet` 构建和测试。SDK 版本由 `global.json` 约束。
- `Directory.Build.props` 集中管理游戏引用：默认正式版 `v0.107.1`；测试版 `v0.111.0` 必须显式传入 `GameAssemblyDir`。不能因为正式版文件缺失而自动改用测试版。
- 代码与配置使用 LF；不要批量改写整库换行。

## 发现与修改

- 优先使用 codebase-memory-mcp 的图查询；当前 Windows 索引名为 `STS2-SkinChanger`。命名、增量索引与变更检测的注意事项见 [docs/agents/codebase-memory.md](docs/agents/codebase-memory.md)。
- MCP 未重新连接时，可用 `codebase-memory-mcp cli` 调用同一套工具。工具不可用、索引排除或解析不完整时再用 `rg`，不要运行 CodeGraph。
- 定位、调用链、影响面先走图查询，不要习惯性先 `rg`；改完代码用 `detect_changes` 自查影响面。只有目标在索引之外（游戏 `sts2.dll`、框架、第三方 Mod 包等二进制）时才用 `rg` 或反编译。
- 每个修复在独立分支上完成，实机验收通过后合并到 master。
- 排查中已确认原因的错误直接修复并验证，不再单独停下来询问是否修复；涉及新的设计取舍、额外权限或无法确认原因时再说明。
- 实机验收通过后提交，并提升四段内测版本：未上传时只加第四位，第三位只在实际上传工坊时提升，并与三段公开版本一起更新。四段内测标记需同步 csproj、Entry、SkinChanger.json 和 workshop/workshop.json。
- AI 不操作游戏。画面、点击、加载顺序相关的改动，交付时附实机测试步骤（含预期结果），由维护者在游戏里确认；离线测试结果和实机结果分开报告。

## 验证与发布

- 常用命令见 README 的“构建”和“开发检查工具”。发布产物必须来自默认正式版引用的 `Release` 构建，保持 `AnyCPU`。
- 测试版验证用 `ReleaseBeta` 配置，避免覆盖正式版发布产物。
- 部署前确认游戏已退出；核对构建产物、workshop/content、测试版工坊目录和正式版快照缓存的 DLL 版本及 SHA-256。
- 仅用户要求时上传；先 Steam 工坊，再推 GitHub。工坊标题/描述变化须同步全部 15 种语言。
- 游戏内工坊清单的重启标签核验规则见 [docs/agents/workshop-catalog.md](docs/agents/workshop-catalog.md)。
- 不向全局 Python 安装项目依赖；本项目核心构建不需要 Python。

## 写代码

- 只为实际出现过、或能写出具体触发条件的情况编写处理分支；每个新分支在提交说明里写明对应的日志行或复现步骤。
- 异常在 Harmony 补丁入口统一捕获并记日志；内部逻辑出错时直接抛出，便于定位。
- 修 bug 时直接改对出错的那段逻辑，保持改动行数最少。
- 配置项、开关和抽象层只在维护者提出时添加。
- 复核时只检查交代的要点，按正常玩家的用法判断；其他想法放进末尾的"可选建议"一节。

## 写回复

- 每次回复分两段：
  1. 给维护者看：三句话以内，用会玩这个游戏、但不写代码的人听得懂的话，说明出了什么问题、玩家会看到什么、现在的状态、需要维护者做什么。
  2. 给 AI 看的技术细节：文件、函数、测试名都写在这一段。
- 每句话只说一件事，先说结论再说原因。
- 术语第一次出现时，用半句话说明它是什么。
- 拿不准的地方，写出确认的办法和下一步动作。
- 更多说明见 [docs/agents/gpt-prompting-notes.md](docs/agents/gpt-prompting-notes.md)。
