"""Packs Builds/Linux/<version> into a .tar.gz with real Linux permissions (the player is built on Windows, which does not record the executable bit).

    python tools/make_linux_tarball.py [version] [output.tar.gz]

The archive unpacks to a folder `WetherellFarmSaga/` and is started with ./Farm. The backup and Burst debug folders are left out. Not part of the game or its tests."""
import os
import sys
import tarfile

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
VERSION = sys.argv[1] if len(sys.argv) > 1 else "0.0.1"
SOURCE = os.path.join(ROOT, "Builds", "Linux", VERSION)
OUT = sys.argv[2] if len(sys.argv) > 2 else os.path.join(ROOT, "Builds", f"WetherellFarmSaga-linux-x64-{VERSION}.tar.gz")
TOP = "WetherellFarmSaga"
SKIP_SUFFIXES = ("_BackUpThisFolder_ButDontShipItWithYourGame", "_BurstDebugInformation_DoNotShip")
README = """Wetherell Farm Saga {version} - Linux (64 bit)

Run:   ./Farm
Needs: a desktop with X11 or Wayland and OpenGL or Vulkan drivers (it was started on Ubuntu 22.04).
Saves and settings: ~/.config/unity3d/Farm Studio/Wetherell Farm Saga/
If ./Farm says "permission denied":  chmod +x Farm
"""


def is_executable(name):
    return name == "Farm" or name.endswith(".so") or ".so." in name or name.endswith(".sh")


def main():
    if not os.path.isdir(SOURCE):
        sys.exit(f"missing {SOURCE}: build the Linux player first (docs/BUILD.md)")
    count = 0
    with tarfile.open(OUT, "w:gz") as tar:
        def add_dir(arcname):
            info = tarfile.TarInfo(arcname)
            info.type = tarfile.DIRTYPE
            info.mode = 0o755
            info.uname = info.gname = "root"
            tar.addfile(info)

        add_dir(TOP)
        for folder, dirs, files in os.walk(SOURCE):
            dirs[:] = sorted(d for d in dirs if not d.endswith(SKIP_SUFFIXES))
            rel = os.path.relpath(folder, SOURCE)
            base = TOP if rel == "." else f"{TOP}/{rel.replace(os.sep, '/')}"
            if rel != ".":
                add_dir(base)
            for name in sorted(files):
                path = os.path.join(folder, name)
                info = tar.gettarinfo(path, f"{base}/{name}")
                info.mode = 0o755 if is_executable(name) else 0o644
                info.uid = info.gid = 0
                info.uname = info.gname = "root"
                with open(path, "rb") as f:
                    tar.addfile(info, f)
                count += 1
        readme = README.format(version=VERSION).encode("utf-8")
        info = tarfile.TarInfo(f"{TOP}/README.txt")
        info.size = len(readme)
        info.mode = 0o644
        import io
        tar.addfile(info, io.BytesIO(readme))
    print(f"{OUT}: {count} files, {os.path.getsize(OUT) / 1048576:.1f} MB")


main()
