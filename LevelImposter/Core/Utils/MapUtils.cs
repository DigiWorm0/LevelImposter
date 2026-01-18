using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.IO;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using LevelImposter.AssetLoader;
using LevelImposter.Builders;
using LevelImposter.Shop;
using PowerTools;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace LevelImposter.Core;

/// <summary>
///     A variety of utility functions for constructing the map
/// </summary>
public static class MapUtils
{
    private static Sprite? _defaultSquare;
    
    private const string RESOURCE_BUNDLE_NAME = "li_resources";
    private static AssetBundle? _resourceBundle;

    /// <summary>
    ///     Adds an element to an Il2CppReferenceArray
    /// </summary>
    /// <typeparam name="T">Array Type</typeparam>
    /// <param name="arr">Array to add to</param>
    /// <param name="value">Element to add</param>
    /// <returns>New array with value appended</returns>
    public static Il2CppReferenceArray<T> AddToArr<T>(Il2CppReferenceArray<T> arr, T value) where T : Il2CppObjectBase
    {
        List<T> list = new(arr)
        {
            value
        };
        return list.ToArray();
    }

    public static Il2CppStringArray AddToArr(Il2CppStringArray arr, string value)
    {
        List<string> list = new(arr)
        {
            value
        };
        return list.ToArray();
    }
    
    public static Il2CppStructArray<T> AddToArr<T>(Il2CppStructArray<T> arr, T value) where T : unmanaged
    {
        List<T> list = new(arr)
        {
            value
        };
        return list.ToArray();
    }

    /// <summary>
    ///     Removes an element from an Il2CppReferenceArray
    /// </summary>
    /// <typeparam name="T">Array Type</typeparam>
    /// <param name="arr">Array to add to</param>
    /// <param name="index">Index of the element to remove</param>
    /// <returns>New array with the value removed</returns>
    public static Il2CppReferenceArray<T> RemoveFromArr<T>(Il2CppReferenceArray<T> arr, int index)
        where T : Il2CppObjectBase
    {
        List<T> list = new(arr);
        list.RemoveAt(index);
        return list.ToArray();
    }

    /// <summary>
    ///     Replaces a prefab with a new, mutable copy
    /// </summary>
    /// <param name="oldPrefab">Old, immutable prefab</param>
    /// <param name="parent">Parent (Must be destroyed on finish)</param>
    /// <returns>New, mutable prefab</returns>
    public static GameObject ReplacePrefab(GameObject oldPrefab, Transform parent)
    {
        var newPrefab = Object.Instantiate(oldPrefab, parent);
        newPrefab.SetActive(false);
        newPrefab.name = oldPrefab.name;
        return newPrefab;
    }

    /// <summary>
    ///     Replaces a prefab with a new, mutable copy
    /// </summary>
    /// <param name="oldPrefab">Old, immutable prefab</param>
    /// <param name="parent">Parent (Must be destroyed on finish)</param>
    /// <returns>New, mutable prefab</returns>
    public static T ReplacePrefab<T>(T oldPrefab, Transform parent) where T : Component
    {
        var inactiveParent = new GameObject();
        inactiveParent.SetActive(false);
        inactiveParent.transform.SetParent(parent);

        var newPrefab = Object.Instantiate(oldPrefab, inactiveParent.transform);
        newPrefab.name = oldPrefab.name;
        return newPrefab;
    }

    /// <summary>
    ///     Shuffles an Il2CppStructArray
    /// </summary>
    /// <param name="arr">Array to shuffle</param>
    /// <returns>New array with values shuffled</returns>
    public static Il2CppStructArray<byte> Shuffle(Il2CppStructArray<byte> arr)
    {
        List<byte> listA = new(arr);
        List<byte> listB = new();
        while (listA.Count > 0)
        {
            var index = Random.Range(0, listA.Count);
            listB.Add(listA[index]);
            listA.RemoveAt(index);
        }

        return listB.ToArray();
    }

    /// <summary>
    ///     Checks if an LIElement has a solid collider
    /// </summary>
    /// <param name="elem">Element to search</param>
    /// <returns>True if element contains a solid collider</returns>
    public static bool HasSolidCollider(LIElement elem)
    {
        if (elem.properties == null)
            return false;
        if (elem.properties.colliders == null)
            return false;
        foreach (var collider in elem.properties.colliders)
            if (collider.isSolid)
                return true;
        return false;
    }

    /// <summary>
    ///     Clones the colliders from a Unity GameObject to another
    /// </summary>
    /// <param name="from">Source GameObject</param>
    /// <param name="to">Target GameObject</param>
    public static void CloneColliders(GameObject from, GameObject to)
    {
        if (from.GetComponent<CircleCollider2D>() != null)
        {
            var origBox = from.GetComponent<CircleCollider2D>();
            var box = to.AddComponent<CircleCollider2D>();
            box.radius = origBox.radius;
            box.offset = origBox.offset;
            box.isTrigger = true;
        }

        if (from.GetComponent<BoxCollider2D>() != null)
        {
            var origBox = from.GetComponent<BoxCollider2D>();
            var box = to.AddComponent<BoxCollider2D>();
            box.size = origBox.size;
            box.offset = origBox.offset;
            box.isTrigger = true;
        }

        if (from.GetComponent<PolygonCollider2D>() != null)
        {
            var origBox = from.GetComponent<PolygonCollider2D>();
            var box = to.AddComponent<PolygonCollider2D>();
            box.points = origBox.points;
            box.pathCount = origBox.pathCount;
            box.offset = origBox.offset;
            box.isTrigger = true;
        }
    }

    /// <summary>
    ///     Either grabs solid colliders from
    ///     prefab or creates new ones. Used
    ///     for any UI buttons or in-game consoles.
    /// </summary>
    /// <param name="src">Object to set colliders</param>
    /// <param name="prefab">Prefab to copy colliders from</param>
    public static Collider2D CreateDefaultColliders(GameObject src, GameObject prefab)
    {
        PolygonCollider2D[] solidColliders = src.GetComponentsInChildren<PolygonCollider2D>();
        if (solidColliders.Length <= 0)
            CloneColliders(prefab, src);
        var collider = src.GetComponent<Collider2D>();
        if (collider == null)
            collider = src.AddComponent<BoxCollider2D>();
        return collider;
    }

    /// <summary>
    ///     Converts a base64 encoded string into a byte array
    /// </summary>
    /// <param name="base64">Base64 encoded data</param>
    /// <returns>Stream of bytes representing raw base64 data</returns>
    public static byte[] ParseBase64(string base64)
    {
        var sub64 = base64.Substring(base64.IndexOf(",", StringComparison.Ordinal) + 1);
        return Convert.FromBase64String(sub64);
    }

    /// <summary>
    ///     Checks if a GameObject is the local player
    /// </summary>
    /// <param name="obj">Game Object to check</param>
    /// <returns>Whether or not obj is considered to be the local player</returns>
    public static bool IsLocalPlayer(GameObject obj)
    {
        if (PlayerControl.LocalPlayer == null)
            return false;
        return obj == PlayerControl.LocalPlayer.gameObject;
    }

    /// <summary>
    ///     Builds a 2D mesh with the given width and height
    /// </summary>
    /// <param name="width">Width in units</param>
    /// <param name="height">Height in units</param>
    /// <returns>Resulting Mesh object</returns>
    public static Mesh Build2DMesh(float width, float height)
    {
        var mesh = new Mesh();
        mesh.vertices = new Vector3[4]
        {
            new(-width / 2, -height / 2, 0),
            new(width / 2, -height / 2, 0),
            new(-width / 2, height / 2, 0),
            new(width / 2, height / 2, 0)
        };
        mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
        mesh.uv = new Vector2[]
        {
            new(0, 0),
            new(1, 0),
            new(0, 1),
            new(1, 1)
        };
        mesh.RecalculateNormals();
        return mesh;
    }
    
    private static Stream OpenResourceStream(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceStream = assembly.GetManifestResourceStream($"LevelImposter.Assets.{name}");
        if (resourceStream == null)
            throw new Exception($"Resource {name} not found in assembly");

        return resourceStream;
    }
    private static byte[]? GetResource(string name)
    {
        using var resourceStream = OpenResourceStream(name);
        return resourceStream.ToManagedArray();
    }
    private static MemoryBlock? GetResourceAsIl2Cpp(string name)
    {
        using var resourceStream = OpenResourceStream(name);
        return resourceStream.ToIl2CppArray();
    }
    
    /// <summary>
    /// Gets or loads the resource asset bundle
    /// </summary>
    /// <returns>AssetBundle instance containing LI resources stored in the AssetBundle</returns>
    private static AssetBundle GetResourceBundle()
    {
        if (_resourceBundle != null)
            return _resourceBundle;
        
        LILogger.Info($"Loading asset bundle {RESOURCE_BUNDLE_NAME}");
        
        _resourceBundle = AssetBundleManager.Load(RESOURCE_BUNDLE_NAME);
        if (_resourceBundle == null)
            throw new Exception("Could not load resource asset bundle");
        
        return _resourceBundle;
    }

    /// <summary>
    ///     Loads a GameObject from assembly resources
    /// </summary>
    /// <param name="name">Name of the AssetBundle file</param>
    /// <returns>GameObject or null if not found</returns>
    public static T? LoadResourceFromAssetBundle<T>(string name) where T : Object
    {
        LILogger.Info($"Loading resource {name}");
        
        var asset = GetResourceBundle().LoadAsset(name, Il2CppType.Of<T>())?.Cast<T>();
        if (asset == null)
            throw new Exception($"Could not find resource {name} in asset bundle");
        
        return asset;
    }

    /// <summary>
    /// Loads a PNG sprite from assembly resources
    /// </summary>
    /// <param name="name">Name of the PNG file</param>
    /// <returns>Sprite or null if not found</returns>
    public static Sprite? LoadSpriteResource(string name)
    {
        // Log
        LILogger.Info($"Loading sprite resource {name}");
        
        // Get Sprite Data
        var spriteData = GetResourceAsIl2Cpp(name);
        if (spriteData == null)
            return null;

        // Create Loadables
        var loadableTexture = LoadableTexture.FromMemory($"{name}-resource", spriteData);
        loadableTexture.Options.GCBehavior = GCBehavior.NeverDispose;
        
        var loadableSprite = LoadableSprite.FromLoadableTexture(loadableTexture);
        loadableSprite.Options.GCBehavior = GCBehavior.NeverDispose;
        
        // Load Sprite (Synchronously)
        return SpriteLoader.Instance.LoadImmediate(loadableSprite);
    }

    /// <summary>
    ///     Deserializes a JSON object from assembly resources
    /// </summary>
    /// <typeparam name="T">JSON type to deserialize to</typeparam>
    /// <param name="name">Name of the JSON file</param>
    /// <returns>JSON object or null if not found</returns>
    public static T? LoadJsonResource<T>(string name) where T : class
    {
        LILogger.Info($"Loading JSON resource {name}");
        var jsonData = GetResource(name);
        if (jsonData == null)
            return null;
        var jsonString = Encoding.UTF8.GetString(jsonData);
        return JsonSerializer.Deserialize<T>(jsonString);
    }

    /// <summary>
    ///     Waits for PlayerControl.LocalPlayer to be initialized, then calls Action
    /// </summary>
    /// <param name="onFinish">Action to call when the local player is initialized</param>
    public static void WaitForPlayer(Action onFinish)
    {
        Coroutines.Start(CoWaitForPlayer(onFinish));
    }

    private static IEnumerator CoWaitForPlayer(Action onFinish)
    {
        {
            while (PlayerControl.LocalPlayer == null)
                yield return null;
            onFinish.Invoke();
        }
    }

    /// <summary>
    ///     Waits for ShipStatus to be ready, then calls Action
    /// </summary>
    /// <param name="timeout">Max amount of time in seconds to wait</param>
    /// <param name="onFinish">Action to call when the map is initialized</param>
    public static void WaitForShip(float timeout, Action onFinish)
    {
        Coroutines.Start(CoWaitForShip(timeout, onFinish));
    }

    private static IEnumerator CoWaitForShip(float timeout, Action? onFinish)
    {
        {
            float timer = 0;
            while (!LIShipStatus.IsReady && timer < timeout)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            onFinish?.Invoke();
            onFinish = null;
        }
    }

    /// <summary>
    ///     Waits for a specific amount of frames, then calls Action
    /// </summary>
    /// <param name="frames">Amount of frames to wait</param>
    /// <param name="onFinish">Action to perform on completion</param>
    public static void WaitForFrames(int frames, Action onFinish)
    {
        Coroutines.Start(CoWaitForFrames(frames, onFinish));
    }

    private static IEnumerator CoWaitForFrames(int frames, Action onFinish)
    {
        {
            for (var i = 0; i < frames; i++)
                yield return null;
            onFinish.Invoke();
            onFinish = null;
        }
    }

    /// <summary>
    ///     Loads all sprites into cache for the current map
    /// </summary>
    public static void PreloadAllMapSprites()
    {
        // Get current map
        var map = GameConfiguration.CurrentMap;
        if (map == null)
            return;

        // Log
        LILogger.Info("Preloading all sprites for " + map.name);

        // Start Async Loading
        GCHandler.SetDefaultBehavior(GCBehavior.DisposeOnMapUnload);
        var spriteBuilder = new SpriteBuilder();
        foreach (var elem in map.elements)
            if (elem.properties.spriteID != null)
                spriteBuilder.LoadSprite(elem, _ => { });
    }

    /// <summary>
    ///     Searches an array of LISounds
    ///     for a specific sound by type
    /// </summary>
    /// <param name="sounds">List of sounds to search</param>
    /// <param name="type">Search query</param>
    /// <returns>LISound with type or null</returns>
    public static LISound? FindSound(LISound[]? sounds, string type)
    {
        if (sounds == null)
            return null;
        foreach (var sound in sounds)
            if (sound.type == type)
                return sound;
        return null;
    }

    /// <summary>
    ///     Clones the sprite from a prefab if the
    ///     object does not already have one.
    /// </summary>
    /// <param name="obj">Object to append sprite to</param>
    /// <param name="prefab">Prefab to clone sprite from</param>
    /// <param name="isSpriteAnim">TRUE if it should clone SpriteAnim components too</param>
    /// <returns>obj's SpriteRenderer</returns>
    public static SpriteRenderer CloneSprite(GameObject obj, GameObject? prefab, bool isSpriteAnim = false)
    {
        var prefabRenderer = prefab?.GetComponentInChildren<SpriteRenderer>(true);
        if (!prefabRenderer)
            throw new Exception("Failed to get SpriteRenderer from prefab");

        var spriteRenderer = obj.GetComponentInChildren<SpriteRenderer>();
        if (!spriteRenderer)
        {
            spriteRenderer = obj.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = prefabRenderer.sprite;

            if (isSpriteAnim)
            {
                var prefabAnim = prefab.GetComponentInChildren<SpriteAnim>();
                var spriteAnim = obj.AddComponent<SpriteAnim>();
                spriteAnim.m_defaultAnim = prefabAnim.m_defaultAnim;
                spriteAnim.m_speed = prefabAnim.m_speed;
                spriteAnim.Play(prefabAnim.m_defaultAnim, prefabAnim.m_speed);
            }
        }

        spriteRenderer.material = prefabRenderer.material;
        obj.layer = (int)Layer.ShortObjects;
        return spriteRenderer;
    }

    /// <summary>
    ///     Adjusts a Vector3 position's Z value by its Y value
    ///     such that the player is always on Z=-5
    /// </summary>
    /// <param name="vector">Vector to scale</param>
    /// <returns>Vector with adjusted Z</returns>
    public static Vector3 ScaleZPositionByY(Vector3 vector)
    {
        return vector - new Vector3(0, 0, -(vector.y / 1000.0f) + LIConstants.PLAYER_POS);
    }

    /// <summary>
    ///     Traverses a transform hierarchy and returns a list of transforms that match the given path.
    ///     <c>Transform.Find("path/to/transform");</c> would perform this. However, it only returns 1 transform per path.
    /// </summary>
    /// <param name="path">The path to search for.  The path is a string of transform names separated by forward slashes.</param>
    /// <param name="parent">The transform to start the search from.</param>
    /// <returns>A list of transforms that match the given path.</returns>
    public static List<Transform> GetTransforms(string path, Transform parent)
    {
        var pathParts = path.Split('/');

        // Abort Recursion
        if (pathParts.Length == 0)
            return new List<Transform>();
        if (pathParts.Length == 1)
        {
            List<Transform> transforms = new();
            for (var i = 0; i < parent.childCount; i++)
                if (parent.GetChild(i).name == pathParts[0])
                    transforms.Add(parent.GetChild(i));
            return transforms;
        }

        // Continue Recursion
        var firstPart = pathParts[0];
        var remainingPath = string.Join("/", pathParts.Skip(1).ToArray());
        var firstPartTransforms = GetTransforms(firstPart, parent);
        var results = new List<Transform>();
        foreach (var firstPartTransform in firstPartTransforms)
            results.AddRange(GetTransforms(remainingPath, firstPartTransform));

        return results;
    }

    /// <summary>
    ///     Gets the default 100x100 white square sprite
    /// </summary>
    /// <returns>A 100x100 default white square</returns>
    public static Sprite GetDefaultSquare()
    {
        if (_defaultSquare != null)
            return _defaultSquare;

        // Create Texture
        var texture = new Texture2D(100, 100, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point,
            hideFlags = HideFlags.HideAndDontSave,
            requestedMipmapLevel = 0
        };

        // Fill Texture
        for (var x = 0; x < 100; x++)
        for (var y = 0; y < 100; y++)
            texture.SetPixel(x, y, Color.white);
        texture.Apply();

        // Generate Sprite
        _defaultSquare = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100.0f,
            0,
            SpriteMeshType.FullRect
        );

        return _defaultSquare;
    }
}