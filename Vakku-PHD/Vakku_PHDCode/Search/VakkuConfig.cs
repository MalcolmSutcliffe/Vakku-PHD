using BaseLib.Config;

namespace Vakku_PHD.Vakku_PHDCode.Search;

/// <summary>The settings screen (BaseLib builds it from these properties) and the file it saves to.</summary>
[ConfigHoverTipsByDefault]
public class VakkuConfig : SimpleModConfig
{
    [ConfigSection("Earring")]
    public static bool ExtraEnergy { get; set; } = true;
    public static bool EveryTurn { get; set; } = false;
    [ConfigSlider(0.25, 5, 0.25, Format = "{0:0.00} s")]
    public static double SearchSeconds { get; set; } = 1.0;
    public static bool ObserveOnly { get; set; } = false;

    [ConfigSection("Values")]
    [ConfigSlider(0, 10, 0.5, Format = "{0:0.0} HP")]
    public static double MaxHpValue { get; set; } = 2.0;
    [ConfigSlider(1, 50, 1, Format = "{0} gold")]
    public static double GoldPerHp { get; set; } = 10.0;
    [ConfigSlider(0, 30, 1, Format = "{0} HP")]
    public static double StolenCardHp { get; set; } = 10.0;

    [ConfigSlider(0, 30, 1, Format = "{0} HP")]
    public static double CardRemovalHp { get; set; } = 10.0;
    [ConfigSlider(0, 30, 1, Format = "{0} HP")]
    public static double CardUpgradeHp { get; set; } = 6.0;
    [ConfigSlider(0, 30, 1, Format = "{0} HP")]
    public static double CardRewardHp { get; set; } = 6.0;

    [ConfigSlider(0, 30, 1, Format = "{0} HP")]
    public static double ScytheGrowthHp { get; set; } = 4.0;
    [ConfigSlider(0, 30, 1, Format = "{0} HP")]
    public static double GeneticAlgorithmGrowthHp { get; set; } = 4.0;
    [ConfigSlider(0, 10, 0.5, Format = "{0:0.0} HP")]
    public static double GoopyGrowthHp { get; set; } = 1.0;

    [ConfigSection("Advanced", CollapsedByDefault = true)]
    [ConfigSlider(0, 1000, 50, Format = "{0}")]
    public static int PairedScenarios { get; set; } = 300;

    [ConfigHideInUI]
    public static bool UsePotions { get; set; } = false;       // the hook does not drink potions yet
}
