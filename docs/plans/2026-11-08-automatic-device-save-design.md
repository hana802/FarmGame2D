# Automatic same-device save design

## Goal

When the WebGL game is opened in another browser on the same computer, find and load the existing cloud save without asking the player to copy a code or PIN. Keep the existing code/PIN flow as recovery when automatic recognition fails.

## Design

The Unity client builds a coarse device signature from stable, non-personal runtime properties: operating-system family, CPU core count, graphics device, system memory bucket, screen resolution, language, and time zone. The Edge Function validates the bounded signature and derives an opaque device key with a server-side HMAC. Only that opaque key is stored; raw hardware properties and IP addresses are not stored in the save table.

On startup the client sends `device-connect` with its local save. The server atomically finds the matching device save or creates one. An existing save is returned and replaces the new browser's default save; a new save returns an internal recovery code and PIN. Normal local saves continue immediately, followed by optimistic cloud updates. Manual code/PIN linking remains available under an advanced recovery section.

Because browsers deliberately do not share local storage, this is probabilistic device recognition rather than an operating-system identity. A changed GPU, privacy mode, remote desktop session, or display configuration can produce a new signature. Recovery code/PIN handles that case. A shared computer intentionally shares the game, matching the requested same-device behavior.

## Security and verification

The signature is length-limited and character-validated. The server applies existing IP and device-key rate limits, uses parameterized Supabase queries, returns generic errors, and never exposes service credentials. RLS remains enabled with no direct browser table policy. Tests cover signature stability, automatic create/load, wrong-input rejection, optimistic conflicts, Unity EditMode/PlayMode, WebGL build, and two isolated browser contexts.
