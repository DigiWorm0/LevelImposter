using System;
using System.Collections.Generic;
using System.Text;
using LevelImposter.Models;
using UnityEngine;

namespace LevelImposter.MinimapGen
{
    class MinimapGenerator : Generator
    {
        private static bool hasGenerated = false;
        public const float MAP_SCALE = 1.0f / 5.0f;
        private float maxX = 0;
        private float minX = 0;
        private float maxY = 0;
        private float minY = 0;

        private Minimap map;
        private LabelGenerator  labelGen;
        private BGGenerator     bgGen;
        private AdminGenerator  adminGen;
        private SabGenerator    sabGen;

        public MinimapGenerator(MapBehaviour mapBehaviour)
        {
            if (hasGenerated)
                return;

            mapBehaviour.gameObject.transform.FindChild("HereIndicatorParent").position = new Vector3(0, 5.0f, -0.1f);

            map = new Minimap(mapBehaviour);
            labelGen    = new LabelGenerator(map);
            bgGen       = new BGGenerator(map);
            adminGen    = new AdminGenerator(map);
            sabGen      = new SabGenerator(map);
        }

        public static void ClearChildren(Transform obj)
        {
            for (int i = 0; i < obj.childCount; i++)
                GameObject.Destroy(obj.GetChild(i).gameObject);
        }

        public void Generate(MapAsset asset)
        {
            if (hasGenerated)
                return;

            labelGen.Generate(asset);
            bgGen   .Generate(asset);
            adminGen.Generate(asset);
            sabGen  .Generate(asset);

            // Scaling
            if (asset.x * MAP_SCALE > maxX)
                maxX = asset.x * MAP_SCALE;
            if (asset.x * MAP_SCALE < minX)
                minX = asset.x * MAP_SCALE;
            if (asset.y * MAP_SCALE > maxY)
                maxY = asset.y * MAP_SCALE;
            if (asset.y * MAP_SCALE < minY)
                minY = asset.y * MAP_SCALE;
        }

        public void Finish()
        {
            if (hasGenerated)
                return;

            // Scaling
            float deltaX = (minX + maxX) / 2;
            float deltaY = (minY + maxY) / -2;
            for (int i = 0; i < map.prefab.transform.childCount; i++)
            {
                Transform child = map.prefab.transform.GetChild(i);
                if (child.name != "CloseButton")
                    child.position -= new Vector3(deltaX, deltaY, 0);
            }

            bgGen.Finish();
            hasGenerated = true;
        }
    }
}
