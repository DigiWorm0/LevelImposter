using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

/// <summary>
///     Constructs a specific subset of map elements or element features.
///     Stored in the build stack located in <c>BuildRouter._buildStack</c>.
/// </summary>
public interface IElemBuilder
{
    /// <summary>
    ///     First step in the build process, called before <see cref="OnBuild"/> for each element in the map.
    /// </summary>
    /// <param name="elem">Element to be parsed and built</param>
    /// <param name="obj">GameObject to append data and components to</param>
    public void OnPreBuild(LIElement elem, GameObject obj)
    {
    }

    /// <summary>
    ///     Primary step in the build process, called for each element in the map.
    /// </summary>
    /// <param name="elem">Element to be parsed and built</param>
    /// <param name="obj">GameObject to append data and components to</param>
    public void OnBuild(LIElement elem, GameObject obj)
    {
    }

    /// <summary>
    ///     Third step in the build process, called after <see cref="OnBuild"/> for each element in the map.
    /// </summary>
    /// <param name="elem">Element to be parsed and built</param>
    /// <param name="obj">GameObject to append data and components to</param>
    public void OnPostBuild(LIElement elem, GameObject obj)
    {
    }

    /// <summary>
    ///     Final clean-up after all elements in a map have been built.
    ///     Only called once per map build.
    /// </summary>
    public void OnCleanup()
    {
    }
}