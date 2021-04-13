using LevelImposter.Builders;
using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace LevelImposter.Map
{
    static class MapGenerator
    {
        public const float MAP_SCALE = 1.0f / 5.0f;

        private static string mapColor = "";

        private static GameObject prefab;
        private static MapBehaviour mapBehaviour;

        private static Mesh bgMesh;
        private static GameObject background;
        private static GameObject countObj;
        private static MapCountOverlay count;
        private static GameObject roomNames;
        private static GameObject roomNameBackup;

        private static float maxX = 0;
        private static float minX = 0;
        private static float maxY = 0;
        private static float minY = 0;

        private static bool hasInit = false;

        public static void Init(MapBehaviour mapBehaviour)
        {
            if (hasInit)
                return;

            MapGenerator.mapBehaviour = mapBehaviour;
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
            background = prefab.transform.FindChild("Background").gameObject;
            background.transform.position = new Vector3(0, 0, 0);
            background.GetComponent<AlphaPulse>().rend = null;
            GameObject.DestroyImmediate(background.GetComponent<SpriteRenderer>());
            MeshFilter filter = background.AddComponent<MeshFilter>();
            filter.mesh = bgMesh;
            MeshRenderer meshRenderer = background.AddComponent<MeshRenderer>();
            background.GetComponent<AlphaPulse>().mesh = meshRenderer;
            meshRenderer.material = new Material(Shader.Find("Sprites/Default"));

            // Admin
            countObj = prefab.transform.FindChild("CountOverlay").gameObject;
            count = countObj.GetComponent<MapCountOverlay>();
            count.CountAreas = new UnhollowerBaseLib.Il2CppReferenceArray<CounterArea>(0);
            ClearChildren(countObj.transform);

            // Sabotages
            GameObject sabMap = prefab.transform.FindChild("InfectedOverlay").gameObject;
            ClearChildren(sabMap.transform);


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
            TextMeshPro text = label.GetComponent<TextMeshPro>();
            text.text = asset.name;
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
                var points2 = new List<Vector3>();
                if (c.isClosed)
                    points.RemoveAt(points._size - 1);
                for (int i = 0; i < points.Count; i++)
                {
                    points[i] = new Vector2((points[i].x + asset.x) * MAP_SCALE, (points[i].y - asset.y) * MAP_SCALE);
                    for (int o = 0; o < 5; o++)
                        points2.Add(new Vector3(points[i].x, points[i].y));
                }

                // Line
                var lineObj = new GameObject(asset.name + "Line");
                lineObj.transform.SetParent(background.transform);
                lineObj.transform.position += new Vector3(0, 0, -1.0f);
                lineObj.layer = (int)Layer.UI;
                LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();
                lineRenderer.startColor = Color.white;
                lineRenderer.endColor = Color.white;
                lineRenderer.startWidth = 0.03f;
                lineRenderer.endWidth = 0.03f;
                lineRenderer.positionCount = points2.Count;
                lineRenderer.useWorldSpace = false;
                lineRenderer.loop = true;
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.SetPositions(points2.ToArray());

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
                    colors.Add(new Color(0, 0, 1.0f, 0.6f));
                }
            }
            bgMesh.vertices = vertices.ToArray();
            bgMesh.uv = uv.ToArray();
            bgMesh.triangles = triangles.ToArray();
            bgMesh.colors = colors.ToArray();

            // Admin Counters
            GameObject counterObj = new GameObject(asset.name);
            counterObj.transform.position = new Vector3(asset.x * MAP_SCALE, -asset.y * MAP_SCALE, -25.0f);
            counterObj.transform.SetParent(countObj.transform);
            CounterArea counterArea = counterObj.AddComponent<CounterArea>();
            counterArea.RoomType = ShipRoomBuilder.db[asset.id];
            counterArea.MaxWidth = 5;
            counterArea.XOffset = 0.3f;
            counterArea.YOffset = 0.3f;
            count.CountAreas = AssetBuilder.AddToArr(count.CountAreas, counterArea);

            // Avg Position
            if (asset.x * MAP_SCALE > maxX)
                maxX = asset.x * MAP_SCALE;
            if (asset.x * MAP_SCALE < minX)
                minX = asset.x * MAP_SCALE;
            if (asset.y * MAP_SCALE > maxY)
                maxY = asset.y * MAP_SCALE;
            if (asset.y * MAP_SCALE < minY)
                minY = asset.y * MAP_SCALE;
        }

        public static void SetColor(Color color)
        {
            if (color.ToString() == mapColor)
                return;
            List<Color> colors = new List<Color>();
            for (int i = 0; i < bgMesh.vertices.Count; i++)
                colors.Add(color);
            bgMesh.colors = colors.ToArray();
            mapColor = color.ToString();
        }

        public static void SetPosition()
        {
            if (hasInit)
                return;

            float deltaX = (minX + maxX) / 2;
            float deltaY = (minY + maxY) / -2;

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
