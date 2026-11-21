"""Reads a bug report saved by the mod ("Save report" in its settings) and replays it against the engine.

    python -m sidecar.replay_report REPORT.json [--search SECONDS] [--fixtures DIR]

Prints, for each state the mod searched: the hand, what the mod played, and whether the engine could load the state (unknown cards, enemies or powers
show up here, with the errors the mod logged). --search searches each state again with the report's settings. --fixtures DIR writes each state as
DIR/<report name>_<n>.json, ready to be used as a test fixture."""
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path


def load_report(path : Path) -> dict:
    report = json.loads(path.read_text(encoding="utf-8"))
    if report.get("report_version") != 1:
        raise SystemExit(f"{path}: not a report this tool reads (report_version {report.get('report_version')!r})")
    return report


def replay(report : dict, search_seconds : float | None = None, fixtures : Path | None = None, name : str = "report") -> list[str]:
    from sidecar.server import Searcher, SearchError
    lines = [f"mod {report.get('mod_version')}  sidecar {report.get('sidecar')}  os {report.get('os')}"]
    searcher = Searcher()
    n = 0
    for row in report.get("fight", []):
        if "event" in row:
            lines.append(f"EVENT {row['event']}")
            continue
        n += 1
        state = row["state"]
        hand = [c["id"] for c in state["player"].get("hand", [])]
        enemies = [e["entity_id"] for e in state["battle"]["enemies"]]
        played = row.get("reply", {}).get("action")
        text = f"#{n} hand={hand} enemies={enemies} energy={state['player'].get('energy')} played={played}"
        if row.get("reply", {}).get("warnings"):
            text += f" warnings={row['reply']['warnings']}"
        if search_seconds is not None:
            settings = row.get("settings", {}).get("search", {})
            try:
                again = searcher.search(state, dict(settings, check_workers=1), search_seconds, bool(row.get("settings", {}).get("use_potions")))
                text += f" | now={again['action']}"
            except SearchError as e:
                text += f" | ENGINE REFUSES: {e.kind}: {e}"
        lines.append(text)
        if fixtures is not None:
            fixtures.mkdir(parents=True, exist_ok=True)
            (fixtures / f"{name}_{n}.json").write_text(json.dumps(state), encoding="utf-8")
    return lines


def main(argv=None):
    p = argparse.ArgumentParser(description=__doc__.split("\n")[0])
    p.add_argument("report", type=Path)
    p.add_argument("--search", type=float, default=None)
    p.add_argument("--fixtures", type=Path, default=None)
    args = p.parse_args(argv)
    for line in replay(load_report(args.report), args.search, args.fixtures, args.report.stem):
        print(line)


if __name__ == "__main__":
    sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
    main()
