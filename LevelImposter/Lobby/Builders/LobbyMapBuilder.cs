using Il2CppInterop.Runtime.InteropTypes.Arrays;
using LevelImposter.Builders;
using LevelImposter.Builders.Lobby;
using LevelImposter.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LevelImposter.Lobby;

public static class LobbyMapBuilder
{
    private static readonly BuildRouter LobbyBuildRouter = new([
        new TransformBuilder(),
        new SpriteBuilder(MapTarget.Lobby),
        new ColliderBuilder(),
        new RoomBuilder(),
        new AmbientSoundBuilder(),
        new StepSoundBuilder(),
        
        new LobbyOptionsConsoleBuilder(),
        new LobbyWardrobeConsoleBuilder(),
        new LobbyMapConsoleBuilder(),
        
        new TriggerShakeBuilder(),
        new StarfieldBuilder(),
    ]);
    
    /// <summary>
    /// Resets and rebuilds the lobby map based on
    /// <see cref="GameConfiguration.CurrentLobbyMap"/>.
    /// </summary>
    public static void Rebuild()
    {
        ResetMap();

        if (GameConfiguration.CurrentLobbyMap != null)
            BuildMap(GameConfiguration.CurrentLobbyMap);
        else
            BuildDropship();
    }
    
    /// <summary>
    /// Clears existing GameObjects and properties in the lobby.
    /// Ensures the builders start with a clean slate.
    /// </summary>
    private static void ResetMap()
    {
        var lobbyBehaviour = LILobbyBehaviour.GetLobbyBehaviour();

        // Fix Lobby to be 0,0
        lobbyBehaviour.transform.position = Vector3.zero;
        
        // Reset LobbyBehaviour Properties
        lobbyBehaviour.AllRooms = new Il2CppReferenceArray<SkeldShipRoom>(0);
        lobbyBehaviour.SpawnPositions = new Il2CppStructArray<Vector2>(1);
        lobbyBehaviour.SpawnPositions[0] = new Vector2(0, 0);
        lobbyBehaviour.GetComponent<Collider2D>().enabled = false;
        
        // Remove All Children
        while (lobbyBehaviour.transform.childCount > 0)
            Object.DestroyImmediate(lobbyBehaviour.transform.GetChild(0).gameObject);
    }

    /// <summary>
    /// Constructs all the GameObjects and properties into a lobby
    /// from a LevelImposter map file.
    /// </summary>
    /// <param name="map">The map file to build from</param>
    private static void BuildMap(LIMap map)
    {
        LILogger.Info($"Building lobby map from {map}...");
        
        GCHandler.SetDefaultBehavior(GCBehavior.DisposeOnLobbyUnload);
        
        LobbyBuildRouter.BuildMap(
            map.elements,
            LILobbyBehaviour.GetInstance().transform);
        
        LILogger.Info($"Built lobby map from {map}");
    }

    /// <summary>
    /// Reloads the original lobby dropship map into the game.
    /// </summary>
    private static void BuildDropship()
    {
        LILogger.Info("Rebuilding original lobby dropship...");
        
        var lobbyBehaviour = LILobbyBehaviour.GetInstance();
        Object.Destroy(lobbyBehaviour.gameObject);
        
        LobbyDropshipPrefab.Instantiate();
        
        // TODO: Replay spawn animation
        
        LILogger.Info("Rebuilt original lobby dropship");
    }
}