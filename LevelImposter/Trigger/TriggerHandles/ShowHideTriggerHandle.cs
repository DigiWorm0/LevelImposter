using System.Collections;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Trigger;

public class ShowHideTriggerHandle : ITriggerHandle
{
    private readonly GameObjectCoroutineManager _fadeManager = new();

    public void OnTrigger(TriggerSignal signal)
    {
        var triggerID = signal.TriggerID;

        // Show/Hide the object
        if (triggerID == "show")
            _fadeManager.Start(signal.TargetObject, FadeInObject(signal));
        else if (triggerID == "hide")
            _fadeManager.Start(signal.TargetObject, FadeOutObject(signal));

        // Enable/Disable the components
        if (triggerID == "enable" || triggerID == "show")
            SetTriggerState(signal.TargetObject, true);
        else if (triggerID == "disable" || triggerID == "hide")
            SetTriggerState(signal.TargetObject, false);
        else if (triggerID == "toggle")
            SetTriggerState(signal.TargetObject);
    }

    private static IEnumerator FadeInObject(TriggerSignal signal)
    {
        return FadeObject(signal, true);
    }

    private static IEnumerator FadeOutObject(TriggerSignal signal)
    {
        return FadeObject(signal, false);
    }

    /// <summary>
    ///     Coroutine to fade effect on a GameObject
    /// </summary>
    /// <param name="signal">Source trigger signal to read from</param>
    /// <param name="fadeIn">True to fade in, false to fade out</param>
    private static IEnumerator FadeObject(TriggerSignal signal, bool fadeIn)
    {
        // Get signal info
        var fadeObject = signal.TargetObject;
        var sourceObject = signal.SourceTrigger?.TargetObject;

        // Always show object during the animation
        fadeObject.SetActive(true);

        // Get the sprite renderer
        var spriteRenderers = fadeObject.GetComponentsInChildren<SpriteRenderer>();
        if (spriteRenderers.Length == 0)
        {
            fadeObject.SetActive(fadeIn);
            yield break;
        }

        // Get the object data
        var objectData = sourceObject?.GetLIData();
        var triggerFadeTime = objectData?.Properties.triggerFadeTime ?? 0;

        // Get colors
        var spriteColor = spriteRenderers[0].color;
        var visibleAlpha = objectData?.Properties.color?.a ?? 1.0f; // Use original sprite color, if applicable

        // Sprite color can be 0 - 255, so clamp it
        var fromAlpha = Mathf.Clamp(spriteColor.a, 0.0f, visibleAlpha);
        var toAlpha = fadeIn ? visibleAlpha : 0.0f;

        // Run Fade
        float t = 0;
        while (t < triggerFadeTime)
        {
            yield return null;
            t += Time.deltaTime * 1000.0f; // s >> ms

            // Get New Color
            var newAlpha = Mathf.Lerp(fromAlpha, toAlpha, t / triggerFadeTime);
            var currentColor = new Color(
                spriteColor.r,
                spriteColor.g,
                spriteColor.b,
                newAlpha
            );

            // Set All Sprite Renderers
            foreach (var spriteRenderer in spriteRenderers)
                spriteRenderer.color = currentColor;
        }

        // Set GameObject Color
        var targetColor = new Color(
            spriteColor.r,
            spriteColor.g,
            spriteColor.b,
            toAlpha
        );
        foreach (var spriteRenderer in spriteRenderers)
            spriteRenderer.color = targetColor;

        // Set GameObject Active
        fadeObject.SetActive(fadeIn);
    }

    /// <summary>
    ///     Sets the state of the trigger components
    /// </summary>
    /// <param name="gameObject">GameObject to modify</param>
    /// <param name="isEnabled">null to toggle, true to enable, false to disable</param>
    private static void SetTriggerState(GameObject gameObject, bool? isEnabled = null)
    {
        // Trigger Sounds
        var triggerSound = gameObject.GetComponent<TriggerSoundPlayer>();
        if (triggerSound != null)
        {
            if (isEnabled ?? !triggerSound.IsPlaying)
                triggerSound.Play(true);
            else
                triggerSound.Stop();
        }

        // Animation
        var animator = gameObject.GetComponent<LIAnimatorBase>();
        if (animator != null)
        {
            if (isEnabled ?? !animator.IsAnimating)
                animator.Play();
            else
                animator.Stop();
        }

        // Get Toggleable Components
        MonoBehaviour[] toggleComponents =
        {
            gameObject.GetComponent<AmbientSoundPlayer>(),
            gameObject.GetComponent<TriggerConsole>(),
            gameObject.GetComponent<SystemConsole>(),
            gameObject.GetComponent<MapConsole>(),
            gameObject.GetComponent<LITeleporter>(),
            gameObject.GetComponent<LIShakeArea>(),
            gameObject.GetComponent<LITriggerArea>(),
            gameObject.GetComponent<LIPlayerMover>()
        };

        // Toggle the components
        foreach (var component in toggleComponents)
        {
            if (component == null)
                continue;
            component.enabled = isEnabled ?? !component.enabled;
        }
    }
}