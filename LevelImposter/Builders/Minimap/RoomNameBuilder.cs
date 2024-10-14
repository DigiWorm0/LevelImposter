using LevelImposter.Core;
using TMPro;
using UnityEngine;

namespace LevelImposter.Builders;

public class RoomNameBuilder : IElemBuilder
{
    private int _nameCount;

    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-room")
            return;

        // Check Visibility
        var isMinimapVisible = elem.properties.isRoomNameVisible ?? true;
        if (!isMinimapVisible)
            return;

        // ShipStatus
        var shipStatus = LIShipStatus.GetShip();

        // Minimap
        var mapBehaviour = MinimapBuilder.GetMinimap();

        // Clone
        var roomNames = mapBehaviour.transform.GetChild(mapBehaviour.transform.childCount - 1);
        var roomNameClone = roomNames.GetChild(0).gameObject;

        // Object
        var mapScale = shipStatus.MapScale;
        var roomName = Object.Instantiate(roomNameClone, roomNames);
        roomName.name = elem.name;
        roomName.layer = (int)Layer.UI;
        roomName.transform.localPosition = new Vector3(
            elem.x / mapScale,
            elem.y / mapScale,
            -1
        );

        // Text
        Object.Destroy(roomName.GetComponent<TextTranslatorTMP>());
        var roomText = roomName.GetComponent<TextMeshPro>();
        roomText.text = elem.name.Replace("\\n", "\n");
        roomText.fontSizeMin = roomText.fontSizeMax;
        roomText.alignment = TextAlignmentOptions.Bottom;
        roomText.enabled = true;
        _nameCount++;

        // Transform
        var rectTransform = roomName.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(10, 0);
    }

    public void OnCleanup()
    {
        var mapBehaviour = MinimapBuilder.GetMinimap();
        var roomNames = mapBehaviour.transform.GetChild(mapBehaviour.transform.childCount - 1);

        while (roomNames.childCount > _nameCount)
            Object.DestroyImmediate(roomNames.GetChild(0).gameObject);
    }
}