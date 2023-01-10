using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using LevelImposter.DB;
using PowerTools;

namespace LevelImposter.Core
{
    public class SabDoorBuilder : IElemBuilder
    {
        private int _doorId = 0;

        public void Build(LIElement elem, GameObject obj)
        {
            if (!elem.type.StartsWith("sab-door"))
                return;
            if (LIShipStatus.Instance?.ShipStatus == null)
                throw new Exception("ShipStatus not found");

            SabData sabData = AssetDB.Sabs[elem.type];

            // Default Sprite
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            Animator animator = obj.AddComponent<Animator>();
            SpriteAnim spriteAnim = obj.AddComponent<SpriteAnim>();
            obj.layer = (int)Layer.ShortObjects;
            bool isSpriteAnim = false;
            if (!spriteRenderer)
            {
                spriteRenderer = obj.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = sabData.SpriteRenderer.sprite;

                if (elem.properties.color != null)
                    spriteRenderer.color = MapUtils.LIColorToColor(elem.properties.color);

                isSpriteAnim = true;
            }
            else
            {
                spriteRenderer.enabled = false;
                spriteAnim.enabled = false;
                animator.enabled = false;
            }
            spriteRenderer.material = sabData.SpriteRenderer.material;

            // Dummy Components
            BoxCollider2D dummyCollider = obj.AddComponent<BoxCollider2D>();
            dummyCollider.isTrigger = true;
            dummyCollider.enabled = false;

            // Colliders
            Collider2D[] colliders = obj.GetComponentsInChildren<Collider2D>();
            foreach (Collider2D collider in colliders)
                collider.enabled = false;

            // Door
            var doorType = elem.properties.doorType;
            bool isManualDoor = doorType == "polus" || doorType == "airship";
            ShipStatus shipStatus = LIShipStatus.Instance.ShipStatus;
            PlainDoor doorClone = sabData.GameObj.GetComponent<PlainDoor>();
            PlainDoor doorComponent;
            if (isManualDoor)
            {
                doorComponent = obj.AddComponent<PlainDoor>();
                shipStatus.Systems[SystemTypes.Doors] = new DoorsSystemType().Cast<ISystemType>();
            }
            else
            {
                doorComponent = obj.AddComponent<AutoOpenDoor>();
                shipStatus.Systems[SystemTypes.Doors] = new AutoDoorsSystemType().Cast<ISystemType>();
            }
            doorComponent.Room = RoomBuilder.GetParentOrDefault(elem);
            doorComponent.Id = _doorId++;
            doorComponent.myCollider = dummyCollider;
            doorComponent.animator = spriteAnim;
            doorComponent.OpenSound = doorClone.OpenSound;
            doorComponent.CloseSound = doorClone.CloseSound;
            shipStatus.AllDoors = MapUtils.AddToArr(shipStatus.AllDoors, doorComponent);

            // SpriteAnim
            if (isSpriteAnim)
            {
                doorComponent.OpenDoorAnim = doorClone.OpenDoorAnim;
                doorComponent.CloseDoorAnim = doorClone.CloseDoorAnim;
            }

            // Console
            if (isManualDoor)
            {
                SabData sabData2 = AssetDB.Sabs["sab-door-" + doorType];
                DoorConsole consoleClone = sabData2.GameObj.GetComponent<DoorConsole>();
                DoorConsole consoleComponent = obj.AddComponent<DoorConsole>();
                consoleComponent.MinigamePrefab = consoleClone.MinigamePrefab;
            }
        }

        public void PostBuild() {}
    }
}
