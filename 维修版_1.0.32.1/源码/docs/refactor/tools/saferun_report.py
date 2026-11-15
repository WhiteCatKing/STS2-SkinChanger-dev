#!/usr/bin/env python3
"""把 saferun-batch1-plan.json 渲染成交付文档的表格部分（行号按当前文件重算）。"""

from __future__ import annotations

import json
import sys
from collections import Counter
from pathlib import Path

import saferun_wrap as W

OUT = W.OUT
PLAN = OUT / "saferun-batch1-plan.json"


def current_line(rel: str, cls: str, method: str) -> str:
    lines = W.read_text(W.ROOT / rel).split("\n")
    span = W.find_class_span(lines, cls)
    if not span:
        return "?"
    for m in W.collect_methods(lines, span[0], span[1]):
        if m["name"] == method:
            return str(m["sig_line"] + 1)
    return "?"


def main() -> int:
    import argparse
    ap = argparse.ArgumentParser()
    ap.add_argument("--batch", type=int, default=1, choices=sorted(W.BATCHES))
    args = ap.parse_args()
    batch = args.batch
    plan = json.loads((OUT / f"saferun-batch{batch}-plan.json").read_text(encoding="utf-8"))
    wrapped = plan["wrapped"]
    skipped = plan["skipped"]

    per_module = Counter(w["key"].replace("patch:", "") for w in wrapped)
    per_file = Counter(w["file"] for w in wrapped)

    md = ["## 改了什么\n",
          f"共 **{len(wrapped)}** 处，涉及 **{len(per_file)}** 个文件、{len({w['class'] for w in wrapped})} 个补丁类。",
          "包裹方式：`void Prefix/Postfix` → `Safe.Run(key, () => { 原方法体 })`；"
          "`bool Prefix` → `return Safe.Get(key, () => { 原方法体 }, true)`（出错放行原方法）。\n",
          "| 模块 key | 处数 |", "|---|---|"]
    for k, v in sorted(per_module.items()):
        md.append(f"| `patch:{k}` | {v} |")
    md.append("\n按文件：\n")
    md.append("| 文件 | 处数 |")
    md.append("|---|---|")
    for f, v in per_file.most_common():
        md.append(f"| `{f}` | {v} |")

    md.append("\n### 明细\n")
    md.append("| 位置 | 补丁类 | 方法 | 返回 | key |")
    md.append("|---|---|---|---|---|")
    for w in sorted(wrapped, key=lambda x: (x["file"], x["class"], x["method"])):
        line = current_line(w["file"], w["class"], w["method"])
        md.append(f"| `{w['file']}:{line}` | {w['class']} | {w['method']} | {w['ret']} | `{w['key']}` |")

    md.append(f"\n## 跳过清单（{len(skipped)} 处）\n")
    md.append("| 位置 | 补丁类 | 方法 | 原因 |")
    md.append("|---|---|---|---|")
    for s in sorted(skipped, key=lambda x: (x["file"], x.get("line", 0))):
        line = current_line(s["file"], s["class"], s["method"])
        md.append(f"| `{s['file']}:{line}` | {s['class']} | {s['method']} | {s['reason']} |")

    md.append("\n### 跳过原因统计\n")
    md.append("| 原因 | 处数 |")
    md.append("|---|---|")
    for r, c in Counter(s["reason"] for s in skipped).most_common():
        md.append(f"| {r} | {c} |")
    forbidden = sum(1 for s in skipped if s["reason"].startswith("不碰的文件"))
    md.append(f"\n> 跳过里有 {forbidden} 处落在任务书的「不碰的文件」清单内（含启动期补丁）。" if forbidden
              else "\n> 本批没有命中「不碰的文件」的补丁类。")

    (OUT / f"saferun-batch{batch}-tables.md").write_text("\n".join(md) + "\n", encoding="utf-8", newline="\n")
    print(f"wrapped={len(wrapped)} skipped={len(skipped)} → saferun-batch{batch}-tables.md")
    return 0


if __name__ == "__main__":
    sys.exit(main())
