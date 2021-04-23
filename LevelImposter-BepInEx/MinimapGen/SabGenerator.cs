using LevelImposter.Builders;
using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace LevelImposter.MinimapGen
{
    class SabGenerator : Generator
    {
        private GameObject commsBackup;
        private GameObject reactorBackup;
        private GameObject doorsBackup;
        private GameObject lightsBackup;

        private GameObject overlayObj;
        private InfectedOverlay overlay;
        private Dictionary<SystemTypes, MapRoom> sabDb;
        private List<long> addedIds;

        public static SabGenerator Instance;

        public SabGenerator(Minimap map)
        {
            Instance = this;
            sabDb = new Dictionary<SystemTypes, MapRoom>();
            addedIds = new List<long>();
            overlayObj = map.prefab.transform.FindChild("InfectedOverlay").gameObject;
            overlay = overlayObj.GetComponent<InfectedOverlay>();
            overlay.rooms = new UnhollowerBaseLib.Il2CppReferenceArray<MapRoom>(0);
            //overlay.doors = ShipStatus.Instance.Systems[SystemTypes.Doors].Cast<IActivatable>();

            commsBackup = BackupRoom("Comms", "bomb"); // um...BOMB!?
            reactorBackup = BackupRoom("Laboratory", "meltdown");
            doorsBackup = BackupRoom("Office", "Doors");
            lightsBackup = BackupRoom("Electrical", "lightsOut");

            AssetHelper.ClearChildren(overlayObj.transform);
        }

        private GameObject BackupRoom(string parent, string child)
        {
            return overlayObj.transform.FindChild(parent).FindChild(child).gameObject;
        }

        public void Generate(MapAsset asset)
        {
            // Object
            GameObject sabRoomObj = new GameObject(asset.name);
            sabRoomObj.transform.localScale = new Vector3(0.8f, 0.8f, 1.0f);
            sabRoomObj.transform.position = new Vector3(
                asset.x * MinimapGenerator.MAP_SCALE,
                -asset.y * MinimapGenerator.MAP_SCALE - 0.25f,
                -25.0f
            );
            sabRoomObj.transform.SetParent(overlayObj.transform);

            // MapRoom
            MapRoom sabMapRoom = sabRoomObj.AddComponent<MapRoom>();
            sabMapRoom.room = ShipRoomBuilder.db[asset.id];
            sabMapRoom.Parent = overlay;
            overlay.rooms = AssetHelper.AddToArr(overlay.rooms, sabMapRoom);

            sabDb.Add(sabMapRoom.room, sabMapRoom);
        }

        private MapRoom GetRoom(SystemTypes sys)
        {
            if (sabDb.ContainsKey(sys))
                return sabDb[sys];

            // GameObject
            GameObject sabRoomObj = new GameObject(sys.ToString());
            sabRoomObj.transform.SetParent(overlayObj.transform);

            // Map Room
            MapRoom sabMapRoom = sabRoomObj.AddComponent<MapRoom>();
            sabMapRoom.room = sys;
            sabMapRoom.Parent = overlay;
            overlay.rooms = AssetHelper.AddToArr(overlay.rooms, sabMapRoom);
            
            sabDb.Add(sabMapRoom.room, sabMapRoom);
            return sabMapRoom;
        }

        public static void AddSabotage(MapAsset asset)
        {
            if (!SabBuilder.SAB_SYSTEMS.ContainsKey(asset.type))
                return;
            if (Instance.addedIds.Contains(asset.id))
                return;
            Instance.addedIds.Add(asset.id);

            // System
            SystemTypes sys = SabBuilder.SAB_SYSTEMS[asset.type];
            MapRoom mapRoom = Instance.GetRoom(sys);

            // GameObject
            GameObject button = new GameObject(asset.name);
            button.transform.SetParent(mapRoom.gameObject.transform);
            button.transform.position = new Vector3(asset.x * MinimapGenerator.MAP_SCALE, -asset.y * MinimapGenerator.MAP_SCALE, -25.0f);

            // Sprite Renderer
            SpriteRenderer spriteRenderer = button.AddComponent<SpriteRenderer>();
            spriteRenderer.material = Instance.commsBackup.GetComponent<SpriteRenderer>().material;
            button.layer = (int)Layer.UI;
            mapRoom.special = spriteRenderer;

            // Collider
            CircleCollider2D colliderClone = Instance.commsBackup.GetComponent<CircleCollider2D>();
            CircleCollider2D collider = button.AddComponent<CircleCollider2D>();
            collider.radius = colliderClone.radius;
            collider.offset = colliderClone.offset;
            collider.isTrigger = true;

            // Sabotage Type
            ButtonBehavior behaviour;

            switch (asset.type)
            {
                case "sab-electric":
                    spriteRenderer.sprite = Instance.lightsBackup.GetComponent<SpriteRenderer>().sprite;
                    behaviour = Instance.lightsBackup.GetComponent<ButtonBehavior>();
                    break;
                case "sab-reactorleft":
                    spriteRenderer.sprite = Instance.reactorBackup.GetComponent<SpriteRenderer>().sprite;
                    behaviour = Instance.reactorBackup.GetComponent<ButtonBehavior>();
                    break;
                case "sab-comms":
                    spriteRenderer.sprite = Instance.commsBackup.GetComponent<SpriteRenderer>().sprite;
                    behaviour = Instance.commsBackup.GetComponent<ButtonBehavior>();
                    break;
                case "sab-doors":
                    spriteRenderer.sprite = Instance.doorsBackup.gameObject.GetComponent<SpriteRenderer>().sprite;
                    behaviour = Instance.doorsBackup.GetComponent<ButtonBehavior>();
                    break;
                default:
                    return;
            }
            
            // Button Behaviour
            ButtonBehavior btnBehaviour = button.AddComponent<ButtonBehavior>();
            btnBehaviour.OnClick = behaviour.OnClick;
            btnBehaviour.OnClick.m_PersistentCalls.m_Calls[0].m_Target = mapRoom;
            btnBehaviour.colliders = new UnhollowerBaseLib.Il2CppReferenceArray<Collider2D>(1);
            btnBehaviour.colliders[0] = collider;

            Instance.overlay.allButtons = AssetHelper.AddToArr(Instance.overlay.allButtons, btnBehaviour);
        }

        public void Finish() { }
    }
}
