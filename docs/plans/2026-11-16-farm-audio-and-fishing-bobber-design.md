# Farm Audio and Fishing Bobber Design

## Goal

Give the farm a light pixel-game soundscape and make every fishing cast land on the water side of a fishing spot rather than on the shore.

## Audio design

- Generate a quiet looping chiptune-style background track at runtime, so the project does not depend on copyrighted or externally downloaded audio.
- Generate short deterministic effects for tilling, planting, watering, harvesting, casting, and catching.
- Keep music and effects on separate `AudioSource` components. The existing master-volume setting continues to control both through `AudioListener.volume`.
- Create clips once at startup and reuse them to avoid allocations during play.
- Start music on the first player input so WebGL browsers accept audio playback.

## Fishing fix

Fishing spot colliders sit at the shoreline. The old bobber used the collider's exact position, which could be dry ground. Resolve a cast target by moving from the spot in the direction away from the player. Because the player approaches each spot from land, this consistently places the bobber on the water side.

## Verification

- Editor tests verify every generated sound is finite, audible, and normalized.
- Editor tests verify cast targets move the expected distance away from the player for pond, west-coast, and south-coast approaches.
- Run the full EditMode suite and rebuild the WebGL game.
