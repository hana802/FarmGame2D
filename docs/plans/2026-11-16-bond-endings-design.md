# Bond endings and companion memories

## Existing gap

Friendship reaches 100 but no event is connected to that value. Dog affection grows to 999 while only 1 and 5 have distinct reactions. Existing saves may already exceed the new thresholds.

## Player flow

- At 100 friendship, the next interaction with that villager opens a short, personal ending scene. A gift that reaches 100 opens it immediately. Each villager has their own lines and portrait.
- After the friendship scene, a proposal opens. The player can accept or postpone. Accepting records one spouse and immediately opens a wedding scene; postponing offers the proposal again on the next interaction. If several villagers already have 100 friendship, the first accepted proposal determines the spouse. Other friendship scenes remain available without further proposals.
- If the wedding scene is closed before finishing, it reopens at the next interaction with the spouse. Once finished, the spouse is labeled in the interaction panel and both the friendship and wedding scenes can be replayed. Existing 100-point saves receive the same flow.
- Finishing the scene records it in the save. Closing early leaves it available for the next interaction. After completion, a replay button remains in that villager's interaction panel.
- At dog affection 10, petting opens a companion memory. At 50, petting opens a final best-friend memory. Existing high-affection saves open the final event on the next pet. Finishing the final event also clears the earlier milestone.
- The event view uses the game's existing dialogue overlay and pauses player movement while open.

## Persistence and checks

Add optional seen flags and a spouse NPC id to existing save records. Tests cover locked, newly unlocked, already-maxed, one-spouse exclusivity, completion, and save round trips. Build and run the WebGL game after the event flow passes Unity tests.
