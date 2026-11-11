using System.Collections.Generic;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using LevelImposter.Build;
using LevelImposter.Core.Components;
using LevelImposter.Core.GarbageCollection;
using LevelImposter.Core.ModCompatibility;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using LevelImposter.Lobby.Components;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LevelImposter.Lobby.Builders;

public static class LobbyMapBuilder
{
    /// <summary>
    ///     Resets and rebuilds the lobby map based on
    ///     <see cref="GameConfiguration.CurrentLobbyMap" />.
    /// </summary>
    public static void Rebuild()
    {
        var currentMap = GameConfiguration.CurrentLobbyMap;
        if (currentMap == null)
            return;

        ResetMap();
        LIBaseShip.Instance?.SetMap(currentMap);
        BuildMap(currentMap);
    }

    /// <summary>
    ///     Clears existing GameObjects and properties in the lobby.
    ///     Ensures the builders start with a clean slate.
    /// </summary>
    private static void ResetMap()
    {
        var lobbyBehaviour = LILobbyBehaviour.GetLobbyBehaviour();

        // Fix Lobby to be 0,0
        lobbyBehaviour.transform.position = Vector3.zero;

        // Reset LobbyBehaviour Properties
        lobbyBehaviour.AllRooms = new Il2CppReferenceArray<SkeldShipRoom>(0);
        lobbyBehaviour.SpawnPositions = new Il2CppStructArray<Vector2>(0);
        lobbyBehaviour.GetComponent<Collider2D>().enabled = false;
        lobbyBehaviour.DropShipSound = null;
        lobbyBehaviour.MapTheme = null;

        // Set Skybox Color
        Camera.main?.backgroundColor = Color.black;

        // Remove All Children
        while (lobbyBehaviour.transform.childCount > 0)
            Object.DestroyImmediate(lobbyBehaviour.transform.GetChild(0).gameObject);

        // Remove StellarLobby
        if (CompatibilityFlags.IsStellarCompatibilityEnabled)
        {
            Object.Destroy(GameObject.Find("StellarLobby(Clone)"));
            Object.Destroy(GameObject.Find("LeftEngine(Clone)"));
        }
    }

    /// <summary>
    ///     Constructs all the GameObjects and properties into a lobby
    ///     from a LevelImposter map file.
    /// </summary>
    /// <param name="map">The map file to build from</param>
    private static void BuildMap(LIMap map)
    {
        var lobbyBehaviour = LILobbyBehaviour.GetLobbyBehaviour();
        LILogger.Info($"Building lobby map from {map}...");

        GCHandler.SetDefaultBehavior(GCBehavior.DisposeOnLobbyUnload);

        BuildRouter.BuildMap(
            map,
            LILobbyBehaviour.GetInstance(),
            new Dictionary<string, object>
            {
                { "lobbyBehaviour", lobbyBehaviour }
            }
        );

        LILogger.Info($"Built lobby map from {map}");
    }
}