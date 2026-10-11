"""Makes generated Unity scene files deterministic, so that regenerating a scene that did not really change gives the same bytes (code review 2026-10-11, item 4).

Unity gives the objects of a freshly built scene new file ids, and writes them in a different order, every time `SceneSetup` runs: a scene that did not change
differs from the committed one on every line. This tool renames the ids and sorts the documents by what each object *is* (its class, its fields, and what it
points at), the way graph canonicalisation does, so that equal scenes come out byte for byte equal and a real change shows as a small diff. Managed-reference
numbers (`rid:`), which are random too, are renumbered inside each object.

Usage:  python tools/normalize_scenes.py [files or folders ...]     (default: Assets/_Project/Scenes)
        python tools/normalize_scenes.py --check [paths]            (exit 1 and list the files that are not normalised yet)

Safe by construction: only ids that point inside the file (`{fileID: N}` with no guid) are renamed, the special scene-roots id and 0 are kept, and the result is
the same set of documents with the same fields. Run after SceneSetup (tools/ship.sh does) and before committing.
"""
import hashlib
import os
import re
import sys

HEADER = re.compile(r'^--- !u!(\d+) &(-?\d+)( stripped)?\s*$')
REF = re.compile(r'\{fileID: (-?\d+)\}')
RID = re.compile(r'\brid: (\d+)')
KEEP_ABOVE = 9000000000000000000          # the scene roots document uses the largest id; leave such ids alone
KEEP_BELOW = 100                          # the scene's own settings documents (&1..&4) have fixed ids too
ROUNDS = 6
FIRST_ID = 100000010


def split_documents(text):
    """Returns (preamble lines, [[class, id, stripped, body lines]]) for text that uses \\n line ends."""
    pre, docs, cur = [], [], None
    for line in text.split('\n'):
        m = HEADER.match(line)
        if m:
            cur = [int(m.group(1)), int(m.group(2)), bool(m.group(3)), []]
            docs.append(cur)
        elif cur is None:
            pre.append(line)
        else:
            cur[3].append(line)
    return pre, docs


def renumber_rids(lines):
    """Managed references carry random numbers; inside one object they are renumbered in order of appearance."""
    seen = {}

    def sub(m):
        return 'rid: %d' % seen.setdefault(m.group(1), 1000 + len(seen))
    return [RID.sub(sub, line) for line in lines]


def normalise(text):
    eol = '\r\n' if '\r\n' in text else '\n'
    text = text.replace('\r\n', '\n')
    pre, docs = split_documents(text)
    for d in docs:
        d[3] = renumber_rids(d[3])
    movable = {d[1] for d in docs if KEEP_BELOW <= abs(d[1]) < KEEP_ABOVE}

    # a signature per document: its class and its text with every internal id replaced by the signature of the thing it points at
    sig = {d[1]: hashlib.sha1(str(d[0]).encode()).hexdigest() for d in docs}
    bodies = {d[1]: '\n'.join(d[3]) for d in docs}
    for _ in range(ROUNDS):
        nxt = {}
        for d in docs:
            def sub(m, sig=sig):
                n = int(m.group(1))
                return '{fileID: @' + sig[n] + '}' if n in sig and n in movable else m.group(0)
            nxt[d[1]] = hashlib.sha1(f'{d[0]}|{d[2]}|{REF.sub(sub, bodies[d[1]])}'.encode()).hexdigest()
        sig = nxt

    moving = sorted((d for d in docs if d[1] in movable), key=lambda d: sig[d[1]])
    first = [d for d in docs if abs(d[1]) < KEEP_BELOW]                         # the scene settings keep their place, at the start
    last = [d for d in docs if abs(d[1]) >= KEEP_ABOVE]                         # the scene roots keep theirs, at the end
    new_id = {d[1]: FIRST_ID + n * 10 for n, d in enumerate(moving)}

    def rename(m):
        n = int(m.group(1))
        return '{fileID: %d}' % new_id[n] if n in new_id else m.group(0)

    out = list(pre)
    for d in first + moving + last:
        out.append('--- !u!%d &%d%s' % (d[0], new_id.get(d[1], d[1]), ' stripped' if d[2] else ''))
        out.extend(REF.sub(rename, line) for line in d[3])
    result = '\n'.join(out)
    return result.replace('\n', '\r\n') if eol == '\r\n' else result


def scene_files(paths):
    for p in paths:
        if os.path.isdir(p):
            for root, _, files in os.walk(p):
                for f in sorted(files):
                    if f.endswith('.unity'):
                        yield os.path.join(root, f)
        elif p.endswith('.unity'):
            yield p


def main(argv):
    check = '--check' in argv
    paths = [a for a in argv if not a.startswith('--')] or [os.path.join('Assets', '_Project', 'Scenes')]
    stale = []
    for path in scene_files(paths):
        with open(path, encoding='utf-8', newline='') as f:
            text = f.read()
        result = normalise(text)
        if result != text:
            stale.append(path)
            if not check:
                with open(path, 'w', encoding='utf-8', newline='') as f:
                    f.write(result)
    if check:
        for p in stale:
            print('not normalised:', p)
        return 1 if stale else 0
    print('normalised %d scene file(s)' % len(stale))
    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
