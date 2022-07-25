using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using LevelImposter.Core;
using LevelImposter.Shop;
using UnhollowerRuntimeLib;

namespace LevelImposter
{
    [BepInPlugin(ID, "LevelImposter", VERSION)]
    [BepInProcess("Among Us.exe")]
    public class LevelImposter : BasePlugin
    {
        public const string ID = "com.DigiWorm.LevelImposter";
        public const string VERSION = "2.0.1";

        public HarmonyLib.Harmony Harmony { get; } = new HarmonyLib.Harmony(ID);

        public override void Load()
        {
            LILogger.Init();
            ClassInjector.RegisterTypeInIl2Cpp<LIShipStatus>();
            ClassInjector.RegisterTypeInIl2Cpp<ShopManager>();
            ClassInjector.RegisterTypeInIl2Cpp<MapButton>();
            ClassInjector.RegisterTypeInIl2Cpp<FilterButton>();
            Harmony.PatchAll();
            LILogger.Msg("LevelImposter Initialized.");
        }
    }
}
