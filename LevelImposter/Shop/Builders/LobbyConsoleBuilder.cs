using System;
using UnityEngine;
using LevelImposter.Core;

namespace LevelImposter.Shop
{
    public static class LobbyConsoleBuilder
    {
        private static Sprite? _consoleSprite;

        public static void Build()
        {
            // Object
            Transform lobbyTransform = LobbyBehaviour.Instance.transform;
            GameObject consolePrefab = lobbyTransform.FindChild("panel_Wardrobe").gameObject;
            GameObject liConsoleObj = UnityEngine.Object.Instantiate(consolePrefab, lobbyTransform);
            liConsoleObj.name = "panel_LevelImposter";
            liConsoleObj.transform.localPosition = new Vector3(-1.41f, 1.84f, -9.998f);

            // Sprite
            SpriteRenderer liRenderer = liConsoleObj.GetComponent<SpriteRenderer>();
            liRenderer.sprite = GetSprite();

            // Console
            GameObject consoleObj = liConsoleObj.transform.GetChild(0).gameObject;
            UnityEngine.Object.Destroy(consoleObj.GetComponent<OptionsConsole>());
            LobbyConsole liConsole = consoleObj.AddComponent<LobbyConsole>();
            liConsole.SetRenderer(liRenderer);

            // Collider
            BoxCollider2D liCollider = liConsoleObj.GetComponentInChildren<BoxCollider2D>();
            liCollider.size = new Vector2(0.01f, 0.01f);
        }

        private static Sprite GetSprite()
        {
            if (_consoleSprite == null)
                _consoleSprite = MapUtils.LoadSpriteResource("LobbyConsole.png");
            if (_consoleSprite == null)
                throw new Exception("The \"LobbyConsole.png\" resource was not found in assembly");
            return _consoleSprite;
        }
    }
}
