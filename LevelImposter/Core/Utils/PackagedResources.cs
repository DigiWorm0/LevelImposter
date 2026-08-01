using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using LevelImposter.AssetLoader;
using Reactor.Utilities;
using UnityEngine;

namespace LevelImposter.Core;

/// <summary>
///     Represents all the resources that are packaged within the plugin DLL.
/// </summary>
public static class PackagedResources
{
    private const string RESOURCE_BUNDLE_NAME = "li_resources";
    private static AssetBundle? _cachedAssetBundle;

    private static AssetBundle GetAssetBundle()
    {
        if (_cachedAssetBundle != null)
            return _cachedAssetBundle;

        LILogger.Info($"Loading asset bundle {RESOURCE_BUNDLE_NAME}");

        _cachedAssetBundle = AssetBundleManager.Load(RESOURCE_BUNDLE_NAME);
        if (_cachedAssetBundle == null)
            throw new Exception("Could not load resource asset bundle");

        return _cachedAssetBundle;
    }

    /// <summary>
    ///     Opens a stream to a resource packaged in the plugin DLL.
    /// </summary>
    /// <param name="name">The name of the resource to open.</param>
    /// <returns>A stream to the resource.</returns>
    /// <exception cref="Exception">Thrown if the resource is not found in the assembly.</exception>
    private static Stream OpenStream(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceStream = assembly.GetManifestResourceStream($"LevelImposter.Assets.{name}");
        if (resourceStream == null)
            throw new Exception($"Resource {name} not found in assembly");

        return resourceStream;
    }

    /// <summary>
    ///     Loads a sprite from packaged resources.
    /// </summary>
    /// <param name="name">Name of the sprite.</param>
    /// <returns>The loaded sprite, or null if loading failed.</returns>
    public static Sprite? LoadSprite(string name)
    {
        LILogger.Info($"Loading sprite resource {name}");

        // Open resource stream
        using var resourceStream = OpenStream(name);

        // Create Loadables
        var loadableTexture = LoadableTexture.FromMemory($"{name}-resource", resourceStream.ToIl2CppArray());
        loadableTexture.Options.GCBehavior = GCBehavior.NeverDispose;

        var loadableSprite = LoadableSprite.FromLoadableTexture(loadableTexture);
        loadableSprite.Options.GCBehavior = GCBehavior.NeverDispose;

        // Load Sprite (Synchronously)
        return SpriteLoader.Instance.LoadImmediate(loadableSprite);
    }

    /// <summary>
    ///     Loads a JSON file from packaged resources.
    ///     Data is deserialized using System.JSON.
    /// </summary>
    /// <param name="name">Name of the JSON file.</param>
    /// <typeparam name="T">Type to deserialize JSON to</typeparam>
    /// <returns>The deserialized object or null if not found</returns>
    public static T? LoadJson<T>(string name) where T : class
    {
        // Resource Stream >> JSON Text
        using var resourceStream = OpenStream(name);
        var jsonString = Encoding.UTF8.GetString(resourceStream.ToManagedArray());

        // Deserialize JSON
        return JsonSerializer.Deserialize<T>(jsonString);
    }


    /// <summary>
    ///     Loads an object from the resource asset bundle and casts it to the specified type.
    /// </summary>
    /// <param name="name">Name of the file within the AssetBundle</param>
    /// <returns>The object or null if not found</returns>
    public static T? LoadFromBundle<T>(string name) where T : Il2CppObjectBase
    {
        var asset = GetAssetBundle().LoadAsset(name, Il2CppType.Of<T>())?.Cast<T>();
        if (asset == null)
            throw new Exception($"Could not find resource {name} in asset bundle");

        return asset;
    }
}