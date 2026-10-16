"""Every trail is the brown of `tile_dirt` (playtest 2026-10-16: "recolor the dirt trails in the village with the brown from the dirt trail", then "replace the white trail on all maps
with the tile_dirt color"). `tile_road` (the village's roads) and `tile_path` (the farm's road and the forest's trail, which was a pale sand colour) are copies of `tile_dirt` under
their own names: tile_dirt can be tilled (in the village the green is farmable) and a road or a trail must not be. Run from the repository root: py tools/art/build_village_road.py"""
import os
import shutil

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
ART = os.path.join(ROOT, "Assets", "_Project", "Art", "Placeholders")


def main():
    for name in ("tile_road", "tile_path"):
        shutil.copyfile(os.path.join(ART, "tile_dirt.png"), os.path.join(ART, name + ".png"))
        print("wrote", name)


if __name__ == "__main__":
    main()
