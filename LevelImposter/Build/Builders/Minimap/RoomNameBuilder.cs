using LevelImposter.Build.Attributes;
using LevelImposter.Core.Models;
using TMPro;
using UnityEngine;

namespace LevelImposter.Build.Builders.Minimap;

internal static class RoomNameBuilder
{
    private static int _nameCount;

    [MapBuilder(Priority = Priority.FIRST)]
    public static void Reset()
    {
        _nameCount = 0;
    }

    [ElementBuilder(
        Target = MapTarget.Game,
        ElementTypes = ["util-room"]
    )]
    public static void Build(ShipStatus shipStatus, LIElement element)
    {
        // Check Visibility
        var isMinimapVisible = element.properties.isRoomNameVisible ?? true;
        if (!isMinimapVisible)
            return;

        // Minimap
        var mapBehaviour = MinimapBuilder.GetMinimap();

        // Clone
        var roomNames = mapBehaviour.transform.GetChild(mapBehaviour.transform.childCount - 1);
        var roomNameClone = roomNames.GetChild(0).gameObject;

        // Object
        var mapScale = shipStatus.MapScale;
        var roomName = Object.Instantiate(roomNameClone, roomNames);
        roomName.name = element.name;
        roomName.layer = (int)Layer.UI;
        roomName.transform.localPosition = new Vector3(
            element.x / mapScale,
            element.y / mapScale,
            -1
        );

        // Text
        Object.Destroy(roomName.GetComponent<TextTranslatorTMP>());
        var roomText = roomName.GetComponent<TextMeshPro>();
        roomText.text = element.name.Replace("\\n", "\n");
        roomText.fontSizeMin = roomText.fontSizeMax;
        roomText.alignment = TextAlignmentOptions.Bottom;
        roomText.enabled = true;
        _nameCount++;

        // Transform
        var rectTransform = roomName.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(10, 0);
    }

    [MapBuilder(Priority = Priority.LAST)]
    public static void CleanupMapChildren()
    {
        var mapBehaviour = MinimapBuilder.GetMinimap();
        var roomNames = mapBehaviour.transform.GetChild(mapBehaviour.transform.childCount - 1);

        while (roomNames.childCount > _nameCount)
            Object.DestroyImmediate(roomNames.GetChild(0).gameObject);
    }
}