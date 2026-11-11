import os
import re
import sys

def all_strings(root, pattern):
    out = set()
    for base, dirs, files in os.walk(root):
        for name in files:
            if not name.endswith('.cs'):
                continue
            if pattern and not re.search(pattern, name):
                continue
            text = open(os.path.join(base, name), encoding='utf-8-sig', errors='replace').read()
            for m in re.finditer(r'"((?:[^"\\\r\n]|\\.){4,})"', text):
                out.add(m.group(1))
    return out

base = all_strings(sys.argv[1], None)
cur = all_strings(sys.argv[2], None)
only = sorted(x for x in (base - cur)
              if not x.startswith(('res://', 'uid://'))
              and not re.search(r'[\u4e00-\u9fff]', x) is None)
print('baseline:', len(base), 'current:', len(cur), 'only-in-baseline:', len(only))
for s in only[:150]:
    print('   ', s[:150])
