using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

/// <summary>
///     Configures the Transform on the GameObject
/// </summary>
public class TransformBuilder : IElemBuilder
{
    public int Priority => IElemBuilder.HIGH_PRIORITY; // <-- Run before other builders that may need transforms
    
    public void OnBuild(LIElement elem, GameObject obj)
    {
        obj.layer = (int)Layer.Ship;
        obj.transform.localPosition = new Vector3(elem.x, elem.y, elem.z);
        obj.transform.localRotation = Quaternion.Euler(0, 0, elem.rotation);
        obj.transform.localScale = new Vector3(elem.xScale, elem.yScale, 1.0f);
        
        // Scale Z position by Y if not a util-layer
        // Layers will mess up the Z position
        if (elem.type != "util-layer")
            obj.transform.position = MapUtils.ScaleZPositionByY(obj.transform.position);
    }
}