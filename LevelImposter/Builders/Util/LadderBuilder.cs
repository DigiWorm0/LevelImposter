using System.Collections.Generic;
using LevelImposter.Core;
using LevelImposter.DB;
using UnityEngine;

namespace LevelImposter.Builders;

internal class LadderBuilder : IElemBuilder
{
    public const float DEFAULT_LADDER_OFFSET = -0.4f;

    public static readonly Dictionary<string, float> DEFAULT_LADDER_HEIGHTS = new()
    {
        { "util-ladder1", 3.0f },
        { "util-ladder2", 1.5f }
    };

    private static readonly List<Ladder> _allLadders = new();

    private byte _ladderID;

    public LadderBuilder()
    {
        _allLadders.Clear();
    }

    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (!elem.type.StartsWith("util-ladder"))
            return;

        // Prefab
        var prefab = AssetDB.GetObject(elem.type);
        if (prefab == null)
            return;
        var topPrefab = prefab.transform.FindChild("LadderTop").GetComponent<Ladder>();
        var bottomPrefab = prefab.transform.FindChild("LadderBottom").GetComponent<Ladder>();

        // Default Sprite
        var spriteRenderer = MapUtils.CloneSprite(obj, prefab);
        
        // Offset
        var heightOffset = elem.properties.ladderOffset ?? DEFAULT_LADDER_OFFSET;

        // Console
        var ladderHeight = elem.properties.ladderHeight ?? DEFAULT_LADDER_HEIGHTS[elem.type];

        GameObject topObj = new("LadderTop");
        topObj.transform.SetParent(obj.transform);
        topObj.transform.localPosition = new Vector3(0, ladderHeight + heightOffset, 0);
        topObj.AddComponent<BoxCollider2D>().isTrigger = true;
        GameObject bottomObj = new("LadderBottom");
        bottomObj.transform.SetParent(obj.transform);
        bottomObj.transform.localPosition = new Vector3(0, -ladderHeight + heightOffset, 0);
        bottomObj.AddComponent<BoxCollider2D>().isTrigger = true;

        var topConsole = topObj.AddComponent<EditableLadderConsole>();
        var bottomConsole = bottomObj.AddComponent<EditableLadderConsole>();
        topConsole.Id = _ladderID++;
        topConsole.IsTop = true;
        topConsole.Destination = bottomConsole;
        topConsole.UseSound = topPrefab.UseSound;
        topConsole.Image = spriteRenderer;
        topConsole.SetCooldownDuration(elem.properties.ladderCooldown ?? 5.0f);
        _allLadders.Add(topConsole);

        bottomConsole.Id = _ladderID++;
        bottomConsole.IsTop = false;
        bottomConsole.Destination = topConsole;
        bottomConsole.UseSound = bottomPrefab.UseSound;
        bottomConsole.Image = spriteRenderer;
        bottomConsole.SetCooldownDuration(elem.properties.ladderCooldown ?? 5.0f);
        _allLadders.Add(bottomConsole);
    }

    public void OnCleanup()
    {
        _allLadders.RemoveAll(ladder => ladder == null);
    }

    /// <summary>
    ///     Trys the find the ladder of specified id
    /// </summary>
    /// <param name="id">ID of the ladder</param>
    /// <param name="ladder">Cooresponding ladder, if found</param>
    /// <returns>TRUE if found</returns>
    public static bool TryGetLadder(byte id, out Ladder? ladder)
    {
        foreach (var l in _allLadders)
            if (l.Id == id)
            {
                ladder = l;
                return true;
            }

        ladder = null;
        return false;
    }
}