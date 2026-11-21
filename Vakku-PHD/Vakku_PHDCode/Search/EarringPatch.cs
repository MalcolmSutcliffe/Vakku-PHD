using System.Text.Json;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace Vakku_PHD.Vakku_PHDCode.Search;

/// <summary>The Whispering Earring plays its opening cards with the search instead of left to right. Any failure plays the vanilla card.</summary>
[HarmonyPatch(typeof(WhisperingEarring), nameof(WhisperingEarring.AfterAutoPrePlayPhaseEnteredLate))]
public static class EarringPlayPatch
{
    private const int MaxCards = 13;

    public static bool Prefix(WhisperingEarring __instance, PlayerChoiceContext choiceContext, Player player, ref Task __result)
    {
        __result = Run(__instance, choiceContext, player);
        return false;
    }

    private static async Task Run(WhisperingEarring earring, PlayerChoiceContext choiceContext, Player player)
    {
        if (player != earring.Owner) return;
        var settings = ModSettings.Current;
        var combatState = player.Creature.CombatState;
        if (combatState == null) return;
        if (!settings.EveryTurn && earring.Owner.PlayerCombatState.TurnNumber > 1) return;

        earring.Flash();
        int startTurnAtEntry = earring.Owner.PlayerCombatState.TurnNumber;
        LastFight.Begin();
        int cardsPlayed = 0;
        bool hitCap;
        using (CardSelectCmd.PushSelector(new VakuuCardSelector()))
        {
            int startTurn = earring.Owner.PlayerCombatState.TurnNumber;
            bool searchWorks = true;
            for (; cardsPlayed < MaxCards; cardsPlayed++)
            {
                if (CombatManager.Instance.IsOverOrEnding) break;
                if (CombatManager.Instance.IsPlayerReadyToEndTurn(player)) break;
                if (earring.Owner.PlayerCombatState.TurnNumber != startTurn) break;
                var pile = PileType.Hand.GetPile(earring.Owner);
                if (!pile.Cards.Any(c => c.CanPlay())) break;

                CardModel? card = null;
                Creature? target = null;
                bool endTurn = false;
                if (searchWorks)
                {
                    try
                    {
                        (card, target, endTurn) = await Choose(player, settings);
                        if (settings.Mode == "observe")
                        {
                            MainFile.Logger.Info($"[observe] search would play {card?.Id.Entry ?? "end turn"}");
                            card = null; endTurn = false;
                        }
                    }
                    catch (Exception e)
                    {
                        MainFile.Logger.Warn($"search failed, vanilla plays this card: {e.Message}");
                        if (e is InvalidOperationException) searchWorks = false;
                    }
                }
                if (endTurn) break;
                if (CombatManager.Instance.IsOverOrEnding) break;     // the search took time: the fight may have moved on
                if (card == null)
                {
                    card = pile.Cards.FirstOrDefault(c => c.CanPlay());
                    if (card == null) break;
                    target = VanillaTarget(earring, card, combatState);
                }
                await card.SpendResources();
                await CardCmd.AutoPlay(choiceContext, card, target, AutoPlayType.Default, skipXCapture: true);
            }
            hitCap = cardsPlayed >= MaxCards;
        }
        if (cardsPlayed > 0)
        {
            var line = hitCap ? new LocString("relics", "WHISPERING_EARRING.warning") : new LocString("relics", "WHISPERING_EARRING.approval");
            TalkCmd.Play(line, earring.Owner.Creature, VfxColor.Purple);
        }
        if (settings.EveryTurn && settings.Mode == "play" && !CombatManager.Instance.IsOverOrEnding
            && earring.Owner.PlayerCombatState.TurnNumber == startTurnAtEntry)
        {
            MainFile.Logger.Info("every turn: ending the turn");
            PlayerCmd.EndTurn(player, canBackOut: false);
        }
    }

    /// <summary>Asks the sidecar for the next card. A null card with endTurn false means "no usable answer".</summary>
    private static async Task<(CardModel?, Creature?, bool)> Choose(Player player, ModSettings settings)
    {
        var state = CombatSnapshot.Build(player);
        var clock = System.Diagnostics.Stopwatch.StartNew();
        var reply = await SidecarClient.Search(state, settings);
        long ms = clock.ElapsedMilliseconds;
        LastFight.Record(state, reply.Raw);
        foreach (var warning in reply.Warnings) MainFile.Logger.Warn($"[sidecar] {warning}");

        string kind = reply.Action.GetProperty("action").GetString() ?? "";
        var handNow = PileType.Hand.GetPile(player).Cards.Select(c => c.Id.Entry);
        int sims = reply.Stats.ValueKind == JsonValueKind.Object && reply.Stats.TryGetProperty("num_sims", out var ns) && ns.ValueKind == JsonValueKind.Number ? ns.GetInt32() : -1;
        MainFile.Logger.Info($"search ({ms} ms, {sims} sims): {reply.Action} hand=[{string.Join(", ", handNow)}] energy={player.PlayerCombatState.Energy} {Summary(reply.Stats)}");
        if (kind == "end_turn") return (null, null, true);
        if (kind != "play_card") throw new InvalidDataException($"search answered {kind}; only cards are played here");

        int index = reply.Action.GetProperty("card_index").GetInt32();
        var hand = PileType.Hand.GetPile(player).Cards;
        if (index < 0 || index >= hand.Count) throw new InvalidDataException($"card_index {index} is not in the hand");
        var card = hand[index];
        if (!card.CanPlay()) throw new InvalidDataException($"{card.Id.Entry} cannot be played now");

        Creature? target = null;
        if (card.TargetType == TargetType.AnyEnemy)
        {
            var enemies = CombatSnapshot.LivingEnemies(player);
            int t = reply.Action.TryGetProperty("target", out var tj) && tj.ValueKind == JsonValueKind.Number ? tj.GetInt32() : -1;
            if (t < 0 || t >= enemies.Count) throw new InvalidDataException($"target {t} is not a living enemy");
            target = enemies[t];
        }
        else if (card.TargetType == TargetType.AnyPlayer)
            target = player.Creature;
        else if (card.TargetType == TargetType.AnyAlly)
            target = VanillaTarget(null, card, player.Creature.CombatState!, player);
        return (card, target, false);
    }

    private static string Summary(JsonElement stats)
    {
        if (stats.ValueKind != JsonValueKind.Object) return "";
        string Get(string n) => stats.TryGetProperty(n, out var v) ? v.ToString() : "-";
        bool paired = stats.TryGetProperty("paired", out var p) && p.ValueKind == JsonValueKind.Object;
        bool order = stats.TryGetProperty("order", out var o) && o.ValueKind == JsonValueKind.Object;
        return $"exit={Get("exit_reason")} paired={(paired ? "yes" : "no")} order={(order ? "yes" : "no")}";
    }

    private static Creature? VanillaTarget(WhisperingEarring? earring, CardModel card, ICombatState combat, Player? owner = null)
    {
        var self = earring?.Owner ?? owner!;
        return card.TargetType switch
        {
            TargetType.AnyEnemy => combat.HittableEnemies.FirstOrDefault(),
            TargetType.AnyAlly => self.RunState.Rng.CombatTargets.NextItem(combat.Allies.Where(c => c != null && c.IsAlive && c.IsPlayer && c != self.Creature)),
            TargetType.AnyPlayer => self.Creature,
            _ => null,
        };
    }
}

/// <summary>"Give extra energy" off removes the relic's +1.</summary>
[HarmonyPatch(typeof(WhisperingEarring), nameof(WhisperingEarring.ModifyMaxEnergy))]
public static class EarringEnergyPatch
{
    public static void Postfix(WhisperingEarring __instance, Player player, decimal amount, ref decimal __result)
    {
        if (player == __instance.Owner && !ModSettings.Current.ExtraEnergy)
            __result = amount;
    }
}
