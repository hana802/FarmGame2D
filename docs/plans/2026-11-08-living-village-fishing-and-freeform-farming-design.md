# Living Village, Fishing, and Freeform Farming Design

## Goal

Extend the playable farming loop without invalidating existing saves. The player can create new plots on walkable grass or soil, animals can produce visibly young offspring, two additional villagers join the town, villagers talk more often to one another and occasionally greet the player, the ocean moves, fishing becomes a playable activity, and the village-square board offers quests that the player chooses.

## Farming

The existing 24 plots remain untouched for save compatibility. New plots are appended to `GameSaveData.farmPlots` with explicit world coordinates and a `freeform` marker. While holding the hoe, pressing E with no nearer interactable attempts to till the tile in front of the player. A tile is valid when ground exists, no road tile covers it, and no blocking collider, building, water, plot, or character occupies it. A runtime field controller creates plot views for saved and newly added plots.

## Animals

At the start of each new day, chickens and cows with at least two adults receive a deterministic breeding roll derived from the world seed and day. Chickens use a higher chance than cows and both have population caps. New animals are saved immediately and recorded as juveniles for three days. Juveniles render smaller, do not produce eggs or milk, and mature automatically. This deterministic roll prevents save reloads from changing the result.

## Villagers

Two villagers are added to the existing route network. The social agent receives a larger paired-line catalogue and keeps its existing face-and-pause behavior. When a villager is near the player, is not already conversing, and is off cooldown, a low-probability check can display a short greeting bubble and briefly face the player without taking control away.

## Ocean and Fishing

The ocean tilemap uses a small looping positional and color pulse for a gentle animated-water effect. Fishing spots are placed at the pond and accessible shoreline. Interacting while holding the fishing rod starts a short cast animation with a line and bobber, waits for a bite, and resolves one of several fish catches. Fish are inventory items, sellable, saved by the existing inventory serializer, and count toward fishing quests.

## Quest Board

The square board opens a dedicated selection panel. The player may accept one available quest at a time, track it in the HUD, and claim the reward automatically when its condition is reached. Existing harvest, egg, and weed progress is retained; fishing and mining jobs are added. Completed quests stay completed and cannot pay twice.

## Verification

Editor tests cover plot coordinate persistence, tilling rules, deterministic breeding, juvenile maturation, selectable quest progression, and fishing rewards. Play-mode smoke coverage checks the extra villagers, animated ocean component, fishing spots, quest-board interaction, and dynamic field controller. Existing save and cloud serialization tests remain green.
