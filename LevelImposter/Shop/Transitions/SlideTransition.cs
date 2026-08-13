using System.Collections;
using UnityEngine;

namespace LevelImposter.Shop.Transitions;

public static class SlideTransition
{
    public static IEnumerator Run(TransitionParams<Vector3> transitionParams)
    {
        return TransitionHelper.RunTransition(transitionParams, SetPosition);
    }

    private static void SetPosition(GameObject gameObject, Vector3 position)
    {
        if (gameObject == null)
            return;
        gameObject.transform.localPosition = position;
    }
}