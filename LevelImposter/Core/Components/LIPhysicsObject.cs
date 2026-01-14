using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Networking;
using LevelImposter.Trigger;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using UnityEngine;
using UnityEngine.Serialization;

namespace LevelImposter.Core;

/// <summary>
///     Object w/ Rigidbody2D that has physics
/// </summary>
public class LIPhysicsObject(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    private const string TRIGGER_ID = "onCollision";
    private const int HOST_UPDATE_INTERVAL = 10; // s
    
    public static readonly Dictionary<uint, LIPhysicsObject> AllObjects = new();

    private static uint _objectCounter;
    private uint _objectID;
    
    public Rigidbody2D? rb;
    public MapObjectData? liObject;

    public void Awake()
    {
        _objectID = _objectCounter++;
        
        AllObjects.Add(_objectID, this);
        
        rb = GetComponent<Rigidbody2D>();
        liObject = gameObject.GetLIData();
    }

    public void Start()
    {
        StartCoroutine(CoUpdatePosAsHost().WrapToIl2Cpp());
    }

    public void OnDestroy()
    {
        _objectCounter = 0;
        AllObjects.Clear();
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

    [HideFromIl2Cpp]
    private IEnumerator CoUpdatePosAsHost()
    {
        while (GameState.IsCustomMapLoaded)
        {
            yield return new WaitForSeconds(HOST_UPDATE_INTERVAL);
            if (GameState.IsHost)
                UpdateObjectPosOverRPC();
        }
    }

    private void UpdateObjectPosOverRPC()
    {
        if (rb == null)
            throw new Exception("Rigidbody2D is null");
        
        Rpc<PhysicsObjectRPC>.Instance.Send(PlayerControl.LocalPlayer, new RPCPhysicsObjectPacket
        {
            ObjectID = _objectID,
            X = transform.position.x,
            Y = transform.position.y,
            Rotation = transform.rotation.eulerAngles.z,
            VelocityX = rb.velocity.x,
            VelocityY = rb.velocity.y,
            AngularVelocity = rb.angularVelocity
        });
    }
}