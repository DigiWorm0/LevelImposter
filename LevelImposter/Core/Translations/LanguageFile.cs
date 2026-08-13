using LevelImposter.Core.Utils;
using LanguageFileType = System.Collections.Generic.Dictionary<string, string>;

namespace LevelImposter.Core.Translations;

public class LanguageFile(string languageID)
{
    private LanguageFileType? _translationsCache;

    private void TryLoadCache()
    {
        if (_translationsCache != null)
            return;

        LILogger.Info($"Loading translations for language: {languageID}");
        _translationsCache = PackagedResources.LoadJson<LanguageFileType>($"i18n.{languageID}.json");

        if (_translationsCache == null)
            LILogger.Warn($"Failed to load translations for language: {languageID}");
    }

    /// <summary>
    ///     Gets a translation from this language file
    /// </summary>
    /// <param name="key">The key to lookup</param>
    /// <param name="args">The arguments to format the string with</param>
    /// <returns>The translated string. If no key is present, this returns null</returns>
    public string? Get(string key, params object[] args)
    {
        TryLoadCache();
        if (_translationsCache?.TryGetValue(key, out var translation) ?? false)
            return string.Format(translation, args);

        return null;
    }
}