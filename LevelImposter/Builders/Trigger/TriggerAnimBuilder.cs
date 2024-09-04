using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

public class TriggerAnimBuilder : IElemBuilder
{
    public void Build(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-triggeranim")
            return;

        obj.AddComponent<TriggerAnim>();
    }

    public void PostBuild()
    {
    }
}