import re
import sys

def read(path):
    return open(path, encoding='utf-8-sig', errors='replace').read()

def members(text):
    names = set()
    # method-ish declarations
    for m in re.finditer(r'\b(?:private|public|internal|protected)\s+(?:static\s+)?(?:readonly\s+)?(?:async\s+)?(?:partial\s+)?[\w<>\[\],\.\?\s]+\s+(\w+)\s*\(', text):
        names.add(('method', m.group(1)))
    # fields / consts / properties
    for m in re.finditer(r'\b(?:private|public|internal|protected)\s+(?:static\s+)?(?:readonly\s+)?(?:const\s+)?[\w<>\[\],\.\?\s]+\s+(\w+)\s*(?:=|;)', text):
        names.add(('field', m.group(1)))
    return names

a = read(sys.argv[1])  # baseline (.82 decompiled)
b = read(sys.argv[2])  # current restored
ma, mb = members(a), members(b)
only_a = sorted(ma - mb)
only_b = sorted(mb - ma)
print('baseline members:', len(ma), 'current:', len(mb))
print()
print('--- in .82 but NOT in current ---')
for kind, name in only_a:
    print(f'  {kind} {name}')
print()
print('--- in current but NOT in .82 ---')
for kind, name in only_b:
    print(f'  {kind} {name}')
