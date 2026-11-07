# Cross-Browser Cloud Save Design

## Goal

Let a player use the same farm save in different browsers without creating an account. The first browser creates an eight-character synchronization code and a six-digit PIN. Another browser links to the save by entering both values.

## Architecture

The Unity WebGL client keeps the existing local JSON save as an offline fallback. A `CloudSyncService` calls one Supabase Edge Function over HTTPS for three operations: create a cloud save, load a linked save, and update a linked save. The Edge Function is the only component allowed to access the database. The WebGL client never receives a database secret or direct table permission.

The Supabase project stores `sync_code`, a random salt, a PBKDF2-SHA256 PIN hash, the save JSON, an optimistic revision number, and timestamps. Row Level Security is enabled and no client policies are created. `anon` and `authenticated` table privileges are revoked. The Edge Function uses its server-only secret key to perform narrowly scoped queries.

## Security and Conflicts

Codes exclude ambiguous characters and are generated with a cryptographic random source. PINs are six random digits and are returned only when a save is created. PIN input is never logged. Requests validate action names, code and PIN formats, save schema version, and a strict payload-size limit. Failed and successful requests share IP- and code-based database rate limits to slow guessing. PIN comparison is constant-time.

Every cloud save has a revision. An update must provide the revision last loaded by that browser. If another browser saved first, the server returns a conflict instead of silently overwriting newer progress. The game then offers to load the newer cloud save or keep the local state and retry after the player chooses.

## User Flow

The phone save screen gains a cloud-save section. “새 동기화 만들기” uploads the current local save and displays the code and PIN. “기존 저장 연결” accepts a code and PIN, downloads the cloud save, normalizes it, writes it locally, and refreshes the world. Once linked, manual saves and existing automatic purchase/sale saves also upload to the cloud. Network failures leave the local save intact and show a retryable message.

## Verification

- Unit tests cover code/PIN validation, response parsing, local fallback, and revision conflicts.
- The live Edge Function is tested for create, load, invalid PIN, oversize payload, rate limiting, and stale revision rejection.
- Supabase security and performance advisors are clean after migration.
- WebGL browser testing confirms a save created in one browser context loads in a separate context.
