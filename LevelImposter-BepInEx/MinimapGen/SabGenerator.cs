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
        private static GameObject commsBackup;
        private static GameObject reactorBackup;
        private static GameObject doorsBackup;
        private static GameObject lightsBackup;
        private static GameObject oxygenBackup;

        private GameObject overlayObj;
        private InfectedOverlay overlay;
        private static Dictionary<long, GameObject> sabDb;

        public SabGenerator(Minimap map)
        {
            sabDb = new Dictionary<long, GameObject>();
            overlayObj = map.prefab.transform.FindChild("InfectedOverlay").gameObject;
            overlay = overlayObj.GetComponent<InfectedOverlay>();
            overlay.rooms = new UnhollowerBaseLib.Il2CppReferenceArray<MapRoom>(0);

            commsBackup = BackupRoom("Comms", "bomb"); // um...BOMB!?
            reactorBackup = BackupRoom("Laboratory", "meltdown");
            doorsBackup = BackupRoom("Office", "Doors");
            lightsBackup = BackupRoom("Electrical", "lightsOut");
            //oxygenBackup = BackupRoom("Comms", "bomb");

            MinimapGenerator.ClearChildren(overlayObj.transform);
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
            overlay.rooms = AssetBuilder.AddToArr(overlay.rooms, sabMapRoom);

            sabDb.Add(asset.id, sabRoomObj);
        }

        public static void AddSabotage(MapAsset asset)
        {
            if (asset.type == "sab-oxygen2" || asset.type == "sab-reactorright")
            {
                return;
            }
            if (asset.targetIds.Length <= 0)
            {
                throw new Exception(asset.name + " has no Target Room");
            }
            if (asset.targetIds[0] <= 0)
            {
                // TODO
            }

            // GameObject
            GameObject parent = sabDb[asset.targetIds[0]];
            GameObject button = new GameObject(asset.name);
            button.transform.SetParent(parent.transform);
            button.transform.position = new Vector3(asset.x * MinimapGenerator.MAP_SCALE, -asset.y * MinimapGenerator.MAP_SCALE, -25.0f);

            // Sprite Renderer
            SpriteRenderer spriteRenderer = button.AddComponent<SpriteRenderer>();
            spriteRenderer.material = commsBackup.GetComponent<SpriteRenderer>().material;
            button.layer = (int)Layer.UI;

            switch (asset.type)
            {
                case "sab-electric":
                    spriteRenderer.sprite = lightsBackup.GetComponent<SpriteRenderer>().sprite;
                    break;
                case "sab-reactorleft":
                    spriteRenderer.sprite = reactorBackup.GetComponent<SpriteRenderer>().sprite;
                    break;
                case "sab-oxygen1":
                    spriteRenderer.sprite = oxygenBackup.GetComponent<SpriteRenderer>().sprite;
                    break;
                case "sab-comms":
                    spriteRenderer.sprite = commsBackup.GetComponent<SpriteRenderer>().sprite;
                    break;
                case "sab-doors":
                    spriteRenderer.sprite = doorsBackup.gameObject.GetComponent<SpriteRenderer>().sprite;
                    break;
            }
        }
    }
}
