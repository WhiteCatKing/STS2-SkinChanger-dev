# 皮肤切换器 · 协作维护规矩

来源：维护者（yuinn 一方）《给 AI 的维护规矩》2026-11-19 版（52 行），本文件为项目内正式副本。
接手或继续开发前，先读本文件，再读下列既有记录：

- [docs/refactor/fix-upstream-1.0.35-gpt.md](../refactor/fix-upstream-1.0.35-gpt.md)
- [docs/refactor/fix-upstream-1.0.35-dsh.md](../refactor/fix-upstream-1.0.35-dsh.md)
- [docs/refactor/fix-upstream-1.0.35-gemini.md](../refactor/fix-upstream-1.0.35-gemini.md)

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

1.0.36.102 新增（2026-11-19）：

- Fool_Waifu 自恢复补丁的拦截（`FoolWaifuRuntimeCompat`）：未选中 Fool_Waifu 时不许它把切换器停掉的补丁装回来（含头像、选人按钮、商店 `AttachMerchant`）——否则其他愚者皮肤头像被占、商店 Hilde 缩小。测试：`FoolWaifuIdentityTests`。
- 角色专属先古对话随选择（`SkinCatalog.CharacterDialogue`、`HildeAncientDialogueCompat`）：整表都是 `*.talk.<角色>.*` 的 `ancients.json` 跟随该角色当前皮肤；Hilde 写入 `AncientDialogueSet` 只在选中时生成。设置 / UI 文本表仍全局。测试：`CharacterDialogueLocalizationTests`。
- 商人热切换：第三方 `_Ready` 前缀**进树后**才重放，只跑一次（`MerchantRuntimeAppearance`）——否则切换后商人点不了。测试：`--test-merchant-lifecycle`。
- 纯商人提供者不得修改玩家商店形象 `NMerchantCharacter`（`ManagedSkinModLoader` 剥离其补丁，保留 `NMerchantButton` / `NMerchantHand`）。测试：`MerchantPlayerCharacterIsolationTests`。
- 恐惧 / 极速选人：模式界面背景重做（`CharacterSelectModeBackgroundPatch`、`SetSelectedCharacterVisuals` 后置）、自绘叠加层的包在新背景入树后重挂（`ProviderModeSelectOverlayBridge`）、恐惧界面 `GrayScaler` / `BigRotatingContainer` 只在选中带点击切档的皮肤时放通点击。测试：`--test-mode-background`（`CharacterSelectModeBackgroundTests`）。
- AncientWaifus：遗物独立 PNG 切片 + `RelicModel.BigIcon`（`RelicSkinResources`）、时间线 `EpochModel.Portrait` / `RealPortrait`（`TimelineEpochSkinResources`）、CznStyleUI 图鉴兼容（`CznRelicCollectionCompat`）、触摸区由先古动画启动时挂载（`InteractiveTouchBoxBridge.AttachToScene`，**不要改回全局 `SceneTree.NodeAdded` 监听**，性能原因）。测试：`AncientWaifusSupportTests`。

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
- 常见的三类坑（2026-11-19 实例）：① 包会自己把补丁装回来（Fool_Waifu），停用后要确认它没有自恢复；② 资源放在包的私有路径、由 DLL 重定向（AncientWaifus、Fool_Waifu 的头像），资源隔离拦不住，要按入口处理；③ 包只在标准选人界面挂自己的叠加层，恐惧 / 极速是另外的界面。
- 包更新后功能失效：先对比新旧两版 DLL（旧版通常还在工坊目录的备份或用户手里），再看切换器哪条路径没接上。
- 皮肤包作者让玩家「把它排在切换器上面」的说法不要采纳：切换器必须排第一。
- 加载顺序：游戏按设置里同 ID **最后一行**的位置排序，忽略 `loadPriority`；同 ID 本地 + 工坊并存会让切换器被挤到后面。排查「皮肤大面积失效 + 主界面报重复 ID」时先查这个。
- 怀疑某个第三方包本身坏了：用 `ModApiScan` 扫它调用的游戏接口和补丁参数（工具源码由 yuinn 一方提供，需要时向其索取）。
