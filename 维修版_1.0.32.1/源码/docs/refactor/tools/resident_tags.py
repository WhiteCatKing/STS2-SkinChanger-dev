"""给常驻补丁贴注释标签 + 生成常驻清单（DSH，2026-11-15）。

只加注释行，不改任何代码行。用法：
    python tools/resident_tags.py --dry-run
    python tools/resident_tags.py --apply
"""

from __future__ import annotations

import argparse
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]      # .../STS2-SkinChanger_修改件
SRC = ROOT / "STS2SkinChanger"
DOC = "详见 docs/refactor/resident-patches.md"

T = "常驻"

# (文件, 类别 class/method, 名字, 类型, 谁在用, 干什么, 为什么常驻, 碰了会坏什么)
ITEMS = [
    # ---- 2.2 character / osty ----
    ("Core/SkinService.CharacterSkinBundleRuns.cs", "class", "CharacterSkinBundleNewRunBindingPatch", "入口",
     "选人界面开局", "记录本局的随机角色与组合包预设", "开局还涉及别的分类的预设，不能只按角色包判断", ""),
    ("Core/SkinService.CharacterSkinBundleRuns.cs", "class", "CharacterSkinBundleSavedRunPatch", "入口",
     "读档", "恢复旧存档里的组合包记录", "当前目录没有角色皮肤时，旧存档也可能带着记录", ""),
    ("Core/SkinService.CharacterSkinBundleRuns.cs", "class", "CharacterSkinBundleRunSavePatch", "入口",
     "存档", "把本局的组合包预设写进存档", "与新建、读档两类成套，缺一个就对不上", ""),
    ("Ui/CharacterAppearanceRuntime.cs", "class", "InRunCharacterAppearanceRuntimePatch", "共享",
     "角色 / 随从 / 怪物 / 局内界面", "挂局内外观运行时节点并接联机同步", "联机同步和提供者范围都由它建立，怪物和局内界面也依赖", "NRun 子节点 Ready 时机、单帧延迟恢复丢失"),
    ("Ui/CharacterAppearanceRuntime.cs", "class", "TopBarRoomIconSkinTexturePatch", "共享",
     "Boss / 房间图标", "接管顶栏房间图标的贴图加载", "和怪物地图呈现共用同一个压缩贴图入口", ""),
    ("Ui/CharacterAppearanceRuntime.cs", "class", "BossMapPointSkinPresentationPatch", "共享",
     "怪物 / Boss 提供者", "地图 Boss 点的皮肤呈现", "目前主要服务怪物提供者回调，归属以后再理", ""),
    ("Ui/CharacterAppearanceRuntime.cs", "class", "CharacterAppearanceCreatureReadyPatch", "共享",
     "角色 / 随从 / 怪物", "生物就绪时记录视觉缩放基准并套变换外壳", "三类生物共用同一套基准与变换包装", "缩放后血条/意图/选择框错位，重置数值丢失"),
    ("Ui/CharacterAppearanceRuntime.cs", "class", "CharacterAppearancePlayerLayoutPatch", "共享",
     "角色与随从", "记录原生排布快照", "角色和随从的排布共用一份快照", ""),
    ("Ui/CharacterAppearanceRuntime.cs", "class", "CombatRoomRuntimeProviderScopePatch", "共享",
     "角色 / 宠物 / 怪物", "进战斗房时扫描双方并建运行时范围", "扫描覆盖 Allies 和 Enemies 两侧", ""),
    ("Ui/CharacterAppearanceRuntime.cs", "class", "CombatRoomRuntimeProviderScopeExitPatch", "共享",
     "角色 / 宠物 / 怪物", "离开战斗房时收尾同一份范围", "和上一项共用场景生命周期，必须成对", ""),
    ("Ui/CharacterAppearanceRuntime.cs", "class", "CharacterAppearanceBoundsPatch", "共享",
     "角色 / 随从 / 怪物", "按变换更新命中框，含纯怪物缩放分支", "命中框跟着共享变换走，含怪物缩放", "缩放后血条/意图/选择框错位，重置数值丢失"),
    ("Ui/CharacterAppearanceRuntime.cs", "class", "CharacterAppearanceOrbPositionPatch", "共享",
     "角色呈现 / 本体外观编辑", "跟随生物设置充能球位置", "角色呈现和本体外观编辑都用它，先随变换服务保留", ""),
    ("Ui/CharacterAppearanceScreen.cs", "class", "CharacterAppearancePauseMenuPatch", "入口",
     "局内外观界面", "在暂停菜单里挂外观面板入口", "面板同时编辑角色、怪物和随从", ""),
    ("Ui/CharSelectStageCycle.cs", "class", "CharSelectStageCyclePatch", "入口",
     "选人界面", "挂战损/阶段切换控件", "选择界面的阶段预览入口", ""),
    ("Ui/ContextualSkinControls.cs", "class", "RandomCharacterLobbySkinPatch", "入口",
     "选人界面 / 大厅", "返回大厅时恢复随机选择的基准", "大厅往返还靠它保持随机选择", ""),
    ("Ui/ContextualSkinControls.cs", "class", "CharacterSelectionSkinPatch", "入口",
     "选人界面", "捕获布局并把皮肤选择器挂上去", "所有角色外观选择的入口", ""),
    ("Ui/ContextualSkinControls.cs", "class", "SingleplayerEmbarkSkinSelectorPatch", "入口",
     "单人开局", "开局时应用组合包、处理随机角色并收起选择器", "开局钩子，缺了组合包不生效", ""),
    ("Ui/ContextualSkinControls.cs", "class", "MultiplayerEmbarkSkinSelectorPatch", "入口",
     "多人开局", "按本地大厅角色应用组合包", "多人开局各自按本地角色应用", ""),
    ("Ui/ContextualSkinControls.cs", "class", "MonsterVisualScalePatch", "共享",
     "怪物 / 随从", "按缩放分组记录恢复怪物缩放；熔断键是 patch:character 属于历史原因", "角色功能停用后，怪物和随从仍要恢复各自节点的皮肤缩放", "缩放后血条/意图/选择框错位，重置数值丢失"),
    ("Ui/CreatureIntentDepth.cs", "class", "CreatureIntentDepthPatch", "共享",
     "怪物 / 有意图的随从", "调整意图图标的显示层级", "主体只处理 IsMonster，但随从也有意图", ""),
    ("Ui/ProviderVariantPathPatch.cs", "class", "ProviderMerchantVariantPathPatch", "共享",
     "角色变体 / 商人房", "解析角色变体在商人房用的场景路径", "和商人场景、预加载共用同一套路径", "贞德设置型变体被拼成不存在的商人/火堆场景"),
    ("Ui/ProviderVariantPathPatch.cs", "class", "ProviderRestSiteVariantPathPatch", "共享",
     "角色变体 / 火堆", "解析角色变体在火堆用的场景路径", "和联机房间的资源作用域共用", "贞德设置型变体被拼成不存在的商人/火堆场景"),
    ("Core/CreatureVisualParentBridge.cs", "method", "Register", "共享",
     "角色 / 怪物 / 随从", "转译第三方对 NCreature 父节点的查询", "变换包装要跨三类生物，还要保留自有 Harmony 的生命周期", ""),
    ("Core/ManagedCharacterAssetRegistrationGuard.cs", "method", "EnsureFrameworkPatchesInstalled", "共享",
     "角色契约 / UI", "在框架角色资源注册入口装守卫补丁", "角色选择会同时控制战斗、商人、火堆、UI 路径", ""),
    ("Core/ManualCharacterVariantBridge.cs", "method", "BindScreen", "入口",
     "选人界面", "观察提供者自己的变体 Toggle 并保存主动选择", "按玩家渲染的作用域要靠它恢复临时设置", "变体更新只改配置，预览/背景/模型仍旧色；双来源执行者混淆"),
    ("Core/CompanionDisplayNameGuard.cs", "class", "OstyDisplayNameGuardPatch", "兼容",
     "随从名字", "把随从名字恢复成游戏原名（奥斯提）", "选非 midori 或原版时也要恢复名字，保留 HarmonyAfter 和 Last", ""),
    ("Core/ManagedSkinModLoader.cs", "method", "EnsureDirectCharacterSkinPathRouter", "共享",
     "角色运行时提供者", "按角色选择分发 Spine 外观路径", "随提供者程序集注册常驻，避免同包里未被选中的角色被污染", ""),
    ("Core/OstyDefectImpostorCompat.cs", "method", "Install", "兼容",
     "独立奥斯提皮肤包", "为第三方补丁提供旧方法名兼容", "必须在提供者加载前装好，否则对方 PatchAll 失败会自卸全部补丁", "独立奥斯提包因游戏方法改名而自卸全部补丁"),
    # ---- 2.3 core 中角色/随从相邻共享项 ----
    ("Core/FrameworkSkinRuntimePatches.cs", "class", "FrameworkCardEnergyColorPatch", "共享",
     "卡池能量描边", "用角色契约值画卡池能量描边", "和卡牌呈现共用同一个契约值", ""),
    ("Core/FrameworkSkinRuntimePatches.cs", "class", "FrameworkOrbDarkenedColorPatch", "共享",
     "充能球 / 联机球作用域", "用角色契约的 Orb 描述驱动球颜色", "和充能球呈现、联机球作用域共用", ""),
    ("Core/FrameworkSkinRuntimePatches.cs", "class", "FrameworkRelicPackedIconPatch", "共享",
     "遗物 UI", "把角色契约提供的遗物图标接进遗物 UI", "和遗物 UI 共用同一份图标", ""),
    ("Core/FrameworkSkinRuntimePatches.cs", "class", "FrameworkRelicOutlinePatch", "共享",
     "遗物 UI", "把角色契约提供的遗物描边接进遗物 UI", "和遗物 UI 共用同一份描边", ""),
    ("Ui/ContextualSkinControls.cs", "class", "CharacterSkinBundleRunCleanupPatch", "入口",
     "退出对局", "退出时恢复此前的卡牌、怪物等预设", "和新建、保存、读档三类成套", ""),
    ("Ui/ContextualSkinControls.cs", "class", "CachedSceneResultPatch", "共享",
     "任意分组", "按资源路径查任意分组的场景缓存", "按路径查表，服务所有分组", "外部骨架加载器与皮肤同时拥有骨架/图集"),
    ("Ui/ContextualSkinControls.cs", "class", "CachedTextureResultPatch", "共享",
     "任意分组", "按资源路径查任意分组的贴图缓存", "同上，还会写事件模块用到的元数据", "外部骨架加载器与皮肤同时拥有骨架/图集"),
    ("Core/CharSelectTakeoverSuppressor.cs", "method", "Install", "入口",
     "选人界面", "抑制第三方选人接管并接收独立随从选择通知", "第三方接管和随从通知都在选人阶段", ""),
    ("Core/FrameworkNativeUiPatch.cs", "method", "Install", "兼容",
     "角色框架选人 UI", "修 debug 查询并给框架 AddEntry 加角色选择门控", "和角色入场动画共用同一段 UI 逻辑", ""),
    ("Core/FrameworkRegistryCooperation.cs", "method", "Bind", "入口",
     "作者原生选择器", "让作者原生选择器和我们的选择器双向衔接，含预览和联机头像", "原生选择器和 SC 选择器必须一起工作", "变体更新只改配置，预览/背景/模型仍旧色；双来源执行者混淆"),
    ("Core/FrameworkRegistrySession.cs", "method", "Install", "共享",
     "角色 registry", "接 registry 的读写与 Resolve（能力/药水/遗物/球）", "球、药水、遗物等契约都走它", ""),
    ("Core/ManagedSlotVisibilityBridge.cs", "method", "BindPreview", "入口",
     "选择器预览", "观察选中提供者的部件开关并应用到独立实例", "预览要用提供者自己的部件开关", ""),
    ("Core/ProviderMenuGateCompat.cs", "method", "PatchGate", "兼容",
     "角色变体 / 战损", "给提供者的菜单门控打补丁", "提供者的选择门控要按变体和战损分别处理", ""),
    ("Core/ProviderMenuGateCompat.cs", "method", "PatchOwnSelectionEntryPoints", "兼容",
     "角色变体 / 战损", "给提供者自己的选择入口打补丁", "同上，入口不止一处", ""),
    ("Core/ScopedSkinBehavior.cs", "method", "Install", "共享",
     "角色 / 玩家", "处理提供者 AppliesTo 的玩家作用域兼容", "玩家作用域和安装记录由主干持有", "", "int Install("),
    ("Core/VariantHotkeyPoller.cs", "method", "Suppress", "入口",
     "快捷键", "抑制提供者自己的快捷键，避免和我们的重复执行", "和 VariantHotkeyPoller 配对，缺一半会双执行", ""),
]


def read(p: Path) -> list[str]:
    raw = p.read_bytes()
    for enc in ("utf-8-sig", "utf-8"):
        try:
            return raw.decode(enc).splitlines()
        except UnicodeDecodeError:
            continue
    return raw.decode("gb18030", "replace").splitlines()


def find_class(lines: list[str], name: str) -> int:
    pat = re.compile(rf"^\s*(?:internal|public|private|protected)?\s*(?:static|sealed|partial|\s)*class\s+{re.escape(name)}\b")
    hits = [i for i, ln in enumerate(lines) if pat.match(ln)]
    if len(hits) != 1:
        raise SystemExit(f"类 {name}: 命中 {len(hits)} 次")
    return hits[0]


def find_method(lines: list[str], name: str, hint: str | None = None) -> int:
    pat = re.compile(rf"^\s*(?:internal|public|private|protected)\s+(?:static\s+)?[\w<>\[\]\.\?]+\s+{re.escape(name)}\s*\(")
    hits = [i for i, ln in enumerate(lines) if pat.match(ln)]
    if hint:
        hits = [i for i in hits if hint in lines[i]]
    if len(hits) != 1:
        raise SystemExit(f"方法 {name}{'/' + hint if hint else ''}: 命中 {len(hits)} 次")
    return hits[0]


def insert_point(lines: list[str], decl: int, indent: str) -> int:
    """把标签插到类/方法上方：越过特性行，再越过紧跟的 /// 文档注释块。"""
    j = decl
    while j - 1 >= 0 and lines[j - 1].strip().startswith("["):
        j -= 1
    k = j
    while k - 1 >= 0 and lines[k - 1].strip().startswith("///"):
        k -= 1
    return k


def body_key(lines: list[str], decl: int) -> str:
    depth, started = 0, False
    for i in range(decl, min(len(lines), decl + 400)):
        code = lines[i].split("//")[0]
        depth += code.count("{") - code.count("}")
        if "{" in code:
            started = True
        m = re.search(r'Safe\.(?:Run|Get)\("([^"]+)"', code)
        if m:
            return m.group(1)
        if started and depth <= 0:
            break
    return "无（未包 Safe）"


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--dry-run", action="store_true")
    ap.add_argument("--apply", action="store_true")
    args = ap.parse_args()
    dry = not args.apply

    by_file: dict[str, list[dict]] = {}
    for item in ITEMS:
        file, kind, name, typ, who, what, why, risk = item[:8]
        hint = item[8] if len(item) > 8 else None
        by_file.setdefault(file, []).append(
            dict(kind=kind, name=name, type=typ, who=who, what=what, why=why, risk=risk, hint=hint))

    plan: list[dict] = []
    for file, entries in by_file.items():
        path = SRC / file
        lines = read(path)
        # 从后往前插，避免行号漂移
        found: list[tuple[int, dict, str]] = []
        for e in entries:
            decl = (find_class(lines, e["name"]) if e["kind"] == "class"
                    else find_method(lines, e["name"], e.get("hint")))
            indent = "" if e["kind"] == "class" else re.match(r"^(\s*)", lines[decl]).group(1)
            pos = insert_point(lines, decl, indent)
            label = f"{indent}// 常驻·{e['type']}：{e['who']} —— {e['what']}。{DOC}"
            e["key"] = body_key(lines, decl)
            found.append((pos, e, label))
        for pos, e, label in sorted(found, key=lambda x: -x[0]):
            existing = None
            if pos < len(lines) and lines[pos].lstrip().startswith("// 常驻"):
                existing = pos
            elif pos - 1 >= 0 and lines[pos - 1].lstrip().startswith("// 常驻"):
                existing = pos - 1
            if existing is not None:
                lines[existing] = label
                e["line"] = existing + 1
                continue
            lines.insert(pos, label)
            e["line"] = pos + 1
        if not dry:
            path.write_text("\n".join(lines) + "\n", encoding="utf-8", newline="\n")
        plan.extend(entries)

    for e in plan:
        print(f"{e['line']:>5}  {e['type']}  {e['name']:<45} key={e['key']}")
    print(f"\n合计 {len(plan)} 处；{'dry-run' if dry else '已写入'}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
