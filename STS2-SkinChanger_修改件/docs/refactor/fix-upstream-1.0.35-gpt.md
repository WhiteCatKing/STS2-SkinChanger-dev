# 1.0.35 缓存与磁盘保护修复

基于 4dbeae7，fix/1.0.35-stability。GPT，2026-11-17。

## 第一轮

统一增加 CacheWriteBudget，默认 8 GiB，合计自有 cache 与 overlay-cache。PckArchive 的两个 writer 在打开输出文件前调用同一个预算入口，按完整 PCK 布局预留。预算不足时按访问时间回收未受保护旧 PCK，保护本进程已生成或复用的文件及其他仍有活动 lease 的会话。写失败移除半成品并允许重试。保守保护范围包含已生成但尚未确认挂载成功的文件，以覆盖 writer 返回到原生 mount 之间的窗口。

Storage 持有预算对象，目录重载继续使用，prepared 与挂载累计计数也保留。SkinService 的原生挂载经 SkinChangerPaths 统一保护，OverlayCacheStore 的命中路径同步登记；原版 alias 的已有同 hash 文件直接复用。

改动文件为 Core/CacheWriteBudget.cs、SkinChangerStorage.cs、SkinChangerPaths.cs、SkinService.cs、OverlayCacheStore.cs、Pck/PckArchive.cs，以及 LogicTests 的新缓存测试、Program 和工程链接。

LogicTests 全套通过，末行 Skin Changer logic policy tests passed。新增用例覆盖跨目录旧文件回收、真实 PCK writer、预算不足不创建文件、受保护文件禁止覆盖、失败写入清理与重试、其他活动会话保护。测试用小预算与临时目录。

本组代码覆盖到临时 4dbeae7 快照后构建 0 警告、0 错误。该隔离输出的 RuntimeTests --test-switch-resource-lifetime 和 --test-runtime-safety 通过。共享修复仓库构建被 DSH 正在改的 CrashDiagnostics.cs:182 的 CS0136 阻塞，位置已在房间 #108 发给他。

第二轮继续补统一实际字节剩余空间检查、低空间拒绝测试和正常回退入口核对，并重跑共享构建。原生显示效果依靠源码与离线边界验证，本轮未启动游戏。

## 第二轮（完成）

统一 PCK writer 按实际 byte[] 长度或源归档索引大小，加目录、头部和 16 字节对齐的布局上界，检查目标盘剩余空间，额外保留 128 MiB。检查在输出打开前进行，普通 Write 和 WriteFromArchives 共用入口，预算目录外的在线缓存生成 PCK 也检查空间。删除了旧的文件数经验估算。自有 cache（含当前 session、prepared/runtime）与 overlay-cache 合计 8 GiB，目录外输出只检查磁盘空间。

预算与低空间拒绝用独立 IOException 子类，覆盖包 writer 及 MountOverlayCore 透传到现有选择事务，避免新选择保存成功但实际仍旧外观。选择事务恢复上次有效选择；视觉创建异常由现有 UI 保护保留原生结果，并非所有路径强制把配置改为 __base__。统一 writer 记录拒绝原因，外层原有事务诊断保留。

新增 LogicTests 覆盖大文件含头部拒绝、恰好空间边界、归档流式复制和预算目录外输出；新增 RuntimeTests/CacheWriteFallbackTests.cs 实际调用 MountArchiveOverlay，验证低空间不生成输出、不进入原生 mount、保持预算拒绝异常。由 RuntimeSafetyTests 调用，未改 DSH 的 Program.cs。

最终共享 RuntimeTests 项目连主 Mod 构建 0 警告 0 错误；LogicTests 构建 0 警告 0 错误且全套通过。--test-runtime-safety（含新回退检查）、--test-switch-resource-lifetime 均通过。最终输出 %TEMP%/sts2-fix-1.0.35-runtime-final，日志 sts2-fix-budget-runtime-final-build.log；逻辑测试日志 sts2-fix-budget-logic-build.log、sts2-fix-budget-logic-run.log。首次回退断言误查包装函数，改查 MountOverlayCore 后通过；旧失败进程占用 DLL，已终止并换独立输出重建通过。DSH 已修好第一轮编译阻塞。

已检查本组 git diff 与 diff --check。完整 RuntimeTests 本轮未跑；DSH #110/#111 报告完整套件有基线同样的 PartTakeoverTests 失败，此处不计为通过。未启动游戏，原生显示效果待后续集成验证。

约束：已生成但未确认挂载的 PCK 保守保护到进程结束，覆盖写入到原生 mount 的窗口，因此预算耗尽可能需要重启。已挂载文件不删不覆盖，重载不清零账本及累计计数。8 GiB 是自有缓存磁盘预算，不是 Godot 内存预算。未 commit、未部署、未改版本号和其他 Mod。
## RuntimeTests 跑通

接 #113 / #119。完整无参数 RuntimeTests 已 exit 0，末尾运行到 Workshop policy tests passed；LogicTests 全套 exit 0。构建连主 Mod 为 0 警告、0 错误。未跳过任何原有测试组。每次测试进程启动前均在父 PowerShell 调用 SetErrorMode(3)，无调试器弹窗阻塞。

### 本轮逐项判断

| 位置 | 分类 | 原因与处理 |
| --- | --- | --- |
| AppearanceControlContractTests | 测试过时 | CreateCreatureAppearanceBinding 与 CanSelectCompanionSkin 明确允许已知奥斯提独立选皮肤。更新可选断言，保留模型/血条控制、随从无怪物意图和怪物缩放的检查。 |
| OtherCompendiumUiTests | 测试过时 | 地区来源已用 ActCatalog.All 纳入额外 Mod 地区，不再直接调用 ModelDb.Acts。改查真实 UI→ActCatalog，并追加注册模型枚举与原版 Acts 回退的调用断言，保留共享事件与刷新接入检查。 |
| EventSkinTests 日志 | 测试隔离过时 | 新目录构建日志进入 Godot Logger/OS，脱离引擎会原生崩溃。仅本组期间安装既有 AuditLogSilencer，finally 清理；测试主体与原生产逻辑仍执行。 |
| EventSkinTests 贴图夹具 | 测试夹具过时 | 原单字节 ctex 现在被 RuntimeLoadSafety 正确拒绝。夹具改为具有 GST2、1×1 尺寸的头部和区别来源的末尾标记；仍逐字节检查 skin→base→skin 的正确数据。这里只供离线头部估算和字节复制，不宣称是可原生解码的完整贴图。 |
| Catalog/SkinCatalog.Events.cs / EventSkinTests 文本契约 | 测试过时（#123 更正） | 维护者有意让纯文本事件包未选中也保留文字；选中来源已定义的键优先，缺键继续回退其他事件包，再到基线。撤回先前“真 bug”判断和生产改动，恢复维护者原实现；测试对齐这项产品行为，并增加独立纯文本包与另一个皮肤同时安装时的优先级检查。 |
| SceneAppearanceLifecycleTests | 测试过时 | 商人已从静态工厂两结果改为实际房间 Ready。更新为一次 Record，并增加 Harmony 目标为 NMerchantRoom._Ready、实例参数登记的检查，所有旧房间不能释放新房间等真实状态机测试保留。 |
| Program.cs 联机握手 | 测试过时 | 旧双开关已作为迁移输入；MultiplayerSkinSyncParticipationPolicy 明确所有联机显示模式均读写能力标记。更新为 0/1/2 三模式的真实 72 bit、协议 9、GSCAP09! 往返检查，额外检查空包不能冒充能力。远端旧缓存断言说明对齐实际离线语义。 |
| CardPresentationLifecycleTests | 测试隔离过时 | 新增 SendPresentationBehindCardVfx 进入 Godot NodePath 原生构造。按本文件已有隔离方式跳过这个纯绘图/树排序叶函数；实际 TrackAddedPresentationNode、重复刷新、基线恢复、外来节点所有权测试仍执行。 |

Claude 在本轮前完成的两项保持原样：商人镜像组选项探测真 bug（SkinCatalog.CountProbeVisualGroups），MultiplayerAppearanceIdentityTests 的 CreatureIntentDepth.Tick 引擎隔离。它们已随完整套件经过，本轮不重复归为 GPT 修复。

### 验证证据

输出目录 `%TEMP%\sts2-runtime-green-gpt`；构建日志 `%TEMP%\sts2-runtime-green-build.log`；完整无参执行日志 `%TEMP%\sts2-runtime-green-run.log`；逻辑套件日志 `%TEMP%\sts2-runtime-green-logic.log`。最后一次完整套件和逻辑套件均 exit 0，构建 0/0。使用显式本机 GameAssemblyDir，未改版本号、未 commit、未部署或启动游戏。检查本轮 git diff 与 diff --check，源码 LF。

范围说明：这里的“完整”指任务要求的不带参数入口全部执行完毕；需要外部包路径的专项参数不属于本次无参运行。按 #123 确认，先前事件生产改动已撤回，事件文字行为保持维护者设计。当前无剩余失败项。

### #123 更正与 1.0.36 重验

目录仍是 `上游修复_1.0.35`，实际 HEAD 已为维护者 1.0.36 `aeefc93`，分支 `fix/1.0.36-stability`。Claude 确认事件文字回退是产品取舍，先前以 README 通用恢复说明推断其为缺陷不成立。本次已逐字恢复 TryResolveEventText 的维护者注释和回退循环，`git diff HEAD -- STS2-SkinChanger_修改件/STS2SkinChanger/Catalog/SkinCatalog.Events.cs` 无输出；没有操作暂存区。

文本测试现在验证：未选、明确 base 仍保留事件包文字；同包中另一事件及假商人事件也按原回退解析；实际 LocTable 挂钩保留这项行为；事件包不拥有的玩法文本键继续交原生语言表。另建独立 text-only.pck 与另一个带图皮肤同时安装，验证未选纯文本正文生效、选中另一个皮肤的同名标题优先、所选皮肤缺少的正文键仍从纯文本包回退。图片与新增 VFX 的选择/回原版检查继续保留。

显式本机 GameAssemblyDir，临时输出 `%TEMP%\sts2-event-fallback-1.0.36-gpt`。构建 0 警告 0 错误；`--test-event-skins` exit 0；完整无参数 RuntimeTests exit 0，运行到 Workshop policy tests passed。运行前均 SetErrorMode(3)。日志分别为 `%TEMP%\sts2-event-fallback-1.0.36-build.log`、`sts2-event-fallback-1.0.36-events.log`、`sts2-event-fallback-1.0.36-full.log`。本轮只恢复该生产文件、更新对应测试与记录，未 commit、未改版本号、未部署。

## #130 缓存拒绝写入界面提示

已完成，详见 cache-write-notice-gpt-20261118.md。15 语言、外观状态栏、手动下拉失败提示、每局按原因一次的自动提示均接入；主工程/RuntimeTests 构建 0/0，通知专项、完整 RuntimeTests、LogicTests 通过。未部署。

## #10 / #9 闪退修复

### A · #10 完整皮肤包与校正层

在主修复分支 fix/1.0.36-stability（aeefc93）上，完整 PCK 成功置顶后，MountedOverlayOwners 逐路径记录实际归档和源资源。归档路径比较忽略大小写，遇到不同写法时用 GetFullPath 对齐，内联条目标识仍按原规则比较。相同来源和路径会跳过重复写入，指向其他归档或映射路径的条目继续参与增量校正。保留已有覆盖文件表和场景刷新入口。

--test-large-provider-overlay 使用含 1,024 条路径的合成 PCK，通过真实置顶记录、增量计算、签名和 writer，模拟 Godot 栈顶行为。实际校正包有 24 条，切去另一皮肤再切回时以相同签名复用已有校正包，选择原版时恢复共享路径。涉及其他来源的实际校正集合变化时仍可形成新的签名。另移除 Catalog/SkinCatalog.cs 遗物归属实现后重复执行的旧块，恢复主工程 0 警告、0 错误，遗物归属专项通过。

LogicTests 与完整 RuntimeTests 均 exit 0。构建及测试日志在 %TEMP% 下的 sts2-crash-a-*.log。真实 CZN 包的画面与进程是否仍闪退需实机确认，离线测试验证的是校正包内容与复用。

### B · #9 隔离卡图会话缓存

CardPortraitSessionCache 按隔离资源路径持有已加载的贴图，并复用需要的 AtlasTexture 视图。直接解码的原始图片也按来源和路径保留。64 项 LRU 继续服务请求和提供者标识查询。淘汰条目或普通分组选择变更只清请求索引，已有隔离贴图可由下次请求复用。保留原有 Ignore / IgnoreDeep 依赖隔离方式。

目录重建、实际对局清理后和游戏进入原生退出流程前，统一重置卡图会话缓存。先清请求索引，再在游戏线程对缓存自己持有的图集视图和贴图调用 Dispose。对局结束保留已有隔离 PCK 的映射，下一局重新加载贴图时可以复用磁盘包。

缓存最多记录 1,024 个路径，贴图常驻以 512 MiB 为估算上限。同一个原生底图只计一次，AtlasTexture 按完整底图尺寸计入，使用每像素 8 字节估算。预算满时新加载回退当前游戏卡图。单张资源解码后发现超限会立即释放，本局记住该路径，避免因另一个请求键反复解码。预算是常驻估算，原生解码峰值和显存用量仍随资源内容变化。

--test-card-portrait-session 经真实请求 LRU 和隔离加载入口，只替代 Godot 原生贴图操作。测试超过 64 个请求后的同路径复用、不同请求键和分组失效后的复用、超预算资源立即且只释放一次、清理整局并显式释放、下一局重新加载，以及 IgnoreDeep 仍传给加载器。

构建连主 Mod 为 0 警告、0 错误。专项、完整无参数 RuntimeTests 和 LogicTests 均 exit 0。日志在 %TEMP% 下的 sts2-crash-b-*.log。真实玩家奖励场景是否仍触发 Godot 终结器崩溃需实机验证，二进制资源首次加载仍可能报告 UID 路径回退。

## #13 涅奥商人同名程序集

基线为 1.0.36.2（198f031）。玩家日志第 760–763 行显示同一 neowmerchant ID 的两个独立来源均已接管，第 1745 行起在同一加载上下文尝试载入 NpcFeminization2 1.3.7.0 时冲突，商人图鉴的程序集检查也抛出了同一异常。旧回退用 DLL 文件名和已加载程序集的简单名比较，文件名与内部名称不同时无法找到它。

ManagedSkinModLoader 根据程序集元数据简单名检查实际加载上下文。确认占用者来自同逻辑 Mod ID 的已登记来源，并核对程序集文化及签名公钥标记后，复用已有行为程序集。也能通过加载记录识别 Location 为空的内存程序集。不同来源仍各自提供资源，行为由已有程序集的提供者统一管理。行为别名不会将另一份 DLL 登记为独立加载。不相关的同名程序集降级至资源模式。

--test-provider-assembly-reuse 生成文件名与内部名不同的 DLL，在实际加载上下文验证跨版本同名冲突、同完整名但字节不同、内存加载归属、不相关来源的资源回退。主工程构建 0 警告 0 错误，完整 RuntimeTests 和 LogicTests exit 0。日志为 %TEMP% 下 sts2-issue13-*.log。玩家两份商人包的真实画面和行为仍需实机确认。

## #14 商人预览内存保护

玩家日志有 10 次带 RequireHeadroom 调用栈的拒绝。9 次直接来自 GetOrPrepareRuntimeOverlay 的组包前检查，此处固定检查 64 MiB，还未生成贴图估算。另 1 次来自 WithRuntimeResourcesForSelection 的加载阶段。日志没有当时的可用物理内存和可用提交量，无法确认该玩家各次拒绝是否为误判。

组包预检将 64 MiB 暂存量计入可用提交量，并保留 512 MiB 的可用物理内存底线。解码阶段继续按预计用量检查物理内存和提交量，保留 512 MiB 预留、每像素 8 字节及单次解码上限。失败消息带组包或解码阶段、两项可用内存、本阶段需求与保护预留，便于复盘。

独立 PCK 中同时携带原始 PNG 和有效导入重定向时，若其 CTEX 目标均位于本次估算范围，估算只计实际加载的目标。缺失、无效或指向外部的导入仍保守计入 PNG。组包的总估算与逐资源估算都应用同一规则。合成样本从重复计算的 68 MiB 降到 64 MiB。未能绑定依赖的复杂图仍按当前独立包的总估算保护，实际峰值取决于原生引擎与供应商脚本。

主工程构建 0 警告 0 错误。内存专项覆盖组包有效余量、原生解码拒绝、边界、有效导入和缺失导入。完整 RuntimeTests 与 LogicTests 均 exit 0。日志为 %TEMP% 下 sts2-issue14-*.log。玩家场景仍需用新增诊断核对。

## #15 铁甲恶魔皮肤

玩家切换 `Ironclad Demon` 后，Spine 加载隔离别名下的 `characterselect_ironclad.png`，报 `No loader found` 和 `Can't load texture`，随后进程以 `0xC0000005` 退出。原包只有 13 个导入产物，没有源图、`.import` 或 `.remap`。用原包和 v0.113.1 基线离线生成隔离 PCK，复现了有图集、缺 `characterselect_ironclad.png.import`。皮肤包带同一源哈希的普通 `.ctex` 和 `.s3tc.ctex`。测试版基线的 `.import` 指向 `.bptc.ctex`，原索引按完整路径匹配，皮肤页没被归入源路径，隔离包也就没为这张页生成导入映射。正式版 v0.111.1 的基线使用 `.s3tc.ctex`。

- `Catalog/ImportedTexturePathPolicy.cs` 与 `Catalog/SkinCatalog.cs` 按无歧义的源文件名和哈希关联不同格式的导入产物。生成隔离包时借用基线 `.import`，让重定向指向选中包的实际载荷。正式版优先精确匹配的 `.s3tc.ctex`，测试版可用同源的普通 `.ctex`。依赖收集纳入图集声明的每一页。
- `Catalog/SkinCatalog.SpinePreflight.cs` 在写入隔离包前检查图集各页及其重定向。`Core/SkinService.cs` 在加载场景、骨架或图集前，先按已有内存预算检查并加载页贴图。缺页或贴图加载失败时撤销该选择本会话的生效状态，重新挂载并用原版重试，输出一条「缺少贴图页，已回退原版」日志。保留玩家保存的皮肤选择。
- `tests/STS2SkinChanger.RuntimeTests/SpineResourcePreflightTests.cs` 和 `Program.cs` 将两种合成 PCK 接入完整套件，覆盖只含导入产物时的基线映射、实际选中包载荷、缺第二页时的拦截和原版重试。

正式版 v0.111.1 与测试版 v0.113.1 引用构建均为 0 警告、0 错误，LogicTests 和完整无参数 RuntimeTests 两端均通过。真实恶魔包重建后，两个版本的选角页导入目标与原包贴图 SHA-256 一致。生成的 PCK、构建和测试日志位于 `%TEMP%\sts2-gpt-154-1795003952897`。Claude 已复跑并将修复打进测试包。真实选角切换的画面和退出状态待云实测。

## #2 奥契丝修复包

静态检查 `STS2-OrchisNecrobinderSkinFix` v0.1.8。该 DLL 独立向 RitsuLib 注册补丁。其商店 `NMerchantRoom.AfterRoomIsLoaded` 后置遍历所有骨妹玩家，替换骨架、比例和动画。火堆 `NRestSiteCharacter._Ready` 后置用两个开关分别替换 `Necro` 与 `Osty`，再处理各自的比例、节点和动画。两处只认角色类型与修复包设置，不读取切换器中每名玩家的选择。切换器的随从专用规则能避免全局激活奥契丝本体 DLL，无法限制这个独立修复包的回调。

战斗里的骨架替换来自奥契丝本体 `Creature.CreateVisuals` 后置。它有独立的骨妹和奥斯提分支。原包的 `NCreature._Ready` 后置再调骨妹比例和奥斯提排序。修复包自行挂的战斗相关入口主要是 `CharacterModel.GenerateAnimator` 的低血量动画回退、`NNecrobinderVfx._Ready / UpdateFlameVisibility` 的缺头节点保护，以及检查奥契丝骨架路径的奥斯提缩放修复。修复包也给本体 DLL 的假商人和选人背景补丁加了设置门控。它的 VFX 代码只在缺头节点时禁用原版粒子，静态结果没有确认预览小人粒子漏出的具体时序。

- `Core/OrchisSkinFixCompat.cs` 由 `Core/SkinService.cs` 安装。商店过滤成对的玩家和模型，只留实际选奥契丝本体的骨妹。火堆按实际玩家分别检查骨妹和奥斯提的选择，保留另一半仍有权执行的修复。奥契丝本体 DLL 中的战斗骨妹分支、假商人、骨妹比例和选人背景补丁按骨妹自己的选择生效。低血量回退也限定为选中的奥契丝骨妹。保留缺头节点保护、奥斯提缩放、卡图、遗物和设置补丁。
- `ProtectedCharacters/CharSelectTakeoverSuppressor.cs` 注册程序集加载通知。精确目标晚到时补装同一个选人接管前缀与切换通知后缀，按方法记账防止重复。回声抑制仍生效。
- `OrchisRoomCallbackTests.cs` 通过同签名模拟回调验证商店配对过滤、火堆两种混搭及真实 Harmony 拦截。`LateTakeoverGuardTests.cs` 使用 `ScriptPathFixtureScripts.cs` 的晚载程序集验证拦截、通知、重复安装与回声抑制。两组已接入 RuntimeTests 的无参入口。

v0.111.1 与 v0.113.1 源码构建均为 0 警告、0 错误，两端 LogicTests、完整 RuntimeTests 与十字军、贞德真实包保护回归均通过。修复包和奥契丝本体 DLL 仅做静态读取。日志在 `%TEMP%\sts2-gpt-161-1795007247265`。

请云用测试包看三组。① 骨妹非奥契丝、奥斯提奥契丝，切进战斗、商店、火堆后核对骨妹仍是刚选的外观，奥斯提保留奥契丝。② 骨妹奥契丝、奥斯提原版或其他皮肤，确认骨妹的比例、火堆动画仍正常，奥斯提保持自己的选择。③ 选人对照两种组合的小模型粒子和背景，切一次独立奥斯提皮肤，确认通知生效且背景没有被接管。

## #1/#5 Fool_Waifu 头像

维护者的 1.1.0 日志先记录切换器卸下 17 个行为补丁，随后 `Fool_Waifu` 在加载对局资源前自行恢复 24 个回调。只读反编译工坊 1.1.1 找到 `FoolWaifu.ResumeSkinCompatibility.BeforeRun(RunState)`。作者用独立动态程序集 `FoolWaifu.ResumeBridge` 把此回调挂在 `NGame.StartRun` 和 `NGame.LoadRun`，所以按提供者 DLL 归属捕获回调时看不到这个转发。作者快照收录了初始化时属于 `Fool_Waifu` 的补丁，排除了 `SelectCharacter`。恢复判断读取 `CharacterAnimationSelector.RuntimeSkinService.GetActiveVariantId("fool")`，找不到那套管理器时直接当作已选。它没有读取切换器的当前选择。

作者的 `PortraitPatch`、`PortraitOutlinePatch` 和 `CharacterIconScenePatch` 覆盖局内头像，`PackedIconPatches` 覆盖普通和锁定的选人图标，`CharacterSelectButtonTexturePatch` 还在按钮的初始化、解锁和 `_Process` 等入口直接写两个图标节点。这些图标补丁都在上述作者快照的覆盖范围内。`SkinAssets.UiTextures` 只按文件名保存私有路径加载的贴图。贴图由回调主动返回，没有接管原生资源路径。现有局内 HUD 刷新会重建 `Character.Icon` 子节点，选人刷新会重新设置 `%Icon` 和 `%IconAdd`，因此保留缓存供下次选中使用。

- `Core/FoolWaifuRuntimeCompat.cs` 在当前愚者提供者不是 Waifu 时拦截 `BeforeRun`。共享贴图入口只拦头像、描边、选人图标和地图标记，防止仍在位的回调从私有缓存返回错误贴图。手部、立绘和半眼设置继续走原路径。选中 Waifu 时放行原方法。
- `Core/SkinService.cs` 安装已加载的目标，`Core/IndependentFoolSkinCompat.cs` 在按需激活时补装。安装按方法幂等。`Core/ManagedSkinModLoader.cs` 在切走时把运行中额外装回的头像回调也纳入原有卸载流程，只识别上述五个补丁类。
- `tests/STS2SkinChanger.RuntimeTests/FoolWaifuIdentityTests.cs` 和 `Program.cs` 验证预先创建的转发委托、未选时禁止重装、切回后恢复、缓存中的头像无法越权，以及只卸头像回调、保留独立设置。

正式版 v0.111.1 和测试版 v0.113.1 主工程均为 0 警告、0 错误，LogicTests 和完整无参数 RuntimeTests 两端通过。工坊三款真实愚者皮肤专项在测试版通过，正式版正确排除本端不存在的愚者。构建与日志在 `%TEMP%\sts2-gpt-164-1795059287020`。

请云在测试版先选一次 Waifu，再换 Hilde 开局并读档，检查左上角头像是否保留 Hilde。回选人界面查看下方愚者按钮，确认普通与锁定图标没有变成 Waifu。最后切回 Waifu，检查两个入口和独立半眼设置。

### #166 · Hilde 商店模型缩小

云的 `godot2026-11-19T11.41.52.log` 第 2578 行停用了 Waifu 的 17 个行为补丁，第 2729 行由 Waifu 自行恢复 24 个。当时选的是 Hilde。进商店时，第 3963–3979 行显示 Waifu 从原版 `_Ready` 同步触发的 `PlayAnimation` 后置调用 `AttachMerchant`，第 4021–4031 行又显示它从自己的 `MerchantReadyPatch` 调用同一入口，两次都报自带骨架初始化失败。商店越权与头像是同一次自恢复。

核对 1.1.1 的 DLL 与 PCK。此次走的是 `OwnRigSkin.TryAttach`，这条路径没有读取 `MerchantHeight` 或改写 Hilde 骨架的缩放。Waifu 将挂载节点移到原骨架前面，之后才执行 `configure`。失败时只调用 `QueueFree`，到帧末才移除节点。CznCore 在原版 `NMerchantCharacter._Ready` 后置发布 `MerchantCharacterReadyEvent`，Hilde 要求第一个子节点能绑定为 SpineSprite 才进入 `MerchantPresentationRuntime.Apply`。抢占第一个子节点会让 Hilde 跳过自己的商店尺寸。原版 `fool_merchant.tscn` 的 SpineSprite 是 `0.47`，Hilde 的正常商店路径由 `CznMerchantSurface` 设为 `2.021`。这条时序能解释模型缩小，旧日志没有该玩家最终显示尺寸的实测值。

`FoolWaifuRuntimeCompat.cs` 用现有愚者选择判断拦截 `SkinVisuals.AttachMerchant(NMerchantCharacter)`，覆盖 Ready 和动画两条调用。选回 Waifu 仍允许它挂载。变更只涉及该兼容文件与 `FoolWaifuIdentityTests.cs`，未涉及商人热切换实现。离线合成测试验证旧回调尚在时，选中 Hilde 会拦住两次挂载，选中 Waifu 会放行。双端主工程均 0 警告、0 错误，LogicTests 和完整 RuntimeTests 均通过。日志在 `%TEMP%\sts2-gpt-166-1795060091778`。

请云用同样的存档或路线复测。先选一次 Waifu，再换 Hilde 进商店，核对愚者玩家模型尺寸和待机动画，并确认没有 Waifu 挂载警告。最后选回 Waifu 再进商店，确认它自己的商店表现仍在。

### #221 · 联机战斗建模降级

#221 已完成。Gemini 可以接手 `Core/SkinService.cs` 的卡图改动。修复留在 `fix/1.0.36-stability` 的工作区，基线 `b3dc0da`。

玩家日志 91958 行显示第三名队友的外观资源申请被内存守卫拒绝，异常从 `Creature.CreateVisuals` 传回 `NCombatRoom._Ready`，中断后续队友、怪物和站位初始化。92008、92094、92347 和 92683 行是同一局的顶栏贴图替换失败。

- `Creature.CreateVisuals`、`NCreature._Ready` 只捕获切换器自己的范围初始化、场景替换和外观后置异常。范围申请失败时释放临时选择及资源覆盖，允许游戏建模和 Ready 继续。后置失败保留游戏调用已经返回的模型，释放放弃的替换节点。创建时已经降级的模型在 Ready 跳过重复申请及相应的外观回调。原生调用抛出的异常由 Harmony Finalizer 原样交还。
- 资源加载失败时删除失败条目的引用，战斗中的全量清理登记到下一次非战斗更新再执行。顶栏头像与接管贴图失败时保留现有控件或贴图，同类警告每 30 秒最多一条。512 MiB 系统内存余量、分别预估运行包与解码内存、只计实际载入贴图的规则继续适用。
- 正式版 v0.111.1 和测试版 v0.113.1 的主工程与测试项目均为 0 警告、0 错误。两端 LogicTests、完整 RuntimeTests、`--test-runtime-safety`、`--test-protected-boundaries`、`--test-preview-binding`、十字军实包 hooks、十字军与两版贞德实包套件均通过。完整套件包含当前共享工作区里的卡图用例。日志在 `%TEMP%/sts2-gpt-221-1795104646919/{official,beta}-final.log`。
- 新回归安装真实 Harmony 补丁，用替身提供原生模型结果和 Ready 完成点，并在创建前置、创建后置、Ready 前置和 Ready 外观回调分别注入内存拒绝。确认游戏返回的模型对象被保留，Ready 继续，下一个单位独立创建，原生异常仍是同一个对象，战斗中待执行的清理请求继续保留。

尚需真实四人联机复测。用同一个 Boss 存档，先在正常内存下进战斗，检查四名队友、怪物、站位和各自皮肤。再用测试构建仅对第三名队友注入一次外观余量拒绝，检查其可见模型、后续单位与站位，双方出牌一回合并核对 checksum。连续打开顶栏和投票头像，检查贴图仍在且警告没有刷屏。实机环节尚未执行。未提交、未部署，也未改第三方包。
