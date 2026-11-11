import re
import sys

def strings(path):
    text = open(path, encoding='utf-8-sig', errors='replace').read()
    return set(m.group(1) for m in re.finditer(r'"((?:[^"\\\r\n]|\\.){4,})"', text))

baseline = set()
for p in sys.argv[1].split(';'):
    baseline |= strings(p)
current = set()
for p in sys.argv[2].split(';'):
    current |= strings(p)

only = sorted(x for x in (baseline - current) if not x.startswith(('res://', 'uid://')))
print('baseline strings:', len(baseline), 'current:', len(current), 'only-in-baseline:', len(only))
for s in only[:120]:
    print('   ', s[:160])
