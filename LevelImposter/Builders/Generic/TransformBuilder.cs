using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

/// <summary>
///     Configures the Transform on the GameObject
/// </summary>
public class TransformBuilder : IElemBuilder
{
    public void OnPreBuild(LIElement elem, GameObject obj)
    {
        obj.layer = (int)Layer.Ship;
        obj.transform.localPosition = MapUtils.ScaleZPositionByY(new Vector3(elem.x, elem.y, elem.z));
        obj.transform.localRotation = Quaternion.Euler(0, 0, elem.rotation);
        obj.transform.localScale = new Vector3(elem.xScale, elem.yScale, 1.0f);
    }
}