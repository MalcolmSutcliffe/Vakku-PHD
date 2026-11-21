namespace Vakku_PHD.Vakku_PHDCode.Search;

/// <summary>A snapshot of the settings screen's values (VakkuConfig), as the search and the hook read them.</summary>
public sealed class ModSettings
{
    public static ModSettings Current => new();

    public string Mode => VakkuConfig.ObserveOnly ? "observe" : "play";
    public bool ExtraEnergy => VakkuConfig.ExtraEnergy;
    public bool EveryTurn => VakkuConfig.EveryTurn;
    public double SearchSeconds => VakkuConfig.SearchSeconds;
    public bool UsePotions => VakkuConfig.UsePotions;
    public bool PotionsOnlyInDanger => VakkuConfig.PotionsOnlyInDanger;
    public int PairedScenarios => VakkuConfig.PairedScenarios;

    /// <summary>The search settings the sidecar takes by name.</summary>
    public Dictionary<string, object> SearchSettings()
    {
        var settings = new Dictionary<string, object>
        {
            ["gold_per_hp"] = VakkuConfig.GoldPerHp,
            ["stolen_card_hp"] = VakkuConfig.StolenCardHp,
            ["max_hp_scaling"] = VakkuConfig.MaxHpValue,
            ["card_removal_hp"] = VakkuConfig.CardRemovalHp,
            ["card_upgrade_hp"] = VakkuConfig.CardUpgradeHp,
            ["card_reward_hp"] = VakkuConfig.CardRewardHp,
            ["scythe_growth_hp"] = VakkuConfig.ScytheGrowthHp,
            ["genetic_algorithm_growth_hp"] = VakkuConfig.GeneticAlgorithmGrowthHp,
            ["goopy_growth_hp"] = VakkuConfig.GoopyGrowthHp,
            ["paired_scenarios"] = PairedScenarios,
        };
        if (UsePotions)
        {
            settings["gated"] = PotionsOnlyInDanger;
            settings["potion_prices"] = VakkuConfig.PotionPriceTable();
        }
        if (OperatingSystem.IsWindows()) settings["check_workers"] = 1;     // no fork there
        return settings;
    }
}
