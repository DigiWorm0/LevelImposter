using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace LevelImposter.MinimapGen
{
    class LabelGenerator : Generator
    {
        private GameObject namesParent;
        private GameObject nameBackup;

        public LabelGenerator(Minimap map)
        {
            namesParent = map.prefab.transform.FindChild("RoomNames").gameObject;
            nameBackup = namesParent.transform.GetChild(0).gameObject;
            MinimapGenerator.ClearChildren(namesParent.transform);
        }

        public void Generate(MapAsset asset)
        {
            // Label
            GameObject label = GameObject.Instantiate(nameBackup);
            label.transform.SetParent(namesParent.transform);
            label.transform.position = new Vector3(asset.x * MinimapGenerator.MAP_SCALE, -asset.y * MinimapGenerator.MAP_SCALE, -25.0f);
            label.name = asset.name;

            // TextMeshPro
            GameObject.Destroy(label.GetComponent<TextTranslator>());
            TextMeshPro text = label.GetComponent<TextMeshPro>();
            text.text = asset.name;
            text.enabled = true;
        }
    }
}
