import re, pathlib, collections, json
root = pathlib.Path(r"D:\STS2-SkinChanger")
files = [p for p in root.rglob("*") if p.is_file() and ".git" not in p.parts]
print("files", len(files), "dirs", len([p for p in root.rglob("*") if p.is_dir()]))
names = collections.Counter()
for p in files:
    names[p.name] += 1
for n,c in sorted(names.items()):
    if c>1: print("DUP", c, n)
# tokens
toks = collections.Counter()
def split(s):
    parts = re.split(r"[._\-\s]+", s)
    out=[]
    for part in parts:
        out += re.findall(r"[A-Z]+(?=[A-Z][a-z])|[A-Z]?[a-z]+|[A-Z]+|\d+", part)
    return out
for p in files:
    for t in split(p.stem): toks[t]+=1
print("unique tokens", len(toks))
print(" ".join(sorted(toks)))
