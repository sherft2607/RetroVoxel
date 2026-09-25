# Box art standoff

_Demo file pending — built from the plugin spec, not a saved canvas._

**Goal:** size a laser-cut standoff stack to a game's real box-art aspect ratio instead of
eyeballing dimensions.

## Components (wiring order)

1. [RA Game Info](../components/01-ingest/RA-Game-Info.md) (`GAME`)
2. [Laser Standoff Stack](../components/04-fabrication/Laser-Standoff-Stack.md) (`STACK`)

## Inputs to set

- `RA Game Info.Token` → your API key
- `RA Game Info.Game ID` → `1446`
- `Laser Standoff Stack.Aspect Ratio` → computed from `ImageBoxArt`'s pixel dimensions
- `Laser Standoff Stack.Stack Count` / `Panel Thickness` → to taste

## Expected result

A stack of flat, aspect-ratio-scaled panel Breps, evenly spaced along Z.

See [demos/README.md](../../demos/README.md#5-box-art-standoff) for the full manual build spec.
