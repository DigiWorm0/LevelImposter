using System;
using System.Collections.Generic;
using LevelImposter.Core;
using LevelImposter.DB;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LevelImposter.Builders;

public class SabMapBuilder : IElemBuilder
{
    private static readonly Dictionary<SystemTypes, MapRoom> _mapRoomDB = new();
    private Material? _btnMat;

    private Sprite? _commsBtnSprite;
    private Sprite? _doorsBtnSprite;
    private bool _hasSabButtons;

    private bool _hasSabConsoles;
    private Sprite? _lightsBtnSprite;
    private Sprite? _mixupBtnSprite;
    private Sprite? _oxygenBtnSprite;
    private Sprite? _reactorBtnSprite;

    public SabMapBuilder()
    {
        _mapRoomDB.Clear();
    }

    public void Build(LIElement elem, GameObject obj)
    {
        if (!elem.type.StartsWith("sab-") || elem.type.StartsWith("sab-door"))
            return;

        // ShipStatus
        var shipStatus = LIShipStatus.GetInstance().ShipStatus;

        _hasSabConsoles = true;

        if (!elem.type.StartsWith("sab-btn"))
            return;
        _hasSabButtons = true;

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
        var systemType = RoomBuilder.GetParentOrDefault(elem);

        // Map Room
        MapRoom mapRoom;
        if (_mapRoomDB.ContainsKey(systemType))
        {
            mapRoom = _mapRoomDB[systemType];
        }
        else
        {
            GameObject roomObj = new(elem.name);
            roomObj.transform.SetParent(infectedOverlay.transform);
            roomObj.transform.localPosition = Vector3.zero;

            mapRoom = roomObj.AddComponent<MapRoom>();
            mapRoom.Parent = infectedOverlay;
            mapRoom.room = systemType;

            _mapRoomDB.Add(systemType, mapRoom);

            var rooms = new MapRoom[_mapRoomDB.Count];
            _mapRoomDB.Values.CopyTo(rooms, 0);
            infectedOverlay.rooms = rooms;
        }

        // Button
        var mapScale = shipStatus.MapScale;
        GameObject sabButton = new(elem.name);
        sabButton.layer = (int)Layer.UI;
        sabButton.transform.SetParent(mapRoom.transform);
        sabButton.transform.localPosition = new Vector3(
            elem.x / mapScale,
            elem.y / mapScale,
            -25.0f
        );
        sabButton.transform.localScale = new Vector3(elem.xScale, elem.yScale, 1);
        sabButton.transform.localRotation = Quaternion.Euler(0, 0, elem.rotation);

        var collider = sabButton.AddComponent<CircleCollider2D>();
        collider.radius = 0.425f;
        collider.isTrigger = true;

        var btnRenderer = sabButton.AddComponent<SpriteRenderer>();
        if (mapRoom.special != null)
            LILogger.Warn("Only 1 sabotage is supported per room");

        var button = sabButton.AddComponent<ButtonBehavior>();
        Action btnAction;
        Sprite btnSprite;
        switch (elem.type)
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
                LILogger.Warn($"{elem.name} has unknown sabotage button type: {elem.type}");
                return;
        }

        btnRenderer.sprite = btnSprite;
        btnRenderer.material = _btnMat;
        button.OnClick.AddListener(btnAction);

        // Sprite Renderer
        if (SpriteLoader.Instance == null)
        {
            LILogger.Warn("Spite Loader is not instantiated");
            return;
        }

        var spriteRenderer = obj.GetComponent<SpriteRenderer>();
        SpriteLoader.Instance.OnLoad += loadedElem =>
        {
            if (loadedElem.id != elem.id || btnRenderer == null)
                return;
            btnRenderer.sprite = spriteRenderer.sprite;
            btnRenderer.color = spriteRenderer.color;
            Object.Destroy(obj);
        };
    }


    public void PostBuild()
    {
        if (_hasSabConsoles && !_hasSabButtons)
            LILogger.Warn("Map does not include sabotage buttons");

        var mapBehaviour = MinimapBuilder.GetMinimap();
        var infectedOverlay = mapBehaviour.infectedOverlay;

        while (infectedOverlay.transform.childCount > _mapRoomDB.Count + MinimapSpriteBuilder.SabCount)
            Object.DestroyImmediate(infectedOverlay.transform.GetChild(0).gameObject);
    }

    /// <summary>
    ///     Collects all necessary sprites and assets for map
    /// </summary>
    private void GetAllAssets()
    {
        // TODO: Move Assets to a SubDB

        // Polus
        var polusShip = AssetDB.GetObject("ss-polus");
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
        var miraShip = AssetDB.GetObject("ss-mira");
        {
            var miraShipStatus = miraShip?.GetComponent<ShipStatus>();
            var miraOverlay = miraShipStatus?.MapPrefab.infectedOverlay;
            if (miraOverlay == null)
                return;
            _oxygenBtnSprite = GetSprite(miraOverlay, "LifeSupp", "bomb"); // Another bomb?
        }

        // Fungle
        var fungleShip = AssetDB.GetObject("ss-fungle");
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
    private Sprite GetSprite(InfectedOverlay overlay, string parent, string child)
    {
        return overlay.transform.Find(parent).Find(child).GetComponent<SpriteRenderer>().sprite;
    }
}