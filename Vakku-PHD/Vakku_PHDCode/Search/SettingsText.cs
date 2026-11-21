using System.Text.Json;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;

namespace Vakku_PHD.Vakku_PHDCode.Search;

/// <summary>
/// The settings screen's labels and tooltips, read from localization/eng/settings_ui.json next to the DLL and merged into the game's
/// "settings_ui" table (the game itself only loads mod text that ships in a .pck). Edit that file to rename a setting.
/// </summary>
public static class SettingsText
{
    /// <summary>Files in localization/eng/ and the game text table each one is merged into.</summary>
    public static readonly string[] Tables = { "settings_ui", "relics" };

    public static string FilePath(string table) => Path.Combine(Path.GetDirectoryName(typeof(SettingsText).Assembly.Location)!, "localization", "eng", table + ".json");


    public static void Merge()
    {
        if (LocManager.Instance == null) { MainFile.Logger.Info("settings text: no LocManager yet"); return; }
        foreach (var name in Tables)
        {
            try
            {
                if (!File.Exists(FilePath(name))) { MainFile.Logger.Warn($"settings text: {FilePath(name)} not found"); continue; }
                var text = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(FilePath(name)));
                if (text == null) continue;
                var table = LocManager.Instance.GetTable(name);
                table.MergeWith(text);
                MainFile.Logger.Info($"settings text: merged {text.Count} entries into {name} (language {LocManager.Instance.Language})");
            }
            catch (Exception e)
            {
                MainFile.Logger.Warn($"settings text not merged into {name}: {e.Message}");
            }
        }
    }
}

/// <summary>The first language load happens inside LocManager's constructor, before Instance is set: merge once Initialize has returned, and again on every language change.</summary>
[HarmonyPatch(typeof(LocManager), nameof(LocManager.Initialize))]
public static class SettingsTextOnInitialize
{
    public static void Postfix() => SettingsText.Merge();
}

[HarmonyPatch(typeof(LocManager), nameof(LocManager.SetLanguage))]
public static class SettingsTextOnLanguage
{
    public static void Postfix() => SettingsText.Merge();
}
