using UnityEngine;

namespace LevelImposter.Shop.Transitions;

public static class MatOpacityTransition
{
    private static readonly int ColorMatProperty = Shader.PropertyToID("_Color");
    
    public static void Run(TransitionParams<float> transitionParams)
    {
        TransitionHelper.RunTransition(transitionParams, SetOpacity);
    }

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