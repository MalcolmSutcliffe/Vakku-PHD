using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Runs;

namespace Vakku_PHD.Vakku_PHDCode.Search;

/// <summary>A "Vakku PhD: Save fight report" button on the combat screen, shown only to a player holding the Whispering Earring.</summary>
[HarmonyPatch(typeof(NCombatUi), nameof(NCombatUi._Ready))]
public static class FightScreenButton
{
    private const string NodeName = "VakkuSaveFightButton";

    public static void Postfix(NCombatUi __instance)
    {
        try
        {
            if (!VakkuConfig.ShowFightButton) return;
            var runState = RunManager.Instance.DebugOnlyGetState();
            var me = runState == null ? null : LocalContext.GetMe(runState);
            if (me == null || !me.Relics.Any(r => r is WhisperingEarring)) return;
            if (__instance.GetNodeOrNull(NodeName) != null) return;

            var button = new Button
            {
                Name = NodeName,
                Text = "Vakku PhD: Save fight report",
                FocusMode = Control.FocusModeEnum.None,
                TooltipText = "Saves the last fight with the Earring to a file you can send with a bug report. Nothing is uploaded.",
                CustomMinimumSize = new Vector2(280, 38),
            };
            button.SetAnchorsPreset(Control.LayoutPreset.TopRight);
            button.OffsetLeft = -304;
            button.OffsetRight = -24;
            button.OffsetTop = 194;
            button.OffsetBottom = 232;
            button.Pressed += () => OnPressed(button);
            __instance.AddChild(button);
        }
        catch (Exception e)
        {
            MainFile.Logger.Warn($"fight screen button not added: {e.Message}");
        }
    }

    private static async void OnPressed(Button button)
    {
        string? path = BugReport.Save();
        button.Text = path == null ? "Vakku PhD: No fight yet" : "Vakku PhD: Saved!";
        await button.ToSignal(button.GetTree().CreateTimer(2.0), SceneTreeTimer.SignalName.Timeout);
        if (GodotObject.IsInstanceValid(button)) button.Text = "Vakku PhD: Save fight report";
    }
}
