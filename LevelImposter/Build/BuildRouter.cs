using System;
using System.Collections.Generic;
using LevelImposter.Build.Attributes;
using LevelImposter.Build.Utils;
using LevelImposter.Core.Components;
using LevelImposter.Core.Models;
using LevelImposter.Core.Services.Ship;
using LevelImposter.Core.Utils;
using UnityEngine;

namespace LevelImposter.Build;

public static class BuildRouter
{
    public static void BuildMap(
        LIMap map,
        LIBaseShip baseShip,
        Dictionary<string, object?> buildMethodParameters)
    {
        // Create GameObjects
        foreach (var element in map.elements)
        {
            var gameObject = CreateGameObject(element, baseShip.transform);
            baseShip.MapObjectDB.AddObject(element, gameObject);
        }

        // Apply Hierarchy
        // Only AFTER all GameObjects are created
        foreach (var element in map.elements)
            ApplyGameObjectHierarchy(element, baseShip.MapObjectDB);

        // Update parameters
        buildMethodParameters["baseShip"] = baseShip;
        buildMethodParameters["map"] = map;

        // Build elements by priority
        foreach (var buildMethods in BuilderRegistry.BuildMethodsByPriority)
        {
            // Slice builders
            var mapBuilders = buildMethods.Slice(
                map.MapTarget,
                MapBuilderAttribute.BuilderType.MapBuilder);

            var elementBuilders = buildMethods.Slice(
                map.MapTarget,
                MapBuilderAttribute.BuilderType.ElementBuilder);

            // Build Map
            foreach (var builder in mapBuilders)
                builder.Invoke(buildMethodParameters);

            // Build Elements
            foreach (var element in map.elements)
                BuildElement(
                    element,
                    baseShip.MapObjectDB.GetObject(element.id),
                    elementBuilders,
                    buildMethodParameters);
        }
    }

    private static void BuildElement(
        LIElement element,
        GameObject? gameObject,
        Builder[] buildMethods,
        Dictionary<string, object?> buildMethodParameters)
    {
        try
        {
            buildMethodParameters["element"] = element;
            buildMethodParameters["gameObject"] = gameObject;

            foreach (var builder in buildMethods)
            {
                // Skip if the builder isn't an element builder
                if (builder.Attribute.Type != MapBuilderAttribute.BuilderType.ElementBuilder)
                    continue;

                // Skip if the element type doesn't match
                if (!(builder.Attribute.ElementTypes?.Contains(element.type) ?? true))
                    continue;

                builder.Invoke(buildMethodParameters);
            }
        }
        catch (Exception ex)
        {
            LILogger.Warn($"Error building {element}: {ex}");
            LILogger.LogException(ex);
        }
    }

    /// <summary>
    ///     Creates a GameObject for the given LIElement and sets its parent transform.
    /// </summary>
    /// <param name="element">Element to create GameObject for</param>
    /// <param name="parentTransform">Parent transform to set for the new GameObject</param>
    /// <returns>The created GameObject</returns>
    private static GameObject CreateGameObject(LIElement element, Transform? parentTransform = null)
    {
        // Create GameObject
        var gameObjectName = element.name.Replace("\\n", " ");
        var gameObject = new GameObject(gameObjectName);

        // Set Transform
        gameObject.transform.SetParent(parentTransform);

        // Add to DB
        return gameObject;
    }

    /// <summary>
    ///     Sets the parent-child relationships of GameObjects based on the properties of their corresponding LIElements.
    ///     Requires all GameObjects in the map to be created beforehand.
    /// </summary>
    /// <param name="element">Element to apply hierarchy to</param>
    /// <param name="mapObjectDB">Object DB to help correlate LIElements to GameObjects</param>
    private static void ApplyGameObjectHierarchy(LIElement element, MapObjectDB mapObjectDB)
    {
        // Get Element Properties
        var elemObject = mapObjectDB.GetObject(element.id);
        if (elemObject == null)
            return;

        // Get Parent ID
        var parent = element.parentID;
        if (parent == null)
            return;

        // Find Parent Object
        var parentObject = mapObjectDB.GetObject((Guid)parent);
        if (parentObject == null)
            return;

        // Get Parent Element Properties
        var parentElement = mapObjectDB.GetElement(parentObject);
        if (parentElement == null)
            return;

        // Check if parent is a util-layer
        if (parentElement.type != "util-layer")
            return;

        // Set Parent
        elemObject.transform.SetParent(parentObject.transform);
    }
}