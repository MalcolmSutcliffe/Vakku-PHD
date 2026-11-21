# Vakku PHD

A Slay the Spire 2 mod: the **Whispering Earring** plays your opening cards with a Monte Carlo tree search instead of left to right.

## How it works

- `Vakku-PHD/` is the C# mod (Harmony patch on the Earring, settings screen via BaseLib, a bug-report button).
- `engine/` is the logic the mod runs on, as a bundled Python process (the "sidecar"):
  - `engine/sts2_lite/`: a headless re-implementation of Slay the Spire 2 combat (cards, enemies, relics, potions).
  - `engine/agents/sts2_MCTS/`: the tree search and how it scores a fight (HP kept, gold, max HP, deck changes).
  - `engine/sidecar/`: the local HTTP server the mod talks to, and the bundle builder.
- Only your machine is involved: the mod starts the sidecar on a free `127.0.0.1` port and stops it when the game exits. Nothing is uploaded; "Save report" writes a file you choose to send.

## Building

`tools/build_sidecar.sh` builds the sidecar bundle (on Windows it adds python.org's embeddable Python from `tools/downloads/`), then `dotnet build` in `Vakku-PHD/` installs the mod. `tools/sync_engine.sh` refreshes `engine/` from the engine repository.
