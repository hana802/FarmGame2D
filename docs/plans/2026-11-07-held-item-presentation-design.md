# Held Item Presentation Design

The current hotbar selection is rendered as an item in the player's hand. The first scope covers the three existing farm tools: hoe, wheat seed pouch, and watering can.

`GameSession` remains the source of truth for the selected tool. `PrototypePlayerController` passes the selection, facing direction, movement state, and availability to a dedicated `HeldItemPresenter`. The presenter owns a child `SpriteRenderer`, selects the matching item sprite, mirrors it when facing left, and places it in front of or behind the character according to direction. A small movement bob keeps the item visually attached without changing gameplay physics.

Seeds are hidden when the inventory contains no wheat seeds. The hoe and watering can are permanent tools and remain visible whenever selected. The design keeps item presentation separate from farming rules so later tools and carried produce can reuse the same renderer without changing movement code.

Play mode coverage verifies that the held-item renderer is created, loads non-placeholder art, begins with the hoe, and changes when the selected tool changes.
