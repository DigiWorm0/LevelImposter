using LevelImposter.Core;
using LevelImposter.DB;
using UnityEngine;

namespace LevelImposter.Builders;

internal class FilterBuilder : IElemBuilder
{
    public void Build(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-filter")
            return;

        // Prefab
        var sporePrefab = AssetDB.GetObject("util-spore");
        if (sporePrefab == null)
            return;
        var maskPrefab = sporePrefab.transform.FindChild("SporeScreenMask").gameObject;
        var maskPrefabRenderer = maskPrefab.GetComponent<SpriteRenderer>();

        // Create Sprite
        var spriteRenderer = obj.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = obj.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = MapUtils.GetDefaultSquare();
            spriteRenderer.color = elem.properties.color?.ToUnity() ?? Color.white;
        }

        // Create Mask
        var maskObj = Object.Instantiate(obj, obj.transform);
        maskObj.name = "Mask";
        maskObj.transform.localScale = Vector3.one;
        maskObj.transform.position = new Vector3(
            obj.transform.position.x,
            obj.transform.position.y,
            10.0f
        );
        var maskRenderer = maskObj.GetComponent<SpriteRenderer>();
        maskRenderer.material = maskPrefabRenderer.material;

        // Custom Sprite Onload
        if (SpriteLoader.Instance == null)
        {
            LILogger.Warn("Spite Loader is not instantiated");
            return;
        }

        SpriteLoader.Instance.OnLoad += loadedElem =>
        {
            if (loadedElem.id != elem.id || maskRenderer == null)
                return;
            maskRenderer.sprite = spriteRenderer.sprite;
            maskRenderer.color = spriteRenderer.color;
        };

        // Set Layer
        obj.layer = (int)Layer.Ship;
        maskObj.layer = (int)Layer.Ship;
    }

    public void PostBuild()
    {
    }
}