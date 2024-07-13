﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LevelImposter.Core
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// <c>MapObjectData</c> is appended to GameObjects that are built from <c>LIElement</c>s.
        /// This pulls that object data or throws an Exception if the object was not an <c>LIElement</c>.
        /// </summary>
        /// <param name="gameObject">GameObject to pull data from</param>
        /// <returns>The cooresponding map object data</returns>
        /// <exception cref="Exception">If the object was not an <c>LIElement</c></exception>
        public static MapObjectData GetLIData(this GameObject gameObject)
        {
            var mapObjectData = gameObject.GetComponent<MapObjectData>();
            if (mapObjectData == null)
                throw new Exception($"{gameObject} is missing LI data");
            return mapObjectData;
        }

        /// <summary>
        /// Equivelent of <c>GameObject.GetComponent</c> but throws an exception if the component is null or missing.
        /// </summary>
        /// <typeparam name="T">Type of component to get</typeparam>
        /// <param name="gameObject">GameObject to search</param>
        /// <returns>Cooresponding component, never null.</returns>
        /// <exception cref="Exception">If the component is null or missing</exception>
        public static T GetComponentOrThrow<T>(this GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (component == null)
                throw new Exception($"{gameObject} is missing {typeof(T).FullName}");
            return component;
        }
    }
}
