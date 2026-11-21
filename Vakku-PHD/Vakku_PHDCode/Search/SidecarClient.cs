using System.Net.Http;
using System.Text;
using System.Text.Json;
using Vakku_PHD.Vakku_PHDCode.Sidecar;

namespace Vakku_PHD.Vakku_PHDCode.Search;

/// <summary>Owns the one sidecar process and talks to it. Starts it on first use; once it fails it stays off for the rest of the session.</summary>
public static class SidecarClient
{
    private static readonly HttpClient Http = new() { Timeout = Timeout.InfiniteTimeSpan };
    private static SidecarProcess? _process;
    private static Task<bool>? _starting;
    private static bool _failed;
    public static string? Hello { get; private set; }

    public static Task<bool> EnsureStarted() => _starting ??= StartAsync();

    private static async Task<bool> StartAsync()
    {
        try
        {
            _process = new SidecarProcess();
            string hello = await _process.StartAsync(TimeSpan.FromSeconds(15));
            Hello = hello;
            MainFile.Logger.Info($"sidecar up on port {_process.Port}: {hello}");
            return true;
        }
        catch (Exception e)
        {
            _failed = true;
            LastFight.Event($"sidecar failed to start: {e.Message}");
            MainFile.Logger.Error($"sidecar failed to start; the Earring plays like vanilla: {e.Message}");
            return false;
        }
    }

    public sealed record Reply(JsonElement Action, JsonElement Stats, List<string> Warnings, string Raw);

    /// <summary>One search. Throws on any failure (timeout, refusal, bad reply): the caller falls back to vanilla.</summary>
    public static async Task<Reply> Search(Dictionary<string, object?> state, ModSettings settings)
    {
        if (_failed || !await EnsureStarted()) throw new InvalidOperationException("sidecar unavailable");
        var body = new Dictionary<string, object?>
        {
            ["state"] = state,
            ["settings"] = settings.SearchSettings(),
            ["time_budget"] = settings.SearchSeconds,
            ["use_potions"] = settings.UsePotions,
        };
        using var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(settings.SearchSeconds + 5));
        using var response = await Http.PostAsync($"{_process!.BaseUrl}/search", content, cts.Token);
        string text = await response.Content.ReadAsStringAsync(cts.Token);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"sidecar refused ({(int)response.StatusCode}): {text}");
        using var doc = JsonDocument.Parse(text);
        var root = doc.RootElement;
        var warnings = new List<string>();
        if (root.TryGetProperty("warnings", out var w))
            foreach (var item in w.EnumerateArray()) warnings.Add(item.GetString() ?? "");
        return new Reply(root.GetProperty("action").Clone(), root.TryGetProperty("stats", out var s) ? s.Clone() : default, warnings, text);
    }

    public static async Task FightOver()
    {
        if (_process == null || _failed) return;
        try
        {
            using var content = new StringContent("{}", Encoding.UTF8, "application/json");
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            await Http.PostAsync($"{_process.BaseUrl}/fight_over", content, cts.Token);
        }
        catch { /* the next fight starts a fresh tree anyway */ }
    }
}
