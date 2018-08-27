using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Urg
{
    public class DetectedLocation
    {
        public float angle;
        public float distance;

        public DetectedLocation(float angle, float distance)
        {
            this.angle = angle;
            this.distance = distance;
        }

        public Vector3 ToPosition()
        {
            return ToPosition(Vector3.right, Vector3.up);
        }

        public Vector3 ToPosition(Vector3 forward, Vector3 normal)
        {
            return distance * (Quaternion.AngleAxis(-angle, normal) * forward);
        }
    }
}
