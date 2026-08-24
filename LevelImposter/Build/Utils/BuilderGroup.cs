using System.Linq;
using LevelImposter.Build.Attributes;
using LevelImposter.Core.Models;

namespace LevelImposter.Build.Utils;

public class BuilderGroup(Builder[] buildMethods)
{
    public Builder[] Slice(MapTarget mapTarget, MapBuilderAttribute.BuilderType builderType)
    {
        return buildMethods.Where(b => (b.Attribute.Type == builderType &&
                                        b.Attribute.Target == mapTarget) ||
                                       b.Attribute.Target == MapTarget.Both)
            .ToArray();
    }
}