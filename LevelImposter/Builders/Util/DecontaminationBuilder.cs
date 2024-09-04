using System;
using System.Collections.Generic;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using LevelImposter.Core;
using LevelImposter.DB;
using UnityEngine;
using UnityEngine.UI;

namespace LevelImposter.Builders;

internal class DecontaminationBuilder : IElemBuilder
{
    private const string DECONTAM_SOUND_NAME = "decontamSound";
    private readonly Dictionary<Guid, LIElement> _deconElemDB = new();

    private readonly Dictionary<Guid, DeconSystem> _deconSystemDB = new();

    public void Build(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-decontamination")
            return;

        // Prefab
        var prefab = AssetDB.GetObject(elem.type);
        if (prefab == null)
            return;
        var prefabBehaviour = prefab.GetComponent<DeconSystem>();

        // Decontamination
        var deconSystem = obj.AddComponent<DeconSystem>();
        deconSystem.SpraySound = prefabBehaviour.SpraySound;
        deconSystem.RoomArea = obj.GetComponent<Collider2D>();
        deconSystem.Particles = new Il2CppReferenceArray<ParticleSystem>(0);
        deconSystem.TargetSystem = SystemDistributor.GetNewDeconSystemType();
        deconSystem.DeconTime = elem.properties.deconDuration ?? 3.0f;

        // Sound
        var deconSound = MapUtils.FindSound(elem.properties.sounds, DECONTAM_SOUND_NAME);
        if (deconSound != null)
            deconSystem.SpraySound = WAVFile.LoadSound(deconSound);

        _deconSystemDB.Add(elem.id, deconSystem);
        _deconElemDB.Add(elem.id, elem);

        // Collider
        if (deconSystem.RoomArea != null)
            deconSystem.RoomArea.isTrigger = true;
    }

    public void PostBuild()
    {
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
    private void AddDoorConsole(DeconSystem deconSystem, PlainDoor door, bool isUpper, bool isInner)
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