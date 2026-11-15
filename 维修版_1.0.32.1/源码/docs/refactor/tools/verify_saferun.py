#!/usr/bin/env python3
"""④ 第 1 批自检：确认包裹只加了壳，补丁体一行没变。

做法：对每个被包裹的方法，取 `git show HEAD:<文件>` 里的原方法体，
和当前文件里 lambda 内部的内容做「去掉所有空白后逐字比较」。
另外核对 `bool Prefix` 的 fallback 全是 true。

用法：python tools/verify_saferun.py
"""

from __future__ import annotations

import re
import subprocess
import sys
from pathlib import Path

import saferun_wrap as W

REPO = W.ROOT.parent
PREFIX_PKG = W.ROOT.name + "/"


def git_show(rel: str) -> str:
    out = subprocess.run(
        ["git", "-C", str(REPO), "show", f"HEAD:{PREFIX_PKG}{rel}"],
        capture_output=True, text=True, encoding="utf-8", errors="replace")
    return out.stdout


def squash(text: str) -> str:
    return re.sub(r"\s+", "", text)


def inner_block(body_lines: list[str]) -> str | None:
    """从包裹后的方法体里取出 lambda 内层（去掉 Safe.Run/Get 那几行）。"""
    for i, ln in enumerate(body_lines):
        if re.search(r'Safe\.(Run|Get)\("patch:', ln):
            depth = 0
            started = False
            out: list[str] = []
            for j in range(i, len(body_lines)):
                cur = body_lines[j]
                if j > i:
                    out.append(cur)
                depth += cur.count("{") - cur.count("}")
                if "{" in cur:
                    started = True
                if started and depth <= 0:
                    # 去掉 lambda 自己的开 `{` 与收尾 `});` / `}, true);`，中间原样保留
                    inner = list(out)
                    if inner and inner[-1].strip() in ("});", "}, true);"):
                        inner = inner[:-1]
                    if inner and inner[0].strip() == "{":
                        inner = inner[1:]
                    return "\n".join(inner)
            return None
    return None


def main() -> int:
    import argparse
    ap = argparse.ArgumentParser()
    ap.add_argument("--batch", type=int, default=1, choices=sorted(W.BATCHES))
    args = ap.parse_args()
    cfg = W.BATCHES[args.batch]
    by_file: dict[str, dict[str, str]] = {}
    for rel, classes in W.load_plan(args.batch).items():
        if any(f in rel for f in cfg["forbidden"]):
            continue
        by_file[rel] = {cls: meta["module"] for cls, meta in classes.items()}

    checked = 0
    bad = 0
    fallbacks: list[tuple[str, str, str]] = []
    for rel in sorted(by_file):
        head_text = git_show(rel)
        cur_text = W.read_text(W.ROOT / rel)
        head_lines = head_text.split("\n")
        cur_lines = cur_text.split("\n")
        for cls in by_file[rel]:
            hs = W.find_class_span(head_lines, cls)
            cs = W.find_class_span(cur_lines, cls)
            if not hs or not cs:
                print(f"[?] {rel} {cls}: 类定位失败"); bad += 1
                continue
            hmap = {m["name"]: m for m in W.collect_methods(head_lines, hs[0], hs[1])}
            for cm in W.collect_methods(cur_lines, cs[0], cs[1]):
                cur_body_lines = (cur_lines[cm["sig_line"]:cm["end_line"] + 1]
                                  if cm["expr"] else cur_lines[cm["body_first"]:cm["body_last"] + 1])
                joined = "\n".join(cur_body_lines)
                if 'Safe.Run("patch:' not in joined and 'Safe.Get("patch:' not in joined:
                    continue
                orig = hmap.get(cm["name"])
                if not orig:
                    print(f"[?] {rel} {cls}.{cm['name']}: HEAD 版找不到同名方法"); bad += 1
                    continue
                if cm["expr"]:
                    # 表达式体：从 Safe.X("patch:…", () => <expr>) / (…, true) 里取 <expr>
                    m_get = re.search(r'Safe\.Get\("patch:[^"]+",\s*\(\)\s*=>\s*(.*),\s*true\s*\)\s*;',
                                      joined, re.S)
                    m_run = re.search(r'Safe\.Run\("patch:[^"]+",\s*\(\)\s*=>\s*(.*)\)\s*;', joined, re.S)
                    mm = m_get or m_run
                    if not mm:
                        print(f"[?] {rel} {cls}.{cm['name']}: 解析不出包裹表达式"); bad += 1
                        continue
                    same = squash(mm.group(1)) == squash(orig["expr_text"])
                    fallback = bool(m_get)
                else:
                    inner = inner_block(cur_body_lines)
                    if inner is None:
                        print(f"[?] {rel} {cls}.{cm['name']}: 取不到 lambda 内层"); bad += 1
                        continue
                    same = squash(inner) == squash(orig["body"])
                    mm = re.search(r'Safe\.Get\("patch:[^"]+",\s*\(\)\s*=>', joined)
                    fallback = bool(re.search(r",\s*true\s*\)\s*;", joined)) if mm else True
                checked += 1
                if not same:
                    print(f"[改动] {rel} {cls}.{cm['name']}: 内容不一致")
                    bad += 1
                if cm["ret"] == "bool" and cm["name"] == "Prefix":
                    fallbacks.append((rel, cls, "true" if fallback else "⚠ 非 true"))

    print(f"\n逐处比对：{checked} 处")
    print(f"内容不一致：{bad}")
    print(f"bool Prefix 数量：{len(fallbacks)}，fallback 全 true：{all(f[2] == 'true' for f in fallbacks)}")
    for f in fallbacks:
        if f[2] != "true":
            print(f"  ⚠ {f[0]} {f[1]} fallback={f[2]}")
    return 0 if bad == 0 else 1


if __name__ == "__main__":
    sys.exit(main())
