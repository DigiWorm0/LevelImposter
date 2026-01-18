using System;
using LevelImposter.Builders.Lobby;
using LevelImposter.Core;
using LevelImposter.Shop;
using UnityEngine;

namespace LevelImposter.Lobby;

public class LILobbyBehaviour(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    private static LILobbyBehaviour? _instance;
    
    private LobbyBehaviour? _lobbyBehaviour;
    
    public void Awake()
    {
        _lobbyBehaviour = GetComponent<LobbyBehaviour>();
        _instance = this;

        LobbyBehaviour.Instance = _lobbyBehaviour;  // <-- Jump start singleton assignment for GameState.IsInLobby
        
        // Run initialization methods
        LobbyDropshipPrefab.OnLobbyLoad();
        LobbyMapConsoleBuilder.Build();
        
        // Build lobby map on startup
        if (GameConfiguration.CurrentLobbyMap != null)
            LobbyMapBuilder.Rebuild();
    }

    public void Start()
    {
        // Set Shadow Quad Mask
        DestroyableSingleton<HudManager>.Instance.ShadowQuad.material.SetInt("_Mask", 7);
    }
    
    /// <summary>
    ///     Gets the current LILobbyBehaviour instance or throws exception if not found
    /// </summary>
    /// <returns>The current LILobbyBehaviour instance</returns>
    /// <exception cref="Exception">If LILobbyBehaviour.Instance is null</exception>
    public static LILobbyBehaviour GetInstance()
    {
        if (_instance == null)
            throw new Exception("LILobbyBehaviour instance not found!");
        return _instance;
    }

    /// <summary>
    ///     Gets the LobbyBehaviour component from the LILobbyBehaviour instance or throws exception if not found
    /// </summary>
    /// <returns>The LobbyBehaviour component</returns>
    /// <exception cref="Exception">>If the LobbyBehaviour component is null</exception>
    public static LobbyBehaviour GetLobbyBehaviour()
    {
        var lobbyBehaviour = GetInstance()._lobbyBehaviour;
        if (lobbyBehaviour == null)
            throw new Exception("LobbyBehaviour component not found on LILobbyBehaviour!");
        return lobbyBehaviour;
    }
}