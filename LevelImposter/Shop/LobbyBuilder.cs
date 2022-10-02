using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Shop
{
    public static class LobbyBuilder
    {
        private static Texture2D _consoleTex;

        public static void OnLoad()
        {
            /*
            Transform lobbyMenu = GameSettingMenu.Instance.transform;
            Transform menuTabs = lobbyMenu.Find("Header").Find("Tabs");
            GameObject gameTab = menuTabs.Find("GameTab").gameObject;
            GameObject roleTab = menuTabs.Find("RoleTab").gameObject;

            GameObject newTab = UnityEngine.Object.Instantiate(gameTab, menuTabs);
            newTab.transform.localPosition += new Vector3(1.5f, 0, 0);
            menuTabs.transform.localPosition += new Vector3(-0.75f, 0, 0);
            */

            GameObject shopSpawner = new GameObject("LIShopSpawner");
            shopSpawner.AddComponent<ShopSpawner>();
            shopSpawner.SetActive(false);

            Transform lobby = LobbyBehaviour.Instance.transform;
            GameObject consolePrefab = lobby.FindChild("panel_Wardrobe").gameObject;

            GameObject liConsoleObj = GameObject.Instantiate(consolePrefab, lobby);
            liConsoleObj.name = "panel_LevelImposter";
            liConsoleObj.transform.localPosition = new Vector3(-1.41f, 1.84f, -9.998f);

            SpriteRenderer liRenderer = liConsoleObj.GetComponent<SpriteRenderer>();
            Texture2D tex = GetTexture();
            liRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);

            OptionsConsole liConsole = liConsoleObj.transform.GetChild(0).GetComponent<OptionsConsole>();
            liConsole.CustomPosition = new Vector3(0, 0, -30);
            liConsole.CustomUseIcon = ImageNames.UseButton;
            liConsole.HostOnly = true;
            liConsole.MenuPrefab = shopSpawner;
            liConsole.Outline = liRenderer;
        }

        public static Texture2D GetTexture()
        {
            if (_consoleTex == null)
            {
                _consoleTex = new Texture2D(1, 1);
                ImageConversion.LoadImage(_consoleTex, Properties.Resources.console);
            }
            return _consoleTex;
        }
    }
}
