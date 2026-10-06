"""Two effect sprites the AI sheet could not give, and the slim library book (item_prop_book) (the spark's thin rays were lost when slicing, the petal came out as an egg): drawn here as
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
