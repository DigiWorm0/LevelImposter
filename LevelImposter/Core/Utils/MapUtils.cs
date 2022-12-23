using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Events;
using System.IO;
using LevelImposter.DB;
using LevelImposter.Shop;
using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime;
using AmongUs.GameOptions;

namespace LevelImposter.Core
{
    /// <summary>
    /// A variety of utility functions for constructing the map
    /// </summary>
    public class MapUtils
    {
        public static Dictionary<SystemTypes, string> SystemRenames = new();
        public static Dictionary<TaskTypes, string> TaskRenames = new();

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
        /// <returns>Byte array from data</returns>
        public static byte[] ParseBase64(string base64)
        {
            string sub64 = base64.Substring(base64.IndexOf(",") + 1);
            return Convert.FromBase64String(sub64);
        }

        /// <summary>
        /// Converts a base64 encoded string into a Unity AudioClip
        /// </summary>
        /// <param name="name">Name of the AudioClip object</param>
        /// <param name="base64">Base64 encoded data</param>
        /// <returns>Unity AudioClip from data</returns>
        public static AudioClip ConvertToAudio(string name, string base64)
        {
            byte[] byteData = ParseBase64(base64);
            AudioClip audio = WAVLoader.LoadPCM(byteData); // TODO Support other audio formats
            LIShipStatus.Instance.AddMapSound(audio);
            return audio;
        }

        /// <summary>
        /// Converts an LIColor to UnityEngine.Color
        /// </summary>
        /// <param name="color">Color to convert from</param>
        /// <returns>UnityEngine.Color to convert to</returns>
        public static Color LIColorToColor(LIColor color)
        {
            return new Color (
                color.r / 255,
                color.g / 255,
                color.b / 255,
                color.a
            );
        }

        /// <summary>
        /// Fires trigger on an object with a specific name
        /// </summary>
        /// <param name="obj">Object to trigger</param>
        /// <param name="triggerID">Trigger ID</param>
        public static void FireTrigger(GameObject obj, string triggerID, GameObject orgin)
        {
            LITriggerable[] triggers = obj.GetComponents<LITriggerable>();
            LITriggerable trigger = Array.Find(triggers, (LITriggerable t) => t.ID == triggerID);
            if (trigger != null)
                trigger.Trigger(orgin);
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
        /// Generates a Sprite from byte dara
        /// </summary>
        /// <param name="data">png formated texture data</param>
        /// <returns>A Unity Sprite representing the texture data</returns>
        public static Sprite GenerateSprite(byte[] data)
        {
            Texture.allowThreadedTextureCreation = true;
            Texture2D texture = new(1, 1);
            ImageConversion.LoadImage(texture, data);
            LIShipStatus.Instance.AddMapTexture(texture);
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
        }

        /// <summary>
        /// Loads a GameObject from the local Asset Bundle by Name
        /// </summary>
        /// <param name="name">Name of the Asset from the Asset Bundle</param>
        /// <returns></returns>
        public static GameObject LoadAssetBundle(string name)
        {
            Stream resourceStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("LevelImposter.Assets." + name);
            using (var ms = new MemoryStream())
            {
                resourceStream.CopyTo(ms);
                byte[] assetData = ms.ToArray();
                AssetBundle assetBundle = AssetBundle.LoadFromMemory(assetData);
                GameObject asset = assetBundle.LoadAsset(name, Il2CppType.Of<GameObject>()).Cast<GameObject>();
                assetBundle.Unload(false);
                return asset;
            }
        }

        /// <summary>
        /// Syncs the current map over RPC
        /// </summary>
        public static void SyncMapID()
        {
            if (!AmongUsClient.Instance.AmHost || DestroyableSingleton<TutorialManager>.InstanceExists || PlayerControl.LocalPlayer == null)
                return;

            Guid mapID = Guid.Empty;
            if (MapLoader.CurrentMap != null)
                Guid.TryParse(MapLoader.CurrentMap.id, out mapID);
            string mapIDStr = mapID.ToString();

            LILogger.Info("[RPC] Transmitting map ID [" + mapIDStr + "]");
            ReactorRPC.RPCSendMapID(PlayerControl.LocalPlayer, mapIDStr);

            // Set Skeld
            if (mapID != Guid.Empty)
            {
                IGameOptions currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
                currentGameOptions.SetByte(ByteOptionNames.MapId, (int)MapNames.Polus);
                GameOptionsManager.Instance.GameHostOptions = GameOptionsManager.Instance.CurrentGameOptions;
                GameManager.Instance.LogicOptions.SyncOptions();
            }
        }
    }
}
