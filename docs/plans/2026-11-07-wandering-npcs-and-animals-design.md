# Wandering NPCs and purchased animals

## Goal

Make the village feel alive by letting NPCs follow small daily routes and by displaying every purchased chicken and cow as an individual roaming animal.

## Components

`WanderingMover` owns movement only. It alternates between waiting and selecting a destination inside a configured rectangular roaming area, rejects paths blocked by solid colliders, gently separates members of the same group, and can move to a night anchor after 20:00. Interactable components pause their mover during dialogue, feeding, or collection.

`FarmAnimalHerd` mirrors saved ownership into scene objects. It listens to `GameSession.StateChanged`, spawns missing animals immediately after a purchase, and gives every animal a stable name, distinct starting position, collider, interaction, movement speed, and roaming area. The first objects retain the existing `Chicken` and `Cow` names for compatibility.

`WorldDepthSorter` calculates sprite order from world Y. It is attached to the player and all moving characters, while static trees, buildings, props, and rocks receive the same depth formula when created. Ground-like sprites remain in their original low layers. Held items render immediately in front of or behind their owner.

## Safety and verification

NPC routes stay near their normal dialogue locations and animals remain inside their pens. Movement tests cover area containment and depth ordering. Play-mode tests confirm that saved ownership equals the number of visible animal objects, that all actors have movement and depth sorting, and that movement never leaves configured bounds. Existing farming, interaction, and world-overlap tests continue to run unchanged.
