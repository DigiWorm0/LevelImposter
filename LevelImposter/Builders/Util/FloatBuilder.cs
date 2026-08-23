using LevelImposter.Build.Attributes;
using LevelImposter.Core.Components;
using UnityEngine;

namespace LevelImposter.Builders.Util;

internal static class FloatBuilder
{
    [ElementBuilder(ElementTypes = ["util-blankfloat"])]
    public static void Build(GameObject gameObject)
    {
        gameObject.AddComponent<LIFloat>();
    }
}