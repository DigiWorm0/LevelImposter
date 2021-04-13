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
        private GameObject overlayObj;
        private InfectedOverlay overlay;
        private static Dictionary<long, GameObject> sabDb;

        public SabGenerator(Minimap map)
        {
            overlayObj = map.prefab.transform.FindChild("InfectedOverlay").gameObject;
            overlay = overlayObj.GetComponent<InfectedOverlay>();
            MinimapGenerator.ClearChildren(overlayObj.transform);
        }

        public void Generate(MapAsset asset)
        {
            // Object
            GameObject sabRoomObj = new GameObject(asset.name);
            sabRoomObj.transform.position = new Vector3(
                asset.x * MinimapGenerator.MAP_SCALE,
                -asset.y * MinimapGenerator.MAP_SCALE - 0.25f,
                -25.0f
            );
            sabRoomObj.transform.SetParent(overlayObj.transform);

            // MapRoom
            MapRoom sabMapRoom = sabRoomObj.AddComponent<MapRoom>();
            sabMapRoom.Parent = overlay;
            sabMapRoom.room = ShipRoomBuilder.db[asset.id];

            sabDb.Add(asset.id, sabRoomObj);
        }
    }
}
