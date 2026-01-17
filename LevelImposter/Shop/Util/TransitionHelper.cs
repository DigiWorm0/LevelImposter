using System.Collections;
using Reactor.Utilities;
using UnityEngine;

namespace LevelImposter.Shop;

public static class TransitionHelper
{
    private static readonly int ColorMatProperty = Shader.PropertyToID("_Color");

    /// <summary>
    /// Fades in the given GameObject over the specified duration.
    /// Warning: This will modify the materials of all renderers in the GameObject and its children.
    /// </summary>
    /// <param name="gameObject">The GameObject to fade in.</param>
    /// <param name="duration">The duration of the fade in seconds.</param>
    /// <param name="startDelay">Optional delay before starting the fade</param>
    public static void FadeIn(
        GameObject gameObject,
        float duration,
        float startDelay = 0.0f)
    {
        Fade(gameObject, 0.0f, 1.0f, duration, startDelay);
    }
    
    /// <summary>
    /// Fades out the given GameObject over the specified duration.
    /// Warning: This will modify the materials of all renderers in the GameObject and its
    /// </summary>
    /// <param name="gameObject">The GameObject to fade out.</param>
    /// <param name="duration">The duration of the fade in seconds.</param>
    /// <param name="startDelay">Optional delay before starting the fade</param>
    public static void FadeOut(
        GameObject gameObject,
        float duration,
        float startDelay = 0.0f)
    {
        Fade(gameObject, 1.0f, 0.0f, duration, startDelay);
    }
    
    /// <summary>
    /// Fades a GameObject's opacity from fromOpacity to toOpacity over the specified duration.
    /// </summary>
    /// <param name="gameObject">The GameObject to fade</param>
    /// <param name="fromOpacity">The starting opacity in % [0,1]</param>
    /// <param name="toOpacity">The target opacity in % [0,1]</param>
    /// <param name="duration">The duration of the fade in seconds</param>
    /// <param name="startDelay">Optional delay before starting the fade</param>
    public static void Fade(
        GameObject gameObject,
        float fromOpacity,
        float toOpacity,
        float duration,
        float startDelay = 0.0f)
    {
        Coroutines.Start(CoFade(fromOpacity, toOpacity, duration, startDelay, gameObject));
    }
    
    /// <summary>
    /// Coroutine to fade a GameObject's opacity
    /// </summary>
    /// <param name="fromOpacity">The starting opacity in % [0,1]</param>
    /// <param name="toOpacity">The target opacity in % [0,1]</param>
    /// <param name="duration">The duration of the fade in seconds</param>
    /// <param name="startDelay">Optional delay before starting the fade</param>
    /// <param name="gameObject">The GameObject to fade</param>
    private static IEnumerator CoFade(
        float fromOpacity,
        float toOpacity,
        float duration,
        float startDelay,
        GameObject gameObject)
    {
        // Apply initial opacity
        SetOpacity(gameObject, fromOpacity);
        
        // Initial delay
        if (startDelay > 0.0f)
            yield return new WaitForSeconds(startDelay);
        
        // Fade over time
        var t = 0.0f;
        while (t < duration)
        {
            // Calculate opacity
            var opacity = Mathf.Lerp(fromOpacity, toOpacity, t / duration);
            SetOpacity(gameObject, opacity);

            // Wait a frame
            yield return null;
            t += Time.deltaTime;
        }
        
        // Ensure final opacity
        SetOpacity(gameObject, toOpacity);
    }
    
    /// <summary>
    /// Sets the opacity of all renderers in the GameObject and its children.
    /// </summary>
    /// <param name="gameObject">The GameObject to modify</param>
    /// <param name="opacity">The target opacity in % [0,1]</param>
    private static void SetOpacity(GameObject gameObject, float opacity)
    {
        // Check if GameObject was destroyed
        if (gameObject == null)
            return;
        
        // Set All Renderers (SpriteRenderer, MeshRenderer, etc.)
        var allRenderers = gameObject.GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in allRenderers)
        {
            if (!renderer.material.HasProperty(ColorMatProperty))
                continue;
            
            var color = renderer.material.color;
            color.a = opacity;
            renderer.material.color = color;
        }
        
        // Set All TextMeshPro
        var allTextMeshPro = gameObject.GetComponentsInChildren<TMPro.TMP_Text>(true);
        foreach (var tmp in allTextMeshPro)
        {
            tmp.color = new Color(
                tmp.color.r,
                tmp.color.g,
                tmp.color.b,
                opacity);
        }
    }
}