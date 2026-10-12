"""Writes tools/servicelocator-baseline.txt: how many times each game source file uses `ServiceLocator.` (code review 2026-10-11, ADR 0006).
The test ServiceLocatorRatchetTests fails when any file uses it more than the baseline says, or when a file the baseline does not know starts to use it.
When a count goes DOWN, run this script and commit the lower numbers: the baseline only ever shrinks.
Run from the repository root: python tools/servicelocator_baseline.py"""
import os
import re

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..'))
SCRIPTS = os.path.join(ROOT, 'Assets', '_Project', 'Scripts')
OUT = os.path.join(ROOT, 'tools', 'servicelocator-baseline.txt')
SKIP = {'Core/ServiceLocator.cs'}


def counts():
    result = {}
    for d, _, files in os.walk(SCRIPTS):
        rel_dir = os.path.relpath(d, SCRIPTS).replace(os.sep, '/')
        if rel_dir.split('/')[0] == 'Editor':
            continue
        for f in files:
            if not f.endswith('.cs'):
                continue
            rel = (rel_dir + '/' + f) if rel_dir != '.' else f
            if rel in SKIP:
                continue
            n = len(re.findall(r'ServiceLocator\.', open(os.path.join(d, f), encoding='utf-8-sig', errors='ignore').read()))
            if n:
                result[rel] = n
    return result


if __name__ == '__main__':
    c = counts()
    with open(OUT, 'w', encoding='utf-8', newline='\n') as out:
        out.write('# ServiceLocator uses per game source file (ADR 0006). Only ever lower these: python tools/servicelocator_baseline.py\n')
        for k in sorted(c):
            out.write(f'{k} {c[k]}\n')
    print(sum(c.values()), 'uses in', len(c), 'files ->', OUT)
