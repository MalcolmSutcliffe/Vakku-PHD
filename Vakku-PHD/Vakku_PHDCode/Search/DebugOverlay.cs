using System.Text.Json;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Vakku_PHD.Vakku_PHDCode.Search;

/// <summary>The debug screen: while the search runs, each card in hand (and the End Turn button) shows the share of the search's visits it has, redrawn every frame.</summary>
public static class DebugOverlay
{
    private const string LabelName = "VakkuDebugShare";
    private static Dictionary<CardModel, double> _cardShare = new();
    private static double _endTurnShare;
    private static bool _active;
    private static bool _connected;

    /// <summary>Polls the sidecar for the search's progress until `search` finishes.</summary>
    public static async Task Follow(Task search, Player player)
    {
        if (!VakkuConfig.DebugScreen) return;
        Connect();
        var hand = PileType.Hand.GetPile(player).Cards.ToList();
        while (!search.IsCompleted)
        {
            var children = await SidecarClient.Progress();
            if (children != null) Update(children.Value, hand);
            await Task.Delay(33);
        }
    }

    private static void Update(JsonElement children, List<CardModel> hand)
    {
        var shares = new Dictionary<CardModel, double>();
        double endTurn = 0, total = 0;
        foreach (var child in children.EnumerateArray())
        {
            double visits = child.GetProperty("visits").GetInt32();
            total += visits;
            var action = child.GetProperty("action");
            string kind = action.GetProperty("action").GetString() ?? "";
            if (kind == "end_turn") endTurn += visits;
            else if (kind == "play_card" && action.GetProperty("card_index").GetInt32() is var i && i >= 0 && i < hand.Count)
                shares[hand[i]] = shares.GetValueOrDefault(hand[i]) + visits;
        }
        if (total <= 0) return;
        _cardShare = shares.ToDictionary(kv => kv.Key, kv => kv.Value / total);
        _endTurnShare = endTurn / total;
        _active = true;
    }

    private static void Connect()
    {
        if (_connected || Engine.GetMainLoop() is not SceneTree tree) return;
        tree.ProcessFrame += Render;
        _connected = true;
    }

    private static void Render()
    {
        if (!_active) return;
        try
        {
            var hand = NPlayerHand.Instance;
            if (hand != null)
                foreach (var (card, share) in _cardShare)
                {
                    var holder = hand.GetCardHolder(card);
                    if (holder != null) SetLabel(holder, share, new Vector2(-70, -330));
                }
            var endTurn = NCombatRoom.Instance?.Ui?.EndTurnButton;
            if (endTurn != null) SetLabel(endTurn, _endTurnShare, new Vector2(0, -60));
        }
        catch (Exception e)
        {
            MainFile.Logger.Warn($"debug screen: {e.Message}");
            _active = false;
        }
    }

    private static void SetLabel(Control parent, double share, Vector2 position)
    {
        var label = parent.GetNodeOrNull<Label>(LabelName);
        if (label == null)
        {
            label = new Label { Name = LabelName, MouseFilter = Control.MouseFilterEnum.Ignore, ZIndex = 100, Position = position };
            label.AddThemeFontSizeOverride("font_size", 34);
            label.AddThemeColorOverride("font_outline_color", new Color(0, 0, 0));
            label.AddThemeConstantOverride("outline_size", 10);
            parent.AddChild(label);
        }
        label.Text = $"{share * 100:0}%";
        label.Modulate = share >= 0.5 ? new Color(0.6f, 1f, 0.6f) : new Color(1f, 1f, 1f);
    }

    /// <summary>Removes the labels (the Earring is done).</summary>
    public static void Clear()
    {
        _active = false;
        _cardShare = new();
        var hand = NPlayerHand.Instance;
        if (hand != null)
            foreach (var holder in hand.GetChildren().SelectMany(c => c.GetChildren().Prepend(c)).OfType<NHandCardHolder>()) Remove(holder);
        var endTurn = NCombatRoom.Instance?.Ui?.EndTurnButton;
        if (endTurn != null) Remove(endTurn);
    }

    private static void Remove(Node parent) => parent.GetNodeOrNull<Label>(LabelName)?.QueueFree();
}
