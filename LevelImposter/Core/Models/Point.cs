using System;
using UnityEngine;

namespace LevelImposter.Core;

[Serializable]
public class Point
{
    public float x { get; set; }
    public float y { get; set; }

    public Vector2 toVector()
    {
        return new Vector2(x, y);
    }
}