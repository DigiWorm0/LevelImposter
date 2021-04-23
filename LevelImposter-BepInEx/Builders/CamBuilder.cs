using HarmonyLib;
using LevelImposter.DB;
using LevelImposter.Map;
using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace LevelImposter.Builders
{
    class CamBuilder : Builder
    {
        private PolusHandler polus;

        public CamBuilder(PolusHandler polus)
        {
            this.polus = polus;
        }

        public bool PreBuild(MapAsset asset)
        {
            if (asset.type != "util-cam")
                return true;
            UtilData utilData = AssetDB.utils[asset.type];

            // Object
            GameObject obj = new GameObject(asset.type);

            // Sprite Renderer
            SpriteRenderer spriteRenderer = obj.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = utilData.SpriteRenderer.sprite;
            spriteRenderer.material = utilData.SpriteRenderer.material;
            obj.layer = (int)Layer.ShortObjects;

            // Camera
            SurvCamera camClone = utilData.GameObj.GetComponent<SurvCamera>();
            SurvCamera camera = obj.AddComponent<SurvCamera>();
            camera.CamName = asset.name;
            camera.camNameString = camClone.camNameString;
            camera.NewName = camClone.NewName;
            camera.OffAnim = camClone.OffAnim;
            camera.CamSize = camClone.CamSize;
            camera.CamAspect = camClone.CamAspect;
            camera.Offset = camClone.Offset;
            camera.OnAnim = camClone.OnAnim;
            camera.Images = camClone.Images;
            polus.shipStatus.AllCameras = AssetHelper.AddToArr(polus.shipStatus.AllCameras, camera);

            // Colliders
            AssetHelper.BuildColliders(asset, obj);

            // Add to Polus
            polus.Add(obj, asset);

            return true;
        }

        public bool PostBuild()
        {
            return false;
        }
    }
}
