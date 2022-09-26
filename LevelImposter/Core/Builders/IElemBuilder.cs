using UnityEngine;

namespace LevelImposter.Core
{
    public interface IElemBuilder
    {
        public void Build(LIElement elem, GameObject obj);
        public void PostBuild();
    }
}