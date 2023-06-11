using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using LevelImposter.Core;
using LevelImposter.DB;
using LevelImposter.Shop;
using Il2CppInterop.Runtime.Injection;
using AmongUs.Data;
using Reactor.Networking.Attributes;

namespace LevelImposter
{
    [BepInAutoPlugin(ID, "LevelImposter")]
    [BepInDependency(ModCompatibility.REACTOR_ID)]
    [BepInDependency(ModCompatibility.SUBMERGED_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(ModCompatibility.TOU_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(ModCompatibility.TOR_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [ReactorModFlags(Reactor.Networking.ModFlags.RequireOnAllClients)]
    [BepInProcess("Among Us.exe")]
    public partial class LevelImposter : BasePlugin
    {
        public const string ID = "com.DigiWorm.LevelImposter";

        public HarmonyLib.Harmony Harmony { get; } = new HarmonyLib.Harmony(ID);

        public override void Load()
        {
            // Init Subsystems
            LILogger.Init();
            LIDeepLink.Init();
            ModCompatibility.Init();
            
            // Bypass Hide and Seek Tutorial
            DataManager.Player.Onboarding.ViewedHideAndSeekHowToPlay = true;

            // Increase max X and Y range from -50 - 50 >>> -500 - 500
            NetHelpers.XRange = new FloatRange(-500f, 500f);
            NetHelpers.YRange = new FloatRange(-500f, 500f);
            
            // IUsable Interface
            RegisterTypeOptions usableInterface = new()
            {
                Interfaces = new(new System.Type[]
                {
                    typeof(IUsable)
                })
            };

            // Inject MonoBehaviours
            ClassInjector.RegisterTypeInIl2Cpp<LIShipStatus>();
            ClassInjector.RegisterTypeInIl2Cpp<LIStar>();
            ClassInjector.RegisterTypeInIl2Cpp<LIFloat>();
            ClassInjector.RegisterTypeInIl2Cpp<LITeleporter>();
            ClassInjector.RegisterTypeInIl2Cpp<LITriggerable>();
            ClassInjector.RegisterTypeInIl2Cpp<LITriggerArea>();
            ClassInjector.RegisterTypeInIl2Cpp<LITriggerSpawnable>();
            ClassInjector.RegisterTypeInIl2Cpp<MinigameSprites>();
            ClassInjector.RegisterTypeInIl2Cpp<GIFAnimator>();
            ClassInjector.RegisterTypeInIl2Cpp<SpriteLoader>();
            ClassInjector.RegisterTypeInIl2Cpp<TriggerConsole>(usableInterface);

            ClassInjector.RegisterTypeInIl2Cpp<AssetDB>();

            ClassInjector.RegisterTypeInIl2Cpp<LevelImposterAPI>();
            ClassInjector.RegisterTypeInIl2Cpp<ThumbnailFileAPI>();
            ClassInjector.RegisterTypeInIl2Cpp<MapCacheAPI>();
            ClassInjector.RegisterTypeInIl2Cpp<GitHubAPI>();
            ClassInjector.RegisterTypeInIl2Cpp<MapFileAPI>();
            ClassInjector.RegisterTypeInIl2Cpp<ConfigAPI>();
            ClassInjector.RegisterTypeInIl2Cpp<MapBanner>();
            ClassInjector.RegisterTypeInIl2Cpp<ShopButtons>();
            ClassInjector.RegisterTypeInIl2Cpp<ShopManager>();
            ClassInjector.RegisterTypeInIl2Cpp<ShopSpawner>();
            ClassInjector.RegisterTypeInIl2Cpp<Spinner>();

            // Patch Methods
            Harmony.PatchAll();
            LILogger.Msg("LevelImposter Initialized.");
        }
    }
}
