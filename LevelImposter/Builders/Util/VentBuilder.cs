using System;
using System.Collections.Generic;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using LevelImposter.Core;
using LevelImposter.DB;
using UnityEngine;
using UnityEngine.Events;

namespace LevelImposter.Builders;

internal class VentBuilder : IElemBuilder
{
    private const string OPEN_SOUND_NAME = "ventOpen";
    private const string MOVE_SOUND_NAME = "ventMove";
    private readonly Dictionary<Guid, Vent> _ventComponentDb = new();

    private readonly Dictionary<int, LIElement> _ventElementDb = new();
    private bool _hasVentSound;
    private int _ventID;

    public void Build(LIElement elem, GameObject obj)
    {
        if (!elem.type.StartsWith("util-vent"))
            return;

        // ShipStatus
        var shipStatus = LIShipStatus.GetInstance().ShipStatus;

        // Prefab
        var prefab = AssetDB.GetObject(elem.type);
        if (prefab == null)
            return;
        var prefabConsole = prefab.GetComponent<VentCleaningConsole>();
        var prefabVent = prefab.GetComponent<Vent>();
        var prefabArrow = prefab.transform.FindChild("Arrow").gameObject;

        // Default Sprite
        var isAnim = elem.type == "util-vent1" || elem.type == "util-vent3";
        var spriteRenderer = MapUtils.CloneSprite(obj, prefab, isAnim);

        // Console
        if (prefabConsole != null)
        {
            var console = obj.AddComponent<VentCleaningConsole>();
            console.Image = spriteRenderer;
            console.ImpostorDiscoveredSound = prefabConsole.ImpostorDiscoveredSound;
            console.TaskTypes = prefabConsole.TaskTypes;
            console.ValidTasks = prefabConsole.ValidTasks;
            if (elem.properties.range != null)
                console.usableDistance = (float)elem.properties.range;
        }

        // Vent
        var vent = obj.AddComponent<Vent>();
        vent.EnterVentAnim = prefabVent.EnterVentAnim;
        vent.ExitVentAnim = prefabVent.ExitVentAnim;
        vent.spreadAmount = prefabVent.spreadAmount;
        vent.spreadShift = prefabVent.spreadShift;
        vent.Offset = prefabVent.Offset;
        vent.Buttons = new Il2CppReferenceArray<ButtonBehavior>(3);
        vent.CleaningIndicators = new Il2CppReferenceArray<GameObject>(0);
        vent.Id = _ventID;

        // Arrows
        var arrowParent = new GameObject($"{obj.name}_arrows");
        arrowParent.transform.SetParent(obj.transform);
        arrowParent.transform.localPosition = Vector3.zero;
        for (var i = 0; i < vent.Buttons.Length; i++)
            GenerateArrow(prefabArrow, vent, i).transform.SetParent(arrowParent.transform);

        // Sounds
        if (!_hasVentSound)
        {
            _hasVentSound = true;

            var openSound = MapUtils.FindSound(elem.properties.sounds, OPEN_SOUND_NAME);
            if (openSound != null)
                shipStatus.VentEnterSound = WAVFile.LoadSound(openSound);

            var moveSound = MapUtils.FindSound(elem.properties.sounds, MOVE_SOUND_NAME);
            if (moveSound != null)
                shipStatus.VentMoveSounds = new Il2CppReferenceArray<AudioClip>(new[]
                {
                    WAVFile.LoadSound(moveSound)
                });
        }

        // Colliders
        MapUtils.CreateDefaultColliders(obj, prefab);

        // DB
        _ventElementDb.Add(_ventID, elem);
        _ventComponentDb.Add(elem.id, vent);
        _ventID++;
    }

    public void PostBuild()
    {
        _ventID = 0;
        _hasVentSound = false;

        foreach (var currentVent in _ventElementDb)
        {
            var ventComponent = GetVentComponent(currentVent.Value.id);
            if (ventComponent == null)
                continue;
            if (currentVent.Value.properties.leftVent != null)
                ventComponent.Left = GetVentComponent((Guid)currentVent.Value.properties.leftVent);
            if (currentVent.Value.properties.middleVent != null)
                ventComponent.Center = GetVentComponent((Guid)currentVent.Value.properties.middleVent);
            if (currentVent.Value.properties.rightVent != null)
                ventComponent.Right = GetVentComponent((Guid)currentVent.Value.properties.rightVent);
        }
    }

    /// <summary>
    ///     Gets a vent component from the vent component db
    /// </summary>
    /// <param name="id">GUID of the vent</param>
    /// <returns>Vent component or null if not found</returns>
    private Vent? GetVentComponent(Guid id)
    {
        var exists = _ventComponentDb.TryGetValue(id, out var ventComponent);
        if (!exists)
            return null;
        return ventComponent;
    }

    /// <summary>
    ///     Generates the vent arrow buttons
    /// </summary>
    /// <param name="arrowPrefab">Prefab to steal from</param>
    /// <param name="vent">Vent target</param>
    /// <param name="dir">Direction to point arrow</param>
    private GameObject GenerateArrow(GameObject arrowPrefab, Vent vent, int dir)
    {
        var cleaningClone = arrowPrefab.transform.FindChild("CleaningIndicator").GetComponent<SpriteRenderer>();
        var arrowCloneSprite = arrowPrefab.GetComponent<SpriteRenderer>();
        var arrowCloneBox = arrowPrefab.GetComponent<BoxCollider2D>();
        GameObject arrowObj = new("Arrow-" + dir);

        // Sprite
        var arrowSprite = arrowObj.AddComponent<SpriteRenderer>();
        arrowSprite.sprite = arrowCloneSprite.sprite;
        arrowSprite.material = arrowCloneSprite.material;
        arrowObj.layer = (int)Layer.UI;

        // Box Collider
        var arrowBox = arrowObj.AddComponent<BoxCollider2D>();
        arrowBox.size = arrowCloneBox.size;
        arrowBox.offset = arrowCloneBox.offset;
        arrowBox.isTrigger = true;

        // Button
        var arrowBtn = arrowObj.AddComponent<ButtonBehavior>();
        arrowBtn.OnMouseOver = new UnityEvent();
        arrowBtn.OnMouseOut = new UnityEvent();

        Action action = dir switch
        {
            0 => vent.ClickRight,
            1 => vent.ClickLeft,
            2 => vent.ClickCenter,
            _ => vent.ClickCenter
        };
        arrowBtn.OnClick.AddListener(action);

        // Transform
        vent.Buttons[dir] = arrowBtn;
        arrowObj.transform.localScale = new Vector3(
            0.4f,
            0.4f,
            1.0f
        );
        arrowObj.active = false;

        // Cleaning Indicator
        var cleaningIndicator = new GameObject("CleaningIndicator");
        cleaningIndicator.transform.SetParent(arrowObj.transform);
        cleaningIndicator.transform.localScale = new Vector3(0.45f, 0.45f, 1.0f);
        cleaningIndicator.transform.localPosition = new Vector3(-0.012f, 0, -0.02f);
        cleaningIndicator.layer = (int)Layer.UI;

        var cleaningIndicatorSprite = cleaningIndicator.AddComponent<SpriteRenderer>();
        cleaningIndicatorSprite.sprite = cleaningClone.sprite;
        cleaningIndicatorSprite.material = cleaningClone.material;
        cleaningIndicator.active = false;
        vent.CleaningIndicators = MapUtils.AddToArr(vent.CleaningIndicators, cleaningIndicator);

        return arrowObj;
    }
}