using LevelImposter.Core;
using LevelImposter.DB;
using UnityEngine;

namespace LevelImposter.Builders;

public class CamBuilder : IElemBuilder
{
    public void Build(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-cam")
            return;

        // Prefab
        var prefab = AssetDB.GetObject(elem.type);
        if (prefab == null)
            return;
        var prefabCam = prefab.GetComponent<SurvCamera>();

        // Sprite
        MapUtils.CloneSprite(obj, prefab, true);

        // Camera
        var survCam = obj.AddComponent<SurvCamera>();
        survCam.CamName = elem.name;
        survCam.Offset = new Vector3(
            elem.properties.camXOffset ?? 0,
            elem.properties.camYOffset ?? 0
        );
        survCam.CamSize = elem.properties.camZoom ?? 3;
        survCam.OnAnim = prefabCam.OnAnim;
        survCam.OffAnim = prefabCam.OffAnim;
    }

    public void PostBuild()
    {
    }
}