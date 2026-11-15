using HarmonyLib;
using LevelImposter.Localization;

namespace LevelImposter.Core.Patches.Utils;

/// <summary>
///     Uses BlockedWords to watch for language changes
///     and relays them to LevelImposter.Localization.
/// </summary>
[HarmonyPatch(typeof(BlockedWords), nameof(BlockedWords.SetLanguage))]
public static class LanguageChangePatch
{
    public static void Postfix([HarmonyArgument(0)] TranslatedImageSet newLang)
    {
        Translation.SetLanguage(newLang.languageID switch
        {
            SupportedLangs.English => LILanguage.English,
            SupportedLangs.Latam => LILanguage.Latam,
            SupportedLangs.Spanish => LILanguage.Spanish,
            SupportedLangs.Japanese => LILanguage.Japanese,
            _ => LILanguage.English
        });
    }
}
