import re
import sys

path = r'D:\steam\steamapps\workshop\appworkshop_2868840.acf'
text = open(path, encoding='utf-8', errors='replace').read()

def section(name):
    key = '"%s"' % name
    i = text.find(key)
    if i < 0:
        return ''
    start = text.find('{', i)
    depth = 0
    out = []
    for j in range(start, len(text)):
        c = text[j]
        if c == '{':
            depth += 1
        elif c == '}':
            depth -= 1
            if depth == 0:
                break
        out.append(c)
    return ''.join(out)

for sec in ['WorkshopItemsInstalled', 'WorkshopItemDetails', 'SubscribedItems', 'WorkshopItemsDisabledLocally']:
    body = section(sec)
    print(sec, 'present' if body else 'MISSING')
    if not body:
        continue
    for iid in ['3826011578', '3789216478', '3747526116', '3783041069']:
        key = '"%s"' % iid
        print('   ', iid, 'listed' if key in body else 'not listed')
