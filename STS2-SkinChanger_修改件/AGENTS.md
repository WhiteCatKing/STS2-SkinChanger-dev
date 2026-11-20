# Skin Changer 开发约定

《杀戮尖塔 2》皮肤切换器（Gurio.SkinChanger）——独立的皮肤管理 Mod，不改写其他 Mod 的安装文件，也不依赖旧版「皮肤管理器」。

## 先读这几份

动手前按顺序读：

- [docs/agents/maintenance-rules.md](docs/agents/maintenance-rules.md) — 维护规矩：不许丢的修复、改完必须做的构建与测试、必须停下来先问的取舍、交接格式。
- [docs/agents/fix-and-test-workflow.md](docs/agents/fix-and-test-workflow.md) — 修复与测试流程：从读日志定根因到双引用构建、完整测试、部署的固定步骤。
- [docs/agents/pitfalls-checklist.md](docs/agents/pitfalls-checklist.md) — 踩坑复盘与按症状排查清单（历史实例，先看再改）。
- [docs/issues/maintenance/](docs/issues/maintenance/) — 维修日志归档（每次合并他人修复后的排查与修复记录）。

## 环境

- Windows + PowerShell；仓库根为上层目录 `工作2/`，唯一可改的源码树是本目录。
- 构建必须传 `GameAssemblyDir`：正式版 `C:\sts2refs\stable`（v0.107.1 基线副本）、测试版 `D:\steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64`（v0.113.1）。
- 代码与配置用 LF，换行与编码不要批量改写。

## 硬性约束

- 只改与本次任务相关的文件；不顺手重构。
- 改动涉及「不许丢的修复」清单里的代码时，必须保留对应行为并让对应测试继续通过。
- 交付前：正式版与测试版两种引用各构建一次，都必须 0 错误；跑完整的 LogicTests 与无参数 RuntimeTests，末行 `Workshop policy tests passed.`。
- 版本号四处同步：`Entry.cs`、`STS2SkinChanger.csproj`、`SkinChanger.json`、`workshop/workshop.json`。
- 不启动游戏做实机测试：游戏内验证由作者本人完成，我负责读日志排查。原因不明的问题先给结论和证据，不动代码。
- 提交信息用中文，写清改了什么、为什么改。

## 仓库与发布

- 远端 `origin` 指向 `https://github.com/WhiteCatKing/STS2-SkinChanger-dev`（公开仓库，`main` 为默认分支）。
- 直连 GitHub 失败时加代理：`git -c http.proxy=http://127.0.0.1:7890 -c https.proxy=http://127.0.0.1:7890 -c http.version=HTTP/1.1`。
- 仅作者要求时才上传工坊或推送远端；工坊标题 / 简介变化须同步 15 种语言。
