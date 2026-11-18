# 皮肤切换器 · 协作维护规矩

来源：维护者（yuinn 一方）2026-11-18 起执行的《给 AI 的维护规矩》，本文件为项目内正式副本。
接手或继续开发前，先读本文件，再读下列既有记录：

- [docs/refactor/fix-upstream-1.0.35-gpt.md](../refactor/fix-upstream-1.0.35-gpt.md)
- [docs/refactor/fix-upstream-1.0.35-dsh.md](../refactor/fix-upstream-1.0.35-dsh.md)

## 开工前

- 只改和本次任务相关的文件；不批量改格式、不改换行（保持 LF）。

## 不许丢的修复（改到相关代码时必须保留，并保持对应测试通过）

- 完整视觉包挂载后保留路径归属（`MountSelectedLargeRuntimeProviderPacks` / `MountedOverlayOwners`）——否则 CZN 等大包每次切换重写上百 MB，几次就闪退。测试：`LargeProviderOverlayTests`。
- 隔离卡图按会话缓存并显式释放（`CardPortraitSessionCache`）——否则领卡牌奖励时在终结器线程崩溃。测试：`CardPortraitSessionTests`。
- Spine 贴图页预检与导入产物映射（`SkinCatalog.SpinePreflight`、`ImportedTexturePathPolicy`）——缺页的图集不许交给 Spine，否则原生崩溃（魅魔战士、铁甲恶魔）。测试：`SpineResourcePreflightTests`。
- 缓存总预算 16 GB 与写包前空间检查（`CacheWriteBudget`），拒绝时游戏内提示（`CacheWriteNotice`）。
- 同 ID 同名程序集复用（涅奥商人两份包）。测试：`ProviderAssemblyReuseTests`。
- 奥契丝修复包的商店 / 火堆回调按玩家选择限制（`OrchisSkinFixCompat`），**不允许整包禁用该修复包**。测试：`OrchisRoomCallbackTests`。
- `CharSelectTakeoverSuppressor`（禁止第三方改骨妹选人背景大图，防循环报错崩溃）必须保留，且支持晚载补装。测试：`LateTakeoverGuardTests`。
- 联机大厅补丁不绑定 `ChangeCharacter` 的第三个参数名（正式版 / 测试版名字不同）。

## 改完必须做

1. 正式版和测试版**两种引用都构建一次**，都要 0 错误（`AnyCPU`）：
   - 正式版：`dotnet build STS2SkinChanger/STS2SkinChanger.csproj -c Release -p:GameAssemblyDir=<正式版目录>`
   - 测试版：`dotnet build STS2SkinChanger/STS2SkinChanger.csproj -c ReleaseBeta -p:GameAssemblyDir=<测试版目录>`
   - 本机现行取值：正式版 `C:\sts2refs\stable`（v0.107.1 基线副本）、测试版 `D:\steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64`（v0.113.1）。
2. 跑 LogicTests 和**完整无参数 RuntimeTests**，必须从头跑到尾（末行 `Workshop policy tests passed.`）。完整套件现在是全绿的，不允许让它重新变红；某条测试失败，先判断是「测试过时」还是「真 bug」，**不许为了变绿去削弱断言**。
3. 运行 RuntimeTests 前先调用 `SetErrorMode(3)`，免得失败时弹调试器窗口卡住。
4. 新增界面文字走 `ModLocalization` / `ModText`，补齐 15 种语言。

## 碰到这些要停下来问维护者，不要自己决定

- 产品行为的取舍（例如未选中的皮肤要不要保留文字、上限给多少、要不要禁用某个第三方包）。
- 需要删除玩家文件、改注册表、改其他 Mod 文件的改动。
- 原因没查清的问题：先写结论和证据，不要先改。

## 交接格式（每次干完写进 docs/refactor/ 下的记录）

- 改了什么（文件和函数）、为什么改（根因）、怎么验证的（构建、哪些测试组、实机步骤）、还剩什么没做或没验证。
- 无法离线验证的画面问题，写明「需实机确认」和具体步骤。

## 排查第三方 Mod 时

- 先看日志和崩溃诊断，再动代码。
- 怀疑某个第三方包本身坏了：用 `ModApiScan` 扫它调用的游戏接口和补丁参数（工具源码由 yuinn 一方提供，需要时向其索取）。
