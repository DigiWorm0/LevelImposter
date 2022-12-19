using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using LevelImposter.Core;
using LevelImposter.DB;
using LevelImposter.Shop;
using Il2CppInterop.Runtime.Injection;
using AmongUs.Data;

namespace LevelImposter
{
    [BepInAutoPlugin(ID, "LevelImposter")]
    [BepInDependency(REACTOR_ID)]
    [BepInProcess("Among Us.exe")]
    public partial class LevelImposter : BasePlugin
    {
        public const string ID = "com.DigiWorm.LevelImposter";
        public const string REACTOR_ID = "gg.reactor.api";

        public HarmonyLib.Harmony Harmony { get; } = new HarmonyLib.Harmony(ID);

        public static ConfigFile ConfigFile { get; private set; }
        
        public override void Load()
        {
            ConfigFile = Config;
            LILogger.Init();
            LIDeepLink.Init();

            DataManager.Player.Onboarding.ViewedHideAndSeekHowToPlay = true;

            ClassInjector.RegisterTypeInIl2Cpp<LIShipStatus>();
            ClassInjector.RegisterTypeInIl2Cpp<LIStar>();
            ClassInjector.RegisterTypeInIl2Cpp<LIFloat>();
            ClassInjector.RegisterTypeInIl2Cpp<LITeleporter>();
            ClassInjector.RegisterTypeInIl2Cpp<GIFAnimator>();
            ClassInjector.RegisterTypeInIl2Cpp<AssetDB>();

            ClassInjector.RegisterTypeInIl2Cpp<LITriggerable>();
            ClassInjector.RegisterTypeInIl2Cpp<LITriggerArea>();
            ClassInjector.RegisterTypeInIl2Cpp<LITriggerSpawnable>();
            ClassInjector.RegisterTypeInIl2Cpp<DummyMinigame>();

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
