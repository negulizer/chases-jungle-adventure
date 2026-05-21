# Jungle Chase Art Assets

This folder contains bright, kid-friendly SVG artwork for the board game.

## Asset list and use

- `Animals/spider.svg` — special board space icon for Spider Web spaces.
- `Animals/monkey.svg` — special board space icon for Monkey spaces.
- `Animals/snake.svg` — special board space icon for Snake spaces.
- `Animals/raft.svg` — special board space icon for Raft Ride spaces.

- `Tokens/token_red.svg` — player token (red outfit/base, dark hair).
- `Tokens/token_blue.svg` — player token (blue outfit/base, blonde hair).
- `Tokens/token_green.svg` — player token (green outfit/base, auburn hair).
- `Tokens/token_yellow.svg` — player token (yellow outfit/base, black hair).

- `waterfall_finish.svg` — finish destination icon with waterfall/rainbow and FINISH text.

- `Cards/card_red.svg`, `card_blue.svg`, `card_green.svg`, `card_yellow.svg`, `card_orange.svg`, `card_purple.svg` — color movement cards.
- `Cards/card_spider.svg`, `card_monkey.svg`, `card_snake.svg`, `card_raft.svg` — animal event cards.

- `Board/path_tile_normal.svg` — normal path-space tile art.
- `Board/board_background.svg` — 1920x1080 board backdrop.
- `Board/jungle_vine_divider.svg` — decorative row divider strip.

## Converting SVG to PNG for Unity

Use any of these methods:

1. **Browser export**
   - Open SVG in a browser.
   - Right-click image and save (or screenshot at target size).
2. **Inkscape (recommended)**
   - File → Open SVG.
   - File → Export PNG.
   - Set exact width/height and export.
3. **Unity Vector Graphics package**
   - Install `com.unity.vectorgraphics` in Package Manager.
   - Import SVG directly and render at runtime.

## Recommended PNG export sizes

- Animal icons: **256x256**
- Player tokens: **128x160** (or 256x320 for extra supersampling)
- Finish icon: **512x512** (or 1024x1024 if used large)
- Cards: **200x280** (or 400x560 for high-DPI)
- Path tile: **100x100** (or 200x200)
- Board background: **1920x1080**
- Vine divider: **1920x80**

