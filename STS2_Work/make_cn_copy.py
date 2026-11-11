import re
import shutil
import hashlib
import pathlib
import sys

SRC = pathlib.Path(r"D:\STS2-SkinChanger")
DST = pathlib.Path(r"D:\STS2皮肤切换器_中文副本")

# ---------------- directory translations ----------------
DIRS = {
    "": "",
    "compat": "兼容层",
    "compat/ThunninoiSkinManagerCompat": "兼容层/Thunninoi皮肤管理器兼容",
    "docs": "文档",
    "docs/agents": "文档/开发代理",
    "docs/plans": "文档/计划",
    "docs/superpowers": "文档/超能力",
    "docs/superpowers/plans": "文档/超能力/计划",
    "docs/superpowers/specs": "文档/超能力/规格",
    "STS2SkinChanger": "皮肤切换器主程序",
    "STS2SkinChanger/Catalog": "皮肤切换器主程序/目录",
    "STS2SkinChanger/Core": "皮肤切换器主程序/核心",
    "STS2SkinChanger/Data": "皮肤切换器主程序/数据",
    "STS2SkinChanger/Pck": "皮肤切换器主程序/PCK归档",
    "STS2SkinChanger/Properties": "皮肤切换器主程序/程序集属性",
    "STS2SkinChanger/Ui": "皮肤切换器主程序/界面",
    "tests": "测试",
    "tests/STS2SkinChanger.FrameworkTests": "测试/框架集成测试",
    "tests/STS2SkinChanger.LogicTests": "测试/逻辑测试",
    "tests/STS2SkinChanger.RuntimeTests": "测试/运行时测试",
    "tests/STS2SkinChanger.ScriptPathFixture": "测试/脚本路径夹具",
    "tests/STS2SkinChanger.SettingsFixture": "测试/设置夹具",
    "tests/STS2SkinChanger.WorkshopBootstrapFixture": "测试/创意工坊启动夹具",
    "tools": "工具",
    "tools/CatalogInspect": "工具/目录检查",
    "tools/FrameworkCompatProbe": "工具/框架兼容探测",
    "tools/OnlineCacheProbe": "工具/在线缓存探测",
    "tools/PckInspect": "工具/PCK检查",
    "tools/WorkshopCatalogExport": "工具/创意工坊清单导出",
    "tools/WorkshopPublisher": "工具/创意工坊发布器",
    "workshop": "创意工坊发布资料",
    "workshop/screenshot": "创意工坊发布资料/截图",
    "workshop/screenshot/upload": "创意工坊发布资料/截图/上传用",
}

DIR_DESC = {
    "": "项目根目录。这里放项目说明、开发约定、构建配置和全局 SDK 版本；真正的代码在下面各个子目录。",
    "compat": "兼容层工程。编译出一个与旧“皮肤管理器”同名同接口的替代 DLL，让依赖该管理器的皮肤 Mod 仍能启动。",
    "compat/ThunninoiSkinManagerCompat": "旧皮肤管理器（thunninoi's Skin Loader and Manager）的兼容替代实现；只提供接口与注册表，不接管画面。",
    "docs": "开发文档。包括已完成/已定案清单、待办反馈、功能计划与多语言发布说明。",
    "docs/agents": "写给开发代理/AI 的代码库索引与工坊清单维护说明。",
    "docs/plans": "单个功能或修复的实施计划。",
    "docs/superpowers": "较大的功能设计与实施计划归档。",
    "docs/superpowers/plans": "按日期归档的功能实施计划。",
    "docs/superpowers/specs": "按日期归档的功能设计规格。",
    "STS2SkinChanger": "Mod 主程序。入口、清单、工程文件和全部 C# 源码；下面按目录、核心、界面等分目录。",
    "STS2SkinChanger/Catalog": "皮肤目录构建：扫描 PCK、DLL、卡图包和散装图片，把资源归类到角色、卡牌、怪物、先古、商人和事件。",
    "STS2SkinChanger/Core": "核心逻辑：配置、选择、资源覆盖包挂载、框架/版本兼容、多人同步、创意工坊与 GitHub 来源、退出清理。",
    "STS2SkinChanger/Data": "随 DLL 一起嵌入的静态数据（内置创意工坊清单）。",
    "STS2SkinChanger/Pck": "Godot PCK 归档的读取/写入实现，用于按需生成资源覆盖包。",
    "STS2SkinChanger/Properties": "程序集版本与元数据。",
    "STS2SkinChanger/Ui": "游戏内界面与控件：选角、其它图鉴、局内“外观”、卡牌/怪物优先级、主题编辑器、工坊面板及 Harmony UI 补丁。",
    "tests": "离线测试。用替代引擎和真实游戏程序集验证逻辑，不启动游戏。",
    "tests/STS2SkinChanger.FrameworkTests": "与原皮肤管理器 DLL 的集成测试（独立进程加载真实 DLL，替换 IO/日志边界）。",
    "tests/STS2SkinChanger.LogicTests": "不依赖 Godot 场景的纯逻辑测试：GitHub 策略、安装台账、路径、排序等。",
    "tests/STS2SkinChanger.RuntimeTests": "运行时测试：加载真实 sts2/Godot 程序集，覆盖目录、挂载、UI 契约、多人、工坊、存储等。",
    "tests/STS2SkinChanger.ScriptPathFixture": "测试夹具：模拟皮肤 Mod 重复注册 Godot 脚本路径。",
    "tests/STS2SkinChanger.SettingsFixture": "测试夹具：模拟带设置数据的皮肤 Mod。",
    "tests/STS2SkinChanger.WorkshopBootstrapFixture": "测试夹具：模拟创意工坊启动入口。",
    "tools": "开发与发布辅助工具，不随 Mod 发布。",
    "tools/CatalogInspect": "用真实游戏与 Mod PCK 构建皮肤目录，打印识别结果，便于回归对比。",
    "tools/FrameworkCompatProbe": "探测框架程序集/接口兼容性。",
    "tools/OnlineCacheProbe": "检查在线皮肤缓存与包内容。",
    "tools/PckInspect": "检查 PCK 目录结构，或复制少量文件验证写入器。",
    "tools/WorkshopCatalogExport": "只读核验本地工坊包并刷新内置清单的重启标记。",
    "tools/WorkshopPublisher": "构建并上传 Steam 创意工坊条目的工具（不会自动上传）。",
    "workshop": "Steam 创意工坊发布素材：封面、截图、标题/描述与 15 语言文案。",
    "workshop/screenshot": "工坊展示截图原图（PNG）。",
    "workshop/screenshot/upload": "工坊展示截图压缩版（JPG，上传用）。",
}

# ---------------- token translations ----------------
TOKENS = {
    # technical / proper nouns
    "STS2": "STS2", "GitHub": "GitHub", "Git": "Git", "Godot": "Godot", "PCK": "PCK",
    "Pck": "PCK", "UI": "界面", "Ui": "界面", "API": "API", "Api": "API", "HUD": "HUD",
    "Hud": "HUD", "LRU": "LRU", "Lru": "LRU", "VFX": "特效", "Vfx": "特效",
    "ZIP": "ZIP", "Zip": "ZIP", "Thunninoi": "Thunninoi", "Mod": "模组", "mod": "模组",
    "Mods": "模组", "BR": "BR", "Hant": "繁体", "In": "", "ID": "ID",
    # common
    "Action": "操作", "Actions": "操作", "Aliased": "别名", "Ancient": "先古", "ancient": "先古",
    "Animation": "动画", "Animator": "动画器", "Appearance": "外观", "Archive": "归档",
    "Art": "卡图", "Assembly": "程序集", "Asset": "资源", "Audit": "审计", "Backdrop": "背景",
    "Baseline": "基线", "Behavior": "行为", "behavior": "行为", "Bestiary": "怪物图鉴",
    "Bootstrap": "启动引导", "Boundary": "边界", "Bounded": "有界", "Bridge": "桥接",
    "Browser": "浏览器", "Build": "构建", "Built": "内置", "Bundle": "包", "Bundles": "皮肤包",
    "bundles": "包", "BundlePreset": "皮肤包预设", "BundlePresets": "皮肤包预设",
    "BundleContentModes": "皮肤包内容模式", "Readme": "说明",
    "Button": "按钮", "Cache": "缓存", "Candidate": "候选",
    "Canonical": "规范", "Canvas": "画布", "Card": "卡牌", "card": "卡牌", "Carousel": "轮播",
    "Catalog": "目录", "catalog": "目录", "Changer": "切换器", "Changes": "变更",
    "Character": "角色", "character": "角色", "Check": "检查", "Choice": "选择",
    "Cleanup": "清理", "Click": "点击", "Client": "客户端", "Code": "代码", "code": "代码",
    "Codec": "编解码", "Codes": "代码", "Coloring": "着色", "Combat": "战斗",
    "Command": "命令", "Community": "社区", "community": "社区", "Companion": "同伴",
    "Compat": "兼容", "Compatibility": "兼容性", "Compendium": "图鉴", "Composition": "组合",
    "composition": "组合", "Config": "配置", "Content": "内容", "Context": "上下文",
    "Contextual": "上下文", "Contract": "契约", "Contracts": "契约", "Control": "控件",
    "Controls": "控件", "Cooperation": "协作", "cooperation": "协作", "Coordinator": "协调器",
    "Cosmetic": "外观", "Cover": "封面", "Creature": "生物", "Cycle": "循环", "Data": "数据",
    "Deferred": "延迟", "Dependency": "依赖", "Depth": "深度", "Details": "详情",
    "Diagnostics": "诊断", "Dialog": "对话框", "Dimming": "变暗", "Direct": "直接",
    "Discovery": "发现", "Discussion": "讨论", "Display": "显示", "Drag": "拖动",
    "Draggable": "可拖动", "Drawer": "抽屉", "Dropdown": "下拉框", "Duplicate": "重复",
    "Dwell": "停留", "Editing": "编辑", "Editor": "编辑器", "Effects": "特效",
    "Entrance": "入口", "Entry": "入口", "Environment": "环境", "Error": "错误",
    "Errors": "错误", "Event": "事件", "Events": "事件", "Evidence": "证据",
    "Exclusion": "排除", "Exit": "退出", "Export": "导出", "Exported": "导出",
    "External": "外部", "Factory": "工厂", "Failure": "失败", "Feedback": "反馈",
    "Figure": "形象", "File": "文件", "Filter": "筛选", "Filters": "筛选", "Fixture": "夹具",
    "Focus": "焦点", "Folder": "文件夹", "Framework": "框架", "framework": "框架",
    "Framing": "取景", "Game": "游戏", "Group": "分组", "Guard": "守卫", "Handle": "手柄",
    "Hint": "提示", "Hover": "悬停", "Hub": "枢纽", "Identity": "身份", "Info": "信息",
    "Initializer": "初始化器", "Inspect": "检查", "Inspection": "检查", "Install": "安装",
    "Instance": "实例", "Instantiation": "实例化", "Integrity": "完整性", "Intent": "意图",
    "Interaction": "交互", "Interception": "拦截", "Intrinsic": "内置", "Introduction": "简介",
    "Isolated": "隔离", "Isolation": "隔离", "Item": "项目", "Layer": "层", "Layout": "布局",
    "Ledger": "台账", "Legacy": "旧版", "Library": "库", "Lifecycle": "生命周期", "Line": "行",
    "Links": "链接", "List": "列表", "Live": "实时", "Load": "加载", "load": "加载",
    "Loader": "加载器", "Loading": "加载", "Localization": "本地化", "Localizations": "本地化",
    "localizations": "本地化", "Locator": "定位器", "Log": "日志", "Logic": "逻辑",
    "Lookup": "查找", "Loose": "散装", "Managed": "托管", "Manager": "管理器",
    "Manifest": "清单", "Manual": "手动", "Menu": "菜单", "Merchant": "商人",
    "Metadata": "元数据", "Method": "方法", "Migration": "迁移", "Minimum": "最低",
    "Mode": "模式", "Modes": "模式", "Model": "模型", "Monster": "怪物",
    "Multiplayer": "多人", "Name": "名称", "Names": "名称", "Naming": "命名",
    "Native": "原生", "native": "原生", "Node": "节点", "Notice": "提示", "Online": "在线",
    "Option": "选项", "Optional": "可选", "Order": "顺序", "Other": "其它",
    "Overlay": "覆盖", "Owner": "所有者", "Ownership": "归属", "Pack": "包", "Package": "包",
    "Page": "页面", "Paged": "分页", "Panel": "面板", "Parent": "父级", "Part": "部件",
    "Participation": "参与", "Patch": "补丁", "Patches": "补丁", "Path": "路径",
    "Paths": "路径", "Pause": "暂停", "Performance": "性能", "Placement": "位置",
    "Policy": "策略", "Pool": "池", "Popup": "弹窗", "Portrait": "立绘",
    "Presentation": "呈现", "Preset": "预设", "Presets": "预设", "Preview": "预览",
    "Priorities": "优先级", "Priority": "优先级", "priority": "优先级", "Private": "私有",
    "Probe": "探测", "Profile": "配置档案", "Program": "程序入口", "Promoted": "提升",
    "Provider": "提供者", "provider": "提供者", "Publisher": "发布器", "Queue": "队列",
    "Quit": "退出", "Random": "随机", "Reader": "读取器", "Ready": "就绪", "Rebuild": "重建",
    "Recognition": "识别", "Reference": "引用", "Refresh": "刷新", "Region": "地区",
    "Regions": "地区", "Registered": "已注册", "Registration": "注册", "Registry": "注册表",
    "Release": "发布", "Relic": "遗物", "Reloading": "重载", "Rendered": "渲染",
    "Replacement": "替换", "Reply": "回复", "Required": "必需", "Reset": "重置",
    "Resolver": "解析器", "Resource": "资源", "Resources": "资源", "Respect": "尊重",
    "Revival": "复活", "Right": "右", "Room": "房间", "Routing": "路由", "Rows": "行",
    "Rules": "规则", "Run": "对局", "Runner": "执行器", "Runs": "运行记录",
    "Runtime": "运行时", "Safety": "安全", "Scan": "扫描", "Scanner": "扫描器",
    "Scene": "场景", "Scope": "作用域", "Scoped": "作用域", "scoped": "作用域",
    "Screen": "界面", "Script": "脚本", "Scripts": "脚本", "Scroll": "滚动",
    "Selection": "选择", "Selector": "选择器", "Service": "服务", "Session": "会话",
    "Settings": "设置", "settings": "设置", "Shadow": "阴影", "Shared": "共享",
    "Sidebar": "侧栏", "Skin": "皮肤", "skin": "皮肤", "Skins": "皮肤", "Slot": "槽位",
    "Sort": "排序", "Source": "来源", "source": "来源", "Sources": "来源",
    "Standalone": "独立", "State": "状态", "Stateful": "有状态", "Storage": "存储",
    "Style": "风格", "Submission": "投稿", "Submissions": "投稿", "submissions": "投稿",
    "Subscription": "订阅", "Surface": "表面", "Swap": "交换", "Switcher": "切换器",
    "Sync": "同步", "Synchronizer": "同步器", "Tags": "标签", "Takeover": "接管",
    "Target": "目标", "Targets": "目标", "Test": "测试", "Tests": "测试", "Text": "文本",
    "Theme": "主题", "Toggle": "开关", "Toolbar": "工具栏", "Tracker": "跟踪器",
    "Transform": "变换", "Type": "类型", "Variant": "变体", "Variants": "变体",
    "Version": "版本", "View": "视图", "Visibility": "可见性", "Visual": "视觉",
    "Visuals": "视觉", "Warm": "预热", "Warning": "警告", "Workshop": "创意工坊",
    "WorkshopBootstrap": "创意工坊启动", "WorkshopCatalog": "创意工坊清单",
    "capabilities": "能力", "design": "设计", "functions": "功能", "hot": "热",
    "memory": "记忆", "codebase": "代码库", "github": "GitHub", "workshop": "创意工坊",
    "publish": "发布", "provider": "提供者", "priority": "优先级", "source": "来源",
    "de": "德文", "es": "西班牙文", "fr": "法文", "it": "意大利文",
    "ja": "日文", "ko": "韩文", "pl": "波兰文", "pt": "葡萄牙文", "ru": "俄文",
    "th": "泰文", "tr": "土耳其文", "zh": "中文", "419": "419",
}

SPECIAL_STEMS = {
    "AGENTS": "开发约定",
    "PRODUCT": "产品说明",
    "README": "说明",
    "Readme": "说明",
    "Directory.Build": "目录构建属性",
    "global": "全局配置",
    "SkinChanger": "皮肤切换器",
    "COMPLETED_FIXED": "已完成与已定案",
    "FEATURE_REQUESTS_PENDING": "功能需求待定",
    "WORKSHOP_FEEDBACK_2026-08-30": "创意工坊反馈_2026-08-30",
    "Test-BuildEnvironment": "测试-构建环境",
    "Test-WorkshopCatalogAudit": "测试-创意工坊清单审计",
    "投稿GitHub皮肤Mod说明": "投稿GitHub皮肤模组说明",
    "SkinChangerShivAnimFix": "皮肤切换器短剑动画修复",
    "es-419": "拉美西班牙文",
    "pt-BR": "巴西葡萄牙文",
    "zh-Hant": "繁体中文",
}

LANG = {
    "de": "德文", "es": "西班牙文", "es-419": "拉美西班牙文", "fr": "法文", "it": "意大利文",
    "ja": "日文", "ko": "韩文", "pl": "波兰文", "pt-BR": "巴西葡萄牙文", "ru": "俄文",
    "th": "泰文", "tr": "土耳其文", "zh-Hant": "繁体中文", "md": "说明",
}

FILE_OVERRIDES = {
    ".gitattributes": ".git属性",
    ".gitignore": ".git忽略",
    "global.json": "全局配置.json",
    "Directory.Build.props": "目录构建属性.props",
    "SkinChanger.json": "皮肤切换器.json",
    "SkinChanger.jpg": "皮肤切换器.jpg",
    "SkinChanger.png": "皮肤切换器.png",
    "localizations.json": "本地化.json",
    "workshop.json": "创意工坊.json",
    "workshop-catalog.json": "创意工坊清单.json",
    "Bootstrap.cs": "启动引导.cs",
    "ScriptPathFixtureScripts.cs": "脚本路径夹具脚本.cs",
    "RegisteredSkinProfile.cs": "已注册皮肤配置档案.cs",
    "CardLayoutVariantTests.cs": "卡牌布局变体测试.cs",
    "CompanionSelectionTests.cs": "同伴选择测试.cs",
    "OmitNativeLogs": "省略原生日志",
}

TOKEN_RE = re.compile(r"[A-Z]+(?=[A-Z][a-z])|[A-Z]?[a-z]+|[A-Z]+|\d+")

POST_FIXES = [
    ("皮肤皮肤包", "皮肤包"),
    ("皮肤皮肤", "皮肤"),
    ("卡牌卡图包", "卡图包"),
    ("卡牌卡牌", "卡牌"),
    ("退出退出清理", "退出流程清理"),
    ("创意工坊创意工坊", "创意工坊"),
    ("包包", "包"),
]

def translate_part(part: str) -> str:
    if part in TOKENS:
        return TOKENS[part]
    if part in SPECIAL_STEMS:
        return SPECIAL_STEMS[part]
    out = []
    for segment in re.split(r"(GitHub)", part):
        if segment == "GitHub":
            out.append("GitHub")
            continue
        for t in TOKEN_RE.findall(segment):
            if t in TOKENS:
                v = TOKENS[t]
                if v:
                    out.append(v)
            else:
                out.append(t)
    return "".join(out) if out else part

def translate_stem(stem: str) -> str:
    if stem in FILE_OVERRIDES:
        return FILE_OVERRIDES[stem]
    if stem in SPECIAL_STEMS:
        return SPECIAL_STEMS[stem]
    m = re.match(r"^(\d{4}-\d{2}-\d{2})-(.+)$", stem)
    if m:
        text = m.group(1) + "-" + "".join(translate_part(p) for p in re.split(r"[-_]", m.group(2)) if p)
    else:
        dotted = []
        for chunk in stem.split("."):
            dotted.append("".join(translate_part(p) for p in re.split(r"[-_\s]+", chunk) if p))
        text = ".".join(dotted)
    for old, new in POST_FIXES:
        text = text.replace(old, new)
    return text

def translate_name(name: str) -> str:
    if name in FILE_OVERRIDES:
        return FILE_OVERRIDES[name]
    if name in (".gitattributes", ".gitignore"):
        return FILE_OVERRIDES[name]
    chunks = name.split(".")
    if len(chunks) == 1:
        return translate_stem(name)
    ext = chunks[-1]
    stem = ".".join(translate_stem(c) for c in chunks[:-1])
    return stem + "." + ext

def chinese_dir(rel: str) -> str:
    key = rel.replace("\\", "/")
    return DIRS[key].replace("/", "\\") if DIRS[key] else ""

# ---------------- copy tree ----------------
if DST.exists():
    print("destination exists, abort")
    sys.exit(1)
DST.mkdir(parents=True)

dirs = [p for p in SRC.rglob("*") if p.is_dir()]
for d in sorted(dirs, key=lambda x: len(x.parts)):
    rel = str(d.relative_to(SRC)).replace("/", "\\")
    if rel.replace("\\", "/") not in DIRS:
        print("MISSING DIR MAP", rel)
        sys.exit(2)
    (DST / chinese_dir(rel)).mkdir(parents=True, exist_ok=True)

import collections
used = collections.defaultdict(set)
mapping = {}
for f in sorted(SRC.rglob("*")):
    if not f.is_file():
        continue
    rel = str(f.relative_to(SRC)).replace("/", "\\")
    parent = str(f.parent.relative_to(SRC)).replace("/", "\\") if f.parent != SRC else ""
    if rel in (".gitattributes", ".gitignore"):
        newname = FILE_OVERRIDES[rel]
    else:
        newname = translate_name(f.name)
    keydir = chinese_dir(parent)
    base = pathlib.Path(newname)
    candidate = newname
    n = 2
    while candidate.lower() in used[keydir]:
        candidate = f"{base.stem}_{n}{base.suffix}"
        n += 1
    used[keydir].add(candidate.lower())
    target = DST / keydir / candidate
    target.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(f, target)
    mapping[rel] = str(target.relative_to(DST))

import json
(DST.parent / "STS2_Work_name_map.json").write_text(json.dumps(mapping, ensure_ascii=False, indent=1), encoding="utf-8")
print("copied", sum(1 for _ in DST.rglob("*") if _.is_file()))
print("mapped", len(mapping))
