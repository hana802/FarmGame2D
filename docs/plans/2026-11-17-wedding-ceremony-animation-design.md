# Animated wedding ceremony

## Player flow

Accepting a 100-friendship proposal opens a full-screen wedding ceremony instead of another text page. An unfinished ceremony is available again from the spouse's next interaction. A completed ceremony is saved and can be replayed from the spouse's interaction menu.

## Scene

Reuse the existing pixel character sprites. Render an outdoor flower arch, an aisle, and all other village residents as guests. Overlay wedding dress or suit shapes on the two characters. The couple walks in from opposite sides, meets at the altar, exchanges vows, and receives a short chime and animated flower petals while the guests clap. Captions advance automatically. The player finishes the ceremony after the animation or leaves early without recording completion.

## Integration and checks

Keep marriage eligibility and persistence in `BondEventService`. The ceremony component owns visual timing and completion; a separate renderer owns the scene art. The existing NPC interaction modal state also blocks player input during playback. Run Unity tests and rebuild WebGL after the interaction is connected.
