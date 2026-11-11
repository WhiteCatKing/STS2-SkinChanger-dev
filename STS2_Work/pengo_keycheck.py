import os
import re
import json
import glob

src = r'C:\Users\Administrator\Desktop\工作2\STS2_Work\pengo_decomp'
loc = r'C:\Users\Administrator\Desktop\工作2\STS2_Work\pengo_loc\PengoTarot\localization'

bal_keys = set()
pairs = set()
table_refs = set()

for root, dirs, files in os.walk(src):
    for f in files:
        if not f.endswith('.cs'):
            continue
        text = open(os.path.join(root, f), encoding='utf-8-sig', errors='replace').read()
        for m in re.finditer(r'"(BAL_[A-Za-z0-9_]+)"', text):
            bal_keys.add(m.group(1))
        for m in re.finditer(r'LocString\("([a-z_]+)",\s*"([A-Za-z0-9_.]+)"', text):
            pairs.add((m.group(1), m.group(2)))
        for m in re.finditer(r'"([a-z_]+)",\s*"(BAL_[A-Za-z0-9_]+)"', text):
            table_refs.add(m.group(1))

tables = {}
for p in glob.glob(os.path.join(loc, '*', '*.json')):
    lang = os.path.basename(os.path.dirname(p))
    name = os.path.splitext(os.path.basename(p))[0]
    tables[(lang, name)] = set(json.load(open(p, encoding='utf-8-sig')))

langs = sorted({l for l, _ in tables})
print('languages:', langs)
print('BAL_ literals in code:', len(bal_keys))
print('explicit LocString pairs:', len(pairs))

print()
print('--- explicit pairs missing from their table ---')
for table, key in sorted(pairs):
    for lang in langs:
        keys = tables.get((lang, table))
        if keys is None:
            continue
        if key not in keys:
            print(f'  {lang}/{table}: {key}')

print()
print('--- BAL_ keys missing from gameplay_ui ---')
for lang in langs:
    keys = tables.get((lang, 'gameplay_ui'), set())
    missing = sorted(k for k in bal_keys if k not in keys)
    print(f'  {lang}: {len(missing)} missing')
    for k in missing[:10]:
        print('     ', k)

print()
print('--- per-language table sizes vs eng ---')
eng = {name for (lang, name) in tables if lang == 'eng'}
for lang in langs:
    sizes = {name: len(tables[(lang, name)]) for name in sorted(eng) if (lang, name) in tables}
    missing_tables = sorted(eng - {name for (l, name) in tables if l == lang})
    print(f'  {lang}: {sizes} missing_tables={missing_tables}')
