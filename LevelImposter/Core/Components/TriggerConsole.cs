using System;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Trigger;
using UnityEngine;

namespace LevelImposter.Core;

public class TriggerConsole(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    public const string TRIGGER_ID = "onUse";
    private bool _ghostsEnabled;
    private Color _highlightColor = Color.yellow;
    private bool _isClientSide;
    private bool _onlyFromBelow;
    private SpriteRenderer? _spriteRenderer;

    public float UsableDistance { get; private set; } = 1.0f;

    public float PercentCool => 0;
    public ImageNames UseIcon => ImageNames.UseButton;

    public void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnDestroy()
    {
        _spriteRenderer = null;
    }

    /// <summary>
    ///     Sets the console's metadata
    /// </summary>
    /// <param name="elem">LIElement the console is attatched to</param>
    [HideFromIl2Cpp]
    public void Init(LIElement elem)
    {
        UsableDistance = elem.properties.range ?? 1.0f;
        _isClientSide = elem.properties.triggerClientSide ?? true;
        _onlyFromBelow = elem.properties.onlyFromBelow ?? false;
        _ghostsEnabled = elem.properties.isGhostEnabled ?? false;
        _highlightColor = elem.properties.highlightColor?.ToUnity() ?? Color.yellow;
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
        _spriteRenderer.material.SetColor("_OutlineColor", _highlightColor);
        _spriteRenderer.material.SetColor("_AddColor", isTargeted ? _highlightColor : Color.clear);
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
        var position = transform.position;

        couldUse = (!playerInfo.IsDead || _ghostsEnabled) &&
                   playerControl.CanMove &&
                   (!_onlyFromBelow || truePosition.y < position.y) &&
                   enabled;
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
        CanUse(PlayerControl.LocalPlayer.Data, out var canUse, out var couldUse);
        if (!canUse)
            return;

        TriggerSignal signal = new(gameObject, TRIGGER_ID, PlayerControl.LocalPlayer);
        if (_isClientSide)
            TriggerSystem.GetInstance().FireTrigger(signal);
        else
            TriggerSystem.GetInstance().FireTriggerRPC(signal);
    }
}