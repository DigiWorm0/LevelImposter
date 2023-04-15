using HarmonyLib;
using LevelImposter.Core;
using LevelImposter.DB;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace LevelImposter.Builders
{
    class PlatformBuilder : IElemBuilder
    {
        // TODO: Support multiple moving platforms in 1 map
        public static MovingPlatformBehaviour? Platform = null;

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
            var shipStatus = LIShipStatus.Instance?.ShipStatus;
            if (shipStatus == null)
                throw new MissingShipException();

            // Prefab
            var prefab = AssetDB.GetObject(elem.type);
            if (prefab == null)
                return;
            var prefabBehaviour = prefab.GetComponent<MovingPlatformBehaviour>();

            // Default Sprite
            SpriteRenderer spriteRenderer = MapUtils.CloneSprite(obj, prefab);

            // Offsets
            Vector3 leftPos = obj.transform.position;
            Vector3 leftUsePos = leftPos + new Vector3(
                elem.properties.platformXEntranceOffset ?? -1.5f,
                elem.properties.platformYEntranceOffset ?? 0,
                0
            );
            Vector3 rightPos = leftPos + new Vector3(
                elem.properties.platformXOffset ?? 3,
                elem.properties.platformYOffset ?? 0,
                0
            );
            Vector3 rightUsePos = rightPos + new Vector3(
                elem.properties.platformXExitOffset ?? 1.5f,
                elem.properties.platformYExitOffset ?? 0,
                0
            );

            // Platform
            MovingPlatformBehaviour movingPlatform = obj.AddComponent<MovingPlatformBehaviour>();
            movingPlatform.LeftPosition = MapUtils.ScaleZPositionByY(leftPos);
            movingPlatform.RightPosition = MapUtils.ScaleZPositionByY(rightPos);
            movingPlatform.LeftUsePosition = MapUtils.ScaleZPositionByY(leftUsePos);
            movingPlatform.RightUsePosition = MapUtils.ScaleZPositionByY(rightUsePos);
            movingPlatform.IsLeft = true;
            movingPlatform.MovingSound = prefabBehaviour.MovingSound;
            Platform = movingPlatform;

            // Consoles
            GameObject leftObj = new("Left Console");
            leftObj.transform.SetParent(shipStatus.transform);
            leftObj.transform.localPosition = leftUsePos;
            leftObj.AddComponent<BoxCollider2D>().isTrigger = true;
            GameObject rightObj = new("Right Console");
            rightObj.transform.SetParent(shipStatus.transform);
            rightObj.transform.localPosition = rightUsePos;
            rightObj.AddComponent<BoxCollider2D>().isTrigger = true;

            PlatformConsole leftConsole = leftObj.AddComponent<PlatformConsole>();
            leftConsole.Image = spriteRenderer;
            leftConsole.Platform = movingPlatform;
            PlatformConsole rightConsole = rightObj.AddComponent<PlatformConsole>();
            rightConsole.Image = spriteRenderer;
            rightConsole.Platform = movingPlatform;
        }

        public void PostBuild() { }
    }
}