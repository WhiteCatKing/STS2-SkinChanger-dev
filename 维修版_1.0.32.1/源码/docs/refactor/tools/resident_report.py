"""渲染 docs/refactor/resident-patches.md（行号取自当前源码里的标签行）。"""

from __future__ import annotations

import re
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
import resident_tags as R  # noqa: E402

OUT = R.ROOT / "docs" / "refactor" / "resident-patches.md"


def enclosing_class(lines: list[str], idx: int) -> str:
    for k in range(idx, -1, -1):
        m = re.search(r"\bclass\s+([A-Za-z_]\w*)", lines[k])
        if m:
            return m.group(1)
    return "?"


def main() -> int:
    by_file: dict[str, list[dict]] = {}
    for item in R.ITEMS:
        file, kind, name, typ, who, what, why, risk = item[:8]
        hint = item[8] if len(item) > 8 else None
        by_file.setdefault(file, []).append(
            dict(kind=kind, name=name, type=typ, who=who, what=what, why=why, risk=risk, hint=hint))

    rows = []
    for file, entries in by_file.items():
        lines = R.read(R.SRC / file)
        for e in entries:
            decl = (R.find_class(lines, e["name"]) if e["kind"] == "class"
                    else R.find_method(lines, e["name"], e.get("hint")))
            pos = R.insert_point(lines, decl, "")
            if pos < len(lines) and lines[pos].lstrip().startswith("// 常驻"):
                pass
            elif pos - 1 >= 0 and lines[pos - 1].lstrip().startswith("// 常驻"):
                pos -= 1
            else:
                print(f"[FAIL] {file} {e['name']}: 标签不在预期位置（{lines[pos][:40]!r}）")
                return 1
            symbol = e["name"] if e["kind"] == "class" else f"{enclosing_class(lines, decl)}.{e['name']}"
            e["key"] = R.body_key(lines, decl)
            rows.append(dict(file=file, line=pos + 1, symbol=symbol, **e))

    rows.sort(key=lambda r: ({"入口": 0, "共享": 1, "兼容": 2}[r["type"]], r["file"], r["line"]))

    out = ["# 常驻补丁清单（DSH，2026-11-15）", "",
           "1. 这些补丁不进 `FeaturePatch`，**不要改成按需安装**：它们服务角色、随从、怪物和多个界面共用的生命周期。",
           "2. 改方法体之前，先看「碰了会坏什么」那一列；空白表示勘察第 6 节没有对应到具体回归点。",
           "3. 新加的常驻补丁，在这里登记一行，并在类（或安装方法）上方贴同样格式的标签："
           "`// 常驻·类型：谁在用 —— 干什么。详见 docs/refactor/resident-patches.md`。",
           "",
           f"共 **{len(rows)}** 处（入口 {sum(r['type'] == '入口' for r in rows)}、"
           f"共享 {sum(r['type'] == '共享' for r in rows)}、兼容 {sum(r['type'] == '兼容' for r in rows)}）。"
           "行号是标签所在行；源码位置相对 `STS2SkinChanger/`。",
           "",
           "| 类 | 文件:行号 | 类型 | 谁在用 | 干什么 | 熔断键 | 为什么必须常驻 | 碰了会坏什么 |",
           "| --- | --- | --- | --- | --- | --- | --- | --- |"]
    for r in rows:
        cells = [f"`{r['symbol']}`", f"`{r['file']}:{r['line']}`", r["type"], r["who"], r["what"],
                 f"`{r['key']}`" if r["key"].startswith("patch:") else "—",
                 r["why"], r["risk"] or ""]
        out.append("| " + " | ".join(cells) + " |")

    out += ["", "## 说明", "",
            "- 「熔断键」— 表示这个补丁体没有包 `Safe.Run`，异常会按原来的方式处理（多数是 `ref __result` 之类的形态，包不了 lambda）。",
            "- `MonsterVisualScalePatch` 的熔断键是 `patch:character` 属于历史原因（它服务怪物和随从，键是早期定的），改动时别被键名误导。",
            "- 手动安装的桥（没有 `[HarmonyPatch]` 特性）标签贴在负责安装的方法上方，清单里写成 `类.方法`。",
            "- `CharacterAppearanceOrbPositionPatch` 勘察第 2.2 节没有直接标常驻，按其「先随常驻变换服务保留，另行拆分」记为共享。",
            "- `ManagedSkinModLoader.EnsureDirectCharacterSkinPathRouter` 原勘察只写「由主干加载器持有」；GPT 复核（#74）确认它按角色选择分发 Spine 外观路径，已补为共享常驻并入表。",
            "- 勘察第 91 行列出的全局 core 补丁（Mod 加载、本地化、模组列表等）按任务书不在本批范围。",
            ""]
    OUT.write_text("\n".join(out), encoding="utf-8", newline="\n")
    print(f"写出 {OUT}（{len(rows)} 行）")
    return 0


if __name__ == "__main__":
    sys.exit(main())
