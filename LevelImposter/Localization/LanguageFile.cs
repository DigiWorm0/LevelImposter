using System;
using System.Text.Json;
using LevelImposter.Core.Utils;
using LevelImposter.Logging;
using LanguageFileType = System.Collections.Generic.Dictionary<string, string>;

namespace LevelImposter.Localization;

public class LanguageFile(string languageID)
{
    private LanguageFileType? _translationsCache;

    private void TryLoadCache()
    {
        if (_translationsCache != null)
            return;

        LILogger.Info($"Loading translations for language: {languageID}");
        var json = TranslationResources.Load($"i18n.{languageID}.json");
        if (json == null)
        {
            LILogger.Warn($"Failed to load translations for language: {languageID}");
            return;
        }

        try
        {
            _translationsCache = JsonSerializer.Deserialize<LanguageFileType>(json);
        }
        catch (JsonException e)
        {
            LILogger.Warn($"Invalid translations for language {languageID}: {e.Message}");
        }

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
        if (!(_translationsCache?.TryGetValue(key, out var translation) ?? false))
            return null;

        try
        {
            return string.Format(translation, args);
        }
        catch (FormatException e)
        {
            // A malformed translation should never take down the UI
            LILogger.Warn($"Invalid translation format for key [{key}]: {e.Message}");
            return translation;
        }
    }
}
