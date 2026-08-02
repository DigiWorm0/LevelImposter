using LevelImposter.Core.Components;
using LevelImposter.Core.Models;
using UnityEngine;

namespace LevelImposter.Builders.Trigger;

public class TriggerAnimBuilder : IElemBuilder
{
    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-triggeranim")
            return;

        obj.AddComponent<TriggerAnim>();
    }
}