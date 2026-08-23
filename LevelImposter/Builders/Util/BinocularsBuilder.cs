using LevelImposter.Build;
using LevelImposter.Build.Attributes;
using LevelImposter.Core.Models;
using UnityEngine;

namespace LevelImposter.Builders.Util;

internal static class BinocularsBuilder
{
    public static float OrthographicSize;
    public static Vector2 LastBinocularsPos;
    public static Vector3 CameraOffset;

    [MapBuilder(Priority = Priority.FIRST)]
    public static void ResetValues()
    {
        OrthographicSize = 3f;
        LastBinocularsPos = Vector2.zero;
        CameraOffset = Vector2.zero;
    }

    [ElementBuilder(ElementTypes = ["util-cams4"])]
    public static void Build(LIElement elem, GameObject obj)
    {
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