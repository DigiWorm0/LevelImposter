using System;
using LevelImposter.Builders;
using UnityEngine;

namespace LevelImposter.Core;

/// <summary>
///     Object that scrolls in a repeating pattern
/// </summary>
public class LIScroll(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");

    private Material? _mat;
    private float _xSpeed = 1.0f;
    private float _ySpeed;

    public void Awake()
    {
        // Get Object Data
        var element = MapObjectDB.Get(gameObject);
        if (element == null)
            throw new Exception("LIScroll is missing LI data");

        // Get Scrolling Speeds
        _xSpeed = element.properties.scrollingXSpeed ?? _xSpeed;
        _ySpeed = element.properties.scrollingYSpeed ?? _ySpeed;

        // Set Layer
        //gameObject.layer = (int)Layer.Ship;


        // Replace SpriteRenderer with MeshRenderer because
        // Unity doesn't support scrolling textures on sprites
        SpriteBuilder.OnSpriteLoad += (loadedElem, _) =>
        {
            if (loadedElem.id != element.id)
                return;
            ReplaceRenderer();
        };
    }

    public void Update()
    {
        var t = Time.time;
        _mat?.SetTextureOffset(MainTex, new Vector2(
            t * _xSpeed,
            t * -_ySpeed
        ));
    }

    public void OnDestroy()
    {
        _mat = null;
    }

    /// <summary>
    ///     Replaces the SpriteRenderer with a MeshRenderer
    ///     to support scrolling textures
    /// </summary>
    private void ReplaceRenderer()
    {
        // Delete Sprite Renderer
        var spriteRenderer = GetComponent<SpriteRenderer>();
        var tex = spriteRenderer.sprite.texture;

        _mat = spriteRenderer.material;

        // Texture Wrapping
        tex.wrapMode = TextureWrapMode.Repeat;

        // Create Material
        _mat = new Material(Shader.Find("Unlit/Transparent"))
        {
            name = $"{gameObject.name}_scrollmat",
            mainTexture = tex,
            hideFlags = HideFlags.HideAndDontSave
        };
        spriteRenderer.material = _mat;
        GCHandler.Register(_mat);
    }
}