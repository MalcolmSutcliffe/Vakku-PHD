using System.Text.Json;

namespace Vakku_PHD.Vakku_PHDCode.Search;

/// <summary>last_fight.jsonl next to the mod's DLL: every state the search was given and its answer for the latest fight with the Earring, and what went wrong. A bug report is this file plus BugReport's header.</summary>
public static class LastFight
{
    private static object? _combat;
    private static string Path_ => System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(LastFight).Assembly.Location)!, "last_fight.jsonl");

    /// <summary>A new fight starts a new file; a later turn of the same fight adds to it.</summary>
    public static void Begin(object combat)
    {
        if (ReferenceEquals(combat, _combat)) return;
        _combat = combat;
        try { File.WriteAllText(Path_, ""); } catch { }
    }

    public static void Record(Dictionary<string, object?> state, string reply)
    {
        Append("{\"state\":" + JsonSerializer.Serialize(state) + ",\"reply\":" + reply + ",\"settings\":" + JsonSerializer.Serialize(Snapshot()) + "}");
    }

    /// <summary>Something that went wrong (a search that failed, an action the game would not take): the bug report shows it next to the states.</summary>
    public static void Event(string what)
    {
        Append("{\"event\":" + JsonSerializer.Serialize(what) + ",\"at\":" + JsonSerializer.Serialize(DateTime.UtcNow.ToString("o")) + "}");
    }

    public static object Snapshot() => new
    {
        mode = ModSettings.Current.Mode,
        extra_energy = ModSettings.Current.ExtraEnergy,
        every_turn = ModSettings.Current.EveryTurn,
        search_seconds = ModSettings.Current.SearchSeconds,
        use_potions = ModSettings.Current.UsePotions,
        search = ModSettings.Current.SearchSettings(),
    };

    public static List<string> Lines()
    {
        try { return File.Exists(Path_) ? File.ReadAllLines(Path_).Where(l => l.Length > 0).ToList() : new(); }
        catch { return new(); }
    }

    private static void Append(string line)
    {
        try { File.AppendAllText(Path_, line + "\n"); } catch { }
    }
}
