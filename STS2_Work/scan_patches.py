import re, pathlib
root = pathlib.Path(r"D:\STS2-SkinChanger\STS2SkinChanger")
issues = []
for f in root.rglob("*.cs"):
    t = f.read_text(encoding="utf-8-sig", errors="replace")
    for m in re.finditer(r"(?:\[[^\]]*\]\s*)*\b(?:internal|public|private|static|sealed|partial|\s)*class\s+(\w+)", t):
        name = m.group(1)
        # find class body extent roughly: next class attr or EOF
        start = m.start()
        nxt = re.search(r"\n(?:\[[^\]]*\]\s*\n)*(?:internal|public|private|static|sealed|partial|\s)*class\s+\w+", t[m.end():])
        end = m.end() + (nxt.start() if nxt else len(t) - m.end())
        body = t[m.end():end]
        if re.search(r"\[Harmony(Prefix|Postfix|Transpiler|Finalizer|ILManipulator)\]", body):
            pre = t[max(0, start-300):m.end()]
            if "[HarmonyPatch" not in pre:
                issues.append(f"{f.relative_to(root)}: class {name}")
print(len(issues))
print("\n".join(issues))
