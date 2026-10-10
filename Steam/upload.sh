#!/bin/bash
# Usage: Steam/upload.sh <branch>     ("default" needs ALLOW_DEFAULT=1: it is what every player gets. While the game is not live (the owner, 2026-10-08) that is how
#                                     playtests are published; before the real release, drop ALLOW_DEFAULT from the build routine and promote on the partner site.)
# Branch "none" uploads the build without setting it live anywhere (nobody receives it): use it for the very first upload, because Steamworks only
# lets you create a branch once a build exists; then create the branch, set a password, and set this build live on it from the Builds page.
# Uploads the Windows, Linux and macOS depots. SKIP_MAC=1 uploads only Windows and Linux (when there is no macOS build of this version yet).
# Needs: STEAMCMD (path to steamcmd), STEAM_USER (build account). Run from the repo root after the release builds exist.
set -euo pipefail
branch="${1:?branch name, e.g. beta}"
# A hold: while Steam/PUBLISH_HOLD exists nothing is uploaded (the owner, 2026-10-10: no publishing until a human review is done). Delete the file to lift it.
if [ -f Steam/PUBLISH_HOLD ]; then
  echo "refusing to upload: Steam/PUBLISH_HOLD exists:"; sed 's/^/  /' Steam/PUBLISH_HOLD; exit 1
fi
# Steam calls the default branch "public" (steamcmd refuses SetLive "default"): both names mean the branch every player gets.
if [ "$branch" = "default" ] || [ "$branch" = "public" ]; then
  [ "${ALLOW_DEFAULT:-}" = "1" ] || { echo "refusing to set the default branch live from a script without ALLOW_DEFAULT=1"; exit 1; }
  branch=public
fi
: "${STEAMCMD:?set STEAMCMD}" "${STEAM_USER:?set STEAM_USER}"

version=$(grep -m1 'bundleVersion:' ProjectSettings/ProjectSettings.asset | awk '{print $2}')
oses="Windows Linux"
[ "${SKIP_MAC:-}" = "1" ] || oses="$oses Mac"
for os in $oses; do
  [ -d "Builds/$os/$version" ] || { echo "missing Builds/$os/$version (build it first; SKIP_MAC=1 skips macOS)"; exit 1; }
done
grep -q '"AppID" "0"' Steam/app_build.vdf && { echo "Steam/app_build.vdf still has the placeholder App ID (0)"; exit 1; }
# The App ID is known (5408390); the depot ids come from the partner site and must replace the 0s in the depot files and in app_build.vdf.
grep -q '"DepotID" "0"' Steam/depot_windows.vdf Steam/depot_linux.vdf Steam/depot_macos.vdf && { echo "Steam/depot_*.vdf still have the placeholder depot id (0)"; exit 1; }
grep -qE '^[[:space:]]*"0"[[:space:]]+"depot_' Steam/app_build.vdf && { echo "Steam/app_build.vdf still lists placeholder depot ids (0)"; exit 1; }

# Stamp the version into temporary copies of the scripts so the checked-in templates stay generic.
tmp=$(mktemp -d)
root=$(pwd -W 2>/dev/null || pwd)      # under Git Bash on Windows this is C:/..., which steamcmd.exe can read ($PWD is /c/..., which it cannot)
sed "s#VERSION#$version#g" Steam/depot_windows.vdf > "$tmp/depot_windows.vdf"
sed "s#VERSION#$version#g" Steam/depot_linux.vdf > "$tmp/depot_linux.vdf"
sed "s#VERSION#$version#g" Steam/depot_macos.vdf > "$tmp/depot_macos.vdf"
sed -e "s#\"SetLive\" \"[a-z]*\"#\"SetLive\" \"$branch\"#" -e "s#Wetherell Farm Saga build.*\"#Wetherell Farm Saga $version\"#" \
    -e "s#\.\./Builds/steam-output/#$root/Builds/steam-output/#" -e "s#\.\./Builds/#$root/Builds/#" Steam/app_build.vdf > "$tmp/app_build.vdf"
if [ "$branch" = "none" ]; then
  grep -v '"SetLive"' "$tmp/app_build.vdf" > "$tmp/app_build.no_live" && mv "$tmp/app_build.no_live" "$tmp/app_build.vdf"
  echo "branch none: uploading without setting any branch live"
fi
if [ "${SKIP_MAC:-}" = "1" ]; then
  grep -v 'depot_macos.vdf' "$tmp/app_build.vdf" > "$tmp/app_build.no_mac" && mv "$tmp/app_build.no_mac" "$tmp/app_build.vdf"
  echo "SKIP_MAC=1: uploading Windows and Linux only"
fi

"$STEAMCMD" +login "$STEAM_USER" +run_app_build "$tmp/app_build.vdf" +quit
rm -rf "$tmp"
