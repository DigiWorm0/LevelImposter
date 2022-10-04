using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Events;
using UnityEngine;
using LevelImposter.DB;
using TMPro;

namespace LevelImposter.Core
{
    public class RoomNameBuilder : IElemBuilder
    {
        private int _nameCount = 0;

        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-room" || elem.properties.isRoomNameVisible == false)
                return;

            MapBehaviour mapBehaviour = MinimapBuilder.GetMinimap();

            // Clone
            Transform roomNames = mapBehaviour.transform.GetChild(mapBehaviour.transform.childCount - 1);
            GameObject roomNameClone = roomNames.GetChild(0).gameObject;

            // Object
            GameObject roomName = UnityEngine.Object.Instantiate(roomNameClone, roomNames);
            roomName.name = elem.name;
            roomName.layer = (int)Layer.UI;
            roomName.transform.localPosition = new Vector3(
                elem.x * MinimapBuilder.MinimapScale,
                elem.y * MinimapBuilder.MinimapScale,
                -1
            );

            // Text
            UnityEngine.Object.Destroy(roomName.GetComponent<TextTranslatorTMP>());
            TextMeshPro roomText = roomName.GetComponent<TextMeshPro>();
            roomText.text = elem.name.Replace("\\n", "\n");
            roomText.fontSizeMin = roomText.fontSizeMax;
            roomText.alignment = TextAlignmentOptions.Bottom;
            roomText.enabled = true;
            _nameCount++;

            // Transform
            RectTransform rectTransform = roomName.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(10, 0);
        }

        public void PostBuild()
        {
            MapBehaviour mapBehaviour = MinimapBuilder.GetMinimap();
            Transform roomNames = mapBehaviour.transform.GetChild(mapBehaviour.transform.childCount - 1);

            while (roomNames.childCount > _nameCount)
                UnityEngine.Object.DestroyImmediate(roomNames.GetChild(0).gameObject);
            _nameCount = 0; 
        }
    }
}
