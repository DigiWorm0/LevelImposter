using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

internal class ScrollBuilder : IElemBuilder
{
    public void Build(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-blankscroll")
            return;

        obj.AddComponent<LIScroll>();
    }

    public void PostBuild()
    {
    }
}