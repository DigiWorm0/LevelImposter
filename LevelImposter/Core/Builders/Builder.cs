using UnityEngine;

namespace LevelImposter.Core
{
    interface Builder
    {
        public void Build(LIElement elem, GameObject obj);
        public void PostBuild();
    }
}