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

        private float height = 0.2f;
        private float speed = 2.0f;
        private float t = 0;
        private float yOffset = 0;

        public void Init(LIElement elem)
        {
            if (elem.properties.floatingHeight != null)
                height = (float)elem.properties.floatingHeight;
            if (elem.properties.floatingSpeed != null)
                speed = (float)elem.properties.floatingSpeed;
            yOffset = elem.y - LIShipStatus.Y_OFFSET;
        }

        public void Update()
        {
            t += Time.deltaTime;
            transform.localPosition = new Vector3(
                transform.localPosition.x,
                (Mathf.Sin(t * speed) + 1) * height / 2 + yOffset,
                transform.localPosition.z
            );
        }
    }
}
