using System;

namespace LevelImposter.Localization;

/// <summary>
///     Seam for loading packaged translation text without referencing Unity or BepInEx.
///     The runtime wires <see cref="TextLoader" /> to <c>PackagedResources.LoadText</c> during plugin startup.
/// </summary>
public static class TranslationResources
{
    /// <summary>
    ///     Loads a packaged text resource by name, returning null when it is unavailable.
    /// </summary>
    public static Func<string, string?>? TextLoader { get; set; }

    /// <summary>
    ///     Loads a text resource through the configured loader.
    /// </summary>
    /// <param name="name">The resource name, such as <c>i18n.en-us.json</c>.</param>
    /// <returns>The text, or null when no loader is configured or the resource is missing.</returns>
    public static string? Load(string name)
    {
        return TextLoader?.Invoke(name);
    }
}
