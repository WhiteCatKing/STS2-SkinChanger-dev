# Skin Changer 注意事项：踩坑复盘与排查清单

截至：2026-11-17，发布版本 1.0.36 / 内部 1.0.36.0。

依据：本次对话、已有项目工作记录、修改件 `docs/issues` 中截至本次发布的 67 份 ISSUE 记录、当前开发约定和保护区说明。这里的“所有”指这些可核对记录中的问题，不补造已经遗失的日志、路径、原生堆栈或测试结论。文末完整索引保留逐项追溯入口。

## 1. 先分清证据与状态

- **已证实的错误**：能从实际 DLL/PCK、调用顺序、日志或回归中验证的缺陷，下面按类别列出原因和排查方法。
- **用户确认**：对应用户测试过的版本与场景。本次用户确认生物图鉴与完整成鸟攻击已可用，随后要求发布；不是对所有第三方包和联机组合的保证。
- **待确认的历史问题**：有些早期记录只有反馈或初步判断，甚至同时写着“未解决”和“已修复待测”。保留历史，不一律改成已解决。
- 离线测试进程崩溃、游戏退出时资源泄漏警告、真正游戏异常退出是三种证据，必须分别记录。日志最后一条、82% 进度或某个 Mod 名称不能单独证明肇因。
- 历史记录还明确记过 `OtherCompendiumUiTests` 遗留失败、部分真实包因默认路径不存在而跳过。不能因为“与本次无关”就把全套测试写成全绿；报告实际调用、失败和跳过，后续重跑再更新结论。

## 2. 环境、源码与构建中犯过的错

| 坑或错误 | 后果 | 每次应检查什么 |
| --- | --- | --- |
| 长期只用测试版编译和验证 | 正式版预览出现 Body getter 等缺失方法，甚至其他角色走同一预览入口也失败 | 生产固定用正式版快照引用，测试版用 ReleaseBeta；把同一个生产 DLL 复制到两个运行器并核对哈希。 |
| 以为同名 API 就 ABI 一致 | Body 属性/方法、SetAnimation 返回值、CreateForTest 参数、MerchantButton 虚调用、事件文本接口均发生过差异 | 查本机 DLL 的真实签名、MVID、IL 和可选参数；用能力适配，禁止静态引用不存在的新版类型。特别检查抽象成员是否用了错误的 call。 |
| 测试命令漏传 GameAssemblyDir | 引用落到过时默认路径，出现 ModelId 等批量编译错误 | 所有涉及游戏引用的 build/run 显式指定快照目录；先核对命令，不把环境错误当程序回归。 |
| 实际包嵌套目录与传入目录不一致 | 旧鸡煲专项找不到 DefectSkinMod.dll | 先枚举实际路径。该包在“杂鱼鸡煲娘化/DefectSkinMod”内还有一层，不能只传外层。 |
| 批量写文件把换行合并，随后用反编译产物替换手写源码 | 下拉和小人预览进一步消失，最终需要从工作备份恢复 | 小范围补丁、保留 LF、立刻构建及 diff 检查；反编译用于核对合同，不作为手写源码整体替代。恢复后逐项补回已验证修改。 |
| PowerShell 直接套用其他 shell 的语法或字符串规则 | 命令解析失败；本次发布脚本还因英文弯引号被 PowerShell 当引号而中断 | 多行文本用结构化参数/文件或明确的 here-string；JSON 用序列化器；不靠手工转义拼长命令。错误命令未执行时先核对状态，不重复提升版本。 |
| 照抄历史路径、分支或索引参数 | 找不到目录；图查询静默返回零变更 | 当前仓库是“工作2”、实际分支 main。旧说明里的 gurio 路径、master、GENERAL.md 等可能失效；核对真实文件与 Git 分支。图工具不可用时按约定用 rg，不能把查不到当符号不存在。 |
| 误把任务开始前的无关变更一起提交 | 覆盖用户已有文件状态 | 当前两份旧公告文档的预先删除状态保持原样；显式 git add 本轮路径，不在根目录无差别 add。 |

## 3. 识别与程序集所有权

| 已遇到的问题 | 正确排查及边界 |
| --- | --- |
| 内容 Mod 被当外观包隔离，整包功能消失 | 先看是否添加角色、卡牌、事件等玩法；皮肤归组只能基于明确合同，不能见到 DLL/贴图就接管整包。 |
| CustomSkeletonLoader 等资源管线工具、CznStyleUI 被误分类 | 核对补丁目标和注册方式，交还原加载器；不能把 UI/资源加载工具当普通卡图皮肤。 |
| CZN 怪物包已“启用”，身体仍原版 | 作者是在 MonsterModel.CreateVisuals 的 Prefix 创建私有骨架，不是替换规范 PCK 文件；确认原生宿主、逐怪物 profile 和 Prefix 是否真正执行，不能只看启动日志。 |
| 新框架声明变化使身体/框架注册丢失 | 检查实际 PE 元数据里的 CharacterSkinProfile、typeof(T)、构造参数与注册回调；目标和骨架必须唯一。不要执行所有静态字段初始化来“扫出”未知声明。 |
| 只识别 .pck 扩展名或规范路径，漏掉 .assets、DLL 内嵌图和私有资源 | Naiwa、静态愚者、Waifu 均出现过。检查实际包格式、图片来源、精确运行时合同；不要用名称包含“皮肤”或某几个 ID 泛化所有未知包。 |
| 同源 DLL 的文件名不同但程序集身份相同 | 两包被 CLR 合并，第二份行为不运行；贞德双包变体和商人别名均受过影响。核对 AssemblyName、版本、MVID、代码身份与路径。资源可以独立，行为别名/隔离要按实际合同处理，不能随意重命名第三方 DLL 解决。 |
| 工坊订阅和本地 mods 同时装同 ID | 游戏可能禁用低版本或加载另一份，排查的不是实际生效包。从日志列出真正加载路径、ID、版本；不要看到硬盘文件已更新就断言游戏用的是它。历史重复包的另一条路径没有完整记录时不要补造。 |
| 用全局 PatchAll、全域初始化补救漏回调 | 可能重复注册、激活无关皮肤，或把玩法与设置钩子一起破坏。只启用所选 provider 的必要行为，初始化幂等；逐类补丁失败有隔离。头像/图鉴探查不得顺便运行整包初始化器。 |

## 4. 缓存、隔离资源、内存与崩溃

1. **骨架变了，纹理还是原版**：奶蛙复用了原版 atlas，而依赖图先从 atlas 所属游戏索引选中原版页面，后续没机会选皮肤页。检查最终隔离图中的 skel、atlas、CTEX 字节和源别名；页面先按当前选择解析，再回退图集来源。只看骨架路径不够。
2. **同一路径多层缓存**：Godot ResourceLoader 与游戏 PreloadManager/AssetCache 是两套缓存。奥斯提猫切回默认/独立皮肤时曾反复被旧场景污染。检查当前选项、最终覆盖层、两套缓存及 failed-to-load 负缓存，不能只清一个字典。
3. **修正包反而把旧猫壳写回来**：还原基线后再次从已经提权的公共路径取“原版”，拿到的仍是旧皮肤。基线必须来自可核对的游戏包，而不是当时 res:// 栈顶。
4. **TakeOverPath 清空旧资源 ResourcePath**：新预览抢占规范路径后，旧背景还活着，但作者用来判断归属和调色页的路径变空。曾只测缓存获胜者，漏测被挤出资源，造成回归。检查活跃旧资源、保留别名、临时所有权恢复；不能为保住一方又清空另一方。
5. **作者严格比较原版路径**：涅奥商人回调确实执行却找不到目标，是 alias/retained 路径不满足作者比较。核对作者实际查找逻辑；兼容路径身份而不取消资源隔离，不用伪造“回调执行成功”代替模型验证。
6. **场景实例化和入树仍会解析依赖**：Load 返回 PackedScene 不代表全部外部资源已完成。保持所选资源作用域覆盖加载、Instantiate 和必要的 AddChild，避免绑定回原版或上一皮肤。
7. **Dispose 后重复挂载本组**：同款本机资源作用域退出时又整包提权，造成额外大包写入与原生挂载。只恢复真正受影响的外部组和不同远端选择；空恢复集合不挂载。
8. **OS 没锁住文件不代表 Godot 没引用**：挂载 PCK 的原生目录项不能靠“能否删除”判断寿命。当前会话引用过的包显式固定；不复用被外部删除后的同一原生路径；离开联机不立即删已挂载包。
9. **压缩大小不是解码内存**：已核对示例估算十字军约 460.8 MiB、贞德 SFW 约 251 MiB、战损包约 748.4 MiB。检查纹理头、像素尺寸、完整依赖和每个加载根的可用提交/物理余量；不要为了加速关闭预算、冷却和淘汰。
10. **资源失败后继续交给 Spine**：日志曾出现 WebP 解码、mem=null，之后仍继续建模。遇到空/无效/零尺寸纹理终止本轮，冷却失败包并保留旧展示，不能 catch 后当成功。
11. **后台加载公共可变 res://**：暂停新预读不能撤回在途请求。预读只接受不可变私有别名，检查路径逃逸；对并发、作用域、缓存引用分别设边界。
12. **先提交选择、后发现画面没加载成功**：造成新下拉配旧画面、旧异步回调又覆盖新选择。背景/头像/必需预览准备成功才提交，失败回滚；所有延迟请求验证代次、身份和节点是否仍在树内。
13. **0xC0000005 不能用普通 try/catch 证明修好**：有转储查原生栈，没有转储只修复已确认的危险路径。日志尾、截图进度和内存估算不等于原生因果证明。

## 5. 选人界面、动作和输入

| 坑 | 必查点 |
| --- | --- |
| 只修标准模式，极速/恐惧仍原版 | 三个实际屏幕类型、SelectCharacter 参数、背景宿主、头像和联机大厅入口都核对；保留灰度、模式说明等自身特色，不复制整套标准 UI 覆盖新模式。 |
| 旧作者回调参数与新版不符，PatchAll 中断后半段预载 | 旧鸡煲模型/背景两个预载字段都要检查；仅恢复已证实的 Preload 和选择回放，避免重跑部分成功的全局初始化。 |
| 局内正常，菜单小人仍原皮 | 选人预览有独立构建链；复用作者模型与待机，不在菜单执行战斗 NCreature.Ready。 |
| 静态图下还叠着原版模型 | Body 在 Ready 后才赋值；隐藏/替换必须在入树和作者呈现收尾之后、截图捕捉之前，不能在 Body 为空时以为隐藏成功。 |
| 背景重建清掉作者大图、工具条或点击接收器 | 只清理切换器拥有的节点；记录作者呈现前后变化并可逆恢复。工具条布局、裁剪、MouseFilter 和输入绑定都要查。 |
| 未点角色的头像是原皮，点后才正确 | DLL getter / 按钮 Init 的代码头像不能依赖当前聚焦 runtime。按每个角色保存的选择读取，普通 Icon 和 IconAdd 两层一起改；保留锁定和资源头像优先级。 |
| “点击切形态”被误当作“快捷键切配色” | 贞德已发生过错误方案：给没有点击形态的包加了点击，顶掉另一包作者行为。先分清配色、骨骼战损、快捷键、各包能力，不能互相替代。 |
| Spine 绑定正确但动画不对 | 动作按实际骨架匹配；Shiv 优先 shiv 再回退，不能一律 attack。不要把制作期动作或不存在动作放进按钮。 |
| 只播成鸟 attack，缺少全屏俯冲 | 完整攻击是身体动作加独立 byrdine_overlay_vfx。核对官方 0.6 秒、attack_overlay、音效、shader/槽位拖尾和视口中心；不调用真实战斗工厂或伤害。 |
| 异鸟成鸟缺羽毛/眼睛 | 原生模型通过 SetupSkins 组合子皮肤；直接 Instantiate 不足。仅原生合同满足时组合，别覆盖不具备该结构的替换骨架。 |
| 奥斯提图鉴与主界面各自维护一套状态 | 共用选项、随机/独立包、保存入口和事件；只用 companion 最小事务，不能顺手切主人。独立工厂、场景皮肤、骨架三条显示路径都保留。 |
| 帧动画皮肤退成原版骨架或根精灵被判空 | 检查 root 本身和子树是否可绘制；边界测量与待机播放也必须包含根节点。动作循环设置只改 SpriteFrames 副本。 |
| 按钮透明层吞右键/拖动重置抢事件 | 核对 MouseFilter、全局输入、Popup/ItemList、焦点、ZIndex、覆盖层；既看控件存在，也看真正的点击目标。 |
| 重复点击、关闭界面后特效/监听仍残留 | 同一展示只保留一个序列；取消 Tween、结束回调、TreeExiting 与静态事件订阅；过期异步回调不得复活已关闭界面。 |

## 6. 商人、房间和官方场景

- **不要强转所有角色为原生 NMerchantCharacter**：第三方角色与混合联机队伍会返回不同节点；创建交给游戏/作者，外观刷新核对实际类型。
- **不能用 GetIndex 推断玩家归属**：热替换时旧/新节点共存，索引不稳定；按真实 owner/player 和 profile.AppliesTo 判断，先判断再执行作者回调。
- **一个商人包要核对普通/假商人/极速三类**：作者没做哪类就用该类原版。补齐选项不等于强行复用骨架；旧 B 包无极速合同不能套普通场景。
- **入树前的 Prefix/ApplyBeforeEnteringTree 不能改成 Ready 后回放**：更新 Merchant2CuteII 在树内会拒绝替换；按原生创建阶段调用，先校验房间、按钮、Inventory、Proceed、六个 Token 与 reticle。
- **局内换商人只换显示，不重建玩法**：沿用原库存、商品、购买记录、剩余选择次数；绑定 Setup 与退出 Teardown，耗尽状态保持。失败撤销新节点并保留旧店。
- **首次进店也要处理，不能只测热切换**：新版拆房间或移除 Create 后，旧工厂补丁不再被调用；核对具体 Ready、预加载缓存和初次绑定。
- **保留作者名字和布局**：把 MerchantButton/FakeMerchantButton 都改成通用名字会破坏作者判型；SubViewport 不是 Control，缺全屏布局宿主会跑左上；也不要给新极速场景复制旧普通商人的透明度、坐标、缩放。
- **可选变体场景先探测存在性**：曾生成 ironclad_3_rest_site.tscn 这样的不存在路径，导致整个火堆初始化失败。设置变体不等于独立场景；缺失则回默认。
- **图鉴没有真实局内同步器**：ProceedButton.Ready/ExitTree 会访问不存在的 MapSelectionSynchronizer；仅预览隔离相关订阅，不给真实商店加全局跳过。
- **假商人背景不是普通战场图层**：误路由到不存在资源组或用商店/事件图拼战场会黑屏；查资源归组、事件完整场景和真实 profile。注册前探查能力，不能填一个会黑屏的选项。

## 7. 战损、保护角色与联机

1. 十字军/贞德及所有变体是保护区。改共享模型注册、ContextualSkinControls、SkinService 的键/加载/清理也必须跑保护回归，不删断言、不通过改停用标志绕开问题。
2. 十字军曾丢 SingletonModel/战斗出牌钩子：只保留模型不等于兼容。检查 ModelDb、正式出招、卡牌动画、事件注册次数与取消选择门控。
3. 贞德多包程序集身份、行为代执行、设置值、静态字段和资源必须隔离；不能把文件名当程序集身份，也不能拿另一包的规范缓存当本包调色基线。
4. 战损后的黄色头发等配色回默认，原因是作者只登记首档调色页。检查所选变体权重、当前档 Page、live atlas/skeleton 归属；补页要原子发布，失败不留下半成品材质，反复切换不无限新增。
5. 接收器还在、MouseFilter=Stop 不等于点击仍有效；重建背景后的 live skeleton、StageResources、Palette.PageOf 与 ResourcePath 都要验证。保证作者点击和兼容点击不双重执行。
6. 选人队友显示策略不应永久覆盖整局。远端皮肤、模型/血条偏移、充能球身体与图标、火堆/商店场景分别核对；队友没有本地记录时不能退到“我的偏移”。
7. 联机临时资源选择必须按玩家/外观实例绑定，嵌套作用域退出恢复之前选择。只在本机保存的选择上取 getter，会把自己的皮肤套给队友。
8. 小人拖动、停靠和瞄准圈坐标转换分开处理；变体重建后再套变换，不能在老节点上套完就认为新节点继承正确。不要重新广播用户要求本机独立的变换。

## 8. 卡牌、字体、背景与特效的额外教训

- 大卡检视在 NCard.UpdateVisuals 后又用 BigPortrait 覆盖皮肤图，补丁必须在最终赋图边界生效；下拉名称正确不代表大卡正确。升级、变化前后与左右键入口分开检查。
- 变化光效看不见，曾先怀疑加载卡顿，实际是切换器卡面盖住了 CardVfxContainer。先检查节点顺序、ZIndex、透明面板、shader，再决定是否预加载；不要只用计时器“补动画”。
- Pengo 塔罗中文破损的已确认原因是只含拉丁字形的像素字体，不是 JSON 编码。先检查字形覆盖、fallback 字体和 Label 覆盖；不要批量重写正常本地化表。
- 固定像素武器/护盾挂在 SpineSlotNode 下只继承节点缩放，不等于继承骨骼单位。更换身体后特效巨大应查实际父节点与局部/全局变换；历史 MoreAnim 独立补丁仍有待测记录，不把统一缩放常数推广到所有角色。
- 战场背景必须有地区/遭遇 profile 与可用层结构。帝皇蟹等场景有自有逻辑节点；只改 Layer_XX，先创建配置成功再换，不能清整棵背景树。
- 瓦库散图需要保留作者 Image，同时提供并隐藏官方流程期待的 TextureRect 空槽；“游戏默认”和“官方彩蛋”分开，按分支能力显示。只 Instantiate 而没走 ConfigureBackgroundScene 会得到空图。

## 9. MLM、诊断与发布

| 坑 | 排查或处理 |
| --- | --- |
| 共享框架被放入可卸载上下文，皮肤却在默认域 | 对照真实 MLM 版本、依赖树、AssemblyLoadContext 与设置注册；协调必须早于框架加载，不能迁移已经加载的 CLR 类型。 |
| 一键整理只改内存，重启 ignorePriority 又回来了 | 原生组织文件需持久化并回读；正确顺序是 MLM 本体后紧接切换器。已有错误上下文需正确排序再重启，不推荐忽略并继续。 |
| 异常提示只藏在技术详情 | 正文说清加载顺序、需要重启和能执行的整理动作，不能只显示一个笼统“冲突”。 |
| 把 ERROR、无响应或零退出码直接叫崩溃 | 查 PID、进程启动时间、退出码、托管异常、系统事件、转储和正常退出标记；“疑似卡死”与确定异常退出分别记录。 |
| 游戏重开/日志轮换后写错 godot.log | 观察器持原会话句柄或核对文件身份；锁定则下次补记一次，不把旧会话写进新会话。双开时一个日志路径尤其要核对。 |
| 无原生 Godot 的离线测试调用 NodePath/GD.Print 等 | 曾导致测试进程 0xC0000005，另有测试首次缺少 AuditLogSilencer。需要原生的调用做隔离/测试替身；不可把离线失败写成游戏崩溃，也不可把未跑部分标成通过。 |
| 认为崩溃监视无所不能 | Mod 加载前、断电、观察器被结束、脚本策略、不可写存储等有边界；不自动归责 Mod，不自动上传，不擅自启游戏或改 WER 注册表。 |
| 发布号、manifest、DLL 和工坊页面不一致 | 内测只加第四位；明确上传时升第三位并归零第四位。公开三段、内部四段、csproj 四字段、Entry、两份 JSON 全部一致。 |
| 只替换一个安装包，游戏或工坊仍用旧 DLL | 同步两个 DLL + manifest 到正式/测试包、staging、指定游戏目录；真正发布再同步订阅目录，逐文件哈希核验。生产必须来自 Release 正式引用。 |
| 只更新英语，其他语言还是旧版 | 15 语言摘要、版本与描述同步，上传后逐项回读；保留原条目 ID、投稿入口、主图与 12 张附加图。只拿 Submit OK 不算所有本地化都成功。 |
| 工坊清单“可免重启”凭印象填写 | 必须核对实际包启动/前置和完整接管；本地包缺失时保留已有标签，不能默认赋分类。发布现有 Mod 本体与修改收录清单是两件事。 |
| Git 直连重置、失败后盲目重复发布 | 按已有可用代理推送；分别检查 Steam 结果和 Git refs。Steam 已成功就只补失败环节，不反复上传或再升版本。 |

## 10. 按症状开始排查

| 症状 | 优先顺序 |
| --- | --- |
| 订阅了却不出现 | 真正加载路径/重复包 → 游戏版本 → Mod 类型归属 → 实际资源格式/元数据 → 明确目标组。 |
| 列出来仍原皮 | 保存选项/运行时激活范围 → 作者 Prefix/预载是否执行 → 隔离依赖实际字节 → 两套缓存 → 显示入口是否最终被覆盖。 |
| 拉伸、碎图、颜色错 | skel/atlas/纹理页是否同一选择 → 源别名和 ResourcePath → 调色页/变体权重 → 父节点缩放。 |
| 叠原模型或大图丢失 | Ready/Body 可用时机 → 作者收尾 → 原身体隐藏 → 背景/控件所有权恢复。 |
| 黑店、空商店、火堆不能用 | 场景是否真实存在 → 普通/假/极速及节点类型 → 原生宿主/布局 → 初始化前后时机 → 库存不变与失败回滚。 |
| 动作不全或不播放 | 骨架实际动作名 → 身体动作与独立 VFX 是否分离 → native 时序/音效 → 对应视口与皮肤组合 → 重复/退出清理。 |
| 连续切换后闪退/卡顿 | 解码余量/失败冷却 → 重复挂载 → PCK 固定与裁剪 → 活跃路径抢占 → 后台公共路径 → 同会话转储证据。 |
| 队友串皮肤或模型飞走 | 真实 owner → 按玩家作用域 → 默认回退是否借了自己 → 本机变换是否错误广播 → 远端场景/充能球缓存。 |
| 启动报框架/设置变少 | MLM 顺序与持久配置 → 共享框架加载上下文 → 依赖版本/同源 DLL → 初始器和注册是否被跳过。 |

## 11. 每次交付的最小检查顺序

1. 记录用户症状、版本、实际包路径和已有改动；读取适用 AGENTS，不动原始源码目录及无关变更。
2. 能从日志/DLL/PCK确认的先确认；未知保留未知，附图/旧文档不是新的执行授权。
3. 做最小范围修复，增加能暴露真实缺陷的检查；不为通过测试删保护断言，也不把只验证声明当作最终渲染。
4. 显式正式版引用构建；同一 DLL 双端；按影响面加实际包/保护区/资源生命周期检查。失败先解释并解决，记录确实跳过的项目。
5. 核对新旧选择、默认回退、跨模式、首次进入、重复切换、无关角色、快速点击、关闭/退出；涉及联机再检查 owner 与嵌套恢复。
6. 更新四段版本与专项记录，确认游戏已退出，同步产物并核对哈希。不要控制游戏做实机测试。
7. 用户明确要求发布时，准备 15 语言、上传原工坊条目、回读核验后再提交推送 Git 与标签。只请求修复时不自动上传。
8. 报告实际完成与限制；用户验证通过可作为对应场景的反馈，但不能替代所有环境的证据。

## 12. 历史待确认事项与后续证据

观者旧火堆回退、早期卡牌下拉失效、右键升级入口、MoreAnim 全角色特效比例等旧反馈，部分只有推测或补丁待测。现代相邻功能通过专项不能自动证明旧反馈的原始环境已解决。后续应按对应包版本、完整日志、实际入口和可重现条件重新核对；没有样本时不要补造绝对路径或“已验证”字样。

以下原始索引包含本次采用的全部 67 份 ISSUE 记录。个别标题或“状态”是当时记录，不等于当前发布结论；当前交付以本次项目/发布记录和对应专项证据为准。

## 附录：完整 ISSUE 索引

| 原始记录 | 历史主题 |
| --- | --- |
| [ISSUE_2026-09-09_pengo_tarot_text](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-09-09_pengo_tarot_text.md) | Pengo's Tarot Enchantments 文本破损 |
| [ISSUE_2026-09-15_merchant_model_top_left](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-09-15_merchant_model_top_left.md) | 商人娘化模型跑到左上角 / 连续进商店错位 |
| [ISSUE_2026-09-16_modlaunchmanager_conflict](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-09-16_modlaunchmanager_conflict.md) | 与 ModLaunchManager 启动冲突 |
| [ISSUE_2026-09-17_rest_site_watcher_skin](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-09-17_rest_site_watcher_skin.md) | 火堆场景观者皮肤回退原版 |
| [ISSUE_2026-09-18_crusader_skin_conflict](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-09-18_crusader_skin_conflict.md) | 战士“十字军”Mod 皮肤冲突 |
| [ISSUE_2026-09-26_card_skin_options_dead](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-09-26_card_skin_options_dead.md) | 卡牌皮肤选项失效 |
| [ISSUE_2026-10-19_card_rightclick_upgrade](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-10-19_card_rightclick_upgrade.md) | 无法右键卡牌查看升级 |
| [ISSUE_2026-11-03_shiv_managed_skin_animation](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-03_shiv_managed_skin_animation.md) | 受管皮肤 Shiv 与普通攻击同动画 |
| [ISSUE_2026-11-04_battle_background_switch](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-04_battle_background_switch.md) | 战斗背景切换（按地区） |
| [ISSUE_2026-11-04_czn_enemy_skin_monster_visuals](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-04_czn_enemy_skin_monster_visuals.md) | CZN 怪物美化包（CznEnemySkin）的怪物模型不生效 |
| [ISSUE_2026-11-06_duplicate_source_dll_behavior](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-06_duplicate_source_dll_behavior.md) | 同 ID 差分包：无法执行自己的 DLL，依赖代码的皮肤错位 |
| [ISSUE_2026-11-07_card_transform_vfx_stutter](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-07_card_transform_vfx_stutter.md) | 事件里“变化”卡牌时特效生硬 / 像没有特效 |
| [ISSUE_2026-11-07_chen_ironclad_recognition](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-07_chen_ironclad_recognition.md) | Chen Ironclad 皮肤"部分运行时识别不到" |
| [ISSUE_2026-11-07_content_mod_hidden](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-07_content_mod_hidden.md) | 内容 Mod（如「杀戮尖塔2晓夫九」）被当成外观包隔离，整包像被隐藏 |
| [ISSUE_2026-11-07_custom_skeleton_loader_compat](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-07_custom_skeleton_loader_compat.md) | 与 Custom Skeleton Loader 同开时模型加载不正常 |
| [ISSUE_2026-11-07_czn_select_icons_and_toolbar](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-07_czn_select_icons_and_toolbar.md) | CZN 皮肤：选人栏头像 / 顶栏头像仍是原皮；背景切换按钮消失 |
| [ISSUE_2026-11-07_czn_select_presentation](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-07_czn_select_presentation.md) | CZN 系皮肤（Mei Lin / Watcher）选人界面背景与大图头像仍是原皮 |
| [ISSUE_2026-11-07_fake_merchant_background](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-07_fake_merchant_background.md) | 假商人事件「背景」：战斗背景跟随事件图 + 下拉可用 |
| [ISSUE_2026-11-07_in_run_monster_skin_stuck](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-07_in_run_monster_skin_stuck.md) | 局内换怪物皮肤「选了不变 + 选项变灰 + SL 不恢复」 |
| [ISSUE_2026-11-07_kaiser_crab_background_crash](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-07_kaiser_crab_background_crash.md) | 进帝皇蟹 Boss 房：背景功能把遭遇自带背景当风景替换，本体无纹理、无卡牌、整局卡死 |
| [ISSUE_2026-11-07_merchant_duplicate_disabled](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-07_merchant_duplicate_disabled.md) | 商人皮肤「Merchant2CuteII」选项消失：同 ID 双份来源被游戏整体禁用 |
| [ISSUE_2026-11-07_multiplayer_skin_independence](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-07_multiplayer_skin_independence.md) | 联机：选人界面的显示方式与局内解耦（队友皮肤互相独立） |
| [ISSUE_2026-11-07_provider_settings_variants](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-07_provider_settings_variants.md) | 皮肤包「外观变体」拆成独立皮肤选项（设置驱动型） |
| [ISSUE_2026-11-07_rebuild_from_backup](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-07_rebuild_from_backup.md) | 1.0.27.82 重建记录（从「工作备份」恢复，回退到文件损毁前一步） |
| [ISSUE_2026-11-07_remote_orb_skin](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-07_remote_orb_skin.md) | 联机里远端 Defect 的充能球皮肤不生效（本地正常） |
| [ISSUE_2026-11-07_remote_rest_site_vanilla](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-07_remote_rest_site_vanilla.md) | 联机里远端玩家在火堆显示原皮（战斗正常） |
| [ISSUE_2026-11-07_variant_rest_site_crash](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-07_variant_rest_site_crash.md) | 火堆/商人被变体后缀打断：`ironclad_3_rest_site.tscn` 加载失败导致火堆不能用 |
| [ISSUE_2026-11-08_more_anims_vfx_scale](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-08_more_anims_vfx_scale.md) | 装了角色皮肤后「More * Animations」系列的道具/特效被放大数倍 |
| [ISSUE_2026-11-08_variant_hotkey_binding](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-08_variant_hotkey_binding.md) | 联机中"同角色变体互相牵连 / 改不了队友位置 / 队友不显示血条" 与变体快捷键双向绑定 |
| [ISSUE_2026-11-09_joan_dual_pack_variant_isolation](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-09_joan_dual_pack_variant_isolation.md) | 战士贞德双包（0.3.5-S / 0.3.6）：变体快捷键、点击切形态、双包互不干扰 |
| [ISSUE_2026-11-10_osty_pack_leak_and_reticle_transform](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-10_osty_pack_leak_and_reticle_transform.md) | 随从皮肤包盖住主角皮肤 + 局内"外观"选择框错位 / 重置无效 |
| [ISSUE_2026-11-11_midori_osty_preview_and_impostor_override](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-11_midori_osty_preview_and_impostor_override.md) | 奥斯提：`midori_black` 选人预览不显示 + 提权后抢占「撑在地上的鸡煲」 |
| [ISSUE_2026-11-16_card_inspect_portrait](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-16_card_inspect_portrait.md) | 1.0.32.4：点开大卡后卡图与选择不一致（2026-11-16） |
| [ISSUE_2026-11-16_character_binding_idle_loading](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-16_character_binding_idle_loading.md) | 1.0.32.12：角色外观同源绑定、必备预览与空闲加载 |
| [ISSUE_2026-11-16_character_owner_presentation](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-16_character_owner_presentation.md) | 1.0.32.8：局内角色切换被其他 CZN 皮肤覆盖（2026-11-16） |
| [ISSUE_2026-11-16_crusader_missing_combat_hooks](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-16_crusader_missing_combat_hooks.md) | 1.0.32.10：CrusaderSkin 局内动作只有前半段（2026-11-16） |
| [ISSUE_2026-11-16_czn_shared_presentation](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-16_czn_shared_presentation.md) | 1.0.32.5：CZN 系列共享呈现兼容与切换安全（2026-11-16） |
| [ISSUE_2026-11-16_initial_shop_cached_merchant](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-16_initial_shop_cached_merchant.md) | 1.0.32.9：新商店首次进入显示缩小原版商人（2026-11-16） |
| [ISSUE_2026-11-16_joan_missing_damage_stage_palettes](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-16_joan_missing_damage_stage_palettes.md) | 1.0.33.1：贞德第二、三档战损保留变体调色 |
| [ISSUE_2026-11-16_joan_variant_background_stage_binding](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-16_joan_variant_background_stage_binding.md) | 1.0.32.13：可战损贞德切换变体后的背景与点击绑定 |
| [ISSUE_2026-11-16_live_resource_path_takeover_regression](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-16_live_resource_path_takeover_regression.md) | 1.0.32.14：修复预览加载抢占活跃背景资源路径的回归 |
| [ISSUE_2026-11-16_merchant_lifecycle_and_actor_scale](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-16_merchant_lifecycle_and_actor_scale.md) | 1.0.32.7：商店角色过小、商人首次切换及提供者隔离（2026-11-16） |
| [ISSUE_2026-11-16_postrun_native_memory_and_protected_skins](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-16_postrun_native_memory_and_protected_skins.md) | 1.0.32.11：结束对局后选角崩溃防护、战士兼容保护及有界预读 |
| [ISSUE_2026-11-16_repeated_switch_native_crash](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-16_repeated_switch_native_crash.md) | 1.0.32.6：连续换肤的重复挂载与 PCK 生命周期（2026-11-16） |
| [ISSUE_2026-11-16_vakuu_external_image_background_contract](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-16_vakuu_external_image_background_contract.md) | 1.0.32.15：采纳玩家提供的瓦库散图背景兼容修复 |
| [ISSUE_2026-11-17_blitz_dread_character_select](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-17_blitz_dread_character_select.md) | 2026-11-17：1.0.33.5 极速/恐惧选人界面同步 |
| [ISSUE_2026-11-17_byrdine_full_attack](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-17_byrdine_full_attack.md) | 异鸟成鸟攻击缺少全屏俯冲 |
| [ISSUE_2026-11-17_character_select_mode_presentation_parity](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-17_character_select_mode_presentation_parity.md) | 1.0.34.2：三种模式的选角背景与头像一致性 |
| [ISSUE_2026-11-17_custom_character_multiplayer_merchant](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-17_custom_character_multiplayer_merchant.md) | 第三方角色联机商店类型边界 |
| [ISSUE_2026-11-17_czn_style_ui_native_loading](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-17_czn_style_ui_native_loading.md) | CznStyleUI 原生加载边界 |
| [ISSUE_2026-11-17_defect_modes_and_merchant_variants](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-17_defect_modes_and_merchant_variants.md) | 1.0.34.3：鸡煲新模式背景与商人三场景 |
| [ISSUE_2026-11-17_embedded_crash_diagnostics](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-17_embedded_crash_diagnostics.md) | 随 Mod 自动运行的崩溃诊断 |
| [ISSUE_2026-11-17_extra_creatures_compendium](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-17_extra_creatures_compendium.md) | 其它图鉴新增奥斯提、异鸟成鸟和瓦皮 |
| [ISSUE_2026-11-17_fox_background_click](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-17_fox_background_click.md) | 狐猎背景点击和同名项目 |
| [ISSUE_2026-11-17_inrun_abstract_merchant_and_crossbow_modes](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-17_inrun_abstract_merchant_and_crossbow_modes.md) | 1.0.34.1：局内切换与极速弩手选角点击 |
| [ISSUE_2026-11-17_legacy_defect_preview](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-17_legacy_defect_preview.md) | 鸡煲娘化小预览仍显示原皮 |
| [ISSUE_2026-11-17_legacy_defect_scene_setup](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-17_legacy_defect_scene_setup.md) | 鸡煲娘化仅头像生效 |
| [ISSUE_2026-11-17_mlm_collectible_framework_context](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-17_mlm_collectible_framework_context.md) | 2026-11-17：1.0.33.6 新版 ModLaunchManager 隔离上下文兼容 |
| [ISSUE_2026-11-17_mlm_early_load_order_guard](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-17_mlm_early_load_order_guard.md) | 2026-11-17：1.0.33.7 MLM 加载顺序前置报错与一键整理 |
| [ISSUE_2026-11-17_mlm_priority_persistence_and_feedback](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-17_mlm_priority_persistence_and_feedback.md) | 2026-11-17：1.0.33.8 MLM 明确排序提示及一键整理持久修复 |
| [ISSUE_2026-11-17_naiwa_atlas_and_static_preview](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-17_naiwa_atlas_and_static_preview.md) | 奶蛙纹理错配与单图预览叠影 |
| [ISSUE_2026-11-17_private_merchant_alias_identity](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-17_private_merchant_alias_identity.md) | 2026-11-17：1.0.33.4 私有商人检测到但仍显示原版 |
| [ISSUE_2026-11-17_private_neow_merchant_discovery](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-17_private_neow_merchant_discovery.md) | 2026-11-17：1.0.33.3 私有目录商人皮肤漏识别 |
| [ISSUE_2026-11-17_stable_beta_single_binary](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-17_stable_beta_single_binary.md) | 正式版与测试版同一产物兼容 |
| [ISSUE_2026-11-17_three_fool_skin_packages](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-17_three_fool_skin_packages.md) | 三个愚者运行时皮肤无法识别 |
| [ISSUE_2026-11-17_unfocused_character_portraits](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-17_unfocused_character_portraits.md) | 未点击角色的选人头像未按保存皮肤显示 |
| [ISSUE_2026-11-17_vakuu_vanilla_easter_egg_preview](STS2-SkinChanger_修改件/docs/issues/ISSUE_2026-11-17_vakuu_vanilla_easter_egg_preview.md) | 2026-11-17：1.0.33.2 瓦库官方原图/彩蛋选项及图鉴预览 |
