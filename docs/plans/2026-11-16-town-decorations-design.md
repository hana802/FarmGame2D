# Town decorations

The general store sells three reusable town decoration items: a flower pot, a street lamp, and a bench. Purchases use the existing confirmation flow, subtract money, and add one item to the bag. The bag has a decoration page where the player can choose an owned item. Space places it on the grass ahead; E picks up a placed decoration. A placement consumes one bag item and pickup returns it.

Placement is limited to the village surface. It requires a ground tile, avoids roads and nearby colliders, and keeps decorations inside the village bounds. These checks protect entrances, paths, the pond, and other actors. Placed item type and position are stored in the existing save file and restored when the world builds. The new field is optional so older saves still load.

Decoration sprites are generated once as small pixel art using the existing point-filtered sprite style. This keeps them visually consistent with the game without an external asset dependency.
