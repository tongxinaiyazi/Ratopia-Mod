from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


OUTPUT = Path(r"D:\SOFTWARE\项目\鼠托邦mod\PopulationCustomizer\NexusMods\人口自定义-v0.1.3\4-封面.png")
EN_FONT = Path(r"C:\Windows\Fonts\segoeuib.ttf")
ZH_FONT = Path(r"C:\Windows\Fonts\msyhbd.ttc")


# Deliberately text-only: a flat background and exactly the two requested titles.
cover = Image.new("RGB", (1920, 1080), (5, 39, 43))
draw = ImageDraw.Draw(cover)
title_en = ImageFont.truetype(str(EN_FONT), 142)
title_zh = ImageFont.truetype(str(ZH_FONT), 102)

draw.text(
    (960, 430),
    "Population Customizer",
    font=title_en,
    fill=(247, 246, 235),
    anchor="mm",
)
draw.text(
    (960, 650),
    "人口自定义",
    font=title_zh,
    fill=(237, 169, 48),
    anchor="mm",
)

OUTPUT.parent.mkdir(parents=True, exist_ok=True)
cover.save(OUTPUT, format="PNG", optimize=True)
print(f"COVER={OUTPUT}")
print(f"SIZE={cover.width}x{cover.height}")
