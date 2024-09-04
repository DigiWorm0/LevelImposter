using System;
using UnityEngine;

namespace LevelImposter.Core;

[Serializable]
public class LIColor
{
    public float r { get; set; }
    public float g { get; set; }
    public float b { get; set; }
    public float a { get; set; }

    public Color ToUnity()
    {
        return new Color(
            r / 255.0f,
            g / 255.0f,
            b / 255.0f,
            a
        );
    }
}