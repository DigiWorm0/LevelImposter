using LevelImposter.Core;
using LevelImposter.Lobby;
using UnityEngine;
using UnityEngine.Events;

namespace LevelImposter.Builders.Lobby;

public class LobbyWardrobeConsoleBuilder : IElemBuilder
{
    public void OnBuild(LIElement elem, GameObject gameObject)
    {
        if (elem.type != "util-lobbywardrobe")
            return;

        // Load Prefab
        var prefab = LobbyDropshipPrefab.GetObjectFromPrefab("panel_Wardrobe/Console");
        var prefabConsole = prefab.GetComponent<OptionsConsole>();
        
        // Build Console
        var console = gameObject.AddComponent<OptionsConsole>();
        console.CustomPosition = prefabConsole.CustomPosition;
        console.HostOnly = false;
        console.MenuPrefab = prefabConsole.MenuPrefab;
        console.Outline = MapUtils.CloneSprite(gameObject, prefab.transform.parent.gameObject);
        console.CustomUseIcon = ImageNames.WardrobeButton;
        
        // Button
        var button = gameObject.AddComponent<ButtonBehavior>();
        button.OnMouseOver = new UnityEvent();
        button.OnMouseOut = new UnityEvent();
        button.OnClick.AddListener((System.Action)console.Use);
        
        // Colliders
        MapUtils.CreateDefaultColliders(gameObject, prefab);
        
    }
}