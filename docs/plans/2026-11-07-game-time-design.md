# Game Time Design

## Scope

The first version adds only a continuously advancing game clock. It does not add stamina, sleeping, shop schedules, seasons, or save data.

## Rules

- A new game starts on day 1 at 06:00.
- Every 7 real-time seconds advances the clock by 10 game minutes.
- Passing 24:00 increments the day and wraps the clock to 00:00.
- The current day and time appear in the top-right corner during play mode.

## Structure

`GameClock` owns framework-independent time calculations. `GameClockController` converts Unity frame time into game-time steps and exposes change notifications. `GameClockOverlay` displays the current value without owning any time logic. `GameTimeBootstrap` makes the system available in any scene without manual scene setup.

This separation lets future farming, chicken, shop, and sleep systems subscribe to the same clock without coupling their rules to the display.
