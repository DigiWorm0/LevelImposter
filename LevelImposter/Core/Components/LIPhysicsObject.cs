using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Trigger;
using Reactor.Networking.Attributes;
using UnityEngine;

namespace LevelImposter.Core;

/// <summary>
///     Object w/ Rigidbody2D that has physics
/// </summary>
public class LIPhysicsObject(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    private const string TRIGGER_ID = "onCollision";
    private const int HOST_UPDATE_INTERVAL = 10; // s

    private static uint _objectCounter;
    private static readonly Dictionary<uint, LIPhysicsObject> _allObjects = new();
    private MapObjectData? _liObject;

    private uint _objectID;
    private Rigidbody2D? _rb;

    public void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _objectID = _objectCounter++;
        _allObjects.Add(_objectID, this);
        _liObject = gameObject.GetLIData();
    }

    public void Start()
    {
        StartCoroutine(CoUpdatePosAsHost().WrapToIl2Cpp());
    }

    public void OnDestroy()
    {
        _objectCounter = 0;
        _allObjects.Clear();
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
        if (_rb == null)
            throw new Exception("Rigidbody2D is null");
        RPCUpdateObjectPos(
            PlayerControl.LocalPlayer,
            _objectID,
            transform.position.x,
            transform.position.y,
            transform.rotation.eulerAngles.z,
            _rb.velocity.x,
            _rb.velocity.y,
            _rb.angularVelocity
        );
    }

    [MethodRpc((uint)LIRpc.UpdateObjectPos)]
    private static void RPCUpdateObjectPos(
        PlayerControl playerControl,
        uint objectID,
        float x,
        float y,
        float rotation,
        float velocityX,
        float velocityY,
        float angularVelocity
    )
    {
        // Log
        LILogger.Debug($"[RPC] {playerControl.name} updated physics object {objectID}");

        // Get object
        if (!_allObjects.TryGetValue(objectID, out var obj))
            return;

        // Update position
        obj.transform.position = new Vector3(
            x,
            y,
            obj._liObject?.Element.z ?? 0
        );
        obj.transform.position = MapUtils.ScaleZPositionByY(obj.transform.position);
        obj.transform.rotation = Quaternion.Euler(0, 0, rotation);

        // Update velocity
        if (obj._rb == null)
            throw new Exception("Rigidbody2D is null");
        obj._rb.velocity = new Vector2(velocityX, velocityY);
        obj._rb.angularVelocity = angularVelocity;
    }
}