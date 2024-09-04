using LevelImposter.Core;
using LevelImposter.DB;
using UnityEngine;

namespace LevelImposter.Builders;

internal class PlatformBuilder : IElemBuilder
{
    private const string MOVE_SOUND_NAME = "platformMove";

    // TODO: Support multiple moving platforms in 1 map
    public static MovingPlatformBehaviour? Platform;

    public PlatformBuilder()
    {
        Platform = null;
    }

    public void Build(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-platform")
            return;

        // Singleton
        if (Platform != null)
        {
            LILogger.Warn("Only 1 util-platform should be used per map");
            return;
        }

        // ShipStatus
        var shipStatus = LIShipStatus.GetInstance().ShipStatus;

        // Prefab
        var prefab = AssetDB.GetObject(elem.type);
        if (prefab == null)
            return;
        var prefabBehaviour = prefab.GetComponent<MovingPlatformBehaviour>();

        // Default Sprite
        var spriteRenderer = MapUtils.CloneSprite(obj, prefab);

        // Offsets
        var leftPos = obj.transform.position;
        var leftUsePos = leftPos + new Vector3(
            elem.properties.platformXEntranceOffset ?? -1.5f,
            elem.properties.platformYEntranceOffset ?? 0,
            0
        );
        var rightPos = leftPos + new Vector3(
            elem.properties.platformXOffset ?? 3,
            elem.properties.platformYOffset ?? 0,
            0
        );
        var rightUsePos = rightPos + new Vector3(
            elem.properties.platformXExitOffset ?? 1.5f,
            elem.properties.platformYExitOffset ?? 0,
            0
        );

        // Platform
        var movingPlatform = obj.AddComponent<MovingPlatformBehaviour>();
        movingPlatform.LeftPosition = MapUtils.ScaleZPositionByY(leftPos);
        movingPlatform.RightPosition = MapUtils.ScaleZPositionByY(rightPos);
        movingPlatform.LeftUsePosition = MapUtils.ScaleZPositionByY(leftUsePos);
        movingPlatform.RightUsePosition = MapUtils.ScaleZPositionByY(rightUsePos);
        movingPlatform.IsLeft = true;
        movingPlatform.MovingSound = prefabBehaviour.MovingSound;
        Platform = movingPlatform;

        // ShipStatus
        shipStatus.Systems.Add(SystemTypes.GapRoom, movingPlatform.Cast<ISystemType>());

        // Sound
        var moveSound = MapUtils.FindSound(elem.properties.sounds, MOVE_SOUND_NAME);
        if (moveSound != null)
            movingPlatform.MovingSound = WAVFile.LoadSound(moveSound);

        // Consoles
        GameObject leftObj = new("Left Console");
        leftObj.transform.SetParent(shipStatus.transform);
        leftObj.transform.localPosition = leftUsePos;
        leftObj.AddComponent<BoxCollider2D>().isTrigger = true;
        GameObject rightObj = new("Right Console");
        rightObj.transform.SetParent(shipStatus.transform);
        rightObj.transform.localPosition = rightUsePos;
        rightObj.AddComponent<BoxCollider2D>().isTrigger = true;

        var leftConsole = leftObj.AddComponent<PlatformConsole>();
        leftConsole.Image = spriteRenderer;
        leftConsole.Platform = movingPlatform;
        var rightConsole = rightObj.AddComponent<PlatformConsole>();
        rightConsole.Image = spriteRenderer;
        rightConsole.Platform = movingPlatform;
    }

    public void PostBuild()
    {
    }
}