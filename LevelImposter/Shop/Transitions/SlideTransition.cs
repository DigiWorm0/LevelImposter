using UnityEngine;

namespace LevelImposter.Shop.Transitions;

public static class SlideTransition
{
    public static void Run(TransitionParams<Vector3> transitionParams)
    {
        TransitionHelper.RunTransition(transitionParams, SetPosition);
    }

    private static void SetPosition(GameObject gameObject, Vector3 position)
    {
        if (gameObject == null)
            return;
        gameObject.transform.localPosition = position;
    }
}