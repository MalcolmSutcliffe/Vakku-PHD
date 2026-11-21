using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using Vakku_PHD.Vakku_PHDCode.Sidecar;

namespace Vakku_PHD.Vakku_PHDCode;

//You're recommended but not required to keep all your code in this package and all your assets in the Vakku_PHD folder.
[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "Vakku_PHD"; //At the moment, this is used only for the Logger and harmony names.

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } = new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        var assembly = Assembly.GetExecutingAssembly();

        //If you want to use scripts defined in your mod for Godot scenes, uncomment the following line.
        //Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(assembly);
     
        Harmony harmony = new(ModId);

        harmony.PatchAll(assembly);

        _ = Task.Run(SmokeTestSidecar);
    }

    /// <summary>Temporary: proves a mod can launch a bundled process and talk to it. Replaced by the real sidecar manager.</summary>
    private static async Task SmokeTestSidecar()
    {
        try
        {
            using var sidecar = new SidecarProcess();
            string reply = await sidecar.StartAsync(TimeSpan.FromSeconds(10));
            Logger.Info($"SIDECAR SMOKE TEST OK on port {sidecar.Port}: {reply}");
            await Task.Delay(Timeout.Infinite);     // keep it alive for the life of the game; ProcessExit stops it
        }
        catch (Exception e)
        {
            Logger.Error($"SIDECAR SMOKE TEST FAILED: {e}");
        }
    }
}
