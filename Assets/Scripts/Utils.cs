using System;
using UnityEngine;

namespace Urg
{
    public class Utils
    {
        public static T[] MedianFilter<T>(T[] array) where T : IComparable, IComparable<T>
        {
            var res = new T[array.Length];
            for (var i = 1; i < array.Length - 1; i++)
            {
                if (array[i].CompareTo(array[i - 1]) >= 0 && array[i].CompareTo(array[i + 1]) <= 0)
                {
                    res[i] = array[i];
                }
                else if (array[i - 1].CompareTo(array[i]) >= 0 && array[i - 1].CompareTo(array[i + 1]) <= 0)
                {
                    res[i] = array[i - 1];
                }
                else if (array[i + 1].CompareTo(array[i]) >= 0 && array[i + 1].CompareTo(array[i - 1]) <= 0)
                {
                    res[i] = array[i + 1];
                }
                else
                {
                    res[i] = array[i];
                }
            }
            return res;
        }

        public static Vector3 Screen2WorldPosition(Vector2 screenPosition, Camera camera, Plane basePlane)
        {
            var ray = camera.ScreenPointToRay(screenPosition);
            var distance = 0f;

            if (basePlane.Raycast(ray, out distance))
            {
                var p = ray.GetPoint(distance);
                return p;
            }
            return Vector3.negativeInfinity;
        }
    }
}
