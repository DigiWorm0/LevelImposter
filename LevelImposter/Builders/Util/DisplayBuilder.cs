using System;
using LevelImposter.Build.Attributes;
using LevelImposter.Core.Components;
using LevelImposter.Core.GarbageCollection;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using LevelImposter.DB;
using UnityEngine;

namespace LevelImposter.Builders.Util;

internal static class DisplayBuilder
{
    private const int DEFAULT_WIDTH = 330;
    private const int DEFAULT_HEIGHT = 220;
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");

    [ElementBuilder(ElementTypes = ["util-display"])]
    public static void Build(LIElement element, GameObject gameObject)
    {
        // Prefab
        var minigamePrefab = PrefabDB
            .GetObject("util-cams")?
            .GetComponent<SystemConsole>()
            .MinigamePrefab
            .Cast<PlanetSurveillanceMinigame>();
        gameObject.layer = (int)Layer.Objects;

        // Options
        var width = element.properties.displayWidth ?? DEFAULT_WIDTH;
        var height = element.properties.displayHeight ?? DEFAULT_HEIGHT;

        // Camera
        var cameraObject = new GameObject("DisplayCamera");
        cameraObject.layer = (int)Layer.UI;
        cameraObject.transform.parent = LIBaseShip.Instance?.transform;
        cameraObject.transform.position = new Vector3(
            (element.properties.camXOffset ?? 0) + gameObject.transform.position.x,
            (element.properties.camYOffset ?? 0) + gameObject.transform.position.y,
            0.0f
        );

        var camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = element.properties.camZoom ?? 3;
        camera.cullingMask = 0b1111001100010111; // Include Shadows: 0b10111001100010111
        camera.farClipPlane = 1000.0f;
        camera.nearClipPlane = -1000.0f;
        GCHandler.Register(camera);

        // Mesh
        var meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = Build2DMesh(width / 100.0f, height / 100.0f);
        GCHandler.Register(meshFilter.mesh);

        var meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = minigamePrefab?.DefaultMaterial;

        // Render Texture
        var pixelArtMode = GameConfiguration.CurrentMap?.properties.pixelArtMode ?? false;
        var renderTexture = RenderTexture.GetTemporary(
            width,
            height,
            16,
            RenderTextureFormat.ARGB32
        );
        renderTexture.filterMode = pixelArtMode ? FilterMode.Point : FilterMode.Bilinear;
        camera.targetTexture = renderTexture;
        meshRenderer.material.SetTexture(MainTex, renderTexture);
        GCHandler.Register(new DisposableRenderTex(renderTexture));
    }

    private static Mesh Build2DMesh(float width, float height)
    {
        var mesh = new Mesh();
        mesh.vertices = new Vector3[4]
        {
            new(-width / 2, -height / 2, 0),
            new(width / 2, -height / 2, 0),
            new(-width / 2, height / 2, 0),
            new(width / 2, height / 2, 0)
        };
        mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
        mesh.uv = new Vector2[]
        {
            new(0, 0),
            new(1, 0),
            new(0, 1),
            new(1, 1)
        };
        mesh.RecalculateNormals();
        return mesh;
    }

    /// <summary>
    ///     Destroy() doesn't release from memory
    ///     This replaces it with RenderTexture.ReleaseTemporary()
    /// </summary>
    private class DisposableRenderTex(RenderTexture tex) : IDisposable
    {
        public void Dispose()
        {
            RenderTexture.ReleaseTemporary(tex);
        }
    }
}