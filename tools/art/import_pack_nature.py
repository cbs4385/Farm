"""Brings pieces of the purchased "Colored 1-bit Nature" pack into the game, scaled up two times with nearest-neighbour so that its 8 x 8 grid becomes the
game's 16 x 16 grid. Run from the repository root: python tools/art/import_pack_nature.py
Used: the mushrooms (the wild mushroom in the woods and its item picture) and a sunflower (a tall meadow flower that grows in summer and fall and can be cut
with the scythe). The pack's jungle pieces (the giant tree, monstera, palm, ruined columns) do not belong in a New England village, and its grey rocks are
dithered to a low contrast that reads badly on grass, so they are not used.
The pack's black background is made transparent."""
import os
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
SHEET = os.path.join(ROOT, "AssetPacks", "colored-1-bit-nature-v1", "colored-1-bit-nature.png")
OUT = os.path.join(ROOT, "Assets", "_Project", "Art", "Placeholders")
SCALE = 2


def load():
    im = Image.open(SHEET).convert("RGBA")
    px = im.load()
    for y in range(im.height):
        for x in range(im.width):
            if px[x, y][:3] == (0, 0, 0):
                px[x, y] = (0, 0, 0, 0)
    return im


def grab(im, box):
    piece = im.crop(box)
    piece = piece.crop(piece.getbbox())
    return piece.resize((piece.width * SCALE, piece.height * SCALE), Image.NEAREST)


def on_canvas(piece, size, bottom=True):
    canvas = Image.new("RGBA", size, (0, 0, 0, 0))
    x = (size[0] - piece.width) // 2
    y = size[1] - piece.height - (1 if bottom else 0) if bottom else (size[1] - piece.height) // 2
    canvas.alpha_composite(piece, (x, y))
    return canvas


def main():
    im = load()
    pair = grab(im, (92, 148, 110, 166))           # two tan mushrooms, 8 x 7 -> 16 x 14
    single = grab(im, (76, 148, 92, 166))          # one tan mushroom, 6 x 5 -> 12 x 10
    sunflower = grab(im, (176, 76, 192, 100))      # 10 x 24 -> 20 x 48
    on_canvas(pair, (16, 16)).save(os.path.join(OUT, "obj_mushroom.png"))
    on_canvas(single, (16, 16), bottom=False).save(os.path.join(OUT, "item_forage_mushroom.png"))
    on_canvas(sunflower, (20, 48)).save(os.path.join(OUT, "obj_sunpatch.png"))
    print("wrote obj_mushroom, item_forage_mushroom, obj_sunpatch")


if __name__ == "__main__":
    main()
