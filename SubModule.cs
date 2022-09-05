using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using Newtonsoft.Json;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace ROT
{
    public class SubModule : MBSubModuleBase
    {
        internal static class Globals
        {
            internal static List<CharacterObject> TemplateWanderers = new();
            internal static Dictionary<Clan, List<int>> ClanTreasuryHistory = new();
        }

        private const string Id = "com.CarolinaWarlord.ROT";

        internal static readonly Harmony Harmony = new("com.CarolinaWarlord.ROT");

        internal static Settings Settings;
        public static float TradeBoundDistance = 1000;

        protected override void OnSubModuleLoad()
        {
            AccessTools.Field(typeof(Module), "_splashScreenPlayed").SetValue(Module.CurrentModule, true);
            Traverse.Create(Module.CurrentModule).Field<List<InitialStateOption>>("_initialStateOptions").Value
                .RemoveAll(i => i.Name.Contains("Campaign") || i.Name.Contains("SandBox"));
            Module.CurrentModule.AddInitialStateOption(new InitialStateOption("Into the Realm", new TextObject("{=171fTtIN}Into the Realm"), 3, delegate
            {
                MBGameManager.StartNewGame(new RealmOfThronesGameModeManager());
            }, () => (Module.CurrentModule.IsOnlyCoreContentEnabled, null)));
            var villageTradeBoundCampaignBehavior = AccessTools.TypeByName("VillageTradeBoundCampaignBehavior");
            var original = AccessTools.Method(villageTradeBoundCampaignBehavior, "TryToAssignTradeBoundForVillage");
            var transpiler = AccessTools.Method(typeof(Patches), nameof(Patches.Transpiler));
            Harmony.Patch(original: original, transpiler: new HarmonyMethod(transpiler));
            Harmony.PatchAll();
            try
            {
                var path = "..\\..\\Modules\\ROT-Content\\config.json";
                Settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(path));
            }
            catch (Exception ex)
            {
                FileLog.Log("Unable to load settings");
                FileLog.Log(ex.ToString());
                Settings = new Settings();
            }
        }
    }
}
