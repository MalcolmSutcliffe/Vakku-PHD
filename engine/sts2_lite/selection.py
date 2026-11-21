"""Matching a choice made in the sim to the card list the game shows for it."""


def game_index(action : dict, cards : list[dict], skip=()) -> int | None:
    """Index in the game's card list of the card an action picks: the first one with the same id and
    upgrade state, since the sim's pile order need not match the game's; one with the same cost is preferred (a free copy
    and a full-price one of the same card look alike otherwise). `skip`: indices already picked
    on this screen (an "up to N" pick can take two identical cards). None if there is no such card."""
    same = [c for c in cards if c["index"] not in skip and c["id"] == action["card_id"] and c["is_upgraded"] == action["is_upgraded"]]
    if not same:
        return None
    cost = action.get("cost")
    exact = [c for c in same if cost is None or str(c.get("cost")) == cost]
    return (exact or same)[0]["index"]
