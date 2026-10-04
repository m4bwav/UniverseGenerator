"""Write API-Reference.md from the published DLL's reflection (v100.tsv), its XML docs and beta.1's reflection (b1.tsv)."""
import re, sys, xml.etree.ElementTree as ET
from collections import OrderedDict

base = sys.argv[1]
out = sys.argv[2]
x = ET.parse(base + '/nz/lib/net10.0/UniverseGenerator.xml').getroot()
docs = {m.get('name'): m for m in x.iter('member')}

def rows(p):
    return [l.split('\t') for l in open(p, encoding='utf-8').read().splitlines() if l]

v1 = rows(base + '/api/v100.tsv')
b1ids = {r[4] for r in rows(base + '/api/b1.tsv')}

def short(cref):
    name = cref.split(':', 1)[-1]
    name = re.sub(r'\(.*\)', '', name)
    parts = name.split('.')
    if parts[0] == 'UniverseGeneration':
        parts = parts[1:]
    if parts and parts[-1] == '#ctor':
        parts = parts[:-1]
    if len(parts) > 2:
        parts = parts[-2:]
    return '.'.join(parts)

def text(el):
    s = el.text or ''
    for c in el:
        if c.tag == 'see':
            s += '`' + (short(c.get('cref')) if c.get('cref') else (c.get('langword') or c.get('href') or '')) + '`'
        elif c.tag in ('paramref', 'typeparamref'):
            s += '`' + c.get('name') + '`'
        elif c.tag == 'c':
            s += '`' + text(c) + '`'
        else:
            s += text(c)
        s += c.tail or ''
    return s

def clean(s):
    s = re.sub(r'\s+', ' ', s).strip()
    s = re.sub(r'\s*\((?:plan [A-Z0-9]|plan ideas? |Stop \d)[^)]*\)', '', s)
    s = re.sub(r'^(plan [A-Z0-9]+: ?)', '', s)
    return s.replace('|', '\\|')

def summary(i):
    m = docs.get(i)
    if m is None:
        return ''
    e = m.find('summary')
    s = clean(text(e)) if e is not None else ''
    r = m.find('remarks')
    if r is not None:
        s += ' ' + clean(text(r))
    for ex in m.findall('exception'):
        s += f" Throws `{short(ex.get('cref'))}` {clean(text(ex))}"
    return s.strip()

def since(i):
    return '' if i in b1ids else '1.0.0'

types = OrderedDict()
for r in v1:
    types.setdefault(r[0], []).append(r)

def name(full):
    return full.split('.')[-1]

GROUPS = [
    ('Generating', 'Entry points: one call per level, addresses and links.',
     ['Universe', 'GalaxyCluster', 'Galaxy', 'StarSystem', 'Planet', 'StarName']),
    ('Options, presets and guarantees', 'What you can ask for, validated before anything is generated.',
     ['GeneratorOptions', 'Preset', 'Guarantee', 'GalaxyShape', 'StarMix', 'NameStyle', 'ClusterKind', 'Epoch', 'GeneratorWarning', 'WarningCode']),
    ('Hooks, custom fields and tables', 'Your own post-processing, your own fields and your own weight tables.',
     ['GeneratorHooks', 'CustomFields', 'GeneratorTables', 'WeightTable', 'WeightEntry']),
    ('Units and distances', 'Conversions on plain doubles.',
     ['Units', 'Distances', 'LengthUnit', 'MassUnit', 'TemperatureUnit', 'TimeUnit', 'MapLevel']),
]
grouped = {t for _, _, ts in GROUPS for t in ts}
records = [n for n in (name(t) for t in types) if n not in grouped and types['UniverseGeneration.' + n][0][1] != 'enum']
enums = [n for n in (name(t) for t in types) if n not in grouped and types['UniverseGeneration.' + n][0][1] == 'enum']
GROUPS.append(('Generated records', 'What generation returns. Every record is immutable; change a copy with `with`.', records))

o = []
o.append('Every public type of UniverseGenerator 1.0.0, read from the published package: the signatures by reflection over `lib/net10.0/UniverseGenerator.dll` from nuget.org, the descriptions from the XML documentation that ships beside it. Everything is in the namespace `UniverseGeneration`. "Since" names 1.0.0 for a member that 1.0.0-beta.1 did not have; a blank means it was there in 1.0.0-beta.1. The `netstandard2.0` build has the same public surface.')
o.append('')
o.append('Records are immutable and have a public parameterless constructor with `init` properties, so `with` works on every one of them; the constructors are not listed below. Equality, `ToString` and `Deconstruct` are the compiler\'s record members.')
o.append('')
o.append('Contents:')
o.append('')
for g, _, ts in GROUPS:
    anchor = re.sub(r'[^a-z0-9 -]', '', g.lower()).replace(' ', '-')
    o.append(f'- [{g}](#{anchor})')
o.append('- [Enums](#enums)')
o.append('')

for g, intro, ts in GROUPS:
    o.append(f'## {g}')
    o.append('')
    o.append(intro)
    o.append('')
    for n in ts:
        rs = types['UniverseGeneration.' + n]
        t = rs[0]
        o.append(f'### {n}')
        o.append('')
        line = f'{t[1]}. ' + summary(t[4])
        if since(t[4]):
            line += ' Since 1.0.0.'
        o.append(line.strip())
        o.append('')
        if t[1] == 'enum':
            o.append('| Value | Description | Since |')
            o.append('|---|---|---|')
            for r in rs[1:]:
                o.append(f'| `{r[3]}` | {summary(r[4])} | {since(r[4]) if not since(t[4]) else ""} |')
            o.append('')
            continue
        mem = [r for r in rs[1:] if not (r[2] == 'ctor' and r[3].endswith('()') and t[1] == 'record')]
        if not mem:
            continue
        o.append('| Member | Description | Since |')
        o.append('|---|---|---|')
        for r in mem:
            sig = r[3].replace('|', '\\|')
            o.append(f'| `{sig}` | {summary(r[4])} | {since(r[4]) if not since(t[4]) else ""} |')
        o.append('')

o.append('## Enums')
o.append('')
o.append('The enums the records use. Every value is listed with its description in the XML documentation; the names are what `ToJson` writes.')
o.append('')
o.append('| Enum | Values | Since |')
o.append('|---|---|---|')
for n in enums:
    rs = types['UniverseGeneration.' + n]
    vals = ', '.join(f'`{r[3]}`' for r in rs[1:])
    newvals = [r[3] for r in rs[1:] if since(r[4])] if not since(rs[0][4]) else []
    s = since(rs[0][4]) or (('1.0.0: ' + ', '.join(f'`{v}`' for v in newvals)) if newvals else '')
    o.append(f'| `{n}`: {summary(rs[0][4])} | {vals} | {s} |')
o.append('')
open(out, 'w', encoding='utf-8', newline='\n').write('\n'.join(o))
print(len(o), 'lines;', sum(1 for r in v1 if since(r[4])), 'members new in 1.0.0')
