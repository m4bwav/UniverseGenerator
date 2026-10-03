#!/usr/bin/env python3
"""Draw the package icon: a two-armed spiral galaxy on a night-blue rounded square, 128 x 128 PNG.

    python scripts/make-icon.py icon.png

Needs Pillow. The stars come from a fixed-seed Random, so the icon is the same on every run.
"""
import math
import random
import sys

from PIL import Image, ImageDraw, ImageFilter

SCALE = 4
SIZE = 128


def main(path: str) -> None:
    n = SIZE * SCALE
    canvas = Image.new("RGBA", (n, n), (0, 0, 0, 0))
    draw = ImageDraw.Draw(canvas)
    draw.rounded_rectangle((0, 0, n - 1, n - 1), radius=26 * SCALE, fill=(14, 22, 52, 255))

    glow = Image.new("RGBA", (n, n), (0, 0, 0, 0))
    g = ImageDraw.Draw(glow)
    c = n / 2
    g.ellipse((c - 18 * SCALE, c - 18 * SCALE, c + 18 * SCALE, c + 18 * SCALE), fill=(255, 226, 170, 200))
    glow = glow.filter(ImageFilter.GaussianBlur(9 * SCALE))
    canvas.alpha_composite(glow)

    rng = random.Random(7)
    stars = ImageDraw.Draw(canvas)
    for arm in range(2):
        for i in range(260):
            t = i / 260
            r = (6 + 46 * t) * SCALE
            a = arm * math.pi + 4.2 * t + rng.gauss(0, 0.16 + 0.1 * t)
            r += rng.gauss(0, 2.2 * SCALE)
            x, y = c + r * math.cos(a), c + r * math.sin(a) * 0.82
            size = (0.7 + rng.random() * (1.6 - t)) * SCALE
            warm = rng.random() < 0.25
            colour = (255, 214, 160, 255) if warm else (205, 222, 255, 255)
            stars.ellipse((x - size, y - size, x + size, y + size), fill=colour)
    for _ in range(40):
        x, y = rng.uniform(8, SIZE - 8) * SCALE, rng.uniform(8, SIZE - 8) * SCALE
        s = rng.uniform(0.4, 0.9) * SCALE
        stars.ellipse((x - s, y - s, x + s, y + s), fill=(255, 255, 255, 170))
    stars.ellipse((c - 5 * SCALE, c - 5 * SCALE, c + 5 * SCALE, c + 5 * SCALE), fill=(255, 246, 225, 255))

    canvas.resize((SIZE, SIZE), Image.LANCZOS).save(path, optimize=True)


if __name__ == "__main__":
    main(sys.argv[1] if len(sys.argv) > 1 else "icon.png")
