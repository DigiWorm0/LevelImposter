using HarmonyLib;
using LevelImposter.Core.Translations;

namespace LevelImposter.Core.Patches.Utils;

/// <summary>
///     Uses BlockedWords to watch for language changes
///     and relays them to LevelImposter.Core.Utils.Translations.
/// </summary>
[HarmonyPatch(typeof(BlockedWords), nameof(BlockedWords.SetLanguage))]
public static class LanguageChangePatch
{
    public static void Postfix([HarmonyArgument(0)] TranslatedImageSet newLang)
    {
        Translation.SetLanguage(newLang.languageID);
    }
}