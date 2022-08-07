using HarmonyLib;
using UnityEngine;

namespace LevelImposter.Shop
{
    /*
     *      Replaces the How To Play
     *      Button in the Main Menu
     *      with the Map Shop Button
     */
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public static class ButtonPatch
    {
        public static void Postfix()
        {
            GameObject button = GameObject.Find("HowToPlayButton").transform.FindChild("Text_TMP").gameObject;
            GameObject.Destroy(button.GetComponent<TextTranslatorTMP>());
            TMPro.TextMeshPro textComponent = button.GetComponent<TMPro.TextMeshPro>();
            textComponent.text = "Maps";
            GameObject.Destroy(button.GetComponent<TextTranslatorTMP>());
        }
    }
}
