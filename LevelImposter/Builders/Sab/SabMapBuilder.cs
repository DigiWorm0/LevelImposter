using System;
using System.Collections.Generic;
using LevelImposter.Build.Attributes;
using LevelImposter.Builders.Generic;
using LevelImposter.Builders.Minimap;
using LevelImposter.Builders.Util;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using LevelImposter.DB;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LevelImposter.Builders.Sab;

internal static class SabMapBuilder
{
    private static readonly Dictionary<SystemTypes, MapRoom> MapRoomDB = new();
    private static Material? _btnMat;

    private static Sprite? _commsBtnSprite;
    private static Sprite? _doorsBtnSprite;
    private static Sprite? _lightsBtnSprite;
    private static Sprite? _mixupBtnSprite;
    private static Sprite? _oxygenBtnSprite;
    private static Sprite? _reactorBtnSprite;

    [MapBuilder(Priority = Priority.FIRST)]
    public static void Reset()
    {
        MapRoomDB.Clear();
    }

    [MapBuilder(Priority = Priority.LAST)]
    public static void CleanupInfectedOverlay()
    {
        // TODO: Re-implement sabotage button check
        // if (_hasSabConsoles && !_hasSabButtons)
        //     LILogger.Warn("Map does not include sabotage buttons");

        var mapBehaviour = MinimapBuilder.GetMinimap();
        var infectedOverlay = mapBehaviour.infectedOverlay;

        while (infectedOverlay.transform.childCount > MapRoomDB.Count + MinimapSpriteBuilder.SabCount)
            Object.DestroyImmediate(infectedOverlay.transform.GetChild(0).gameObject);
    }

    [ElementBuilder(
        Target = MapTarget.Game,
        ElementTypes =
        [
            "sab-btnreactor",
            "sab-btnoxygen",
            "sab-btnlights",
            "sab-btncomms",
            "sab-btndoors",
            "sab-btnmixup"
        ])
    ]
    public static void Build(ShipStatus shipStatus, LIElement element, GameObject gameObject)
    {
        // Assets
        var mapBehaviour = MinimapBuilder.GetMinimap();
        var infectedOverlay = mapBehaviour.infectedOverlay;
        if (_btnMat == null)
            GetAllAssets();
        if (_btnMat == null ||
            _lightsBtnSprite == null ||
            _doorsBtnSprite == null ||
            _oxygenBtnSprite == null ||
            _reactorBtnSprite == null ||
            _commsBtnSprite == null ||
            _mixupBtnSprite == null)
        {
            LILogger.Warn("1 or more sabotage map sprites were not found");
            return;
        }

        // System
        var systemType = RoomBuilder.GetParentOrDefault(element);

        // Map Room
        MapRoom mapRoom;
        if (MapRoomDB.ContainsKey(systemType))
        {
            mapRoom = MapRoomDB[systemType];
        }
        else
        {
            GameObject roomObj = new(element.name);
            roomObj.transform.SetParent(infectedOverlay.transform);
            roomObj.transform.localPosition = Vector3.zero;

            mapRoom = roomObj.AddComponent<MapRoom>();
            mapRoom.Parent = infectedOverlay;
            mapRoom.room = systemType;

            MapRoomDB.Add(systemType, mapRoom);

            var rooms = new MapRoom[MapRoomDB.Count];
            MapRoomDB.Values.CopyTo(rooms, 0);
            infectedOverlay.rooms = rooms;
        }

        // Button
        var mapScale = shipStatus.MapScale;
        GameObject sabButton = new(element.name);
        sabButton.layer = (int)Layer.UI;
        sabButton.transform.SetParent(mapRoom.transform);
        sabButton.transform.localPosition = new Vector3(
            element.x / mapScale,
            element.y / mapScale,
            -25.0f
        );
        sabButton.transform.localScale = new Vector3(element.xScale, element.yScale, 1);
        sabButton.transform.localRotation = Quaternion.Euler(0, 0, element.rotation);

        var collider = sabButton.AddComponent<CircleCollider2D>();
        collider.radius = 0.425f;
        collider.isTrigger = true;

        var btnRenderer = sabButton.AddComponent<SpriteRenderer>();
        if (mapRoom.special != null)
            LILogger.Warn("Only 1 sabotage is supported per room");

        var button = sabButton.AddComponent<ButtonBehavior>();
        Action btnAction;
        Sprite btnSprite;
        switch (element.type)
        {
            case "sab-btnreactor":
                btnSprite = _reactorBtnSprite;
                btnAction = mapRoom.SabotageReactor;
                mapRoom.special = btnRenderer;
                break;
            case "sab-btnoxygen":
                btnSprite = _oxygenBtnSprite;
                btnAction = mapRoom.SabotageOxygen;
                mapRoom.special = btnRenderer;
                break;
            case "sab-btncomms":
                btnSprite = _commsBtnSprite;
                btnAction = mapRoom.SabotageComms;
                mapRoom.special = btnRenderer;
                break;
            case "sab-btnlights":
                btnSprite = _lightsBtnSprite;
                btnAction = mapRoom.SabotageLights;
                mapRoom.special = btnRenderer;
                break;
            case "sab-btnmixup":
                btnSprite = _mixupBtnSprite;
                btnAction = mapRoom.SabotageMushroomMixup;
                mapRoom.special = btnRenderer;
                break;
            case "sab-btndoors":
                btnSprite = _doorsBtnSprite;
                btnAction = mapRoom.SabotageDoors;
                mapRoom.door = btnRenderer;
                //sabButton.transform.localScale *= 0.8f;
                break;
            default:
                LILogger.Warn($"{element.name} has unknown sabotage button type: {element.type}");
                return;
        }

        btnRenderer.sprite = btnSprite;
        btnRenderer.material = _btnMat;
        button.OnClick.AddListener(btnAction);

        // Load Sprite
        var spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        SpriteBuilder.OnSpriteLoad += (loadedElem, _) =>
        {
            if (loadedElem.id != element.id || btnRenderer == null)
                return;
            btnRenderer.sprite = spriteRenderer.sprite;
            btnRenderer.color = spriteRenderer.color;
            Object.Destroy(gameObject);
        };
    }

    /// <summary>
    ///     Collects all necessary sprites and assets for map
    /// </summary>
    private static void GetAllAssets()
    {
        // TODO: Move Assets to a SubDB

        // Polus
        var polusShip = PrefabDB.GetObject("ss-polus");
        {
            var polusShipStatus = polusShip?.GetComponent<ShipStatus>();
            var polusOverlay = polusShipStatus?.MapPrefab.infectedOverlay;
            if (polusOverlay == null)
                return;

            _commsBtnSprite = GetSprite(polusOverlay, "Comms", "bomb"); // um...BOMB!?
            _reactorBtnSprite = GetSprite(polusOverlay, "Laboratory", "meltdown");
            _doorsBtnSprite = GetSprite(polusOverlay, "Office", "Doors");
            _lightsBtnSprite = GetSprite(polusOverlay, "Electrical", "lightsOut");
            _btnMat = polusOverlay.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().material;
        }

        // Mira
        var miraShip = PrefabDB.GetObject("ss-mira");
        {
            var miraShipStatus = miraShip?.GetComponent<ShipStatus>();
            var miraOverlay = miraShipStatus?.MapPrefab.infectedOverlay;
            if (miraOverlay == null)
                return;
            _oxygenBtnSprite = GetSprite(miraOverlay, "LifeSupp", "bomb"); // Another bomb?
        }

        // Fungle
        var fungleShip = PrefabDB.GetObject("ss-fungle");
        {
            var fungleShipStatus = fungleShip?.GetComponent<ShipStatus>();
            var fungleOverlay = fungleShipStatus?.MapPrefab.infectedOverlay;
            if (fungleOverlay == null)
                return;
            _mixupBtnSprite = GetSprite(fungleOverlay, "Jungle", "mushroomMixup");
        }
    }

    /// <summary>
    ///     Searches an object for a sprite in a parent and child
    /// </summary>
    /// <param name="overlay">Object to search</param>
    /// <param name="parent">Parent object name</param>
    /// <param name="child">Child object name</param>
    /// <returns>Sprite attatched to SpriteRenderer</returns>
    private static Sprite GetSprite(InfectedOverlay overlay, string parent, string child)
    {
        return overlay.transform.Find(parent).Find(child).GetComponent<SpriteRenderer>().sprite;
    }
}