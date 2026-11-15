using System.Collections.Generic;
using LevelImposter.Logging;

namespace LevelImposter.Localization;

public static class Translation
{
    public delegate void LanguageChangedEvent();

    private static readonly Dictionary<LILanguage, LanguageFile> AllLanguages = new()
    {
        { LILanguage.English, new LanguageFile("en-us") },
        { LILanguage.Latam, new LanguageFile("es") },
        { LILanguage.Spanish, new LanguageFile("es") },
        { LILanguage.Japanese, new LanguageFile("ja") }
    };

    private static LanguageFile? _currentLanguage;
    private static LanguageFile FallbackLanguage => AllLanguages[LILanguage.English];
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
    /// <param name="language">The language to set</param>
    public static void SetLanguage(LILanguage language)
    {
        if (AllLanguages.TryGetValue(language, out var file))
            _currentLanguage = file;
        else
            LILogger.Warn($"Language {language} is not supported.");

        OnLanguageChanged?.Invoke();
    }
}
