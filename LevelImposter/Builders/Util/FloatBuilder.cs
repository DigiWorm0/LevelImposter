using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

internal class FloatBuilder : IElemBuilder
{
    public void Build(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-blankfloat")
            return;

        // Build Floating Parent
        obj.AddComponent<LIFloat>();
    }

    public void PostBuild()
    {
    }
}