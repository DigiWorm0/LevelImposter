using System;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.AssetLoader;
using LevelImposter.Shop;
using LevelImposter.Trigger;
using UnityEngine;

namespace LevelImposter.Core;

/// <summary>
///     Component adds additional functionality added to AU's built-in ShipStatus.
///     Always added to ShipStatus on Awake, but dormant until LoadMap is fired.
/// </summary>
public class LIShipStatus(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    private static LIShipStatus? _instance;

    // Components
    [HideFromIl2Cpp] public TriggerSystem TriggerSystem { get; } = new();
    [HideFromIl2Cpp] public RenameHandler Renames { get; } = new();
    [HideFromIl2Cpp] public ImStuck ImStuck { get; } = new();

    public static bool IsReady => !MapBuilder.IsBuilding && !LoadingBar.IsVisible;
    public static MapObjectDB MapObjectDB => MapBuilder.MapBuildRouter.MapObjectDB;

    public ShipStatus? ShipStatus { get; private set; }

    public void Awake()
    {
        ShipStatus = GetComponent<ShipStatus>();
        _instance = this;

        if (GameConfiguration.CurrentMap != null)
            MapBuilder.RebuildMap();
        else
            LILogger.Warn("No LevelImposter maps are available to load!");
    }

    public void Start()
    {
        // Set Shadow Quad Mask
        DestroyableSingleton<HudManager>.Instance.ShadowQuad.material.SetInt("_Mask", 7);

        // Run Stuck Checker
        ImStuck.Init();
    }

    public void OnDestroy()
    {
        _instance = null;

        // Wipe Asset Queues
        TextureLoader.Instance.ClearQueue();
        SpriteLoader.Instance.ClearQueue();
        AudioLoader.Instance.ClearQueue();
        
        // Wipe Cache (Freeplay Only)
        if (GameState.IsInFreeplay && LIConstants.FREEPLAY_FLUSH_CACHE)
            GCHandler.DisposeAll(GCBehavior.DisposeOnMapUnload);
    }

    /// <summary>
    ///     Gets the current LevelImposter instance or throws MissingShip if not found
    /// </summary>
    /// <returns>The current LevelImposter instance</returns>
    /// <exception cref="MissingShipException">If the instance is null or missing</exception>
    public static LIShipStatus GetInstance()
    {
        if (_instance == null)
            throw new MissingShipException();
        return _instance;
    }

    /// <summary>
    ///     Gets the current LevelImposter instance or null if not found
    /// </summary>
    /// <returns>The current LevelImposter instance or null</returns>
    public static LIShipStatus? GetInstanceOrNull()
    {
        return _instance;
    }

    /// <summary>
    ///     Gets ShipStatus or throws MissingShip if not found
    /// </summary>
    /// <returns>The local ShipStatus</returns>
    /// <exception cref="MissingShipException">ShipStatus or LIShipStatus is null</exception>
    public static ShipStatus GetShip()
    {
        var instance = GetInstance().ShipStatus;
        if (instance == null)
            throw new MissingShipException();
        return instance;
    }

    /// <summary>
    ///     Checks if the player is currently in a LevelImposter map.
    /// </summary>
    /// <returns>True if the player is in a LevelImposter map, false otherwise</returns>
    public static bool IsInstance()
    {
        return _instance != null;
    }
}