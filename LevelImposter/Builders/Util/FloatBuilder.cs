using LevelImposter.Core.Components;
using LevelImposter.Core.Models;
using UnityEngine;

namespace LevelImposter.Builders.Util;

internal class FloatBuilder : IElemBuilder
{
    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-blankfloat")
            return;

        // Build Floating Parent
        obj.AddComponent<LIFloat>();
    }
}