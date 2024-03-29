using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders
{
    class ScrollBuilder : IElemBuilder
    {
        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-blankscroll")
                return;

            LIScroll objScroll = obj.AddComponent<LIScroll>();
            objScroll.Init(elem);
        }

        public void PostBuild() { }
    }
}