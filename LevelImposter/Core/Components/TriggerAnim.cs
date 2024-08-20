﻿using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using LibCpp2IL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LevelImposter.Core
{
    public class TriggerAnim : MonoBehaviour
    {
        public TriggerAnim(IntPtr intPtr) : base(intPtr)
        {
        }

        LIAnimTarget[] _animTargets = Array.Empty<LIAnimTarget>();
        Dictionary<Guid, GameObject> _objectDB = new Dictionary<Guid, GameObject>();
        Coroutine? _currentAnimation = null;
        bool _loop = false;
        float _duration = 0;
        float _t = 0;

        /// <summary>
        /// Initializes the component
        /// </summary>
        private void Init()
        {
            // Get Object Data
            MapObjectData? elementData = gameObject.GetLIData();
            if (elementData == null)
                throw new Exception("No LIElement data found on object");

            // Get Animation Data
            _animTargets = elementData.Properties.animTargets ?? Array.Empty<LIAnimTarget>();
            _loop = elementData.Properties.triggerLoop ?? false;

            // Sort All Keyframes by Time
            // Also calculate the duration of the animation
            foreach (LIAnimTarget target in _animTargets)
            {
                foreach (LIAnimProperty property in target.properties.Values)
                {
                    if (property.keyframes == null || property.keyframes.Length == 0)
                        continue;

                    Array.Sort(property.keyframes, (a, b) => a.t.CompareTo(b.t));
                    _duration = Mathf.Max(_duration, property.keyframes.Last().t);
                }
            }
        }

        public void Play()
        {
            // Do nothing if already playing
            if (_currentAnimation != null)
                return;

            // Start Animation
            _currentAnimation = StartCoroutine(CoAnimate().WrapToIl2Cpp());
        }

        public void Pause()
        {
            // Do nothing if not playing
            if (_currentAnimation == null)
                return;

            // Stop Animation Coroutine
            StopCoroutine(_currentAnimation);
            _currentAnimation = null;
        }

        public void Stop()
        {
            // Stop Animation Coroutine
            Pause();

            // Reset T
            _t = 0;

            // Reset Targets
            foreach (GameObject targetObject in _objectDB.Values)
            {
                targetObject.transform.localPosition = Vector3.zero;
                targetObject.transform.localScale = Vector3.one;
                targetObject.transform.localRotation = Quaternion.identity;
            }
        }

        private GameObject GetAnimContainer(Guid id)
        {
            // Get Object from Cache
            GameObject? targetObject = _objectDB.GetOrDefault(id);
            if (targetObject != null)
                return targetObject;

            // Get Object from Ship Status
            targetObject = LIShipStatus.GetInstance().MapObjectDB.GetObject(id);
            if (targetObject == null)
                throw new Exception($"Could not find object with ID {id}");

            // Create Container
            GameObject containerObject = new GameObject($"AnimTarget_{id}");

            // Match Transform
            containerObject.transform.SetParent(targetObject.transform.parent);
            containerObject.transform.position = targetObject.transform.position;
            containerObject.transform.rotation = targetObject.transform.rotation;
            containerObject.transform.localScale = targetObject.transform.localScale;

            // Set Object as Child
            targetObject.transform.SetParent(containerObject.transform);
            targetObject.transform.localPosition = Vector3.zero;
            targetObject.transform.localRotation = Quaternion.identity;
            targetObject.transform.localScale = Vector3.one;

            // Cache Object
            _objectDB[id] = targetObject;

            return targetObject;
        }

        [HideFromIl2Cpp]
        private float? GetPropertyValue(LIAnimTarget target, string property)
        {
            LIAnimProperty? propertyData = target.properties.GetOrDefault(property);

            // Check for Null
            if (propertyData == null)
                return null;

            // Get Keyframes
            LIAnimKeyframe[] keyframes = propertyData.keyframes ?? Array.Empty<LIAnimKeyframe>();

            // Get Adjacent Keyframes
            LIAnimKeyframe? prevKeyframe = null;
            LIAnimKeyframe? nextKeyframe = null;
            foreach (LIAnimKeyframe keyframe in keyframes)
            {
                if (keyframe.t <= _t)
                    prevKeyframe = keyframe;
                else if (keyframe.t > _t && nextKeyframe == null)
                    nextKeyframe = keyframe;
            }

            // Check for singular keyframe
            if (prevKeyframe == null)
                return nextKeyframe?.value;
            if (nextKeyframe == null)
                return prevKeyframe.value;

            // Interpolation Parameters
            string method = prevKeyframe.nextCurve ?? "linear";
            float t1 = prevKeyframe.t;
            float t2 = nextKeyframe.t;
            float v1 = prevKeyframe.value;
            float v2 = nextKeyframe.value;

            // Linear
            if (method == "linear")
                return Mathf.Lerp(v1, v2, (_t - t1) / (t2 - t1));
            if (method == "easeIn")
                return Mathf.Lerp(v1, v2, Mathf.Pow((_t - t1) / (t2 - t1), 2));
            if (method == "easeOut")
                return Mathf.Lerp(v1, v2, 1 - Mathf.Pow(1 - (_t - t1) / (t2 - t1), 2));
            if (method == "easeInOut")
                return Mathf.Lerp(v1, v2, Mathf.SmoothStep(0, 1, (_t - t1) / (t2 - t1)));

            // Unknown Method
            return null;
        }

        [HideFromIl2Cpp]
        private void UpdateTarget(LIAnimTarget target)
        {
            // Get Container
            GameObject targetObject = GetAnimContainer(target.id);

            // Get Property Values
            float x = GetPropertyValue(target, "x") ?? 0;
            float y = GetPropertyValue(target, "y") ?? 0;
            float xScale = GetPropertyValue(target, "xScale") ?? 1;
            float yScale = GetPropertyValue(target, "yScale") ?? 1;
            float rotation = GetPropertyValue(target, "rotation") ?? 0;

            // Apply Transform
            targetObject.transform.localPosition = new Vector3(x, y, targetObject.transform.localPosition.z);
            targetObject.transform.localScale = new Vector3(xScale, yScale, targetObject.transform.localScale.z);
            targetObject.transform.localRotation = Quaternion.Euler(0, 0, -rotation);
        }

        [HideFromIl2Cpp]
        private IEnumerator CoAnimate()
        {
            // TODO: Trigger OnStart/OnEnd Events

            // Get the start timestamp
            float startTimestamp = Time.time;

            // Get the start T
            float startT = _t;
            if (startT >= _duration)
                startT = 0;

            // Loop Animation
            while (true)
            {
                // Update T
                float elapsed = Time.time - startTimestamp;
                _t = startT + elapsed;

                // Break if done
                if (_t >= _duration && !_loop)
                {
                    _t = _duration;
                    _currentAnimation = null;
                    yield break;
                }

                // Loop T
                _t = _t % _duration;

                // Update Targets
                foreach (LIAnimTarget target in _animTargets)
                    UpdateTarget(target);

                yield return null;
            }
        }


        public void Awake()
        {
            Init();
        }

#pragma warning disable CS8625
        public void OnDestroy()
        {
            _animTargets = null;
            _objectDB = null;
            _currentAnimation = null;
        }
#pragma warning restore CS8625
    }
}
