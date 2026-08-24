using LevelImposter.AssetLoader.Loaders;
using LevelImposter.Build.Attributes;
using LevelImposter.Core.Components;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using LevelImposter.DB;
using UnityEngine;

namespace LevelImposter.Builders.Util;

internal static class PlatformBuilder
{
    private const string MOVE_SOUND_NAME = "platformMove";

    // TODO: Support multiple moving platforms in 1 map
    public static MovingPlatformBehaviour? Platform;

    [ElementBuilder(ElementTypes = ["util-platform"])]
    public static void Build(LIElement element, GameObject gameObject)
    {
        // Singleton
        if (Platform != null)
        {
            LILogger.Warn("Only 1 util-platform should be used per map");
            return;
        }

        // ShipStatus
        var shipStatus = LIShipStatus.GetInstance().ShipStatus;

        // Prefab
        var prefab = PrefabDB.GetObject(element.type);
        if (prefab == null)
            return;
        var prefabBehaviour = prefab.GetComponent<MovingPlatformBehaviour>();

        // Default Sprite
        var spriteRenderer = gameObject.CloneSprite(prefab);

        // Offsets
        var leftPos = gameObject.transform.localPosition;
        var leftUsePos = GetOffsetFromTransform(gameObject.transform, new Vector3(
            element.properties.platformXEntranceOffset ?? -1.5f,
            element.properties.platformYEntranceOffset ?? 0,
            0
        ));

        var rightPos = GetOffsetFromTransform(gameObject.transform, new Vector3(
            element.properties.platformXOffset ?? 3.0f,
            element.properties.platformYOffset ?? 0,
            0
        ));

        var rightUsePos = GetOffsetFromTransform(gameObject.transform, new Vector3(
            (element.properties.platformXExitOffset ?? 1.5f) + (element.properties.platformXOffset ?? 3.0f),
            (element.properties.platformYExitOffset ?? 0) + (element.properties.platformYOffset ?? 0),
            0
        ));

        // Platform
        var movingPlatform = gameObject.AddComponent<MovingPlatformBehaviour>();
        movingPlatform.LeftPosition = leftPos.ScaleZPositionByY();
        movingPlatform.RightPosition = rightPos.ScaleZPositionByY();
        movingPlatform.LeftUsePosition = leftUsePos.ScaleZPositionByY();
        movingPlatform.RightUsePosition = rightUsePos.ScaleZPositionByY();
        movingPlatform.IsLeft = true;
        movingPlatform.MovingSound = prefabBehaviour.MovingSound;
        Platform = movingPlatform;

        // ShipStatus
        shipStatus?.Systems.Add(SystemTypes.GapRoom, movingPlatform.Cast<ISystemType>());

        // Sound
        var moveSound = element.properties.sounds.FindSound(MOVE_SOUND_NAME);
        if (moveSound != null)
            movingPlatform.MovingSound = WAVLoader.Load(moveSound);

        // Consoles
        GameObject leftObj = new("Left Console");
        leftObj.transform.SetParent(gameObject.transform.parent);
        leftObj.transform.localPosition = leftUsePos;
        leftObj.AddComponent<BoxCollider2D>().isTrigger = true;

        GameObject rightObj = new("Right Console");
        rightObj.transform.SetParent(gameObject.transform.parent);
        rightObj.transform.localPosition = rightUsePos;
        rightObj.AddComponent<BoxCollider2D>().isTrigger = true;

        var leftConsole = leftObj.AddComponent<PlatformConsole>();
        leftConsole.Image = spriteRenderer;
        leftConsole.Platform = movingPlatform;

        var rightConsole = rightObj.AddComponent<PlatformConsole>();
        rightConsole.Image = spriteRenderer;
        rightConsole.Platform = movingPlatform;
    }

    [MapBuilder(Priority = Priority.FIRST)]
    public static void Reset()
    {
        Platform = null;
    }

    private static Vector3 GetOffsetFromTransform(Transform transform, Vector3 offset)
    {
        return transform.parent.InverseTransformPoint(transform.TransformPoint(offset));
    }
}