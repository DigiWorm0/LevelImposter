using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Core.Models;
using LevelImposter.Core.Services.Ship;
using LevelImposter.Core.Utils;
using LevelImposter.Networking.RPC;
using LevelImposter.Trigger;
using Reactor.Networking.Rpc;
using UnityEngine;

namespace LevelImposter.Core.Components;

/// <summary>
///     Object w/ Rigidbody2D that has physics
/// </summary>
public class LIPhysicsObject(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    private const string TRIGGER_ID = "onCollision";
    private const int HOST_UPDATE_INTERVAL = 10; // s

    public static readonly Dictionary<uint, LIPhysicsObject?> AllObjects = new();

    private uint _objectID;

    [HideFromIl2Cpp] public LIElement? Element { get; private set; }
    [HideFromIl2Cpp] public Rigidbody2D? Rigidbody { get; private set; }

    public void Awake()
    {
        Element = MapObjectDB.Get(gameObject);
        Rigidbody = GetComponent<Rigidbody2D>();
    }

    public void Start()
    {
        StartCoroutine(CoUpdatePosAsHost().WrapToIl2Cpp());
    }

    public void OnCollisionEnter2D(Collision2D other)
    {
        // Trigger
        var triggerSignal = new TriggerSignal(gameObject, TRIGGER_ID, PlayerControl.LocalPlayer);
        TriggerSystem.GetInstance().FireTrigger(triggerSignal);

        try
        {
            // Check if we are the collision target
            var otherObject = other.gameObject;
            if (!otherObject.TryGetComponent(out PlayerControl? otherPlayer))
                return;
            if (otherPlayer?.AmOwner ?? false)
                UpdateObjectPosOverRPC();
        }
        catch (Exception)
        {
            // Ignore errors
        }
    }

    /// <summary>
    ///     Assigns the net ID of this physics object and adds it to the list of global objects.
    /// </summary>
    /// <param name="id">The net ID to assign</param>
    public void AssignID(uint id)
    {
        _objectID = id;
        AllObjects[id] = this;
    }

    [HideFromIl2Cpp]
    private IEnumerator CoUpdatePosAsHost()
    {
        while (true)
        {
            yield return new WaitForSeconds(HOST_UPDATE_INTERVAL);
            if (GameState.IsHost && isActiveAndEnabled)
                UpdateObjectPosOverRPC();
        }
    }

    private void UpdateObjectPosOverRPC()
    {
        if (Rigidbody == null)
            throw new Exception("Rigidbody2D is null");

        Rpc<PhysicsObjectRPC>.Instance.Send(PlayerControl.LocalPlayer, new RPCPhysicsObjectPacket
        {
            ObjectID = _objectID,
            X = transform.position.x,
            Y = transform.position.y,
            Rotation = transform.rotation.eulerAngles.z,
            VelocityX = Rigidbody.velocity.x,
            VelocityY = Rigidbody.velocity.y,
            AngularVelocity = Rigidbody.angularVelocity
        }, true);
    }
}