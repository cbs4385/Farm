"""The village's roads, paths and streets are brown like the dirt of the woods' track (playtest 2026-10-16: "recolor the dirt trails in the village with the brown from the dirt
trail"). `tile_road` is a copy of `tile_dirt` under its own name, so that the village roads are a different kind of ground: tile_dirt can be tilled in the village (the green is
farmable) and a road must not be. Run from the repository root: py tools/art/build_village_road.py"""
import os
import shutil

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
ART = os.path.join(ROOT, "Assets", "_Project", "Art", "Placeholders")


def main():
    shutil.copyfile(os.path.join(ART, "tile_dirt.png"), os.path.join(ART, "tile_road.png"))
    print("wrote tile_road")


if __name__ == "__main__":
    main()
