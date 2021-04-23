using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace LevelImposter.MinimapGen
{
    class BGGenerator : Generator
    {
        private static string mapColor = "";
        private static Mesh bgMesh;
        private GameObject bgObj;

        public BGGenerator(Minimap map)
        {
            // Init Mesh
            bgMesh = new Mesh();
            bgMesh.vertices = new Vector3[0];
            bgMesh.uv = new Vector2[0];
            bgMesh.triangles = new int[0];
            bgMesh.colors = new Color[0];

            // Get Object
            bgObj = map.prefab.transform.FindChild("Background").gameObject;
            bgObj.transform.localPosition = new Vector3(0, 0, 0);
            bgObj.GetComponent<AlphaPulse>().rend = null;

            // Replace Sprite with Mesh
            GameObject.DestroyImmediate(bgObj.GetComponent<SpriteRenderer>());
            MeshFilter filter = bgObj.AddComponent<MeshFilter>();
            filter.mesh = bgMesh;
            MeshRenderer meshRenderer = bgObj.AddComponent<MeshRenderer>();
            bgObj.GetComponent<AlphaPulse>().mesh = meshRenderer;
            meshRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        public void Generate(MapAsset asset)
        {
            // Array => List
            List<Vector3> vertices = new List<Vector3>(bgMesh.vertices);
            List<Vector2> uv = new List<Vector2>(bgMesh.uv);
            List<int> triangles = new List<int>(bgMesh.triangles);
            List<Color> colors = new List<Color>(bgMesh.colors);

            // Iterate Colliders
            foreach (MapCollider c in asset.colliders)
            {
                // Get Points
                var points = c.GetPoints();
                var points2 = new List<Vector3>();
                if (c.isClosed)
                    points.RemoveAt(points._size - 1);
                for (int i = 0; i < points.Count; i++)
                {
                    points[i] = new Vector2(
                        (points[i].x + asset.x) * MinimapGenerator.MAP_SCALE,
                        (points[i].y - asset.y) * MinimapGenerator.MAP_SCALE
                    );

                    for (int o = 0; o < 5; o++)
                        points2.Add(new Vector3(points[i].x, points[i].y));
                }

                // LineRenderer
                var lineObj = new GameObject(asset.name + "Line");
                lineObj.transform.SetParent(bgObj.transform);
                lineObj.transform.localPosition = new Vector3(0, 0, -1.0f);
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

            // List => Array
            bgMesh.vertices = vertices.ToArray();
            bgMesh.uv = uv.ToArray();
            bgMesh.triangles = triangles.ToArray();
            bgMesh.colors = colors.ToArray();
        }

        public void Finish()
        {
            bgMesh.RecalculateNormals();
            bgMesh.RecalculateBounds();
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
    }
}
