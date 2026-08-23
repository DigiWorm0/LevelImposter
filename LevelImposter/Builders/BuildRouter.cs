using System;
using System.Collections.Generic;
using System.Diagnostics;
using LevelImposter.Core.Components;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using UnityEngine;

namespace LevelImposter.Builders;

public static class BuildRouter
{
    /// Time to warn the user (in ms) when an element is taking too long to load
    private const int WARN_MAX_BUILD_DURATION = 200;

    private static readonly Stopwatch BuildTimer = new();

    public static void BuildMap(
        LIMap map,
        LIBaseShip baseShip,
        Dictionary<string, object> buildMethodParameters)
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
            ApplyGameObjectHierarchy(element, baseShip);

        // Update parameters
        buildMethodParameters["baseShip"] = baseShip;
        buildMethodParameters["map"] = map;

        // Build elements by priority
        var builderGroups = BuildMethodRegistry.GroupBuildersByPriority();
        foreach (var buildMethods in builderGroups)
        foreach (var element in map.elements)
            RunBuildMethods(element, baseShip, buildMethods, buildMethodParameters);
    }

    private static void RunBuildMethods(
        LIElement element,
        LIBaseShip baseShip,
        BuildMethod[] buildMethods,
        Dictionary<string, object> buildMethodParameters)
    {
        BuildTimer.Restart();

        try
        {
            var gameObject = baseShip.MapObjectDB.GetObject(element.id);
            if (gameObject == null)
                throw new Exception("GameObject is null");

            buildMethodParameters["element"] = element;
            buildMethodParameters["gameObject"] = gameObject;

            foreach (var builder in buildMethods)
            {
                // Skip if the element type doesn't match
                if (!(builder.Attribute.ElementTypes?.Contains(element.type) ?? true))
                    continue;

                LILogger.Info($"Running {builder.Method.DeclaringType?.Name} on {element.name}");

                builder.Invoke(buildMethodParameters);
            }
        }
        catch (Exception ex)
        {
            LILogger.Warn($"Error building {element}: {ex}");
            LILogger.LogException(ex);
        }

        BuildTimer.Stop();
        if (BuildTimer.ElapsedMilliseconds > WARN_MAX_BUILD_DURATION)
            LILogger.Warn($"{element} took {BuildTimer.ElapsedMilliseconds}ms to build");
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
    /// <param name="baseShip">Base ship containing the MapObjectDB</param>
    private static void ApplyGameObjectHierarchy(LIElement element, LIBaseShip baseShip)
    {
        // Get Element Properties
        var elemObject = baseShip.MapObjectDB.GetObject(element.id);
        if (elemObject == null)
            return;

        // Get Parent ID
        var parent = element.parentID;
        if (parent == null)
            return;

        // Find Parent Object
        var parentObject = baseShip.MapObjectDB.GetObject((Guid)parent);
        if (parentObject == null)
            return;

        // Get Parent Element Properties
        var parentElement = baseShip.MapObjectDB.GetElement(parentObject);
        if (parentElement == null)
            return;

        // Check if parent is a util-layer
        if (parentElement.type != "util-layer")
            return;

        // Set Parent
        elemObject.transform.SetParent(parentObject.transform);
    }
}