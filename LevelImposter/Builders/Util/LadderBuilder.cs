using System.Collections.Generic;
using LevelImposter.Build.Attributes;
using LevelImposter.Core.Components;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using LevelImposter.DB;
using UnityEngine;

namespace LevelImposter.Builders.Util;

internal static class LadderBuilder
{
    private const float DEFAULT_LADDER_OFFSET = -0.4f;

    private static readonly List<Ladder> AllLadders = [];

    private static readonly Dictionary<string, float> DefaultLadderHeights = new()
    {
        { "util-ladder1", 3.0f },
        { "util-ladder2", 1.5f }
    };

    private static byte _ladderID;

    [MapBuilder(Priority = Priority.FIRST)]
    public static void Reset()
    {
        AllLadders.Clear();
        _ladderID = 0;
    }

    [ElementBuilder(ElementTypes = ["util-ladder1", "util-ladder2"])]
    public static void Build(LIElement element, GameObject gameObject)
    {
        // Prefab
        var prefab = PrefabDB.GetObject(element.type);
        if (prefab == null)
            return;
        var topPrefab = prefab.transform.FindChild("LadderTop").GetComponent<Ladder>();
        var bottomPrefab = prefab.transform.FindChild("LadderBottom").GetComponent<Ladder>();

        // Default Sprite
        var spriteRenderer = gameObject.CloneSprite(prefab);

        // Offset
        var heightOffset = element.properties.ladderOffset ?? DEFAULT_LADDER_OFFSET;

        // Console
        var ladderHeight = element.properties.ladderHeight ?? DefaultLadderHeights[element.type];

        GameObject topObj = new("LadderTop");
        topObj.transform.SetParent(gameObject.transform);
        topObj.transform.localPosition = new Vector3(0, ladderHeight + heightOffset, 0);
        topObj.AddComponent<BoxCollider2D>().isTrigger = true;
        GameObject bottomObj = new("LadderBottom");
        bottomObj.transform.SetParent(gameObject.transform);
        bottomObj.transform.localPosition = new Vector3(0, -ladderHeight + heightOffset, 0);
        bottomObj.AddComponent<BoxCollider2D>().isTrigger = true;

        var topConsole = topObj.AddComponent<EditableLadderConsole>();
        var bottomConsole = bottomObj.AddComponent<EditableLadderConsole>();
        topConsole.Id = _ladderID++;
        topConsole.IsTop = true;
        topConsole.Destination = bottomConsole;
        topConsole.UseSound = topPrefab.UseSound;
        topConsole.Image = spriteRenderer;
        topConsole.SetCooldownDuration(element.properties.ladderCooldown ?? 5.0f);
        AllLadders.Add(topConsole);

        bottomConsole.Id = _ladderID++;
        bottomConsole.IsTop = false;
        bottomConsole.Destination = topConsole;
        bottomConsole.UseSound = bottomPrefab.UseSound;
        bottomConsole.Image = spriteRenderer;
        bottomConsole.SetCooldownDuration(element.properties.ladderCooldown ?? 5.0f);
        AllLadders.Add(bottomConsole);
    }

    [MapBuilder(Priority = Priority.LAST)]
    public static void CleanupDestroyedLadders()
    {
        AllLadders.RemoveAll(ladder => ladder == null);
    }

    /// <summary>
    ///     Trys the find the ladder of specified id
    /// </summary>
    /// <param name="id">ID of the ladder</param>
    /// <param name="ladder">Cooresponding ladder, if found</param>
    /// <returns>TRUE if found</returns>
    public static bool TryGetLadder(byte id, out Ladder? ladder)
    {
        ladder = AllLadders.Find(l => l.Id == id);
        return ladder != null;
    }
}