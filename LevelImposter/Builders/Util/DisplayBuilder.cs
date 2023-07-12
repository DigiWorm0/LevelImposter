using UnityEngine;
using LevelImposter.Core;
using LevelImposter.DB;
using System;

namespace LevelImposter.Builders
{
    class DisplayBuilder : IElemBuilder
    {
        private const int DEFAULT_WIDTH = 330;
        private const int DEFAULT_HEIGHT = 220;

        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-display")
                return;

            // ShipStatus
            var shipStatus = LIShipStatus.Instance?.ShipStatus;
            if (shipStatus == null)
                throw new MissingShipException();

            // Prefab
            var minigamePrefab = AssetDB.GetObject("util-cams")?.GetComponent<SystemConsole>().MinigamePrefab.Cast<PlanetSurveillanceMinigame>();
            obj.layer = (int)Layer.Objects;

            // Options
            int width = elem.properties.displayWidth ?? DEFAULT_WIDTH;
            int height = elem.properties.displayHeight ?? DEFAULT_HEIGHT;

            // Camera
            var cameraObject = new GameObject("DisplayCamera");
            cameraObject.layer = (int)Layer.UI;
            cameraObject.transform.parent = shipStatus.transform;
            cameraObject.transform.position = new Vector3(
                (elem.properties.camXOffset ?? 0) + obj.transform.position.x,
                (elem.properties.camYOffset ?? 0) + obj.transform.position.y,
                0.0f
            );

            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = elem.properties.camZoom ?? 3;
            camera.cullingMask = 0b1111001100010111; // Include Shadows: 0b10111001100010111
            camera.farClipPlane = 1000.0f;
            camera.nearClipPlane = -1000.0f;
            GCHandler.Register(camera);

            // Mesh
            var meshFilter = obj.AddComponent<MeshFilter>();
            meshFilter.mesh = MapUtils.Build2DMesh(width / 100.0f, height / 100.0f);
            GCHandler.Register(meshFilter.mesh);

            var meshRenderer = obj.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = minigamePrefab?.DefaultMaterial;

            // Render Texture
            bool pixelArtMode = LIShipStatus.Instance?.CurrentMap?.properties.pixelArtMode ?? false;
            var renderTexture = RenderTexture.GetTemporary(
                width,
                height,
                16,
                RenderTextureFormat.ARGB32
            );
            renderTexture.filterMode = pixelArtMode ? FilterMode.Point : FilterMode.Bilinear;
            camera.targetTexture = renderTexture;
            meshRenderer.material.SetTexture("_MainTex", renderTexture);
            GCHandler.Register(new DisposableRenderTex(renderTexture)); 
        }

        public void PostBuild() { }

        /// <summary>
        /// Destroy() doesn't release from memory
        /// This replaces it with RenderTexture.ReleaseTemporary()
        /// </summary>
        public class DisposableRenderTex : IDisposable
        {
            private RenderTexture _tex;

            public DisposableRenderTex(RenderTexture tex)
            {
                _tex = tex;
            }

            public void Dispose()
            {
                RenderTexture.ReleaseTemporary(_tex);
            }
        }
    }
}