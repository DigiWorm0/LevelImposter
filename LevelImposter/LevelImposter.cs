using System;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using LevelImposter.Builders.Generic;
using LevelImposter.Core.Components;
using LevelImposter.Core.Services;
using LevelImposter.Core.Utils;
using LevelImposter.DB;
using LevelImposter.FileIO.API;
using LevelImposter.FileIO.Cache;
using LevelImposter.Lobby.Components;
using LevelImposter.Lobby.Utils;
using LevelImposter.Shop.Components;
using LevelImposter.Shop.Utils;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;

namespace LevelImposter;

[BepInAutoPlugin(ID, "LevelImposter")]
[BepInDependency("gg.reactor.api")]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
[BepInProcess("Among Us.exe")]
public partial class LevelImposter : BasePlugin
{
    public const string ID = "com.DigiWorm.LevelImposter";

    public Harmony Harmony { get; } = new(ID);

    public static string DisplayVersion => Version.Contains('+')
        ? Version.Substring(0, Version.IndexOf("+", StringComparison.Ordinal))
        : Version;

    public static bool IsDevBuild => Version.Contains("dev");

    public override void Load()
    {
        // Init Global Subsystems
        LILogger.Init();
        MapFileAPI.Init();
        ConfigAPI.Load();
        FileCache.Init();
        ImStuckService.Init();
        LobbyUIService.Init();
        SpriteBuilder.Init();

        // Load Mod Compatibility
        IL2CPPChainloader.Instance.Finished += ModCompatibility.Init;

        // IUsable Interface
        RegisterTypeOptions usableInterface = new()
        {
            Interfaces = new Il2CppInterfaceCollection([typeof(IUsable)])
        };

        // Inject MonoBehaviours
        ClassInjector.RegisterTypeInIl2Cpp<LIBaseShip>();
        ClassInjector.RegisterTypeInIl2Cpp<LIShipStatus>();
        ClassInjector.RegisterTypeInIl2Cpp<LIStar>();
        ClassInjector.RegisterTypeInIl2Cpp<LIFloat>();
        ClassInjector.RegisterTypeInIl2Cpp<LIScroll>();
        ClassInjector.RegisterTypeInIl2Cpp<LITeleporter>();
        ClassInjector.RegisterTypeInIl2Cpp<LITriggerArea>();
        ClassInjector.RegisterTypeInIl2Cpp<TriggerAnim>();
        ClassInjector.RegisterTypeInIl2Cpp<LIDeathArea>();
        ClassInjector.RegisterTypeInIl2Cpp<LIShakeArea>();
        ClassInjector.RegisterTypeInIl2Cpp<LITriggerSpawnable>();
        ClassInjector.RegisterTypeInIl2Cpp<MinigameSprites>();
        ClassInjector.RegisterTypeInIl2Cpp<LagLimiter>();
        ClassInjector.RegisterTypeInIl2Cpp<GIFAnimator>();
        ClassInjector.RegisterTypeInIl2Cpp<SpriteAnimator>();
        ClassInjector.RegisterTypeInIl2Cpp<TriggerSoundPlayer>();
        ClassInjector.RegisterTypeInIl2Cpp<LIPlayerMover>();
        ClassInjector.RegisterTypeInIl2Cpp<TriggerConsole>(usableInterface);
        ClassInjector.RegisterTypeInIl2Cpp<EditableLadderConsole>();
        ClassInjector.RegisterTypeInIl2Cpp<LIExileController>();
        ClassInjector.RegisterTypeInIl2Cpp<LIPhysicsObject>();
        ClassInjector.RegisterTypeInIl2Cpp<LITextTranslatorTMP>();

        ClassInjector.RegisterTypeInIl2Cpp<PrefabDB>();

        ClassInjector.RegisterTypeInIl2Cpp<ModUpdater>();
        ClassInjector.RegisterTypeInIl2Cpp<Shop.Components.ProgressBar>();
        ClassInjector.RegisterTypeInIl2Cpp<ConnectionAnimation>();
        ClassInjector.RegisterTypeInIl2Cpp<FloatingAnimation>();
        ClassInjector.RegisterTypeInIl2Cpp<PulseAnimation>();
        ClassInjector.RegisterTypeInIl2Cpp<LoadingOverlay>();
        ClassInjector.RegisterTypeInIl2Cpp<RandomOverlay>();
        ClassInjector.RegisterTypeInIl2Cpp<MapBanner>();
        ClassInjector.RegisterTypeInIl2Cpp<GameObjectGrid>();
        ClassInjector.RegisterTypeInIl2Cpp<ShopTabButton>();
        ClassInjector.RegisterTypeInIl2Cpp<ShopManager>();
        ClassInjector.RegisterTypeInIl2Cpp<MapsFolderWatcher>();
        ClassInjector.RegisterTypeInIl2Cpp<Spinner>();
        ClassInjector.RegisterTypeInIl2Cpp<LoadingBar>();
        ClassInjector.RegisterTypeInIl2Cpp<LILobbyBehaviour>();
        ClassInjector.RegisterTypeInIl2Cpp<LobbyVersionTag>();
        ClassInjector.RegisterTypeInIl2Cpp<LobbyMapConsole>(usableInterface);

        // Reactor Version Patch
        ReactorCredits.Register(
            "LevelImposter",
            DisplayVersion,
            false,
            ReactorCredits.AlwaysShow
        );

        // Patch Methods
        Harmony.PatchAll();
        LILogger.Msg("LevelImposter Initialized.");
    }
}