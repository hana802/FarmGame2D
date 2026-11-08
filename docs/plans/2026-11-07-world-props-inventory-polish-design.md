# World props and inventory polish

## Goal

Replace the remaining prototype blocks for the cow and mining nodes, and make every inventory label stay inside a dedicated visual slot at the 1280×720 reference resolution.

## Visual direction

- Match the warm outlined pixel-art language already used by the chicken, weeds, and village props.
- Keep the cow and rock silhouettes readable at normal gameplay zoom.
- Preserve transparent padding in source images, then crop to the opaque subject in `WorldArtCatalog` so scaling and collisions are predictable.

## Inventory layout

The inventory window is divided into five non-overlapping vertical bands: illustrated header, description, tool row, seed grid, owned-item summary, and close action. The 820×550 panel keeps the UI-atlas aspect close to its source while leaving the world hotbar visible below it.

## Automatic verification

- EditMode layout tests assert ordered bands and panel containment.
- PlayMode smoke tests reject 1×1 placeholder textures for the cow and mining nodes.
- A browser script opens the WebGL build, selects a character, opens the inventory, walks to the farm animals/mining area, captures screenshots, and reports browser console errors.
