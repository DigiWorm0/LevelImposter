using System;

namespace LevelImposter.Build.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class ElementBuilderAttribute : MapBuilderAttribute
{
    public ElementBuilderAttribute()
    {
        Type = BuilderType.ElementBuilder;
    }
}