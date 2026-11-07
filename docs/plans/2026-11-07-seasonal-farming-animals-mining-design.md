# Seasonal farming, animals, and mining design

## Goal

Turn the fixed wheat-only prototype into a broader farm loop. The player can till any tile in the farm field with the hoe, choose a specific seed from the bag, grow crops sold for the current season, care for chickens and cows, mine persistent ore nodes, and sell all gathered products.

## Domain model

`CropCatalog` owns crop rules: seed item, harvested item, Korean label, season, growth days, seed pack price, and sell price. `ItemCatalog` owns labels and sell prices for non-crop products. `FarmPlotData` stores its grid position and planted seed so old six-plot saves can be expanded without losing state. Harvest returns a plot to tilled soil instead of removing the field.

Animal ownership and daily production live in `GameSaveData`. Feeding a herd consumes one feed per animal and makes the matching product available the following day. Mining nodes use stable IDs and store their ore type and remaining regeneration days. This keeps every player-created or depleted state serializable.

## World and interaction flow

The farm contains a 6-by-4 grid of interactable ground tiles. Empty tiles become tilled only when the hoe is selected. Seed, watering, and harvest actions continue through the same nearby-interaction system. A cow pen and persistent mining area are added with lightweight prototype visuals, while existing authored farm art remains untouched.

The seed shop displays only the two crops for the current season and also sells feed and animals. The general store becomes a mining shop where the pickaxe and ores are sold. The bag exposes every owned seed separately and adds the pickaxe as a selectable tool once purchased.

## Compatibility and verification

Loading normalizes old saves: it extends the six plots to the new field size, assigns stable grid coordinates, migrates ready eggs, and initializes mining nodes. Existing enum values remain in place and new values are appended. Editor domain tests cover seasonal stock, crop-specific growth, plot expansion, mining regeneration, and item prices; the play-mode smoke test verifies the new world interactions.
