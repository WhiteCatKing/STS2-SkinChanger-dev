# 接手说明（给 AI：GPT / Codex / 其他）

《杀戮尖塔 2》皮肤切换器（Skin Changer）的交接文档。2026-11-16 重写，取代旧 `START_HERE.md` 与 `D:\给接手AI的说明.md`。
作者是「白猫大魔王」（GitHub `WhiteCatKing`）。工坊条目 ID `3787302680`，appId `2868840`。

只读这一份就能开工；细节按需跳转 [项目自带交接](STS2-SkinChanger_修改件/docs/handoff/GENERAL.md)、[开发约定](STS2-SkinChanger_修改件/AGENTS.md)、[索引笔记](STS2-SkinChanger_修改件/docs/agents/)。

---

## 1. 只改这一个目录

```
D:\Storage\Desktop\工作2\STS2-SkinChanger_修改件\   ← 唯一要改的源码工程（git 仓库内容）
```

`D:\Storage\Desktop\工作2` 是 git 仓库根（remote `https://github.com/WhiteCatKing/STS2-SkinChanger.git`，主线 `main`）。

绝对不要动：

- `STS2-SkinChanger_源文件（坚决不能动）\` —— 原始未改版快照，只作对照。
- `STS2_Work\workshop_content_1.0.20\`、`STS2_Work\steamcmd\`、`STS2_Work\workshop_build.vdf`、`STS2_Work\publish.ps1` —— 部署与上传链。

---

## 2. 这台机器的路径（全部实测）

| 用途 | 路径 |
| --- | --- |
| 游戏安装 | `D:\steam\steamapps\common\Slay the Spire 2\` |
| 游戏程序集（构建引用） | `D:\steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64` |
| 游戏 mods 目录 | `…\Slay the Spire 2\mods\STS2皮肤切换器_mod测试版安装包\` |
| 工坊上传内容目录 | `D:\steam\steamapps\workshop\content\2868840\3787302680\` |
| 游戏日志 | `C:\Users\Administrator\AppData\Roaming\SlayTheSpire2\logs\godot.log` |
| 运行期缓存 | `C:\Users\Administrator\AppData\Roaming\SlayTheSpire2\Gurio.SkinChanger\`（`cache\`、`overlay-cache\` 可随时删，`skin_changer.json` 要留） |

注意：`%APPDATA%\SlayTheSpire2` 是指向 `D:\Storage\AppData\Roaming\SlayTheSpire2` 的 **junction**。路径比较、清理逻辑都要考虑这一点（见 §6.2）。

---

## 3. 日常四步

### 3.1 构建

```powershell
cd 'D:\Storage\Desktop\工作2\STS2-SkinChanger_修改件'
dotnet build STS2SkinChanger\STS2SkinChanger.csproj -c Release --nologo -v:m `
  -p:GameAssemblyDir="D:\steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64"
```

要求 **0 警告 0 错误**。

`-p:GameAssemblyDir` **必须显式传**：`Directory.Build.props` 的默认值指向 `D:/Programs/Steam/...`，本机不存在。测试版验证用 `-c ReleaseBeta`，避免覆盖正式版产物。

### 3.2 测试

```powershell
dotnet run --project tests\STS2SkinChanger.LogicTests\STS2SkinChanger.LogicTests.csproj -c Release `
  -p:GameAssemblyDir="D:\steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64"
```

末行必须是 `Skin Changer logic policy tests passed.`，否则视为失败。

其它测试：`tests\STS2SkinChanger.FrameworkTests`（加载原管理器 DLL）、`tests\STS2SkinChanger.RuntimeTests`（真实临时文件、`--test-storage`、`--test-github-swap`、`--audit-stateful-card-art`）。命令见 [README](STS2-SkinChanger_修改件/README.md) 第 116–187 行。

### 3.3 部署四处（同一个 `Gurio.SkinChanger.dll` + `SkinChanger.json`）

1. `STS2皮肤切换器_mod正式版安装包\`
2. `STS2皮肤切换器_mod测试版安装包\`
3. `STS2_Work\workshop_content_1.0.20\`
4. `D:\steam\steamapps\common\Slay the Spire 2\mods\STS2皮肤切换器_mod测试版安装包\`

构建产物在 `STS2SkinChanger\bin\Release\`。部署后核对四处 SHA-256 一致：

```powershell
Get-FileHash <四处 dll> -Algorithm SHA256
```

### 3.4 提交

中文提交信息写文件再传（PowerShell 不支持 bash heredoc）：

```powershell
$tmp = "$env:TEMP\commit_msg.txt"
[System.IO.File]::WriteAllText($tmp, $msg, (New-Object System.Text.UTF8Encoding($false)))
git commit -q -F $tmp
```

---

## 4. 版本号与发布

四段内测版本号要 **四处同步**（漏一处就会出现版本不一致）：

| 位置 | 字段 |
| --- | --- |
| `STS2SkinChanger\Entry.cs` | `InternalTestVersion` |
| `STS2SkinChanger\SkinChanger.json` | `internal_test_version` |
| `workshop\workshop.json` | `internalTestVersion` |
| `STS2SkinChanger\STS2SkinChanger.csproj` | `Version` / `AssemblyVersion` / `FileVersion` / `InformationalVersion` |

规则：**没上传工坊只加第四位**；真正上传工坊时提升第三位、第四位归零，并同步公开三段版本与安装包 manifest。当前公开版本为 `1.0.33`；后续内测保持此公开版本，只提高四段内测标记。

当前值：`1.0.33.4`（工坊公开 `1.0.33`）。本轮修复与验证限制见 `STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-17_private_merchant_alias_identity.md`；上次工坊发布见 `RELEASE_2026-11-16_workshop_1.0.33.md`。

发布（**只在作者明确要求时做**，顺序：先工坊再 GitHub）：

- 工坊：优先使用修改件 `tools/WorkshopPublisher`，通过已登录 Steam 客户端发布 `workshop/workshop.json`（含 15 语言回读核验；contentFolder 指向仓库 staging）。SteamCMD 旧通路仍为 `STS2_Work\publish.ps1` + `workshop_build.vdf`。上传前同步四处三文件和 Steam 工坊内容目录，并核对版本/哈希。
- GitHub：`sync.ps1`，或手工 `git -c http.proxy=http://127.0.0.1:7890 -c http.version=HTTP/1.1 push origin main`（直连会 `Recv failure: Connection was reset`）。

工坊标题/描述改动必须同步 15 种语言（`docs\publish-github-skin-mod.*.md`）。

---

## 5. 代码结构

### 5.1 启动链（`STS2SkinChanger\Entry.cs`）

```
Initialize()
 ├─ ModPrivateDependencyResolver / StartupWatchdog / SkinChangerPaths
 ├─ FrameworkCompatibilityLayer → ManagedSkinModLoader
 ├─ new Harmony(ModId)
 ├─ Entry.cs:53  Core.Features.FeatureBootstrap.InstallGroups(harmony, loadableTypes)   ← 八个模块成组装
 ├─ InstallPatchClasses(harmony, loadableTypes)                                          ← 其余常驻补丁
 │    └─ Entry.cs:107  if (FeatureBootstrap.IsManagedByFeature(type)) continue;           ← 防重复装
 ├─ 六个手装兼容层：EmperorCrabCompat / CustomSkeletonLoaderCompat / PengoTarotFontCompat
 │                  / OstyDefectImpostorCompat / BlackedEventMerchantCompat / MerchantRoomConcreteReadyPatch
 └─ 25 秒后 BackgroundSkinService.RunSelfCheck + DebugSnapshot.EnsureInstalled
```

`InstallPatchClasses` 逐个类 `CreateClassProcessor(type).Patch()`，失败就 `Unpatch()` 并记名跳过——**单个补丁类坏掉不会拖垮整个 mod**，这是刻意设计，别改成 `PatchAll`。

### 5.2 八个模块（`Core\Features\`）

共 **182 个 `[HarmonyPatch]`**：**90 个归入八模块，92 个常驻**。

| 模块 | 补丁类数 | 目录 |
| --- | --- | --- |
| Ancient | 14 | `Core\Features\Ancient\`（`AncientCompendium.cs`、`BestiaryEntranceSwitcher.cs`、`DecimillipedeBestiarySupport.cs`） |
| Background | 4 | `Core\Features\Background\` |
| Card | 25 | `Core\Features\Card\` |
| Character | 17 | `Core\Features\Character\`（6 个文件） |
| Event | 4 | `Core\Features\Event\` |
| Merchant | 14 | `Core\Features\Merchant\` |
| Monster | 1 | `Core\Features\Monster\` |
| StatefulCard | 11 | `Core\Features\StatefulCard\` |

骨架四件：`FeatureIds.cs`（模块 ID 常量 + `All` 集合）、`FeaturePatchAttribute.cs`（类级标记）、`FeatureRegistry.cs`（分组安装 / 整组回滚）、`FeatureBootstrap.cs`（`InstallGroups`、`IsManagedByFeature`）。

给补丁类打归属就是在类上加一行：

```csharp
[FeaturePatch(FeatureIds.Merchant)]
```

**目前是「启动即全部安装，只是分组隔离」**，不是按需激活。要做成按需激活（像维修版那样只在装了对应包时才装），需要给每个模块实现 `ShouldBeActive()`/`IsInUse()` 并在 `FeatureRegistry` 里加卸载路径——这是尚未做的设计决定，先问作者。

### 5.3 常驻的 92 个（不要随意改）

入口（`Entry.cs`）、联机（`Core\MultiplayerSkinSync.cs`、`Core\MultiplayerRoomAppearancePatches.cs`）、跨模块混装（`Core\FrameworkSkinRuntimePatches.cs` 同时管卡池/角色/遗物/球体）、面板共用（`Ui\ContextualSkinControls.cs`）、六个手装兼容层、退出与预加载（`Core\QuitCacheCleanup.cs`、`Core\SkinPreloader.cs`、`Core\ManagedSkinModLoader.cs`）。

### 5.4 其它关键目录

`Catalog\`（皮肤包扫描/契约判定）、`Ui\`（界面与运行时外观）、`Core\`（服务、存储、兼容层）、`Pck\`、`Data\`（内置清单与语言表）、`compat\`（第三方管理器兼容）、`tests\`（6 个测试项目）、`tools\`（工坊清单导出、发布器）。

---

## 6. 最近三次改动的机制（读代码前先懂这三处）

### 6.1 八模块分区（`058c5d6`）

`FeatureBootstrap.InstallGroups` 先按 `[FeaturePatch]` 分组安装，某组失败就整组回滚、只禁用该模块，其余模块与 92 个常驻补丁不受影响；`InstallPatchClasses` 跳过已登记类避免重复装。
`FeatureRegistryTests` 三条断言（正常装、失败整组回滚且不误卸别的模块、重复装抛异常）守着这条隔离。

### 6.2 皮肤缓存无上限（`106b643`）

症状：缓存目录涨到 63 GB，退出日志「退出缓存清理：已删除 0 个文件」。
根因：`Core\SkinChangerStorage.cs` 的 `SafePath(path, root)` 把 **root 自身**也算进 reparse-point 判定，而用户目录是 junction → 恒 false → `StartCacheSession` 抛 `Unsafe SC cache directory`、`CleanSessionRoot` 直接 return、`TrimCacheBudget` 整轮被过滤掉，8 GB 预算从未生效。
修法：① 根目录不参与 reparse 判定（先 `break` 再判）；② 裁剪按 `LastWriteTimeUtc` 最旧优先；③ 新增 `MeasureSessionBytes`，单个会话量算失败不再中断整轮。
回归测试 `tests\STS2SkinChanger.LogicTests\SkinCacheStorageTests.cs`（建真 junction、验最旧先删）。

### 6.3 退出看门狗（`fd0d344`）

原来 12 秒强杀计时从 `Prefix` 就开始，可能抢在原版保存前杀进程 → 丢进度。改成 `Postfix(bool __runOriginal)` 里、原版 `Quit` 返回后才计时。

### 6.4 骨架包交回原加载器

`Catalog\SkinCatalog.cs` 的 `AppliesSkeletonThroughCharacterModel` 用 `PEReader` 匹配游戏程序集里是否引用 `set_BodySkeletonDataPath` / `set_NonCombatBodySkeletonPaths`，命中就把骨架交回游戏原加载器，不再自己替换（修 CznCore 系列显示原皮）。1.0.32.5 起，能明确读取 CharacterSkinProfile 身体声明的包改由统一兼容桥接管；未知声明仍保留此交回规则。

---

## 7. 已知坑（都踩过）

1. `bin\Release\` 里**没有** `SkinChanger.json`（csproj 不产出），部署时要单独从 `STS2SkinChanger\SkinChanger.json` 复制；用 `-ErrorAction SilentlyContinue` 会静默漏掉。
2. `Directory.Build.props` 默认 `GameAssemblyDir` 指向 `D:/Programs/Steam/...`，本机不存在 → 每条 `dotnet` 命令都要带 `-p:GameAssemblyDir=...`，LogicTests 也不例外（否则报一堆 `CS0246 ModelId/Texture2D`）。
3. 测试 `Program.cs` 的调用块**行首无缩进**（如第 2055 行 `FeatureRegistryTests.Run();`），复制粘贴带前导空格会改不进去。
4. 测试项目靠默认 glob 收 .cs，手工加 `<Compile Include>` 会 `NETSDK1022 重复项`。
5. 用备份覆盖源文件后 `Copy-Item` 保留旧时间戳，MSBuild 会跳过重编、跑的还是旧二进制。还原后补：`(Get-Item $f).LastWriteTime = Get-Date`。
6. `PowerShell` 里 `Select-String -Path 'dir\**\*.cs'` **不递归**，统计会少算；要用 `Get-ChildItem -Recurse -Filter *.cs | Select-String`。
7. 提交信息：PowerShell 没有 heredoc，写临时文件 + `git commit -F`（UTF-8 无 BOM）。
8. 直连 GitHub push 被重置，必须走 `http.proxy=http://127.0.0.1:7890`。
9. 版本号四处漏一处就不一致——改版本时先搜 `1.0.32`：`git grep -n "1\.0\.32\."`。

---

## 8. 当前状态 / 待办

- 版本 `1.0.32.7` 已构建、离线测试、部署四处；按作者要求提交并推送 origin/main，不上传工坊。上一版大卡问题作者已实机确认修复；CZN 兼容与闪退防护的画面表现尚未真机核验。
- 最近提交（新→旧）：`106b643` 缓存无上限修复 → `058c5d6` 版本统一 + 八模块分区 → `f044092` 合入维修版 1.0.32.1 → `fd0d344` 看门狗热修。
- 待作者真机确认：进游戏看日志有无「退出缓存清理：已删除 N 个文件」或「皮肤缓存超出 8 GB 预算」。
- 待决定：八个模块是否改按需激活（见 §5.2）。
- 已搁置：卡面下拉「打开即变默认」（`Ui\CardSkinControls.cs` 已回退到 1.0.31.99，`a08b5c0`；此前查明是引擎程序化搬焦点导致 `PopupMenu.IdFocused` 收到 0→1→2→3→1，再动前先找可用事件源，别直接改 `IdFocused`）。
- 待真机验证：商人读档后的刷新。

---

## 9. 协作规则

- **不要启动游戏做实机测试**。作者本人进游戏验证；你的交付靠离线测试 + 反编译核对 + 静态审查，覆盖不到的画面类风险要如实说明。
- 已确认原因的错误直接修并验证，不用中途反复请示；涉及新的设计取舍或需要额外权限时先说。
- 只改与任务相关的文件，不做无关重构；`AGENTS.md` 的规则优先于本文档的重复描述。
- 回答简短：结论、当前状态、需要作者做什么。

- 版本 1.0.32.8：修复商店局内角色被 CZN 外来皮肤覆盖，双重检查事件/呈现归属，核对战斗与火堆边界；作者确认上一版商人正常。本版离线回归通过，未实机核验；三文件同步四处，提交并推送 main/标签，不上传工坊。

- 版本 1.0.32.9：修复首次进新商店旧缓存商人套用当前皮肤位姿的问题；Ready 后一次性重建独立商人外观，保留库存/购买状态。新增 200 房间去重及生命周期回归通过，未实机验证；三文件同步四处并提交推送 main/标签，不上传工坊。

- 版本 1.0.32.10：作者确认上版首次进店问题已解决；补注册 CrusaderSkin 新版自定义钩子模型，修复只有选牌前摇而缺正式出招，并按玩家皮肤限制常驻订阅。真实 DLL/逻辑/CZN/商店离线回归通过，未实机核验；三文件同步四处，提交并推送 main/标签，不上传工坊。

- 版本 1.0.32.11：十字军/贞德相关 12 模块移入 STS2SkinChanger/ProtectedCharacters，后续须遵守该目录 AGENTS.md 并运行 tools/check-protected-characters.ps1。加入解码/缓存预算、低内存/缺失失败冷却、有界遍历和选角文件预读；结束对局不删除已挂载联机 PCK；开局/退出兜底保持。真实 13 选项及专项/既有回归通过，未宣称消除所有原生崩溃；三文件同步四处并推送 main/标签。

- 版本 1.0.32.12：选项变体优先，同源外观标识与选人失败回滚；角色预览始终启用、奥斯提仅骨妹显示；完整依赖图隔离复用；空闲分片文件预读、需求插队、输入暂停，保留开局/退出兜底。详细边界及离线验证见本版本问题记录。

- 1.0.32.13：修复可战损贞德变体重建后背景调色/点击绑定；隔离加载三档并共用稳定图，消费一次点击，保留 WearEnabled 与 SFW 无战损边界。

- 1.0.32.14：定位 .12 规范缓存接管导致旧背景资源路径被清空；保护被替换者及私有别名占用者的路径。新增测试在 .13 DLL 明确失败、修复版通过 200 次替换与临时恢复，纳入战士保护必跑脚本。

- 1.0.32.15：采纳玩家瓦库散图背景补丁，保留 Image 并添加供官方回调写图的隐藏 TextureRect 空槽；防缺失节点中断与彩蛋覆盖。旧 DLL 负对照失败、修复版与先古卡牌专项通过；从当前源码打包，不部署玩家旧注入 DLL。

- 正式版 1.0.33（内部 1.0.33.0）已上传原工坊条目 3787302680，15 语言逐一回读成功；汇总至 .32.15 的修复，更新 GitHub 链接及说明，保留 12 张预览图。发布详情见 RELEASE_2026-11-16_workshop_1.0.33.md。

- 1.0.33.1：修复可战损贞德第二/三档遗漏调色页而退回原色，保护区 StagePalettePages 按所选包补齐并继承当前权重；四变体 24 次档位循环和真实三档调色资源纳入保护回归。

- 1.0.33.2：恢复瓦库原图默认预览，新增仅瓦库可选的官方彩蛋，沿用默认菱形/黄色样式；选择可保存，分类不自动选中彩蛋，外部散图及原生进度保留。

2026-11-17 内测 1.0.33.3：修复雪霁改一代涅奥替换商人漏识别，接入私有 Spine/商店房间回调，详见 [问题记录](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-17_private_neow_merchant_discovery.md)。公开版仍为 1.0.33。

- 1.0.33.4：修复私有商人选中仍显示原版：真实作者骨架路径匹配器适配切换器隔离身份；同时拦住新版具体商店的预览游戏初始化，保留真实商店 Ready，详见对应问题记录。
