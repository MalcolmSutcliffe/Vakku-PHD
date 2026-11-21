using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;

namespace Vakku_PHD.Vakku_PHDCode.Search;

/// <summary>
/// With "play all turns" on, the Earring is the one playing the fight, so card choices that come up outside its loop (an enemy's curse
/// choice such as the Knowledge Demon's, a power that asks for a card between turns) are answered the way the loop answers them
/// instead of waiting for a click that nobody is there to make.
/// </summary>
[HarmonyPatch(typeof(CardSelectCmd), nameof(CardSelectCmd.Selector), MethodType.Getter)]
public static class AutoChoosePatch
{
    private static readonly VakuuCardSelector Fallback = new();

    public static void Postfix(ref MegaCrit.Sts2.Core.TestSupport.ICardSelector? __result)
    {
        if (__result != null) return;
        try
        {
            if (!ModSettings.Current.EveryTurn || CombatManager.Instance?.IsInProgress != true) return;
            var runState = RunManager.Instance.DebugOnlyGetState();
            var me = runState == null ? null : LocalContext.GetMe(runState);
            if (me != null && me.Relics.Any(r => r is WhisperingEarring)) __result = Fallback;
        }
        catch { /* the player answers it */ }
    }
}
