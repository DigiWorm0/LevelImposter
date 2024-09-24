using System.Collections;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Core;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LevelImposter.DB;

/// <summary>
///     A Singleton GameObject that stores references to
///     sprites, minigames, and prefabs from other locations
///     within the Among Us game.
/// </summary>
internal class AssetDB : MonoBehaviour
{
    public const string LEVELIMPOSTER_MAP_NAME = "Random LI Map";
    private const string SUBMERGED_MAP_GUID = "Submerged";
    private readonly Stack<MapType> _loadedShips = new();
    private bool _isInit;
    private ObjectDB? _objectDB;
    private PathDB? _pathDB;
    private SerializedAssetDB? _serializedAssetDB;
    private SoundDB? _soundDB;

    private TaskDB? _taskDB;
    public static AssetDB? Instance { get; private set; }
    public static bool IsInit => Instance?._isInit == true;

    public string Status { get; private set; } = "Initializing";

    public void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Start()
    {
        StartCoroutine(CoLoadAssets().WrapToIl2Cpp());
    }

    /// <summary>
    ///     Gets a GameObject from the ObjectDB
    /// </summary>
    /// <param name="id">ID to lookup</param>
    /// <returns>GameObject or null if object couldn't be found</returns>
    public static GameObject? GetObject(string id)
    {
        var prefab = Instance?._objectDB?.Get(id);
        if (prefab == null)
            LILogger.Warn($"Could not find prefab of type {id}");
        return prefab;
    }

    /// <summary>
    ///     Gets a PlayerTask from the TaskDB
    /// </summary>
    /// <typeparam name="T">Type to cast abstract PlayerTask to</typeparam>
    /// <param name="id">ID to lookup</param>
    /// <returns>PlayerTask or null if task couldn't be found</returns>
    public static T? GetTask<T>(string id) where T : PlayerTask
    {
        var taskPrefab = Instance?._taskDB?.Get(id);
        if (taskPrefab == null)
            LILogger.Warn($"Could not find task of type {id}");
        return taskPrefab?.Cast<T>();
    }

    /// <summary>
    ///     Gets a list of paths for an object
    ///     within a transform from the PathDB
    /// </summary>
    /// <param name="id">ID to lookup</param>
    /// <returns>String or null if object id couldn't be found</returns>
    public static string[]? GetPaths(string id)
    {
        var path = Instance?._pathDB?.Get(id);
        if (path == null || path.Length == 0)
            LILogger.Warn($"Could not find path of type {id}");
        return path;
    }

    /// <summary>
    ///     Checks if an element ID contains a task behaviour
    /// </summary>
    /// <param name="id">ID of the task</param>
    /// <returns>TRUE if the task exists</returns>
    public static bool HasTask(string id)
    {
        var taskPrefab = Instance?._taskDB?.Get(id);
        return taskPrefab != null;
    }

    /// <summary>
    ///     Gets the length of a task from the TaskDB
    /// </summary>
    /// <param name="id">ID to lookup</param>
    /// <returns>TaskLength or TaskLength.Short if task couldn't be found</returns>
    public static TaskLength GetTaskLength(string id)
    {
        var serializedTask = Instance?._serializedAssetDB?.TaskDB.Find(elem => elem.ID == id);
        return serializedTask?.TaskType ?? TaskLength.Short;
    }

    /// <summary>
    ///     Gets an AudioClip from the SoundDB
    /// </summary>
    /// <param name="id">ID to lookup</param>
    /// <returns>AudioClip or null if couldn't be found</returns>
    public static AudioClip? GetSound(string id)
    {
        var audioClip = Instance?._soundDB?.Get(id);
        if (!audioClip)
            LILogger.Warn($"Could not find audio of type {id}");
        return audioClip;
    }

    /// <summary>
    ///     Coroutine to load all assets into the AssetDB
    /// </summary>
    [HideFromIl2Cpp]
    private IEnumerator CoLoadAssets()
    {
        {
            // Add Ship Prefab
            var shipPrefabs = AmongUsClient.Instance.ShipPrefabs;
            var miraPrefab = shipPrefabs[(int)MapType.Mira];
            var mapCount = (int)MapType.LevelImposter;
            while (shipPrefabs.Count <= mapCount)
                shipPrefabs.Add(miraPrefab); // TODO: Use Own Ship AssetReference
            while (Constants.MapNames.Count <= mapCount)
                Constants.MapNames = MapUtils.AddToArr(Constants.MapNames,
                    Constants.MapNames.Count == mapCount ? LIConstants.MAP_NAME : "");

            // Deserialize AssetDB
            _serializedAssetDB = MapUtils.LoadJsonResource<SerializedAssetDB>("SerializedAssetDB.json");
            if (_serializedAssetDB == null)
            {
                LILogger.Warn("Serialized AssetDB was not found in Assembly resources");
                yield break;
            }

            // Sub-DBs
            _objectDB = new ObjectDB(_serializedAssetDB);
            _taskDB = new TaskDB(_serializedAssetDB);
            _soundDB = new SoundDB(_serializedAssetDB);
            _pathDB = new PathDB(_serializedAssetDB);

            // Ship References
            Status = "Loading ship references";
            LILogger.Info("Loading AssetDB...");
            for (var i = 0; i < shipPrefabs.Count; i++)
            {
                // Load AssetReference
                AssetReference shipRef = shipPrefabs[i];
                while (true)
                {
                    if (shipRef.Asset != null)
                        break;
                    if (shipRef.AssetGUID == SUBMERGED_MAP_GUID)
                        break;
                    AsyncOperationHandle op = shipRef.LoadAssetAsync<GameObject>();
                    if (!op.IsValid())
                    {
                        LILogger.Warn(
                            $"Could not import [{shipRef.AssetGUID}] due to invalid Async Operation. Trying again in 5 seconds...");
                        yield return new WaitForSeconds(5);
                        continue;
                    }

                    yield return op;
                    if (op.Status != AsyncOperationStatus.Succeeded)
                        LILogger.Warn(
                            $"Could not import [{shipRef.AssetGUID}] due to failed Async Operation. Ignoring...");
                }

                // Import GameObject
                if (shipRef.Asset != null)
                {
                    var shipPrefab = shipRef.Asset.Cast<GameObject>();
                    LoadShip(shipPrefab);
                    yield return null;
                }
                else
                {
                    LILogger.Warn($"Could not import [{shipRef.AssetGUID}]. Ignoring...");
                }
            }

            Status = "Finalizing";
            _objectDB.Load();
            _taskDB.Load();
            _soundDB.Load();
            _pathDB.Load();
            _isInit = true;
        }
    }

    /// <summary>
    ///     Imports a single prefab into the AssetDB
    /// </summary>
    /// <param name="prefab">Ship prefab to load</param>
    private void LoadShip(GameObject prefab)
    {
        Status = $"Loading \"{prefab.name}\"";
        var shipStatus = prefab.GetComponent<ShipStatus>();
        var mapType = prefab.name switch
        {
            "SkeldShip" => MapType.Skeld,
            "MiraShip" => MapType.Mira,
            "PolusShip" => MapType.Polus,
            "Airship" => MapType.Airship,
            "FungleShip" => MapType.Fungle,
            _ => MapType.LevelImposter
        };
        if (mapType == MapType.LevelImposter)
            return;
        if (_loadedShips.Contains(mapType))
            return;
        _loadedShips.Push(mapType);

        _objectDB?.LoadShip(shipStatus, mapType);
        _taskDB?.LoadShip(shipStatus, mapType);
        _soundDB?.LoadShip(shipStatus, mapType);
        _pathDB?.LoadShip(shipStatus, mapType);

        LILogger.Info($"...{prefab.name} Loaded");
    }
}