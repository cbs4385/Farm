#!/usr/bin/env python3
"""Shows a scene as the player sees it at chosen settings, from the compiled overlay.

    python3 render_levels.py bram_heart2 --horror 2 --dread 65 --lore 3 --hearts bram=4 [--flag x]

Only the atoms the variants use are understood (horror, var:dread, var:lore, hearts, flag, choice); anything else counts as false.
"""
import argparse, json, re, sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parent))
from common import MYTHOS, load_en, load_story

def ev_cond(c, ctx):
    c = c.strip()
    def atom(a):
        a = a.strip(); neg = a.startswith('!'); a = a.lstrip('!').strip()
        m = re.fullmatch(r'horror:(\d)', a)
        if m: v = ctx['horror'] >= int(m.group(1))
        elif (m := re.fullmatch(r'var:(\w+)(>=|<=|>|<|==)(\d+)', a)): 
            x = ctx.get(m.group(1), 0); n = int(m.group(3)); v = eval(f"{x}{m.group(2)}{n}")
        elif (m := re.fullmatch(r'hearts:(\w+)>=(\d+)', a)): v = ctx['hearts'].get(m.group(1), 0) >= int(m.group(2))
        elif (m := re.fullmatch(r'(?:flag|choice):([\w.]+)', a)): v = m.group(1) in ctx['flags']
        else: v = False
        return (not v) if neg else v
    # parentheses with ! are rare in variant text; support flat && and !( ... )
    c2 = re.sub(r'!\(([^()]*)\)', lambda m: str(not ev_cond(m.group(1), ctx)), c)
    c2 = re.sub(r'\(([^()]*)\)', lambda m: str(ev_cond(m.group(1), ctx)), c2)
    parts = [p for p in re.split(r'\s*&&\s*', c2)]
    return all((p == 'True') if p in ('True','False') else atom(p) for p in parts)

def resolve(t, ctx):
    toks = re.split(r'(\{if [^}]*\}|\{else\}|\{/if\})', t)
    def parse(i, stop):
        out = ''
        while i < len(toks):
            p = toks[i]
            if p in ('{else}', '{/if}'):
                if stop: return out, i
                i += 1; continue
            if p.startswith('{if '):
                a, j = parse(i + 1, True); b = ''
                if toks[j] == '{else}': b, j = parse(j + 1, True)
                out += a if ev_cond(p[4:-1], ctx) else b; i = j + 1
            else: out += p; i += 1
        return out, i
    return parse(0, False)[0]

ap = argparse.ArgumentParser(); ap.add_argument('scene'); ap.add_argument('--horror', type=int, default=2)
ap.add_argument('--dread', type=int, default=0); ap.add_argument('--lore', type=int, default=0)
ap.add_argument('--hearts', default=''); ap.add_argument('--flag', action='append', default=[])
a = ap.parse_args()
ctx = {'horror': a.horror, 'dread': a.dread, 'lore': a.lore, 'flags': set(a.flag),
       'hearts': {k: int(v) for k, v in (x.split('=') for x in a.hearts.split(',') if x)}}
en = load_en(); S = load_story()
M = json.loads((MYTHOS / 'mythos_scenes.json').read_text(encoding='utf-8'))
ov = {e['id']: e for e in M['events']}
dl = {d['id']: d for d in M['dialogues']}
ev = ov.get(a.scene + '__m') if a.horror >= 1 else None
src = ev or S['events'][a.scene]
print(f"== {a.scene} ({'overlay' if ev else 'cozy'}) horror={a.horror} dread={a.dread} lore={a.lore} hearts={ctx['hearts']}")
for s in src['steps']:
    if s['type'] == 'say':
        if s.get('condition') and not ev_cond(s['condition'], ctx): continue
        print(f"  {s['speaker'] or 'narration':9}: {resolve(en[s['text']], ctx)}")
    elif s['type'] == 'dialogue':
        d = dl.get(s['dialogue']) or S['dialogues'][s['dialogue']]
        n = next(n for n in d['nodes'] if n['id'] == d.get('start', d['nodes'][0]['id']))
        if n.get('text'): print(f"  {(n.get('speaker') or 'narration'):9}: {resolve(en[n['text']], ctx)}  [dialogue {s['dialogue']}]")
