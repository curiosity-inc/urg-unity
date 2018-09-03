using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Urg
{
    public class AffineConverter
    {
        private Vector2[] sensorCorners;
        private Vector3[] screenCorners;
        private Vector3 screenBasis1;
        private Vector3 screenBasis2;
        private Vector3 screenBasis3;
        private Vector2 sensorBasis1;
        private Vector2 sensorBasis2;
        private Vector2 sensorBasis3;
        private Vector2 perpendicular1;
        private Vector2 perpendicular2;
        private Vector2 perpendicular3;

        public AffineConverter(Vector2[] sensorCorners, Vector3[] worldCorners)
        {
            this.screenCorners = worldCorners;
            this.sensorCorners = sensorCorners;

            screenBasis1 = worldCorners[0] - worldCorners[3];
            screenBasis2 = worldCorners[1] - worldCorners[3];
            screenBasis3 = worldCorners[2] - worldCorners[3];

            sensorBasis1 = sensorCorners[0] - sensorCorners[3];
            sensorBasis2 = sensorCorners[1] - sensorCorners[3];
            sensorBasis3 = sensorCorners[2] - sensorCorners[3];

            perpendicular1 = new Vector2(sensorBasis1.y, -sensorBasis1.x);
            perpendicular2 = new Vector2(sensorBasis2.y, -sensorBasis2.x);
            perpendicular3 = new Vector2(sensorBasis3.y, -sensorBasis3.x);
        }

        public bool Sensor2WorldPosition(Vector2 sensorPos, out Vector3 worldPos)
        {
            if (sensorPos.x <= 0)
            {
                worldPos = Vector3.zero;
                return false;
            }

            sensorPos -= sensorCorners[3];

            var value1 = Vector2.Dot(sensorPos, perpendicular2) / Vector2.Dot(sensorBasis1, perpendicular2);
            var value2 = Vector2.Dot(sensorPos, perpendicular1) / Vector2.Dot(sensorBasis2, perpendicular1);

            var value3 = Vector2.Dot(sensorPos, perpendicular3) / Vector2.Dot(sensorBasis2, perpendicular3);
            var value4 = Vector2.Dot(sensorPos, perpendicular2) / Vector2.Dot(sensorBasis3, perpendicular2);

            if (value1 >= 0 && value2 >= 0 && value1 + value2 <= 1)
            {
                // is in triangle composed with kitei1 and kitei2
                worldPos = value1 * screenBasis1 + value2 * screenBasis2 + screenCorners[3];
                return true;
            }
            else if (value3 >= 0 && value4 >= 0 && value3 + value4 <= 1)
            {
                // is in triangle composed with kitei2 and kitei3
                worldPos = value3 * screenBasis2 + value4 * screenBasis3 + screenCorners[3];
                return true;
            }
            worldPos = Vector3.zero;
            return false;
        }
    }
}
