extern alias JetBrainsAnnotations;
using System;
using JetBrainsAnnotations::JetBrains.Annotations;
using LevelImposter.Core.Models;

namespace LevelImposter.Build.Attributes;

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method)]
public class MapBuilderAttribute : Attribute
{
    public enum BuilderType
    {
        // Executes once per map
        MapBuilder,

        // Executes once per element in a map
        ElementBuilder
    }

    public BuilderType Type { get; set; } = BuilderType.MapBuilder;
    public int Priority { get; set; } = Attributes.Priority.DEFAULT;
    public MapTarget Target { get; set; } = MapTarget.Both;
    public string[]? ElementTypes { get; set; } = null;
}

/// <summary>
///     Map Builder priority levels
/// </summary>
public static class Priority
{
    /// SpriteRenderers and Colliders are added at this level
    public const int FIRST = 1000;

    // Rooms are added at this level
    public const int VERY_HIGH = 500;
    public const int HIGH = 100;
    public const int DEFAULT = 0;
    public const int LOW = -100;
    public const int VERY_LOW = -500;
    public const int LAST = -1000;
}