using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace Vakku_PHD.Vakku_PHDCode.Search;

/// <summary>The combat state in the shape the bridge reports it and the sidecar reads it.</summary>
public static class CombatSnapshot
{
    public static Dictionary<string, object?> Build(Player player)
    {
        var combat = player.Creature.CombatState ?? throw new InvalidOperationException("no combat state");
        var run = player.RunState;
        var playerState = BridgeStateCopy.BuildPlayerState(player);

        var enemies = new List<Dictionary<string, object?>>();
        var counts = new Dictionary<string, int>();
        var reviving = new List<Dictionary<string, object?>>();
        foreach (var creature in combat.Enemies)
        {
            if (creature.IsAlive)
                enemies.Add(BridgeStateCopy.BuildEnemyState(creature, counts));
            else if (creature.Powers.Any(p => p.GetType().Name == "ReattachPower"))
                reviving.Add(new Dictionary<string, object?>
                {
                    ["entity_id"] = $"{creature.Monster?.Id.Entry ?? "unknown"}_0",
                    ["max_hp"] = creature.MaxHp,
                    ["move_id"] = (creature.Monster?.NextMove as MoveState)?.StateId,
                });
        }

        int act = run.CurrentActIndex + 1;
        return new Dictionary<string, object?>
        {
            ["state_type"] = "monster",
            ["battle"] = new Dictionary<string, object?>
            {
                ["round"] = combat.RoundNumber,
                ["turn"] = "player",
                ["player"] = playerState,
                ["enemies"] = enemies,
                ["reviving"] = reviving,
            },
            ["player"] = playerState,
            ["run"] = new Dictionary<string, object?> { ["act"] = act, ["floor"] = run.TotalFloor, ["ascension"] = run.AscensionLevel },
            ["context"] = new Dictionary<string, object?> { ["act"] = act, ["floor"] = run.TotalFloor, ["ascension"] = run.AscensionLevel },
        };
    }

    /// <summary>The living enemies in the order the state lists them (the search's target index points into this).</summary>
    public static List<MegaCrit.Sts2.Core.Entities.Creatures.Creature> LivingEnemies(Player player) =>
        (player.Creature.CombatState?.Enemies ?? Array.Empty<MegaCrit.Sts2.Core.Entities.Creatures.Creature>()).Where(c => c.IsAlive).ToList();
}
