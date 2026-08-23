using System;
using System.Collections.Generic;
using LevelImposter.AssetLoader;
using LevelImposter.Build;
using LevelImposter.Build.Attributes;
using LevelImposter.Builders.Util;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using LevelImposter.DB;
using PowerTools;
using UnityEngine;

namespace LevelImposter.Builders.Sab;

internal static class SabDoorBuilder
{
    private const string OPEN_SOUND_NAME = "doorOpen";
    private const string CLOSE_SOUND_NAME = "doorClose";

    private static readonly Dictionary<Guid, PlainDoor> DoorDB = new();

    private static int _doorId;
    private static List<Guid>? _specialDoorIDs;

    [MapBuilder(Priority = Priority.FIRST)]
    public static void Reset()
    {
        DoorDB.Clear();
        _doorId = 0;
        _specialDoorIDs = null;
    }

    [ElementBuilder(
        Target = MapTarget.Game,
        ElementTypes = ["sab-doorh", "sab-doorv"]
    )]
    public static void Build(ShipStatus shipStatus, LIElement element, GameObject gameObject)
    {
        // Special Doors
        if (_specialDoorIDs == null)
        {
            _specialDoorIDs = [];
            var mapElems = GameConfiguration.CurrentMap?.elements;
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

        var isSpecialDoor = _specialDoorIDs.Contains(element.id);

        // Prefab
        var prefab = PrefabDB.GetObject(element.type);
        if (prefab == null)
            return;
        var prefabRenderer = prefab.GetComponent<SpriteRenderer>();
        var prefabDoor = prefab.GetComponent<PlainDoor>();

        // Default Sprite
        var spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        var animator = gameObject.AddComponent<Animator>();
        var spriteAnim = gameObject.AddComponent<SpriteAnim>();
        gameObject.layer = (int)Layer.ShortObjects; // <-- Required for Decontamination Doors
        var isSpriteAnim = false;
        if (!spriteRenderer)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
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
        var dummyCollider = gameObject.AddComponent<BoxCollider2D>();
        dummyCollider.isTrigger = true;
        dummyCollider.enabled = false;

        // Colliders
        Collider2D[] colliders = gameObject.GetComponentsInChildren<Collider2D>();
        foreach (var collider in colliders)
            collider.enabled = false;

        // Door
        var doorType = element.properties.doorType;
        var isManualDoor = doorType == "polus" || doorType == "airship";
        PlainDoor? doorComponent;
        if (isManualDoor || isSpecialDoor)
        {
            doorComponent = gameObject.AddComponent<PlainDoor>();
            shipStatus.Systems[SystemTypes.Doors] = new DoorsSystemType().Cast<ISystemType>();
        }
        else
        {
            doorComponent = gameObject.AddComponent<AutoOpenDoor>();
            shipStatus.Systems[SystemTypes.Doors] = new AutoDoorsSystemType().Cast<ISystemType>();
        }

        doorComponent.Room = isSpecialDoor ? 0 : RoomBuilder.GetParentOrDefault(element);
        doorComponent.Id = _doorId++;
        doorComponent.myCollider = dummyCollider;
        doorComponent.animator = spriteAnim;
        doorComponent.OpenSound = prefabDoor.OpenSound;
        doorComponent.CloseSound = prefabDoor.CloseSound;

        // Add to DB
        DoorDB.Add(element.id, doorComponent);
        if (!isSpecialDoor)
            shipStatus.AllDoors = shipStatus.AllDoors.Add(doorComponent);

        // Load Sounds
        var openSound = element.properties.sounds.FindSound(OPEN_SOUND_NAME);
        if (openSound != null)
            AudioLoader.LoadAsync(
                openSound.dataID,
                loadedSound => doorComponent.OpenSound = loadedSound);

        var closeSound = element.properties.sounds.FindSound(CLOSE_SOUND_NAME);
        if (closeSound != null)
            AudioLoader.LoadAsync(
                closeSound.dataID,
                loadedSound => doorComponent.CloseSound = loadedSound);

        // SpriteAnim
        if (isSpriteAnim)
        {
            doorComponent.OpenDoorAnim = prefabDoor.OpenDoorAnim;
            doorComponent.CloseDoorAnim = prefabDoor.CloseDoorAnim;
        }

        // Console
        var isInteractable = element.properties.isDoorInteractable ?? true;
        if (isManualDoor && isInteractable && !isSpecialDoor)
        {
            // Prefab
            var prefab2 = PrefabDB.GetObject($"sab-door-{doorType}"); // "sab-door-polus" or "sab-door-airship"
            var prefab2Console = prefab2?.GetComponent<DoorConsole>();

            // Object
            var doorConsole = new GameObject(gameObject.name + "_Console");
            doorConsole.transform.position = gameObject.transform.position;
            doorConsole.layer = (int)Layer.Objects;

            // Console
            var consoleComponent = doorConsole.AddComponent<DoorConsole>();
            consoleComponent.MinigamePrefab = prefab2Console?.MinigamePrefab;
            consoleComponent.MyDoor = doorComponent;
            consoleComponent.Image = spriteRenderer;

            // Colliders
            doorConsole.CreateDefaultColliders(gameObject);
        }

        // Set Default State
        var isDoorClosed = element.properties.isDoorClosed ?? false;

        doorComponent.Start(); // <-- Run initialization tasks
        doorComponent.SetDoorway(!isDoorClosed);
    }

    /// <summary>
    ///     Gets a door component by its element ID.
    /// </summary>
    /// <param name="elementID">Element ID of the door object</param>
    /// <returns><c>PlainDoor</c> component of the object or <c>null</c> if not found</returns>
    public static PlainDoor? GetDoor(Guid elementID)
    {
        return DoorDB.GetValueOrDefault(elementID);
    }
}