using UnityEngine;

namespace LevelImposter.Shop.Transitions;

public static class SpriteOpacityTransition
{
    public static void Run(TransitionParams<float> transitionParams)
    {
        TransitionHelper.RunTransition(transitionParams, SetOpacity);
    }

    private static void SetOpacity(GameObject gameObject, float opacity)
    {
        // Check if GameObject was destroyed
        if (gameObject == null)
            return;
        
        // Set All SpriteRenderers
        var allSpriteRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var renderer in allSpriteRenderers)
        {
            renderer.color = new Color(
                renderer.color.r,
                renderer.color.g,
                renderer.color.b,
                opacity);
        }
    }
}