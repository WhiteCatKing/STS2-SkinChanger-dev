#!/usr/bin/env python3
"""重构清点扫描器 v3（DSH / 肥鱼，2026-11-14）。

对应任务书 `docs/refactor/task-dsh-inventory-20261114.md` 的 ①②③：
  ① patches.csv + patches-summary.md
  ② risk-sites.md（写盘 / 无界循环 / 重试 三张表）
  ③ reflection.csv + reflection-summary.md

只读源码，只写 docs/refactor/。可重复运行（结果确定性）。
用法：python tools/inventory_scan.py
类型表（判定 目标归属 game/godot/self/thirdparty）默认读本目录下
`game-types-sts2-v0.113.0.txt`、`godot-types-v4.5.1.txt`；重新生成见 README。
"""

from __future__ import annotations

import csv
import os
import re
import subprocess
import sys
from collections import Counter
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]      # .../STS2-SkinChanger_修改件
OUT = Path(__file__).resolve().parents[1]       # .../docs/refactor
TOOLS = OUT / "tools"
SCAN_ROOTS = ["STS2SkinChanger", "compat"]
SKIP_DIRS = {"bin", "obj"}
BASELINE = "092ec98 (1.0.31.93)"
HEAD = "092ec98"
REPO = ROOT.parent
# 任务书要求「Claude 同时在改的文件按开工前状态记」：这几个文件一律读 HEAD 版本。
BASELINE_TEXT: dict[Path, str] = {}
TRACKED: set[str] = set()


def git(*args: str) -> str:
    return subprocess.run(["git", "-C", str(REPO), *args], capture_output=True,
                          text=True, encoding="utf-8", errors="replace").stdout


def load_baseline() -> set[str]:
    """返回 HEAD 里存在的文件（相对 ROOT 的 posix 路径）；被改过的读 HEAD 内容。"""
    tracked = set()
    prefix = ROOT.name + "/"
    for line in git("ls-tree", "-r", "--name-only", HEAD).splitlines():
        line = line.strip()
        if line.startswith(prefix) and line.endswith(".cs"):
            tracked.add(line[len(prefix):])
    dirty = set()
    for line in git("diff", "--name-only", HEAD).splitlines():
        line = line.strip()
        if line.startswith(prefix) and line.endswith(".cs"):
            dirty.add(line[len(prefix):])
    for rel in dirty:
        text = git("show", f"{HEAD}:{prefix}{rel}")
        if text:
            BASELINE_TEXT[ROOT / rel] = text
    print(f"基线文件 {len(tracked)} 个；其中 {len(dirty)} 个被改过，读 {HEAD} 版本："
          + ", ".join(sorted(Path(d).name for d in dirty)))
    return tracked


# ================================================================== 基础工具

def iter_cs() -> list[Path]:
    out: list[Path] = []
    for rel in SCAN_ROOTS:
        base = ROOT / rel
        if not base.is_dir():
            continue
        for dirpath, dirnames, filenames in os.walk(base):
            dirnames[:] = [d for d in dirnames if d not in SKIP_DIRS]
            for name in filenames:
                if name.endswith(".cs"):
                    p = Path(dirpath) / name
                    # 只扫 HEAD 里有的文件（= 开工前就存在的源码）
                    if p.relative_to(ROOT).as_posix() in TRACKED:
                        out.append(p)
    return sorted(out)


def read_lines(path: Path) -> list[str]:
    if path in BASELINE_TEXT:
        return BASELINE_TEXT[path].splitlines()
    raw = path.read_bytes()
    for enc in ("utf-8-sig", "utf-8", "gb18030"):
        try:
            return raw.decode(enc).splitlines()
        except UnicodeDecodeError:
            continue
    return raw.decode("utf-8", "replace").splitlines()


def strip_comment(line: str) -> str:
    in_str = False
    i = 0
    while i < len(line):
        c = line[i]
        if in_str:
            if c == "\\":
                i += 2
                continue
            if c == '"':
                in_str = False
        elif c == '"':
            in_str = True
        elif c == "/" and line[i + 1:i + 2] == "/":
            return line[:i]
        i += 1
    return line


def brace_delta(code: str) -> int:
    depth = 0
    in_str = False
    i = 0
    while i < len(code):
        c = code[i]
        if in_str:
            if c == "\\":
                i += 2
                continue
            if c == '"':
                in_str = False
        elif c == '"':
            in_str = True
        elif c == "{":
            depth += 1
        elif c == "}":
            depth -= 1
        i += 1
    return depth


def expr_at(lines: list[str], idx: int, max_lines: int = 8) -> str:
    buf = strip_comment(lines[idx])
    bal = buf.count("(") - buf.count(")")
    j = idx
    while bal > 0 and j + 1 < len(lines) and j - idx < max_lines:
        j += 1
        nxt = strip_comment(lines[j])
        buf += " " + nxt.strip()
        bal += nxt.count("(") - nxt.count(")")
    return buf


DECL_RE = re.compile(r"\b(?:class|struct|interface|enum|record)\s+([A-Za-z_]\w*)")


NOT_METHODS = {
    "if", "while", "for", "foreach", "switch", "catch", "lock", "using", "return", "new", "get",
    "set", "nameof", "typeof", "FileStream", "StreamWriter", "BinaryWriter", "MemoryStream",
    "HarmonyMethod", "Harmony", "Action", "Func", "Predicate", "List", "Dictionary", "HashSet",
    "Queue", "Stack", "Task", "ValueTask", "Guid", "Path", "Directory", "File", "StringBuilder",
    "Exception", "IEnumerable", "Span", "FileInfo", "DirectoryInfo", "AssemblyName", "Version",
    "Uri", "Stream", "CancellationToken", "TimeSpan", "Regex", "StringComparer", "Comparer",
}


def enclosing_member(lines: list[str], idx: int) -> str:
    pat = re.compile(r"\b(?:private|public|internal|protected|static|async|override|virtual|"
                     r"partial|extern|unsafe|sealed|new)[\w\s<>,\[\]\?\.]*\b([A-Za-z_]\w*)\s*\(")
    for k in range(idx - 1, max(-1, idx - 80), -1):
        code = strip_comment(lines[k])
        if "(" not in code:
            continue
        m = pat.search(code)
        if m and m.group(1) not in NOT_METHODS:
            return m.group(1)
    return "(未知)"


def method_body(lines: list[str], decl_idx: int) -> tuple[str, int]:
    depth, started, body = 0, False, []
    for i in range(decl_idx, min(len(lines), decl_idx + 400)):
        code = strip_comment(lines[i])
        body.append(code)
        depth += brace_delta(code)
        if not started and "{" in code:
            started = True
        if started and depth <= 0:
            return "\n".join(body), i
    return "\n".join(body), min(len(lines) - 1, decl_idx + 400)


# ================================================================== 类型归属

def load_type_set(name: str) -> tuple[set[str], set[str]]:
    p = TOOLS / name
    if not p.is_file():
        return set(), set()
    full, simple = set(), set()
    for ln in read_lines(p):
        ln = ln.strip()
        if not ln:
            continue
        full.add(ln)
        simple.add(ln.split(".")[-1])
    return full, simple


GAME_FULL, GAME_SIMPLE = load_type_set("game-types-sts2-v0.113.0.txt")
GODOT_FULL, GODOT_SIMPLE = load_type_set("godot-types-v4.5.1.txt")

SELF_FULL: set[str] = set()
SELF_SIMPLE: set[str] = set()

THIRD_PARTY_HINTS = re.compile(
    r"^(?:BaseLib|Orchis|Midori|DefectSkin|AncientWaifu|KaiserCrab|EmperorCrab|Pengo|"
    r"HpWear|Thunninoi|RitsuLib|CustomSkeleton|NSpineAutoPlayer|SkinRegistry)\b")

PRIMITIVES = {"string", "bool", "int", "uint", "long", "ulong", "short", "ushort", "byte", "sbyte",
              "float", "double", "decimal", "char", "object", "void", "IntPtr", "UIntPtr", "nint", "nuint"}
BCL_PREFIX = re.compile(r"^(?:System\.|Microsoft\.|Mono\.|Newtonsoft\.|HarmonyLib\.)")
BCL_SIMPLE = {"AssemblyLoadContext", "Stream", "FileStream", "MemoryStream", "Type", "MethodInfo",
              "PropertyInfo", "FieldInfo", "ConstructorInfo", "Assembly", "Encoding", "IDisposable"}


def classify_type(full_name: str) -> str:
    """game / godot / self / thirdparty / unknown。

    口径：self = 非游戏、非第三方皮肤包（我们自己的类型、BCL、工具库）；
    unknown 只留给"目标写在变量/字符串里、静态判不出"的情况。
    """
    t = (full_name or "").strip()
    if not t or t in PRIMITIVES:
        return "unknown"
    t = t.split("<")[0].strip()
    if "." not in t and t in PRIMITIVES:
        return "unknown"
    simple = t.split(".")[-1]
    if THIRD_PARTY_HINTS.match(t):
        return "thirdparty"
    if t in GODOT_FULL or simple in GODOT_SIMPLE or t.startswith("Godot."):
        return "godot"
    if t in GAME_FULL or simple in GAME_SIMPLE:
        return "game"
    if BCL_PREFIX.match(t) or simple in BCL_SIMPLE:
        return "self"
    if t.startswith("STS2SkinChanger.") or t in SELF_FULL or simple in SELF_SIMPLE:
        return "self"
    if t.startswith("MegaCrit."):
        return "game"
    return "unknown"


def display_type(t: str) -> str:
    """游戏/Godot 类型统一显示成短名，便于跨文件聚合（如 OrbModel vs MegaCrit...OrbModel）。"""
    if not t:
        return t
    owner = classify_type(t)
    if owner in ("game", "godot") and "." in t:
        return t.split(".")[-1]
    return t


# ================================================================== 建议模块

COMPAT_RULES = [
    ("emperorcrabcompat", "compat:EmperorCrab"),
    ("ostydefectimpostor", "compat:OstyDefectImpostor"),
    ("pengotarot", "compat:PengoTarotFont"),
    ("customskeletonloader", "compat:CustomSkeletonLoader"),
    ("summonmodcompat", "compat:SummonMod"),
    ("ritsulib", "compat:RitsuLib"),
]

FILE_FEATURE = [
    ("merchant", "merchant"), ("background", "background"), ("ancient", "ancient"),
    ("event", "event"), ("card", "card"), ("relic", "card"), ("orb", "card"),
    ("osty", "osty"), ("companion", "osty"), ("monster", "monster"),
    ("bestiary", "monster"), ("multiplayer", "multiplayer"), ("lobby", "multiplayer"),
    ("workshop", "workshop"), ("character", "character"),
]

TARGET_FEATURE = [
    ("merchant", "merchant"), ("background", "background"), ("ancient", "ancient"),
    ("event", "event"), ("card", "card"), ("relic", "card"), ("orb", "card"),
    ("potion", "card"), ("energyicon", "card"), ("osty", "osty"),
    ("companion", "osty"), ("necrobinder", "character"), ("monster", "monster"),
    ("encounter", "monster"), ("bestiary", "monster"), ("multiplayer", "multiplayer"),
    ("lobby", "multiplayer"), ("net", "multiplayer"), ("workshop", "workshop"),
    ("character", "character"), ("creature", "character"),
]


def suggest_module(relfile: str, target_types: list[str], extra: str = "") -> tuple[str, str]:
    low = relfile.lower()
    for needle, mod in COMPAT_RULES:
        if needle in low:
            return mod, "兼容层文件"
    stem = Path(relfile).stem.lower()
    for needle, mod in FILE_FEATURE:
        if needle in stem:
            return mod, "文件名"
    for t in target_types:
        base = t.split("<")[0].split(".")[-1].lower()
        for needle, mod in TARGET_FEATURE:
            if needle in base:
                return mod, "目标类型"
    if target_types and classify_type(target_types[0]) == "game":
        return "core", "游戏基础设施（框架/启动/存档类，非具体功能）"
    if "/ui/" in low.replace("\\", "/"):
        return "ui-common", "UI 目录"
    if any(x in low.replace("\\", "/") for x in ("/core/", "/catalog/", "/pck/")):
        return "core", "Core 目录"
    if extra:
        base = extra.lower()
        for needle, mod in TARGET_FEATURE:
            if needle in base:
                return mod, "类名"
    return "unknown", "未命中规则"


# ================================================================== ① 补丁

PATCH_KINDS = {
    "Prefix": re.compile(r"\[HarmonyPrefix\]|\bPrefix\s*\("),
    "Postfix": re.compile(r"\[HarmonyPostfix\]|\bPostfix\s*\("),
    "Transpiler": re.compile(r"\[HarmonyTranspiler\]|\bTranspiler\s*\("),
    "Finalizer": re.compile(r"\[HarmonyFinalizer\]|\bFinalizer\s*\("),
}


def parse_attr_targets(attr_text: str) -> tuple[list[tuple[str, str]], str]:
    types = [m.group(1) for m in re.finditer(r"typeof\(\s*([A-Za-z0-9_\.<>]+)\s*\)", attr_text)]
    member = ""
    mm = re.search(r"nameof\(\s*([A-Za-z0-9_\.]+)\s*\)", attr_text)
    if mm:
        member = mm.group(1).split(".")[-1]
    else:
        ms = re.search(r'"([^"]+)"', attr_text)
        if ms:
            member = ms.group(1)
    notes = []
    if re.search(r"MethodType\.(Getter|Setter)", attr_text):
        notes.append(re.search(r"MethodType\.(Getter|Setter)", attr_text).group(1) + " 属性访问器")
    if "MethodType.Constructor" in attr_text:
        member = ".ctor"
        notes.append("构造函数")
    if len(types) > 1:
        notes.append("参数签名: " + ", ".join(types[1:]))
    if not types:
        return [], "; ".join(notes)
    return [(types[0], member)], "; ".join(notes)


def scan_attribute_patches() -> tuple[list[dict], int]:
    rows: list[dict] = []
    classes = 0
    for path in iter_cs():
        lines = read_lines(path)
        rel = path.relative_to(ROOT).as_posix()
        i, n = 0, len(lines)
        while i < n:
            if not re.match(r"^\s*\[Harmony", strip_comment(lines[i])):
                i += 1
                continue
            parts: list[str] = []
            j = i
            while j < n:
                cur = strip_comment(lines[j]).strip()
                if cur.startswith("["):
                    parts.append(cur)
                    bal = sum(p.count("[") - p.count("]") for p in parts)
                    j += 1
                    while bal > 0 and j < n:
                        more = strip_comment(lines[j]).strip()
                        parts.append(more)
                        bal += more.count("[") - more.count("]")
                        j += 1
                    continue
                if not cur:
                    j += 1
                    continue
                break
            attr_text = " ".join(parts)
            if "[HarmonyPatch" not in attr_text:
                i = j + 1 if j > i else i + 1
                continue
            k = j
            while k < n and (not lines[k].strip() or lines[k].strip().startswith("[")):
                k += 1
            cm = DECL_RE.search(strip_comment(lines[k]) if k < n else "")
            if not cm:
                i = j + 1 if j > i else i + 1
                continue
            cls = cm.group(1)
            body, bend = method_body(lines, k)
            kinds = [name for name, rx in PATCH_KINDS.items() if rx.search(body)]
            pairs, note = parse_attr_targets(attr_text)
            classes += 1
            if not pairs:
                found = re.findall(
                    r"AccessTools\.(?:Method|PropertyGetter|PropertySetter|Constructor)\(\s*typeof\(\s*([A-Za-z0-9_\.]+)\s*\)\s*,\s*(?:nameof\(\s*([A-Za-z0-9_\.]+)\s*\)|\"([^\"]+)\")",
                    body)
                if found:
                    pairs = list(dict.fromkeys((t, (nm or raw).split(".")[-1]) for t, nm, raw in found))
                    note = (note + "; " if note else "") + "TargetMethod(s) 列出"
                elif "TargetMethods" in body:
                    pairs = [("(动态)", "TargetMethods 全类型扫描")]
                    note = (note + "; " if note else "") + "目标在运行时枚举"
                elif "TargetMethod" in body:
                    pairs = [("(动态)", "TargetMethod")]
                    note = (note + "; " if note else "") + "目标在运行时解析"
                else:
                    pairs = [("(未解析)", "")]
                    note = (note + "; " if note else "") + "特性无目标且无 TargetMethod"
            module, why = suggest_module(rel, [t for t, _ in pairs], cls)
            for t, mem in pairs:
                rows.append({
                    "file": rel,
                    "line": str(i + 1),
                    "patch_class": cls,
                    "mode": "attribute",
                    "target_type": display_type(t),
                    "target_member": mem,
                    "kinds": "+".join(kinds) if kinds else "?",
                    "owner": classify_type(t) if t not in ("(动态)", "(未解析)") else "unknown",
                    "module": module,
                    "note": (note + "; " if note else "") + f"模块判定: {why}",
                })
            i = bend + 1
    return rows, classes


CALL_RE = re.compile(r"\b(PatchAll|Patch)\s*\(")


def scan_manual_patches() -> list[dict]:
    rows: list[dict] = []
    for path in iter_cs():
        lines = read_lines(path)
        rel = path.relative_to(ROOT).as_posix()
        for idx, raw in enumerate(lines):
            if raw.lstrip().startswith(("//", "///", "*")):
                continue
            code = strip_comment(raw)
            m = CALL_RE.search(code)
            if not m:
                continue
            if m.group(1) == "Patch" and not re.search(
                    r"(harmony|Harmony|Observer|Patches|CreateClassProcessor|\.Patch\()", code):
                continue
            expr = expr_at(lines, idx)
            mode = "patchall" if m.group(1) == "PatchAll" else "manual"
            if "CreateClassProcessor" in code:
                rows.append({
                    "file": rel, "line": str(idx + 1), "patch_class": "(批量安装)", "mode": "manual",
                    "target_type": "(全部 [HarmonyPatch] 类)", "target_member": "", "kinds": "",
                    "owner": "self", "module": "core",
                    "note": f"批量安装入口，所在方法 {enclosing_member(lines, idx)}",
                })
                continue
            tm = re.search(
                r"AccessTools\.(?:Method|PropertyGetter|PropertySetter|Constructor|Field)\(\s*typeof\(\s*([A-Za-z0-9_\.]+)\s*\)\s*,\s*(?:nameof\(\s*([A-Za-z0-9_\.]+)\s*\)|\"([^\"]+)\")",
                expr)
            if tm:
                ttype, tmem = tm.group(1), (tm.group(2) or tm.group(3)).split(".")[-1]
            else:
                tm2 = re.search(r"typeof\(\s*([A-Za-z0-9_\.]+)\s*\)", expr)
                ttype = tm2.group(1) if tm2 else "(运行时解析)"
                tmem = ""
            module, why = suggest_module(rel, [ttype], "")
            kinds = "+".join(k for k, rx in PATCH_KINDS.items()
                            if re.search(k.lower(), expr.lower())) or "?"
            rows.append({
                "file": rel, "line": str(idx + 1), "patch_class": "(手动)", "mode": mode,
                "target_type": display_type(ttype), "target_member": tmem, "kinds": kinds,
                "owner": classify_type(ttype) if ttype != "(运行时解析)" else "unknown",
                "module": module,
                "note": f"所在方法 {enclosing_member(lines, idx)}；模块判定: {why}",
            })
    return rows


# ================================================================== ② 风险点

WRITE_RE = re.compile(
    r"(File\.(?:WriteAllText|WriteAllBytes|WriteAllLines|AppendAllText|AppendAllLines|Copy|Create|CreateText|Move|Delete|Replace|OpenWrite)"
    r"|new\s+FileStream\s*\(|new\s+StreamWriter\s*\(|new\s+BinaryWriter\s*\("
    r"|Directory\.CreateDirectory|ZipFile\.CreateFromDirectory"
    r"|PckArchive\.Write\w*|Storage\.WriteCache|SkinChangerPaths\.WriteCache)")

CAP_RE = re.compile(
    r"(?i)(?:Max\w*(?:Bytes|Size|Count|Files|Entries|Attempts|Frames)|Trim\w*Budget|Budget|"
    r"上限|Limit\s*=\s*\d+|\d+(?:\.\d+)?\s*(?:GB|MB|KB))")

WHAT_RULES = [
    (r"\.pck|PckArchive|OverlayPack|overlay", "覆盖包 PCK"),
    (r"(?i)cache", "缓存"),
    (r"(?i)log", "日志"),
    (r"(?i)http|download|Download|WebClient", "下载"),
    (r"(?i)config|settings|\.json|Save\(", "配置"),
]


def guess_what(code: str, window: str) -> str:
    for rx, label in WHAT_RULES:
        if re.search(rx, code):
            return label
    for rx, label in WHAT_RULES:
        if re.search(rx, window):
            return label
    return "其他/未分类"


READONLY_STREAMS = 0


def scan_writes() -> list[dict]:
    global READONLY_STREAMS
    rows: list[dict] = []
    for path in iter_cs():
        lines = read_lines(path)
        rel = path.relative_to(ROOT).as_posix()
        joined = "\n".join(strip_comment(x) for x in lines)
        for idx, raw in enumerate(lines):
            if raw.lstrip().startswith(("//", "///", "*", "/*")):
                continue
            code = strip_comment(raw)
            m = WRITE_RE.search(code)
            if not m:
                continue
            api = m.group(0)
            window = "\n".join(strip_comment(x) for x in lines[max(0, idx - 6): idx + 8])
            if api.startswith("new FileStream"):
                forward = "\n".join(strip_comment(x) for x in lines[idx: idx + 6])
                if re.search(r"FileAccess\.Read\b", forward) and not re.search(r"FileAccess\.(?:Write|ReadWrite)", forward):
                    READONLY_STREAMS += 1          # 只读扫描（IL/元数据），不是写盘
                    continue
            what = "目录创建（不写数据）" if api == "Directory.CreateDirectory" else guess_what(code, window)
            cap = ""
            for c in CAP_RE.finditer(window):
                cap = c.group(0)
                break
            if not cap:
                for mm in re.finditer(r"\d+(?:\.\d+)?\s*(?:GB|MB|KB)", joined):
                    cap = "文件级: " + mm.group(0)
                    break
            if not cap and CAP_RE.search(joined):
                cap = "文件内另有上限常量"
            rows.append({
                "site": f"{rel}:{idx + 1}",
                "line": str(idx + 1),
                "file": rel,
                "member": enclosing_member(lines, idx),
                "api": api,
                "what": what,
                "cap": cap or "⚠ 无",
                "code": code.strip()[:150],
                "module": suggest_module(rel, [])[0],
            })
    return rows


LOOP_HEAD = re.compile(r"\bwhile\s*\(\s*true\s*\)|\bfor\s*\(\s*;\s*;\s*\)|\bfor\s*\(\s*;;\s*\)")
FRAME_HOOKS = re.compile(r"\b(?:override|private|public|internal|protected)\s+[\w<>\[\]\.]+\s+"
                         r"(?:_Process|_PhysicsProcess|_Notification)\s*\(")
FRAME_SUB = re.compile(r"\.\s*ProcessFrame\s*\+=\s*([A-Za-z_]\w*)")
MOUNT_CALLS = re.compile(r"\b(?:MountOverlay\w*|MountArchiveOverlay|TryRebuild\w*|Rebuild\w*|"
                         r"Refresh\w*|Apply\w*Selection\w*|WriteCache|Write\w*Pck)\s*\(")
WORK_CALLS = re.compile(r"(\b(?:Mount|Rebuild|Refresh|Apply|Write|Save|Update|Tick|Process)\w*)\s*\(")
DEF_RE = re.compile(r"\b(?:void|bool|int|Task|ValueTask|IEnumerable<[^>]+>|async\s+void)\s+([A-Za-z_]\w*)\s*\(")


def find_member_body(lines: list[str], name: str) -> str:
    for idx, ln in enumerate(lines):
        if ln.lstrip().startswith(("//", "///")):
            continue
        m = DEF_RE.search(strip_comment(ln))
        if m and m.group(1) == name:
            body, _ = method_body(lines, idx)
            return body
    return ""


def frame_work_chain(lines: list[str], body: str, depth: int = 2) -> list[str]:
    """每帧钩子直接或经一层调用后，是否触发重建/挂载/写盘。"""
    hits = sorted({c.group(0).rstrip("(") for c in MOUNT_CALLS.finditer(body)})
    if hits or depth <= 0:
        return hits
    for call in sorted({c.group(1) for c in WORK_CALLS.finditer(body)}):
        if call in ("Mount", "Rebuild", "Refresh", "Apply", "Write", "Save"):
            continue
        sub = find_member_body(lines, call)
        if not sub or sub == body:
            continue
        sub_hits = frame_work_chain(lines, sub, depth - 1)
        if sub_hits:
            return [f"{call} → " + ", ".join(sub_hits)]
    return []


def scan_resource_saves() -> int:
    """任务书表 1 不含 `Resource/.Save()`；这里只统计个数，写进 risk-sites 的说明里。"""
    n = 0
    for path in iter_cs():
        for ln in read_lines(path):
            if ln.lstrip().startswith(("//", "///")):
                continue
            if re.search(r"\.Save\s*\(\s*\w+\s*\)", strip_comment(ln)):
                n += 1
    return n


def scan_loops() -> tuple[list[dict], list[dict]]:
    infinite: list[dict] = []
    frames: list[dict] = []
    for path in iter_cs():
        lines = read_lines(path)
        rel = path.relative_to(ROOT).as_posix()
        for idx, raw in enumerate(lines):
            if raw.lstrip().startswith(("//", "///")):
                continue
            code = strip_comment(raw)
            if LOOP_HEAD.search(code):
                body, _ = method_body(lines, idx)
                exit_hits = []
                for mm in re.finditer(r"\b(?:break|return)\b", body):
                    snippet = body[max(0, mm.start() - 90): mm.start() + 20].strip().splitlines()
                    exit_hits.append(snippet[-1] if snippet else "")
                infinite.append({
                    "site": f"{rel}:{idx + 1}",
                    "file": rel,
                    "line": str(idx + 1),
                    "member": enclosing_member(lines, idx),
                    "head": code.strip()[:80],
                    "exit": "有 break/return" if exit_hits else "⚠ 未找到 break/return",
                    "exit_sample": exit_hits[0][:110] if exit_hits else "",
                    "module": suggest_module(rel, [])[0],
                })
            hook_name = None
            body = ""
            if FRAME_HOOKS.search(code):
                m = re.search(r"(_Process|_PhysicsProcess|_Notification)\s*\(", code)
                hook_name = m.group(1) if m else None
                body, _ = method_body(lines, idx)
            else:
                m = FRAME_SUB.search(code)
                if m:
                    hook_name = "ProcessFrame += " + m.group(1)
                    body = find_member_body(lines, m.group(1))
            if hook_name and body:
                chain = frame_work_chain(lines, body)
                direct = sorted({c.group(1) for c in WORK_CALLS.finditer(body)})[:6]
                frames.append({
                    "site": f"{rel}:{idx + 1}",
                    "file": rel,
                    "line": str(idx + 1),
                    "member": hook_name,
                    "calls": "; ".join(chain)[:160] if chain else "直接调用: " + ", ".join(direct)[:120],
                    "note": "每帧钩子会触发重建/挂载/刷新/写盘" if chain else "每帧钩子（未直接命中重建/挂载/写盘，供人工判断）",
                    "module": suggest_module(rel, [])[0],
                })
    return infinite, frames


RETRY_TOKEN = re.compile(r"(?i)retry|attempt|重试")
STR_LITERAL = re.compile(r'"(?:\\.|[^"\\])*"')
NUM_LIMIT = re.compile(
    r"(?i)(?:>=\s*(\d+)|<\s*(\d+)|Max(?:Attempts|Retries|Frames)\s*=\s*(\d+)|"
    r"(\d+)\s*(?:帧|frames)|TimeSpan\.FromSeconds\(\s*(\d+(?:\.\d+)?))")


def scan_retries() -> list[dict]:
    groups: dict[tuple[str, str], dict] = {}
    for path in iter_cs():
        lines = read_lines(path)
        rel = path.relative_to(ROOT).as_posix()
        for idx, raw in enumerate(lines):
            if raw.lstrip().startswith(("//", "///")):
                continue
            code = strip_comment(raw)
            if not RETRY_TOKEN.search(STR_LITERAL.sub('""', code)):
                continue
            member = enclosing_member(lines, idx)
            key = (rel, member)
            g = groups.setdefault(key, {"file": rel, "member": member, "line": str(idx + 1),
                                        "count": 0, "first_code": code.strip()[:110], "limits": set()})
            g["count"] += 1
            for x in lines[max(0, idx - 8): idx + 9]:
                for m in NUM_LIMIT.finditer(strip_comment(x)):
                    for v in m.groups():
                        if v:
                            g["limits"].add(v)
    rows: list[dict] = []
    for g in groups.values():
        plausible = sorted({v for v in g["limits"] if v.isdigit() and 1 <= int(v) <= 100000}, key=int)
        cap = "、".join(plausible[:4]) if plausible else "⚠ 未见上限"
        rows.append({
            "site": f"{g['file']}:{g['line']}",
            "line": g["line"],
            "file": g["file"],
            "member": g["member"],
            "retry_mentions": str(g["count"]),
            "cap": cap,
            "first_code": g["first_code"],
            "module": suggest_module(g["file"], [])[0],
        })
    return sorted(rows, key=lambda r: (r["file"], int(r["line"])))


# ================================================================== ③ 反射

REFLECT_DETECT = re.compile(
    r"AccessTools\.\w+|\bGetMethod\s*\(|\bGetProperty\s*\(|\bGetField\s*\(|"
    r"\bGetType\s*\(\s*\"|\bTypeByName\s*\(")


def reflect_labels(code: str) -> list[str]:
    """把命中片段归成 查找方式 标签（同一行多种则并列）。"""
    labels = set()
    for m in REFLECT_DETECT.finditer(code):
        tok = m.group(0)
        if tok.startswith("AccessTools."):
            labels.add(tok.rstrip("("))
        elif tok.startswith("GetType"):
            labels.add("GetType(\"...\")")
        else:
            labels.add(tok.rstrip("(").strip())
    return sorted(labels)


def fail_behavior(lines: list[str], idx: int) -> tuple[str, str]:
    text = "\n".join(strip_comment(x) for x in lines[idx: idx + 7])
    if re.search(r"\?\?\s*throw|throw\s+new|\bthrow\b", text):
        return "抛异常", "后续有 throw"
    if re.search(r"ModLog\.(?:Warn|Error|Info|Debug)|Log\.(?:Warn|Error|Info)", text):
        return "日志", "后续有日志调用"
    if re.search(r"\?\?\s*return|return\s+null|return\s+false|is\s+null|==\s*null|is\s+not\s+null|!=\s*null", text):
        return "返回null静默", "后续有 null 判断/回退"
    return "未处理", "查找后无显式兜底"


def scan_reflection() -> list[dict]:
    rows: list[dict] = []
    for path in iter_cs():
        lines = read_lines(path)
        rel = path.relative_to(ROOT).as_posix()
        for idx, raw in enumerate(lines):
            if raw.lstrip().startswith(("//", "///")):
                continue
            code = strip_comment(raw)
            labels = reflect_labels(code)
            if not labels:
                continue
            expr = expr_at(lines, idx)
            ttype = ""
            tmem = ""
            m = re.search(r"AccessTools\.(?:Method|PropertyGetter|PropertySetter|Constructor|Field)\(\s*typeof\(\s*([A-Za-z0-9_\.]+)\s*\)\s*,\s*(?:nameof\(\s*([A-Za-z0-9_\.]+)\s*\)|\"([^\"]+)\")", expr)
            if m:
                ttype, tmem = m.group(1), (m.group(2) or m.group(3)).split(".")[-1]
            if not ttype:
                m2 = re.search(r"(?:AccessTools\.TypeByName|GetType|Type\.GetType|Assembly\.GetType)\(\s*\"([A-Za-z0-9_\.\+]+)\"", expr)
                if m2:
                    ttype = m2.group(1)
            if not ttype:
                # 变量. GetMethod("名") → 目标在变量里，只能记变量名
                m4 = re.search(r"([A-Za-z_][\w\.]*)\s*\.\s*Get(?:Method|Property|Field)\s*\(\s*\"([^\"]+)\"", expr)
                if m4:
                    recv = m4.group(1)
                    ttype = recv if "." in recv else "变量:" + recv
                    tmem = m4.group(2)
            if not ttype:
                m3 = re.search(r"typeof\(\s*([A-Za-z0-9_\.]+)\s*\)", expr)
                if m3 and m3.group(1) not in PRIMITIVES:
                    ttype = m3.group(1)
            if not tmem:
                m5 = re.search(r"Get(?:Method|Property|Field)\s*\([^)]*\"([^\"]+)\"", expr)
                if m5:
                    tmem = m5.group(1)
            behavior, why = fail_behavior(lines, idx)
            owner = classify_type(ttype.replace("变量:", "")) if ttype else "unknown"
            if ttype.startswith("变量:"):
                owner = "unknown"
            module, _ = suggest_module(rel, [ttype] if ttype else [], "")
            rows.append({
                "file": rel,
                "line": str(idx + 1),
                "api": "+".join(labels),
                "target_type": display_type(ttype) if ttype else "(变量/动态)",
                "target_member": tmem,
                "owner": owner,
                "fail": behavior,
                "note": f"{why}；所在方法 {enclosing_member(lines, idx)}",
                "module": module,
            })
    return rows


# ================================================================== 输出

HEADERS = {
    "file": "文件", "line": "行号", "patch_class": "补丁类", "mode": "方式",
    "target_type": "目标类型", "target_member": "目标方法", "kinds": "补丁种类",
    "owner": "目标归属", "module": "建议模块", "note": "备注",
    "api": "查找方式", "fail": "失败时行为",
    "site": "位置", "member": "所在方法", "what": "写的是什么", "cap": "上限",
    "head": "循环头", "exit": "退出条件", "exit_sample": "退出样本",
    "calls": "会调用", "retry_mentions": "retry提及", "first_code": "首行", "code": "代码",
}


def write_csv(name: str, rows: list[dict], fields: list[str], labels: list[str] | None = None) -> None:
    cols = labels or [HEADERS.get(f, f) for f in fields]
    with (OUT / name).open("w", encoding="utf-8-sig", newline="") as f:
        w = csv.writer(f)
        w.writerow(cols)
        for r in rows:
            w.writerow([r.get(f, "") for f in fields])


def md_table(headers: list[str], rows: list[list[str]]) -> str:
    out = ["| " + " | ".join(headers) + " |",
           "|" + "|".join(["---"] * len(headers)) + "|"]
    for r in rows:
        out.append("| " + " | ".join(str(c).replace("|", "\\|").replace("\n", " ") for c in r) + " |")
    return "\n".join(out)


def write_patches_summary(attr_rows: list[dict], manual_rows: list[dict], class_count: int) -> None:
    all_rows = attr_rows + manual_rows
    by_mod = Counter(r["module"] for r in all_rows)
    by_owner = Counter(r["owner"] for r in all_rows)
    by_type = Counter(r["target_type"] for r in all_rows
                      if r["target_type"] not in ("(动态)", "(未解析)", "(运行时解析)"))
    dyn = sum(1 for r in all_rows if r["target_type"] in ("(动态)", "(未解析)"))
    rt = sum(1 for r in all_rows if r["target_type"] == "(运行时解析)")
    multi = Counter((r["target_type"], r["target_member"]) for r in all_rows
                    if r["target_member"] and r["target_type"] not in
                    ("(动态)", "(未解析)", "(运行时解析)", "(全部 [HarmonyPatch] 类)"))
    hot = [(t, m, c) for (t, m), c in multi.most_common() if c >= 3]

    parts = [f"# 补丁清单汇总（①）\n",
             f"基线 `{BASELINE}`；扫描范围 `STS2SkinChanger/` + `compat/`（不含 tests/tools/bin/obj）。明细见 `patches.csv`。\n",
             f"- `[HarmonyPatch]` 补丁类：**{class_count}** 个",
             f"- 明细：特性 **{len(attr_rows)}** 行 + 手动安装 **{len(manual_rows)}** 行\n",
             "## 按模块\n",
             md_table(["模块", "行数"], [[k, v] for k, v in by_mod.most_common()]),
             "\n口径：模块按**文件名 → 目标类型 → 目录**的顺序判定（兼容层按目录归 `compat:<包名>`）；"
             "`core` 含框架/启动/存档/PCK/装载等非具体功能的补丁。\n",
             "\n## 按目标归属\n",
             md_table(["归属", "行数"], [[k, v] for k, v in by_owner.most_common()]),
             f"\n口径：`game` = 目标类型在 `sts2.dll` v0.113.0 里；`self` = 我们自己的类型 / BCL；"
             f"`thirdparty` = provider 包；`unknown` = **{dyn + rt}** 行（{dyn} 动态目标 + {rt} `(运行时解析)`）。\n",
             "\n## 按目标类型 Top 30\n",
             md_table(["目标类型", "补丁数"], [[k, v] for k, v in by_type.most_common(30)]),
             "\n## 同一游戏方法挂了 ≥3 个补丁（双份执行高风险点）\n"]
    parts.append(md_table(["目标类型", "方法", "补丁数"], [[t, m, c] for t, m, c in hot]) if hot else "无（阈值 3）。")
    (OUT / "patches-summary.md").write_text("\n".join(parts), encoding="utf-8", newline="\n")


def write_risk_sites(writes: list[dict], infinite: list[dict], frames: list[dict],
                     retries: list[dict], save_count: int) -> None:
    parts = [f"# 风险点清单（②）\n",
             f"基线 `{BASELINE}`。位置 = `文件:行号`；方法名由向上回溯最近声明得到，仅供参考。\n",
             f"## 1. 写盘（{len(writes)} 处）\n",
             f"口径按任务书：`File.Write*/Copy/Move/Create/AppendAll*`、`new FileStream`、`OpenWrite`、"
             f"`PckArchive.Write*`、`Directory.CreateDirectory`。",
             f"另有 **{save_count} 处** `资源/配置 .Save(...)`（Godot `Resource.Save`、配置序列化）不在本表口径内，"
             f"但它们同样落盘，`DiskGuard` 接入时要一起考虑；"
             f"**{READONLY_STREAMS} 处** `new FileStream(..., FileAccess.Read)` 是只读的 IL/元数据扫描，"
             f"已从表里剔除。\n",
             md_table(["位置", "所在方法", "API", "写的是什么", "上限"],
                      [[f"`{r['file']}:{r['line']}`", r["member"], r["api"], r["what"], r["cap"]]
                       for r in writes]),
             f"\n## 2. 无界循环 / 每帧触发（{len(infinite)} 处无限循环 + {len(frames)} 处每帧钩子）\n",
             "### 2.1 无限循环\n",
             md_table(["位置", "所在方法", "循环头", "退出条件", "样本"],
                      [[f"`{r['file']}:{r['line']}`", r["member"], r["head"], r["exit"], r["exit_sample"]]
                       for r in infinite]),
             "\n### 2.2 每帧钩子里会触发重建/挂载/写盘\n",
             (md_table(["位置", "钩子", "会调用", "说明"],
                       [[f"`{r['file']}:{r['line']}`", r["member"], r["calls"], r["note"]] for r in frames])
              if frames else "无。"),
             f"\n## 3. 重试（{len(retries)} 处逻辑块）\n",
             f"按「同一方法内所有 retry/attempt/重试」聚成一块；**{sum(1 for r in retries if '⚠' in r['cap'])} 块没有可识别的数值上限**（标 ⚠）。"
             "「上限（数字）」是从块内代码里抽出的常量，可能是帧数/次数/秒，也可能是无关数字，需对照源码确认。\n",
             md_table(["位置", "所在方法", "retry 提及", "上限（数字）", "首行"],
                      [[f"`{r['file']}:{r['line']}`", r["member"], r["retry_mentions"], r["cap"], r["first_code"]]
                       for r in retries])]
    (OUT / "risk-sites.md").write_text("\n".join(parts), encoding="utf-8", newline="\n")


def write_reflection_summary(reflect: list[dict]) -> None:
    by_owner = Counter(r["owner"] for r in reflect)
    by_fail = Counter(r["fail"] for r in reflect)
    risky = [r for r in reflect if r["owner"] == "game" and r["fail"] in ("未处理", "抛异常")]
    dyn = sum(1 for r in reflect if r["target_type"] == "(变量/动态)")
    var = sum(1 for r in reflect if r["target_type"].startswith("变量:"))
    dup = Counter((r["target_type"], r["target_member"]) for r in reflect
                  if r["target_type"] != "(变量/动态)" and r["target_member"])
    merged = [(t, m, c) for (t, m), c in dup.most_common() if c >= 2]

    parts = [f"# 反射分类汇总（③）\n",
             f"基线 `{BASELINE}`。明细见 `reflection.csv`（共 {len(reflect)} 行）。",
             "失败时行为是**静态启发式**：看查找语句之后 7 行里有没有 `throw` / 日志调用 / null 判断，都没有就记「未处理」。它不等于运行时会崩，只是「代码里没显式兜底」。\n",
             "归属口径：`game` = 目标类型在 `sts2.dll`（v0.113.0，6378 个类型定义）里；`godot` = GodotSharp；"
             "`self` = 我们自己的类型 / BCL / 工具库；`thirdparty` = provider 包；"
             f"`unknown` = **{by_owner.get('unknown', 0)}** 行，其中 {dyn} 行目标完全动态、{var} 行只知道变量名（如 `变量:type`），静态判不出。\n",
             "## 按归属\n",
             md_table(["归属", "行数"], [[k, v] for k, v in by_owner.most_common()]),
             "\n## 按失败时行为\n",
             md_table(["行为", "行数"], [[k, v] for k, v in by_fail.most_common()]),
             f"\n## 目标是游戏本体、失败时「未处理」或「抛异常」（{len(risky)} 行，阶段 B 优先收拢）\n"]
    if risky:
        parts.append(md_table(["位置", "API", "目标", "失败行为", "备注"],
                              [[f"`{r['file']}:{r['line']}`", r["api"],
                                f"{r['target_type']}.{r['target_member']}".rstrip("."),
                                r["fail"], r["note"]] for r in risky[:80]]))
        if len(risky) > 80:
            parts.append("\n（只列前 80 行，完整看 `reflection.csv`。）")
    else:
        parts.append("无。")
    parts.append(f"\n## 同一成员被多处反射查找（≥2 次，合并候选：{len(merged)} 组）\n")
    if merged:
        parts.append(md_table(["目标类型", "成员", "次数"], [[t, m, c] for t, m, c in merged[:60]]))
        if len(merged) > 60:
            parts.append("\n（只列前 60 组。）")
    else:
        parts.append("无。")
    (OUT / "reflection-summary.md").write_text("\n".join(parts), encoding="utf-8", newline="\n")


# ================================================================== 校验

def verify(attr_rows: list[dict], manual_rows: list[dict], writes: list[dict],
           loops: list[dict], frames: list[dict], retries: list[dict], reflect: list[dict]) -> int:
    cache: dict[str, list[str]] = {}
    bad = 0

    def lines_of(rel: str) -> list[str]:
        if rel not in cache:
            cache[rel] = read_lines(ROOT / rel)
        return cache[rel]

    for r in attr_rows:
        lines = lines_of(r["file"])
        ln = int(r["line"]) - 1
        if ln >= len(lines) or "[Harmony" not in lines[ln]:
            print(f"  [补丁] {r['file']}:{r['line']} 不含 [Harmony"); bad += 1
    for r in manual_rows:
        lines = lines_of(r["file"])
        ln = int(r["line"]) - 1
        if ln >= len(lines) or "Patch" not in lines[ln]:
            print(f"  [手动] {r['file']}:{r['line']} 不含 Patch"); bad += 1
    for r in writes:
        lines = lines_of(r["file"])
        ln = int(r["line"]) - 1
        if ln >= len(lines) or r["api"].split("(")[0] not in lines[ln]:
            print(f"  [写盘] {r['file']}:{r['line']} 不含 {r['api']}"); bad += 1
    for r in loops:
        lines = lines_of(r["file"])
        ln = int(r["line"]) - 1
        if ln >= len(lines) or not re.search(r"\bwhile\b|\bfor\b", lines[ln]):
            print(f"  [循环] {r['file']}:{r['line']} 不含 while/for"); bad += 1
    for r in frames:
        lines = lines_of(r["file"])
        ln = int(r["line"]) - 1
        if ln >= len(lines) or not re.search(r"_Process|_PhysicsProcess|_Notification|ProcessFrame", lines[ln]):
            print(f"  [每帧] {r['file']}:{r['line']} 不含每帧钩子"); bad += 1
    for r in retries:
        lines = lines_of(r["file"])
        ln = int(r["line"]) - 1
        if ln >= len(lines) or not RETRY_TOKEN.search(lines[ln]):
            print(f"  [重试] {r['file']}:{r['line']} 不含 retry 标识"); bad += 1
    for r in reflect:
        lines = lines_of(r["file"])
        ln = int(r["line"]) - 1
        if ln >= len(lines) or not reflect_labels(strip_comment(lines[ln])):
            print(f"  [反射] {r['file']}:{r['line']} 不含反射 API"); bad += 1
    return bad


# ================================================================== main

def main() -> int:
    global TRACKED
    TRACKED = load_baseline()
    files = iter_cs()
    for p in files:
        for ln in read_lines(p):
            if ln.lstrip().startswith(("//", "///")):
                continue
            m = DECL_RE.search(strip_comment(ln))
            if m:
                SELF_SIMPLE.add(m.group(1))
                SELF_FULL.add(m.group(1))

    attr_rows, class_count = scan_attribute_patches()
    manual_rows = scan_manual_patches()
    writes = scan_writes()
    save_count = scan_resource_saves()
    infinite, frames = scan_loops()
    retries = scan_retries()
    reflect = scan_reflection()

    write_csv("patches.csv", attr_rows + manual_rows,
              ["file", "line", "patch_class", "mode", "target_type", "target_member",
               "kinds", "owner", "module", "note"])
    write_csv("reflection.csv", reflect,
              ["file", "line", "api", "target_type", "target_member", "owner", "fail", "note", "module"],
              ["文件", "行号", "查找方式", "目标类型名", "成员名", "归属", "失败时行为", "备注", "建议模块"])
    write_csv("disk_writes.csv", writes,
              ["site", "line", "file", "member", "api", "what", "cap", "module", "code"])
    write_csv("loops.csv", infinite,
              ["site", "file", "line", "member", "head", "exit", "exit_sample", "module"])
    write_csv("frame_hooks.csv", frames,
              ["site", "file", "line", "member", "calls", "note", "module"])
    write_csv("retry.csv", retries,
              ["site", "line", "file", "member", "retry_mentions", "cap", "first_code", "module"])

    write_patches_summary(attr_rows, manual_rows, class_count)
    write_risk_sites(writes, infinite, frames, retries, save_count)
    write_reflection_summary(reflect)

    bad = verify(attr_rows, manual_rows, writes, infinite, frames, retries, reflect)

    print(f"源码文件            : {len(files)}（类型表 game={len(GAME_FULL)} godot={len(GODOT_FULL)}）")
    print(f"① 补丁类            : {class_count}  明细 {len(attr_rows)} 行（特性）/ {len(manual_rows)} 行（手动）")
    print(f"② 写盘 {len(writes)}   无限循环 {len(infinite)}   每帧钩子 {len(frames)}   重试块 {len(retries)}")
    print(f"③ 反射              : {len(reflect)}")
    print(f"回读校验            : {'全部通过' if bad == 0 else str(bad) + ' 条不一致'}")
    print()
    print("① 按模块：")
    for mod, cnt in Counter(r["module"] for r in attr_rows + manual_rows).most_common():
        print(f"  {mod:<26}{cnt}")
    print("① 按归属：")
    for o, cnt in Counter(r["owner"] for r in attr_rows + manual_rows).most_common():
        print(f"  {o:<12}{cnt}")
    print()
    print("③ 反射归属：")
    for o, cnt in Counter(r["owner"] for r in reflect).most_common():
        print(f"  {o:<12}{cnt}")
    print("③ 失败行为：")
    for o, cnt in Counter(r["fail"] for r in reflect).most_common():
        print(f"  {o:<12}{cnt}")
    print()
    print("② 写盘按文件 Top8：")
    for f, cnt in Counter(r["file"] for r in writes).most_common(8):
        print(f"  {cnt:>3}  {f}")
    print("② 重试无上限块：", sum(1 for r in retries if "⚠" in r["cap"]), "/", len(retries))
    return 0


if __name__ == "__main__":
    sys.exit(main())
