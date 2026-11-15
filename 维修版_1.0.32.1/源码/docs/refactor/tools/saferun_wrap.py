#!/usr/bin/env python3
"""④ 第 1 批：给 card/background/ancient/event/monster 五个模块的补丁体包 Safe。

只做机械包裹：把 `void Prefix/Postfix` 的方法体塞进 `Safe.Run(key, () => { … })`，
`bool Prefix` 塞进 `return Safe.Get(key, () => { … }, true);`。补丁逻辑一行不改。

用法：
    python tools/saferun_wrap.py --dry-run     # 只打印决策，不改文件
    python tools/saferun_wrap.py --apply       # 真正写回

决策表由 patches.csv（模块列）驱动，文件清单也在任务书里写死。
"""

from __future__ import annotations

import argparse
import csv
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]      # .../STS2-SkinChanger_修改件
OUT = Path(__file__).resolve().parents[1]       # .../docs/refactor
MODULES = ("card", "background", "ancient", "event", "monster")
USING = "using STS2SkinChanger.Core.Safety;"

# 任务书里写死的「不碰的文件」（第 1 批）
FORBIDDEN_1 = (
    "Core/SkinService", "Core/MultiplayerSkinSync.cs", "Ui/CharacterAppearanceRuntime.cs",
    "Core/ManagedSkinModLoader.cs", "Catalog/SkinCatalog.cs", "Ui/MerchantRuntimeAppearance.cs",
    "Core/Safety/", "Entry.cs",
)
# 第 2 批多了 Core/Features（Claude 新建的模块骨架）
FORBIDDEN_2 = FORBIDDEN_1 + ("Core/Features/",)

BATCHES = {
    1: {
        "modules": {"card", "background", "ancient", "event", "monster"},
        "forbidden": FORBIDDEN_1,
        "key": lambda m: f"patch:{m}",
        "out": "saferun-batch1-plan.json",
    },
    2: {
        "modules": {"character", "core", "osty", "workshop"},
        "module_prefixes": ("compat:",),
        "forbidden": FORBIDDEN_2,
        "key": lambda m: "patch:compat" if m.startswith("compat:") else f"patch:{m}",
        "out": "saferun-batch2-plan.json",
    },
}

# 启动期补丁：一旦熔断整局都缺初始化，任务书要求一律跳过（第 2 批）
STARTUP_TARGETS = {
    ("OneTimeInitialization", "*"),
    ("ModelDb", "Init"),
    ("ModManager", "TryLoadMod"),
    ("ModManager", "GetModdedLocTables"),
    ("ModManifest", "ReadFromStream"),
    ("AssemblyLoadContext", "LoadFromAssemblyPath"),
    ("DevConsole", ".ctor"),
    ("NGame", "_Ready"),
}
STARTUP_CLASS_WORDS = re.compile(r"(?i)essential_?initialization|one_?time_?init|bootstrap")

NON_PATCH_METHODS = {"Transpiler", "Finalizer", "Prepare", "TargetMethod", "TargetMethods"}
CLOSING_WORDS = re.compile(
    r"(?i)restore|cleanup|exit_?tree|dispose|unpatch|revert|original|reset|detach|shutdown|teardown")

# GPT 复核（review-saferun-batch1-gpt.md）认定的 8 处收尾/还原/解绑：名字和函数体都不带收尾关键词，
# 但语义是"退出/关闭/复用/回合切换时恢复原状"，功能熔断后仍必须执行，所以显式跳过。
SKIP_OVERRIDES = {
    ("Ui/AncientCompendium.cs", "AncientRuntimeProviderScopeExitPatch", "Postfix"): "收尾：先古场景 _ExitTree 后恢复行为范围",
    ("Ui/ContextualSkinControls.cs", "BestiaryRuntimeProviderScopePatch", "Postfix"): "收尾：图鉴关闭后恢复/清空范围",
    ("Ui/CardSkinControls.cs", "CardLayoutModelRebindPatch", "Prefix"): "收尾：换模型前释放旧呈现",
    ("Ui/CardSkinControls.cs", "CardLayoutPoolReleasePatch", "Prefix"): "收尾：对象池回收时释放呈现",
    ("Ui/StatefulCardArtView.cs", "StatefulCardTreeExitPatch", "Prefix"): "收尾：_ExitTree 释放 lease/节点",
    ("Ui/StatefulCardCosmeticPatches.cs", "StatefulCharacterSceneExitPatch", "Prefix"): "收尾：退出选人时复位手指位置",
    ("Ui/CardSurfaceView.cs", "TinyCardAnonymousSurfacePatch", "Prefix"): "收尾：清除 tiny icon 的旧卡牌关联",
    ("Ui/StatefulCardCosmeticPatches.cs", "StatefulSideTurnCosmeticPatch", "Postfix"): "收尾：侧回合开始清旧效果",
}

SIG_RE = re.compile(
    r"^(?P<ind>[ \t]*)"
    r"(?P<mods>(?:(?:public|private|internal|protected|static|async|override|virtual|extern|unsafe|sealed|new)\s+)+)"
    r"(?P<ret>[A-Za-z_][\w<>\[\]\.\?,]*)\s+"
    r"(?P<name>[A-Za-z_]\w*)\s*\(")


def read_text(path: Path) -> str:
    raw = path.read_bytes()
    for enc in ("utf-8-sig", "utf-8"):
        try:
            return raw.decode(enc)
        except UnicodeDecodeError:
            continue
    return raw.decode("gb18030", "replace")


def load_plan(batch: int) -> dict[str, dict[str, dict]]:
    """读 patches.csv，返回 {文件: {补丁类: {module, targets}}}，按批次范围过滤。"""
    cfg = BATCHES[batch]
    modules = cfg["modules"]
    prefixes = cfg.get("module_prefixes", ())
    plan: dict[str, dict[str, dict]] = {}
    with (OUT / "patches.csv").open(encoding="utf-8-sig") as f:
        for row in csv.DictReader(f):
            module = row["建议模块"]
            if row["方式"] != "attribute":
                continue
            if module not in modules and not any(module.startswith(p) for p in prefixes):
                continue
            cls = plan.setdefault(row["文件"], {}).setdefault(
                row["补丁类"], {"module": module, "targets": set()})
            if row["目标类型"] and not row["目标类型"].startswith("("):
                cls["targets"].add((row["目标类型"], row["目标方法"]))
    return plan


def load_modules() -> dict[tuple[str, str], str]:
    """兼容第 1 批的旧接口（verify_saferun.py 用）。"""
    mapping: dict[tuple[str, str], str] = {}
    for rel, classes in load_plan(1).items():
        for cls, meta in classes.items():
            mapping[(rel, cls)] = meta["module"]
    return mapping


def strip_comment(line: str) -> str:
    out, in_str, i = [], False, 0
    while i < len(line):
        c = line[i]
        if in_str:
            if c == "\\":
                out.append(line[i:i + 2])
                i += 2
                continue
            if c == '"':
                in_str = False
        elif c == '"':
            in_str = True
        elif c == "/" and line[i:i + 2] == "//":
            break
        out.append(c)
        i += 1
    return "".join(out)


def find_class_span(lines: list[str], name: str) -> tuple[int, int] | None:
    for i, ln in enumerate(lines):
        if re.search(rf"\bclass\s+{re.escape(name)}\b", strip_comment(ln)):
            depth, started = 0, False
            for j in range(i, len(lines)):
                depth += strip_comment(lines[j]).count("{") - strip_comment(lines[j]).count("}")
                if "{" in strip_comment(lines[j]):
                    started = True
                if started and depth <= 0:
                    return i, j
            return i, len(lines) - 1
    return None


def paren_close(text: str, start: int) -> int:
    depth = 0
    for k in range(start, len(text)):
        if text[k] == "(":
            depth += 1
        elif text[k] == ")":
            depth -= 1
            if depth == 0:
                return k
    return -1


def block_close(lines: list[str], start_line: int, brace_col: int) -> int:
    depth = 0
    for j in range(start_line, len(lines)):
        seg = lines[j][brace_col:] if j == start_line else lines[j]
        depth += seg.count("{") - seg.count("}")
        if depth <= 0:
            return j
    return len(lines) - 1


def decision_for(cls: str, method: str, ret: str, params: str, body: str, expr_body: bool) -> str | None:
    """返回 None 表示可包，否则返回跳过原因。"""
    if method in NON_PATCH_METHODS:
        return "Transpiler/Finalizer/Prepare/TargetMethod"
    if re.search(r"\b(ref|out)\s", params):
        return "ref/out 参数"
    if CLOSING_WORDS.search(cls) or CLOSING_WORDS.search(method):
        return "收尾/还原"
    if not expr_body:
        first = next((strip_comment(x).strip() for x in body.splitlines() if strip_comment(x).strip()), "")
        if first.startswith("try"):
            return "已有 try"
    if ret != "void" and not (ret == "bool" and method == "Prefix"):
        # 非 void 且不是 bool Prefix：包进 lambda 会丢返回值，整类跳过
        return "有返回值"
    return None


def scan_to_semicolon(lines: list[str], line_idx: int, col: int, limit: int) -> tuple[int, int]:
    """从 (line_idx, col) 起按 () {} 配平扫描，返回语句结束 ';' 的位置。"""
    depth_p = depth_b = 0
    in_str = False
    for i in range(line_idx, min(limit + 1, len(lines))):
        line = lines[i]
        j = col if i == line_idx else 0
        while j < len(line):
            c = line[j]
            if in_str:
                if c == "\\":
                    j += 2
                    continue
                if c == '"':
                    in_str = False
            elif c == '"':
                in_str = True
            elif c == "(":
                depth_p += 1
            elif c == ")":
                depth_p -= 1
            elif c == "{":
                depth_b += 1
            elif c == "}":
                depth_b -= 1
            elif c == ";" and depth_p == 0 and depth_b == 0:
                return i, j
            j += 1
    return limit, -1


def flatten(lines: list[str], line_idx: int, col: int, end_line: int, end_col: int) -> str:
    """把多行表达式拼成一段文本（保留内部换行，便于原样比对）。"""
    if end_line == line_idx:
        return lines[line_idx][col:end_col if end_col >= 0 else None]
    parts = [lines[line_idx][col:]]
    parts += lines[line_idx + 1:end_line]
    tail = lines[end_line] if end_line < len(lines) else ""
    parts.append(tail[:end_col] if end_col >= 0 else tail)
    return "\n".join(parts)


def collect_methods(lines: list[str], cstart: int, cend: int) -> list[dict]:
    """在类体里找补丁方法（带 [HarmonyPrefix]/[HarmonyPostfix] 或就叫 Prefix/Postfix）。"""
    out: list[dict] = []
    i = cstart
    pending_attr: list[str] = []
    while i <= cend:
        code = strip_comment(lines[i]).strip()
        if code.startswith("["):
            pending_attr.append(code)
            i += 1
            continue
        m = SIG_RE.match(strip_comment(lines[i]))
        if not m:
            if code:
                pending_attr = []
            i += 1
            continue
        name = m.group("name")
        is_patch = any("HarmonyPrefix" in a or "HarmonyPostfix" in a or "HarmonyTranspiler" in a
                       or "HarmonyFinalizer" in a for a in pending_attr) \
            or name in ("Prefix", "Postfix", "Transpiler", "Finalizer", "Prepare",
                        "TargetMethod", "TargetMethods")
        if not is_patch:
            pending_attr = []
            i += 1
            continue
        # 拼出签名（参数可能跨行）
        text = strip_comment(lines[i])
        p_start = text.index("(", m.end("name"))
        buf = text
        j = i
        while paren_close(buf, p_start) < 0 and j + 1 <= cend:
            j += 1
            buf += " " + strip_comment(lines[j]).strip()
        p_end = paren_close(buf, p_start)
        params = buf[p_start + 1:p_end]
        rest = buf[p_end + 1:]
        after = rest.strip()
        expr_body = after.startswith("=>")
        # 定位函数体
        if expr_body:
            # 找到 "=>" 的真实列（签名可能跨行）；再从那里按括号/大括号配平扫到语句结束的 ';'
            arrow_line, arrow_col = -1, -1
            for k in range(i, j + 1):
                pos = strip_comment(lines[k]).find("=>")
                if pos >= 0:
                    arrow_line, arrow_col = k, pos
                    break
            if arrow_line < 0:
                pending_attr = []
                i += 1
                continue
            end_line, end_col = scan_to_semicolon(lines, arrow_line, arrow_col + 2, cend)
            expr_text = flatten(lines, arrow_line, arrow_col + 2, end_line, end_col).strip()
            expr_col = arrow_col if arrow_line == i else -1
            out.append({"name": name, "ret": m.group("ret"), "params": params,
                        "indent": m.group("ind"), "sig_line": i, "end_line": end_line,
                        "expr": True, "expr_text": expr_text,
                        "expr_col": expr_col, "expr_line": arrow_line,
                        "body": "", "body_first": -1, "body_last": -1})
            pending_attr = []
            i = end_line + 1
            continue
        # 块体
        brace_line, brace_col = None, -1
        k = j
        while k <= cend:
            seg = strip_comment(lines[k])
            idx = seg.find("{")
            if idx >= 0:
                brace_line, brace_col = k, idx
                break
            k += 1
        if brace_line is None:
            pending_attr = []
            i += 1
            continue
        end = block_close(lines, brace_line, brace_col)
        body = "\n".join(lines[brace_line + 1:end])
        out.append({"name": name, "ret": m.group("ret"), "params": params,
                    "indent": m.group("ind"), "sig_line": i, "end_line": end,
                    "expr": False, "expr_text": "", "body": body,
                    "body_first": brace_line + 1, "body_last": end - 1})
        pending_attr = []
        i = end + 1
    return out


def reindent(body_lines: list[str], target_indent: int) -> list[str]:
    non_empty = [ln for ln in body_lines if ln.strip()]
    cur = min((len(ln) - len(ln.lstrip(" ")) for ln in non_empty), default=0)
    out = []
    for ln in body_lines:
        if not ln.strip():
            out.append("")
        else:
            stripped = ln[cur:] if len(ln) >= cur else ln.lstrip(" ")
            out.append(" " * target_indent + stripped)
    return out


def wrap_block(lines: list[str], info: dict, key: str) -> tuple[list[str], int]:
    """返回新行与改动条数（把 body_first..body_last 换成包裹后的块）。"""
    ind = info["indent"]
    body = lines[info["body_first"]:info["body_last"] + 1]
    inner = reindent(body, len(ind) + 8)
    if info["ret"] == "bool" and info["name"] == "Prefix":
        head = [f'{ind}    return Safe.Get("{key}", () =>', f"{ind}    {{"]
        tail = [f"{ind}    }}, true);"]
    else:
        head = [f'{ind}    Safe.Run("{key}", () =>', f"{ind}    {{"]
        tail = [f"{ind}    }});"]
    return head + inner + tail, 1


def wrap_expr(lines: list[str], info: dict, key: str) -> tuple[list[str], int] | None:
    """把 `=> expr;` 换成 `=> Safe.X(...)`；签名保留，跨行签名合并成一行。"""
    ind = info["indent"]
    expr = info["expr_text"]
    if info["ret"] == "bool" and info["name"] == "Prefix":
        call = f'Safe.Get("{key}", () => {expr}, true);'
    else:
        call = f'Safe.Run("{key}", () => {expr});'
    if info.get("expr_col", -1) >= 0:
        prefix = strip_comment(lines[info["sig_line"]])[:info["expr_col"]]
        return [prefix + "=> " + call], 1
    # 签名跨行：合并成一行，保留缩进
    sig = " ".join(strip_comment(x).strip() for x in lines[info["sig_line"]:info["expr_line"] + 1])
    head = sig.split("=>", 1)[0].strip()
    return [f"{ind}{head} => {call}"], 1


def add_using(text: str) -> tuple[str, bool]:
    if USING in text:
        return text, False
    lines = text.split("\n")
    last_using = -1
    for i, ln in enumerate(lines[:80]):
        if ln.startswith("using ") and ln.rstrip().endswith(";"):
            last_using = i
        if ln.startswith("namespace "):
            break
    if last_using >= 0:
        lines.insert(last_using + 1, USING)
    else:
        # 没有 using：放在 namespace 之前
        for i, ln in enumerate(lines[:80]):
            if ln.startswith("namespace "):
                lines.insert(i, USING)
                lines.insert(i + 1, "")
                break
        else:
            lines.insert(0, USING)
            lines.insert(1, "")
    return "\n".join(lines), True


def process_file(rel: str, classes: dict[str, dict], dry: bool, batch: int,
                 forbidden: bool = False) -> tuple[list[dict], list[dict]]:
    cfg = BATCHES[batch]
    path = ROOT / rel
    text = read_text(path)
    original = text
    lines = text.split("\n")
    wrapped: list[dict] = []
    skipped: list[dict] = []

    # 需要从后往前改
    edits: list[tuple[int, int, list[str], dict, str, bool]] = []
    for cls, meta in classes.items():
        span = find_class_span(lines, cls)
        if not span:
            skipped.append({"file": rel, "class": cls, "method": "-", "reason": "找不到类"})
            continue
        key = cfg["key"](meta["module"])
        # 启动期补丁：整个类跳过（挂了 OneTimeInitialization / 装载 / 清单读取等启动流程）
        startup = STARTUP_CLASS_WORDS.search(cls) or any(
            (t, m) in STARTUP_TARGETS or (t, "*") in STARTUP_TARGETS for t, m in meta["targets"])
        for info in collect_methods(lines, span[0], span[1]):
            if forbidden and startup:
                reason = "不碰的文件（启动期补丁）"
            elif forbidden:
                reason = "不碰的文件"
            elif startup:
                reason = "启动期"
            else:
                override = SKIP_OVERRIDES.get((rel.split("STS2SkinChanger/", 1)[-1], cls, info["name"]))
                reason = override or decision_for(cls, info["name"], info["ret"], info["params"],
                                                  info["body"], info["expr"])
            if reason:
                skipped.append({"file": rel, "class": cls, "method": info["name"], "reason": reason,
                                "line": info["sig_line"] + 1})
                continue
            if info["expr"]:
                res = wrap_expr(lines, info, key)
                if res is None:
                    skipped.append({"file": rel, "class": cls, "method": info["name"],
                                    "reason": "表达式体跨行", "line": info["sig_line"] + 1})
                    continue
                new, n = res
                start, end = info["sig_line"], info["end_line"]
            else:
                new, n = wrap_block(lines, info, key)
                start, end = info["body_first"], info["body_last"]
            edits.append((start, end, new, info, key, info["expr"]))
            wrapped.append({"file": rel, "class": cls, "method": info["name"],
                            "ret": info["ret"], "key": key,
                            "line": info["sig_line"] + 1})

    # 从后往前应用，行号不失效
    for start, end, new, info, key, is_expr in sorted(edits, key=lambda e: -e[0]):
        lines[start:end + 1] = new

    text = "\n".join(lines)
    if wrapped:
        text, added = add_using(text)
    if not dry and text != original:
        path.write_text(text, encoding="utf-8", newline="\n")
    return wrapped, skipped


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--dry-run", action="store_true")
    ap.add_argument("--apply", action="store_true")
    ap.add_argument("--batch", type=int, default=1, choices=sorted(BATCHES))
    args = ap.parse_args()
    dry = not args.apply
    batch = args.batch
    cfg = BATCHES[batch]

    plan = load_plan(batch)
    by_file: dict[str, dict[str, dict]] = {}
    forbidden_files: set[str] = set()
    for rel, classes in plan.items():
        if any(f in rel for f in cfg["forbidden"]):
            forbidden_files.add(rel)
        by_file[rel] = classes
    print(f"批次 {batch}：{len(by_file)} 个文件（其中不碰的文件 {len(forbidden_files)} 个）")

    all_wrapped: list[dict] = []
    all_skipped: list[dict] = []
    for rel in sorted(by_file):
        w, s = process_file(rel, by_file[rel], dry, batch, forbidden=rel in forbidden_files)
        all_wrapped += w
        all_skipped += s
        print(f"{rel}: 包 {len(w)} 处，跳过 {len(s)} 处")

    print(f"\n合计：包 {len(all_wrapped)} 处，跳过 {len(all_skipped)} 处，文件 {len(by_file)} 个")
    from collections import Counter
    print("跳过原因：")
    for reason, cnt in Counter(s["reason"] for s in all_skipped).most_common():
        print(f"  {reason}: {cnt}")
    print("\n跳过明细：")
    for s in all_skipped:
        print(f"  {s['file']}:{s.get('line', '?')} {s['class']}.{s['method']} —— {s['reason']}")

    if dry:
        print("\n（dry-run，未写文件。确认后加 --apply）")
    else:
        import json
        out_name = cfg["out"]
        (OUT / out_name).write_text(
            json.dumps({"batch": batch, "wrapped": all_wrapped, "skipped": all_skipped},
                       ensure_ascii=False, indent=1),
            encoding="utf-8", newline="\n")
        print(f"\n计划已写 docs/refactor/{out_name}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
