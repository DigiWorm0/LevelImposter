using UnityEngine;

namespace LevelImposter.Shop.Transitions;

public enum TransitionCurve
{
    Linear,
    EaseIn,
    EaseOut,
    EaseInOut
}

public struct TransitionParams<T>
{
    public GameObject TargetObject;
    public T FromValue;
    public T ToValue;
    public float Duration;
    public float StartDelay;
    public TransitionCurve? Curve;
    public System.Action? OnComplete;
}