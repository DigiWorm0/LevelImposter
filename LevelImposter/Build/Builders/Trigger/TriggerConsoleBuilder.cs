using LevelImposter.Build.Attributes;
using LevelImposter.Core.Components;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using LevelImposter.DB;
using UnityEngine;

namespace LevelImposter.Build.Builders.Trigger;

public static class TriggerConsoleBuilder
{
    [ElementBuilder(ElementTypes = ["util-triggerconsole"])]
    public static void Build(LIElement element, GameObject gameObject)
    {
        // Prefab
        var prefab = PrefabDB.GetObject("util-computer");
        if (prefab == null)
            return;
        var prefabRenderer = prefab.GetComponent<SpriteRenderer>();

        // Sprite
        var rend = gameObject.GetComponent<SpriteRenderer>();
        gameObject.layer = (int)Layer.ShortObjects;
        if (rend == null)
        {
            LILogger.Warn($"{element.name} is missing a sprite.");
            return;
        }

        rend.material = prefabRenderer.material;

        // Console
        var console = gameObject.AddComponent<TriggerConsole>();
        console.Init(element);

        // Colliders
        gameObject.CreateDefaultColliders(prefab);
    }
}