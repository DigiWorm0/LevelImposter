using System;
using LevelImposter.Core.Translations;
using LevelImposter.Core.Utils;
using TMPro;
using UnityEngine;

namespace LevelImposter.Core.Components;

/// <summary>
///     Automatically translates the text on language change
/// </summary>
public class LITextTranslatorTMP(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    private string _key = "";
    private TMP_Text? _textComponent;

    public void Awake()
    {
        _textComponent = GetComponent<TMP_Text>();
        Translation.OnLanguageChanged += ResetText;
        SetText(name); // <-- Avoids using Il2CppStringField which appears to be unstable currently
    }

    public void OnDestroy()
    {
        Translation.OnLanguageChanged -= ResetText;
    }

    /// <summary>
    ///     Sets the text of this component to the translation for the given key, and updates the text immediately.
    /// </summary>
    /// <param name="newKey">The translation key to use for the text</param>
    public void SetText(string newKey)
    {
        _key = newKey;
        ResetText();
    }

    private void ResetText()
    {
        if (_textComponent != null)
            _textComponent.text = Translation.Get(_key);
    }

    /// <summary>
    ///     Adds a text translator to a GameObject, replacing any existing TextTranslatorTMP component.
    ///     This is useful for ensuring that the text on the GameObject is automatically translated when the language changes.
    /// </summary>
    /// <param name="gameObject">The GameObject w/ TMP_Text component to translate</param>
    /// <param name="key">The translation key to use for the text</param>
    public static void AddTranslator(GameObject gameObject, string key)
    {
        // Remove any existing translator
        var existingTranslator = gameObject.GetComponent<TextTranslatorTMP>();
        if (existingTranslator != null)
            Destroy(existingTranslator);

        // Replace with our own
        var newTranslator = gameObject.GetOrAddComponent<LITextTranslatorTMP>();
        newTranslator.SetText(key);
    }
}