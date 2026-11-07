# Game Time Design

## Scope

The original first version added only a continuously advancing game clock. The playable prototype now connects sleeping, 28-day seasons, and save data to the same clock. Stamina and shop schedules remain out of scope.

## Rules

- A new game starts on day 1 at 06:00.
- Every 3 real-time seconds advances the clock by 1 game minute, matching the confirmed 20x speed.
- Passing 24:00 increments the day and wraps the clock to 00:00.
- The current day and time appear in the top-right corner during play mode.
- Sleeping advances to 06:00 on the next day and triggers an automatic save.

## Structure

`GameClock` owns framework-independent time calculations. `GameClockController` converts Unity frame time into game-time steps and exposes change notifications. `GameClockOverlay` displays the current value without owning any time logic. `GameTimeBootstrap` makes the system available in any scene without manual scene setup.

This separation lets future farming, chicken, shop, and sleep systems subscribe to the same clock without coupling their rules to the display.
