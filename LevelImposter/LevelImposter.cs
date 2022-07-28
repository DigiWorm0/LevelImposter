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
        public const string VERSION = "0.4.0";

        public HarmonyLib.Harmony Harmony { get; } = new HarmonyLib.Harmony(ID);

        public override void Load()
        {
            LILogger.Init();
            MapLoader.Init();
            ClassInjector.RegisterTypeInIl2Cpp<LIShipStatus>();
            ClassInjector.RegisterTypeInIl2Cpp<ShopManager>();
            ClassInjector.RegisterTypeInIl2Cpp<MapBanner>();
            ClassInjector.RegisterTypeInIl2Cpp<MapBannerButton>();
            ClassInjector.RegisterTypeInIl2Cpp<FilterButton>();
            ClassInjector.RegisterTypeInIl2Cpp<FolderButton>();
            ClassInjector.RegisterTypeInIl2Cpp<GIFAnimator>();
            Harmony.PatchAll();
            LILogger.Msg("LevelImposter Initialized.");
        }
    }
}
