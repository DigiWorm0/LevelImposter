using System;
using System.Collections.Generic;
using LevelImposter.Core;
using LevelImposter.DB;
using PowerTools;
using UnityEngine;

namespace LevelImposter.Builders;

public class SabDoorBuilder : IElemBuilder
{
    private const string OPEN_SOUND_NAME = "doorOpen";
    private const string CLOSE_SOUND_NAME = "doorClose";
    private static readonly Dictionary<Guid, PlainDoor> _doorDB = new();
    private int _doorId;
    private List<Guid>? _specialDoorIDs;

    public SabDoorBuilder()
    {
        _doorDB.Clear();
    }

    public void Build(LIElement elem, GameObject obj)
    {
        if (!elem.type.StartsWith("sab-door"))
            return;

        // ShipStatus
        var liShipStatus = LIShipStatus.GetInstance();
        var shipStatus = liShipStatus.ShipStatus;
        if (shipStatus == null)
            throw new MissingShipException();

        // Special Doors
        if (_specialDoorIDs == null)
        {
            _specialDoorIDs = new List<Guid>();
            var mapElems = liShipStatus.CurrentMap?.elements;
            if (mapElems == null)
                throw new MissingShipException();

            foreach (var mapElem in mapElems)
            {
                if (mapElem.properties.doorA != null)
                    _specialDoorIDs.Add(mapElem.properties.doorA.Value);
                if (mapElem.properties.doorB != null)
                    _specialDoorIDs.Add(mapElem.properties.doorB.Value);
            }
        }

        var isSpecialDoor = _specialDoorIDs.Contains(elem.id);

        // Prefab
        var prefab = AssetDB.GetObject(elem.type);
        if (prefab == null)
            return;
        var prefabRenderer = prefab.GetComponent<SpriteRenderer>();
        var prefabDoor = prefab.GetComponent<PlainDoor>();

        // Default Sprite
        var spriteRenderer = obj.GetComponent<SpriteRenderer>();
        var animator = obj.AddComponent<Animator>();
        var spriteAnim = obj.AddComponent<SpriteAnim>();
        obj.layer = (int)Layer.ShortObjects; // <-- Required for Decontamination Doors
        var isSpriteAnim = false;
        if (!spriteRenderer)
        {
            spriteRenderer = obj.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = prefabRenderer.sprite;
            isSpriteAnim = true;
        }
        else
        {
            spriteRenderer.enabled = false;
            spriteAnim.enabled = false;
            animator.enabled = false;
        }

        spriteRenderer.material = prefabRenderer.material;

        // Dummy Components
        var dummyCollider = obj.AddComponent<BoxCollider2D>();
        dummyCollider.isTrigger = true;
        dummyCollider.enabled = false;

        // Colliders
        Collider2D[] colliders = obj.GetComponentsInChildren<Collider2D>();
        foreach (var collider in colliders)
            collider.enabled = false;

        // Door
        var doorType = elem.properties.doorType;
        var isManualDoor = doorType == "polus" || doorType == "airship";
        PlainDoor? doorComponent = null;
        if (isManualDoor || isSpecialDoor)
        {
            doorComponent = obj.AddComponent<PlainDoor>();
            shipStatus.Systems[SystemTypes.Doors] = new DoorsSystemType().Cast<ISystemType>();
        }
        else
        {
            doorComponent = obj.AddComponent<AutoOpenDoor>();
            shipStatus.Systems[SystemTypes.Doors] = new AutoDoorsSystemType().Cast<ISystemType>();
        }

        doorComponent.Room = isSpecialDoor ? 0 : RoomBuilder.GetParentOrDefault(elem);
        doorComponent.Id = _doorId++;
        doorComponent.myCollider = dummyCollider;
        doorComponent.animator = spriteAnim;
        doorComponent.OpenSound = prefabDoor.OpenSound;
        doorComponent.CloseSound = prefabDoor.CloseSound;

        // Add to DB
        _doorDB.Add(elem.id, doorComponent);
        if (!isSpecialDoor)
            shipStatus.AllDoors = MapUtils.AddToArr(shipStatus.AllDoors, doorComponent);

        // Sound
        var openSound = MapUtils.FindSound(elem.properties.sounds, OPEN_SOUND_NAME);
        if (openSound != null)
            doorComponent.OpenSound = WAVFile.LoadSound(openSound);

        var closeSound = MapUtils.FindSound(elem.properties.sounds, CLOSE_SOUND_NAME);
        if (closeSound != null)
            doorComponent.CloseSound = WAVFile.LoadSound(closeSound);

        // SpriteAnim
        if (isSpriteAnim)
        {
            doorComponent.OpenDoorAnim = prefabDoor.OpenDoorAnim;
            doorComponent.CloseDoorAnim = prefabDoor.CloseDoorAnim;
        }

        // Console
        var isInteractable = elem.properties.isDoorInteractable ?? true;
        if (isManualDoor && isInteractable && !isSpecialDoor)
        {
            // Prefab
            var prefab2 = AssetDB.GetObject($"sab-door-{doorType}"); // "sab-door-polus" or "sab-door-airship"
            var prefab2Console = prefab2?.GetComponent<DoorConsole>();

            // Object
            var doorConsole = new GameObject(obj.name + "_Console");
            doorConsole.transform.position = obj.transform.position;
            doorConsole.layer = (int)Layer.Objects;

            // Console
            var consoleComponent = doorConsole.AddComponent<DoorConsole>();
            consoleComponent.MinigamePrefab = prefab2Console?.MinigamePrefab;
            consoleComponent.MyDoor = doorComponent;
            consoleComponent.Image = spriteRenderer;

            // Colliders
            MapUtils.CreateDefaultColliders(doorConsole, obj);
        }

        // Set Default State
        var isDoorClosed = elem.properties.isDoorClosed ?? false;

        doorComponent.Start(); // <-- Run initialization tasks
        doorComponent.SetDoorway(!isDoorClosed);
    }

    public void PostBuild()
    {
    }

    /// <summary>
    ///     Gets a door component by its element ID.
    /// </summary>
    /// <param name="elementID">Element ID of the door object</param>
    /// <returns><c>PlainDoor</c> component of the object or <c>null</c> if not found</returns>
    public static PlainDoor? GetDoor(Guid elementID)
    {
        return _doorDB.GetValueOrDefault(elementID);
    }
}