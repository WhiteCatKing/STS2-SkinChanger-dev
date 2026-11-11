import difflib, pathlib, sys

old = pathlib.Path(r"D:\STS2_Work\old_decomp")
new = pathlib.Path(r"D:\STS2_Work\new_decomp")
out = pathlib.Path(r"D:\STS2_Work\diffs")
out.mkdir(exist_ok=True)

for f in sorted(new.rglob("*.cs")):
    if "RegexGenerator" in str(f):
        continue
    rel = f.relative_to(new)
    a = old / rel
    if not a.exists():
        continue
    al = a.read_text(encoding="utf-8-sig", errors="replace").splitlines(keepends=True)
    bl = f.read_text(encoding="utf-8-sig", errors="replace").splitlines(keepends=True)
    if al == bl:
        continue
    d = list(difflib.unified_diff(al, bl, fromfile="OLD/" + str(rel), tofile="NEW/" + str(rel), n=3))
    if len(d) > 4:
        target = out / (str(rel).replace("\\", "_") + ".diff")
        target.write_text("".join(d), encoding="utf-8")
        print(f"{len(d):6d} {rel}")
