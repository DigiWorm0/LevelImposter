using System.Linq;
using Il2CppSystem.Collections.Generic;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

/// <summary>
///     Configures the Collider2D on the GameObject
/// </summary>
public class ColliderBuilder : IElemBuilder
{
    private static readonly string[] ShadowOnlyTypes = [
        "util-onewaycollider"
    ];
    
    public int Priority => IElemBuilder.HIGH_PRIORITY; // <-- Run before other builders that may need colliders

    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (elem.properties.colliders == null)
            return;

        // Iterate through colliders
        foreach (var colliderData in elem.properties.colliders)
        {
            // Shadow Object
            if (colliderData.blocksLight)
            {
                GameObject shadowObj = new("Shadow " + colliderData.id);
                shadowObj.transform.SetParent(obj.transform);
                shadowObj.transform.localPosition = Vector3.zero;
                shadowObj.transform.localRotation = Quaternion.Euler(Vector3.zero);
                shadowObj.transform.localScale = Vector3.one;
                shadowObj.layer = (int)Layer.Shadow;

                var shadowCollider = shadowObj.AddComponent<EdgeCollider2D>();
                shadowCollider.SetPoints(GetPoints(colliderData, colliderData.isSolid));
            }

            // Shadow Only
            if (ShadowOnlyTypes.Contains(elem.type))
                continue;

            // PolygonCollider2D
            if (colliderData.isSolid)
            {
                var collider = obj.AddComponent<PolygonCollider2D>();
                collider.pathCount = 1;
                collider.SetPath(0, GetPoints(colliderData));
            }
            // EdgeCollider2D
            else
            {
                var collider = obj.AddComponent<EdgeCollider2D>();
                collider.SetPoints(GetPoints(colliderData));
            }
        }
    }

    private List<Vector2> GetPoints(LICollider collider, bool wrap = false)
    {
        var list = new List<Vector2>(collider.points.Length);
        foreach (var point in collider.points)
            list.Add(new Vector2(point.x, -point.y));
        if (wrap && list.Count > 0)
            list.Add(list[0]);
        return list;
    }
}