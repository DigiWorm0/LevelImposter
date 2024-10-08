using System;
using System.Collections.Generic;
using UnityEngine;

namespace LevelImposter.Core;

/// <summary>
///     Object that teleports the player on contact
/// </summary>
public class LITeleporter(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    private static readonly List<LITeleporter> _teleList = new();
    private readonly List<Collider2D> _colliderBuffer = new();

    private bool _clientSide = true;
    private LIElement? _element;
    private bool _preserveOffset = true;
    private LITeleporter? _targetTeleporter;

    public void Awake()
    {
        _teleList.Add(this);
    }

    public void Start()
    {
        _element = gameObject.GetLIData().Element;
        _preserveOffset = _element.properties.preserveOffset ?? true;
        _clientSide = _element.properties.triggerClientSide ?? false;

        var targetID = _element.properties.teleporter;
        if (targetID != null)
            _targetTeleporter = _teleList.Find(tele => tele._element?.id == targetID);
    }

    public void OnDestroy()
    {
        _teleList.Clear();
        _targetTeleporter = null;
        _element = null;
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
    ///     Checks if the local player is in the teleporter and teleports them
    /// </summary>
    public void TeleportOnce()
    {
        foreach (var collider in _colliderBuffer)
            if (TryTeleport(collider))
                return;
    }

    /// <summary>
    ///     Teleports the collider to the target teleporter if it's the local player
    /// </summary>
    /// <param name="collider">Collider to teleport</param>
    /// <returns>True if the collider was teleported</returns>
    private bool TryTeleport(Collider2D? collider)
    {
        if (_targetTeleporter == null || collider == null)
            return false;

        // Find the player
        var player = collider.GetComponent<PlayerControl>();
        if (player == null)
            return false;

        // Only teleport the local player if server-side
        if (!_clientSide && !player.AmOwner)
            return false;

        // Disable the BoxCollider2D which is around the player
        if (collider.TryCast<CircleCollider2D>() == null)
            return false;

        // Calculate offset
        Vector3 offset;
        if (_preserveOffset)
            offset = transform.position - _targetTeleporter.transform.position;
        else
            offset = player.transform.position - _targetTeleporter.transform.position;
        offset.z = 0;

        // Pet
        var pet = player.cosmetics.currentPet;
        if (pet != null)
            pet.transform.position -= offset;

        // Player
        player.transform.position -= offset;

        // Camera
        if (Camera.main != null)
        {
            Camera.main.transform.position -= offset;

            var followerCam = Camera.main.gameObject.GetComponent<FollowerCamera>();
            if (followerCam != null)
                followerCam.centerPosition = Camera.main.transform.position;
        }

        // RPC
        if (_clientSide)
            player.NetTransform.SnapTo(player.transform.position);
        else
            player.NetTransform.RpcSnapTo(player.transform.position);

        return true;
    }
}