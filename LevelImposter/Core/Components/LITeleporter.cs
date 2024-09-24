using System;
using System.Collections.Generic;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Networking.Attributes;
using UnityEngine;

namespace LevelImposter.Core;

/// <summary>
///     Object that teleports the player on contact
/// </summary>
public class LITeleporter(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    private static readonly List<LITeleporter> _teleList = new();
    private readonly List<Collider2D> _colliderBuffer = new();

    private bool _preserveOffset = true;

    [HideFromIl2Cpp] public LIElement? CurrentElem { get; private set; }

    public LITeleporter? CurrentTarget { get; private set; }

    public void Awake()
    {
        _teleList.Add(this);
    }

    public void Start()
    {
        if (CurrentElem == null)
            return;
        foreach (var teleporter in _teleList)
        {
            var targetID = CurrentElem.properties.teleporter;
            if (targetID != null)
                CurrentTarget = _teleList.Find(tele => tele.CurrentElem?.id == targetID);
        }
    }

    public void OnDestroy()
    {
        _teleList.Clear();
        CurrentElem = null;
        CurrentTarget = null;
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        _colliderBuffer.Add(collider);
        if (enabled)
            TryTeleport(collider);
    }

    public void OnTriggerExit2D(Collider2D collider)
    {
        _colliderBuffer.RemoveAll(col => col == collider);
    }


    /// <summary>
    ///     Sets the Teleporter's LIElement source
    /// </summary>
    /// <param name="elem">Element to read properties from</param>
    [HideFromIl2Cpp]
    public void SetElement(LIElement elem)
    {
        CurrentElem = elem;
        _preserveOffset = elem.properties.preserveOffset ?? true;
    }

    /// <summary>
    ///     RPC that is ran when the player is teleported
    /// </summary>
    /// <param name="player">PlayerControl that is teleported</param>
    /// <param name="x">Global X position to teleport to</param>
    /// <param name="y">Global Y position to teleport to</param>
    [MethodRpc((uint)LIRpc.TeleportPlayer)]
    public static void RPCTeleport(PlayerControl player, float x, float y)
    {
        LILogger.Info($"Teleported {player.name} to ({x},{y})");
        player.NetTransform.SnapTo(player.transform.position);
    }

    /// <summary>
    ///     Checks if the local player is in the teleporter and teleports them
    /// </summary>
    public void TeleportOnce()
    {
        foreach (var collider in _colliderBuffer)
            if (TryTeleport(collider))
                return;
    }

    /// <summary>
    ///     Teleports the collider to the target teleporter if its the local player
    /// </summary>
    /// <param name="collider">Collider to teleport</param>
    /// <returns>True if the collider was teleported</returns>
    private bool TryTeleport(Collider2D? collider)
    {
        if (CurrentElem == null || CurrentTarget == null || collider == null)
            return false;

        // Check Player
        var player = collider.GetComponent<PlayerControl>();
        if (player == null)
            return false;
        if (!MapUtils.IsLocalPlayer(player.gameObject))
            return false;
        if (collider.TryCast<CircleCollider2D>() == null) // Disable BoxCollider2D
            return false;

        // Offset
        Vector3 offset;
        if (_preserveOffset)
            offset = transform.position - CurrentTarget.transform.position;
        else
            offset = player.transform.position - CurrentTarget.transform.position;
        offset.z = 0;

        // Pet
        var pet = player.cosmetics.currentPet;
        if (pet != null) pet.transform.position -= offset;

        // Player
        player.transform.position -= offset;

        // Camera
        Camera.main.transform.position -= offset;
        var followerCam = Camera.main.GetComponent<FollowerCamera>();
        followerCam.centerPosition = Camera.main.transform.position;

        // RPC
        RPCTeleport(
            player,
            player.transform.position.x,
            player.transform.position.y
        );
        return true;
    }
}