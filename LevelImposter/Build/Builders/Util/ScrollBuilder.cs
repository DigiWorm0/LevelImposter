using LevelImposter.Build.Attributes;
using LevelImposter.Core.Components;
using UnityEngine;

namespace LevelImposter.Build.Builders.Util;

internal static class ScrollBuilder
{
    [ElementBuilder(ElementTypes = ["util-blankscroll"])]
    public static void Build(GameObject gameObject)
    {
        gameObject.AddComponent<LIScroll>();
    }
}