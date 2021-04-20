using HarmonyLib;
using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Harmony.Patches
{
    [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
    public static class VersionPatch
    {
        public static void Postfix(VersionShower __instance)
        {
            byte[] logoData = Properties.Resources.logo;

            GameObject logo = new GameObject("LevelImposterLogo");
            logo.transform.position = __instance.text.transform.position;
            logo.transform.position += new Vector3(2.2f, -0.015f, 0);
            logo.transform.localScale = new Vector3(0.5f, 0.5f, 1.0f);
            logo.layer = (int)Layer.UI;

            Texture2D tex = new Texture2D(1, 1);
            ImageConversion.LoadImage(tex, logoData);
            SpriteRenderer spriteRenderer = logo.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);

            __instance.text.text += "\t\t\t      <size=65%>v" + MainHarmony.VERSION + "<size=100%>";
        }
    }
}
