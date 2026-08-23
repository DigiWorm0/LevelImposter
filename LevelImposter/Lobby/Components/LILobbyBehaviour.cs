using System;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Builders.Lobby;
using LevelImposter.Core.Components;
using LevelImposter.Core.Utils;
using LevelImposter.Lobby.Builders;

namespace LevelImposter.Lobby.Components;

public class LILobbyBehaviour(IntPtr intPtr) : LIBaseShip(intPtr)
{
    private static LILobbyBehaviour? _instance;

    public LobbyBehaviour? LobbyBehaviour { get; private set; }

    [HideFromIl2Cpp] public IStepWatcher[] AllStepWatchers { get; private set; } = [];

    protected override void Awake()
    {
        base.Awake();

        // Get LobbyBehaviour component
        _instance = this;
        LobbyBehaviour = GetComponent<LobbyBehaviour>();

        // Run initialization methods
        LobbyMapConsoleBuilder.Build();

        // Build lobby map on startup
        if (GameConfiguration.CurrentLobbyMap != null)
            LobbyMapBuilder.Rebuild();
    }

    protected override void Start()
    {
        base.Start();

        // Get components in children
        AllStepWatchers = GetComponentsInChildren<IStepWatcher>(true);
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
        var lobbyBehaviour = GetInstance().LobbyBehaviour;
        if (lobbyBehaviour == null)
            throw new Exception("LobbyBehaviour component not found on LILobbyBehaviour!");
        return lobbyBehaviour;
    }


    /// <summary>
    ///     Checks if the player is currently in a LevelImposter lobby.
    /// </summary>
    /// <returns>True if the player is in a LevelImposter lobby, false otherwise</returns>
    public static bool IsInstance()
    {
        return _instance != null;
    }
}