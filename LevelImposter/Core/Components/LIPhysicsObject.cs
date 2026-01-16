using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Networking;
using LevelImposter.Shop;
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
    
    [HideFromIl2Cpp] public LIElement? Element { get; private set; }
    [HideFromIl2Cpp] public Rigidbody2D? Rigidbody { get; private set; }

    public void Awake()
    {
        _objectID = _objectCounter++;
        
        AllObjects.Add(_objectID, this);

        Element = MapObjectDB.Get(gameObject);
        Rigidbody = GetComponent<Rigidbody2D>();
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
        while (GameConfiguration.CurrentMap != null)
        {
            yield return new WaitForSeconds(HOST_UPDATE_INTERVAL);
            if (GameState.IsHost)
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
        });
    }
}