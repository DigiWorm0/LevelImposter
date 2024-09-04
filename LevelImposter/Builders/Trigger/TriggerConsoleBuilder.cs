using LevelImposter.Core;
using LevelImposter.DB;
using UnityEngine;

namespace LevelImposter.Builders;

public class TriggerConsoleBuilder : IElemBuilder
{
    public void Build(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-triggerconsole")
            return;

        // Prefab
        var prefab = AssetDB.GetObject("util-computer");
        if (prefab == null)
            return;
        var prefabRenderer = prefab.GetComponent<SpriteRenderer>();

        // Sprite
        var rend = obj.GetComponent<SpriteRenderer>();
        obj.layer = (int)Layer.ShortObjects;
        if (rend == null)
        {
            LILogger.Warn($"{elem.name} is missing a sprite.");
            return;
        }

        rend.material = prefabRenderer.material;

        // Console
        var console = obj.AddComponent<TriggerConsole>();
        console.Init(elem);

        // Colliders
        MapUtils.CreateDefaultColliders(obj, prefab);
    }

    public void PostBuild()
    {
    }
}