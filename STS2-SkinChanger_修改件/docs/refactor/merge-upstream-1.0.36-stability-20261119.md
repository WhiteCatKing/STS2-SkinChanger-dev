# 合并上游 `fix/1.0.36-stability`（1.0.37.0）

日期：2026-11-19 · 维护者：yuinn 一方提供补丁分支，本地合并发布

## 改了什么

**一、合并上游分支**

- 来源：`origin/fix/1.0.36-stability`，单个提交 `e23097a`「1.0.36.101：防崩溃与兼容修复（基于 1.0.36.2）」，merge base 为 `198f031`（我方 1.0.36.2）。
- 规模：67 个文件、+3693 / −447，纯源码与测试文档，无二进制产物。
- 合并方式：`git merge --no-ff`，自动合并无冲突。

**二、版本号 1.0.36.101 → 1.0.37.0**（公开号 `1.0.37`）

| 文件 | 字段 |
| --- | --- |
| `STS2SkinChanger/STS2SkinChanger.csproj` | `Version` / `AssemblyVersion` / `FileVersion` / `InformationalVersion` |
| `STS2SkinChanger/Entry.cs` | `InternalTestVersion` |
| `STS2SkinChanger/SkinChanger.json` | `version` = `1.0.37`、`internal_test_version` = `1.0.37.0` |
| `workshop/workshop.json` | `version` = `1.0.37`、`internalTestVersion` = `1.0.37.0` |

`localizations.json` 的 15 语言版本串与更新文案未动；`workshop.json` 的 `featureUpdate` / `changeNote` 仍是 1.0.36 文案。两者都在实际上传工坊时统一重写。

**三、规矩落盘**

- 维护者的《给 AI 的维护规矩》落到 [docs/agents/maintenance-rules.md](../agents/maintenance-rules.md)（本机正式版引用目录按实际值补注）。
- `AGENTS.md` 顶部加了指向该文件的链接。

## 为什么改（这次合并带进来的根因修复）

| 症状 | 根因 | 修法 |
| --- | --- | --- |
| 切换 CZN 等大包几次就闪退 | 完整视觉包挂载后「文件归属」记录被整批删除，下次把上千个未变文件当成变更，每次切换重写 155–195 MB 副本且卸不掉 | 挂载后把路径归属记到整包名，同包再选直接复用（`MountSelectedLargeRuntimeProviderPacks` / `MountedOverlayOwners`） |
| 领卡牌奖励时进程消失 | 读卡面每次绕过缓存重载整张图，托管缓存只有 64 张，回收线程里崩 | `CardPortraitSessionCache`：本局每卡只加载一次，退出显式释放，上限 1024 张 / 512 MB |
| 铁甲「恶魔」皮肤在选人就崩 | `ironcladSkin4` 包里只有导入成品、没有 `.import`，选人图集少一页贴图 | `SkinCatalog.SpinePreflight` + `ImportedTexturePathPolicy` 补贴图页预检，缺页退回原版并记日志 |
| 正式版联机大厅补丁装不上 | 补丁按参数名 `isRotatingDread` 对接，正式版 v0.111.1 改名为 `isRandomCharacterResolution` | 不再绑定该参数（它只决定一句日志文案） |
| 缓存无限增长 | 只有单包上限、没有总量约束 | `CacheWriteBudget` 总预算 16 GB + 写包前按字节查余量 + `CacheWriteNotice` 游戏内提示（15 语言） |

另外带进来的兼容与功能：同 ID 同名程序集复用（涅奥商人两份包）、`OrchisSkinFixCompat` 商店 / 火堆回调按玩家选择限制（未整包禁用）、`CharSelectTakeoverSuppressor` 支持晚载补装、遗物图跟随皮肤包选择、卡池未识别的 Mod 卡保留原图、新功能「把怪物设置应用到所有英雄」。

## 怎么验证的

1. **双引用构建**（都 0 警告 0 错误，`AnyCPU`）
   - 正式版 `-c Release -p:GameAssemblyDir=C:\sts2refs\stable` → `bin/Release/Gurio.SkinChanger.dll` 3,588,096 B
   - 测试版 `-c ReleaseBeta -p:GameAssemblyDir="D:\steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64"` → `bin/ReleaseBeta/Gurio.SkinChanger.dll` 3,784,192 B
   - 说明：本机只有一套游戏安装（测试版 v0.113.1），正式版引用取历史构建留下的 v0.107.1 基线副本，已转存到仓库外 `C:\sts2refs\stable`（`sts2.dll` 9,364,480 B / 2026-11-02），不再受 `clean` 影响。
2. **LogicTests**：末行 `Skin Changer logic policy tests passed.`，新增的缓存预算、怪物设置广播、PCK 预检用例全过。
3. **完整无参数 RuntimeTests**（先 `SetErrorMode(3)`）：退出码 0，91 条 `passed`，末行 `Workshop policy tests passed.`。新增组全部命中：`CardPortraitSessionTests`、`LargeProviderOverlayTests`、`SpineResourcePreflightTests`、`ProviderAssemblyReuseTests`、`OrchisRoomCallbackTests`、`LateTakeoverGuardTests`、`CacheWriteFallbackTests`、`CacheWriteNoticeTests`、`MlmLaunchPlanSyncTests`、补丁逐类安装隔离。
4. **双引用一致性**：`tools/check-dual-branch.ps1` 两个引用各跑六组 `--test-*`，`beta: same production SHA-256 E392F0A33EB8C5C498123D3BF1FFCA65D6F8E6BD73F105FA7B02937A72C0507E passed`，退出码 0。

## 还没做 / 未验证

- **需实机确认**：本次合并未在游戏内跑过（按约定实机由维护者本人做）。建议验证顺序：① 开一局打几个怪，确认进战斗与卡牌领奖不闪退；② 领卡牌奖励 3–5 次；③ 选人界面连续切 CZN 角色约 10 次，确认每次切换写入量是 0–5 MB 而不是上百 MB；④ 缓存被拒时是否弹出提示。
- 缓存 16 GB 上限、崩溃诊断目录限额（512 MB / 14 天 / 份数）只有离线用例覆盖，真机磁盘行为未观测。
- 上游明确未修、仍然开着：鸡煲充能球与联机同步、沙耶开局黑屏（无法复现）、选人界面 CZN 娜嘉小人偏小、「其它图鉴」事件选项文字位置偏移。
- 发布相关：`localizations.json` 的 15 语言版本与更新文案、`workshop.json` 的 `featureUpdate` / `changeNote` 仍是 1.0.36 的内容，上传工坊前需按 1.0.37 重写。
