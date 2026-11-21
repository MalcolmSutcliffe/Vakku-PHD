#!/bin/sh
# Builds the sidecar bundle (the engine + MCTS + server, made to run on Steam's Python 3.9) into Vakku-PHD/sidecar/.
# The engine lives in the SlayTheSpireII-Agent repo; point SIDECAR_SRC at its scripts/ folder.
# Windows has no Python of its own: python.org's embeddable build is added from tools/downloads/ (checked against the hash below).
set -eu
HERE="$(cd "$(dirname "$0")/.." && pwd)"
SRC="${SIDECAR_SRC:-/home/user/Documents/Malcolm_Stuff/Projects/SlayTheSpireII-Agent-autorunner/scripts}"
[ -f "$SRC/sidecar/build.py" ] || { echo "SIDECAR_SRC=$SRC has no sidecar/build.py" >&2; exit 1; }
EMBED="$HERE/tools/downloads/python-3.12.10-embed-amd64.zip"
EMBED_SHA256="4acbed6dd1c744b0376e3b1cf57ce906f9dc9e95e68824584c8099a63025a3c3"
if [ -f "$EMBED" ]; then
  echo "$EMBED_SHA256  $EMBED" | sha256sum -c - >/dev/null || { echo "$EMBED does not match the recorded hash" >&2; exit 1; }
  cd "$SRC" && python3 -m sidecar.build "$HERE/Vakku-PHD/sidecar" "$EMBED"
else
  echo "note: no Windows Python in tools/downloads; the bundle will only run on Linux" >&2
  cd "$SRC" && python3 -m sidecar.build "$HERE/Vakku-PHD/sidecar"
fi
