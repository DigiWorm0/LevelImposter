using System;
using System.Collections.Generic;
using System.Text;
using LevelImposter.Models;
using UnityEngine;

namespace LevelImposter.MinimapGen
{
    class MinimapGenerator
    {
        private Minimap map;
        public Generator[] generators;

        public const float MAP_SCALE = 0.2f;
        public const float OFFSET_X = 14.5f;
        public const float OFFSET_Y = 21.4f;

        public static List<MapAsset> shipRooms;
        private static float maxX = 0;
        private static float minX = 0;
        private static float maxY = 0;
        private static float minY = 0;

        public static void Reset()
        {
            shipRooms = new List<MapAsset>();
            maxX = 0;
            minX = 0;
            maxY = 0;
            minY = 0;
        }

        public static void AddRoom(MapAsset asset)
        {
            shipRooms.Add(asset);

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

        public void PreGen(MapBehaviour mapBehaviour)
        {
            // Player Position
            mapBehaviour.gameObject.transform.FindChild("HereIndicatorParent").localPosition = new Vector3(0, 5.0f, -0.1f);

            // Map
            map = new Minimap(mapBehaviour);

            // Generators
            generators = new Generator[]
            {
                new BGGenerator(map),
                new LabelGenerator(map),
                new AdminGenerator(map),
                new SabGenerator(map)
            };

            // Generate
            foreach (Generator gen in generators)
            {
                foreach (MapAsset asset in shipRooms)
                {
                    gen.Generate(asset);
                }
            }
        }

        public void Finish()
        {
            // Scaling
            float deltaX = (minX + maxX) / 2;
            float deltaY = (minY + maxY) / -2;

            for (int i = 0; i < map.prefab.transform.childCount; i++)
            {
                Transform child = map.prefab.transform.GetChild(i);
                if (child.name != "CloseButton")
                    child.localPosition -= new Vector3(deltaX, deltaY, 0);
            }

            foreach (var generator in generators)
            {
                generator.Finish();
            }
        }
    }
}
