"""An agent for finding engine mismatches fast: every choice is random. In a fight it plays a random legal action (End Turn only when nothing else is legal), and it picks
random cards and options on every other screen, so that no niche interaction is skipped because a policy never plays that way."""
from sts2_lite.combat import CombatState, CombatSimulator

from agents.base_agent import BaseAgent
from agents.decision import Decision, Option


class FastAgent(BaseAgent):
    def __init__(self):
        super().__init__()
        self.name = "random choices"

    def get_combat_action(self, combat_state : CombatState, time_budget : float | None = None):
        actions = CombatSimulator(combat_state).get_available_actions()
        playable = [a for a in actions if a["action"] != "end_turn"] or actions
        return self._rng.choice(playable)

    def _get_map_decision(self, decision : Decision) -> Option:
        return self._rng_choice(decision.options)

    def _get_shop_decision(self, decision : Decision) -> Option:
        return self._rng_choice(decision.options)

    def _get_rest_decision(self, decision : Decision) -> Option:
        return self._rng_choice(decision.options)

    def _card_to_select(self, decision : Decision, picks : list[Option]) -> Option:
        return self._rng_choice(picks)      # the base class keeps count of how many cards a screen wants and when to confirm

    def _get_relic_decision(self, decision : Decision) -> Option:
        return self._rng_choice(decision.options)

    def _get_treasure_decision(self, decision : Decision) -> Option:
        return self._rng_choice(decision.options)
