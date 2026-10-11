"""Small helper for exact, checked edits of villager lines (used for the consistency pass of 2026-10-11).

A line lives in up to two places: its FScript source (Assets/_Project/Narrative/*.fscript, either a one-line set entry `... => id ... :: text` or a node
`n0 speaker: text` inside a `dialogue id` block) and the compiled string table (Resources/Localization/en.json, key `dlg.<id>.<n>` or an overheard/event key).
The edits here change both, and refuse (print and skip) when the old text is not found, so that nothing is changed silently.

    python tools/narrative_edit.py edits.py        # edits.py defines EDITS = [("key or id", "old text fragment", "new text"), ...]

`key` is the en.json key. For a set entry the FScript line is found through its dialogue id (the key without the `dlg.` prefix and the trailing `.0`).
`old` must be a fragment (or the whole text) that currently appears in the en.json value; the whole value is replaced by `new` when `whole=True` is
given as the fourth item, otherwise only the fragment is replaced.
"""
import glob
import json
import os
import re
import sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..'))
NARR = os.path.join(ROOT, 'Assets', '_Project', 'Narrative')
EN = os.path.join(ROOT, 'Assets', '_Project', 'Resources', 'Localization', 'en.json')


def read(p):
    with open(p, encoding='utf-8', newline='') as f:
        return f.read()


def write(p, s):
    with open(p, 'w', encoding='utf-8', newline='') as f:
        f.write(s)


def jstr(s):
    return json.dumps(s, ensure_ascii=False)[1:-1]


def main(path):
    ns = {}
    exec(open(path, encoding='utf-8').read(), ns)
    edits = ns['EDITS']
    en = read(EN)
    data = json.loads(en)
    dry = '--dry' in sys.argv
    # an edit may name the line by a fragment of its present text instead of its key: ("~fragment", "old part or whole text", "new", whole)
    resolved = []
    for item in edits:
        if item[0].startswith('~'):
            hits = [k for k, v in data.items() if item[0][1:] in v]
            if len(hits) != 1:
                print('AMBIGUOUS' if hits else 'NOT FOUND', item[0][:60], hits[:4]); continue
            item = (hits[0],) + tuple(item[1:])
            if dry: print('FOUND', hits[0], '=>', data[hits[0]][:200].replace(chr(10), ' / '))
        resolved.append(item)
    edits = resolved
    if dry:
        return
    sources = {p: read(p) for p in glob.glob(os.path.join(NARR, '**', '*.fscript'), recursive=True)}
    done = skipped = 0
    for item in edits:
        key, old, new = item[0], item[1], item[2]
        whole = len(item) > 3 and item[3]
        current = data.get(key)
        if current is None:
            print('NO KEY  ', key); skipped += 1; continue
        if old not in current:
            print('NO MATCH', key, '| has:', current[:90]); skipped += 1; continue
        value = new(current) if callable(new) else (new if whole else current.replace(old, new, 1))
        # the string table: replace the JSON-escaped old value by the new one, on that key's line only
        pattern = re.compile(r'("' + re.escape(key) + r'"\s*:\s*)"((?:[^"\\]|\\.)*)"')
        m = pattern.search(en)
        en = en[:m.start()] + m.group(1) + '"' + jstr(value) + '"' + en[m.end():]
        data[key] = value
        # the FScript source: the text as written there (one line, \n for a line break)
        raw_old = current.replace('\n', '\\n')
        raw_new = value.replace('\n', '\\n')
        hit = False
        for p, text in sources.items():
            if raw_old in text:
                sources[p] = text.replace(raw_old, raw_new, 1)
                hit = True
                break
        print('OK      ', key, '' if hit else '(en.json only: not in any .fscript)')
        done += 1
    write(EN, en)
    for p, text in sources.items():
        if text != read(p):
            write(p, text)
    json.loads(read(EN))
    print(f'{done} changed, {skipped} skipped')


if __name__ == '__main__':
    main(sys.argv[1])
