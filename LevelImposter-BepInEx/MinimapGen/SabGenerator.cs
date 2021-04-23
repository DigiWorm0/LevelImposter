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
        private static List<MapAsset> sabObjs = new List<MapAsset>();

        public SabGenerator(Minimap map)
        {
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
            sabRoomObj.transform.SetParent(overlayObj.transform);
            sabRoomObj.transform.localPosition = new Vector3(0, 0, -25.0f);

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
            sabRoomObj.transform.localScale = new Vector3(0.8f, 0.8f, 1.0f);
            sabRoomObj.transform.SetParent(overlayObj.transform);
            sabRoomObj.transform.localPosition = new Vector3(0, 0, -25.0f);

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
            sabObjs.Add(asset);
        }

        public void Finish()
        {
            foreach (MapAsset sabotage in sabObjs)
            {
                DrawSabotage(sabotage);
            }
            sabObjs.Clear();
        }
        private void DrawSabotage(MapAsset asset)
        {
            if (!SabBuilder.SAB_SYSTEMS.ContainsKey(asset.type))
                return;
            if (addedIds.Contains(asset.id))
                return;
            addedIds.Add(asset.id);

            // System
            SystemTypes sys = SabBuilder.SAB_SYSTEMS[asset.type];
            MapRoom mapRoom = GetRoom(sys);

            // GameObject
            GameObject button = new GameObject(asset.name);
            button.transform.SetParent(mapRoom.gameObject.transform);
            button.transform.localPosition = new Vector3(asset.x * MinimapGenerator.MAP_SCALE * 1.25f, -asset.y * MinimapGenerator.MAP_SCALE * 1.25f, -25.0f);

            // Sprite Renderer
            SpriteRenderer spriteRenderer = button.AddComponent<SpriteRenderer>();
            spriteRenderer.material = commsBackup.GetComponent<SpriteRenderer>().material;
            button.layer = (int)Layer.UI;
            mapRoom.special = spriteRenderer;

            // Collider
            CircleCollider2D colliderClone = commsBackup.GetComponent<CircleCollider2D>();
            CircleCollider2D collider = button.AddComponent<CircleCollider2D>();
            collider.radius = colliderClone.radius;
            collider.offset = colliderClone.offset;
            collider.isTrigger = true;

            // Sabotage Type
            ButtonBehavior behaviour;

            switch (asset.type)
            {
                case "sab-electric":
                    spriteRenderer.sprite = lightsBackup.GetComponent<SpriteRenderer>().sprite;
                    behaviour = lightsBackup.GetComponent<ButtonBehavior>();
                    break;
                case "sab-reactorleft":
                    spriteRenderer.sprite = reactorBackup.GetComponent<SpriteRenderer>().sprite;
                    behaviour = reactorBackup.GetComponent<ButtonBehavior>();
                    break;
                case "sab-comms":
                    spriteRenderer.sprite = commsBackup.GetComponent<SpriteRenderer>().sprite;
                    behaviour = commsBackup.GetComponent<ButtonBehavior>();
                    break;
                case "sab-doors":
                    spriteRenderer.sprite = doorsBackup.gameObject.GetComponent<SpriteRenderer>().sprite;
                    behaviour = doorsBackup.GetComponent<ButtonBehavior>();
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

            overlay.allButtons = AssetHelper.AddToArr(overlay.allButtons, btnBehaviour);
        }
    }
}
