using Il2CppSystem.Collections.Generic;
using LevelImposter.Core;
using LevelImposter.DB;
using UnityEngine;

namespace LevelImposter.Builders
{
    public class SporeBuilder : IElemBuilder
    {
        private static List<Mushroom> _mushrooms = new();
        public static List<Mushroom> Mushrooms => _mushrooms;

        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-spore")
                return;

            // Prefab
            var prefab = AssetDB.GetObject(elem.type);
            if (prefab == null)
                return;
            var prefabSpore = prefab.GetComponent<Mushroom>();

            // Sprite
            var spriteRenderer = MapUtils.CloneSprite(obj, prefab, true);

            // Screen Mask
            var screenMaskPrefab = prefab.transform.FindChild("SporeScreenMask").gameObject;
            var screenMaskObj = new GameObject("ScreenMask");
            screenMaskObj.transform.parent = obj.transform;
            screenMaskObj.transform.localPosition = new Vector3(0, 0, 4.3f);
            var screenMaskRenderer = MapUtils.CloneSprite(screenMaskObj, screenMaskPrefab, true);

            // Screen Graphic
            var screenGraphicPrefab = prefab.transform.FindChild("SporeScreenGraphic").gameObject;
            var screenGraphicObj = new GameObject("ScreenGraphic");
            screenGraphicObj.transform.parent = obj.transform;
            screenGraphicObj.transform.localPosition = new Vector3(0, 0, -10.0f);
            var sceenGraphicRenderer = MapUtils.CloneSprite(screenGraphicObj, screenGraphicPrefab, true);

            // Set Color
            if (elem.properties.gasColor != null)
                sceenGraphicRenderer.color = elem.properties.gasColor.ToUnity();

            // Collider
            CircleCollider2D collider = obj.AddComponent<CircleCollider2D>();
            collider.radius = elem.properties.range ?? 0.25f;
            collider.isTrigger = true;

            // Mushroom
            Mushroom mushroom = obj.AddComponent<Mushroom>();
            mushroom.id = _mushrooms.Count;
            mushroom.mushroomCollider = collider;
            mushroom.mushroom = spriteRenderer;
            mushroom.mushroomAnimator = spriteRenderer.GetComponent<PowerTools.SpriteAnim>();
            mushroom.sporeMask = screenMaskObj;
            mushroom.sporeCloudMaskAnimator = screenMaskRenderer.GetComponent<PowerTools.SpriteAnim>();
            mushroom.spores = screenMaskRenderer;
            mushroom.sporeCloudAnimator = screenGraphicObj.GetComponent<PowerTools.SpriteAnim>();
            mushroom.mushroomIdle = prefabSpore.mushroomIdle;
            mushroom.mushroomAppear = prefabSpore.mushroomAppear;
            mushroom.mushroomSteppedOn = prefabSpore.mushroomSteppedOn;
            mushroom.sporeCloudIdle = prefabSpore.sporeCloudIdle;
            mushroom.sporeCloudAppear = prefabSpore.sporeCloudAppear;
            mushroom.sporeCloudDisappear = prefabSpore.sporeCloudDisappear;
            mushroom.spawnSound = prefabSpore.spawnSound;
            mushroom.activateSporeSound = prefabSpore.activateSporeSound;

            mushroom.ResetState();
            mushroom.enabled = true;
            _mushrooms.Add(mushroom);
        }

        public void PostBuild() { }
    }
}
