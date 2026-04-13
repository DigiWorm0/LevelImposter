using System;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Lobby;
using LevelImposter.Trigger;
using UnityEngine;

namespace LevelImposter.Core;

/// <summary>
/// Represents the base class for all custom ship implementations in LevelImposter.
/// Extended by <see cref="LIShipStatus"/> and <see cref="LILobbyBehaviour"/>.
/// </summary>
public class LIBaseShip(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    // Singleton instance
    public static LIBaseShip? Instance { get; private set; }
    
    // Cached shader property IDs
    private static readonly int Mask = Shader.PropertyToID("_Mask");

    // Subsystems
    [HideFromIl2Cpp] public LIMap? CurrentMap { get; private set; }
    [HideFromIl2Cpp] public TriggerSystem TriggerSystem { get; } = new();
    [HideFromIl2Cpp] public RenameHandler Renames { get; } = new();
    [HideFromIl2Cpp] public MapObjectDB MapObjectDB { get; } = new();

    protected virtual void Awake()
    {
        Instance = this;
    }

    protected virtual void Start()
    {
        // Apply a fixed shadow quad mask for LevelImposter maps
        DestroyableSingleton<HudManager>.Instance.ShadowQuad.material.SetInt(Mask, 7);
    }

    protected virtual void OnDestroy()
    {
        // It's possible both lobby and game ship exist at the same time during scene transitions,
        // so we only clear the instance if it's this one
        if (Instance == this)
            Instance = null;
    }
    
    public virtual void SetMap(LIMap? map)
    {
        CurrentMap = map;
    }
}