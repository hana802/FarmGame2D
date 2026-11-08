# Building depth occlusion

Buildings use the same Y-axis depth formula as the player, with a small positive building offset. The offset keeps the roof in front while the player's feet are behind or close to the building footprint, but the player's lower Y position still wins when standing in front of the door. This avoids permanently forcing every house above the player and preserves natural front/back movement.

The PlayMode smoke test verifies that the farm house receives the shared building depth offset.
