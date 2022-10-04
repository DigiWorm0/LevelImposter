using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    public class LIFloat : MonoBehaviour
    {
        public LIFloat(IntPtr intPtr) : base(intPtr)
        {
        }

        private float _height = 0.2f;
        private float _speed = 2.0f;
        private float _t = 0;
        private float _yOffset = 0;

        public void Init(LIElement elem)
        {
            if (elem.properties.floatingHeight != null)
                _height = (float)elem.properties.floatingHeight;
            if (elem.properties.floatingSpeed != null)
                _speed = (float)elem.properties.floatingSpeed;
            _yOffset = elem.y - LIShipStatus.Y_OFFSET;
        }

        public void Update()
        {
            _t += Time.deltaTime;
            transform.localPosition = new Vector3(
                transform.localPosition.x,
                (Mathf.Sin(_t * _speed) + 1) * _height / 2 + _yOffset,
                transform.localPosition.z
            );
        }
    }
}
