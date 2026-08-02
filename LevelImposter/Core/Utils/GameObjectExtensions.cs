using System;
using LevelImposter.Core.Models;
using PowerTools;
using UnityEngine;

namespace LevelImposter.Core.Utils;

public static class GameObjectExtensions
{
    /// <summary>
    ///     Equivelent of <c>GameObject.GetComponent</c> but throws an exception if the component is null or missing.
    /// </summary>
    /// <typeparam name="T">Type of component to get</typeparam>
    /// <param name="gameObject">GameObject to search</param>
    /// <returns>Cooresponding component, never null.</returns>
    /// <exception cref="Exception">If the component is null or missing</exception>
    public static T GetComponentOrThrow<T>(this GameObject gameObject) where T : Component
    {
        var component = gameObject.GetComponent<T>();
        if (component == null)
            throw new Exception($"{gameObject} is missing {typeof(T).FullName}");
        return component;
    }

    /// <summary>
    ///     Recursively sets the layer of a GameObject and all of its children.
    /// </summary>
    /// <param name="gameObject">Parent GameObject</param>
    /// <param name="layer">Layer to set to</param>
    public static void SetLayerOfChildren(this GameObject gameObject, int layer)
    {
        gameObject.layer = layer;
        for (var i = 0; i < gameObject.transform.childCount; i++)
            gameObject.transform.GetChild(i).gameObject.SetLayerOfChildren(layer);
    }

    /// <summary>
    ///     Gets a component of type T from the GameObject.
    ///     If the component does not exist, it is added.
    /// </summary>
    /// <param name="gameObject">The GameObject to get or add the component to.</param>
    /// <typeparam name="T">The type of component to get or add.</typeparam>
    /// <returns>The existing or newly added component of type T.</returns>
    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        var component = gameObject.GetComponent<T>();
        if (component == null)
            component = gameObject.AddComponent<T>();
        return component;
    }

    /// <summary>
    ///     Clones the colliders from a Unity GameObject to another
    /// </summary>
    /// <param name="from">Source GameObject</param>
    /// <param name="to">Target GameObject</param>
    public static void CloneColliders(this GameObject from, GameObject to)
    {
        if (from.GetComponent<CircleCollider2D>() != null)
        {
            var origBox = from.GetComponent<CircleCollider2D>();
            var box = to.AddComponent<CircleCollider2D>();
            box.radius = origBox.radius;
            box.offset = origBox.offset;
            box.isTrigger = true;
        }

        if (from.GetComponent<BoxCollider2D>() != null)
        {
            var origBox = from.GetComponent<BoxCollider2D>();
            var box = to.AddComponent<BoxCollider2D>();
            box.size = origBox.size;
            box.offset = origBox.offset;
            box.isTrigger = true;
        }

        if (from.GetComponent<PolygonCollider2D>() != null)
        {
            var origBox = from.GetComponent<PolygonCollider2D>();
            var box = to.AddComponent<PolygonCollider2D>();
            box.points = origBox.points;
            box.pathCount = origBox.pathCount;
            box.offset = origBox.offset;
            box.isTrigger = true;
        }
    }

    /// <summary>
    ///     Either grabs solid colliders from
    ///     prefab or creates new ones. Used
    ///     for any UI buttons or in-game consoles.
    /// </summary>
    /// <param name="src">Object to set colliders</param>
    /// <param name="prefab">Prefab to copy colliders from</param>
    public static Collider2D CreateDefaultColliders(this GameObject src, GameObject prefab)
    {
        PolygonCollider2D[] solidColliders = src.GetComponentsInChildren<PolygonCollider2D>();
        if (solidColliders.Length <= 0)
            prefab.CloneColliders(src);

        var collider = src.GetComponent<Collider2D>();
        if (collider == null)
            collider = src.AddComponent<BoxCollider2D>();
        return collider;
    }


    /// <summary>
    ///     Clones the sprite from a prefab if the
    ///     object does not already have one.
    /// </summary>
    /// <param name="obj">Object to append sprite to</param>
    /// <param name="prefab">Prefab to clone sprite from</param>
    /// <param name="isSpriteAnim">TRUE if it should clone SpriteAnim components too</param>
    /// <returns>obj's SpriteRenderer</returns>
    public static SpriteRenderer CloneSprite(this GameObject obj, GameObject? prefab, bool isSpriteAnim = false)
    {
        var prefabRenderer = prefab?.GetComponentInChildren<SpriteRenderer>(true);
        if (!prefabRenderer)
            throw new Exception("Failed to get SpriteRenderer from prefab");

        var spriteRenderer = obj.GetComponentInChildren<SpriteRenderer>();
        if (!spriteRenderer)
        {
            spriteRenderer = obj.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = prefabRenderer?.sprite;

            if (isSpriteAnim)
            {
                var prefabAnim = prefab?.GetComponentInChildren<SpriteAnim>();
                if (prefabAnim == null)
                    throw new Exception("Failed to get SpriteAnim from prefab");

                var spriteAnim = obj.AddComponent<SpriteAnim>();
                spriteAnim.m_defaultAnim = prefabAnim.m_defaultAnim;
                spriteAnim.m_speed = prefabAnim.m_speed;
                spriteAnim.Play(prefabAnim.m_defaultAnim, prefabAnim.m_speed);
            }
        }

        spriteRenderer.material = prefabRenderer?.material;
        obj.layer = (int)Layer.ShortObjects;
        return spriteRenderer;
    }
}