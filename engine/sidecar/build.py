"""Builds the sidecar bundle the mod ships: the engine, the MCTS and the server, made to run on Python 3.9 (the Steam Linux runtime's).

    python -m sidecar.build OUT_DIR [PYTHON_EMBED_ZIP]

OUT_DIR gets:  sts2_lite/  agents/sts2_MCTS/  agents/deck_eval/card_profile.json  sidecar/server.py  sidecar.json  run  run.cmd  [python/ when the Windows embeddable zip is given]
Every copied module gets `from __future__ import annotations` (the engine relies on Python 3.14's lazy annotations: forward references in a
class body, `X | None` in a signature); nothing else about the code changes. Tests, caches and the campaign/run code are left out."""
from __future__ import annotations

import ast
import json
import shutil
import subprocess
import sys
import zipfile
from pathlib import Path

SCRIPTS = Path(__file__).resolve().parents[1]
PACKAGES = ["sts2_lite", "agents/sts2_MCTS"]
DATA_FILES = ["agents/deck_eval/card_profile.json"]
SKIP_DIRS = {"Tests", "__pycache__"}
FUTURE = "from __future__ import annotations\n"

LAUNCHER = """#!/bin/sh
# Starts the sidecar on the Python the game runs under (the Steam Linux runtime's python3).
# LD_PRELOAD is Steam's 32-bit overlay, which only prints a warning for every process started under the game.
cd "$(dirname "$0")" || exit 1
unset LD_PRELOAD
exec python3 -m sidecar.server --version-file sidecar.json "$@"
"""
LAUNCHER_CMD = """@echo off
rem Runs the Python shipped in python\\ (python.org's embeddable build); one on PATH is the fallback.
cd /d "%~dp0"
if exist python\\python.exe (
  python\\python.exe -m sidecar.server --version-file sidecar.json %*
) else (
  python -m sidecar.server --version-file sidecar.json %*
)
"""


def with_future_import(source : str) -> str:
    """`source` with `from __future__ import annotations` after the module docstring and any other __future__ imports (it must come first)."""
    if FUTURE.strip() in source:
        return source
    tree = ast.parse(source)
    lines = source.splitlines(keepends=True)
    end = 0
    for node in tree.body:
        is_doc = isinstance(node, ast.Expr) and isinstance(getattr(node, "value", None), ast.Constant) and isinstance(node.value.value, str)
        is_future = isinstance(node, ast.ImportFrom) and node.module == "__future__"
        if (node is tree.body[0] and is_doc) or is_future:
            end = node.end_lineno
        else:
            break
    return "".join(lines[:end]) + FUTURE + "".join(lines[end:])


def engine_version() -> str:
    try:
        out = subprocess.run(["git", "-C", str(SCRIPTS), "rev-parse", "--short", "HEAD"], capture_output=True, text=True, timeout=10)
        return out.stdout.strip() or "unknown"
    except (OSError, subprocess.SubprocessError):
        return "unknown"


def add_windows_python(out : Path, embeddable_zip : Path):
    """Unpacks python.org's Windows embeddable Python into out/python/. Its python3xx._pth replaces the search path, so the bundle's
    own folder (one up from python/) is added to it: without that `-m sidecar.server` would not be found."""
    target = out / "python"
    with zipfile.ZipFile(embeddable_zip) as z:
        z.extractall(target)
    pths = list(target.glob("python*._pth"))
    if len(pths) != 1:
        raise SystemExit(f"{embeddable_zip} is not an embeddable Python (expected one python3xx._pth, found {len(pths)})")
    lines = [line for line in pths[0].read_text().splitlines() if line.strip() and not line.startswith("#") and line.strip() != "import site"]
    pths[0].write_text("\n".join(lines + [p for p in (".", "..") if p not in lines]) + "\n")


def build(out : Path, version : str = "1.0.0", windows_python : Path | None = None) -> list[Path]:
    if out.exists():
        if any(out.iterdir()) and not (out / "sidecar.json").exists():
            raise SystemExit(f"{out} exists and is not a sidecar bundle (no sidecar.json): refusing to delete it")
        shutil.rmtree(out)
    written = []
    sources = [SCRIPTS / pkg for pkg in PACKAGES] + [SCRIPTS / "sidecar" / "server.py"]
    for src in sources:
        files = [src] if src.is_file() else sorted(p for p in src.rglob("*.py") if not SKIP_DIRS & set(p.relative_to(src).parts))
        for f in files:
            dest = out / f.relative_to(SCRIPTS)
            dest.parent.mkdir(parents=True, exist_ok=True)
            dest.write_text(with_future_import(f.read_text(encoding="utf-8")), encoding="utf-8")
            written.append(dest)
    for pkg_dir in {p.parent for p in written}:       # `agents` and `sidecar` are real packages in the bundle
        init = pkg_dir / "__init__.py"
        if not init.exists():
            init.write_text("", encoding="utf-8")
    (out / "agents").mkdir(exist_ok=True)
    (out / "agents" / "__init__.py").touch()
    for name in DATA_FILES:
        dest = out / name
        dest.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(SCRIPTS / name, dest)
    (out / "sidecar.json").write_text(json.dumps({"version": version, "engine": engine_version()}), encoding="utf-8")
    run = out / "run"
    run.write_text(LAUNCHER, encoding="utf-8")
    run.chmod(0o755)
    (out / "run.cmd").write_text(LAUNCHER_CMD, encoding="utf-8")
    if windows_python is not None:
        add_windows_python(out, windows_python)
    return written


if __name__ == "__main__":
    if len(sys.argv) not in (2, 3):
        sys.exit(__doc__)
    files = build(Path(sys.argv[1]), windows_python=Path(sys.argv[2]) if len(sys.argv) == 3 else None)
    print(f"built {len(files)} modules into {sys.argv[1]}")
