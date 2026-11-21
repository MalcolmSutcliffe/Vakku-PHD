"""The sidecar the Whispering Earring mod talks to: the engine and the MCTS behind a small HTTP server on localhost.

  GET  /hello        {"sidecar", "version", "protocol", "engine", "python", "pid"}
  GET  /progress     {"searching": bool, "children": [{"action", "visits"}]}  the root moves of the search under way
  POST /search       {"state": <the bridge's combat state>, "settings": {name: value}, "time_budget": seconds, "use_potions": false}
                     -> {"action": <bridge action>, "stats": {...}, "warnings": [...]}   (a SearchConfig field or group field by name in `settings`;
                        a card the engine does not model is searched as a Wound and named in `warnings`)
  POST /fight_over   drops the kept search tree
  POST /shutdown     stops the server

Errors answer {"error": text, "kind": "bad_request" | "bad_settings" | "unknown_content" | "internal"} with the matching status, so the mod can
fall back to the vanilla Whispering Earring instead of guessing (an enemy or power the engine does not model is "unknown_content").

Runs on Python 3.9 (the Steam Linux runtime's): the bundle builder (sidecar/build.py) adds `from __future__ import annotations` to every module,
because the engine is written for newer Pythons. python -m sidecar.server --port N [--parent-pid P] [--version-file F]"""
from __future__ import annotations

import argparse
import json
import os
import sys
import threading
import time
import traceback
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer

PROTOCOL = 1
DEFAULT_TIME_BUDGET = 1.0
MAX_BODY = 8 * 1024 * 1024


class SearchError(Exception):
    def __init__(self, status : int, kind : str, message : str):
        super().__init__(message)
        self.status, self.kind = status, kind


class Searcher:
    """One search tree kept across the calls of a fight (the next search reuses the part of it the game went into)."""

    def __init__(self):
        from agents.sts2_MCTS.MCTS import sts2_MCTS
        self._mcts_class = sts2_MCTS
        self._mcts = sts2_MCTS()
        self._lock = threading.Lock()
        self.searching = False

    def progress(self) -> dict:
        """The root moves of the search in progress and the visits each has so far (read while the search runs: a snapshot, not exact)."""
        try:
            children = [{"action" : c.action, "visits" : c.visits} for c in list(self._mcts._root_node.children)] if self.searching and self._mcts._root_node is not None else []
        except (RuntimeError, AttributeError):      # the tree changed under the read: the next poll gets it
            children = []
        return {"searching" : self.searching, "children" : children}

    def fight_over(self):
        with self._lock:
            self._mcts = self._mcts_class()

    def search(self, state : dict, settings : dict, time_budget : float, use_potions : bool = False) -> dict:
        from agents.sts2_MCTS.params import SearchConfig
        from sts2_lite.combat import CombatSimulator, CombatState
        settings = dict(settings)
        prices = settings.pop("potion_prices", None)        # {"BLOOD_POTION": 18, ...}: HP each potion is worth (when potions are priced, not gated)
        try:
            config = SearchConfig(time_budget=time_budget).tuned(**settings)
            if prices:
                config = _with_potion_prices(config, prices)
        except (KeyError, ValueError, TypeError) as e:
            raise SearchError(400, "bad_settings", str(e))
        if not use_potions:         # the search cannot drink what it does not see
            state = _without_potions(state)
        try:
            combat_state = CombatState.from_dict(state)
        except (KeyError, ValueError, TypeError, AttributeError) as e:
            raise SearchError(422, "unknown_content", f"{type(e).__name__}: {e}")      # a card, relic or enemy the engine does not know (a game update, a mod)
        unknown_cards, unknown_enemies = unknown_content(combat_state)
        if unknown_enemies:
            raise SearchError(422, "unknown_content", "the engine does not model the enemy: " + ", ".join(unknown_enemies))
        warnings = [f"card not modelled, searched as a Wound: {name}" for name in unknown_cards]
        with self._lock:
            actions = CombatSimulator(combat_state).get_available_actions()
            if not actions:
                raise SearchError(422, "bad_request", "no action is available in this state")
            if len(actions) == 1:
                return {"action": actions[0], "stats": {"forced": True}, "warnings": warnings}
            self.searching = True
            try:
                action, stats = self._mcts.monte_carlo_tree_search(combat_state, config)
            finally:
                self.searching = False
            return {"action": action, "stats": stats, "warnings": warnings}


def unknown_content(combat_state) -> tuple[list[str], list[str]]:
    """(cards, enemies) of this fight the engine does not model. The engine tolerates both: an unknown card plays as a Wound, an unknown
    enemy does nothing. A Wound in the hand costs the search a little accuracy (a warning in the reply); an enemy that does nothing makes
    every line look safer than it is, so a state with one is refused and the mod plays vanilla."""
    from sts2_lite.card import CardId
    from sts2_lite.enemy_moves.registry import MOVE_TABLES
    cards, enemies = [], []
    player = combat_state.player
    for card in (*player.hand, *player.draw_pile, *player.discard_pile, *player.exhaust_pile):
        if card.card_id == CardId.NOT_IMPLEMENTED and card.name not in cards:
            cards.append(card.name)
    for enemy in combat_state.enemies:
        if enemy.enemy_id.rpartition("_")[0] not in MOVE_TABLES and enemy.enemy_id not in enemies:
            enemies.append(enemy.enemy_id)
    return cards, enemies


def _with_potion_prices(config, prices : dict):
    """The config with these potions priced (by game id, "BLOOD_POTION") over the built-in table. An id the engine does not know is refused by name."""
    import dataclasses
    from sts2_lite.potion import PotionId
    table = dict(config.reward.potion_bonus)
    for game_id, hp in prices.items():
        potion = PotionId.from_game_id(game_id)
        if potion is None:
            raise KeyError(f"unknown potion {game_id!r}")
        table[potion] = float(hp)
    return dataclasses.replace(config, reward=dataclasses.replace(config.reward, potion_bonus=table))


def _without_potions(state : dict) -> dict:
    """The state with every potion removed (the player appears twice: on its own and nested under "battle")."""
    state = dict(state)
    if isinstance(state.get("battle"), dict):
        state["battle"] = dict(state["battle"])
    for holder in (state, state.get("battle")):
        if isinstance(holder, dict) and isinstance(holder.get("player"), dict):
            holder["player"] = dict(holder["player"], potions=[])
    return state


def _json_safe(value):
    return json.loads(json.dumps(value, default=str))


def make_handler(searcher : Searcher, info : dict, stop : threading.Event):
    class Handler(BaseHTTPRequestHandler):
        def _send(self, status : int, body : dict):
            data = json.dumps(_json_safe(body)).encode()
            self.send_response(status)
            self.send_header("Content-Type", "application/json")
            self.send_header("Content-Length", str(len(data)))
            self.end_headers()
            self.wfile.write(data)

        def _body(self) -> dict:
            length = int(self.headers.get("Content-Length") or 0)
            if length > MAX_BODY:
                raise SearchError(413, "bad_request", "request body too large")
            try:
                body = json.loads(self.rfile.read(length) or b"{}")
            except ValueError as e:
                raise SearchError(400, "bad_request", f"not JSON: {e}")
            if not isinstance(body, dict):
                raise SearchError(400, "bad_request", "the body must be a JSON object")
            return body

        def do_GET(self):
            if self.path == "/hello":
                self._send(200, dict(info, pid=os.getpid(), python=sys.version.split()[0]))
            elif self.path == "/progress":
                self._send(200, searcher.progress())
            else:
                self._send(404, {"error": "not found", "kind": "bad_request"})

        def do_POST(self):
            try:
                if self.path == "/search":
                    body = self._body()
                    if not isinstance(body.get("state"), dict):
                        raise SearchError(400, "bad_request", "missing 'state'")
                    settings = body.get("settings") or {}
                    if not isinstance(settings, dict):
                        raise SearchError(400, "bad_settings", "'settings' must be an object")
                    budget = body.get("time_budget", DEFAULT_TIME_BUDGET)
                    if not isinstance(budget, (int, float)) or budget <= 0:
                        raise SearchError(400, "bad_settings", "'time_budget' must be a positive number of seconds")
                    self._send(200, searcher.search(body["state"], settings, float(budget), bool(body.get("use_potions", False))))
                elif self.path == "/fight_over":
                    searcher.fight_over()
                    self._send(200, {"ok": True})
                elif self.path == "/shutdown":
                    self._send(200, {"ok": True})
                    stop.set()
                else:
                    self._send(404, {"error": "not found", "kind": "bad_request"})
            except SearchError as e:
                self._send(e.status, {"error": str(e), "kind": e.kind})
            except Exception as e:       # the mod falls back to vanilla on any non-200
                traceback.print_exc()
                self._send(500, {"error": f"{type(e).__name__}: {e}", "kind": "internal"})

        def log_message(self, *args):
            pass

    return Handler


def parent_alive(pid : int, platform : str = sys.platform) -> bool:
    """Whether process `pid` still runs. On Windows os.kill(pid, 0) does not test: it terminates the process, so the handle is asked instead."""
    if platform == "win32":
        import ctypes
        kernel32 = ctypes.windll.kernel32
        kernel32.OpenProcess.restype = ctypes.c_void_p
        handle = kernel32.OpenProcess(0x00100000, 0, pid)       # SYNCHRONIZE
        if not handle:
            return False
        try:
            return kernel32.WaitForSingleObject(ctypes.c_void_p(handle), 0) == 0x102      # WAIT_TIMEOUT: not finished
        finally:
            kernel32.CloseHandle(ctypes.c_void_p(handle))
    try:
        os.kill(pid, 0)
    except OSError:
        return False
    return True


def watch_parent(pid : int, stop : threading.Event):
    """The sidecar must not outlive the game."""
    while not stop.is_set():
        if not parent_alive(pid):
            os._exit(0)
        time.sleep(2)


def read_info(version_file : str | None) -> dict:
    info = {"sidecar": "sts2-mcts", "version": "dev", "engine": "dev", "protocol": PROTOCOL}
    if version_file and os.path.exists(version_file):
        with open(version_file, encoding="utf-8") as f:
            info.update(json.load(f))
    return info


def main(argv=None):
    p = argparse.ArgumentParser()
    p.add_argument("--port", type=int, required=True)
    p.add_argument("--parent-pid", type=int, default=0)
    p.add_argument("--version-file", default=None)
    a = p.parse_args(argv)
    stop = threading.Event()
    if a.parent_pid:
        threading.Thread(target=watch_parent, args=(a.parent_pid, stop), daemon=True).start()
    server = ThreadingHTTPServer(("127.0.0.1", a.port), make_handler(Searcher(), read_info(a.version_file), stop))
    threading.Thread(target=lambda: (stop.wait(), server.shutdown()), daemon=True).start()
    print(f"sts2-mcts sidecar listening on 127.0.0.1:{a.port}", flush=True)
    server.serve_forever()


if __name__ == "__main__":
    main()
