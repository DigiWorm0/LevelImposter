using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LevelImposter.Build.Attributes;
using LevelImposter.Core.Utils;

namespace LevelImposter.Build.Utils;

public static class BuilderRegistry
{
    public static List<Builder> AllBuildMethods { get; } = [];
    public static List<BuilderGroup> BuildMethodsByPriority { get; } = [];

    public static void RegisterAll()
    {
        AllBuildMethods.Clear();
        BuildMethodsByPriority.Clear();

        var buildMethodEnum = EnumerateAllBuilders();
        foreach (var buildMethod in buildMethodEnum)
            AllBuildMethods.Add(buildMethod);

        var buildMethodGroupEnum = EnumerateBuildersByPriority();
        foreach (var buildMethodGroup in buildMethodGroupEnum)
            BuildMethodsByPriority.Add(buildMethodGroup);


        LILogger.Info($"Registered {AllBuildMethods.Count} build methods");
    }

    private static IEnumerable<BuilderGroup> EnumerateBuildersByPriority()
    {
        // Find all unique priority values (and sort them)
        var priorityValues = new SortedSet<int>();
        foreach (var builder in AllBuildMethods)
            priorityValues.Add(builder.Attribute.Priority);

        // Group builders by priority
        // Builders with equal priority are grouped together
        var groupedBuildersByPriority = priorityValues
            .Reverse() // <-- Higher priority first
            .Select(buildPriority => new BuilderGroup(
                AllBuildMethods
                    .Where(b => b.Attribute.Priority == buildPriority)
                    .ToArray()
            ));

        return groupedBuildersByPriority;
    }

    /// <summary>
    ///     Gets all build methods in the assembly that have the MapBuilderAttribute applied to them.
    /// </summary>
    /// <returns>An enumerable of BuildMethod instances</returns>
    private static IEnumerable<Builder> EnumerateAllBuilders()
    {
        var methods = Assembly
            .GetExecutingAssembly() // TODO: Search other plugin assemblies
            .GetTypes()
            .SelectMany(type => type.GetMethods(
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Static
            ));

        foreach (var method in methods)
        {
            var builderAttribute = method.GetCustomAttribute<MapBuilderAttribute>();
            if (builderAttribute != null)
                yield return new Builder(method, builderAttribute);
        }
    }
}