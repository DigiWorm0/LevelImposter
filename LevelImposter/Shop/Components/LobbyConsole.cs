using System;
using UnityEngine;

namespace LevelImposter.Shop;

public class LobbyConsole(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    private readonly Color HIGHLIGHT_COLOR = Color.white;

    private SpriteRenderer? _spriteRenderer;

    // IUsable
    public float UsableDistance => 1.0f;
    public float PercentCool => 0;
    public ImageNames UseIcon => ImageNames.UseButton;

    public void OnDestroy()
    {
        _spriteRenderer = null;
    }

    /// <summary>
    ///     Sets the sprite renderer for the console
    /// </summary>
    /// <param name="renderer">Sprite renderer to use</param>
    public void SetRenderer(SpriteRenderer renderer)
    {
        _spriteRenderer = renderer;
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

        _spriteRenderer.material.SetFloat("_Outline", isVisible ? 1 : 0);
        _spriteRenderer.material.SetColor("_OutlineColor", HIGHLIGHT_COLOR);
        _spriteRenderer.material.SetColor("_AddColor", isTargeted ? HIGHLIGHT_COLOR : Color.clear);
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