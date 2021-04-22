using HarmonyLib;
using LevelImposter.Builders;
using LevelImposter.Map;
using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Harmony.Patches
{
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public static class CustomBtnPatch1
    {
        public static void Postfix()
        {
            if (!MapHandler.Load())
                return;

            string customBtn = MapHandler.GetMap().btn;
            byte[] defaultBtn = Properties.Resources.custom_btn;

            Sprite sprite;
            if (string.IsNullOrEmpty(customBtn))
                sprite = AssetHelper.SpriteFromBase64(defaultBtn);
            else
                sprite = AssetHelper.SpriteFromBase64(customBtn);
            if (sprite == null)
                return;

            SpriteRenderer freeplayButton = GameObject.Find("FreeplayPopover").transform.GetChild(0).FindChild("PlanetButton").GetComponent<SpriteRenderer>();
            freeplayButton.sprite = sprite;
            freeplayButton.transform.localScale = new Vector3(146.0f / sprite.texture.width, 47.0f / sprite.texture.height, 1.0f);
            freeplayButton.transform.GetChild(0).localScale = new Vector3(sprite.texture.width / 146.0f, sprite.texture.height / 47.0f, 1.0f);

            BoxCollider2D btnCollider = freeplayButton.GetComponent<BoxCollider2D>();
            btnCollider.size = new Vector2((btnCollider.size.x * sprite.texture.width) / 146.0f, (sprite.texture.height * btnCollider.size.y) / 47.0f);
        }
    }

    [HarmonyPatch(typeof(MMOnlineManager), nameof(MMOnlineManager.Start))]
    public static class CustomBtnPatch2
    {
        public static void Postfix()
        {
            if(!MapHandler.Load())
                return;

            string customBtn = MapHandler.GetMap().btn;
            byte[] defaultBtn = Properties.Resources.custom_btn;

            Sprite sprite;
            if (string.IsNullOrEmpty(customBtn))
                sprite = AssetHelper.SpriteFromBase64(defaultBtn);
            else
                sprite = AssetHelper.SpriteFromBase64(customBtn);
            if (sprite == null)
                return;

            SpriteRenderer freeplayButton = GameObject.Find("OptionsMenu").transform.GetChild(0).FindChild("Map").FindChild("2").FindChild("MapIcon2").GetComponent<SpriteRenderer>();
            freeplayButton.sprite = sprite;
            freeplayButton.transform.localScale = new Vector3(146.0f / sprite.texture.width, 47.0f / sprite.texture.height, 1.0f);
        }
    }
}
