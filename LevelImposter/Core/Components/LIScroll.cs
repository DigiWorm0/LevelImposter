using System;
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
        var objData = gameObject.GetLIData();
        if (objData == null)
            throw new Exception("Missing MapObjectData");

        // Get Scrolling Speeds
        _xSpeed = objData.Element.properties.scrollingXSpeed ?? _xSpeed;
        _ySpeed = objData.Element.properties.scrollingYSpeed ?? _ySpeed;

        // Set Layer
        //gameObject.layer = (int)Layer.Ship;


        // Replace SpriteRenderer with MeshRenderer because
        // Unity doesn't support scrolling textures on sprites
        if (SpriteLoader.Instance == null)
            throw new Exception("Sprite Loader is not instantiated");
        SpriteLoader.Instance.OnLoad += loadedElem =>
        {
            if (loadedElem.id != objData.ID)
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