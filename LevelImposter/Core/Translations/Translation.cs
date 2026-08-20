using System.Collections.Generic;
using LevelImposter.Core.Utils;

namespace LevelImposter.Core.Translations;

public static class Translation
{
    public delegate void LanguageChangedEvent();

    private static readonly Dictionary<SupportedLangs, LanguageFile> AllLanguages = new()
    {
        { SupportedLangs.English, new LanguageFile("en-us") },
        { SupportedLangs.Latam, new LanguageFile("es") },
        { SupportedLangs.Spanish, new LanguageFile("es") },
        { SupportedLangs.Japanese, new LanguageFile("ja") }
    };

    private static LanguageFile? _currentLanguage;
    private static LanguageFile FallbackLanguage => AllLanguages[SupportedLangs.English];
    public static event LanguageChangedEvent? OnLanguageChanged;

    /// <summary>
    ///     Gets a string from the language file
    /// </summary>
    /// <param name="key">The string to lookup</param>
    /// <param name="args">The arguments to format the string with</param>
    /// <returns>The translated string. If no key is present, this returns the string key</returns>
    public static string Get(string key, params object[] args)
    {
        return _currentLanguage?.Get(key, args) ??
               FallbackLanguage.Get(key, args) ??
               key;
    }

    /// <summary>
    ///     Sets the current language
    /// </summary>
    /// <param name="lang">The language to set</param>
    public static void SetLanguage(SupportedLangs lang)
    {
        if (AllLanguages.TryGetValue(lang, out var file))
            _currentLanguage = file;
        else
            LILogger.Warn($"Language {lang} is not supported.");

        OnLanguageChanged?.Invoke();
    }
}