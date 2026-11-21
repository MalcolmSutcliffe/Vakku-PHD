#!/bin/sh
# Assembles the Steam Workshop upload workspace from the installed mod (build it first: dotnet build in Vakku-PHD/).
# Usage: tools/stage_workshop.sh [workspace-dir]   (default: workshop-test/)
set -eu
HERE="$(cd "$(dirname "$0")/.." && pwd)"
WS="${1:-$HERE/workshop-test}"
GAME="${STS2_GAME_DIR:-/home/user/snap/steam/common/.local/share/Steam/steamapps/common/Slay the Spire 2}"
SRC="$GAME/mods/Vakku-PHD"
[ -f "$SRC/Vakku_PHD.dll" ] || { echo "build the mod first: $SRC/Vakku_PHD.dll is missing" >&2; exit 1; }
mkdir -p "$WS/content"
rm -rf "$WS/content"/*
cp "$SRC/Vakku_PHD.dll" "$SRC/Vakku_PHD.json" "$WS/content/"
cp -r "$SRC/sidecar" "$WS/content/sidecar"
chmod -x "$WS/content/sidecar/run"        # a Workshop download may not keep the executable bit; the mod has to cope
cp "$HERE/Vakku-PHD/Vakku_PHD/mod_image.png" "$WS/image.png"
[ -f "$WS/workshop.json" ] || cat > "$WS/workshop.json" <<'J'
{
  "title": "Vakku PHD (smoke test)",
  "description": "Test upload for a Whispering Earring mod. Do not subscribe.",
  "visibility": "unlisted",
  "changeNote": "smoke test",
  "tags": [],
  "dependencies": [],
  "contentDescriptors": []
}
J
echo "staged: $WS"; find "$WS" -type f | sort
