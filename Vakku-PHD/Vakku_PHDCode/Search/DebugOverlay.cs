using System.Text.Json;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Potions;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Vakku_PHD.Vakku_PHDCode.Search;

/// <summary>The debug screen: while the search runs, each card in hand (and the End Turn button) shows the share of the search's visits it has, redrawn every frame.</summary>
public static class DebugOverlay
{
    private const string LabelName = "VakkuDebugShare";
    private static Dictionary<CardModel, double> _cardShare = new();
    private static Dictionary<PotionModel, double> _potionShare = new();
    private static Dictionary<PotionModel, NPotionHolder> _holders = new();
    private static double _endTurnShare;
    private static bool _active;
    private static bool _connected;
    private static CardModel? _playedCard;        // what the search finally chose (after the paired and order checks)
    private static bool _playedEndTurn;
    private static string _playedNote = "";

    /// <summary>Polls the sidecar for the search's progress until `search` finishes.</summary>
    public static async Task Follow(Task search, Player player)
    {
        if (!VakkuConfig.DebugScreen) return;
        Connect();
        _playedCard = null; _playedEndTurn = false; _playedNote = "";
        var hand = PileType.Hand.GetPile(player).Cards.ToList();
        _holders = new();
        if (Engine.GetMainLoop() is SceneTree tree) FindHolders(tree.Root);
        while (!search.IsCompleted)
        {
            var children = await SidecarClient.Progress();
            if (children != null) Update(children.Value, hand, player);
            await Task.Delay(33);
        }
    }

    /// <summary>The search's final answer: that card (or End Turn) is marked, and a note says when a check replaced the most visited move.</summary>
    public static void Final(CardModel? card, bool endTurn, string note)
    {
        if (!VakkuConfig.DebugScreen) return;
        _playedCard = card; _playedEndTurn = endTurn; _playedNote = note;
    }

    private static void FindHolders(Node node)
    {
        if (node is NPotionHolder h && h.Potion?.Model is PotionModel m) _holders[m] = h;
        foreach (var child in node.GetChildren()) FindHolders(child);
    }

    private static void Update(JsonElement children, List<CardModel> hand, Player player)
    {
        var shares = new Dictionary<CardModel, double>();
        var potions = new Dictionary<PotionModel, double>();
        double endTurn = 0, total = 0;
        foreach (var child in children.EnumerateArray())
        {
            double visits = child.GetProperty("visits").GetInt32();
            total += visits;
            var action = child.GetProperty("action");
            string kind = action.GetProperty("action").GetString() ?? "";
            if (kind == "end_turn") endTurn += visits;
            else if (kind == "use_potion" && action.GetProperty("slot").GetInt32() is var slot && slot >= 0 && slot < player.PotionSlots.Count
                     && player.GetPotionAtSlotIndex(slot) is PotionModel potion)
                potions[potion] = potions.GetValueOrDefault(potion) + visits;
            else if (kind == "play_card" && action.GetProperty("card_index").GetInt32() is var i && i >= 0 && i < hand.Count)
                shares[hand[i]] = shares.GetValueOrDefault(hand[i]) + visits;
        }
        if (total <= 0) return;
        _cardShare = shares.ToDictionary(kv => kv.Key, kv => kv.Value / total);
        _potionShare = potions.ToDictionary(kv => kv.Key, kv => kv.Value / total);
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
                    if (holder != null) SetLabel(holder, share, new Vector2(-70, -330), card == _playedCard ? _playedNote : null, card == _playedCard);
                }
            foreach (var (potion, holder) in _holders.ToList())
                if (!GodotObject.IsInstanceValid(holder) || holder.Potion?.Model != potion)
                {
                    if (GodotObject.IsInstanceValid(holder)) Remove(holder);      // the potion was used: its percentage goes with it
                    _holders.Remove(potion);
                    _potionShare.Remove(potion);
                }
            foreach (var (potion, share) in _potionShare)
                if (_holders.TryGetValue(potion, out var holder) && GodotObject.IsInstanceValid(holder))
                    SetLabel(holder, share, new Vector2(0, 70), null, false);
            var endTurn = NCombatRoom.Instance?.Ui?.EndTurnButton;
            if (endTurn != null) SetLabel(endTurn, _endTurnShare, new Vector2(0, -60), _playedEndTurn ? _playedNote : null, _playedEndTurn);
        }
        catch (Exception e)
        {
            MainFile.Logger.Warn($"debug screen: {e.Message}");
            _active = false;
        }
    }

    private static void SetLabel(Control parent, double share, Vector2 position, string? note = null, bool chosen = false)
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
        label.Text = $"{share * 100:0}%" + (chosen ? " \u25B6" + (string.IsNullOrEmpty(note) ? "" : " " + note) : "");
        label.Modulate = chosen ? new Color(1f, 0.85f, 0.3f) : share >= 0.5 ? new Color(0.6f, 1f, 0.6f) : new Color(1f, 1f, 1f);
    }

    /// <summary>Removes the labels (the Earring is done).</summary>
    public static void Clear()
    {
        _active = false;
        _cardShare = new();
        _potionShare = new();
        foreach (var holder in _holders.Values) if (GodotObject.IsInstanceValid(holder)) Remove(holder);
        _holders = new();
        var hand = NPlayerHand.Instance;
        if (hand != null)
            foreach (var holder in hand.GetChildren().SelectMany(c => c.GetChildren().Prepend(c)).OfType<NHandCardHolder>()) Remove(holder);
        var endTurn = NCombatRoom.Instance?.Ui?.EndTurnButton;
        if (endTurn != null) Remove(endTurn);
    }

    private static void Remove(Node parent) => parent.GetNodeOrNull<Label>(LabelName)?.QueueFree();
}
