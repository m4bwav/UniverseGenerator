"""Write Feature-Matrix.md from the repository's kb/features/feature-matrix.md (generated there by build_matrix.py)."""
import re, sys

src, out = sys.argv[1], sys.argv[2]
lines = open(src, encoding='utf-8').read().split('\n')

STATUS = {'have': 'in 1.0.0', '1.0': 'in 1.0.0', '1.x': 'later (1.x)', 'rejected': 'rejected'}

def clean_why(s):
    s = re.sub(r'^built for 1\.0\.0: ', '', s)
    s = re.sub(r'; planned as: .*$', '', s)
    s = re.sub(r'\b(D|S|N|U|P|A)\d+\b(, )?', '', s)
    s = re.sub(r'Mark 2026-\d\d-\d\d( \([^)]*\))?:? ?', '', s)
    s = re.sub(r'\(\s*\)', '', s)
    s = re.sub(r'^[:;, ]+', '', s)
    s = re.sub(r' ?\((?:idea|ideas) [^)]*\)', '', s)
    s = re.sub(r' ?\(ai-docs/[^)]*\)', '', s)
    s = re.sub(r'; [^;]*(SpaceDeckBuilder2|the game.s home system)[^;]*', '', s)
    s = re.sub(r'; the lab page draws in 1\.0, the library export can follow', '', s)
    s = s.replace(' , ', ', ')
    s = re.sub(r'\s{2,}', ' ', s).strip(' ;,')
    return s

sections = []
cur = None
for l in lines:
    if l.startswith('## ') and l != '## Sources':
        cur = (l[3:], [])
        sections.append(cur)
    elif l.startswith('## Sources'):
        cur = None
    elif cur and l.startswith('| `'):
        cells = [c.strip() for c in l.strip().strip('|').split('|')]
        key, meaning, who, count, status, seeds, package, size, why = cells[:9]
        cur[1].append((key, meaning, int(count), status, package, why))

totals = {}
for _, rows in sections:
    for r in rows:
        k = STATUS.get(r[3], r[3])
        totals[k] = totals.get(k, 0) + 1
nsrc = re.search(r'from (\d+) sources', '\n'.join(lines)).group(1)

o = [
    f'Every feature found in {nsrc} other generators (games, libraries, engine assets, web tools, tabletop and writing tools), whether UniverseGenerator has it, and how many of the surveyed generators do. It is the short form of the knowledge base\'s [feature matrix](https://github.com/m4bwav/UniverseGenerator/blob/master/kb/features/feature-matrix.md), which also names every generator per row, the package each feature lives in and its size cost; that file is generated from the survey by `kb/features/build_matrix.py` and never edited by hand.',
    '',
    'Status: **in 1.0.0** is in the package today; **later (1.x)** is planned for a 1.x release and is additive, so no seed changes when it arrives; **rejected** says why. "Others" is how many surveyed generators have the feature; 0 means only UniverseGenerator has or plans it.',
    '',
    'Totals: ' + ', '.join(f'{v} {k}' for k, v in totals.items()) + '.',
    '',
]
for title, rows in sections:
    o.append(f'## {title}')
    o.append('')
    o.append('| Feature | What it means | Others | Status | Notes |')
    o.append('|---|---|---|---|---|')
    for key, meaning, count, status, package, why in rows:
        st = STATUS.get(status, status)
        note = ''
        if status in ('rejected', '1.x'):
            note = clean_why(why)
            note = note.replace("the lab page's job", 'for the planned web page').replace('the lab page', 'the planned web page')
            if package == 'lab page':
                note = ('the planned web page; ' + note).strip('; ')
            elif package.startswith('UniverseGenerator.'):
                note = (f'add-on package `{package.split(" (")[0]}`; ' + note).strip('; ')
            elif package not in ('core', '-', 'docs', 'tests'):
                note = (f'{package}; ' + note).strip('; ')
        o.append(f'| {key} | {meaning} | {count} | {st} | {note} |')
    o.append('')
o.append('See also: [Recipes](Recipes) for the 1.0.0 features in use, and [Versions and upgrading](Versions-and-Upgrading) for what arrived in which release.')
o.append('')
open(out, 'w', encoding='utf-8', newline='\n').write('\n'.join(o))
print(totals)
