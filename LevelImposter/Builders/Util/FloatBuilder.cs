using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

internal class FloatBuilder : IElemBuilder
{
    public void Build(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-blankfloat")
            return;

        var objFloat = obj.AddComponent<LIFloat>();
        objFloat.Init(elem);
    }

    public void PostBuild()
    {
    }
}