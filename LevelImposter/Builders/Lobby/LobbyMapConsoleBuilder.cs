using System;
using LevelImposter.Core;
using LevelImposter.Lobby;
using UnityEngine;
using UnityEngine.Events;

namespace LevelImposter.Builders.Lobby;

public class LobbyMapConsoleBuilder : IElemBuilder
{
    private static Sprite? _defaultConsoleSprite;
    
    public void OnBuild(LIElement elem, GameObject gameObject)
    {
        if (elem.type != "util-lobbymaps")
            return;

        Build(gameObject);
    }

    /// <summary>
    /// Builds a Lobby Map Console at the specified GameObject or creates a new one if null
    /// </summary>
    /// <param name="gameObject">The GameObject to build the console on, or null to create a new one</param>
    public static void Build(GameObject? gameObject = null)
    {
        if (gameObject == null)
        {
            gameObject = new GameObject("panel_LevelImposter");
            gameObject.transform.parent = LILobbyBehaviour.GetInstance().transform;
            gameObject.transform.localPosition = new Vector3(-1.41f, 1.84f, -9.998f);   // <-- Default Position
        }
        
        // Load Prefab
        var panelPrefab = LobbyDropshipPrefab.GetObjectFromPrefab("panel_Wardrobe");
        var prefabSpriteRenderer = panelPrefab.GetComponent<SpriteRenderer>();
        
        // SpriteRenderer
        var spriteRenderer = gameObject.GetOrAddComponent<SpriteRenderer>();
        spriteRenderer.material = prefabSpriteRenderer.material;
        if (spriteRenderer.sprite == null)
            spriteRenderer.sprite = GetDefaultSprite();
        
        // Console
        var console = gameObject.AddComponent<LobbyMapConsole>();
        
        // Button
        var button = gameObject.AddComponent<ButtonBehavior>();
        button.OnMouseOver = new UnityEvent();
        button.OnMouseOut = new UnityEvent();
        button.OnClick.AddListener((Action)console.Use);
        
        // Colliders
        MapUtils.CreateDefaultColliders(gameObject, panelPrefab);
        
        // Layer
        gameObject.layer = (int)Layer.ShortObjects;
    }

    /// <summary>
    /// Gets the default console sprite from assembly resources
    /// </summary>
    /// <returns>The default console sprite</returns>
    /// <exception cref="Exception">Thrown if the resource could not be found</exception>
    private static Sprite GetDefaultSprite()
    {
        if (_defaultConsoleSprite == null)
            _defaultConsoleSprite = MapUtils.LoadSpriteResource("LobbyConsole.png");
        if (_defaultConsoleSprite == null)
            throw new Exception("The \"LobbyConsole.png\" resource was not found in assembly");
        return _defaultConsoleSprite;
    }
}