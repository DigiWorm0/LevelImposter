using LevelImposter.Core;
using LevelImposter.DB;
using UnityEngine;

namespace LevelImposter.Builders;

internal class StarfieldBuilder : IElemBuilder
{
    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-starfield")
            return;

        // Prefab
        var prefab = AssetDB.GetObject("dec-rock4");
        if (prefab == null)
            return;
        var prefabRenderer = prefab.GetComponent<SpriteRenderer>();

        // Sprite
        var spriteRenderer = obj.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            LILogger.Warn($"{elem.name} missing a sprite");
        else
            spriteRenderer.material = prefabRenderer.material;

        // Star Prefab
        var starPrefab = Object.Instantiate(obj);
        starPrefab.transform.localScale = Vector3.one;
        starPrefab.transform.localRotation = Quaternion.identity;
        var prefabComp = starPrefab.AddComponent<LIStar>();

        var count = elem.properties.starfieldCount ?? 20;
        var liStars = new LIStar[count];
        for (var i = 0; i < count; i++)
        {
            var liStar = Object.Instantiate(prefabComp, obj.transform);
            liStar.Init(elem);
            liStars[i] = liStar;
        }

        Object.Destroy(starPrefab);

        // Clones
        if (SpriteLoader.Instance == null)
        {
            LILogger.Warn("Spite Loader is not instantiated");
            return;
        }

        SpriteLoader.Instance.OnLoad += loadedElem =>
        {
            if (loadedElem.id != elem.id)
                return;

            foreach (var liStar in liStars)
            {
                var starRenderer = liStar.GetComponent<SpriteRenderer>();
                starRenderer.sprite = spriteRenderer?.sprite;
                starRenderer.color = spriteRenderer?.color ?? starRenderer.color;
            }
        };

        // Disable SpriteRenderers
        SpriteRenderer[] spriteRenderers = obj.GetComponents<SpriteRenderer>();
        foreach (var renderer in spriteRenderers)
            renderer.enabled = false;

        // Disable Colliders
        Collider2D[] colliders = obj.GetComponents<Collider2D>();
        foreach (var collider in colliders)
            collider.enabled = false;
    }
}