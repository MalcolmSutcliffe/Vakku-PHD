#!/bin/sh
# Builds the sidecar bundle (the engine + MCTS + server, made to run on Steam's Python 3.9) into Vakku-PHD/sidecar/.
# The engine lives in the SlayTheSpireII-Agent repo; point SIDECAR_SRC at its scripts/ folder.
set -eu
HERE="$(cd "$(dirname "$0")/.." && pwd)"
SRC="${SIDECAR_SRC:-/home/user/Documents/Malcolm_Stuff/Projects/SlayTheSpireII-Agent-autorunner/scripts}"
[ -f "$SRC/sidecar/build.py" ] || { echo "SIDECAR_SRC=$SRC has no sidecar/build.py" >&2; exit 1; }
cd "$SRC" && python3 -m sidecar.build "$HERE/Vakku-PHD/sidecar"
