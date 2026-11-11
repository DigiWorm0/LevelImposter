using LevelImposter.Build.Attributes;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using UnityEngine;

namespace LevelImposter.Build.Builders.Generic;

/// <summary>
///     Configures the Transform on the GameObject
/// </summary>
public static class TransformBuilder
{
    [ElementBuilder(Priority = Priority.FIRST)]
    public static void ApplyObjectTransform(LIElement element, GameObject gameObject)
    {
        gameObject.layer = (int)Layer.Ship;
        gameObject.transform.localPosition = new Vector3(element.x, element.y, element.z);
        gameObject.transform.localRotation = Quaternion.Euler(0, 0, element.rotation);
        gameObject.transform.localScale = new Vector3(element.xScale, element.yScale, 1.0f);

        // Scale Z position by Y if not a util-layer
        // Layers will mess up the Z position
        gameObject.transform.position = element.type != "util-layer"
            ? gameObject.transform.position.ScaleZPositionByY()
            : new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, 0.0f);
    }
}