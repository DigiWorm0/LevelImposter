using System;
using LevelImposter.Shop;
using UnityEngine;

namespace LevelImposter.Lobby;

// Disable warnings since this indirectly inherits IUsable
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeMadeStatic.Global
#pragma warning disable CA1822

public class LobbyMapConsole(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    private static readonly int OutlineProperty = Shader.PropertyToID("_Outline");
    private static readonly int OutlineColorProperty = Shader.PropertyToID("_OutlineColor");
    private static readonly int AddColorProperty = Shader.PropertyToID("_AddColor");
    
    private readonly Color _highlightColor = Color.white;

    private SpriteRenderer? _spriteRenderer;

    // IUsable
    public float UsableDistance => 1.0f;
    public float PercentCool => 0;
    public ImageNames UseIcon => ImageNames.UseButton;

    public void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void OnDestroy()
    {
        _spriteRenderer = null;
    }

    /// <summary>
    ///     Updates the sprite outline for the consoles
    /// </summary>
    /// <param name="isVisible">TRUE iff the console is within vision</param>
    /// <param name="isTargeted">TRUE iff the console is the main target selected</param>
    public void SetOutline(bool isVisible, bool isTargeted)
    {
        if (_spriteRenderer == null)
            return;

        _spriteRenderer.material.SetFloat(OutlineProperty, isVisible ? 1 : 0);
        _spriteRenderer.material.SetColor(OutlineColorProperty, _highlightColor);
        _spriteRenderer.material.SetColor(AddColorProperty, isTargeted ? _highlightColor : Color.clear);
    }

    /// <summary>
    ///     Checks whether or not the console is usable by a player
    /// </summary>
    /// <param name="playerInfo">Player to check</param>
    /// <param name="canUse">TRUE iff the player can access this console currently</param>
    /// <param name="couldUse">TRUE iff the player could access this console in the future</param>
    /// <returns>Distance from console</returns>
    public float CanUse(NetworkedPlayerInfo playerInfo, out bool canUse, out bool couldUse)
    {
        var playerControl = playerInfo.Object;
        var truePosition = playerControl.GetTruePosition();

        couldUse = playerControl.CanMove && AmongUsClient.Instance.AmHost;
        canUse = couldUse;

        if (couldUse)
        {
            var playerDistance = Vector2.Distance(truePosition, transform.position);
            canUse = couldUse && playerDistance <= UsableDistance;
            return playerDistance;
        }

        return float.MaxValue;
    }

    /// <summary>
    ///     Activates the associated console trigger
    /// </summary>
    public void Use()
    {
        CanUse(PlayerControl.LocalPlayer.Data, out var canUse, out _);
        if (!canUse)
            return;
        
        DestroyableSingleton<TransitionFade>.Instance.DoTransitionFade(null, ShopBuilder.Build(), null);
    }
}
#pragma warning restore CA1822