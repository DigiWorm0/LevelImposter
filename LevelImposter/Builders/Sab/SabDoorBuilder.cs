using UnityEngine;
using LevelImposter.DB;
using PowerTools;
using LevelImposter.Core;

namespace LevelImposter.Builders
{
    public class SabDoorBuilder : IElemBuilder
    {
        private const string OPEN_SOUND_NAME = "doorOpen";
        private const string CLOSE_SOUND_NAME = "doorClose";
        private int _doorId = 0;

        public void Build(LIElement elem, GameObject obj)
        {
            if (!elem.type.StartsWith("sab-door"))
                return;

            // ShipStatus
            var shipStatus = LIShipStatus.Instance?.ShipStatus;
            if (shipStatus == null)
                throw new MissingShipException();

            // Prefab
            var prefab = AssetDB.GetObject(elem.type);
            if (prefab == null)
                return;
            var prefabRenderer = prefab.GetComponent<SpriteRenderer>();
            var prefabDoor = prefab.GetComponent<PlainDoor>();

            // Default Sprite
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            Animator animator = obj.AddComponent<Animator>();
            SpriteAnim spriteAnim = obj.AddComponent<SpriteAnim>();
            obj.layer = (int)Layer.Ship;
            bool isSpriteAnim = false;
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
            doorComponent.OpenSound = prefabDoor.OpenSound;
            doorComponent.CloseSound = prefabDoor.CloseSound;
            shipStatus.AllDoors = MapUtils.AddToArr(shipStatus.AllDoors, doorComponent);

            // Sound
            LISound? openSound = MapUtils.FindSound(elem.properties.sounds, OPEN_SOUND_NAME);
            if (openSound != null)
                doorComponent.OpenSound = WAVFile.Load(openSound.data);

            LISound? closeSound = MapUtils.FindSound(elem.properties.sounds, CLOSE_SOUND_NAME);
            if (closeSound != null)
                doorComponent.CloseSound = WAVFile.Load(closeSound.data);

            // SpriteAnim
            if (isSpriteAnim)
            {
                doorComponent.OpenDoorAnim = prefabDoor.OpenDoorAnim;
                doorComponent.CloseDoorAnim = prefabDoor.CloseDoorAnim;
            }

            // Console
            bool isInteractable = elem.properties.isDoorInteractable ?? true;
            if (isManualDoor && isInteractable)
            {
                // Prefab
                var prefab2 = AssetDB.GetObject($"sab-door-{doorType}"); // "sab-door-polus" or "sab-door-airship"
                DoorConsole? prefab2Console = prefab2?.GetComponent<DoorConsole>();

                // Object
                GameObject doorConsole = new GameObject(obj.name + "_Console");
                doorConsole.transform.position = obj.transform.position;
                doorConsole.layer = (int)Layer.Objects;

                // Console
                DoorConsole consoleComponent = doorConsole.AddComponent<DoorConsole>();
                consoleComponent.MinigamePrefab = prefab2Console?.MinigamePrefab;
                consoleComponent.MyDoor = doorComponent;
                consoleComponent.Image = spriteRenderer;

                // Colliders
                MapUtils.CreateDefaultColliders(doorConsole, obj);
            }
        }

        public void PostBuild() {}
    }
}
