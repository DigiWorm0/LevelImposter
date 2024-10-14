using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

internal class PhysicsObjectBuilder : IElemBuilder
{
    private bool _isCameraFixed;

    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-physics")
            return;

        // Add Rigidbody2D
        var rb = obj.AddComponent<Rigidbody2D>();
        rb.mass = elem.properties.physicsMass ?? 10.0f;
        rb.drag = elem.properties.physicsDrag ?? 100.0f;
        rb.angularDrag = elem.properties.physicsAngularDrag ?? 100.0f;
        rb.freezeRotation = elem.properties.physicsFreezeRotation ?? false;
        rb.gravityScale = 0;

        // Add Constraints
        var constraints = RigidbodyConstraints2D.None;
        if (elem.properties.physicsFreezeX ?? false)
            constraints |= RigidbodyConstraints2D.FreezePositionX;
        if (elem.properties.physicsFreezeY ?? false)
            constraints |= RigidbodyConstraints2D.FreezePositionY;
        rb.constraints = constraints;

        // Create Physics Material
        var physicsMaterial = new PhysicsMaterial2D
        {
            bounciness = elem.properties.physicsBounciness ?? 0.6f,
            friction = elem.properties.physicsFriction ?? 0.6f
        };
        rb.sharedMaterial = physicsMaterial;

        // Set Layer
        obj.layer = (int)Layer.Physics;

        // Add Physics Object Component
        obj.AddComponent<LIPhysicsObject>();

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