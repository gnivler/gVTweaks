using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Steamworks;
using static gVTweaks.Settings;

namespace gVTweaks
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class Mod : BaseUnityPlugin
    {
        public const string PluginGUID = "AD0FD0DF-3F70-418F-9EA4-FCFBFA49165E";
        public const string PluginName = "gVTweaks";
        public const string PluginVersion = "1.0.0";

        internal static void L(object input)
        {
            //FileLog.Log(input?.ToString() ?? "null");
        }

        private void Awake()
        {
            // Acceptable value ranges can be defined to allow configuration via a slider in the BepInEx ConfigurationManager: https://github.com/BepInEx/BepInEx.ConfigurationManager
            smelterSize = Config.Bind("Smelter and Kiln (-1 means leave default)", "Smelter and Kiln Size", -1, new ConfigDescription("Smelter and kiln limits (restart needed)", new AcceptableValueList<int>(-1, 100, 200, 300, 400, 500, 1000, 2000, 10000)));
            deposits = Config.Bind("Smelter and Kiln (-1 means leave default)", "Max Deposits", true, "Deposit ALL wood, coal or ore with one keypress (live)");
            smelterSpeed = Config.Bind("Smelter and Kiln (-1 means leave default)", "Smelter and Kiln Speed", -1, new ConfigDescription("Ingot production in seconds (restart needed)", new AcceptableValueList<int>(-1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10)));
            shipSpeed = Config.Bind("", "Ship Speed (CAUTION!)", false, "Increase ship speeds at all levels - BE CAREFUL!!! (restart needed)");
            viewDistance = Config.Bind("Settings", "View Distance (EXPERIMENTAL)", false, "Increase view distance (more adjacent zones are loaded - longer loading, more processing required (restart needed))");
            lootDelay = Config.Bind("Settings", "Loot Delay", true, "Remove 0.5s delay on new items being lootable (live)");
            camera = Config.Bind("Settings", "Camera Distance", true, "Increase how far you can zoom out (restart needed)");
            bigHead = Config.Bind("Settings", "Big Head Toggle", true, "Comical.  Only shows locally.  Toggle with LeftShift-H (live)");
            var harmony = new Harmony("ca.gnivler.Valheim.gVTweaks");
            harmony.PatchAll();
        }
    }
}
