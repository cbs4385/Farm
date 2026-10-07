#!/bin/bash
# Usage: Steam/upload.sh <branch>     (branch must not be "default": promote that on the partner site)
# Needs: STEAMCMD (path to steamcmd), STEAM_USER (build account). Run from the repo root after the release builds exist.
set -euo pipefail
branch="${1:?branch name, e.g. beta}"
[ "$branch" != "default" ] || { echo "refusing to set default live from a script"; exit 1; }
: "${STEAMCMD:?set STEAMCMD}" "${STEAM_USER:?set STEAM_USER}"

version=$(grep -m1 'bundleVersion:' ProjectSettings/ProjectSettings.asset | awk '{print $2}')
for os in Windows Linux; do
  [ -d "Builds/$os/$version" ] || { echo "missing Builds/$os/$version (build it first)"; exit 1; }
done
grep -q '"AppID" "0"' Steam/app_build.vdf && { echo "Steam/app_build.vdf still has the placeholder App ID (0)"; exit 1; }
# The App ID is known (5408390); the depot ids come from the partner site and must replace the 0s in the depot files and in app_build.vdf.
grep -q '"DepotID" "0"' Steam/depot_windows.vdf Steam/depot_linux.vdf && { echo "Steam/depot_*.vdf still have the placeholder depot id (0)"; exit 1; }
grep -qE '^[[:space:]]*"0"[[:space:]]+"depot_' Steam/app_build.vdf && { echo "Steam/app_build.vdf still lists placeholder depot ids (0)"; exit 1; }

# Stamp the version into temporary copies of the scripts so the checked-in templates stay generic.
tmp=$(mktemp -d)
root=$(pwd -W 2>/dev/null || pwd)      # under Git Bash on Windows this is C:/..., which steamcmd.exe can read ($PWD is /c/..., which it cannot)
sed "s#VERSION#$version#g" Steam/depot_windows.vdf > "$tmp/depot_windows.vdf"
sed "s#VERSION#$version#g" Steam/depot_linux.vdf > "$tmp/depot_linux.vdf"
sed -e "s#\"SetLive\" \"[a-z]*\"#\"SetLive\" \"$branch\"#" -e "s#Wetherell Farm Saga build.*\"#Wetherell Farm Saga $version\"#" \
    -e "s#\.\./Builds/steam-output/#$root/Builds/steam-output/#" -e "s#\.\./Builds/#$root/Builds/#" Steam/app_build.vdf > "$tmp/app_build.vdf"

"$STEAMCMD" +login "$STEAM_USER" +run_app_build "$tmp/app_build.vdf" +quit
rm -rf "$tmp"
