# 交接：正式版兼容（单一产物）现状 · 2026-11-22

交接对象：接手"同一份 DLL 同时跑正式版与测试版"这条线的人。本文只讲**当前事实与约束**，历史来龙去脉见 `docs/issues/ISSUE_2026-11-17_stable_beta_single_binary.md`。

## 一、规则（已经在跑，别改）

- **一个产物，两端可用。** 生产构建使用最低支持版本的引用，正式版与测试版跑同一份 `Gurio.SkinChanger.dll`。
- **引用目录**：正式版/最低版本 `C:\sts2refs\stable`（当前内容是游戏 v0.111.1 的文件）；测试版必须**显式**传 `-p:GameAssemblyDir='D:\steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64'`，用 `ReleaseBeta` 配置，且不能覆盖正式版发布产物。
- `Directory.Build.props` 里的默认 `GameAssemblyDir` 指向一台 Steam depot 路径，本机不存在，所以**任何构建都要显式传**（`C:\sts2refs\stable` 或测试版目录），否则会失败而不是"自动回退"。
- 版本号双轨：公开三位（如 `1.0.38`）+ 内测第四位（如 `1.0.38.12`）。只有真正上传工坊时才升第三位；四位内测号要同步 `STS2SkinChanger.csproj`、`Entry.cs`、`SkinChanger.json`、`workshop/workshop.json` 四处。
- AI 不启动游戏。离线门禁跑完，画面/交互类改动附实机步骤，由维护者确认。

## 二、必须小心的两处构建陷阱（都实际踩过）

1. **测试项目会覆盖发布产物。** 执行 `dotnet build tests\STS2SkinChanger.RuntimeTests -c Release -p:GameAssemblyDir=<测试版目录>` 时，MSBuild 顺带重编产品项目，把 `STS2SkinChanger\bin\Release\Gurio.SkinChanger.dll` 换成"引用测试版"的版本（大小可能一样、SHA-256 不同，1.0.38.1 就此差点发错包）。
   → 部署前一律用 `dotnet build STS2SkinChanger\STS2SkinChanger.csproj -c Release --no-incremental -p:GameAssemblyDir='C:\sts2refs\stable'` 重编，并核对 3,7xx,xxx B 与 SHA-256 前缀。
2. **RuntimeTests 的引用要选对目录。** 用 `D:\steam\steamapps\common\Slay Spire 2\data_sts2_windows_x86_64`（**没有 "the"** 的那个目录）编译会报
   `error CS0246: 未能找到类型或命名空间名"AssemblyHasScriptsAttribute"`；
   换成 `C:\sts2refs\stable` 能编过但运行时报
   `System.IO.FileNotFoundException: Could not load file or assembly 'Sentry.Godot, Version=1.0.0.0…'`（`EXIT=-532462766`）。
   正解：先删 `tests\STS2SkinChanger.RuntimeTests\bin\Release` 与 `tests\STS2SkinChanger.ScriptPathFixture\bin\Release`，再用**带 "the"** 的 `D:\steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64` 重编。

## 三、当前门禁清单（每次交件都跑）

1. `Release` + `C:\sts2refs\stable`：0 警告 0 错误。
2. `tests\STS2SkinChanger.LogicTests\bin\Release\net9.0\STS2SkinChanger.LogicTests.exe`，末行必须是 `Skin Changer logic policy tests passed.`。
3. `tests\STS2SkinChanger.RuntimeTests\bin\Release\net9.0\STS2SkinChanger.RuntimeTests.exe`：完整跑 116 行、`EXIT=0`、末行 `Workshop policy tests passed.`。
4. 受保护子集（按改动面选跑）：`--test-runtime-safety`、`--test-protected-boundaries`、`--test-preview-binding`、`--test-crusader-hooks 'D:\steam\steamapps\workshop\content\2868840\3801530111\CrusaderSkin.dll'`，各自 `EXIT=0`。
5. 部署两处 mod 目录（`D:\Storage\Desktop\工作2\STS2皮肤切换器_mod测试版安装包`、`D:\steam\steamapps\common\Slay the Spire 2\mods\STS2皮肤切换器_mod测试版安装包`），旧文件改名 `*.disabled_<日期><序号>`，不删；写完回读大小与 SHA-256。

## 四、正式版侧已知边界

- **API 面**：`Creature.Monster`、`Creature.PetOwner`、`NCreature.Entity` 等在 `C:\sts2refs\stable`（v0.111.1）里都存在，所以 1.0.38.x 这些改动能编在最低版本引用上，正式版不会因为类型缺失而起不来。
- **第三方面向正式版的功能缺失不算我们的 bug**：小红帽两包（工坊 `3841828104`/`3841824454`）的 manifest 写着 `min_game_version: 0.113.1`，正式版 v0.107.1 根本不会加载它们，因此"小红帽兼容"只在测试版可见；正式版用户看不到这两个包，也就看不到相关选项。
- **正式版专属修复不能丢**：`60eb38f4`（1.0.37.4）给 `Core/VakuuVanillaArt.cs` 加 `Prepare()`，修的是正式版图鉴「切换 / 其他图鉴」入口消失，回归时按这条检查。维护规矩里"不许丢的修复"清单见 `docs/agents/maintenance-rules.md`。
- **离线证明的边界**：离线门禁只能证明"同一产物在两端都能加载并跑通既定断言"，不能证明最终画面、网络会话、任意第三方组合。这些必须由维护者实机确认。

## 五、下一步（按优先级）

1. 魄罗换肤不生效（见 `docs/refactor/handoff-redhood-poro-20261122.md` 第四节）——只影响测试版，但会挡住 1.0.38 之后的发布。
2. 双端回归：下次发包前把 `tools/check-dual-branch.ps1` 与第 3、4 条门禁一起跑一遍，确认正式版引用下产物 SHA-256 与测试版运行时一致。
3. `C:\sts2refs\stable` 里残留旧 `Gurio.SkinChanger.dll`（3,523,072 B，属 1.0.36.2）——只是引用目录里的历史副本，不影响构建，但容易被误当"最新产物"，需要时清掉。
