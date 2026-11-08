# Starter quest chain

The prototype uses a three-step sequential quest chain built from systems already present in the game: harvest three crops, collect one egg, and pull three weeds. The HUD shows only the active quest so it remains readable, while the phone shows all three entries and their progress.

Quest progress and rewards live in `StarterQuestProgress`, separate from HUD rendering. Completion flags are serialized in `GameSaveData`, and rewards are granted only when a quest changes from incomplete to complete. Existing saves are normalized to version 3 with incomplete new quest flags.

Automatic tests verify quest order, rewards, duplicate-reward prevention, UI layout, and the complete playable scene.
