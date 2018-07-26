using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Urg
{
    public class DebugRenderer : MonoBehaviour
    {
        public UrgSensor urg;
        public float clusteringDistance = 0.05f;

        private float[] distances;
        private readonly float MAX_DISTANCE = 3.0f;
        private readonly float MIN_DISTANCE = 0.001f;

        void Awake()
        {
            urg.OnDistanceReceived += Urg_OnDistanceReceived;
        }

        void Update()
        {
            if (urg != null)
            {
                if (distances != null && distances.Length > 0)
                {
                    for (int i = 0; i < distances.Length; i++)
                    {
                        float distance = distances[i];
                        float angle = urg.StepAngleRadians * i + urg.OffsetRadians;
                        var cos = Mathf.Cos(angle);
                        var sin = Mathf.Sin(angle);
                        var dir = new Vector3(cos, sin, 0);
                        var pos = distance * dir;

                        Debug.DrawRay(urg.transform.position, pos, Color.blue);
                    }
                }
            }
        }

        private void Urg_OnDistanceReceived(float[] distances)
        {
            // this.distances = distances;
            this.distances = Utils.MedianFilter<float>(distances);
        }
    }
}
