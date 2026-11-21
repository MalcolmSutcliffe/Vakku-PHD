#!/bin/sh
# Copies the engine the sidecar runs (sts2_lite, agents, sidecar; not the tests, which need the engine repository's captured game data) into engine/ so this repository holds all of the logic the mod runs on.
# The source is the engine repository's scripts/ folder:  SIDECAR_SRC=/path/to/scripts tools/sync_engine.sh
set -eu
HERE="$(cd "$(dirname "$0")/.." && pwd)"
SRC="${SIDECAR_SRC:?set SIDECAR_SRC to the engine repository's scripts/ folder}"
[ -f "$SRC/sidecar/build.py" ] || { echo "SIDECAR_SRC=$SRC has no sidecar/build.py" >&2; exit 1; }
for dir in sts2_lite agents sidecar; do
  mkdir -p "$HERE/engine/$dir"
  rsync -a --delete --delete-excluded --exclude 'Tests' --exclude '__pycache__' --exclude '*.pyc' --exclude '.pytest_cache' "$SRC/$dir/" "$HERE/engine/$dir/"
done
echo "engine/ now matches $SRC"; du -sh "$HERE/engine"
