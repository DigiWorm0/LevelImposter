using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using LevelImposter.Core;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace LevelImposter.DB
{
    /// <summary>
    /// Utility class used to search GameObjects in
    /// Among Us Map prefabs for the AssetDB.
    /// </summary>
    public static class MapSearchUtil
    {
        /// <summary>
        /// Recursively searches a GameObject's children for a
        /// GameObject with a specific name and component.
        /// </summary>
        /// <typeparam name="T">Component type</typeparam>
        /// <param name="parent">Parent GameObject</param>
        /// <param name="name">Child GameObject's name</param>
        /// <returns>The cooresponding component. Null otherwise.</returns>
        public static T SearchComponent<T>(GameObject parent, string name)
        {
            GameObject obj = SearchChildren(parent, name);
            if (obj != null)
                return obj.GetComponent<T>();
            LILogger.Warn("Could not find " + name + " in " + parent);
            return default(T);
        }

        /// <summary>
        /// Recursively searches a GameObject's children for
        /// a specific SpriteRenderer with a specific sprite.
        /// </summary>
        /// <param name="parent">Parent GameObject</param>
        /// <param name="name">Child GameObject's name</param>
        /// <param name="spriteName">Sprite name</param>
        /// <returns>The cooresponding SpriteRenderer. Null otherwise.</returns>
        public static SpriteRenderer SearchSprites(GameObject parent, string name, string spriteName)
        {
            List<Transform> output = new();
            SearchChildren(parent.transform, name, output);

            foreach (Transform t in output)
            {
                SpriteRenderer r = t.gameObject.GetComponent<SpriteRenderer>();
                if (r == null)
                    continue;
                if (r.sprite == null)
                    continue;
                if (r.sprite.name == spriteName)
                    return r;
            }
            LILogger.Warn("Could not find " + spriteName + " in " + name);
            return null;
        }

        /// <summary>
        /// Recursively searches a GameObject's children
        /// for a GameObject with a specific name
        /// </summary>
        /// <param name="parent">Parent GameObject</param>
        /// <param name="name">Child GameObject's name</param>
        /// <returns>The first GameObject with the cooresponding name. Null otherwise.</returns>
        public static GameObject SearchChildren(GameObject parent, string name)
        {
            List<Transform> output = new();
            SearchChildren(parent.transform, name, output);

            if (output.Count() > 0)
                return output[0].gameObject;
            LILogger.Warn("Could not find " + name + " in " + parent);
            return null;

        }

        /// <summary>
        /// Recursively searches a GameObject's children
        /// for list of GameObjects with a specific name
        /// </summary>
        /// <param name="parent">Parent GameObject</param>
        /// <param name="name">Child GameObject's name</param>
        /// <returns>A list of GameObjects with the cooresponding name.</returns>
        public static List<Transform> SearchMultipleChildren(GameObject parent, string name)
        {
            List<Transform> output = new();
            SearchChildren(parent.transform, name, output);

            if (output.Count() > 0)
                return output;
            LILogger.Warn("Could not find " + name + " in " + parent);
            return null;
        }

        private static void SearchChildren(Transform parent, string name, List<Transform> output)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name == name)
                {
                    output.Add(child);
                }
                SearchChildren(child, name, output);
            }
        }

        /// <summary>
        /// Searches a list of MonoBehaviours for 
        /// GameObjects with a specific name.
        /// </summary>
        /// <typeparam name="T">MonoBehaviour Type</typeparam>
        /// <param name="list">List of components</param>
        /// <param name="name">Name of the GameObject</param>
        /// <returns>The cooresponding component. Null otherwise.</returns>
        public static T SearchList<T>(Il2CppReferenceArray<T> list, string name) where T : MonoBehaviour
        {
            IEnumerable<T> elem = list.Where(t => t.name == name);
            if (elem.Count() > 0)
                return elem.First();
            LILogger.Warn("Could not find " + name + " in list");
            return null;
        }
    }
}
