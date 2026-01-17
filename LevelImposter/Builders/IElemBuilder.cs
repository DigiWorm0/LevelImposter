using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

/// <summary>
///     Constructs a specific subset of map elements or element features.
///     Stored in the build stack located in <c>BuildRouter._buildStack</c>.
/// </summary>
public interface IElemBuilder
{
    protected const int LOW_PRIORITY = -100;
    protected const int HIGH_PRIORITY = 100;
    
    /// <summary>
    /// The priority of this builder.
    /// Builders with a higher priority value will execute on EVERY element
    /// before builders with a lower priority value. If this is not what you want,
    /// consider re-ordering your build stack instead.
    /// </summary>
    public int Priority => 0;

    /// <summary>
    /// Runs before any elements are built.
    /// </summary>
    public void OnPreBuild() {}

    /// <summary>
    ///     Called once before building each element.
    ///     Applies whatever components/properties
    ///     that the current builder is responsible for.
    /// </summary>
    /// <param name="elem">Map element to be parsed and built</param>
    /// <param name="obj">GameObject to append data and components to</param>
    public void OnBuild(LIElement elem, GameObject obj) {}

    /// <summary>
    /// Runs after all elements have been built.
    /// </summary>
    public void OnPostBuild() {}
}