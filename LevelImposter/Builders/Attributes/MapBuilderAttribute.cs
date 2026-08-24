using System;
using LevelImposter.Core.Models;

namespace LevelImposter.Build.Attributes;

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
    public int Priority { get; set; } = Builders.Priority.DEFAULT;
    public MapTarget Target { get; set; } = MapTarget.Both;
    public string[]? ElementTypes { get; set; } = null;
}