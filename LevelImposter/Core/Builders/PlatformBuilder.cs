using HarmonyLib;
using LevelImposter.DB;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace LevelImposter.Core
{
    class PlatformBuilder : IElemBuilder
    {
        // TODO: Support multiple moving platforms in 1 map
        public static MovingPlatformBehaviour Platform = null;

        public PlatformBuilder()
        {
            Platform = null;
        }

        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-platform")
                return;
            if (Platform != null)
            {
                LILogger.Warn("Only 1 util-platform should be used per map");
                return;
            }

            UtilData utilData = AssetDB.Utils[elem.type];

            // Default Sprite
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            obj.layer = (int)Layer.ShortObjects;
            if (!spriteRenderer)
            {
                spriteRenderer = obj.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = utilData.SpriteRenderer.sprite;
                if (elem.properties.color != null)
                    spriteRenderer.color = MapUtils.LIColorToColor(elem.properties.color);
            }
            spriteRenderer.material = utilData.SpriteRenderer.material;

            // Offsets
            Vector2 leftPos = obj.transform.position;
            Vector2 leftUsePos = new Vector2(
                (elem.properties.platformXEntranceOffset == null ? -1.5f : (float)elem.properties.platformXEntranceOffset) + leftPos.x,
                (elem.properties.platformYEntranceOffset == null ? 0 : (float)elem.properties.platformYEntranceOffset) + leftPos.y
            );
            Vector2 rightPos = new Vector2(
                (elem.properties.platformXOffset == null ? 3 : (float)elem.properties.platformXOffset) + leftPos.x,
                (elem.properties.platformYOffset == null ? 0 : (float)elem.properties.platformYOffset) + leftPos.y
            );
            Vector2 rightUsePos = new Vector2(
                (elem.properties.platformXExitOffset == null ? 1.5f : (float)elem.properties.platformXExitOffset) + rightPos.x,
                (elem.properties.platformYExitOffset == null ? 0 : (float)elem.properties.platformYExitOffset) + rightPos.y
            );

            // Platform
            MovingPlatformBehaviour movingPlatform = obj.AddComponent<MovingPlatformBehaviour>();
            movingPlatform.LeftPosition = leftPos;
            movingPlatform.RightPosition = rightPos;
            movingPlatform.LeftUsePosition = leftUsePos;
            movingPlatform.RightUsePosition = rightUsePos;
            movingPlatform.IsLeft = true;
            movingPlatform.MovingSound = utilData.GameObj.GetComponent<MovingPlatformBehaviour>().MovingSound;
            Platform = movingPlatform;

            // Consoles
            GameObject leftObj = new GameObject("Left Console");
            leftObj.transform.SetParent(LIShipStatus.Instance.transform);
            leftObj.transform.localPosition = leftUsePos;
            leftObj.AddComponent<BoxCollider2D>().isTrigger = true;
            GameObject rightObj = new GameObject("Right Console");
            rightObj.transform.SetParent(LIShipStatus.Instance.transform);
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