using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using LevelImposter.Core;

namespace LevelImposter.Shop
{
    public static class LobbyConsoleBuilder
    {
        private static Sprite? _consoleSprite;

        public static void Build()
        {
            // Spawnable Prefab
            GameObject shopSpawner = new GameObject("LIShopSpawner");
            shopSpawner.AddComponent<ShopSpawner>();
            shopSpawner.SetActive(false);

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
            OptionsConsole liConsole = liConsoleObj.transform.GetChild(0).GetComponent<OptionsConsole>();
            liConsole.CustomPosition = new Vector3(0, 0, -30);
            liConsole.CustomUseIcon = ImageNames.UseButton;
            liConsole.HostOnly = true;
            liConsole.MenuPrefab = shopSpawner;
            liConsole.Outline = liRenderer;
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
