using UnityEngine;

namespace LevelImposter.Core
{
    /// <summary>
    /// Constructs a specific subset of map elements or element features. 
    /// Stored in the build stack located in <c>BuildRouter._buildStack</c>.
    /// </summary>
    public interface IElemBuilder
    {
        /// <summary>
        /// Parses and builds a GameObject based on <c>LIElement</c> data.
        /// </summary>
        /// <param name="elem">Element to be parsed and built</param>
        /// <param name="obj">GameObject to append data and components to</param>
        public void Build(LIElement elem, GameObject obj);

        /// <summary>
        /// Final clean-up after all elements in a map have been built.
        /// </summary>
        public void PostBuild();
    }
}