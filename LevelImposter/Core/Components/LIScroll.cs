using System;
using Il2CppInterop.Runtime.Attributes;
using UnityEngine;

namespace LevelImposter.Core;

/// <summary>
///     Object that scrolls in a repeating pattern
/// </summary>
public class LIScroll(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    private Material? _mat;
    private float _xSpeed = 1.0f;
    private float _ySpeed;

    private float _t => Time.time;

    public void Update()
    {
        if (_mat == null)
            return;

        _mat.SetTextureOffset("_MainTex", new Vector2(
            _t * _xSpeed,
            _t * -_ySpeed
        ));
    }

    public void OnDestroy()
    {
        _mat = null;
    }

    [HideFromIl2Cpp]
    public void Init(LIElement elem)
    {
        _xSpeed = elem.properties.scrollingXSpeed ?? _xSpeed;
        _ySpeed = elem.properties.scrollingYSpeed ?? _ySpeed;

        gameObject.layer = (int)Layer.Default;

        if (SpriteLoader.Instance == null)
            throw new Exception("Sprite Loader is not instantiated");

        // Replace SpriteRenderer with MeshRenderer because
        // Unity doesn't support scrolling textures on sprites
        SpriteLoader.Instance.OnLoad += loadedElem =>
        {
            if (loadedElem.id != elem.id)
                return;
            ReplaceRenderer();
        };
    }

    /// <summary>
    ///     Replaces the SpriteRenderer with a MeshRenderer
    ///     to support scrolling textures
    /// </summary>
    private void ReplaceRenderer()
    {
        // Delete Sprite Renderer
        var renderer = GetComponent<SpriteRenderer>();
        var tex = renderer.sprite.texture;
        var spriteRect = renderer.sprite.rect;
        DestroyImmediate(renderer);

        // Texture Wrapping
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.Apply();

        // Create Material
        _mat = new Material(Shader.Find("Unlit/Transparent"))
        {
            name = $"{gameObject.name}_scrollmat",
            mainTexture = tex,
            hideFlags = HideFlags.HideAndDontSave
        };
        GCHandler.Register(_mat);

        // Mesh Filter
        var meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = MapUtils.Build2DMesh(
            spriteRect.width / 100.0f,
            spriteRect.height / 100.0f
        );
        GCHandler.Register(meshFilter.mesh);

        // Mesh Renderer
        var meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = _mat;
    }
}