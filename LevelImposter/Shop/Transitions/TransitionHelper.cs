using System;
using System.Collections;
using LevelImposter.Shop.Transitions;
using Reactor.Utilities;
using UnityEngine;

namespace LevelImposter.Shop;

public static class TransitionHelper
{
    /// <summary>
    /// Runs a transition on the target object
    /// </summary>
    /// <param name="transitionParams">A struct containing all parameters for the transition</param>
    /// <param name="applyToTarget">A method that applies the value to the target object</param>
    /// <typeparam name="T">The type of value being transitioned (e.g. float, Vector3)</typeparam>
    public static void RunTransition<T>(
        TransitionParams<T> transitionParams,
        Action<GameObject, T> applyToTarget)
    {
        Coroutines.Start(CoRunTransition(transitionParams, applyToTarget));
    }
    
    private static IEnumerator CoRunTransition<T>(
        TransitionParams<T> transitionParams,
        Action<GameObject, T> applyToTarget)
    {
        
        // Apply initial opacity
        applyToTarget(transitionParams.TargetObject, transitionParams.FromValue);
        
        // Initial delay
        if (transitionParams.StartDelay > 0.0f)
            yield return new WaitForSeconds(transitionParams.StartDelay);
        
        // Fade over time
        var t = 0.0f;
        while (t < transitionParams.Duration)
        {
            // Calculate value
            var value = LerpTransition(transitionParams, t / transitionParams.Duration);
            applyToTarget(transitionParams.TargetObject, value);

            // Wait a frame
            yield return null;
            t += Time.deltaTime;
        }
        
        // Ensure final value
        applyToTarget(transitionParams.TargetObject, transitionParams.ToValue);
        transitionParams.OnComplete?.Invoke();
    }
    
    private static T LerpTransition<T>(TransitionParams<T> transitionParams, float t)
    {
        var easedT = GetEasedT(transitionParams.Curve, t);
        return transitionParams switch
        {
            TransitionParams<float> floatParams => (T)(object)Mathf.Lerp(floatParams.FromValue, floatParams.ToValue, easedT),
            TransitionParams<Vector3> vector3Params => (T)(object)Vector3.Lerp(vector3Params.FromValue, vector3Params.ToValue, easedT),
            _ => throw new NotImplementedException($"Transition type {typeof(T)} not implemented")
        };
    }
    
    private static float GetEasedT(TransitionCurve? curve, float t)
    {
        if (curve == null)
            return t;
        
        return curve switch
        {
            TransitionCurve.Linear => t,
            TransitionCurve.EaseIn => t * t,
            TransitionCurve.EaseOut => t * (2 - t),
            TransitionCurve.EaseInOut => t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t,
            _ => t
        };
    }
}