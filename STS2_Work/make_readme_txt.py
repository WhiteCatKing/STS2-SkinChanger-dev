import json
import pathlib
import re

DST = pathlib.Path(r"D:\STS2皮肤切换器_中文副本")
MAP_PATH = pathlib.Path(r"D:\STS2_Work_name_map.json")
mapping = json.loads(MAP_PATH.read_text(encoding="utf-8"))

DIRS = {
    "": "整个项目",
    "兼容层": "兼容层工程",
    "兼容层/Thunninoi皮肤管理器兼容": "旧皮肤管理器兼容替代",
    "文档": "开发文档",
    "文档/开发代理": "开发代理说明",
    "文档/计划": "功能计划",
    "文档/超能力": "设计与计划归档",
    "文档/超能力/计划": "实施计划归档",
    "文档/超能力/规格": "设计规格归档",
    "皮肤切换器主程序": "Mod 主程序",
    "皮肤切换器主程序/目录": "皮肤目录构建",
    "皮肤切换器主程序/核心": "核心逻辑",
    "皮肤切换器主程序/数据": "内置静态数据",
    "皮肤切换器主程序/PCK归档": "PCK 读写",
    "皮肤切换器主程序/程序集属性": "程序集元数据",
    "皮肤切换器主程序/界面": "游戏内界面",
    "测试": "离线测试",
    "测试/框架集成测试": "原管理器集成测试",
    "测试/逻辑测试": "纯逻辑测试",
    "测试/运行时测试": "运行时测试",
    "测试/脚本路径夹具": "脚本路径测试夹具",
    "测试/设置夹具": "设置测试夹具",
    "测试/创意工坊启动夹具": "工坊启动测试夹具",
    "工具": "开发与发布工具",
    "工具/目录检查": "皮肤目录检查工具",
    "工具/框架兼容探测": "框架兼容探测工具",
    "工具/在线缓存探测": "在线缓存探测工具",
    "工具/PCK检查": "PCK 检查工具",
    "工具/创意工坊清单导出": "工坊清单导出工具",
    "工具/创意工坊发布器": "工坊发布工具",
    "创意工坊发布资料": "Steam 工坊发布素材",
    "创意工坊发布资料/截图": "工坊截图原图",
    "创意工坊发布资料/截图/上传用": "工坊截图上传版",
}

DIR_DESC = {
    "": "项目根目录。这里放项目说明、开发约定、构建配置和全局 SDK 版本；真正的代码在下面各个子目录。\n本副本只翻译了目录名与文件名，所有文件内容均从原项目逐字节复制，未改一个字；原项目 D:\\STS2-SkinChanger 保持原样。",
    "兼容层": "兼容层工程。编译出一个与旧“皮肤管理器”同名同接口的替代 DLL（thunninoiSkinManager.dll），让依赖该管理器的皮肤 Mod 仍能启动。",
    "兼容层/Thunninoi皮肤管理器兼容": "旧皮肤管理器（thunninoi's Skin Loader and Manager）的兼容替代实现；只提供接口、数据类与注册表，不接管画面。",
    "文档": "开发文档。包括已完成/已定案清单、待办反馈、功能计划、多语言发布说明和设计规格。",
    "文档/开发代理": "写给开发代理/AI 的代码库索引与工坊清单维护说明。",
    "文档/计划": "单个功能或修复的实施计划。",
    "文档/超能力": "较大的功能设计与实施计划归档。",
    "文档/超能力/计划": "按日期归档的功能实施计划。",
    "文档/超能力/规格": "按日期归档的功能设计规格。",
    "皮肤切换器主程序": "Mod 主程序。入口、清单、工程文件和全部 C# 源码；下面按目录、核心、界面等分目录。",
    "皮肤切换器主程序/目录": "皮肤目录构建：扫描 PCK、DLL、卡图包和散装图片，把资源归类到角色、卡牌、怪物、先古、商人和事件。",
    "皮肤切换器主程序/核心": "核心逻辑：配置、选择、资源覆盖包挂载、框架/版本兼容、多人同步、创意工坊与 GitHub 来源、退出清理。",
    "皮肤切换器主程序/数据": "随 DLL 一起嵌入的静态数据（内置创意工坊清单）。",
    "皮肤切换器主程序/PCK归档": "Godot PCK 归档的读取/写入实现，用于按需生成资源覆盖包。",
    "皮肤切换器主程序/程序集属性": "程序集版本与元数据。",
    "皮肤切换器主程序/界面": "游戏内界面与控件：选角、其它图鉴、局内“外观”、卡牌/怪物优先级、主题编辑器、工坊面板及 Harmony UI 补丁。",
    "测试": "离线测试。用替代引擎和真实游戏程序集验证逻辑，不启动游戏。",
    "测试/框架集成测试": "与原皮肤管理器 DLL 的集成测试（独立进程加载真实 DLL，替换 IO/日志边界）。",
    "测试/逻辑测试": "不依赖 Godot 场景的纯逻辑测试：GitHub 策略、安装台账、路径、排序等。",
    "测试/运行时测试": "运行时测试：加载真实 sts2/Godot 程序集，覆盖目录、挂载、UI 契约、多人、工坊、存储等。",
    "测试/脚本路径夹具": "测试夹具：模拟皮肤 Mod 重复注册 Godot 脚本路径。",
    "测试/设置夹具": "测试夹具：模拟带设置数据的皮肤 Mod。",
    "测试/创意工坊启动夹具": "测试夹具：模拟创意工坊启动入口。",
    "工具": "开发与发布辅助工具，不随 Mod 发布。",
    "工具/目录检查": "用真实游戏与 Mod PCK 构建皮肤目录，打印识别结果，便于回归对比。",
    "工具/框架兼容探测": "探测框架程序集/接口兼容性。",
    "工具/在线缓存探测": "检查在线皮肤缓存与包内容。",
    "工具/PCK检查": "检查 PCK 目录结构，或复制少量文件验证写入器。",
    "工具/创意工坊清单导出": "只读核验本地工坊包并刷新内置清单的重启标记。",
    "工具/创意工坊发布器": "构建并上传 Steam 创意工坊条目的工具（不会自动上传）。",
    "创意工坊发布资料": "Steam 创意工坊发布素材：封面、截图、标题/描述与 15 语言文案。",
    "创意工坊发布资料/截图": "工坊展示截图原图（PNG）。",
    "创意工坊发布资料/截图/上传用": "工坊展示截图压缩版（JPG，上传用）。",
}

DESC = {
    # root
    "README.md": "项目总说明：完整功能清单、使用方法、配置与退出清理、构建/测试命令、兼容策略。接手前先读这一份。",
    "AGENTS.md": "开发约定：构建环境、常用命令、验证与发布规则、日志与目录限制。",
    "PRODUCT.md": "产品定位、目标用户、品牌气质、设计原则与无障碍要求。",
    "Directory.Build.props": "所有工程共享的构建属性：游戏程序集路径、obj 缓存隔离、默认引用正式版 v0.107.1。",
    "global.json": "把 .NET SDK 锁定在 9.0.100（rollForward=latestFeature）。",
    ".gitattributes": "Git 换行/文本属性配置。",
    ".gitignore": "Git 忽略规则（bin/obj、缓存、本地配置等）。",
    # main
    "STS2SkinChanger\\Entry.cs": "Mod 入口（[ModInitializer]）：打印内测版本，安装私有依赖解析器，初始化路径/框架兼容/托管加载器，并逐类安装 Harmony 补丁；还包含 ModelDb 重复模型兼容补丁。",
    "STS2SkinChanger\\ProviderSettingsApi.cs": "暴露给皮肤 Mod 作者的设置接口（兼容旧管理器）。",
    "STS2SkinChanger\\SkinChanger.json": "Mod 清单：ID、名称、作者、描述、三段公开版本 1.0.19、四段内测版本 1.0.19.11、has_pck=false、has_dll=true、最低游戏版本 0.107.1。",
    "STS2SkinChanger\\STS2SkinChanger.csproj": "主工程：net9.0、AnyCPU、程序集名 Gurio.SkinChanger，引用 sts2/GodotSharp/0Harmony/Steamworks.NET，并把兼容层 DLL 复制到输出目录。",
    "STS2SkinChanger\\Properties\\AssemblyInfo.cs": "程序集版本/文件版本/信息版本（1.0.19.11）。",
    "STS2SkinChanger\\Pck\\PckArchive.cs": "Godot PCK 归档读取/写入器：解析目录、按需取文件、生成覆盖包。",
    "STS2SkinChanger\\Data\\workshop-catalog.json": "内置创意工坊清单（皮肤条目、类型、对象、是否可免重启等），编译时嵌入 DLL。",
    # catalog
    "STS2SkinChanger\\Catalog\\SkinCatalog.cs": "皮肤目录核心：扫描 PCK/DLL/卡图包、识别角色/卡牌/怪物/先古/商人/事件分组、读取声明式呈现配置、生成选择事务。文件很大，是接手时最需要通读的一份。",
    "STS2SkinChanger\\Catalog\\SkinCatalog.Events.cs": "皮肤目录的事件部分：事件资源表、事件分组与覆盖判断。",
    "STS2SkinChanger\\Catalog\\SkinCatalog.Multiplayer.cs": "皮肤目录的多人部分：远端皮肤来源匹配与协议数据。",
    "STS2SkinChanger\\Catalog\\SkinCatalog.Workshop.cs": "皮肤目录的工坊部分：工坊包识别与清单映射。",
    "STS2SkinChanger\\Catalog\\CardArtPack.cs": "纯素材卡图包（*.cardartpack.json）的数据模型与图片解码。",
    "STS2SkinChanger\\Catalog\\CardArtPackScanner.cs": "扫描并读取 Card Art Editor 导出的纯素材卡图包。",
    "STS2SkinChanger\\Catalog\\LooseCardPortraitPackScanner.cs": "识别“整目录都是散装卡面图片”的皮肤提供者目录。",
    "STS2SkinChanger\\Catalog\\ManagedCharacterAssetReplacementScanner.cs": "扫描 DLL 皮肤对角色资源的替换声明，归入角色分组。",
    "STS2SkinChanger\\Catalog\\ManagedMonsterSceneScanner.cs": "扫描 DLL 皮肤对怪物场景/资源的替换，归入怪物分组。",
    "STS2SkinChanger\\Catalog\\ManagedCardPortraitReplacementScanner.cs": "扫描 DLL 皮肤对卡图/卡面的替换。",
    "STS2SkinChanger\\Catalog\\ManagedCardPresentationScanner.cs": "扫描 DLL 的卡牌呈现（卡框、遮罩、材质、布局）意图。",
    "STS2SkinChanger\\Catalog\\ManagedCardSurfaceScanner.cs": "扫描 DLL IL，识别卡牌表面/横幅等呈现补丁。",
    "STS2SkinChanger\\Catalog\\FrameworkSkinContractScanner.cs": "扫描框架（RitsuLib/BaseLib 等）的皮肤契约声明。",
    "STS2SkinChanger\\Catalog\\DirectCharacterRuntimeTargetScanner.cs": "识别“运行时直接替换角色骨骼/贴图”的皮肤目标。",
    "STS2SkinChanger\\Catalog\\ManualCharacterVariantScanner.cs": "识别手动角色差分（配色、部件显隐）。",
    "STS2SkinChanger\\Catalog\\ProviderCardBehaviorScanner.cs": "识别提供者的卡牌行为/呈现补丁。",
    "STS2SkinChanger\\Catalog\\RuntimeMonsterVisualModeScanner.cs": "识别怪物运行时的视觉模式（整包/部件）。",
    "STS2SkinChanger\\Catalog\\SkinPackagePaths.cs": "皮肤包（[P] bundle）路径与引用解析。",
    "STS2SkinChanger\\Catalog\\StatefulCardArtContract.cs": "有状态卡图（升级/状态变化）的声明契约。",
    "STS2SkinChanger\\Catalog\\CardLayoutVariantPolicy.cs": "卡牌布局变体（beta 图等）的归类策略。",
    "STS2SkinChanger\\Catalog\\AppearanceResourceFilePolicy.cs": "判断某个资源文件是否属于外观资源。",
    "STS2SkinChanger\\Catalog\\ProviderInstanceIdentityPolicy.cs": "提供者实例身份/来源 ID 的规范化规则。",
    "STS2SkinChanger\\Catalog\\SkinProviderPatchEvidencePolicy.cs": "根据补丁目标判断一个 DLL 是否真的提供外观。",
    # core
    "STS2SkinChanger\\Core\\ManagedSkinModLoader.cs": "启动时的 Mod 扫描器：读取 mods/工坊目录，识别皮肤提供者，决定哪些 PCK/DLL 被隔离、哪些留给游戏原加载器；负责目录构建与提供者根目录。",
    "STS2SkinChanger\\Core\\SkinService.cs": "换肤总服务（最大文件）：保存选择、构建资源事务、生成/挂载覆盖包、刷新界面、处理随机皮肤、先古优先级、预读商店资源等。",
    "STS2SkinChanger\\Core\\SkinService.AncientPriorities.cs": "先古“皮肤优先级”分类：all_ancients 分组、启用/排序、跟随分类/手动选择、把结果写回选择。",
    "STS2SkinChanger\\Core\\SkinService.EventPriorities.cs": "事件地区优先级：按地区/全部事件登记、启用/排序、计算每个事件最终来源。",
    "STS2SkinChanger\\Core\\SkinService.CharacterSkinBundles.cs": "皮肤包（[P]）的角色皮肤应用逻辑。",
    "STS2SkinChanger\\Core\\SkinService.CharacterSkinBundleRuns.cs": "皮肤包对局的独立外观记录：开局应用、继续游戏恢复、退出还原。",
    "STS2SkinChanger\\Core\\SkinService.Workshop.cs": "皮肤服务中的工坊部分：订阅、下载、热加载判断。",
    "STS2SkinChanger\\Core\\SkinConfig.cs": "配置模型（skin_changer.json）：选择、优先级、预设、皮肤包、外观参数、先古优先级、被尊重的加载顺序等；含旧配置迁移与规范化。",
    "STS2SkinChanger\\Core\\SkinChangerStorage.cs": "配置/缓存/备份的读写与原子保存。",
    "STS2SkinChanger\\Core\\SkinChangerPaths.cs": "user://Gurio.SkinChanger 下各文件/目录的路径定义与初始化。",
    "STS2SkinChanger\\Core\\FrameworkCompatibilityLayer.cs": "框架兼容层：对接 RitsuLib/BaseLib 等皮肤框架，转换旧接口调用。",
    "STS2SkinChanger\\Core\\ProviderAssemblyCompatibility.cs": "皮肤 DLL 的跨游戏版本兼容：JIT 核验、接口桥接、失败隔离。",
    "STS2SkinChanger\\Core\\ProviderAssemblyCompatibility.Rules.cs": "跨版本接口转换规则表。",
    "STS2SkinChanger\\Core\\ProviderAssemblyCompatibility.Audit.cs": "跨版本兼容的只读审计输出。",
    "STS2SkinChanger\\Core\\ProviderManifestCompatibility.cs": "旧依赖清单/最低游戏版本的等价迁移与核验。",
    "STS2SkinChanger\\Core\\MultiplayerSkinSync.cs": "多人皮肤同步：消息协议、发送/接收开关、按玩家应用、失败回退。",
    "STS2SkinChanger\\Core\\OnlineSkinCache.cs": "在线皮肤包缓存：下载、大小限制、zip 安全检查、sha256 校验、清理。",
    "STS2SkinChanger\\Core\\GitHubSkinService.cs": "GitHub 皮肤来源服务：搜索 topics、读 sc.info、安装/更新/卸载、缓存。",
    "STS2SkinChanger\\Core\\GitHubSkinCatalog.cs": "GitHub 仓库清单模型与合并。",
    "STS2SkinChanger\\Core\\GitHubFolderSwap.cs": "GitHub 安装的“旁路目录 + 清单改名”热替换/下次启动删除。",
    "STS2SkinChanger\\Core\\GitHubInstallLedger.cs": "GitHub 安装台账（目录、清单 ID、版本、包 sha256）。",
    "STS2SkinChanger\\Core\\SkinWorkshopService.cs": "游戏内创意工坊服务：内置/社区清单、订阅、搜索、详情。",
    "STS2SkinChanger\\Core\\WorkshopSubmissionCodec.cs": "SCM1/SCM2/SCM3 投稿码的编解码与校验。",
    "STS2SkinChanger\\Core\\WorkshopSubmissionScanner.cs": "扫描已订阅皮肤并生成投稿码片段。",
    "STS2SkinChanger\\Core\\ModLocalization.cs": "15 种界面语言的文本表与查找逻辑（最大的本地化文件）。",
    "STS2SkinChanger\\Core\\ModThemeSettings.cs": "主题设置模型与持久化。",
    "STS2SkinChanger\\Core\\ModThemeRuntime.cs": "主题运行时：把颜色/透明度/模糊应用到控件。",
    "STS2SkinChanger\\Core\\PartTakeoverVisualScope.cs": "部件接管：只接管作者包覆盖的部件，其余保持原版。",
    "STS2SkinChanger\\Core\\VisualPatchGuard.cs": "视觉补丁守卫：未选中时卸载提供者的视觉补丁。",
    "STS2SkinChanger\\Core\\QuitCleanupCoordinator.cs": "退出清理协调：确认退出后先清缓存，再走游戏原退出流程。",
    "STS2SkinChanger\\Core\\ExternalCardVisualBridge.cs": "外部卡牌视觉桥：把第三方卡牌呈现接入本 Mod 的覆盖层。",
    "STS2SkinChanger\\Core\\EventSkinRuntime.cs": "局内事件皮肤的刷新与恢复。",
    "STS2SkinChanger\\Core\\EventRegionPolicy.cs": "事件按地区分组（含 __global__ 全部事件）。",
    "STS2SkinChanger\\Core\\EventSkinPriorityPolicy.cs": "事件/先古优先级设置类与“第一个启用来源”解析。",
    "STS2SkinChanger\\Core\\SkinWorkshopService.Community.cs": "社区投稿清单的读取、核验与缓存。",
    "STS2SkinChanger\\Core\\ManagedSkinModLoader.cs" : "启动时的 Mod 扫描器：读取 mods/工坊目录，识别皮肤提供者，决定哪些 PCK/DLL 被隔离、哪些留给游戏原加载器；负责目录构建与提供者根目录。",
    # ui
    "STS2SkinChanger\\Ui\\CharacterAppearanceScreen.cs": "局内“外观”界面：点击角色/怪物/先古/商人后打开设置面板，切换皮肤与模型/血条参数。",
    "STS2SkinChanger\\Ui\\CharacterAppearanceRuntime.cs": "外观界面的运行时状态与场景刷新。",
    "STS2SkinChanger\\Ui\\ContextualSkinControls.cs": "选角/图鉴通用的换肤控件与角色预览重建。",
    "STS2SkinChanger\\Ui\\AncientCompendium.cs": "“其它图鉴”主界面：先古、商人、生物、事件分类。",
    "STS2SkinChanger\\Ui\\AncientCompendiumScreen.cs": "其它图鉴的屏幕实现：先古优先级面板、局内外观入口开关、事件地区选择。",
    "STS2SkinChanger\\Ui\\CardSkinControls.cs": "卡牌总览/单卡检视的皮肤选择与优先级控件。",
    "STS2SkinChanger\\Ui\\MerchantRuntimeAppearance.cs": "商人/假商人外观的运行时替换；文件末尾含商店房间预读补丁。",
    "STS2SkinChanger\\Ui\\SkinWorkshopPanel.cs": "游戏内创意工坊面板主实现。",
    "STS2SkinChanger\\Ui\\SkinWorkshopPanel.Rows.cs": "工坊面板的物品行/分页复用。",
    "STS2SkinChanger\\Ui\\SkinWorkshopPanel.Filters.cs": "工坊面板的类型/地区/对象/来源筛选。",
    "STS2SkinChanger\\Ui\\ModThemeEditor.cs": "主题编辑器面板。",
    "STS2SkinChanger\\Ui\\MonsterSkinPriorityControls.cs": "怪物皮肤优先级控件。",
    "STS2SkinChanger\\Ui\\LoadOrderWarning.cs": "加载顺序提示与自动排序；调用 LoadOrderRespectPolicy 尊重玩家手动排序。",
    "STS2SkinChanger\\Ui\\LoadOrderRespectPolicy.cs": "F35 新增：区分“玩家刻意留在本 Mod 之上”的提供者与需要自动排序的新提供者。",
    # compat
    "compat\\ThunninoiSkinManagerCompat\\SkinContracts.cs": "旧管理器接口契约的替代定义。",
    "compat\\ThunninoiSkinManagerCompat\\SkinData.cs": "旧管理器的皮肤数据类。",
    "compat\\ThunninoiSkinManagerCompat\\SkinRegistry.cs": "旧管理器的注册表实现。",
    "compat\\ThunninoiSkinManagerCompat\\ThunninoiSkinManagerCompat.csproj": "兼容层工程文件，输出 thunninoiSkinManager.dll。",
    # docs
    "docs\\COMPLETED_FIXED.md": "已完成/已定案清单（18 项），列出问题、结论、对应版本/提交与反馈来源。",
    "docs\\FEATURE_REQUESTS_PENDING.md": "待拍板的新功能需求清单（皮肤分场景、血量自动切换等）。",
    "docs\\WORKSHOP_FEEDBACK_2026-08-30.md": "工坊/邮件反馈待办（F 编号）、处理顺序与收集规则。",
    "docs\\publish-github-skin-mod.md": "GitHub 皮肤 Mod 投稿指南（英文）。",
    "docs\\publish-github-skin-mod.zh-Hant.md": "GitHub 皮肤 Mod 投稿指南（繁体中文）。",
    "docs\\投稿GitHub皮肤Mod说明.md": "GitHub 皮肤 Mod 投稿说明（简体中文）。",
    "docs\\agents\\codebase-memory.md": "代码库记忆索引（codebase-memory-mcp）的使用说明。",
    "docs\\agents\\workshop-catalog.md": "内置工坊清单的维护与重启标记核验说明。",
    "docs\\plans\\2026-09-06-ancient-card-priority.md": "角色专属先古牌分类优先级的实施计划。",
    # tests / tools generic handled
    "tests\\STS2SkinChanger.FrameworkTests\\Program.cs": "框架集成测试入口：加载原皮肤管理器 DLL，验证状态/缓存/保存/防回声。",
    "tests\\STS2SkinChanger.LogicTests\\Program.cs": "逻辑测试入口：依次运行 GitHub、安装、路径、排序等纯逻辑用例。",
    "tests\\STS2SkinChanger.RuntimeTests\\Program.cs": "运行时测试入口：按参数运行默认套件或各 --test-*/--audit-* 子命令。",
    "tools\\Test-BuildEnvironment.ps1": "用真实 MSBuild 检查五个工程的正式版引用、测试版覆盖、缓存隔离和 AnyCPU。",
    "tools\\Test-WorkshopCatalogAudit.ps1": "验证缺失工坊包时内置清单逐字节不变。",
    "tools\\WorkshopPublisher\\Program.cs": "工坊发布工具入口（构建/上传）。",
    "tools\\CatalogInspect\\Program.cs": "目录检查工具入口：用真实 PCK 构建目录并输出识别结果。",
}

def describe(orig_rel: str, new_name: str) -> str:
    if orig_rel in DESC and DESC[orig_rel]:
        return DESC[orig_rel]
    p = pathlib.PureWindowsPath(orig_rel)
    name = p.name
    stem = name.split(".")[0]
    ext = name.rsplit(".", 1)[-1].lower() if "." in name else ""
    chinese = pathlib.Path(new_name).stem
    if ext == "cs":
        if name == "Program.cs":
            return "该测试/工具的可执行入口，负责解析参数并调用具体用例。"
        if "Tests" in name or name.endswith("Test.cs") or name.endswith("Tests.cs"):
            subject = chinese.replace("测试", "")
            return f"针对“{subject}”的离线测试用例，验证对应模块的行为与边界。"
        if "Patch" in name:
            return f"Harmony 运行时补丁：{chinese}。只在目标类型存在时安装，失败按类跳过。"
        if name.endswith("Policy.cs"):
            return f"“{chinese}”相关规则/策略，集中判断条件与边界。"
        if name.endswith("Scanner.cs"):
            return f"扫描并识别“{chinese}”相关资源，产出目录条目。"
        if "Controls" in name or "Screen" in name or "Panel" in name or "View" in name or "Dialog" in name or "Dropdown" in name or "Editor" in name or "Button" in name or "Sidebar" in name or "Drawer" in name or "Toolbar" in name:
            return f"游戏内 UI：{chinese}。构造控件、绑定事件并跟随主题/语言。"
        if "Config" in name or "Settings" in name:
            return f"配置模型或读写：{chinese}。"
        if "Service" in name:
            return f"服务层：{chinese}，协调配置、状态与资源。"
        if "Cache" in name:
            return f"缓存实现：{chinese}。"
        if "Bridge" in name:
            return f"桥接层：{chinese}，在游戏对象与本 Mod 逻辑之间转发。"
        if "Loader" in name:
            return f"加载器：{chinese}。"
        if "Factory" in name:
            return f"工厂：{chinese}。"
        if "Contract" in name:
            return f"接口/数据契约：{chinese}。"
        if "Migration" in name:
            return f"数据迁移：{chinese}。"
        if "Audit" in name:
            return f"只读审计：{chinese}，输出诊断信息供回归对比。"
        return f"实现“{chinese}”相关逻辑（C# 源码）。"
    if ext == "csproj":
        return f"MSBuild 项目文件：{chinese}，定义目标框架、源码与依赖。"
    if ext == "md":
        return f"文档：{chinese}。"
    if ext == "json":
        return f"JSON 数据/配置文件：{chinese}。"
    if ext == "ps1":
        return f"PowerShell 校验脚本：{chinese}。"
    if ext in ("png", "jpg", "jpeg"):
        return f"图片素材：{chinese}。"
    if ext in ("props",):
        return f"MSBuild 属性文件：{chinese}。"
    return f"项目文件：{chinese}。"

# 原目录 -> 中文目录（单层替换，避免重复反转出错）
ORIG_TO_NEW_DIR = {
    "": "", "compat": "兼容层", "compat/ThunninoiSkinManagerCompat": "兼容层/Thunninoi皮肤管理器兼容",
    "docs": "文档", "docs/agents": "文档/开发代理", "docs/plans": "文档/计划",
    "docs/superpowers": "文档/超能力", "docs/superpowers/plans": "文档/超能力/计划",
    "docs/superpowers/specs": "文档/超能力/规格", "STS2SkinChanger": "皮肤切换器主程序",
    "STS2SkinChanger/Catalog": "皮肤切换器主程序/目录", "STS2SkinChanger/Core": "皮肤切换器主程序/核心",
    "STS2SkinChanger/Data": "皮肤切换器主程序/数据", "STS2SkinChanger/Pck": "皮肤切换器主程序/PCK归档",
    "STS2SkinChanger/Properties": "皮肤切换器主程序/程序集属性", "STS2SkinChanger/Ui": "皮肤切换器主程序/界面",
    "tests": "测试", "tests/STS2SkinChanger.FrameworkTests": "测试/框架集成测试",
    "tests/STS2SkinChanger.LogicTests": "测试/逻辑测试", "tests/STS2SkinChanger.RuntimeTests": "测试/运行时测试",
    "tests/STS2SkinChanger.ScriptPathFixture": "测试/脚本路径夹具",
    "tests/STS2SkinChanger.SettingsFixture": "测试/设置夹具",
    "tests/STS2SkinChanger.WorkshopBootstrapFixture": "测试/创意工坊启动夹具",
    "tools": "工具", "tools/CatalogInspect": "工具/目录检查",
    "tools/FrameworkCompatProbe": "工具/框架兼容探测", "tools/OnlineCacheProbe": "工具/在线缓存探测",
    "tools/PckInspect": "工具/PCK检查", "tools/WorkshopCatalogExport": "工具/创意工坊清单导出",
    "tools/WorkshopPublisher": "工具/创意工坊发布器", "workshop": "创意工坊发布资料",
    "workshop/screenshot": "创意工坊发布资料/截图", "workshop/screenshot/upload": "创意工坊发布资料/截图/上传用",
}
NEW_TO_ORIG_DIR = {v: k for k, v in ORIG_TO_NEW_DIR.items()}

orig_by_new = {}
for orig, new in mapping.items():
    orig_by_new[new.replace("/", "\\")] = orig

all_dirs = [DST] + [p for p in DST.rglob("*") if p.is_dir()]
for d in sorted(all_dirs, key=lambda p: len(p.parts)):
    rel_new = "" if d == DST else str(d.relative_to(DST)).replace("/", "\\")
    rel_key = rel_new.replace("\\", "/")
    orig_dir = NEW_TO_ORIG_DIR.get(rel_key, "?")
    title = DIRS.get(rel_key, rel_new)
    lines = []
    lines.append("=" * 72)
    lines.append(f"目录：{rel_new if rel_new else '(项目根)'}")
    lines.append(f"原目录：{orig_dir}")
    lines.append(f"本目录作用：{title}")
    lines.append(DIR_DESC.get(rel_key, "（暂无专门说明，请结合文件名与源码阅读。）"))
    lines.append("说明：本副本由原项目逐字节复制而来，仅目录名/文件名翻译为中文，文件内容未改。")
    lines.append("本文件由分析工具生成，解释同目录下每个文件/子目录的用途，便于接手时快速定位。")
    lines.append("=" * 72)
    lines.append("")
    subdirs = sorted([x for x in d.iterdir() if x.is_dir()], key=lambda x: x.name)
    if subdirs:
        lines.append("【子目录】")
        for sd in subdirs:
            srel = str(sd.relative_to(DST)).replace("/", "\\")
            skey = srel.replace("\\", "/")
            lines.append(f"- {sd.name}\\：{DIR_DESC.get(skey, DIRS.get(skey, '子目录'))}")
        lines.append("")
    files = sorted([x for x in d.iterdir() if x.is_file() and x.name != "_目录说明.txt"], key=lambda x: x.name)
    if files:
        lines.append("【文件】")
        for f in files:
            nrel = str(f.relative_to(DST)).replace("/", "\\")
            orel = orig_by_new.get(nrel, f.name)
            desc = describe(orel, f.name)
            lines.append(f"- {f.name}（原名：{pathlib.PureWindowsPath(orel).name}）")
            lines.append(f"    用途：{desc}")
        lines.append("")
    (d / "_目录说明.txt").write_text("\n".join(lines), encoding="utf-8-sig")

print("wrote", len([p for p in DST.rglob("_目录说明.txt")]))
