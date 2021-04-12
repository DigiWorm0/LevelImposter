using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Map
{
    static class PolusMapBehavior
    {
        public const float MAP_SCALE = 1.0f / 5.0f;

        private static GameObject prefab;
        private static MapBehaviour mapBehaviour;

        private static Mesh bgMesh;
        private static GameObject roomNames;
        private static GameObject roomNameBackup;

        private static float averageX = 0;
        private static float averageY = 0;
        private static int averageCount = 0;

        private static bool hasInit = false;

        public static void Init(MapBehaviour mapBehaviour)
        {
            if (hasInit)
                return;

            PolusMapBehavior.mapBehaviour = mapBehaviour;
            prefab = mapBehaviour.gameObject;

            bgMesh = new Mesh();
            bgMesh.vertices = new Vector3[0];
            bgMesh.uv = new Vector2[0];
            bgMesh.triangles = new int[0];
            bgMesh.colors = new Color[0];

            Clear();
        }

        private static void Clear()
        {
            // Names
            roomNames = prefab.transform.FindChild("RoomNames").gameObject;
            roomNameBackup = roomNames.transform.GetChild(0).gameObject;
            ClearChildren(roomNames.transform);

            // Background
            GameObject background = prefab.transform.FindChild("Background").gameObject;
            background.transform.position = new Vector3(0, 0, 0);
            background.GetComponent<AlphaPulse>().rend = null;
            GameObject.DestroyImmediate(background.GetComponent<SpriteRenderer>());
            MeshFilter filter = background.AddComponent<MeshFilter>();
            filter.mesh = bgMesh;
            MeshRenderer meshRenderer = background.AddComponent<MeshRenderer>();
            background.GetComponent<AlphaPulse>().mesh = meshRenderer;
            meshRenderer.material = new Material(Shader.Find("Sprites/Default"));

            // Indicator
            prefab.transform.FindChild("HereIndicatorParent").position = new Vector3(0, 5.0f, -0.1f);
        }

        private static void ClearChildren(Transform obj)
        {
            for (int i = 0; i < obj.childCount; i++)
            {
                GameObject.Destroy(obj.GetChild(i).gameObject);
            }
        }

        public static void AddRoom(MapAsset asset)
        {
            if (hasInit)
                return;

            // Label Obj
            GameObject label = GameObject.Instantiate(roomNameBackup);
            label.transform.SetParent(roomNames.transform);
            label.transform.position = new Vector3(asset.x * MAP_SCALE, -asset.y * MAP_SCALE, -25.0f);
            label.name = asset.name;

            // Label Text
            GameObject.Destroy(label.GetComponent<TextTranslator>());
            TextRenderer text = label.GetComponent<TextRenderer>();
            text.Text = asset.name;
            text.enabled = true;

            // Mesh
            List<Vector3> vertices = new List<Vector3>(bgMesh.vertices);
            List<Vector2> uv = new List<Vector2>(bgMesh.uv);
            List<int> triangles = new List<int>(bgMesh.triangles);
            List<Color> colors = new List<Color>(bgMesh.colors);
            
            foreach (MapCollider c in asset.colliders)
            {
                // Get Points
                var points = c.GetPoints(asset.xScale, asset.yScale);
                points.RemoveAt(points._size - 1);
                for (int i = 0; i < points.Count; i++)
                {
                    points[i] = new Vector2((points[i].x + asset.x) * MAP_SCALE, (points[i].y - asset.y) * MAP_SCALE);
                }

                // Triangulate
                var triangulator = new Triangulator(points.ToArray());
                var triangulation = triangulator.Triangulate();
                for (int i = 0; i < triangulation.Length; i++)
                {
                    triangulation[i] += vertices.Count;
                }

                // Add to Array
                triangles.AddRange(triangulation);
                foreach (Vector2 point in points)
                {
                    vertices.Add(point);
                    uv.Add(point);
                    colors.Add(new Color(0, 0, 255, 200));
                }
            }
            bgMesh.vertices = vertices.ToArray();
            bgMesh.uv = uv.ToArray();
            bgMesh.triangles = triangles.ToArray();
            bgMesh.colors = colors.ToArray();

            // Avg Position
            averageX += asset.x * MAP_SCALE;
            averageY += -asset.y * MAP_SCALE;
            averageCount++;
        }

        public static void SetPosition()
        {
            if (hasInit)
                return;

            float deltaX = averageX / averageCount;
            float deltaY = averageY / averageCount;

            for (int i = 0; i < prefab.transform.childCount; i++)
            {
                Transform child = prefab.transform.GetChild(i);
                if (child.name != "CloseButton")
                    child.position -= new Vector3(deltaX, deltaY, 0);
            }

            bgMesh.RecalculateNormals();
            bgMesh.RecalculateBounds();
            hasInit = true;
        }
    }
}
