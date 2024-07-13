using LevelImposter.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LevelImposter.Trigger
{
    public class AnimTriggerHandle : ITriggerHandle
    {
        private GameObjectCoroutineManager _animManager = new();

        public void OnTrigger(TriggerSignal signal)
        {
            if (signal.TriggerID != "playAnim" &&
                signal.TriggerID != "stopAnim" &&
                signal.TriggerID != "pauseAnim")
                return;

            // Get the object data
            var objectData = signal.TargetObject.GetLIData();

            // TODO: Implement Me!
        }

        public IEnumerator CoAnimateElement()
        {
            yield return null;

            // TODO: Implement Me!
        }
    }
}
