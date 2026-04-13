using System;
using UnityEngine;

namespace LevelImposter.Core;

public static class MapTargetExtensions
{
    /// <summary>
    /// Gets the default <see cref="GCBehavior"/> for the given <see cref="MapTarget"/>.
    /// </summary>
    /// <param name="mapTarget">The map target to get the GC behavior for.</param>
    /// <returns>The corresponding GC behavior.</returns>
    public static GCBehavior GetGCBehavior(this MapTarget mapTarget)
    {
        return mapTarget == MapTarget.Game ? 
            GCBehavior.DisposeOnMapUnload : 
            GCBehavior.DisposeOnLobbyUnload;
    }
    
    /// <summary>
    /// Gets the currently loaded <see cref="LIMap"/> for the given <see cref="MapTarget"/>.
    /// </summary>
    /// <param name="mapTarget">The map target to get the loaded map for.</param>
    /// <returns>>The currently loaded LIMap, or null if none is loaded.</returns>
    public static LIMap? GetLoadedMap(this MapTarget mapTarget)
    {
        return mapTarget == MapTarget.Game ? 
            GameConfiguration.CurrentMap : 
            GameConfiguration.CurrentLobbyMap;
    }
}