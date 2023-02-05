using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using System.IO;
using LevelImposter.DB;
using LevelImposter.Shop;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime;
using AmongUs.GameOptions;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using System.Collections;

namespace LevelImposter.Core
{
    /// <summary>
    /// A variety of utility functions for constructing the map
    /// </summary>
    public static class MapUtils
    {
        public static Dictionary<SystemTypes, string> SystemRenames = new();
        public static Dictionary<TaskTypes, string> TaskRenames = new();
        private static int _randomSeed = 0;

        /// <summary>
        /// Adds an element to an Il2CppReferenceArray
        /// </summary>
        /// <typeparam name="T">Array Type</typeparam>
        /// <param name="arr">Array to add to</param>
        /// <param name="value">Element to add</param>
        /// <returns>New array with value appended</returns>
        public static Il2CppReferenceArray<T> AddToArr<T>(Il2CppReferenceArray<T> arr, T value) where T : Il2CppObjectBase
        {
            List<T> list = new(arr);
            list.Add(value);
            return list.ToArray();
        }
        public static Il2CppStringArray AddToArr(Il2CppStringArray arr, string value)
        {
            List<string> list = new(arr);
            list.Add(value);
            return list.ToArray();
        }

        /// <summary>
        /// Shuffles an Il2CppStructArray
        /// </summary>
        /// <param name="arr">Array to shuffle</param>
        /// <returns>New array with values shuffled</returns>
        public static Il2CppStructArray<byte> Shuffle(Il2CppStructArray<byte> arr)
        {
            List<byte> listA = new(arr);
            List<byte> listB = new();
            while (listA.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, listA.Count);
                listB.Add(listA[index]);
                listA.RemoveAt(index);
            }
            return listB.ToArray();
        }

        /// <summary>
        /// Checks if an LIElement has a solid collider
        /// </summary>
        /// <param name="elem">Element to search</param>
        /// <returns>True if element contains a solid collider</returns>
        public static bool HasSolidCollider(LIElement elem)
        {
            if (elem.properties == null)
                return false;
            if (elem.properties.colliders == null)
                return false;
            foreach (LICollider collider in elem.properties.colliders)
            {
                if (collider.isSolid)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Clones the colliders from a Unity GameObject to another
        /// </summary>
        /// <param name="from">Source GameObject</param>
        /// <param name="to">Target GameObject</param>
        public static void CloneColliders(GameObject from, GameObject to)
        {
            if (from.GetComponent<CircleCollider2D>() != null)
            {
                CircleCollider2D origBox = from.GetComponent<CircleCollider2D>();
                CircleCollider2D box = to.AddComponent<CircleCollider2D>();
                box.radius = origBox.radius;
                box.offset = origBox.offset;
                box.isTrigger = true;
            }
            if (from.GetComponent<BoxCollider2D>() != null)
            {
                BoxCollider2D origBox = from.GetComponent<BoxCollider2D>();
                BoxCollider2D box = to.AddComponent<BoxCollider2D>();
                box.size = origBox.size;
                box.offset = origBox.offset;
                box.isTrigger = true;
            }
            if (from.GetComponent<PolygonCollider2D>() != null)
            {
                PolygonCollider2D origBox = from.GetComponent<PolygonCollider2D>();
                PolygonCollider2D box = to.AddComponent<PolygonCollider2D>();
                box.points = origBox.points;
                box.pathCount = origBox.pathCount;
                box.offset = origBox.offset;
                box.isTrigger = true;
            }
        }

        /// <summary>
        /// Either grabs solid colliders from 
        /// prefab or creates new ones. Used
        /// for any UI buttons or in-game consoles.
        /// </summary>
        /// <param name="src">Object to set colliders</param>
        /// <param name="prefab">Prefab to copy colliders from</param>
        public static Collider2D CreateDefaultColliders(GameObject src, GameObject prefab)
        {
            PolygonCollider2D[] solidColliders = src.GetComponentsInChildren<PolygonCollider2D>();
            if (solidColliders.Length <= 0)
                CloneColliders(prefab, src);
            Collider2D? collider = src.GetComponent<Collider2D>();
            if (collider == null)
                collider = src.AddComponent<BoxCollider2D>();
            return collider;
        }

        /// <summary>
        /// Renames a SystemType in the TranslationController
        /// </summary>
        /// <param name="system">System to rename</param>
        /// <param name="name">String to rename to</param>
        public static void Rename(SystemTypes system, string name)
        {
            SystemRenames[system] = name;
        }

        /// <summary>
        /// Renames a TaskTypes in the TranslationController
        /// </summary>
        /// <param name="system">Task to rename</param>
        /// <param name="name">String to rename to</param>
        public static void Rename(TaskTypes system, string name)
        {
            TaskRenames[system] = name;
        }

        /// <summary>
        /// Converts a base64 encoded string into a byte array
        /// </summary>
        /// <param name="base64">Base64 encoded data</param>
        /// <returns>Stream of bytes representing raw base64 data</returns>
        public static byte[] ParseBase64(string base64)
        {
            string sub64 = base64.Substring(base64.IndexOf(",") + 1);
            return Convert.FromBase64String(sub64);
        }

        /// <summary>
        /// Converts an LIColor to UnityEngine.Color
        /// </summary>
        /// <param name="color">Color to convert from</param>
        /// <returns>UnityEngine.Color to convert to</returns>
        public static Color LIColorToColor(LIColor? color)
        {
            if (color == null)
                return Color.white;
            return new Color(
                color.r / 255,
                color.g / 255,
                color.b / 255,
                color.a
            );
        }

        /// <summary>
        /// Checks if a GameObject is the local player
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
        /// Grabs a resource from the assembly
        /// </summary>
        /// <param name="name">Name of the resource file</param>
        /// <returns>Raw resource data</returns>
        private static byte[]? GetResource(string name)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream? resourceStream = assembly.GetManifestResourceStream($"LevelImposter.Assets.{name}"))
            {
                if (resourceStream == null)
                    return null;

                byte[] resourceData = new byte[resourceStream.Length];
                resourceStream.Read(resourceData);
                return resourceData;
            }
        }

        /// <summary>
        /// Loads a GameObject from asembly resources
        /// </summary>
        /// <param name="name">Name of the AssetBundle file</param>
        /// <returns>GameObject or null if not found</returns>
        public static GameObject? LoadAssetBundle(string name)
        {
            byte[]? assetData = GetResource(name);
            if (assetData == null)
                return null;
            AssetBundle assetBundle = AssetBundle.LoadFromMemory(assetData);
            GameObject asset = assetBundle.LoadAsset(name, Il2CppType.Of<GameObject>()).Cast<GameObject>();
            assetBundle.Unload(false);
            return asset;
        }

        /// <summary>
        /// Loads a Sprite from assembly resources
        /// </summary>
        /// <param name="name">Name of the sprite file</param>
        /// <returns>Sprite or null if not found</returns>
        public static Sprite? LoadSpriteResource(string name)
        {
            byte[]? spriteData = GetResource(name);
            if (spriteData == null)
                return null;
            return SpriteLoader.Instance?.LoadSprite(spriteData);
        }

        /// <summary>
        /// Deserializes a JSON object from assembly resources
        /// </summary>
        /// <typeparam name="T">JSON type to deserialize to</typeparam>
        /// <param name="name">Name of the json file</param>
        /// <returns>JSON object or null if not found</returns>
        public static T? LoadJsonResource<T>(string name) where T : class
        {
            byte[]? jsonData = GetResource(name);
            if (jsonData == null)
                return null;
            string jsonString = Encoding.UTF8.GetString(jsonData);
            return JsonSerializer.Deserialize<T>(jsonString);
        }

        /// <summary>
        /// Syncs the current map over RPC
        /// </summary>
        public static void SyncMapID()
        {
            if (!AmongUsClient.Instance.AmHost || DestroyableSingleton<TutorialManager>.InstanceExists || PlayerControl.LocalPlayer == null)
                return;

            string mapIDStr = MapLoader.CurrentMap?.id ?? Guid.Empty.ToString();
            if (!Guid.TryParse(mapIDStr, out _))
            {
                LILogger.Error($"Invalid map ID [{mapIDStr}]");
                return;
            }
            LILogger.Info($"[RPC] Transmitting map ID [{mapIDStr}]");
            MapSync.RPCSendMapID(PlayerControl.LocalPlayer, mapIDStr);

            // Set Skeld
            if (mapIDStr != Guid.Empty.ToString())
            {
                IGameOptions currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
                currentGameOptions.SetByte(ByteOptionNames.MapId, (byte)MapType.LevelImposter); // TODO: Move MapID outside default range
                GameOptionsManager.Instance.GameHostOptions = GameOptionsManager.Instance.CurrentGameOptions;
                GameManager.Instance.LogicOptions.SyncOptions();
            }
        }

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
                onFinish = null;
            }
        }

        /// <summary>
        /// Gets a random value based on an element GUID.
        /// Given the same GUID, weight, and seed will always return with the same value.
        /// Random seeds are synchronized it across all clients and
        /// regenerated with <c>MapUtils.SyncRandomSeed()</c>
        /// </summary>
        /// <param name="id">GUID identifier</param>
        /// <param name="weight">A weight value to generate new numbers with the same GUID</param>
        /// <returns>A random float between 0.0 and 1.0 (inclusive)</returns>
        public static float GetRandom(Guid id, int weight = 0)
        {
            int trueSeed = id.GetHashCode() + _randomSeed + weight;
            UnityEngine.Random.InitState(trueSeed);
            return UnityEngine.Random.value;
        }

        /// <summary>
        /// Generates a new random seed and
        /// synchronizes it across all clients.
        /// </summary>
        public static void SyncRandomSeed()
        {
            bool isConnected = AmongUsClient.Instance.AmConnected;
            if (isConnected && (!AmongUsClient.Instance.AmHost || PlayerControl.LocalPlayer == null))
                return;
            UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
            int newSeed = UnityEngine.Random.RandomRange(int.MinValue, int.MaxValue);
            if (isConnected)
                RPCSyncRandomSeed(PlayerControl.LocalPlayer, newSeed);
            else
                _randomSeed = newSeed;
        }
        [MethodRpc((uint)LIRpc.SyncRandomSeed)]
        private static void RPCSyncRandomSeed(PlayerControl _, int randomSeed)
        {
            LILogger.Info($"[RPC] New random seed set: {randomSeed}");
            _randomSeed = randomSeed;
        }

        /// <summary>
        /// Searches an array of LISounds
        /// for a specific sound by name
        /// </summary>
        /// <param name="sounds">List of sounds to search</param>
        /// <param name="name">Search query</param>
        /// <returns>LISound with name or null</returns>
        public static LISound? FindSound(LISound[]? sounds, string name)
        {
            if (sounds == null)
                return null;
            foreach (LISound sound in sounds)
                if (sound.type == name)
                    return sound;
            return null;
        }

        /// <summary>
        /// Clones the sprite from a prefab if the
        /// object does not already have one.
        /// </summary>
        /// <param name="obj">Object to append sprite to</param>
        /// <param name="prefab">Prefab to clone sprite from</param>
        /// <param name="isSpriteAnim">TRUE if it should clone SpriteAnim components too</param>
        /// <returns>obj's SpriteRenderer</returns>
        public static SpriteRenderer CloneSprite(GameObject obj, GameObject prefab, bool isSpriteAnim = false)
        {
            var prefabRenderer = prefab.GetComponent<SpriteRenderer>();
            var spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (!spriteRenderer)
            {
                spriteRenderer = obj.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = prefabRenderer.sprite;

                if (isSpriteAnim)
                {
                    var prefabAnim = prefab.GetComponent<PowerTools.SpriteAnim>();
                    var spriteAnim = obj.AddComponent<PowerTools.SpriteAnim>();
                    spriteAnim.Play(prefabAnim.m_defaultAnim, prefabAnim.Speed);
                }
            }
            spriteRenderer.material = prefabRenderer.material;
            obj.layer = (int)Layer.ShortObjects;
            return spriteRenderer;
        }
    }
}
