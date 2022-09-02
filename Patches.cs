using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;

namespace ROT;

public class Patches
{
    [HarmonyPatch(typeof(DefaultClanFinanceModel), "AddVillagesIncome")]
    public static class DefaultClanFinanceModelAddVillagesIncome
    {
        public static void Postfix(ref ExplainedNumber goldChange)
        {
            goldChange.Add(goldChange.ResultNumber * SubModule.Settings.VillagesFactor - goldChange.ResultNumber);
        }
    }

    [HarmonyPatch(typeof(DefaultClanFinanceModel), "AddTownTaxes")]
    public static class DefaultClanFinanceModelAddTownTaxes
    {
        public static void Postfix(ref ExplainedNumber goldChange)
        {
            goldChange.Add(goldChange.ResultNumber * SubModule.Settings.TownsFactor - goldChange.ResultNumber);
        }
    }

    [HarmonyPatch]
    public static class DefaultPartyFoodBuyingModelMinimumDaysFoodToLastWhileBuyingFoodTown
    {
        public static void Postfix(ref float __result)
        {
            __result = SubModule.Settings.MinimumFoodDaysTown;
        }
    }

    [HarmonyPatch]
    public static class DefaultPartyFoodBuyingModelMinimumDaysFoodToLastWhileBuyingFoodVillage
    {
        public static void Postfix(ref float __result)
        {
            __result = SubModule.Settings.MinimumFoodDaysVillage;
        }
    }

    [HarmonyPatch(typeof(DefaultVillageProductionCalculatorModel), "CalculateDailyFoodProductionAmount")]
    public static class DefaultVillageProductionCalculatorModelCalculateDailyFoodProductionAmount
    {
        public static void Postfix(ref float __result)
        {
            __result *= SubModule.Settings.FoodProductionFactor;
        }
    }

    [HarmonyPostfix]
    private static void CalculateBenefitScore(ref float __result)
    {
        __result *= SubModule.Settings.RiskFactor;
    }

    [HarmonyPostfix]
    private static void SpawnNewCompanionIfNeeded(ref float __result)
    {
        __result *= SubModule.Settings.WandererFactor;
    }
}
