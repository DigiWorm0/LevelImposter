using HarmonyLib;
using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Harmony.Patches
{
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public static class CustomLogoPatch1
    {
        public static void Postfix()
        {
            byte[] customData = Properties.Resources.custom_btn;
            Texture2D tex = new Texture2D(1, 1);
            ImageConversion.LoadImage(tex, customData);

            SpriteRenderer freeplayButton = GameObject.Find("FreeplayPopover").transform.GetChild(0).FindChild("PlanetButton").GetComponent<SpriteRenderer>();
            freeplayButton.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
        }
    }

    [HarmonyPatch(typeof(MMOnlineManager), nameof(MMOnlineManager.Start))]
    public static class CustomLogoPatch2
    {
        public static void Postfix()
        {
            byte[] customData = Properties.Resources.custom_btn;
            Texture2D tex = new Texture2D(1, 1);
            ImageConversion.LoadImage(tex, customData);

            SpriteRenderer freeplayButton = GameObject.Find("OptionsMenu").transform.GetChild(0).FindChild("Map").FindChild("2").FindChild("MapIcon2").GetComponent<SpriteRenderer>();
            freeplayButton.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
        }
    }
}
