using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using SandBox;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.ModuleManager;

namespace ROT
{
    public class Patches
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldc_R4 && instruction.OperandIs(150))
                    instruction.operand = SubModule.TradeBoundDistance;
                yield return instruction;
            }
        }

        [HarmonyPatch(typeof(SettlementPositionScript), "SettlementsDistanceCacheFilePath", MethodType.Getter)]
        public static class SettlementPositionScriptSettlementsDistanceCacheFilePath
        {
            public static void Postfix(ref string __result) => __result = ModuleHelper.GetModuleFullPath("ROT-Content") + "settlements_distance_cache.bin";
        }

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

        //[HarmonyPatch]
        //public static class DefaultPartyFoodBuyingModelMinimumDaysFoodToLastWhileBuyingFoodTown
        //{
        //    public static void Postfix(ref float __result)
        //    {
        //        __result = SubModule.Settings.MinimumFoodDaysTown;
        //    }
        //}

        //[HarmonyPatch]
        //public static class DefaultPartyFoodBuyingModelMinimumDaysFoodToLastWhileBuyingFoodVillage
        //{
        //    public static void Postfix(ref float __result)
        //    {
        //        __result = SubModule.Settings.MinimumFoodDaysVillage;
        //    }
        //}

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
}
