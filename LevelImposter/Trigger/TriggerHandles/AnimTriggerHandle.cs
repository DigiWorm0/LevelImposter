using LevelImposter.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LevelImposter.Trigger
{
    public class AnimTriggerHandle : ITriggerHandle
    {
        private Dictionary<Guid, IEnumerator> _animCoroutines = new();

        public void OnTrigger(GameObject gameObject, string triggerID, PlayerControl? orgin, int stackSize = 0)
        {
            if (triggerID != "playAnim" &&
                triggerID != "stopAnim" &&
                triggerID != "pauseAnim")
                return;

            // Get the object data
            var objectData = gameObject.GetComponent<MapObjectData>();
            if (objectData == null)
            {
                LILogger.Warn($"{gameObject} is missing LI data");
                return;
            }

            // TODO: Implement Me!
        }

        public IEnumerator CoAnimateElement()
        {
            yield return null;

            // TODO: Implement Me!
        }
    }
}
