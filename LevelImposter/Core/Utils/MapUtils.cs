using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    class MapUtils
    {
        public static Dictionary<SystemTypes, string> systemRenames = new();
        public static Dictionary<TaskTypes, string> taskRenames = new();

        /// <summary>
        /// Adds an element to an Il2CppReferenceArray
        /// </summary>
        /// <typeparam name="T">Array Type</typeparam>
        /// <param name="arr">Array to add to</param>
        /// <param name="value">Element to add</param>
        /// <returns>New array with value appended</returns>
        public static UnhollowerBaseLib.Il2CppReferenceArray<T> AddToArr<T>(UnhollowerBaseLib.Il2CppReferenceArray<T> arr, T value) where T : UnhollowerBaseLib.Il2CppObjectBase
        {
            List<T> list = new List<T>(arr);
            list.Add(value);
            return list.ToArray();
        }

        /// <summary>
        /// Shuffles an Il2CppStructArray
        /// </summary>
        /// <param name="arr">Array to shuffle</param>
        /// <returns>New array with values shuffled</returns>
        public static UnhollowerBaseLib.Il2CppStructArray<byte> Shuffle(UnhollowerBaseLib.Il2CppStructArray<byte> arr)
        {
            List<byte> listA = new List<byte>(arr);
            List<byte> listB = new List<byte>();
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
            systemRenames[system] = name;
        }

        /// <summary>
        /// Renames a TaskTypes in the TranslationController
        /// </summary>
        /// <param name="system">Task to rename</param>
        /// <param name="name">String to rename to</param>
        public static void Rename(TaskTypes system, string name)
        {
            taskRenames[system] = name;
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
            byte[] byteData = MapUtils.ParseBase64(base64);
            AudioClip audio = WAVLoader.Load(name, byteData); // TODO Support other audio formats
            return audio;
        }
    }
}
