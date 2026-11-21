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

    [ConfigSection("Potions")]
    public static bool UsePotions { get; set; } = false;
    public static bool PotionsOnlyInDanger { get; set; } = true;
    // <potion-prices>
    [ConfigSection("PotionPrices", CollapsedByDefault = true)]
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceAttackPotion { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceBlockPotion { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceColorlessPotion { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceDexterityPotion { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceEnergyPotion { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceExplosiveAmpoule { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceFirePotion { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceFlexPotion { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPricePowerPotion { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceSkillPotion { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceSpeedPotion { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceStrengthPotion { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceSwiftPotion { get; set; } = 12;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceVulnerablePotion { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceWeakPotion { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceBloodPotion { get; set; } = 18;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceBlessingOfTheForge { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceClarityExtract { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceCureAll { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceDuplicator { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceFortifier { get; set; } = 15;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceFyshOil { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceGamblersBrew { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceHeartOfIron { get; set; } = 18;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceLiquidBronze { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPricePotionOfBinding { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPricePowderedDemise { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceRadiantTincture { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceRegenPotion { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceStableSerum { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceTouchOfInsanity { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceAshwater { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceBeetleJuice { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceBottledPotential { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceDistilledChaos { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceDropletOfPrecognition { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceEntropicBrew { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceFairyInABottle { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceFruitJuice { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceGigantificationPotion { get; set; } = 20;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceLiquidMemories { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceLuckyTonic { get; set; } = 18;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceMazalethsGift { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceOrobicAcid { get; set; } = 20;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceShacklingPotion { get; set; } = 18;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceShipInABottle { get; set; } = 20;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceSneckoOil { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceSoldiersStew { get; set; } = 15;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceAmbergris { get; set; } = 45;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceFoulPotion { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceGlowwaterPotion { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPricePotionShapedRock { get; set; } = 2;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPricePoisonPotion { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceCunningPotion { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceGhostInAJar { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceStarPotion { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceKingsCourage { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceCosmicConcoction { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPricePotionOfDoom { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceBoneBrew { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPricePotOfGhouls { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceFocusPotion { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPricePotionOfCapacity { get; set; } = 16;
    [ConfigSlider(0, 60, 1, Format = "{0} HP")]
    public static double PotionPriceEssenceOfDarkness { get; set; } = 16;

    /// <summary>The price of every potion, by game id.</summary>
    public static Dictionary<string, double> PotionPriceTable() => new()
    {
        ["ATTACK_POTION"] = PotionPriceAttackPotion,
        ["BLOCK_POTION"] = PotionPriceBlockPotion,
        ["COLORLESS_POTION"] = PotionPriceColorlessPotion,
        ["DEXTERITY_POTION"] = PotionPriceDexterityPotion,
        ["ENERGY_POTION"] = PotionPriceEnergyPotion,
        ["EXPLOSIVE_AMPOULE"] = PotionPriceExplosiveAmpoule,
        ["FIRE_POTION"] = PotionPriceFirePotion,
        ["FLEX_POTION"] = PotionPriceFlexPotion,
        ["POWER_POTION"] = PotionPricePowerPotion,
        ["SKILL_POTION"] = PotionPriceSkillPotion,
        ["SPEED_POTION"] = PotionPriceSpeedPotion,
        ["STRENGTH_POTION"] = PotionPriceStrengthPotion,
        ["SWIFT_POTION"] = PotionPriceSwiftPotion,
        ["VULNERABLE_POTION"] = PotionPriceVulnerablePotion,
        ["WEAK_POTION"] = PotionPriceWeakPotion,
        ["BLOOD_POTION"] = PotionPriceBloodPotion,
        ["BLESSING_OF_THE_FORGE"] = PotionPriceBlessingOfTheForge,
        ["CLARITY_EXTRACT"] = PotionPriceClarityExtract,
        ["CURE_ALL"] = PotionPriceCureAll,
        ["DUPLICATOR"] = PotionPriceDuplicator,
        ["FORTIFIER"] = PotionPriceFortifier,
        ["FYSH_OIL"] = PotionPriceFyshOil,
        ["GAMBLERS_BREW"] = PotionPriceGamblersBrew,
        ["HEART_OF_IRON"] = PotionPriceHeartOfIron,
        ["LIQUID_BRONZE"] = PotionPriceLiquidBronze,
        ["POTION_OF_BINDING"] = PotionPricePotionOfBinding,
        ["POWDERED_DEMISE"] = PotionPricePowderedDemise,
        ["RADIANT_TINCTURE"] = PotionPriceRadiantTincture,
        ["REGEN_POTION"] = PotionPriceRegenPotion,
        ["STABLE_SERUM"] = PotionPriceStableSerum,
        ["TOUCH_OF_INSANITY"] = PotionPriceTouchOfInsanity,
        ["ASHWATER"] = PotionPriceAshwater,
        ["BEETLE_JUICE"] = PotionPriceBeetleJuice,
        ["BOTTLED_POTENTIAL"] = PotionPriceBottledPotential,
        ["DISTILLED_CHAOS"] = PotionPriceDistilledChaos,
        ["DROPLET_OF_PRECOGNITION"] = PotionPriceDropletOfPrecognition,
        ["ENTROPIC_BREW"] = PotionPriceEntropicBrew,
        ["FAIRY_IN_A_BOTTLE"] = PotionPriceFairyInABottle,
        ["FRUIT_JUICE"] = PotionPriceFruitJuice,
        ["GIGANTIFICATION_POTION"] = PotionPriceGigantificationPotion,
        ["LIQUID_MEMORIES"] = PotionPriceLiquidMemories,
        ["LUCKY_TONIC"] = PotionPriceLuckyTonic,
        ["MAZALETHS_GIFT"] = PotionPriceMazalethsGift,
        ["OROBIC_ACID"] = PotionPriceOrobicAcid,
        ["SHACKLING_POTION"] = PotionPriceShacklingPotion,
        ["SHIP_IN_A_BOTTLE"] = PotionPriceShipInABottle,
        ["SNECKO_OIL"] = PotionPriceSneckoOil,
        ["SOLDIERS_STEW"] = PotionPriceSoldiersStew,
        ["AMBERGRIS"] = PotionPriceAmbergris,
        ["FOUL_POTION"] = PotionPriceFoulPotion,
        ["GLOWWATER_POTION"] = PotionPriceGlowwaterPotion,
        ["POTION_SHAPED_ROCK"] = PotionPricePotionShapedRock,
        ["POISON_POTION"] = PotionPricePoisonPotion,
        ["CUNNING_POTION"] = PotionPriceCunningPotion,
        ["GHOST_IN_A_JAR"] = PotionPriceGhostInAJar,
        ["STAR_POTION"] = PotionPriceStarPotion,
        ["KINGS_COURAGE"] = PotionPriceKingsCourage,
        ["COSMIC_CONCOCTION"] = PotionPriceCosmicConcoction,
        ["POTION_OF_DOOM"] = PotionPricePotionOfDoom,
        ["BONE_BREW"] = PotionPriceBoneBrew,
        ["POT_OF_GHOULS"] = PotionPricePotOfGhouls,
        ["FOCUS_POTION"] = PotionPriceFocusPotion,
        ["POTION_OF_CAPACITY"] = PotionPricePotionOfCapacity,
        ["ESSENCE_OF_DARKNESS"] = PotionPriceEssenceOfDarkness,
    };
    // </potion-prices>

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

}
