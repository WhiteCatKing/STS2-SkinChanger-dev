# 给下一个 AI 的通用交接说明（2026-11-12，项目 1.0.30.200）

和你对话的是本 Mod 的作者：直接执行，不要反复确认；只有缺关键信息时才问。

## 项目一句话

《杀戮尖塔 2》的皮肤管理 Mod「皮肤切换器 / Gurio.SkinChanger」：统一接管角色、随从（奥斯提）、怪物、先古、事件、卡牌、商人、战斗背景的外观，提供选择/组合/预设/优先级、局内外观面板、联机皮肤同步和游戏内创意工坊；不改玩法。

## 新对话 5 分钟上手

按顺序做完这 6 步，就具备动手条件了，不要通读源码：

1. **读本文件**（就这一份，133 行变 190 行，是唯一的总入口）。
2. `cd 'D:\Storage\Desktop\工作2'; git log --oneline -5; git status --short` —— 看清"上一次存档是什么、现在有没有没提交的改动"。有未提交改动时先问作者是不是他上一轮允许但没提交的，别直接丢。
3. 打开 `CHANGELOG.md` 看**最上面两节** —— 这就是最近两次改了什么、为什么改。
4. 核对版本号 4 处是否一致（`Entry.cs`、`.csproj`、`SkinChanger.json`、`workshop/workshop.json`），并看一眼 `bin/Release/Gurio.SkinChanger.dll` 的修改时间是否晚于源码。
5. 要动某个功能，按文末「阅读指引」只打开对应文件；动手前先 `Stop-Process -Name SlayTheSpire2 -Force`。
6. 改完 → 构建 → `LogicTests` → 部署 4 处 → 向作者汇报（**不要自己提交**，见下条）。

**提交规则（作者 2026-11-12 明确要求，必须遵守）**

- **只有作者明确说「提交 / 存档 / 存到 git / 记一个版本」时才 commit。** 平时改完、构建完、部署完就停，报告作者即可；否则版本会碎成几十条，作者没法看。
- 一次提交 = 一个可交付的版本，**不要一个功能一个提交**，也不要把"试错—回退"的中间态提进去。
- 提交信息用中文，标题带内部版本号，正文写清三件事：**根因 / 改法 / 验证**（哪几行日志、哪张截图、哪个配置项）。写不出来说明还没改完。
- 同一次提交打一个附注标签 `v<内部版本号>`，并在 `CHANGELOG.md` 顶部补一节（格式照抄现有条目）。
- 仓库**没有远端**，不涉及 push；上传工坊是另一套流程（见「更新工坊」），两件事不要混。
- 不要用 `git reset --hard`、不要 `--amend` 已提交的内容：历史是作者的回退依据。

## 运行与构建

**构建**（先 `Stop-Process -Name SlayTheSpire2 -Force`，DLL 被占用会复制失败）

- 发布：`dotnet build STS2SkinChanger/STS2SkinChanger.csproj -c Release --no-incremental -p:GameAssemblyDir="<repo>/tests/STS2SkinChanger.RuntimeTests/bin/Release/net9.0"`。必须传 `GameAssemblyDir`，否则引用不到 `sts2.dll`（`Directory.Build.props` 里的默认路径可能过期，一律显式传）；发布产物必须来自 Release、保持 AnyCPU，同一份 DLL 同时兼容正式版与测试版。
- 测试版验证单独构建，避免覆盖发布产物：`-c ReleaseBeta -p:Optimize=true -p:GameAssemblyDir="D:\steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64"`。
- .NET 的“当前目录”不跟随 PowerShell 的 `cd`：要么每条命令都用绝对路径，要么先 `cd` 再在同一行执行。

**测试**（都不启动游戏）

- 必跑：`dotnet run --project tests/STS2SkinChanger.LogicTests/STS2SkinChanger.LogicTests.csproj -c Release -p:GameAssemblyDir="<同上>"`，必须全绿（末行 `Skin Changer logic policy tests passed.`）。
- 回归：`tests/STS2SkinChanger.RuntimeTests`、`STS2SkinChanger.FrameworkTests`。

**产物与部署**

- 三件：`STS2SkinChanger/bin/Release/Gurio.SkinChanger.dll`、同目录 `thunninoiSkinManager.dll`、`STS2SkinChanger/SkinChanger.json`。
- 复制到 4 处：`STS2皮肤切换器_mod测试版安装包`、`..._mod正式版安装包`、`STS2_Work\workshop_content_1.0.20`、`D:\steam\steamapps\common\Slay the Spire 2\mods\STS2皮肤切换器_mod测试版安装包`（**游戏真正加载的是最后这份**）。
- 启动：Steam 已运行时直接跑 `D:\steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.exe`；依赖 .NET 9 SDK（`global.json`）、游戏程序集、Steam。

**常用路径**：配置 `%APPDATA%\SlayTheSpire2\Gurio.SkinChanger\skin_changer.json`；日志 `%APPDATA%\SlayTheSpire2\logs\godot.log`；覆盖包缓存 `...\Gurio.SkinChanger\overlay-cache`（上限 3 GB，可删，启动重建）；测试版程序集 `D:\steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64`；工坊条目 <https://steamcommunity.com/sharedfiles/filedetails/?id=3787302680>。

## 版本、Git 与更新日志

**版本号**

- 公开版本 `SkinChanger.json` 用三段；内部四段同步 4 处：`STS2SkinChanger/Entry.cs`、`STS2SkinChanger/STS2SkinChanger.csproj`（4 个字段）、`STS2SkinChanger/SkinChanger.json`、`workshop/workshop.json`。四个文件都是 UTF-8 **无 BOM**（用 `[System.IO.File]::WriteAllText` + `UTF8Encoding($false)`）。
- 本地构建只加第四位；真正上传工坊时才提升第三位并同步三段公开版本。

**Git 仓库**（2026-11-12 建立）

- 仓库根是 `工作2`（`git init -b main`，提交 `79d03c6` 初始化，`.git` 约 11.5 MB，pack 9.81 MiB）。
- `.gitignore` 排除全部二进制与图片：`*.dll/*.pdb/*.exe/*.pck/*.zip`、`**/bin/`、`**/obj/`、`*.bak*`、`*.png/*.jpg/...`、四个部署包目录、`*.log/*.cache`。⇒ **仓库里没有可运行产物**，回退代码后必须重建 + 重新部署 4 处，游戏里才会变。
- 只配了仓库级身份（`Gurio <gurio@localhost>`）与 `core.quotepath false`，没有改全局 git 配置。
- 提交信息用中文，标题带内部版本号；每个交付版本打一个附注标签（首个：`v1.0.30.200`）。

**每次交付的动作（按顺序）**

改代码 → 构建（0 错误，只允许既有的 3 个 CS8601 警告）→ `LogicTests` 全绿 → 部署 4 处并回读 `internal_test_version` + DLL 哈希 → **向作者汇报（停在这里等指令）**。

作者说"提交"之后才继续：`git add -A` → 中文提交（标题带版本号，正文写根因/改法/验证）→ `git tag -a v<版本> -m "<一句话说明>"` → `CHANGELOG.md` 顶部追加一节。

**CHANGELOG**

- `工作2\CHANGELOG.md`：按日期倒序记录每一版，含根因、改法（文件/函数）、已知 DLL 前缀、回退命令。已回溯到 **1.0.27.78**（2026-11-07 起），来源是各天的长交接文档。
- 2026-11-12 之前的版本早于仓库建立，**只能靠 CHANGELOG 追溯**，打标签也指不到当时的代码。

**回退**

```powershell
cd 'D:\Storage\Desktop\工作2'
git log --oneline                     # 找版本
git revert <哈希>                     # 撤销某次提交（推荐，历史不断）
git checkout <哈希> -- <文件路径>      # 只恢复某个文件
git checkout v1.0.30.200              # 回看某标签（只读，改之前回 main）
```

**更新工坊**（前提：用户明确要求；只替换三个文件，别的一概不动）

1. 用测试版安装包的三件覆盖 `STS2皮肤切换器_mod正式版安装包`，再放进 `STS2_Work\workshop_content_1.0.20`（或上传工具的 `ContentFolder`）。
2. 用 `STS2_Work\workshop_update` 走 Steam API 提交（工具硬编码条目 `3787302680` 与暂存目录，运行前核对），或手动在工坊页面只更新内容；**更新说明留空**。
3. 不改标题、简介、图片、截图、标签、更新日志。
4. 上传后用 `workshop_query` 核对：标题不变、文件大小等于三件之和、更新时间已变。上传前确认 Steam 已登录且条目属于作者。

**工坊更新说明**：先去工坊条目点「改动说明」看历代格式；风格先英文后 `/ 中文`、简洁分段；除非用户明确要求，不写进工坊页面。仓库内的 `CHANGELOG.md` 是开发视角的记录，与工坊文案不是一回事。

## 目录地图

上级目录 `C:\Users\Administrator\Desktop\工作2`（同 `D:\Storage\Desktop\工作2`），**已是 git 仓库**：

| 路径 | 是什么 | 规则 |
| --- | --- | --- |
| `STS2-SkinChanger_源文件（坚决不能动）` | 原始源码，只读参考 | **任何情况下都不修改** |
| `STS2-SkinChanger_备份件` | 修改件的完整备份 | 不在这里开发，只在回滚时用 |
| `STS2-SkinChanger_修改件` | 唯一工作源码，构建/测试/改文档都在这里 | 只改这里 |
| `STS2皮肤切换器_mod测试版安装包` | 三个运行时文件，做测试部署 | 测试用，不入库 |
| `STS2皮肤切换器_mod正式版安装包` | 三个运行时文件，更新工坊用 | 发布用，不入库 |
| `STS2_Work` | 逆向、工坊上传、日志等临时工作区 | 不参与运行、不打包（802 MB） |
| `CHANGELOG.md` | 全部版本的更新日志 | 每交付一版在顶部追加 |
| `.gitignore` | 排除二进制/图片/部署包 | 别为了让文件入库而放宽它 |
| `未解决问题清单.txt` | 给作者看的未解决问题 | 编号 + 一句问题，不写反馈者/链接/过程 |

文本交接文档（历史长文档）已移到 `C:\Users\Administrator\Desktop\新建文件夹`（即 `D:\Storage\Desktop\新建文件夹`），不进仓库：`给下一个AI的交接说明_2026-11-07 / -11-09 / -11-10 / -11-11 / -11-12.md`、`给下一个AI的通用交接说明.md`（本文件）。

`STS2-SkinChanger_修改件` 内部：

| 路径 | 职责 |
| --- | --- |
| `STS2SkinChanger/` | Mod 主工程：`Entry.cs`、`Core/`（210 个文件）、`Catalog/`（28）、`Ui/`（85）、`Pck/`、`Data/`、`SkinChanger.json`、`.csproj` |
| `compat/ThunninoiSkinManagerCompat/` | 兼容层，产出 `thunninoiSkinManager.dll` |
| `tests/` | LogicTests（纯策略，必须全绿）、RuntimeTests、FrameworkTests、各 Fixture |
| `tools/CatalogInspect` | 离线目录/覆盖包检查（`--visual-overlay`、`--bench-overlay`、`--baseline-probe`、`--force-gameplay`），不随 Mod 发布 |
| `tools/PckInspect`、`WorkshopPublisher` 等 | PCK 查看、工坊发布等一次性工具 |
| `workshop/workshop.json` | 工坊元数据：标题、描述、版本、更新说明 |
| `docs/` | 问题单 `issues/`、已修总表、反馈汇总、GitHub 发布文案；详见“处理反馈” |
| `README.md` / `AGENTS.md` / `PRODUCT.md` | 完整手册 / 开发约定（旧路径可能过期，以本文件为准）/ 产品边界 |
| `*.bak_*`、`_backup/` | 历史备份，不用读也不用改 |

`STS2_Work` 内部：`src`/`beta_src`（正式/测试版源码快照）、`betacompat`（测试版产物）、`deployed_check`（已部署 DLL 反编译核对）、`diffs`（版本差异）、`dump`/`game_types`/`stable_types`/`new_decomp`/`old_decomp`（反编译与 API 导出）、`tools`（`ilspycmd`）、`workshop_content_1.0.20`（上传暂存）、`workshop_update`、`workshop_query`；根目录脚本与日志都是临时分析，不是发布物。

**铁律**：源文件绝不改；备份件不用于开发；一切改动、配置、文档都在修改件里；不改其他 Mod（如 MLM、Orchis、midori）的文件/DLL/清单/PCK，兼容问题只在本 Mod 内解决。

## 关键入口与全局变量

| 名称 | 位置 | 作用 |
| --- | --- | --- |
| `[ModInitializer]` 入口 | `STS2SkinChanger/Entry.cs` | 初始化、版本号、补丁装载 |
| `SkinService.MountOverlay(groups)` | `Core/SkinService.cs` | 所有换肤/场景切换的统一重建入口（含重入合并与全量 reassert） |
| `SkinService.Catalog` / `SkinService.Config` | 同上 | 全局单例：皮肤目录 + 玩家配置；UI 都读它 |
| `SkinService.Sync` | 同上 | 所有公开 API 都在这把锁里，跨线程只走公开方法 |
| `SkinService.MountedOverlayOwners` | 同上 | 分层覆盖包“每个路径当前由谁提供”，**增量挂载的唯一依据** |
| `SkinService._runtimeProviderBehaviorScope` | 同上 | 当前场景可见分组，决定哪些皮肤 DLL 处于激活状态 |
| `GetActiveRuntimeProviders(catalog)` | 同上 | 决定"谁能装补丁"；会剔除只在随从分组被选中的提供者（防串皮） |
| `ProviderActivationSchedulePolicy` / `DeferredProviderActivator` | `Core/` 两个文件 | 启动只同步激活角色/随从，其余进后台队列 |
| `PerfTrace.Enabled` / `WarnMilliseconds` | `Core/PerfTrace.cs` | `[性能]` 日志开关与慢阶段阈值 |
| 环境变量 | — | `STS2SC_NO_DEFER=1` 关掉启动延后（排查用）；`STS2SC_DEBUG=1` 打开引擎内快照/文件驱动换肤/坐标监视（见“调试设施”）；`CATALOG_INSPECT_*` 只影响离线工具 |

配置关键项：`Selections`（分组→选项）、`CompanionSelectionDecoupled`、`CompanionImpostorSkinId`、`CharacterCombatTransforms`（每皮肤的面板变换，自动化点过面板后务必核对）。

## 当前状态

**已完成（1.0.30.200，2026-11-12）**

- **退出重进皮肤被重置**：启动清理用严格完整性判定，`necrobinder=某提供者` + `osty=另一个提供者` 时角色提供者被判"不完整"→ 卸载补丁并改配置（日志 `已将 … 的不完整联动选择恢复为游戏原版`）。改为 `Catalog.IsFullRuntimeProviderSelectedIgnoringCompanions(...)`（随从分组有自己的挂载事务，不参与角色判定）。
- **骨妹被串成 Orchis 的真因**：不是我们的代码，是工坊 `3759170855`（STS2-OrchisNecrobinderSkinFix）的 manifest 声明依赖 `OrchisNecrobinderSkinMod`，触发"被依赖则交回原加载器"的安全阀，Orchis 全局 `PatchAll`（`Creature.CreateVisuals` 后置无条件换骨）。**该目录必须保持 `.disabled`**；Steam 会在更新时把它重新下载回来（`3759170855` 复活过两次）。
- **Orchis 本体头顶蓝火（局内）**：节点 `Visuals/HeadBoneNode/SteppedFireMix_dark`；根因是 `ApplySelectedVisualPostfix` 的 `allowed` 要求"完整运行时提供者全组选中"，`osty` 分给「撑在地上的鸡煲」后 Orchis 被判不完整，骨妹分组的后处理不回放。修法：`allowed` 只看本组 + `TryRebuildCreatureVisuals` 挂载后重放一次。
- **奥斯提坐标被带走**：Orchis 的 `NCreature.OstyScaleToSize` Prefix 会 tween 宠物根节点；用 `MonsterVisualCallback.OstyPositioning` + `ProviderOwnsOstyPet` 隔离，宠物组自带提供者时不再重放主人的 `NCreature._Ready`。
- **「坚果」名字**：`CompanionDisplayNameGuard` 挂 `LocString.GetFormattedText`，只处理 `OSTY.name`，非 midori 时从游戏原包 `res://localization/<lang>/monsters.json` 读回「奥斯提」。
- **鸡煲显示原皮**：别名场景只用于 `PreloadManager.Cache.SetAsset` 登记到规范路径，身体仍走 `CreateVisuals`，独立 Mod 才能接管。
- **联机**：`ReconcileRemoteAppearances` 按选择签名去重（省掉 8 秒一次的重复全量重建）；"整包已在栈顶就跳过提权"这个优化是**错的**（会导致分层覆盖层把整包反压下去、VFX 变原版），已回退。

**已完成（1.0.30.187–.201 期间，数字见长交接）**：midori 的奥斯提进预览框（预览新增“场景型”分支）、依赖闭包允许带空格的路径（600 → 648 文件）、校正包按游戏原包还原 + 选中项覆盖后再按归属分组还原 + 挂载后按包栈刷新 `PreloadManager.Cache`、启动批量激活拆分（6.8 s → 3.45 s）、覆盖包分层增量（470–900 ms/630 MB → 49–171 ms、多数 0 写盘）。

**已知坑 / 待办**

1. **选角界面预览小人仍带 Orchis 头顶蓝火**（局内已修）：预览走 `FrameworkModelPreview` 的私有工厂，不走 `Creature.CreateVisuals`；`SkinService.ApplySelectedVisualPostfix` 在预览路径下探针计数为 0（入口没被调用）。两版"预览期间临时激活提供者"均无效，已回退。下一步先确认 `FrameworkModelPreview.Refresh` 是否执行、`player.Creature` 是否为 null。
2. **选角界面偶发卡死**（`Responding=False`、内存 2.1 GB、连引擎内截图都拿不到）：日志尾部常见第三方 `AncientWaifus` 资源缺失（`biiig_hug_outline.png`）；`char_select_random_character.png` 的刷屏报错已用 `ResourceLoader.Exists` 预检缓解，卡死未定位。
3. **硬守卫待作者批准**：Steam 重新下载 `3759170855` 后如何仍不串皮（思路：检测到本该隔离的提供者被依赖强行全局加载时，摘掉它对玩家本体的影响）。
4. 局内把奥斯提切到 `midori_black`：`catimage/**` 帧图不在运行时包里 → 刷 `Parse Error` 且外观不变（根因见长交接 14.2）。
5. 偶发黑屏卡在 `Preloading 'characters=…'`（6 次里 2 次），杀进程重开即恢复，未定位。
6. 每次进游戏第一次装配仍是整份 630–830 MB / ~500 ms；要压掉得改“整包提权”策略。
7. `necrobinderHpWear` 会刷 atlas 尺寸警告；`card_atlas.sprites/.../ascenders_bane.tres` 偶发 `resource_format_binary` 警告——都不影响观感。

## 调试设施（默认关闭）

- 全部由**环境变量 `STS2SC_DEBUG=1`** 打开；正常启动不带它。
- `%TEMP%\sts2_snap.req` 写任意内容 → 引擎内截一张到 `%TEMP%\sts2ctl\snap.png`（Vulkan 下外部截图只有黑屏，必须从引擎内取视口纹理）。
- `%TEMP%\sts2_pick.txt` 写 `<分组>|<选项ID>`（如 `osty|OstyDefectImpostor.ground_defect`）→ 等价于点下拉换肤。
- `%TEMP%\sts2_watch.txt` 写秒数 → 每 ~80 ms 追加坐标样本到 `%TEMP%\sts2ctl\watch.log`。
- 外部输入/截图脚本在 `C:\Users\Administrator\Documents\deepseek-harness\default-workspace\sts2ctl\`：`click.ps1`（`-X -Y`）、`key.ps1`（`-Vk 0x1B` 发 ESC）、`focus.ps1`、`shot.ps1`（桌面 `CopyFromScreen`，老版本没有引擎内钩子时用它）。坐标换算：预览图 1708x961 → 实际 2560x1440，系数 1.4988。
- **Steam 崩溃弹窗是模态的**：`Steam错误! Steam应用已崩溃…`（约在屏幕中央，"了解了"按钮 ≈ `1491,959`）不点掉，整个界面既不 hover 也不响应点击——"点了没反应"先检查它。成因是 `Start-Process SlayTheSpire2.exe` 直接启动 + 每轮 `Stop-Process -Force` 强杀；用 `Start-Process 'steam://rungameid/2868840'` 或直接点掉即可。

## 阅读指引

| 要改什么 | 先看哪里 |
| --- | --- |
| 这一版改了什么 / 某行为是哪版引入的 | `CHANGELOG.md`；版本时间线与证据在 `新建文件夹\给下一个AI的交接说明_2026-11-12.md` |
| 目录/分组/选项识别、覆盖包内容 | `Catalog/SkinCatalog.cs`（近 1 万行，按方法名检索） |
| 覆盖包什么时候写、写什么、挂哪一层 | `Core/SkinService.cs` 的 `MountOverlayCore` / `MountOverlayDelta` / `MountArchiveOverlay` |
| 皮肤 DLL 的加载、激活、补丁隔离 | `Core/ManagedSkinModLoader.cs` |
| 局内外观面板、下拉、拖动 | `Ui/CharacterAppearanceScreen.cs`、`Ui/ContextualSkinControls.cs` |
| 换肤后模型重建/补丁回放 | `Ui/CharacterAppearanceRuntime.cs` |
| 选人界面预览（含预览蓝火） | `Ui/FrameworkModelPreview.cs`、`Ui/OstyModelPreviewControls.cs` |
| 启动激活拆分 | `Core/DeferredProviderActivator.cs` + `Core/ProviderActivationSchedulePolicy.cs` |
| 奥斯提/独立随从 Mod 兼容 | `Core/SummonModCompat.cs`、`OstyImpostorCompat.cs`、`OstyImpostorBridge.cs` |
| 联机同步 | `Core/MultiplayerSkinSync.cs` |
| 本地化名字 | `Core/CompanionDisplayNameGuard.cs` |
| 配置字段与迁移 | `Core/SkinConfig.cs` |

**处理反馈**：每个具体问题在 `docs/issues/` 单独一个 md，状态行固定写 `- 状态：**未解决**` 或 `- 状态：**已解决**`；已解决总表 `docs/COMPLETED_FIXED.md`；反馈汇总 `docs/WORKSHOP_FEEDBACK_2026-08-30.md`；功能请求 `docs/FEATURE_REQUESTS_PENDING.md`。来源是工坊评论/讨论区、作者邮箱等；收集后建 issue 或补记录 → 更新 `未解决问题清单.txt`（只写编号 + 一句问题）→ 标记已解决后去工坊删掉对应评论/讨论，避免后续重复记录。

**不用读**：`bin/`、`obj/`、`*.bak_*`、`_backup/`（构建产物与历史备份）、`Data/workshop-catalog.json`（生成物）、`docs/publish-github-*`（多语言发布文案）、第三方皮肤包本身的 DLL/PCK（在 `mods` 与 `steamapps\workshop` 里，禁止修改）。

**要细节时**：`新建文件夹` 下的长交接按日期读——`_2026-11-12.md`（本轮：Orchis 复活真因、退出重进、两处蓝火、git/CHANGELOG）、`_2026-11-11.md`（875 行，第 12–18 节是性能与奥斯提专题）、`_2026-11-10.md`、`_2026-11-09.md`；更早的散在 `D:\工作2026.11.7\`、`D:\工作2026.11.8a\`、`D:\工作2026.11.8b\`。不要通读源码。

---

新对话开始时只需读本文件；需要改具体功能时，再按“阅读指引”读取对应文件，不要全量读取项目。
