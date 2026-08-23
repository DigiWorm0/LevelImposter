using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LevelImposter.Build.Attributes;
using LevelImposter.Core.Utils;

namespace LevelImposter.Builders;

public static class BuildMethodRegistry
{
    public static List<BuildMethod> AllBuildMethods { get; } = [];

    public static void RegisterAll()
    {
        AllBuildMethods.Clear();

        var buildMethodEnum = EnumerateBuildMethods();
        foreach (var buildMethod in buildMethodEnum)
            AllBuildMethods.Add(buildMethod);

        LILogger.Info($"Registered {AllBuildMethods.Count} build methods");
    }

    public static IEnumerable<BuildMethod[]> GroupBuildersByPriority()
    {
        // Find all unique priority values (and sort them)
        var priorityValues = new SortedSet<int>();
        foreach (var builder in AllBuildMethods)
            priorityValues.Add(builder.Attribute.Priority);

        // Group builders by priority
        // Builders with equal priority are grouped together
        var groupedBuildersByPriority = priorityValues
            .Reverse() // <-- Higher priority first
            .Select(buildPriority => AllBuildMethods
                .Where(b => b.Attribute.Priority == buildPriority)
                .ToArray());

        return groupedBuildersByPriority;
    }

    /// <summary>
    ///     Gets all build methods in the assembly that have the MapBuilderAttribute applied to them.
    /// </summary>
    /// <returns>An enumerable of BuildMethod instances</returns>
    private static IEnumerable<BuildMethod> EnumerateBuildMethods()
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
                yield return new BuildMethod(method, builderAttribute);
        }
    }
}