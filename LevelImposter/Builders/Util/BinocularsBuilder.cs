using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

internal class BinocularsBuilder : IElemBuilder
{
    public static float OrthographicSize = 3f;
    public static Vector2 LastBinocularsPos = Vector2.zero;
    public static Vector3 CameraOffset = Vector2.zero;

    public void Build(LIElement elem, GameObject obj)
    {
        if (!(elem.type == "util-cams4"))
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

    public void PostBuild()
    {
    }
}