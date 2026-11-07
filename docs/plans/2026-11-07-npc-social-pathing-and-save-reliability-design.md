# NPC Social Pathing and Save Reliability Design

## Goals

- NPCs must not pass through buildings, trees, ponds, props, the player, or each other.
- NPCs should travel between meaningful village destinations instead of circling a small rectangle.
- Nearby NPCs may stop, face one another, exchange short speech bubbles, and then continue independently.
- Night schedules still return NPCs to their homes.
- Current and legacy saves must load without replacing money, livestock, inventory, or farm progress with new-game defaults.

## Chosen Approach

Use a small waypoint graph for the handcrafted village, combined with collider-sized movement casts. This is more reliable for the current map than random straight-line movement and much lighter than introducing a navigation package or a custom full-grid A* system. Each NPC receives a set of named destinations and follows linked waypoints between them. If a segment becomes blocked, the mover selects a different linked waypoint; a progress watchdog abandons a route when the actor has not advanced.

NPCs and animals receive a kinematic `Rigidbody2D` and a solid feet collider. Interaction remains available from the same object because the player's proximity query detects both trigger and solid colliders. Movement uses the rigidbody during fixed updates and shape casts before each step, so the actor's width is considered instead of testing only a zero-width ray.

## Social Behavior

An `NpcSocialAgent` periodically searches for another idle NPC nearby. A deterministic coordinator grants one conversation request so two agents cannot start competing conversations. Both pause, face each other, and alternate one or two short context-free village lines in world-space speech bubbles. A cooldown prevents the same pair from immediately talking again. Player interaction takes priority and cancels an NPC conversation cleanly.

## Save Reliability

`GameSaveData.CurrentVersion` is the single version source. The loader accepts supported versions from 1 through the current version, then `GameSaveDataNormalizer` migrates them in memory. Unknown future versions are rejected without overwriting the file. Save tests use an isolated temporary path and verify that `100G`, two chickens, and one cow survive a round trip. WebGL persistent-path autosync remains enabled.

## Verification

- EditMode tests cover supported save versions, future-version rejection, waypoint selection, stuck-route recovery, and conversation cooldown rules.
- PlayMode tests confirm NPCs move across more than their old local rectangle, do not intersect solid scenery, and can complete a social exchange.
- The WebGL build is run in a browser with console capture and screenshots of village movement and speech bubbles.
