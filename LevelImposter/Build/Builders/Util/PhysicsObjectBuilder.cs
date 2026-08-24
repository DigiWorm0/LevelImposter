using LevelImposter.Build.Attributes;
using LevelImposter.Core.Components;
using LevelImposter.Core.GarbageCollection;
using LevelImposter.Core.Models;
using UnityEngine;

namespace LevelImposter.Build.Builders.Util;

internal static class PhysicsObjectBuilder
{
    private static bool _isCameraFixed;
    private static uint _objectCounter;

    [MapBuilder(Priority = Priority.FIRST)]
    public static void Reset()
    {
        _isCameraFixed = false;
        _objectCounter = 0;

        // Disable collision between physics objects & UI
        Physics2D.IgnoreLayerCollision((int)Layer.Physics, (int)Layer.UI);
    }

    [ElementBuilder(ElementTypes = ["util-physics"])]
    public static void Build(LIElement element, GameObject gameObject)
    {
        // Add Rigidbody2D
        var rb = gameObject.AddComponent<Rigidbody2D>();
        rb.mass = element.properties.physicsMass ?? 10.0f;
        rb.drag = element.properties.physicsDrag ?? 100.0f;
        rb.angularDrag = element.properties.physicsAngularDrag ?? 100.0f;
        rb.gravityScale = 0;

        // Add Constraints
        var constraints = RigidbodyConstraints2D.None;
        if (element.properties.physicsFreezeX ?? false)
            constraints |= RigidbodyConstraints2D.FreezePositionX;
        if (element.properties.physicsFreezeY ?? false)
            constraints |= RigidbodyConstraints2D.FreezePositionY;
        if (element.properties.physicsFreezeRotation ?? false)
            constraints |= RigidbodyConstraints2D.FreezeRotation;
        rb.constraints = constraints;

        // Create Physics Material
        var physicsMaterial = new PhysicsMaterial2D
        {
            bounciness = element.properties.physicsBounciness ?? 0.6f,
            friction = element.properties.physicsFriction ?? 0.6f
        };
        rb.sharedMaterial = physicsMaterial;
        GCHandler.Register(physicsMaterial);

        // Set Layer
        gameObject.layer = (int)Layer.Physics;

        // Add Physics Object Component
        var physicsObject = gameObject.AddComponent<LIPhysicsObject>();
        physicsObject.AssignID(++_objectCounter);

        // Fix Camera
        if (_isCameraFixed)
            return;

        // Fix Camera to render physics objects
        var camera = Camera.main;
        if (camera != null)
            camera.cullingMask |= 1 << (int)Layer.Physics;

        // Fix Shadow camera to render physics objects
        var shadowCamera = camera?.transform.Find("ShadowCamera")?.GetComponent<Camera>();
        if (shadowCamera != null)
            shadowCamera.cullingMask |= 1 << (int)Layer.Physics;

        _isCameraFixed = true;
    }
}