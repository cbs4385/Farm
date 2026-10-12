#!/bin/bash
# The release flow in one place (code review 2026-10-11, item 5): regenerate, test, commit, build the three platforms, and (only when asked) upload and push.
#
#   tools/ship.sh "commit message"            tests, commit, builds; stops at the first failure and commits nothing if a test fails
#   PUBLISH=1 tools/ship.sh "commit message"  the same, then Steam default and `git push origin main` (not while Steam/PUBLISH_HOLD exists)
#   SKIP_PLAYMODE=1 tools/ship.sh "..."       EditMode only (a fast check; never for a release)
#
# Needs: Unity 6000.6.2f1 at the usual place (UNITY overrides it), the project closed in the Editor, git, python, and for PUBLISH=1: STEAMCMD and STEAM_USER
# (defaults: C:/steamcmd/steamcmd.exe and cbs4385). Logs and results go to Builds/ (not committed). Bump bundleVersion and CHANGELOG.md before running.
set -u
message="${1:?commit message (one line; say what changed for a player)}"
cd "$(dirname "$0")/.."
U="${UNITY:-C:/Program Files/Unity/Hub/Editor/6000.6.2f1/Editor/Unity.exe}"
mkdir -p Builds
stamp=$(date +%Y%m%d-%H%M%S)

unity() { rm -f Temp/UnityLockfile; "$U" -batchmode "$@" > /dev/null 2>&1; }
cleanup() { git checkout ProjectSettings/UnityConnectSettings.asset 2>&1 | grep -v warning; }

# Counts failed tests in an NUnit results file; prints the summary and the failures.
summarize() {
  python - "$1" <<'PY'
import re, sys
try:
    t = open(sys.argv[1], encoding='utf-8').read()
except OSError:
    print('no results file'); sys.exit(2)
m = re.search(r'<test-run [^>]*result="([^"]*)" total="(\d+)" passed="(\d+)" failed="(\d+)"', t)
if not m: print('unreadable results'); sys.exit(2)
print(f'{m.group(1)}: {m.group(3)} passed of {m.group(2)}, {m.group(4)} failed')
for f in re.finditer(r'<test-case [^>]*fullname="([^"]*)"[^>]*result="Failed"', t): print('  FAIL', f.group(1))
sys.exit(0 if m.group(4) == '0' else 1)
PY
}

step() {   # step <label> <log> <unity args...>: stops the whole flow when Unity exits with an error
  local label="$1" log="$2"; shift 2
  unity -nographics -projectPath . "$@" -logFile "Builds/$log"
  local code=$?
  echo "$label $code"
  [ $code -eq 0 ] || { echo "$label FAILED: see Builds/$log"; cleanup; exit 1; }
}

step INPUT input.log -executeMethod Farm.Editor.InputAssetGenerator.GenerateAndExit
step SCENES scenes.log -executeMethod Farm.Editor.SceneSetup.CreateScenesAndExit
python tools/normalize_scenes.py || { echo 'SCENE NORMALISING FAILED'; exit 1; }          # same scene, same bytes (otherwise every ship rewrites every scene file)
step UIART uiart.log -executeMethod Farm.Editor.UiArtBuilder.BuildAndExit
step ATLAS atlas.log -executeMethod Farm.Editor.AtlasBuilder.BuildAndExit
step FSCRIPT fscript.log -executeMethod Farm.Editor.FScriptCompiler.CompileAndExit          # villager lines: .fscript to story JSON and the string table (tests fail if the sources and the JSON disagree)
step GEN gen.log -executeMethod Farm.Editor.ContentGenerator.GenerateAllAndExit
cleanup

# Tests. `-runTests` must not be combined with `-quit`. EditMode runs without graphics, PlayMode needs them.
em="Builds/r-edit-$stamp.xml"
unity -nographics -projectPath . -runTests -testPlatform EditMode -testResults "$em" -logFile "Builds/r-edit-$stamp.log"
summarize "$em" || { echo "EDITMODE FAILED - nothing committed or built"; cleanup; exit 1; }
if [ "${SKIP_PLAYMODE:-}" != "1" ]; then
  pm="Builds/r-play-$stamp.xml"
  unity -projectPath . -runTests -testPlatform PlayMode -testResults "$pm" -logFile "Builds/r-play-$stamp.log"
  summarize "$pm" || { echo "PLAYMODE FAILED - nothing committed or built. A test from the timing-sensitive family in docs/QA.md must pass twice when run alone before it counts as a flake."; cleanup; exit 1; }
fi
cleanup

git add -A Assets docs tools ProjectSettings/ProjectSettings.asset CHANGELOG.md Steam 2>&1 | grep -v warning
git commit -q -m "$message

Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>" 2>&1 | grep -v warning
git log --oneline | head -1

for t in Windows Linux Mac; do
  unity -nographics -projectPath . -executeMethod Farm.Editor.BuildScript.Build$t -logFile "Builds/build-$t.log"
  echo "$t $?"
  grep "\[BuildScript\]" "Builds/build-$t.log" | grep -i "Succeeded\|Failed\|guard"
  grep -q "Succeeded, 0 errors" "Builds/build-$t.log" || { echo "$t BUILD FAILED"; cleanup; exit 1; }
done
cleanup

if [ "${PUBLISH:-}" = "1" ]; then
  STEAMCMD="${STEAMCMD:-C:/steamcmd/steamcmd.exe}" STEAM_USER="${STEAM_USER:-cbs4385}" ALLOW_DEFAULT=1 Steam/upload.sh default || { echo "UPLOAD FAILED"; exit 1; }
  git push origin main || { echo "PUSH FAILED"; exit 1; }
fi
echo DONE
