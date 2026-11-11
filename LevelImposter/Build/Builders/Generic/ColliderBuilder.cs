using System.Linq;
using Il2CppSystem.Collections.Generic;
using LevelImposter.Build.Attributes;
using LevelImposter.Core.Models;
using UnityEngine;

namespace LevelImposter.Build.Builders.Generic;

/// <summary>
///     Configures the Collider2D on the GameObject
/// </summary>
internal static class ColliderBuilder
{
    private static readonly string[] ShadowOnlyTypes =
    [
        "util-onewaycollider"
    ];

    [ElementBuilder(Priority = Priority.FIRST)]
    public static void BuildShadows(LIElement element, GameObject gameObject)
    {
        foreach (var colliderData in element.properties.colliders ?? [])
        {
            if (!colliderData.blocksLight)
                continue;

            // Shadow Object
            GameObject shadowObj = new("Shadow " + colliderData.id);
            shadowObj.transform.SetParent(gameObject.transform);
            shadowObj.transform.localPosition = Vector3.zero;
            shadowObj.transform.localRotation = Quaternion.Euler(Vector3.zero);
            shadowObj.transform.localScale = Vector3.one;
            shadowObj.layer = (int)Layer.Shadow;

            var shadowCollider = shadowObj.AddComponent<EdgeCollider2D>();
            shadowCollider.SetPoints(GetPoints(colliderData, colliderData.isSolid));
        }
    }

    [ElementBuilder(Priority = Priority.FIRST)]
    public static void BuildColliders(LIElement element, GameObject gameObject)
    {
        foreach (var colliderData in element.properties.colliders ?? [])
        {
            // Shadow Only
            // (Prevents building the physical barrier that collides w/ players)
            if (ShadowOnlyTypes.Contains(element.type))
                continue;

            // PolygonCollider2D
            if (colliderData.isSolid)
            {
                var collider = gameObject.AddComponent<PolygonCollider2D>();
                collider.pathCount = 1;
                collider.SetPath(0, GetPoints(colliderData));
            }
            // EdgeCollider2D
            else
            {
                var collider = gameObject.AddComponent<EdgeCollider2D>();
                collider.SetPoints(GetPoints(colliderData));
                collider.edgeRadius = 0.05f; // <-- Matches default in-game edge radius
            }
        }
    }

    /// <summary>
    ///     Gets a Vector2[] from a given LICollider.
    ///     The Vector2[] can directly be passed into Collider2D.SetPoints().
    /// </summary>
    /// <param name="collider">The collider to read</param>
    /// <param name="wrap">If true, the list will wrap around to the beginning</param>
    /// <returns>A Vector2[] containing the collider points</returns>
    private static List<Vector2> GetPoints(LICollider collider, bool wrap = false)
    {
        var list = new List<Vector2>(collider.points.Length);
        foreach (var point in collider.points)
            list.Add(new Vector2(point.x, -point.y));
        if (wrap && list.Count > 0)
            list.Add(list[0]);
        return list;
    }
}