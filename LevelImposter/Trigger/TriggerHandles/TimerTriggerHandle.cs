using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Core;
using Reactor.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LevelImposter.Trigger
{
    public class TimerTriggerHandle : ITriggerHandle
    {
        private Dictionary<Guid, IEnumerator> _timerCoroutines = new();

        public void OnTrigger(GameObject gameObject, string triggerID, PlayerControl? orgin, int stackSize = 0)
        {
            if (triggerID != "startTimer" && triggerID != "stopTimer")
                return;

            // Get the object data
            var objectData = gameObject.GetComponent<MapObjectData>();
            if (objectData == null)
            {
                LILogger.Warn($"{gameObject} is missing LI data");
                return;
            }

            // Start timer
            if (triggerID == "startTimer")
            {
                Coroutines.Stop(_timerCoroutines.GetValueOrDefault(objectData.ID));
                var timerCoroutine = Coroutines.Start(CoTimerTrigger(gameObject, objectData, orgin, stackSize));
                _timerCoroutines[objectData.ID] = timerCoroutine;
            }
            // Stop timer
            else if (triggerID == "stopTimer")
            {
                Coroutines.Stop(_timerCoroutines.GetValueOrDefault(objectData.ID));
                _timerCoroutines.Remove(objectData.ID);
            }
        }


        /// <summary>
        /// Coroutine to run timer trigger. Fires onStart on the start and onFinish on completion.
        /// </summary>
        /// <param name="duration">Duration of the timer in seconds</param>
        [HideFromIl2Cpp]
        private IEnumerator CoTimerTrigger(GameObject gameObject, MapObjectData objectData, PlayerControl? orgin, int stackSize = 0)
        {
            // Get timer properties
            float duration = objectData.Properties.triggerTime ?? 1;
            bool isLoop = objectData.Properties.triggerLoop ?? false;

            // Loop Timer
            do
            {
                TriggerSystem.Trigger(gameObject, "onStart", null, stackSize);
                yield return new WaitForSeconds(duration);
                TriggerSystem.Trigger(gameObject, "onFinish", null, stackSize);
            }
            while (isLoop);

            // Remove timer from list
            _timerCoroutines.Remove(objectData.ID);
        }

    }
}
