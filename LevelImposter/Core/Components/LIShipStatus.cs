using System;
using Reactor.Networking.Extensions;

namespace LevelImposter.Core;

/// <summary>
///     Component adds additional functionality added to AU's built-in ShipStatus.
///     Always added to ShipStatus on Awake, but dormant until LoadMap is fired.
/// </summary>
public class LIShipStatus(IntPtr intPtr) : LIBaseShip(intPtr)
{
    private static LIShipStatus? _instance;

    public ShipStatus? ShipStatus { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        
        // Update Instance
        _instance = this;
        ShipStatus = GetComponent<ShipStatus>();

        // Load Map Data
        if (GameConfiguration.CurrentMap != null)
        {
            MapBuilder.RebuildMap();
            return;
        }
        
        // No map data found, disconnect
        LILogger.Error("LIShipStatus loaded without any map data!");
        AmongUsClient.Instance?.DisconnectWithReason("LevelImposter couldn't find any map data.");
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
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
        return _instance ?? throw new MissingShipException();
    }

    /// <summary>
    ///     Gets ShipStatus or throws MissingShip if not found
    /// </summary>
    /// <returns>The local ShipStatus</returns>
    /// <exception cref="MissingShipException">ShipStatus or LIShipStatus is null</exception>
    public static ShipStatus GetShip()
    {
        return _instance?.ShipStatus ?? throw new MissingShipException();
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