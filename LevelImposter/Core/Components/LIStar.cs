using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    public class LIStar : MonoBehaviour
    {
        public LIStar(IntPtr intPtr) : base(intPtr)
        {
        }

        private float height = 10;
        private float length = 10;
        private float minSpeed = 2;
        private float maxSpeed = 2;
        private float speed = 0;

        public void Init(LIElement elem)
        {
            if (elem.properties.starfieldHeight != null)
                height = (float)elem.properties.starfieldHeight;
            if (elem.properties.starfieldLength != null)
                length = (float)elem.properties.starfieldLength;
            if (elem.properties.starfieldMinSpeed != null)
                minSpeed = (float)elem.properties.starfieldMinSpeed;
            if (elem.properties.starfieldMaxSpeed != null)
                maxSpeed = (float)elem.properties.starfieldMaxSpeed;
            Respawn(true);
        }

        public void Respawn(bool isInitial)
        {
            speed = UnityEngine.Random.Range(minSpeed, maxSpeed);
            transform.localPosition = new Vector3(
                isInitial ? UnityEngine.Random.Range(-length, 0) : 0,
                UnityEngine.Random.Range(-height / 2, height / 2),
                0
            );
        }

        public void Update()
        {
            transform.localPosition -= new Vector3(
                speed * Time.deltaTime,
                0,
                0
            );
            if (transform.localPosition.x < -length)
                Respawn(false);
        }
    }
}
