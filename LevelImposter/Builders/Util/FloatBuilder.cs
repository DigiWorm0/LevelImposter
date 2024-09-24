using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

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