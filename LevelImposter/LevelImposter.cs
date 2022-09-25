using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using LevelImposter.Core;
using LevelImposter.DB;
using LevelImposter.Shop;
using UnhollowerRuntimeLib;

namespace LevelImposter
{
    [BepInPlugin(ID, "LevelImposter", VERSION)]
    [BepInDependency(REACTOR_ID)]
    [BepInProcess("Among Us.exe")]
    public class LevelImposter : BasePlugin
    {
        public const string ID = "com.DigiWorm.LevelImposter";
        public const string VERSION = "0.6.0";
        public const string REACTOR_ID = "gg.reactor.api";

        public HarmonyLib.Harmony Harmony { get; } = new HarmonyLib.Harmony(ID);

        public static ConfigFile ConfigFile { get; private set; }
        
        public override void Load()
        {
            ConfigFile = Config;
            LILogger.Init();
            LIDeepLink.Init();

            ClassInjector.RegisterTypeInIl2Cpp<LIShipStatus>();
            ClassInjector.RegisterTypeInIl2Cpp<LIStar>();
            ClassInjector.RegisterTypeInIl2Cpp<LIFloat>();
            ClassInjector.RegisterTypeInIl2Cpp<LITeleporter>();
            ClassInjector.RegisterTypeInIl2Cpp<LITriggerable>();
            ClassInjector.RegisterTypeInIl2Cpp<GIFAnimator>();
            ClassInjector.RegisterTypeInIl2Cpp<AssetDB>();
            ClassInjector.RegisterTypeInIl2Cpp<LIMapSelector>();
            ClassInjector.RegisterTypeInIl2Cpp<LIMapConsole>();

            ClassInjector.RegisterTypeInIl2Cpp<LevelImposterAPI>();
            ClassInjector.RegisterTypeInIl2Cpp<MapBanner>();
            ClassInjector.RegisterTypeInIl2Cpp<MapFileAPI>();
            ClassInjector.RegisterTypeInIl2Cpp<ShopButtons>();
            ClassInjector.RegisterTypeInIl2Cpp<ShopManager>();
            ClassInjector.RegisterTypeInIl2Cpp<ShopSpawner>();
            ClassInjector.RegisterTypeInIl2Cpp<Spinner>();
            ClassInjector.RegisterTypeInIl2Cpp<ThumbnailFileAPI>();

            Harmony.PatchAll();
            LILogger.Msg("LevelImposter Initialized.");
        }
    }
}
