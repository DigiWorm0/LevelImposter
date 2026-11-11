using System;
using System.Collections.Generic;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using LevelImposter.AssetLoader.Loaders;
using LevelImposter.Build.Attributes;
using LevelImposter.Build.Builders.Sab;
using LevelImposter.Core.Models;
using LevelImposter.Core.Services;
using LevelImposter.Core.Utils;
using LevelImposter.DB;
using UnityEngine;
using UnityEngine.UI;

namespace LevelImposter.Build.Builders.Util;

internal static class DecontaminationBuilder
{
    private const string DECONTAM_SOUND_NAME = "decontamSound";

    private static readonly Dictionary<Guid, LIElement> _deconElemDB = new();
    private static readonly Dictionary<Guid, DeconSystem> _deconSystemDB = new();

    [MapBuilder(Priority = Priority.FIRST)]
    public static void Reset()
    {
        _deconElemDB.Clear();
        _deconSystemDB.Clear();
    }

    [ElementBuilder(ElementTypes = ["util-decontamination"])]
    public static void Build(LIElement element, GameObject gameObject)
    {
        // Prefab
        var prefab = PrefabDB.GetObject(element.type);
        if (prefab == null)
            return;
        var prefabBehaviour = prefab.GetComponent<DeconSystem>();

        // Decontamination
        var deconSystem = gameObject.AddComponent<DeconSystem>();
        deconSystem.SpraySound = prefabBehaviour.SpraySound;
        deconSystem.RoomArea = gameObject.GetComponent<Collider2D>();
        deconSystem.Particles = new Il2CppReferenceArray<ParticleSystem>(0);
        deconSystem.TargetSystem = SystemDistributionService.GetNewDeconSystemType();
        deconSystem.DeconTime = element.properties.deconDuration ?? 3.0f;

        // Sound
        var deconSound = element.properties.sounds.FindSound(DECONTAM_SOUND_NAME);
        if (deconSound != null)
            deconSystem.SpraySound = WAVLoader.Load(deconSound);

        _deconSystemDB.Add(element.id, deconSystem);
        _deconElemDB.Add(element.id, element);

        // Collider
        if (deconSystem.RoomArea != null)
            deconSystem.RoomArea.isTrigger = true;
    }

    [MapBuilder(Priority = Priority.LAST)]
    public static void LinkConsoles()
    {
        // TODO: Move this to OnPostBuild
        // Assign Doors
        foreach (var deconInfo in _deconSystemDB)
        {
            var deconElem = _deconElemDB[deconInfo.Key];
            var deconSystem = deconInfo.Value;

            var doorA = SabDoorBuilder.GetDoor(deconElem.properties.doorA ?? Guid.Empty);
            deconSystem.UpperDoor = doorA;
            if (doorA != null)
            {
                AddDoorConsole(deconSystem, doorA, true, true);
                AddDoorConsole(deconSystem, doorA, true, false);
            }

            var doorB = SabDoorBuilder.GetDoor(deconElem.properties.doorB ?? Guid.Empty);
            deconSystem.LowerDoor = doorB;
            if (doorB != null)
            {
                AddDoorConsole(deconSystem, doorB, false, true);
                AddDoorConsole(deconSystem, doorB, false, false);
            }
        }
    }

    /// <summary>
    ///     Replaces the default door console with a decontamination console
    /// </summary>
    /// <param name="deconSystem">Associated decontamination system</param>
    /// <param name="door">Door object to append to</param>
    /// <param name="isUpper"><c>true</c> if the door is the upper one</param>
    /// <param name="isInner"><c>true</c> if the console is on the inner side of the door</param>
    private static void AddDoorConsole(DeconSystem deconSystem, PlainDoor door, bool isUpper, bool isInner)
    {
        // GameObject
        var doorConsole = new GameObject("DoorConsole");
        doorConsole.transform.SetParent(door.transform);
        var offset = (door.transform.position - deconSystem.transform.position).normalized * 0.2f;
        doorConsole.transform.localPosition = isInner ? -offset : offset;

        // Console Collider
        var consoleCollider = doorConsole.AddComponent<CircleCollider2D>();
        consoleCollider.isTrigger = true;

        // DeconControl
        var deconControl = doorConsole.AddComponent<DeconControl>();
        deconControl.System = deconSystem;
        deconControl.Image = door.GetComponent<SpriteRenderer>();
        deconControl.OnUse = new Button.ButtonClickedEvent();
        if (isInner)
            deconControl.OnUse.AddListener((Action)(() => deconSystem.OpenFromInside(isUpper)));
        else
            deconControl.OnUse.AddListener((Action)(() => deconSystem.OpenDoor(isUpper)));

        // Close Door By Default
        door.Open = true; // Ensure there is a "state change"
        door.SetDoorway(false);
    }
}