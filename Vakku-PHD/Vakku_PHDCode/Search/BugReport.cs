using System.Reflection;
using System.Text;
using System.Text.Json;
using Godot;

namespace Vakku_PHD.Vakku_PHDCode.Search;

/// <summary>Writes the last fight with the Earring to one JSON file the player can read and send. Nothing is uploaded.</summary>
public static class BugReport
{
    public static string Folder => ProjectSettings.GlobalizePath("user://Vakku_PHD_reports");

    /// <summary>The path of the file written, or null when there was no fight to report.</summary>
    public static string? Save()
    {
        var lines = LastFight.Lines();
        if (lines.Count == 0)
        {
            MainFile.Logger.Info("bug report: no fight with the Earring yet");
            return null;
        }
        Directory.CreateDirectory(Folder);
        string path = Path.Combine(Folder, $"report_{DateTime.Now:yyyyMMdd_HHmmss}.json");

        var sb = new StringBuilder("{\n");
        sb.Append("\"report_version\":1,\n");
        sb.Append("\"created_utc\":").Append(JsonSerializer.Serialize(DateTime.UtcNow.ToString("o"))).Append(",\n");
        sb.Append("\"mod_version\":").Append(JsonSerializer.Serialize(typeof(BugReport).Assembly.GetName().Version?.ToString() + " / " + ModManifestVersion())).Append(",\n");
        sb.Append("\"os\":").Append(JsonSerializer.Serialize(System.Runtime.InteropServices.RuntimeInformation.OSDescription)).Append(",\n");
        sb.Append("\"game\":").Append(JsonSerializer.Serialize(GameVersion())).Append(",\n");
        sb.Append("\"sidecar\":").Append(JsonSerializer.Serialize(SidecarClient.Hello ?? "not started")).Append(",\n");
        sb.Append("\"settings\":").Append(JsonSerializer.Serialize(LastFight.Snapshot())).Append(",\n");
        sb.Append("\"fight\":[\n").Append(string.Join(",\n", lines)).Append("\n]\n}\n");
        File.WriteAllText(path, sb.ToString());
        MainFile.Logger.Info($"bug report saved: {path}");
        return path;
    }

    private static string ModManifestVersion()
    {
        try
        {
            var json = Path.Combine(Path.GetDirectoryName(typeof(BugReport).Assembly.Location)!, "Vakku_PHD.json");
            using var doc = JsonDocument.Parse(File.ReadAllText(json));
            return doc.RootElement.GetProperty("version").GetString() ?? "?";
        }
        catch { return "?"; }
    }

    private static string GameVersion()
    {
        try
        {
            var file = Path.Combine(Path.GetDirectoryName(OS.GetExecutablePath())!, "release_info.json");
            return File.Exists(file) ? File.ReadAllText(file).Replace("\n", " ").Trim() : "unknown";
        }
        catch { return "unknown"; }
    }
}
