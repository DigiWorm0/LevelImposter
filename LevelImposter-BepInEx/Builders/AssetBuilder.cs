using LevelImposter.Map;
using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Builders
{
    class AssetBuilder : Builder
    {
        private PolusHandler    polus;
        private TaskBuilder     taskBuilder;
        private CustomBuilder   customBuilder;
        private DummyBuilder    dummyBuilder;
        private UtilBuilder     utilBuilder;
        private DecBuilder      decBuilder;
        private RoomBuilder     roomBuilder;
        private SabBuilder      sabBuilder;
        private ShipRoomBuilder shipRoomBuilder;
        private VentBuilder     ventBuilder;
        private CamBuilder      camBuilder;
        private SpawnBuilder    spawnBuilder;

        public AssetBuilder(PolusHandler polus)
        {
            this.polus = polus;

            taskBuilder     = new TaskBuilder(polus);
            customBuilder   = new CustomBuilder(polus);
            dummyBuilder    = new DummyBuilder(polus);
            utilBuilder     = new UtilBuilder(polus);
            decBuilder      = new DecBuilder(polus);
            roomBuilder     = new RoomBuilder(polus);
            sabBuilder      = new SabBuilder(polus);
            shipRoomBuilder = new ShipRoomBuilder(polus);
            ventBuilder     = new VentBuilder(polus);
            camBuilder      = new CamBuilder(polus);
            spawnBuilder    = new SpawnBuilder(polus);
        }

        public bool Build(MapAsset asset)
        {
            try
            {
                if (asset.spriteType == "existing")
                {
                    if (asset.type == "util-player")
                        return dummyBuilder.Build(asset);
                    else if (asset.type == "util-room")
                        return shipRoomBuilder.Build(asset);
                    else if (asset.type.StartsWith("util-vent"))
                        return ventBuilder.Build(asset);
                    else if (asset.type.StartsWith("util-spawn"))
                        return spawnBuilder.Build(asset);
                    else if (asset.type == "util-cam")
                        return camBuilder.Build(asset);
                    else if (asset.type.StartsWith("util-"))
                        return utilBuilder.Build(asset);
                    else if (asset.type.StartsWith("dec-"))
                        return decBuilder.Build(asset);
                    else if (asset.type.StartsWith("room-"))
                        return roomBuilder.Build(asset);
                    else if (asset.type.StartsWith("task-"))
                        return taskBuilder.Build(asset);
                    else if (asset.type.StartsWith("sab-"))
                        return sabBuilder.Build(asset);
                }
                else if (asset.spriteType == "custom")
                    return customBuilder.Build(asset);
                return false;
            }
            catch (Exception e)
            {
                LILogger.LogInfo(e.Message + "\n" + e.StackTrace);
                return false;
            }
        }

        public void Finish()
        {
            // TODO
        }

        public static UnhollowerBaseLib.Il2CppReferenceArray<T> AddToArr<T>(UnhollowerBaseLib.Il2CppReferenceArray<T> arr, T value) where T : UnhollowerBaseLib.Il2CppObjectBase
        {
            List<T> list = new List<T>(arr);
            list.Add(value);
            return list.ToArray();
        }

        public static void BuildColliders(MapAsset asset, GameObject obj, float scale = 1.0f)
        {
            // Colliders
            GameObject shadowObj = new GameObject("Shadows");
            shadowObj.layer = (int)Layer.Shadow;
            shadowObj.transform.SetParent(obj.transform);
            foreach (MapCollider collider in asset.colliders)
            {
                EdgeCollider2D edgeCollider = obj.AddComponent<EdgeCollider2D>();
                edgeCollider.SetPoints(collider.GetPoints(scale, scale));

                if (collider.blocksLight)
                {
                    EdgeCollider2D lightCollider = shadowObj.AddComponent<EdgeCollider2D>();
                    lightCollider.SetPoints(collider.GetPoints(scale, scale));
                }
            }
        }

        public static Sprite SpriteFromBase64(byte[] data)
        {
            Texture2D tex = new Texture2D(1, 1);
            ImageConversion.LoadImage(tex, data);
            return Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
        }

        public static Sprite SpriteFromBase64(string b64)
        {
            if (string.IsNullOrEmpty(b64))
                return null;

            // Base64
            string base64 = b64.Substring(b64.IndexOf(",") + 1);
            byte[] data;
            try
            {
                data = System.Convert.FromBase64String(base64);
                return SpriteFromBase64(data);
            }
            catch
            {
                LILogger.LogError("Could not parse custom asset texture");
                return null;
            }

         }
    }
}
