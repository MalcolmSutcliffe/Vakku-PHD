using System.Text.Json;

namespace Vakku_PHD.Vakku_PHDCode.Search;

/// <summary>last_fight.jsonl next to the mod's DLL: every state the search was given and its full answer, for the latest Earring turn. A bug report is this file.</summary>
public static class LastFight
{
    private static string Path_ => System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(LastFight).Assembly.Location)!, "last_fight.jsonl");

    public static void Begin()
    {
        try { File.WriteAllText(Path_, ""); } catch { }
    }

    public static void Record(Dictionary<string, object?> state, string reply)
    {
        try
        {
            File.AppendAllText(Path_, "{\"state\":" + JsonSerializer.Serialize(state) + ",\"reply\":" + reply + ",\"settings\":" + JsonSerializer.Serialize(new { mode = ModSettings.Current.Mode, extra_energy = ModSettings.Current.ExtraEnergy, every_turn = ModSettings.Current.EveryTurn, search_seconds = ModSettings.Current.SearchSeconds, search = ModSettings.Current.SearchSettings() }) + "}\n");
        }
        catch { }
    }
}
