using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

/// <summary>
/// Routes the building of map elements through a stack of IElemBuilders
/// </summary>
/// <param name="buildStack">The stack of builders to use when building elements</param>
public class BuildRouter(IElemBuilder[] buildStack)
{
    /// Time to warn the user (in ms) when an element is taking too long to load
    private const int WARN_MAX_BUILD_DURATION = 200;
    
    private static MapObjectDB MapObjectDB => LIBaseShip.Instance!.MapObjectDB;
    
    private readonly Stopwatch _buildTimer = new();
    
    /// <summary>
    /// Builds the provided LIElements into GameObjects under the specified parent transform.
    /// </summary>
    /// <param name="elements">Elements to build</param>
    /// <param name="parentTransform">Parent transform for the built GameObjects</param>
    public void BuildMap(LIElement[] elements, Transform? parentTransform = null)
    {
        // Check for LIBaseShip instance
        if (LIBaseShip.Instance == null)
            throw new Exception("LIBaseShip instance not found!");
        
        // Create GameObjects
        foreach (var element in elements)
        {
            var gameObject = CreateGameObject(element, parentTransform);
            MapObjectDB.AddObject(element, gameObject);
        }
        
        // Apply Hierarchy
        // Only AFTER all GameObjects are created
        foreach (var element in elements)
            ApplyGameObjectHierarchy(element);
        
        // Find all unique priority values (and sort them)
        var priorityValues = new SortedSet<int>();
        foreach (var builder in buildStack)
            priorityValues.Add(builder.Priority);
        
        // Group builders by priority
        // Builders with equal priority are grouped together
        var groupedBuildersByPriority = priorityValues
            .Reverse()      // <-- Higher priority first
            .Select(buildPriority => buildStack
            .Where(b => b.Priority == buildPriority)
            .ToArray());

        // Run Pre-Build hooks
        foreach (var builder in buildStack)
            builder.OnPreBuild();
        
        // Build elements by priority
        foreach (var builderGroup in groupedBuildersByPriority)
            foreach (var element in elements)
                BuildElement(builderGroup, element);
        
        // Run Post-Build hooks
        foreach (var builder in buildStack)
            builder.OnPostBuild();
    }
    
    /// <summary>
    /// Builds a single LIElement using the provided stack of IElemBuilders.
    /// </summary>
    /// <param name="targetStack">The stack of builders to use for building the element</param>
    /// <param name="element">The LIElement to be built</param>
    private void BuildElement(IElemBuilder[] targetStack, LIElement element)
    {
        try
        {
            // Start debug timer
            _buildTimer.Restart();
            
            // Check GameObject
            var gameObject = MapObjectDB.GetObject(element.id);
            if (gameObject == null)
                throw new Exception("GameObject is null");

            // Run through build stack
            foreach (var builder in targetStack)
                builder.OnBuild(element, gameObject);

            // Stop debug timer
            _buildTimer.Stop();
            if (_buildTimer.ElapsedMilliseconds > WARN_MAX_BUILD_DURATION)
                LILogger.Warn($"{element} took {_buildTimer.ElapsedMilliseconds}ms to build");
        }
        catch (Exception ex)
        {
            LILogger.Warn($"Error building {element}: {ex}");
            LILogger.LogException(ex);
        }
    }

    /// <summary>
    /// Creates a GameObject for the given LIElement and sets its parent transform.
    /// </summary>
    /// <param name="element">Element to create GameObject for</param>
    /// <param name="parentTransform">Parent transform to set for the new GameObject</param>
    /// <returns>The created GameObject</returns>
    private GameObject CreateGameObject(LIElement element, Transform? parentTransform = null)
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
    /// Sets the parent-child relationships of GameObjects based on the properties of their corresponding LIElements.
    /// Requires all GameObjects in the map to be created beforehand.
    /// </summary>
    /// <param name="element">Element to apply hierarchy to</param>
    private void ApplyGameObjectHierarchy(LIElement element)
    {
        // Get Element Properties
        var elemObject = MapObjectDB.GetObject(element.id);
        if (elemObject == null)
            return;

        // Get Parent ID
        var parent = element.parentID;
        if (parent == null)
            return;

        // Find Parent Object
        var parentObject = MapObjectDB.GetObject((Guid)parent);
        if (parentObject == null)
            return;

        // Get Parent Element Properties
        var parentElement = MapObjectDB.GetElement(parentObject);
        if (parentElement == null)
            return;

        // Check if parent is a util-layer
        if (parentElement.type != "util-layer")
            return;

        // Set Parent
        elemObject.transform.SetParent(parentObject.transform);
    }
}