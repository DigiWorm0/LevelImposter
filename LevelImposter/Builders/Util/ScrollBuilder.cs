using LevelImposter.Core.Components;
using LevelImposter.Core.Models;
using UnityEngine;

namespace LevelImposter.Builders.Util;

internal class ScrollBuilder : IElemBuilder
{
    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-blankscroll")
            return;

        obj.AddComponent<LIScroll>();
    }
}