using Il2CppInterop.Runtime.Attributes;
using System;
using UnityEngine;

namespace LevelImposter.Core
{
    public class MapObjectData : MonoBehaviour
    {
        public MapObjectData(IntPtr intPtr) : base(intPtr)
        {
        }

        private LIElement _sourceElement = new();

        [HideFromIl2Cpp] public LIElement Element => _sourceElement;
        [HideFromIl2Cpp] public Guid ID => _sourceElement.id;
        [HideFromIl2Cpp] public LIProperties Properties => _sourceElement.properties;

        [HideFromIl2Cpp]
        public void SetSourceElement(LIElement sourceElement)
        {
            _sourceElement = sourceElement;
        }

        public void OnDestory()
        {
            _sourceElement = null;
        }
    }
}
