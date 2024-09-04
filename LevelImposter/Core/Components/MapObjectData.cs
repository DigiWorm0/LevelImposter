using System;
using Il2CppInterop.Runtime.Attributes;
using UnityEngine;

namespace LevelImposter.Core;

public class MapObjectData(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    [HideFromIl2Cpp] public LIElement Element { get; private set; } = new();

    [HideFromIl2Cpp] public Guid ID => Element.id;
    [HideFromIl2Cpp] public LIProperties Properties => Element.properties;

    [HideFromIl2Cpp]
    public void SetSourceElement(LIElement sourceElement)
    {
        Element = sourceElement;
    }
}