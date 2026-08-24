using LevelImposter.Build.Attributes;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using LevelImposter.DB;
using UnityEngine;

namespace LevelImposter.Build.Builders.Util;

internal static class CamBuilder
{
    [ElementBuilder(ElementTypes = ["util-cam"])]
    public static void Build(LIElement element, GameObject gameObject)
    {
        // Prefab
        var prefab = PrefabDB.GetObject(element.type);
        if (prefab == null)
            return;
        var prefabCam = prefab.GetComponent<SurvCamera>();

        // Sprite
        gameObject.CloneSprite(prefab, true);

        // Camera
        var survCam = gameObject.AddComponent<SurvCamera>();
        survCam.CamName = element.name;
        survCam.Offset = new Vector3(
            element.properties.camXOffset ?? 0,
            element.properties.camYOffset ?? 0
        );
        survCam.CamSize = element.properties.camZoom ?? 3;
        survCam.OnAnim = prefabCam.OnAnim;
        survCam.OffAnim = prefabCam.OffAnim;
    }
}