using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Urg
{
    public class AffineFilter
    {
        private Vector2[] sensorCorners;
        private Camera camera;
        private Plane plane;
        private Vector3[] screenCorners;
        private Vector3 screenKitei1;
        private Vector3 screenKitei2;
        private Vector3 screenKitei3;
        private Vector2 sensorKitei1;
        private Vector2 sensorKitei2;
        private Vector2 sensorKitei3;
        private Vector2 perpendicular1;
        private Vector2 perpendicular2;
        private Vector2 perpendicular3;

        public AffineFilter(Vector2[] sensorCorners, Camera camera, Plane plane)
        {
            this.sensorCorners = sensorCorners;
            this.camera = camera;
            this.plane = plane;

            screenCorners = new Vector3[4];
            screenCorners[0] = Utils.Screen2WorldPosition(new Vector2(0, Screen.height), camera, plane);
            screenCorners[1] = Utils.Screen2WorldPosition(new Vector2(Screen.width, Screen.height), camera, plane);
            screenCorners[2] = Utils.Screen2WorldPosition(new Vector2(Screen.width, 0), camera, plane);
            screenCorners[3] = Utils.Screen2WorldPosition(new Vector2(0, 0), camera, plane);

            screenKitei1 = screenCorners[0] - screenCorners[3];
            screenKitei2 = screenCorners[1] - screenCorners[3];
            screenKitei3 = screenCorners[2] - screenCorners[3];

            sensorKitei1 = sensorCorners[0] - sensorCorners[3];
            sensorKitei2 = sensorCorners[1] - sensorCorners[3];
            sensorKitei3 = sensorCorners[2] - sensorCorners[3];

            perpendicular1 = new Vector2(sensorKitei1.y, -sensorKitei1.x);
            perpendicular2 = new Vector2(sensorKitei2.y, -sensorKitei2.x);
            perpendicular3 = new Vector2(sensorKitei3.y, -sensorKitei3.x);
        }

        public bool Sensor2WorldPosition(Vector2 sensorPos, out Vector3 worldPos)
        {
            if (sensorPos.x <= 0)
            {
                worldPos = Vector3.zero;
                return false;
            }

            sensorPos -= sensorCorners[3];

            var value1 = Vector2.Dot(sensorPos, perpendicular2) / Vector2.Dot(sensorKitei1, perpendicular2);
            var value2 = Vector2.Dot(sensorPos, perpendicular1) / Vector2.Dot(sensorKitei2, perpendicular1);

            var value3 = Vector2.Dot(sensorPos, perpendicular3) / Vector2.Dot(sensorKitei2, perpendicular3);
            var value4 = Vector2.Dot(sensorPos, perpendicular2) / Vector2.Dot(sensorKitei3, perpendicular2);

            if (value1 >= 0 && value2 >= 0 && value1 + value2 <= 1)
            {
                // is in triangle composed with kitei1 and kitei2
                worldPos = value1 * screenKitei1 + value2 * screenKitei2 + screenCorners[3];
                return true;
            }
            else if (value3 >= 0 && value4 >= 0 && value3 + value4 <= 1)
            {
                // is in triangle composed with kitei2 and kitei3
                worldPos = value3 * screenKitei2 + value4 * screenKitei3 + screenCorners[3];
                return true;
            }
            worldPos = Vector3.zero;
            return false;
        }
    }
}
