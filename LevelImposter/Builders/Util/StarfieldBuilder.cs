using LevelImposter.Core;
using LevelImposter.DB;
using UnityEngine;

namespace LevelImposter.Builders
{
    class StarfieldBuilder : IElemBuilder
    {
        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-starfield")
                return;

            // Prefab
            var prefab = AssetDB.GetObject("dec-rock4");
            if (prefab == null)
                return;
            var prefabRenderer = prefab.GetComponent<SpriteRenderer>();

            // Sprite
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                LILogger.Warn($"{elem.name} missing a sprite");
                return;
            }
            spriteRenderer.material = prefabRenderer.material;

            // Star Prefab
            GameObject starPrefab = UnityEngine.Object.Instantiate(obj);
            starPrefab.transform.localScale = Vector3.one;
            starPrefab.transform.localRotation = Quaternion.identity;
            LIStar prefabComp = starPrefab.AddComponent<LIStar>();

            int count = elem.properties.starfieldCount ?? 20;
            LIStar[] liStars = new LIStar[count];
            for (int i = 0; i < count; i++)
            {
                LIStar liStar = UnityEngine.Object.Instantiate(prefabComp, obj.transform);
                liStar.Init(elem);
                liStars[i] = liStar;
            }
            UnityEngine.Object.Destroy(starPrefab);

            // Clones
            if (SpriteLoader.Instance == null)
            {
                LILogger.Warn("Spite Loader is not instantiated");
                return;
            }
            SpriteLoader.Instance.OnLoad += (LIElement loadedElem) =>
            {
                if (loadedElem.id != elem.id || liStars == null)
                    return;
                foreach (LIStar liStar in liStars)
                {
                    SpriteRenderer starRenderer = liStar.GetComponent<SpriteRenderer>();
                    starRenderer.sprite = spriteRenderer.sprite;
                    starRenderer.color = spriteRenderer.color;
                }
                liStars = null;
            };

            // Disable SpriteRenderers
            SpriteRenderer[] spriteRenderers = obj.GetComponents<SpriteRenderer>();
            foreach (SpriteRenderer renderer in spriteRenderers)
                renderer.enabled = false;

            // Disable Colliders
            Collider2D[] colliders = obj.GetComponents<Collider2D>();
            foreach (Collider2D collider in colliders)
                collider.enabled = false;
        }

        public void PostBuild() { }
    }
}