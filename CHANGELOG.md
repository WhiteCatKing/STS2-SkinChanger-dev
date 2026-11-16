## 1.0.33.13（内测）

- 补齐鸡煲娘化的选人小模型预览：加载作者模型和待机动画，隐藏原版身体，避免执行战斗 Ready 生命周期。
- 真实 DLL 合同、预览接入边界与受保护皮肤回归通过；未实机验证画面。

## 1.0.33.12（内测）

- 修复鸡煲娘化 DefectSkinMod 在新版游戏中只有头像生效的问题：补做被旧参数补丁中断的模型/背景预载，并在选中时重放原背景回调。
- 真实 DLL 合同、商店生命周期及受保护角色回归通过；未实机验证画面。

## 1.0.33.11（内测）

- 修复含第三方角色的联机队伍进入商店时，切换器替代创建流程可能将 Node2D 强制转换为 NMerchantCharacter 的问题；混合队伍交回原生和角色作者流程，外观刷新同步避让。
- 自定义角色边界、商店生命周期及受保护角色测试通过；未复现玩家完整联机环境。

## 1.0.33.10（内测）

- 兼容狐猎血量换装 1.0.98 的选角背景点击入口：绕过旧版 SelectCharacter 参数失配，重用作者七档背景逻辑。
- 真实 DLL 调用与受保护角色回归通过；未运行游戏验证画面。

## 1.0.33.9（内测）

- 修复 CznStyleUI 3.1.4 被误识别为卡图皮肤、原始 DLL/PCK 无法正常加载的问题；同名副本也适用。
- 真实包目录回归与受保护角色检查通过；未运行游戏验证界面。

## 2026-11-17

### 1.0.33.8 —— MLM 强化排序提示与一键整理修复

- 排序错误正文直接强调正常使用必须将切换器放到 MLM 正下方第 2 位，不再埋在通用技术信息里。
- 修复忽略优先级只改内存、重启后失效的问题；通过 MLM 保存自己的标记，并在原生一键整理后校验和定位第 2 位。
- 真实 MLM 的旧行为负对照、保存重载、正文/按钮挂接、依赖及禁用边界和保护回归通过，未实机验证。详见修改件 docs/issues/ISSUE_2026-11-17_mlm_priority_persistence_and_feedback.md。

## 2026-11-17

### 1.0.33.7 —— MLM 内提前拦截错误排序并支持一键整理

- 清单声明最先加载，MLM 原生一键整理将切换器提前到本体之后。
- 加载到切换器时先检查前置皮肤及已隔离框架；错误顺序交给 MLM 自身加载错误界面，明确提示一键整理、置于第 2 位及重启。
- 不更改其他模组偏好；真实 MLM 原生排序/错误识别、原上下文专项及保护回归通过，未实机验证。详见修改件 docs/issues/ISSUE_2026-11-17_mlm_early_load_order_guard.md。

## 2026-11-17

### 1.0.33.6 —— 新版 ModLaunchManager 共享框架兼容

- 修复 MLM 将共享框架隔离到可卸载上下文后，皮肤设置注册失败及启动类型扫描中断的问题。
- 在框架实际加载前协调游戏上下文决策，覆盖预缓存/冷启动计划；保留 MLM 加载、重启移除及其他模组隔离行为。
- 实际 MLM DLL、CLR 负/正对照和保护回归通过，未实机验证；详见修改件 docs/issues/ISSUE_2026-11-17_mlm_collectible_framework_context.md。

## 2026-11-17

### 1.0.33.5 —— 极速/恐惧选人界面接入皮肤控件

- 三种官方选人界面共用皮肤下拉、包/合并、角色模型及骨妹专属奥斯提预览、头像和选择加载流程。
- 新模式联机大厅及开局应用同步；恐惧补充外观控件容器，读取原生子视口背景并保留灰度材质和模式规则。
- 真实三种界面入口专项、十字军/贞德及商人/瓦库保护回归通过；未实机验证布局，作者写死标准界面的专属面板仍需作者支持。详见修改件 docs/issues/ISSUE_2026-11-17_blitz_dread_character_select.md。

## 2026-11-17

### 1.0.33.4 —— 修复私有涅奥商人选中后仍显示原版

- 兼容作者按原始骨架路径查找商人的逻辑，使身体和手部的隔离资源可被正确识别，保留原有缓存隔离。
- 修复新版 Standard/Blitz 商店图鉴预览仍运行派生游戏初始化而报空引用的问题；真实商店初始化保持。
- 真实 DLL 匹配器、资源置换、两种房间预览门控及保护回归通过，未实机验证；详见修改件 docs/issues/ISSUE_2026-11-17_private_merchant_alias_identity.md。

## 2026-11-17

### 1.0.33.3 —— 修复一代涅奥替换商人漏识别

- 支持依靠商店房间回调创建私有 Spine 模型的商人包，雪霁改一代涅奥替换商人现归入商人皮肤。
- 保留作者身体、手和交互资源，补齐预览及局内切换的选中房间回调，不扩大其他内容包接管范围。
- 真实 DLL/PCK、商人生命周期、十字军/贞德与瓦库回归通过；未实机验证。详见修改件 docs/issues/ISSUE_2026-11-17_private_neow_merchant_discovery.md。

## 2026-11-17

### 1.0.33.2 —— 瓦库默认原图与默认彩蛋预览

- 修复其他图鉴瓦库官方预览空白，游戏默认明确显示原图，新增瓦库专属 ◈游戏默认彩蛋，沿用黄色默认样式并同步 15 语言。
- 选择保存及显式局内选择生效，跟随分类维持原生行为；内置彩蛋不进入自动优先级，外部散图修复与进度逻辑保持。
- 实际 PCK、选择/本地化及既有专项通过；未实机验证，详见修改件 docs/issues/ISSUE_2026-11-17_vakuu_vanilla_easter_egg_preview.md。

## 2026-11-16

### 1.0.33.1 —— 贞德三档战损维持所选变体配色

- 修复作者启动仅登记首档调色页后，第二/三档战损显示原色的问题；保护模块按所选包补齐缺页并继承当前调色权重，保持作者后续变体更新。
- 已有页复用，失败不发布半成品；保持无战损包能力、十字军动作和资源/退出保护。
- 增加真实作者四份权重、三档 24 次循环和各档调色 PNG 资源回归。未实机验证；详见修改件 docs/issues/ISSUE_2026-11-16_joan_missing_damage_stage_palettes.md。

## 2026-11-16

### 1.0.33 —— 创意工坊正式更新（内部 1.0.33.0）

- 将当前 .32.15 修复集合正式发布到原工坊条目 3787302680；公开三段版本与安装包同步，业务代码保持本轮修复结果。
- 汇总角色外观一致性、CZN、十字军/贞德、商店/火堆、卡图、瓦库及加载/缓存改进；更新 15 种语言说明、GitHub 链接和中英文更新日志。
- Steam 上传成功，15 语言逐一回读核验；三文件五处 SHA-256 一致，保护回归与瓦库专项通过。详见修改件 docs/issues/RELEASE_2026-11-16_workshop_1.0.33.md。

## 2026-11-16

### 1.0.32.15 —— 瓦库散图背景节点与彩蛋兼容

- 采纳玩家提供的补丁方案：先古散图场景保留 Image 显示皮肤，并加入官方 ConfigureBackgroundScene 所需的 TextureRect 空槽。
- 空槽显式隐藏、忽略纹理尺寸与鼠标输入，避免普通/彩蛋官方图覆盖皮肤或阻挡选项；不跳过原生彩蛋与进度逻辑。
- 实际游戏节点合同、新旧 DLL 正负对照和先古卡牌优先级检查通过；未实机验证。详见修改件 docs/issues/ISSUE_2026-11-16_vakuu_external_image_background_contract.md。

## 2026-11-16

### 1.0.32.14 —— 修复小预览抢占背景骨架路径的根因

- 修正 .12 引入的规范路径长期接管回归：新资源接管前保护仍被背景引用的旧资源路径，避免作者调色及战损归属判断读到空字符串。私有别名碰撞和临时恢复同样保护。
- 规范资源所有权统一通过一个入口；维持现有功能与资源预算、开局/退出保护。
- 同一新增回归对 .13 DLL 明确失败、修复版通过 200 次换主；纳入战士保护必跑，真实 13 变体及既有专项通过。未实机验证画面；详见修改件 docs/issues/ISSUE_2026-11-16_live_resource_path_takeover_regression.md。

## 2026-11-16

### 1.0.32.13 —— 贞德变体背景调色与点击战损绑定

- 可战损贞德每次重建后绑定当前包战损骨架并重施调色，持久接收层独占消费一次点击，保留作者开关。
- 三档共用稳定隔离图，移除公共缓存优先读取，防止两包共享路径串用；SFW 无战损、十字军动作及开局/退出保护保持。
- 新增真实两包全部变体战损能力、三档图复用与调色/点击边界回归；未启动游戏核验画面。详见修改件 docs/issues/ISSUE_2026-11-16_joan_variant_background_stage_binding.md。

## 2026-11-16

### 1.0.32.12 —— 角色外观绑定、预览恢复与空闲加载队列

- 显式选择的变体优先于提供者默认场景；角色外观共享有效选择标识，拒收过期结果，选人展示未完整完成则回滚选项。
- 角色预览不再被框架协作隐藏；奥斯提仅骨妹显示。保留拖动/停靠，补充窄屏布局与有界失败重试。
- 完整图按依赖优先隔离加载、同源不可变资源复用；不完整图继续深隔离，保留内存/缓存/退出保护。
- 文件预读分片调度，点击需求可插队，操作时暂停推测任务，鼠标移动不计；原生预加载同样让前台操作优先。保留 Common 超时和退出兜底。
- 真实 13 变体及十字军动作、新预览/队列/缓存归属专项与既有回归；未启动游戏验证渲染与帧耗。详见修改件 docs/issues/ISSUE_2026-11-16_character_binding_idle_loading.md。
## 2026-11-16

### 1.0.32.11 —— 战士皮肤保护、选角内存防护与有界预读

- 十字军/贞德及变体的 12 个兼容模块归拢保护目录，保留接口、原动作及停用标记；增加边界说明和一键真实包回归。
- 修复日志中原生分配/贴图加载失败后继续建立 Spine 的危险路径：解码预算、系统余量检查、失败冷却、停止重复回退，背景建立成功后再替换旧节点。
- 旧资源引用按组数/估算字节淘汰，准备包按键复用并限制会话生成量；离开对局不再删除 Godot 已挂载的联机运行包。
- 增加低开销选角文件预读和暖缓存提示，限制并发/时间/总量，UI 等待超时放行；保留 Common 超时、退出保存与防卡死兜底。
- 节点/依赖遍历、优先级计算、文件读循环增加有界检查。Release 0 警告 0 错误，真实 13 变体、十字军正式出招、安全/退出边界、逻辑/CZN/商店/卡图回归通过；未实机验证原生崩溃消除或帧耗。详见修改件 docs/issues/ISSUE_2026-11-16_postrun_native_memory_and_protected_skins.md。
## 2026-11-16

### 1.0.32.10 —— 补齐 CrusaderSkin 的正式出招钩子

- 修复局内晚加载 CrusaderSkin 只启用选牌前摇、未构造新版自定义战斗/房间订阅模型的问题；通过模型数据库补注册，重复激活保持单份监听。
- 在作者 Player/Creature 适用判断处检查实际玩家当前选择，阻止常驻钩子在切走皮肤后或同角色队友身上继续生效；保留作者动画映射和设置。
- Release 0 警告 0 错误；真实 Crusader DLL 三次补注册、正式 CardPlay/Smite、切走与队友归属检查通过，逻辑、CZN、商店回归通过。未启动游戏核验动作画面。详见修改件 docs/issues/ISSUE_2026-11-16_crusader_missing_combat_hooks.md。
## 2026-11-16

### 1.0.32.9 —— 新商店首次进入按当前皮肤重建商人

- 修复新商店仍实例化旧预加载商人、却套上当前皮肤位姿，导致原版商人缩小并偏到右下角。
- 基类/具体商店 Ready 后一次性完成独立商人外观事务，按房间弱键去重；跳过预览、原版及已离开的房间，保留商品/购买状态，不重跑房间玩法初始化。
- Release 0 警告 0 错误；逻辑、商人生命周期与新增 200 房间去重、CZN 实包及切换生命周期回归通过。未启动游戏验证画面。详见修改件 docs/issues/ISSUE_2026-11-16_initial_shop_cached_merchant.md。
## 2026-11-16

### 1.0.32.8 —— 修复局内角色皮肤被其他 CZN 模型覆盖

- 商店玩家重建明确绑定角色与节点归属，共享框架按 PlayerVisuals 引用解析，避免旧/新节点共存时用兄弟序号误判。
- 在作者事件回调和共享模型写入前双重检查所选角色皮肤，阻止 Hilde 等先行绑定自己的骨架；保留商人 NPC 修复。
- 核对战斗/火堆已有实际玩家归属检查，新增真实事件拒绝与 200 次嵌套归属恢复回归；Release 0 警告 0 错误，逻辑、CZN 实包、商人/切换生命周期、大卡专项通过。
- 作者确认上一版商人已正常；本版未启动游戏验证画面。详见修改件 docs/issues/ISSUE_2026-11-16_character_owner_presentation.md。
## 2026-11-16

### 1.0.32.7 —— 商店角色尺寸与商人首次切换

- 为商店/火堆角色恢复作者可识别的原始场景身份，底层资源继续隔离，避免场景路径检查跳过作者尺寸/姿态流程。
- 所选商人同步激活并在建节点前确保就绪，修复阿塔首次选择先建节点、后初始化的问题。
- 仅保留当前商人行为，移除按 Merchant 名称保留旧包、临时 PatchAll 和固定尺寸坐标纠偏通路；继续事务式替换外观，保留库存和购买状态，不模拟完整 SL。
- 验证：Release 0 警告 0 错误；LogicTests、商店/火堆场景身份与商人生命周期、CZN 实包、连续切换生命周期、大卡回归通过。未启动游戏，实际尺寸与连续商人切换尚未实机确认。详见修改件 docs/issues/ISSUE_2026-11-16_merchant_lifecycle_and_actor_scale.md。
## 2026-11-16

### 1.0.32.6 —— 修复连续换肤中的重复挂载与缓存包生命周期

- 作者确认 1.0.32.5 连续切换稳定原生崩溃；报告退出码 0xC0000005，Meirin 背景和预览完成后再次写出 106.9 MB 覆盖包，缺少原生转储。
- 修复本机同款预览作用域退出时无条件恢复本组、重复提权并挂载覆盖包的逻辑；保留联机不同选择与其他被影响分组的恢复。
- 固定本进程引用的 PCK 路径，禁止运行期裁剪；外部删除后改用新路径，避免旧原生目录项引用同名重建文件。新增挂载开始/完成日志。
- 验证：Release 0 警告 0 错误、LogicTests、500 次实际作用域释放和真实文件裁剪专项、CZN 实包与大卡回归通过。500 次为离线测试，未启动游戏；原生崩溃消除仍待实机确认。详见修改件 docs/issues/ISSUE_2026-11-16_repeated_switch_native_crash.md。
## 2026-11-16

### 1.0.32.5 —— CZN 系列统一兼容与切换加载安全

- 新增识别 Hilde/Heidemarie/Tressa/Chizuru/Meirin 的 CharacterSkinProfile 战斗骨架声明；Tiphera 保留玩法内容加载流程。
- 接入 CznCore 共享选择边界，解决仅卸载皮肤补丁仍残留背景/火堆/商店事件的问题；选角背景改为登记实际构造的展示对象再重放，预览入树前绑定战斗身体，关闭受管皮肤重复强制套用卡图的入口。
- 切换安全：不再全域扫描并读取第三方静态字段；后台预加载只接收不可变私有路径，避免公共资源在加载期间换归属。
- 验证：Release 0 警告 0 错误、LogicTests、五个真实包声明/探测/下拉目录、真实 CznCore Harmony 接口及选择边界、大卡与卡图契约回归通过。
- 限制：未启动游戏验证画面；本次闪退没有原生堆栈，仅移除已确认的危险路径，不能宣称所有闪退已消除。详见修改件 docs/issues/ISSUE_2026-11-16_czn_shared_presentation.md。
## 2026-11-16

### 1.0.32.4 —— 修复点开大卡后卡图与皮肤名称不一致

- 根因：游戏 UpdateCardDisplay 在 UpdateVisuals 后用 BigPortrait 覆盖皮肤卡图，原补丁仅同步下拉名称。
- 改法：在大卡显示最终后置步骤恢复所选卡图，再同步外部卡牌编辑器；适用于打开、翻卡及升级预览，不改配置和玩法。
- 验证：Release 0 警告 0 错误；LogicTests、大卡覆盖顺序回归和卡图包契约测试通过。额外布局复用测试因离线 Godot NodePath 原生调用崩溃未完成；未启动游戏验证画面。详见修改件 docs/issues/ISSUE_2026-11-16_card_inspect_portrait.md。
- 交付：三个运行文件同步四处，核对 SHA-256；按作者要求提交并推送 origin/main，不上传 Steam 工坊。
### 1.0.32.0 —— 修「启动黑屏 / 预加载死锁」

- 根因：皮肤切换器的后台预加载与游戏自身的 `IntroLogo`/`MainMenuEssentials`/`Common` 预加载并发，占满 Godot 的低优先级加载名额（线程数 × 0.3，8 线程机器只有 2 个），游戏侧资源永远排队 → 黑屏，或主菜单可见但后台加载永不完成。
- `Core/SkinPreloader.cs`：同一时间只在途 1 个请求、`LoadThreadedRequest` 关闭 sub-threads；等游戏 `Common` 预加载完成后才开始派发（`GameCommonPreloadCompletePatch` 监听 `AssetLoadingSession.Process`，60 秒超时兜底）；退出时只丢弃未派发任务，在途交给引擎收尾。
- 验收：编译 0 错误、`LogicTests` 通过；冷缓存连续 5 次启动全部正常（预加载 48 个、失败 0、无 `startup_hang.txt`）。

### 未采用（已回退到 1.0.31.99）—— 打开卡面列表后预览显示的不是所选那一张

- 现象：普通卡面包（与角色/其他卡面包无关），点开单卡皮肤下拉后，游戏里那张卡显示的是别的皮肤，既不是上方选中的，也不是切换前那张。
- 原因：`popup.IdFocused` 送来的是弹出菜单条目 ID，代码直接拿去 `popup.GetItemIndex(id)` 再当下拉行号用；弹出菜单与下拉列表在插入分组标题/跳过不可用项时行号与 ID 不是一一对应，取到的就是另一行。
- 改法：新增 `ResolvePreviewIndex(dropdown, popup, itemId)`，同时校验行号范围与「弹出项文本 == 下拉项文本」，对不上就拒绝这次预览并打 `[卡面预览]` 日志，宁可保持原样也不显示错的那张。
### 1.0.31.99 —— 正式版 v0.107.1 启动即崩（ModManifest 运算符）

- 玩家在工坊反馈：正式版 v0.107.1 上启动直接崩，`Method not found: 'Boolean MegaCrit.Sts2.Core.Modding.ModManifest.op_Inequality(ModManifest, ModManifest)'`，位置 `EssentialInitializationPatch`。
- 根因：`ManagedSkinModLoader.cs:149` 写的是 `mod.manifest != null`；DLL 编译时以 v0.113 的 `ModManifest`（重载了 `==`/`!=`）为准，绑定到了 `op_Inequality`，而正式版该类型只有普通 class 语义、没有运算符 → 运行时找不到方法。
- 改为 `mod.manifest is not null`（编译期就是引用判空），两个版本都能跑。
### 1.0.31.98 —— 游戏自己加载的程序集，切走时不再卸它的补丁

- `ManagedSkinModLoader` 新增 `AdoptedGameAssemblies`：走「复用游戏已加载程序集」这条路的包（十字军、狐猎等），其补丁是包自己的初始化器在游戏启动时装上的，不归我们管。
- `CaptureProviderPatches` 对这类程序集直接返回空集合，于是切换皮肤时的 `UnpatchProviderCallbacks` 不会再把它们卸掉——修十字军「换过一次皮肤后动作全丢」。
### 1.0.31.97 —— 修贞德点击换装失效与退出卡死兜底

- 程序集已被游戏加载过时不再重复加载：`ManagedSkinModLoader.GetOrLoadProviderAssembly` 在 `LoadFromAssemblyPath` 抛 `FileLoadException`（Assembly with same name is already loaded）时，按文件名复用 AppDomain 里已有的那一份；复用不到才退回纯资源模式。贞德「点击换装」失效就是因为它整包被回滚成资源皮肤。
- 退出兜底：`QuitCacheCleanup` 在 Quit 前缀启动看门狗，12 秒后进程仍未退出则记一条 `[退出诊断]` 并强制结束（存档在此之前已写完）。
### 1.0.31.96 —— 减少「切皮肤死锁 / 退出卡死 / 选角图标刷错误」

- 重建选人界面外观期间暂停皮肤场景预加载派发（`SkinPreloader.Paused`，由 `ContextualSkinControls.RunRefresh` 包裹）：主线程同步读资源时不再与后台线程加载并发，避免 Godot 资源加载器互等。
- 退出时先取消未派发/在途的预加载任务（`SkinPreloader.CancelAll()`，挂到 `QuitCacheCleanup` 的 Quit 前缀），避免引擎关停时等后台加载。
- `LoadCharacterSelectIcon` 先用 `ResourceLoader.Exists` 探测：v0.113 已没有 `char_select_random_character.png`，不再每进一次选角就刷十几条 `No loader found`。
### 1.0.31.92 —— 定版：十字军变体可用

- 清除临时诊断日志（`[变体诊断] 提供者后置前/后`、`[变体诊断] 场景对象`），保留 `[十字军变体] 已把场景写回变体` 一行便于日后排查。
- 保留修复本体：变体选项仍经 `ResolveVariantCombatPath` 写回变体场景，提供者 `ProviderId` 不回退为 null（保证其动画补丁保持激活）。

### 1.0.31.91 —— 场景对象诊断（未发布）
### 1.0.31.56 —— 商人位姿延迟矫正

- 换皮后包可能又把节点写回基础坐标，故在补回位姿后挂 3 个 0.05 s 的延迟检查（\GuardPoseForFrames\），发现仍停在 X < -500 就再写一次并打 \[商人] 延迟矫正商人位姿\。


## 2026-11-14

### 1.0.31.55 —— 商人位姿就地补回

- 1.0.31.53/54 的 \ApplyRememberedTransform\ 挂在 \TryRefreshMerchant\ 的换皮分支上，实际走的是更早的返回路径，日志里一次都没触发。
- 改为在 \RememberMerchantFigureProvider\ 内就地判断：量到基础坐标（X < -500）且已有记录时，立即把记录写回节点并打 \[商人] 已重放皮肤位姿\。


## 2026-11-14

### 1.0.31.54 —— 商人位姿记忆只存“像样”的值

- 切换瞬间节点常停在基础坐标（`-1122.7 / 0.4701`），原先的 `TryAdd` 把这种值当成记忆，重放等于把错误再写一遍。改为只在 `Position.X > -500` 时覆盖 `RememberedTransforms`。
- `ApplyRememberedTransform` 找不到记录时打 `[商人] 没有可重放的位姿（选择=…），保持节点原样。`，便于区分“没记住”与“记住但错”。


## 2026-11-14

### 1.0.31.53 —— 商人皮肤局内切换后重放位姿

- `Ui\MerchantRuntimeAppearance.cs`：`RememberMerchantFigureProvider` 把皮肤自己写下的商人位姿（位置/缩放）按 providerId 记进 `RememberedTransforms`；`TryRefreshMerchant` 成功换皮后调用 `ApplyRememberedTransform(refreshedVisual)` 重放，避免切换后退回基础坐标（位置 `-1122.7, -396.7`、缩放 `0.4701`）导致「模型放大并飞到左上角」。
- 不改动 1.0.31.51 的补丁摘除范围；未命中记录时保持原样。### 1.0.31.52 —— 商人骨骼变换按 source 隔离

三套商人包共用 manifest id `Merchant2CuteII`，`KeepsMerchantFigureTransform` 用
`ResolveBehaviorProviderId(selected)` 比较，把三份来源都归一到同一个 id，于是 a/b/c
互相认成"还是它"，旧骨骼变换不还原：a/b 跑到左上角，c 变原版。
现在按完整选择 id（含 `::source:`）比较，只有完全同一个来源才保留变换；
`MarkMerchantFigureProvider` 记录的也是原始选择 id 而非归一后的行为 id。
### 1.0.31.51 —— 商人包的行为补丁重新纳入摘除范围

1.0.31.31 给 `VisualPatchGuard` 加了「只摘本 Mod 接管过的程序集」前置，副作用是
Merchant2CuteII 自己的 9 个补丁（`NMerchantButton._Ready`、`NMerchantCharacter.PlayAnimation`
等）从「被摘」变成「一律保留」，和我们挂载的资源同时改商人骨骼/动画 → 商人回原版。
现在改为：受管程序集 **或** 面向商人场景的补丁都要摘。
### 1.0.31.50 —— 有场景范围时也拆分激活；慢包排到延后队列末尾

1. 1.0.31.48 的 4441 ms 真正原因是 `_runtimeProviderBehaviorScope != null` 让
   `deferNonFirstScreen` 直接为 false（全日志 `延后初始化: 0`），29 个包全同步激活。
   去掉这个前置条件：只受 `STS2SC_NO_DEFER` 控制，续档直达场景时同样拆分。
2. 延后队列按「已知慢包（AncientWaifus，实测 1.4 s）排最后」排序，主菜单先出来。
### 1.0.31.49 —— 回退 1.0.31.48 的启动期激活范围改动

1.0.31.48 实测反而把启动重建成 4441 ms（`激活行为补丁=4187ms`，延后队列为空），
比 1.0.31.47 的 1883 ms 差得多，说明"已选中优先"的判据让原本的拆分失效。
整段回退（`git checkout v1.0.31.47 -- SkinCatalog.cs SkinService.cs`），恢复 1.0.31.47 的行为。
### 1.0.31.48 —— 启动期只同步激活已选中的 provider，其余延后到选中时

1.0.31.47 的实测显示延后初始化仍是 `累计 3083 ms、最慢 1268 ms（AncientWaifus_Beta）`，
说明瓶颈是"某个包自己初始化就要 1.2 秒"，不是一帧塞太多。改成从源头少激活：
`SkinCatalog.GetSelectedRuntimeProviders(selections)` 收集当前每个分组已选中的 provider，
`ProviderActivationSchedulePolicy.Plan` 的 immediate 判据加上"或当前已选中"，其余继续走
延后队列；玩家在皮肤下拉里选中某个包时，既有的"按当前选择启用"路径会把它激活。
### 1.0.31.47 —— 清理临时诊断日志 + Info 日志预算 + 收紧延后初始化时间片

1. 删掉 `[奥斯提诊断]`、`[选角闸门]`、`[选角背景]` 全部临时日志（`DriverGateProbe.cs` 整个移除）。
2. `ModLog.Info` 加 24 MB 字符预算：超过后本会话不再输出 Info，只保留 Warn/Error 并提示一次
   （此前单份 godot.log 到过 39 MB）。
3. `DeferredProviderActivator.MaxTickMilliseconds` 45 → 12：延后初始化每个时间片只做更少
   provider，避免单帧卡顿（原累计 2989 ms、最慢 1356 ms）。
### 1.0.31.46 —— 奥斯提预览框改为面板内部判定（按日志定位）

日志证据（1.0.31.45）：
- `ShowCharacter 进入：角色=IRONCLAD` 时 `隐藏标记=False`，紧接着 `Refresh 面板存在=True 面板可见=False`，
  然后 `RebuildCharacterDisplay 分组=ironclad 隐藏标记=True` —— 标记是**画完之后**才设的，所以当帧已经显示了。
- 面板自身 `_Process` 每帧 `Visible = area != null && IsEnabled`，外部 `Visible=false` 下一帧就被改回去。

改法：`OstyModelPreviewControls.AllowedForCurrentCharacter` 静态开关，`ShowCharacter` 里按
"当前角色是否等于随从主人"直接写；面板的 `ShowOsty()` 与 `_Process` 都读它，不是骨妹就不显示。
### 1.0.31.45 —— 奥斯提预览框三处诊断日志（不改行为）

1.0.37~1.0.43 连续几版都没能让预览框稳定隐藏，先取证：在 `ShowCharacter`、
`RebuildCharacterDisplay`、`OstyModelPreviewControls.Refresh` 各打一行 `[奥斯提诊断]`，
记录进入顺序与当时的隐藏标记、面板存在/可见状态。
### 1.0.31.44 —— 回退奥斯提预览框改动，修复贞德战损切换抖动

作者实测 1.0.39~1.0.43 期间：①奥斯提预览框始终无法稳定隐藏（判定路径没被触发）；
②贞德战损切换出现"混乱画面→默认色→变体色"的循环抖动。
为避免继续扩大影响，把 `Ui\ContextualSkinControls.cs` 与 `Ui\OstyModelPreviewControls.cs`
整体退回 1.0.31.36 的状态（奥斯提只保留"非主人页面收起下拉"这一层，贞德恢复原样）。
### 1.0.31.43 —— 随从预览框的判定挪到角色切换入口

前几版把判定放在 `RebuildCharacterDisplay`，实测切角色时那条路径并不总是跑，于是状态陈旧。

这一版把 `CompanionPreviewAllowed` 的计算放进 `ShowCharacter(screen, character)`——
游戏的 `SelectCharacter` 每次点击都会进来——不是骨妹就当场收起预览框。
### 1.0.31.42 —— 奥斯提预览框改单一真源判定

作者实测：框出现后切角色仍会重现，说明 screen 上的标记被多条路径互相覆盖（`PopulateOsty` 会写、`RebuildCharacterDisplay` 也会写）。

这一版不再用 screen 标记做判定，改成 `ContextualSkinControls.CompanionPreviewAllowed` 单一静态真源：
每次 `RebuildCharacterDisplay` 按当前选中角色重算；`OstyModelPreviewControls.Refresh` 只看它，
不是骨妹就 `Hide` 并 return，任何"显示"调用都过不去。
### 1.0.31.41 —— 修奥斯提预览框"切角色后又冒出来"

1.0.40 里 `PopulateOsty` 每次都会无条件 `screen.SetMeta(CompanionHiddenMeta, false)`，
切角色时它跟着跑，把 1.0.39 设好的「隐藏」标记又擦成 false，于是随后的 `Refresh` 照样 `ShowOsty`。

这一版删掉那行无条件复位，改为：只有归属不符（非骨妹）时才置 true 并收起；骨妹页面由
`RebuildCharacterDisplay` 置 false。
### 1.0.31.40 —— 切角色时把奥斯提预览框按"刷新"处理

作者实测：框存在时只要切一下皮肤就消失了 —— 说明隐藏逻辑本身对，只是「切角色」这条路径没跑到它。

这一版在判定为隐藏时：①递归找出所有 `STS2OstyModelPreview` 节点逐个设为不可见（切角色时面板可能换了父节点）；
②再调一次 `OstyModelPreviewControls.Refresh(screen)`，让包内状态按同一套"刷新"逻辑复位。
### 1.0.31.39 —— 非骨妹页面立刻收起奥斯提预览框

1.0.37 只把"该不该显示"记在 screen 上，真正收起还是等 `Refresh`；而切角色时 `Refresh` 不一定被调用，
于是「选过骨妹再切别的角色」框会一直留着。

这一版在 `RebuildCharacterDisplay` 里判定为隐藏时直接 `Hide(screen)` 并把
`InfoPanel/STS2OstyModelPreview` 设为不可见——切换的瞬间就收掉，符合作者要求的四条：
选其他角色不显示、只有骨妹显示、每次切换都检测、销毁后回到"不显示"状态。
### 1.0.31.38 —— 退出耗时诊断（只加日志）

在 `Core\QuitExitCleanup.cs` 追加 `QuitTimingDiagnostics`：`NGame.Quit` 进入/返回各打一行，
返回时带耗时。用来区分「退出卡在我们自己的清理里」还是「日志写完后卡在引擎侧收尾」。
### 1.0.31.37 —— 奥斯提预览框与骨妹强绑定（作者要求）

1.0.31.36 的标记只在 `PopulateOsty` 里写，切到别的角色时那条路径不一定重跑，所以选过骨妹后再切别的角色，框又回来了。

这一版把判定挪到 `RebuildCharacterDisplay(screen, character, groupId)` 开头：每次重建角色展示时都按**当前选中的分组**
重写 `sts2_skin_companion_hidden`——只有 `groupId` 等于随从主人（骨妹）时才为 false。
`OstyModelPreviewControls.Refresh` 继续读这个标记，非骨妹页面直接 `Hide` 且不再 `ShowOsty`。
只作用于选人界面，局内不碰（局内其他角色仍可取到奥斯提）。
### 1.0.31.36 —— 随从预览框按页面归属彻底隐藏（作者要求）

1.0.31.35 只收了一次面板，但 `OstyModelPreviewControls.Refresh` 会无条件 `ShowOsty()`，
而且面板实际挂在 `InfoPanel/STS2OstyModelPreview` 下（1.0.31.35 查错了父路径）。

这一版：隐藏时在 screen 上打 `sts2_skin_companion_hidden` 标记，`Refresh` 开头读到就
`Hide` + 不再 `ShowOsty`；回到骨妹页面时标记被清掉，预览框照常出现。
### 1.0.31.35 —— 非骨妹页面连随从预览框一起隐藏（作者要求）

1.0.31.34 只隐藏了下拉，右侧的随从预览框还在。这一版在同一个归属判定里补上
`OstyModelPreviewControls.Hide(screen)`，并按 `OstyModelPreviewControls.PanelName`
（`STS2OstyModelPreview`）隐藏预览面板本身。
### 1.0.31.34 —— 随从下拉只在骨妹页面显示（作者要求）

作者要求：除骨妹（死灵之缚）之外的所有角色都不显示随从/手部下拉。

`Catalog\SkinCatalog.cs` 新增 `public string? CompanionOwnerGroup(string companionGroupId)`
（随从分组 → 主人分组，`CompanionGroups` 里 osty → necrobinder）。
`Ui\ContextualSkinControls.cs` 的 `PopulateOsty` 增加归属判定：选择器所在分组（`GroupMeta`）
不等于随从主人的分组时，直接把下拉隐藏并清空 `OstyGroupMeta`。
### 1.0.31.33 —— 主人页面不再列出随从专用皮肤（玩家反馈修复）

玩家在战士页选中「随从分组 osty」条目后整局卡死（1.0.31.32 已加兜底，这一版治因）。

`Catalog\SkinCatalog.cs` 新增 `public bool IsCompanionOnlyProvider(string providerId)`：不看当前选择，
只看声明——某个 provider 是否**只**被随从分组的选项引用。旧的
`IsCompanionOnlyProviderSelection(providerId, selections)` 要求选择已经落在随从分组里，
玩家在主人页选择时恒为 false，所以排除表一直为空。

`Ui\ContextualSkinControls.cs` 的选项循环（`:907`）加过滤：当前页面不是随从分组时，
跳过只服务随从分组的条目。
### 1.0.31.32 —— 随从分组不再占用主人选人槽位 + 整包重建兜底（玩家反馈修复）

玩家在战士页选中随从专用条目后整局卡死：
`NSpineAutoPlayer's parent's skeleton data must have exactly 1 animation. This has 6.`
栈为 `WithRuntimeResourcesForSelection` ← `RebuildMountedFullRuntimeCharacterDisplay` ← `RebuildCharacterDisplay`。

`Ui\ContextualSkinControls.cs:1991` 的整包运行时重建分支改为：
1. 随从分组（`SkinService.Catalog.IsCompanionGroup`）不再走整包重建，改走普通资源挂载；
2. 整段 try/catch 兜底，异常打 `[骨骼防护]` 后退回资源挂载，不再 `return` 卡死整局。
### 1.0.31.31 —— 不再摘除「我们没接管」的 provider 呈现补丁（玩家反馈修复）

`Core\VisualPatchGuard.cs` 摘补丁的判据原来是「程序集不是激活中的 provider」。对**排在本 Mod 前面、
由游戏原生加载**的皮肤包（玩家实测 Merchant2CuteII 排第 2、我们排第 51），`IsProviderAssemblyActive`
恒为 false，于是它的 8 个呈现补丁（含 `NMerchantButton._Ready`、`NMerchantCharacter.PlayAnimation`）
被我们摘掉，商人回原版。

现在加一道前提：只有**由本 Mod 加载/接管**的程序集（`ManagedSkinModLoader.IsManagedProviderAssembly`）
才可能被摘。没接管的包一律保留原样。
### 1.0.31.30 —— 外观覆盖包跨会话缓存命中（性能）

修复「每次启动都重写 587.8 MB 外观覆盖包」。原因是覆盖包签名里用了归档的**物理路径**，
而运行期生成的缓存包路径包含每会话的目录名与递增编号，同一个覆盖包每次启动都被算成新内容。

签名改用稳定归档标识：文件名去掉数字后的骨架 + 归档文件字节数 + 该资源字节数。
会话号变化不再影响命中；包内容变了仍然会失效并重写。

效果预期：`覆盖包[visual] 写盘 587.8 MB = 321 ms` 变为 `命中磁盘缓存`（<1 ms），
启动的「写包+挂载」337 ms 与 588 MB 磁盘写入一起消失。
### 1.0.31.29 —— 修「读档后商人仍显示旧皮肤」

读档进入商店时，房间在皮肤包资源挂载之前就 `_Ready` 完，之后再没有人回来补这一次刷新；只靠事件（`_Ready` / `SetCurrentRoom`）都不可靠，日志里连一行 `[商人]` 都没有。

改成在一直运行的每帧 tick（`MultiplayerSkinSync.Tick`）里看住当前 `NMerchantRoom`：每个房间实例只处理一次，最多重试 240 帧（约 4 秒），成功打 `[商人] 房间就绪后已套用当前选择：<选项>`，换房间或换选择自动重新开始跟踪。不依赖任何事件是否触发。
### 1.0.31.17 —— 一个补丁类装不上，不再拖垮整包行为补丁

`DefectSkinMod`（鸡煲娘化）的补丁类里有一个方法声明了 `characterModel` 参数，而 v0.113.0 把
`NCharacterSelectScreen.SelectCharacter` 改成只接收按钮（`SelectCharacter(NCharacterSelectButton)`），
Harmony 绑不上这个参数就抛异常；我们此前用的是整程序集 `PatchAll`，一处失败整包回滚，
于是它的局内换装、背景绘制补丁一起失效，只剩走规范路径的头像还能换。

改为 `PatchAllTolerant`：逐个补丁类走 `CreateClassProcessor(type).Patch()`，装不上的类跳过并记警告，
其余照常生效，最后汇总一行「共 N 个补丁类无法应用（游戏接口变更），其余 M 个已生效」。
### 1.0.31.16 —— 「这个包带不带规范背景」改用包自身 PCK 判定

1.0.31.15 的检查用 `ReadProviderFile`，而它会在「包没有该文件」时命中游戏本体和我方缓存覆盖包里的
同名文件 —— 而 `res://scenes/screens/char_select/char_select_bg_defect.tscn` 这类规范场景在游戏本体里
**每个角色都有**，于是判定恒为真，仍然去替换背景，鸡煲背景依旧退回原版。

新增 `SkinCatalog.ProviderShipsFile(groupId, optionId, relativePath)`：只在「不是游戏本体
`SlayTheSpire2.pck`、也不是 `cache\` 下的覆盖包」的档案里找到该路径时才算「包自带」。背景守卫改用它。
### 1.0.31.15 —— 不自带选人背景场景的皮肤包，不再用原版背景覆盖

`DefectSkinMod`（鸡煲娘化）、`succubusIroncladHpWear`（魅魔战士）这类包没有
`res://scenes/screens/char_select/char_select_bg_<角色>.tscn`，它们用自己的补丁把背景画上去
（`DefectAnimatedBg` / `AnimatedBg` 之下）。此前重建选角展示时会走「按规范路径取场景」的流程，
资源解析在覆盖包里查不到该路径就退回游戏原版场景，于是原版背景被装上去，包自己画的那套被冲掉，
表现为「选人界面只剩下方头像不一样」。

现在重建前先问一句 `ProviderShipsCanonicalBackground`：该选择是否真的带规范背景场景。
不带就跳过 `TakeOverCharacterSelectSceneCache` 与 `ReplaceCharacterBackground`，只刷新图标、
重放呈现步骤并保留包自己的背景，同时打一行
`[选角背景] 分组=… 没有规范背景场景，保留皮肤包自己的背景。`。
带规范背景的包（贞德、A 类战损等）行为完全不变。

同时把 `[选角背景]` 诊断也接到这条分支上，便于确认背景容器里到底还剩哪些节点。
### 1.0.31.14 —— 加「选角背景」诊断日志（不改任何行为）

`DefectSkinMod`（鸡煲娘化）的选人背景不由规范路径提供，而是它自己的 DLL 补丁在选人界面塞一个
`DefectAnimatedBg`（场景 `res://Scenes/DefectSelectBg.tscn`）到 `AnimatedBg` / `BgContainer` 之下；
我们的重建如果把这个节点整段换掉，背景就会退回原版。这一版只记录现状：重建之后打印
`AnimatedBg` 与 `BgContainer` 的每个子节点名、类型、`可见`、`在树可见`、子节点数，据此判断
「节点被我们删了」还是「节点还在但被隐藏」。
### 1.0.31.13 —— 加「选角点击」诊断日志（不改任何行为）

1.0.31.12 让骨妹包重新量到背景并成功挂上点击接收器（日志 `char-select toggle attached (necrobinder, stage 1/3, silenced 0 skin handler(s))`），
但点击仍无反应。这一版只在重挂之后把接收器节点本身的情况打出来，用来区分「点击没送到」还是「送到了被它自己的条件挡回」：
节点路径、尺寸、位置、自身可见性、是否在树中可见、`MouseFilter`、父节点可见性，都写进 `[选角点击]` 一行。
`DBWCharSelectClickCatcher` 若尺寸为 0 或父节点不可见，就说明点击根本不会落在它身上。
### 1.0.31.12 —— 让血量换装族皮肤包在选人界面重新量取背景

necrobinderHpWear 这一族包给选人背景挂「点击切战损」时，会先量当前背景 SpineSprite 的图集页尺寸，
只有等于自己预设尺寸才接管；量测发生在包初始化那一刻，而我们的皮肤资源包要等用户选中后才挂载，
于是它们量到原版图集、判定「没装对应皮肤」并永久放弃（日志 `atlas page 9132x5386 != 4757x4757`）。

新增 `Core\CharSelectPackRebindCompat.cs`：每次重建选角展示后，按当前选择的提供者程序集名找到它的
`TryReattachFromScreen` 并再调用一次，让它重新量一次背景；带 700ms 节流，包内部仍用自己的
`SkinSelectionGate.MenuAllowed()` 决定是否接管，未选中的包不会响应。
### 1.0.31.11 —— 加「选角闸门」诊断日志（不改任何行为）

血量换装那一族皮肤包（魅魔战士、骨妹「明前奶绿」等）在选人界面自己挂 `NCharacterSelectScreen.SelectCharacter` postfix 做点击切战损，但被它们自己的三道闸门挡住时**一行日志都不打**。这一版只加观测：

- `Core\SkinService.cs` 的 `GetSelectedRuntimeProvider(groupId)` 在返回值变化时打 `[选角闸门] 分组=… 选择=… 是运行包=… 返回=…`（皮肤包反射调这个方法判断「当前选中的是不是我」，返回 null 或名字不匹配它们就整个关掉）。
- 新增 `Core\DriverGateProbe.cs`：选角小模型刷新后，反射扫描所有 `*HpWear*` 程序集里的 `BattleWear.SkinSelectionGate.MenuAllowed()` 与 `BattleWear.WearActivation.PeekActiveDriver()`，把实测布尔值打进日志；同一分组只打一次。
- 副作用：无（只读探测 + 一次 Info 日志）。诊断完成后整个 `DriverGateProbe.cs` 与这两行日志应当删除。
### 1.0.31.10 —— 撤销 1.0.31.9（贞德变体方案暂缓）

1.0.31.9 让变体 id 回退到基础 option 查包根，实际效果是把贞德的「变体 + 战损」两条链搅在一起、点击切换更乱。整段回退，`SkinCatalog` 回到 1.0.31.8 状态。

贞德这类「一个主体既有变体又有战损」的皮肤暂不处理，先解包其他单一形态的战损皮肤，弄清它们自己的运行原理。
### 1.0.31.9 —— 修「战损变体的选人背景/头像全都一样」（真因：变体 id 查不到包根）

设置变体的选项 id 是 `<mod>::variant:<key>`，它的资产列表里只有配色文件、没有 `s1_*`，于是 `GetAssetRootDirectory` 返回 null，`ReadProviderFile` 也拿不到包内文件——`VariantPortraits` 因此读不到 `portrait_palettes/<变体>/s1_select_icon.png`，四个变体全退回规范图。

改法：`SkinCatalog` 新增 `StripVariantSuffix` 与 `FindOption`，`ReadProviderFile` / `GetAssetRootDirectory` 在按变体 id 查不到时回退到基础 option id（并不改动任何选中值）。

贞德包里确认存在 `portrait_palettes/1..3/s1_{run,select}_icon.png`，索引 0 是包自己的默认外观、本来就没有目录。
### 1.0.31.8 —— 修「战损变体的背景/头像全都一样」

1.0.31.3 为了让战损档能读到 `s2_/s3_*_data.tres`，把 `ResolvePlan` 里的 `directory` 直接改成了包根目录。`directory` 同时被拿去拼 `portrait_palettes/<变体>/s<阶段>_select_icon.png` 与配色路径，于是四个变体的选人背景、选人头像全退回到同一张默认图——**同一个变量被两件事共用**，是这次倒退的根因。

改法（做减法，不加新条件）：

- `StagePlan` 拆出 `DataDirectory`（只用于找 `s<阶段>_<角色>_data.tres`），`Directory` 恢复为「当前骨架所在目录」，供 `portrait_palettes`/配色路径使用；
- 删除 1.0.31.7 的「包自命名背景兜底」（一次都没触发，日志里背景路径始终是规范路径），`SkinCatalog`/`ContextualSkinControls` 回到 1.0.31.5 的干净版本。

战损档点击仍从包根读数据文件，变体区分恢复。### 1.0.31.7 —— 选人背景：规范路径优先 + 包自命名兜底（排除别名）

1.0.31.4 的兜底把贞德弄坏了：它选到了 `res://custom/…` 这类只在挂载期有效的别名路径，实例化失败后背景直接没了。

这一版改成两条硬约束：

- 只有该皮肤包**不带**规范 `char_select_bg_<角色>.tscn` 时才启用兜底；
- 兜底时**拒绝** `res://custom/` 与 `res://sts2_skin_runtime/` 开头的别名路径。

同时把上一版写错的诊断字段（`包内自带背景` 恒为 False，因为把包根目录拼在了路径前面）换成直接输出最终选中的背景路径。
### 1.0.31.6 —— 皮肤缓存总量兜底 + 选角诊断日志

玩家磁盘被 %APPDATA%\SlayTheSpire2\Gurio.SkinChanger\cache 撑到 63 GB：缓存按「一次游戏会话一个目录」存放，清理只在正常退出时执行，崩溃/卡死/强退会整份留下，越积越多（单目录 1–4 GB）。

- Core\SkinChangerStorage.cs：新增 TrimCacheBudget(long maxBytes)，按最后写入时间从最旧开始整目录删除，直到总量回落到预算内；正在使用的会话目录永不删除。
- Core\SkinService.cs：启动清理后追加一次 TrimCacheBudget(8 GB)，并打出删除数量、释放量、剩余量。
- Ui\ContextualSkinControls.cs：RebuildCharacterDisplay 增加一行 [选角诊断]，输出分组、当前选择、规范背景路径、以及该皮肤包是否自带这份背景资源。纯日志。
### 1.0.31.5 —— 撤销 1.0.31.4 的选人背景识别放宽

1.0.31.4 把选人背景改成「先找包里的 `*selectbg*` 场景、否则退回规范路径」，实测把贞德的选人背景也弄丢了（回退后恢复），问题也没真正解决，因此整段撤销，回到 1.0.31.3 的选人背景逻辑。

- `Catalog\SkinCatalog.cs`：删除 `FindCharacterSelectBackgroundScene`。
- `Ui\ContextualSkinControls.cs`：`RebuildCharacterDisplay` 恢复只按 `res://scenes/screens/char_select/char_select_bg_<角色>.tscn` 取背景。

结论记录：故障机器人（DefectSkinMod）的背景用的是它自己命名的 `res://Scenes/DefectSelectBg.tscn`，靠「放宽文件名匹配」这条路走不通，需要按包注册（provider contract）显式声明背景场景，另开改动。
### 1.0.31.4 —— 选人界面背景识别放宽（支持包自己的场景命名）

v0.113.0 之后，故障机器人（DefectSkinMod）这类把选人背景放在自己命名下（es://Scenes/DefectSelectBg.tscn）的皮肤包，选人界面一直被换回原版背景：我们只认游戏的规范路径 es://scenes/screens/char_select/char_select_bg_<角色>.tscn。

- Catalog\SkinCatalog.cs：新增 FindCharacterSelectBackgroundScene(groupId, optionId, characterId)，优先用规范名，找不到时在该选项自己的资源里挑 *selectbg* / char_select_bg_* 的 .tscn（同名多份时取路径最短的一个）。
- Ui\ContextualSkinControls.cs：RebuildCharacterDisplay 里背景路径先问上面这个方法，取不到再退回规范路径；后续的资源收集、整包快照重建都沿用同一个路径。

贞德（JoanOfArcIroncladHpwear）两份资源本来就在规范路径上，走的是第一个分支，行为不变，可作回归基准。魅魔战士（succubusIroncladHpWear）包里根本没有选人背景场景，这一版不会让它凭空多出背景。
### 1.0.31.3 —— 修「商人皮肤刷新报『独立皮肤资源不是场景』」
根因：v0.113.0 把商店房间场景从 `res://scenes/rooms/merchant_room.tscn` 拆成 `res://scenes/rooms/standard_merchant_room.tscn` 与 `res://scenes/rooms/blitz_merchant_room.tscn`，旧路径在新版游戏包里已不存在。本 Mod 的 `MerchantRoomScenePath` 仍是旧路径，运行时 overlay 里取不到 PackedScene，于是在 `SkinService.InstantiateRuntimeScene` 抛 `InvalidOperationException: 独立皮肤资源不是场景：res://scenes/rooms/merchant_room.tscn`，面板里切换商人皮肤一律失败。
- `Ui\MerchantRuntimeAppearance.cs`：新增 `StandardMerchantRoomScenePath` / `BlitzMerchantRoomScenePath` 与 `ResolveMerchantRoomScenePath(NMerchantRoom?)`（按活体房间类型名选场景，`NBlitzMerchantRoom` 走 blitz，其余走 standard）；`MerchantRoomScenePath` 保留为 standard 默认值，供预览预热与无房间实例的场景使用。
- `TryCreateCurrentMerchantRoom` 改用 `ResolveMerchantRoomScenePath(preloadCreatedRoom)`，主路径与兜底路径都用同一个场景。

### 1.0.31.2 —— 修「商人皮肤无法切换」
根因：v0.113.0 把商店房间拆成 `NStandardMerchantRoom` / `NBlitzMerchantRoom`，`NMerchantRoom.MerchantButton` 变成 **abstract 属性**，真正的 `<MerchantButton>k__BackingField` 只存在于具体子类。本 Mod 仍按 `typeof(NMerchantRoom)` 取该字段 → 反射得到 null，`TryRefreshMerchant` 直接以 `merchant visual fields unavailable` 退出，面板里点商人皮肤不生效。同时两个子类都覆写 `_Ready()` 并在第一行调 `base._Ready()`，挂在基类上的作用域捕获早于子类建好自己的 `%MerchantButton` / `%Inventory`。
- `Ui\MerchantRuntimeAppearance.cs`：新增 `ResolveMerchantButtonField(object)`，按实例真实类型（含基类链）缓存解析 `<MerchantButton>k__BackingField` 或 `_merchantButton`；预览、刷新、回滚、预览原生引用四处改用它，不再依赖基类的静态字段。
- 新增 `MerchantRoomConcreteReadyPatch`：同时挂 `NStandardMerchantRoom._Ready` 与 `NBlitzMerchantRoom._Ready` 的 `Priority.Last` postfix，等具体房间完成自身初始化后再跑一次 `ScopePartTakeover`。

### 1.0.31.1 —— 让「撑在地上的鸡煲」继续可用（奥斯提皮肤包兼容层）
根因：工坊 `3820801911`（`OstyDefectImpostor`）把全部补丁塞进一次 `Harmony.PatchAll()`，只要有一个目标解析不到，它的 catch 就 `UnpatchAll()` 把自己整个停掉。v0.113.0 把 `NCreature.OstyScaleToSize(float)` 改名成 `ScaleAndPositionOsty(float, double)`，它仍按旧名打补丁 → 日志 `[OstyDefectImpostor] Patch unavailable; original visuals retained`，皮肤选项随之消失。
- `Core\OstyDefectImpostorCompat.cs`（新增）：在 `AccessTools.Method(Type, string, Type[], Type[])` 上挂 postfix，只有当查找结果为 null、且请求的正是 `NCreature.OstyScaleToSize` 时，才返回改名后的 `NCreature.ScaleAndPositionOsty`；其它任何查找原样放行。
- 定位该重载必须用 `typeof(AccessTools).GetMethod(..., [typeof(Type), typeof(string), typeof(Type[]), typeof(Type[])], null)`：`AccessTools` 有两个同名 `Method` 重载，用 `AccessTools.Method(typeof(AccessTools), ...)` 去查会二义性命中失败、静默返回 null。
- 类型判定按 `FullName + 程序集名` 比较，不用 `typeof(NCreature)` 引用比较：每个模组各有 AssemblyLoadContext，同一个游戏类型在不同 ALC 里是不同的 `Type` 实例，引用比较永远不相等、重定向不会触发。
- `Entry.cs`：在 `PengoTarotFontCompat.Install(harmony)` 之后调用 `OstyDefectImpostorCompat.Install(harmony)`，早于任何第三方模组的初始化器。

### 1.0.31.0 —— 适配游戏 v0.113.0（修「启动即致命错误」）
根因：v0.113.0 删除了 `MegaCrit.Sts2.Core.Models.SingletonModel`。本 Mod 挂在 `OneTimeInitialization.ExecuteEssential` 上的补丁体直接引用该类型，补丁方法 JIT 时抛 `TypeLoadException`，游戏一启动就进「致命错误」画面（日志栈 `OneTimeInitialization.ExecuteEssential_Patch3` ← `NGame.GameStartup`）。
- `Core\ProviderModelDiscovery.cs`：改用 `typeof(ModelDb).Assembly.GetType("MegaCrit.Sts2.Core.Models.SingletonModel", throwOnError:false)` 反射取类型；缺类型时只打一次日志并跳过「给皮肤包补注册单例模型」，改由 `ModelDb` 自身类型发现负责。补丁体不再持有该类型的编译期令牌。
- `Ui\StatefulCardCosmeticPatches.cs`：删除已消失的 `MegaCrit.Sts2.Core.Models.Singleton` 命名空间引用。
- `Ui\MerchantRuntimeAppearance.cs`：`NMerchantRoom.Create` 已删除 → 房间补丁改挂 `NMerchantRoom._Ready`（「丢弃启动预加载房间并按选择重建」随静态工厂入口一起退役，已留注释；玩家外观仍由同文件 `MerchantRoomPlayerAppearancePatch` 负责）。`MerchantInventory.AddRelicEntry` 已删除 → 假商人预览改用 `MerchantInventory.CreateForNormalMerchant(previewPlayer)`。
- `Ui\CharSelectStageCycle.cs`、`Ui\ContextualSkinControls.cs`：`NCharacterSelectScreen.SelectCharacter` 只剩 `NCharacterSelectButton` 一个参数 → 改从 `charSelectButton.Character` 取角色（游戏内部本来就这么取），补丁类恢复安装。
- `Core\MultiplayerSkinSync.cs`：`StartRunLobby.ChangeCharacter` 形参改名 `isRotatingDread`（语义为轮转恐惧），该值只用于日志措辞。
- 启动期健壮性：`Entry.GetLoadableTypes` 提为 `internal`，`ModelTypeCompatibility` 枚举 GameAssembly 类型改走它；`EssentialInitializationPatch` 的前置/后置步骤各自 try/catch 并打 Error 日志。第三方包引用已删除类型时，异常不再顺着启动流程把玩家挡在「致命错误」外。
- 新增 `Core\OstyDefectImpostorCompat.cs`：工坊 `3820801911`（撑在地上的鸡煲）仍按旧名 `NCreature.OstyScaleToSize` 打补丁，失败即 `UnpatchAll` 自己。这里给 `AccessTools.Method` 挂后置，把旧名请求重定向到 v0.113.0 的 `ScaleAndPositionOsty`；取重载用精确反射，避免同名重载二义性返回 null。
- 战损档：`Ui\CharSelectStageCycle.cs` 的 `ResolvePlan` 改用皮肤包根目录（`SkinCatalog.GetAssetRootDirectory`）拼 `s<stage>_<group>_data.tres`，修掉「点角色切战损」每次都读到 0 个资源的问题。
- 按作者要求，撤销此前为工坊 `3747527467` 做的延迟注册/作用域门闩等兼容改动（7 个文件回到 .30.200 状态），只保留本次版本适配。
- 验证：构建 0 错误（仅 3 个既有 CS8601）、LogicTests 末行 `Skin Changer logic policy tests passed.`、启动进主菜单显示 v0.113.0、日志无「补丁类安装失败」、贞德变体与战损档点击切换恢复（作者实测确认）。
---

## 2026-11-12

### 1.0.30.201 —— 撤销 .200 的完整性放宽（当时被误判）
- 背景：作者复测后报「奥斯提=Orchis 时骨妹本体也被换成 Orchis」，据此把启动完整性判定改回严格版 `IsFullRuntimeProviderFullySelected`。
- 真相（同日晚确认）：串皮与这条判定无关，是工坊 `3759170855`（STS2-OrchisNecrobinderSkinFix）被 Steam 重新下载并启用，它声明依赖 Orchis，触发"被依赖则交回原加载器"的安全阀，Orchis 因此全局 `PatchAll`。
- 代价：严格判定会让「骨妹与奥斯提不同款」的组合每次启动都把角色皮肤重置成原版。DLL 前缀 `FBB05DC56986`。

### 1.0.30.200 —— 精准修「退出重进变原皮」
- 根因：启动清理把随从分组的选择算进"完整运行时提供者是否完整"，`necrobinder=Orchis` + `osty=别的` 时 Orchis 被判不完整 → 卸载补丁 + 重置选择（日志 `已将 … 的 1 个不完整联动选择恢复为游戏原版`）。
- 改法：判定改用 `Catalog.IsFullRuntimeProviderSelectedIgnoringCompanions(providerId, Config.Selections)`（随从分组有自己的挂载事务，不参与角色提供者的完整性判定）；只动这一行判定。
- 验证：重启后配置保持、无「不完整联动选择」；骨妹 = HpWear、奥斯提 = Orchis，无串皮。DLL 前缀 `25D454CC2125`（后来按同一逻辑重编为 `0251B42C98E7`）。

### 1.0.30.199 —— 启动完整性判定（第一版）/ 选角图标缺失预检
- 就是把 .200 的判定改动 + `LoadCharacterSelectIcon` 的 `ResourceLoader.Exists` 预检一起做的那版；预检用于修 `char_select_random_character.png` 每帧报错拖到界面无响应。DLL 前缀 `007E9809680B`。

### 1.0.30.198 —— Orchis 本体头顶蓝火（局内）修好
- 节点：`Visuals/HeadBoneNode/SteppedFireMix_dark`（原版 `necrobinder.tscn` 就有；`NNecrobinderVfx` 只负责显隐）。
- 根因：`SkinService.ApplySelectedVisualPostfix` 的 `allowed` 要求"完整运行时提供者必须全组选中"，局内 osty 分给「撑在地上的鸡煲」时 Orchis 被判不完整，骨妹分组的视觉后处理整体不回放。
- 改法：① `allowed` 只看本组选择；② `TryRebuildCreatureVisuals` 在挂载后重放一次 `ApplySelectedCreatureVisualPostfix`（热重建实例不在游戏创建路径上）。
- 验证：`后置重放前后 HeadBoneNode：False → False`（修前 `True → True`），截图蓝火消失。DLL 前缀 `74E84AAE0273`。

### 1.0.30.197 —— 奥斯提坐标被 Orchis 摆位补丁带走
- 反编译 Orchis：`NCreature.OstyScaleToSize` 的 Prefix 会把宠物根节点 tween 到主人身旁，`Creature.CreateVisuals` 后置还会加 `OstyCombatPositionOffset`。
- 改法：`MonsterRuntimePresentationPolicy` 增加 `MonsterVisualCallback.OstyPositioning`；`ManagedSkinModLoader.ProviderOwnsOstyPet(providerId)` 判定；`GetSelectedCreatureProviderIds` 在宠物组自带提供者时不再重放主人的 `NCreature._Ready` 行为。
- 结论：静止 18 s 根节点恒定，结束回合波动 ≤0.7 px——此前的"漂移"是切换皮肤后重建身体的滑入缓动。DLL 前缀 `ADF4844F0F8C`。

### 1.0.30.196 —— 三项小修
- `RefreshCompanionGroup`：osty 选「游戏默认」时先 `RebuildSummonBodyForImpostor` 再返回，显示真正的原版身体。
- 局内伙伴下拉去掉「随机皮肤」项。
- 回退 .195 的「整包已在栈顶就跳过提权」——它会让分层覆盖层把整包反压下去，Orchis 的 VFX 变回原版。DLL 前缀 `B55A65605044`。

### 1.0.30.195 —— 名字守卫 + 联机重建去重
- `CompanionDisplayNameGuard`：`LocString.GetFormattedText` 的 Postfix，只处理 `OSTY.name`；osty 选 midori 时保留「坚果」，否则从游戏原包 `res://localization/<lang>/monsters.json` 读回「奥斯提」（第三方改的是本地化表原文，`GetRawText` 拿不到原名）。
- `MultiplayerSkinSync.ReconcileRemoteAppearances` 按选择签名去重（`LastAppliedSelectionSignatures`）。DLL 前缀 `1C71092D424F`。

### 1.0.30.194 —— 修「鸡煲显示原皮」
- 根因：.193 的别名兜底自己 `Instantiate` 原版身体，绕过了 `OstyDefectImpostor` 挂在 `MonsterModel.CreateVisuals` 上的 Postfix。
- 改法：别名场景只用来 `PreloadManager.Cache.SetAsset` 登记到规范路径（`ForceCanonicalSceneFromAlias`），身体仍走 `CreateVisuals`，独立 Mod 才能接管。DLL 前缀 `EE0E7DD58D83`。

### 1.0.30.193 —— 别名兜底（当时以为修好了「猫 → 鸡煲」）
- `EnsureGameSceneOnTop` + `ProbeSceneHasSpine` + 私有别名路径 `res://__skin_changer_base/{hash}/{file}.tscn`；包名每次递增，避免同名 `LoadResourcePack` 不重新压栈。DLL 前缀 `3AE9F0004A8F`。

### 1.0.30.192 —— 第三个根因：按路径的资源缓存
- 结论：`AssetCache`（`PreloadManager.Cache`）按原样路径字符串长期持有，`ResourceLoader.CacheMode.Reuse` 同理；猫形态一旦进缓存，后面压包栈/写覆盖包都没用。
- 改法基础：`EnsureGameSceneOnTop` 写 `base_scene_{hash}_{seq}.pck` + `CacheMode.Replace` + `SetAsset`；`ProbeSceneHasSpine` 判断规范路径是否已真是原版 Spine。

---

## 2026-11-11

### 1.0.30.191 —— 校正包装配顺序：选中项覆盖把提权包的猫壳写回
- 复现规律：进游戏直接选「撑在地上的鸡煲」正常；先选猫再切鸡煲显示猫；先选别的再切鸡煲正常。
- 改法：`LargeProviderMountPlan` 带 `PromotedProviderIds`；选中项覆盖之后，对"属提权包、属游戏原包、且归属分组当前没选这个提供者"的路径用 `BuildGameBaselineOverlay` 写回（日志 `[提权] 已按游戏原包还原 N 条其它分组的规范路径`）；缓存刷新条件改为 `overlayPlanRebuilt || mountedDeltaFiles > 0`。DLL 前缀 `8F72FC9168DF`。

### 1.0.30.190 —— 场景缓存是第二根因（没修好）
- 新增 `RefreshCachedCreatureVisualScenes`：对覆盖层里的 `/creature_visuals/*.tscn` 用 `CacheMode.Ignore` 重读并 `SetAsset`。
- 结果：缓存确实刷了，但刷到的还是猫（选中项覆盖写回），留给 .191。DLL 前缀 `A1B93E01E4E6`。

### 1.0.30.189 —— midori 的奥斯提：预览 + 抢占鸡煲（第一轮）
- 依赖闭包允许空格：`SkinCatalog.EmbeddedResourcePathRegex` 不再把空格当终止符，`catimage/猫召唤 2/**` 回到闭包（600 → 648 文件），修掉 `No loader found resource: …猫召唤 2_00000.png`。
- 预览框支持场景型奥斯提：`Ui/OstyModelPreviewControls.cs` 增加 `TryLoadSceneForCurrentSelection / TryShowScene / MeasureSceneBounds / FitScene`。
- 校正包只按游戏原包还原提权路径（`BuildGameBaselineOverlay` 限定 `_baselineIndexes[0]`）。DLL 前缀 `6D61E87DCB60`。

### 1.0.30.188 —— 上述三项的开发版（问题单第 18 节起点）
- 与 .189 同一批改动，.189 为对外交付版本。

### 1.0.30.187 —— 收尾清理
- 删掉临时性能探针与调试残留，保留 `[外观诊断]` 一族日志与文档。四个部署位置一致，DLL 前缀 `64FAA0383C70`。

### 1.0.30.186 —— 每切一次写 630 MB → 分层增量
- 覆盖包改成增量层（`SkinService.MountOverlayDelta`）：换肤 49–171 ms、多数情况 0 写盘，修掉连续切换时的闪退/磁盘打满。

### 1.0.30.185 —— 随从换肤比例收尾 + 两条新发现
- 9.5-2 结案：代码改了，但"换随从皮肤偏小"在当前包里无法复现。
- 新发现（未修）：局内把奥斯提切到 `midori_black` 会把场景加载炸掉。

### 1.0.30.184 —— 9.5 遗留收尾
- Yuzuki 包作废、随从换肤不再沿用旧基准缩放。

### 1.0.30.183 —— 全量重建复测
- 冷启动 2.1 s / 热缓存 106–135 ms / 换一个提供者 15 ms ⇒ 结论：热缓存有效，不做"分组产物缓存"。

### 1.0.30.182 —— 拆分批量激活
- 启动时把"立即激活"与"延后激活"拆开（`DeferredProviderActivator`），启动阻塞 6.8 s → 3.45 s。

### 1.0.30.179 / .180 / .181 —— 性能探针、预加载、依赖闭包缓存
- `.179`–`.181`：`Core/PerfTrace.cs` 性能探针、`Core/SkinPreloader.cs` 预加载、依赖闭包缓存。

### 1.0.30.178 —— 覆盖包跨会话缓存
- 新增 `Core/OverlayCacheStore.cs`，覆盖包命中缓存时不再重新组装。

### 1.0.30.177 —— 随从兼容通用化 + 加载性能 + 写盘兜底
- 新增 `Core/SummonModCompat.cs`：随从皮肤只分"资源型 / 运行时型"两种情况，统一处理。
- 加载性能优化；磁盘满时不再崩溃（写盘兜底）。

### 1.0.30.176 —— 两个包的奥斯提部分补完
- 改 `Catalog/SkinCatalog.cs`：补齐场景依赖闭包与"随从私有场景映射"，让只提供私有路径皮肤的包（Yuzuki 等）也能被 osty 分组识别。
- 工具修复：`CatalogInspect --visual-overlay` 统计口径。DLL 前缀 `DD1FA37F1A53`。

### 1.0.30.175 —— 一批"外观显示≠实际"的修复
| 问题 | 修法 |
| --- | --- |
| 骨妹被强制变成 Orchis | 环境侧禁用 `3759170855`；代码侧 `GetActiveRuntimeProviders` 排除"只在随从分组被选中"的提供者 |
| 面板数值与实际模型不符、重置没反应 | 重置时一并删 `GroupTransformMemoryKey`（`__group_default__`） |
| 换肤后面板绑定旧节点 | `CharacterAppearanceScreen.RefreshTargetCreatureReference()` |
| 面板被手牌遮挡 | 面板移入 `CanvasLayer(105)` |
| 局内奥斯提下拉项比选人界面少 | 补 `OstyImpostorBridge.Options` 与"随机皮肤" |
| 局内切奥斯提无效 | 改走 `ApplyCompanionSelection`；独立 Mod 皮肤不重建；重建后 `ReapplyCompanionImpostorSkin()` |
| 战斗中奥斯提点不到 | 「下一个目标 ▶」按钮 `SelectNextAppearanceTarget` |
| 奥斯提 Orchis 皮肤带原版火焰 | `ApplySelectedVisualPostfix` 对随从分组只看本组选择 |
| 「撑在地上的鸡煲」不显示 | `OstyImpostorCompat.OwnsSummonBody()` 两条重建路径提前 return |
| 局内 Orchis → 鸡煲不即时 | `RebuildSummonBodyForImpostor()` + `OstyImpostorBridge.RefreshLiveSummons()` |
| 选角预览里 Orchis 本体带原版火焰 | `FrameworkModelPreview` 补 `ApplySelectedCreatureVisualPostfix` |

---

## 2026-11-10

### 1.0.30.167 —— 深夜：Orchis 全局补丁真因 + 随从视觉后处理
- `OrchisNecrobinderSkinMod.dll` 在 `Init` 里 `PatchAll()`，`Creature.CreateVisuals` 后置无条件换骨；它能绕过隔离是因为 `3759170855` 的 manifest **声明依赖它**（安全阀"被依赖则交回原加载器"）→ 禁用该 Fix Mod 后日志恢复 `已隔离皮肤提供者 Orchis…原 PCK 未全局挂载`。
- `ApplySelectedVisualPostfix` 对随从分组不再要求"全部分组选中"。
- 重建召唤物后必须再 `OstyImpostorBridge.ApplyQuiet` 一次（`ReapplyCompanionImpostorSkin`），否则精灵被重建覆盖。
- 遗留：随从组选「游戏默认」时基线资源数=0。

### 1.0.30.161 —— 「重置没反应」真凶 + 随从局内可换肤
- `SetCharacterCombatTransform` 写非默认值时同时写 `__group_default__`，重置只删 `options[optionId]` ⇒ 面板显示 100% 而模型还是旧值；改为默认值分支一并删组级记忆。
- 新增 `SkinService.CanSelectCompanionSkin`（解耦后允许随从换肤），选择走 `ApplySelection("osty", …)`。

### 1.0.30.159 —— 骨妹本体被换回旧包 + 滑条不动 + 邪教徒纹理
- `BuildOverlay` 恢复无条件基线回写；新增 `RequestFullOverlayReassert`（多分组提供者被替换/降级为随从专用时，下次重建自动扩成全部分组，日志 `外观一致性保护：…`）。
- `CharacterAppearanceScreen.RefreshTargetCreatureReference()` 重新绑定活节点。
- 新增 `Core/ProviderRuntimeCacheReset.cs`：选择变化时清空提供者程序集里的静态缓存（名字含 cache/applied/hurt）。
- 新增 `[外观诊断]` 日志族。

### 1.0.30.158 —— 随从包泄漏 + 外观面板缩放/重置
- 新增 `SkinCatalog.IsCompanionOnlyProviderSelection`：只被随从分组选中的提供者做 `replaceFiles:false` 低优先级挂载、永不置顶，`BuildOverlay` 跳过它的基线回写。
- 选择框改成绕矩形中心按 `size` 缩放；`OnTransformChanged` 排队期间先存值、`OnQueuedSelectionFinished` 再应用；设置面板移入 `CanvasLayer(105)`。

### 1.0.30.157 —— 双贞德包并存 + 外观变体 + 点角色切战损
- 两个贞德包（0.3.5-S / 0.3.6）可并存：`ProviderMenuGateCompat` 放行同契约变体的门控，`VariantHotkeyPoller` 以 0.05 s 轮询接管快捷键。
- 外观变体改为"设置型变体"；局内外观同步；已知缺口：选人头像不随战损档变化。

### 1.0.30.138 —— 回退一个有副作用的优化
- 某个省 ~1 ms 的优化会导致选人背景/预览/局内模型颜色不更新，已回退。

### 1.0.30.125 / .124
- `.125` 局内左上角头像随变体（a/b）变化，实机通过。
- `.124` 爆衣：b 在 16/80 时进入第 2 档，实机通过。

---

## 2026-11-09

### 1.0.30.107 —— 战士贞德双包（0.3.5-S / 0.3.6）兼容
- 区分"外观差分快换"（包自己设置，默认 `=`）与"点角色切形态"（`CharSelectCycle`，仅 b 有），避免把后者当成前者。
- a 的快捷键本来被包自己的 `SkinSelectionGate` 关掉；用 `ProviderMenuGateCompat` 放行，`CharSelectCycle.Attach/OnInput` 前缀保证点击轮换只属于包自己。
- `Core/VariantHotkeyPoller.cs` 接管快捷键（UI 会吃掉 `_input`），只在该包的变体家族里前进一格。

### 1.0.30.102 —— 骨妹四档血量换装（necrobinderHpWear）修复 + 奥斯提独立下拉
- 骨妹四档：根因是在"运行中的" SpineSprite 上换骨架，引擎要断上千条连接（实测 1182/2554/3487）→ 原生崩溃。改为 `pristine.Duplicate()` 预建独立精灵、进树前设好骨架、换档只切 `Visible`；新增 `BattleWear/SpineWrappers.cs` 用原生 `Variant` 调用即时释放。
- 皮肤切换器：新增「奥斯提」独立下拉 + 右侧预览框，把角色皮肤与奥斯提皮肤解耦；修选人头像串图、Orchis 背景动画不播。
- 把"拦截第三方在选人界面接管外观"的补丁提取成通用件。

---

## 2026-11-07

### 1.0.27.88 —— 垫层按相机缩放反向补偿
- 帝皇蟹房 `GetCameraScaling()=0.75`，canvas 0 的垫层只盖 75%；改为"垫层树里没有 `CanvasLayer` 时才按 `1 / GetCameraScaling()` 反向缩放"，有 CanvasLayer 的包不动。其它缩放 0.85/0.9 的 Boss 一并修好。

### 1.0.27.87 —— 别抽图层，整块垫在本体之后
- `.86` 只替换 `Layer_XX` 槽位导致帝皇蟹上半身被背景盖住（CZN 战场在 `CanvasLayer -1`，天空 z_index 2400+）。
- 改为：保留选中战场的完整节点（含它自己的 CanvasLayer），插到遭遇背景第 0 位，隐藏遭遇自己的 `Layer_XX` 布景，保留本体与 `Foreground`。

### 1.0.27.86 —— 尝试只换图层（失败，被 .87 取代）

### 1.0.27.85 —— 帝皇蟹 Boss 房卡死
- 根因：「背景即本体」的 Boss（`EncounterModel.HasCustomBackground`）被背景功能当成风景 `QueueFree()`，`Crusher.AfterAddedToRoom` 撞上已释放对象，战斗初始化中断。
- 修法：遭遇自带背景一律不替换（`EncounterOwnsBackground` / `CurrentEncounter`，三处入口统一走 `CanApplyActBackground`）；`CombatCrabSkinPatch` 写钳子引用前校验节点有效性。

### 1.0.27.84 / .83 —— 两项在干净树上重做
- `.84` `RemoveStaleProviderCharacterSelectControls` 增加 `character` 参数，运行时提供者跳过"清理 CharacterSelectOptionsPanel"（CZN 背景切换工具条）。
- `.83` `ShouldDeferCharacterIconResourceToExternalRuntime` 去掉 `GetRuntimeImagePath != null` 要求，非资源型运行时提供者一律让位（CZN 选人栏/顶栏头像）。

### 1.0.27.82 / .78
- `.82` `FrameworkSelectPresentationBridge.Reapply(...)` 调用；`.78` `VariantSceneAvailable` 守卫；两者是后来重做 .83/.84 的干净基线。

---

## 怎么回退

```powershell
cd 'D:\Storage\Desktop\工作2'

git log --oneline                     # 找版本对应的提交
git show <哈希> --stat                # 看那次动了哪些文件
git revert <哈希>                     # 撤销某次提交（推荐，历史不断）
git checkout <哈希> -- <文件路径>      # 只把某个文件恢复到旧版本
git checkout v1.0.30.200              # 回到某个标签看代码（只读，改之前先回 main）
```

回退代码之后必须：重建 → `LogicTests` 通过 → 复制到 4 处部署目录 → 重启游戏确认 `godot.log` 里的 `内测版本` 与提交相符。
2026-11-12 之前的版本没有 git 标签，只有本文件与交接文档的记录；那几天要"回退"只能改代码或改用 `D:\工作2026.11.10`、`D:\工作2026.11.11` 里的历史安装包。




























