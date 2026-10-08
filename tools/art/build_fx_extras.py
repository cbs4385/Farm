"""Effect sprites the AI sheet could not give (and the sleeping "Z z z" over a villager asleep in bed, fx_sleep_zzz), and the slim library book (item_prop_book) (the spark's thin rays were lost when slicing, the petal came out as an egg): drawn here as
pixel grids. Project-made, not AI-generated. Writes Art/Placeholders/fx_ore_spark.png and fx_petal.png (16 x 16)."""
import os
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
PH = os.path.join(ROOT, "Assets", "_Project", "Art", "Placeholders")

PALETTE = {"o": (58, 38, 24), "y": (255, 232, 140), "a": (242, 193, 78), "r": (217, 152, 59), "t": (184, 85, 58),
           "p": (236, 148, 170), "q": (248, 196, 208), "d": (196, 100, 128), "b": (74, 111, 165), "B": (49, 78, 125), "c": (244, 230, 195), "g": (242, 193, 78)}

SPARK = [
    "................",
    ".......y........",
    ".......a........",
    "..r....a....r...",
    "...a...y...a....",
    "....a..y..a.....",
    ".....a.y.a......",
    ".yaaayyyyyaaay..",
    ".....a.y.a......",
    "....a..y..a.....",
    "...a...a...a....",
    "..r....a....r...",
    ".......a........",
    ".......y........",
    "................",
    "................",
]
PETAL = [
    "................",
    "................",
    "................",
    "......oo........",
    ".....oqqoo......",
    "....oqqqppo.....",
    "...oqqqpppdo....",
    "...oqqppppdo....",
    "...oqpppppdo....",
    "....opppppdo....",
    ".....oppddo.....",
    "......oddo......",
    ".......oo.......",
    "................",
    "................",
    "................",
]


BOOK = [
    "................",
    "................",
    "................",
    "..oooooooooo....",
    "..oBBBBBBBBoo...",
    "..oBbbbbbbBco...",
    "..oBbggggbBco...",
    "..oBbbbbbbBco...",
    "..oBbggggbBco...",
    "..oBbbbbbbBco...",
    "..oBbbbbbbBco...",
    "..oBBBBBBBBco...",
    "..ooooooooooo...",
    "................",
    "................",
    "................",
]


def sleep_icon():
    """Three Zs climbing to the right, pale blue with a darker outline (fx_sleep_zzz, 16 x 16)."""
    fill = set()
    for x0, y0, n in ((8, 1, 7), (3, 7, 5), (1, 12, 3)):
        for i in range(n):
            fill.add((x0 + i, y0)); fill.add((x0 + i, y0 + n - 1))                 # top and bottom bars
            fill.add((x0 + n - 1 - i, y0 + i))                                        # the diagonal from top right to bottom left
    im = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
    for (x, y) in fill:
        for dx in (-1, 0, 1):
            for dy in (-1, 0, 1):
                ox, oy = x + dx, y + dy
                if 0 <= ox < 16 and 0 <= oy < 16 and (ox, oy) not in fill: im.putpixel((ox, oy), (74, 100, 160, 255))
    for (x, y) in fill: im.putpixel((x, y), (236, 244, 255, 255))
    im.save(os.path.join(PH, "fx_sleep_zzz.png"))


def draw(rows, name):
    im = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
    for y, row in enumerate(rows):
        for x, ch in enumerate(row):
            if ch != ".": im.putpixel((x, y), PALETTE.get(ch, (255, 255, 255)) + (255,))
    im.save(os.path.join(PH, name))


if __name__ == "__main__":
    draw(SPARK, "fx_ore_spark.png")
    draw(PETAL, "fx_petal.png")
    draw(BOOK, "item_prop_book.png")
    sleep_icon()
