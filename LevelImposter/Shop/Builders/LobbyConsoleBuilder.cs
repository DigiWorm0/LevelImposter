using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Shop
{
    public static class LobbyConsoleBuilder
    {
        private static Sprite _consoleSprite;

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
            {
                Texture2D consoleTex = new Texture2D(1, 1);
                ImageConversion.LoadImage(consoleTex, Properties.Resources.console);
                _consoleSprite = Sprite.Create(
                    consoleTex,
                    new Rect(0, 0, consoleTex.width, consoleTex.height),
                    new Vector2(0.5f, 0.5f),
                    100.0f,
                    0,
                    SpriteMeshType.FullRect
                );
            }
            return _consoleSprite;
        }
    }
}
