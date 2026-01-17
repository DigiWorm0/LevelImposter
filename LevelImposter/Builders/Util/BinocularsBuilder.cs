using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

internal class BinocularsBuilder : IElemBuilder
{
    public static float OrthographicSize;
    public static Vector2 LastBinocularsPos;
    public static Vector3 CameraOffset;

    public void OnPreBuild()
    {
        OrthographicSize = 3f;
        LastBinocularsPos = Vector2.zero;
        CameraOffset = Vector2.zero;
    }

    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-cams4")
            return;

        // Building is done by UtilBuilder, this handles Binoculars properties
        OrthographicSize = elem.properties.camZoom ?? 3.0f;
        LastBinocularsPos = Vector2.zero;
        CameraOffset = new Vector3(
            elem.properties.camXOffset ?? 0,
            elem.properties.camYOffset ?? 0,
            0
        );
    }
}