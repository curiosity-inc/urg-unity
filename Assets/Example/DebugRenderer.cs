using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Urg
{
    public class DebugRenderer : MonoBehaviour
    {
        public UrgSensor urg;
        public Vector2[] sensorCorners;

        private float[] distances;
        private readonly float MAX_DISTANCE = 3.0f;
        private readonly float MIN_DISTANCE = 0.001f;
        private ClusteringFilter clusteringFilter;
        private AffineFilter affineFilter;

        void Awake()
        {
            urg.OnDistanceReceived += Urg_OnDistanceReceived;
            clusteringFilter = new ClusteringFilter(urg, 0.2f);
            affineFilter = new AffineFilter(sensorCorners, Camera.main, new Plane(Vector3.up, Vector3.zero));
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
                        var dir = new Vector3(cos, 0, sin);
                        var pos = distance * dir;

                        Debug.DrawRay(urg.transform.position, pos, Color.blue);
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (clusteringFilter != null)
            {
                var clustered = clusteringFilter.Fetch();
                //Debug.Log(clustered.Count);
                var region = new Rect(0.6f, -0.7f, 1.4f, 1.4f);
                //Gizmos.DrawLine(region.)

                foreach (var location in clustered)
                {
                    var pos = location.ToPosition();
                    var loc2d = new Vector2(pos.x, pos.z);
                    Vector3 worldPos;
                    var inRegion = affineFilter.Sensor2WorldPosition(loc2d, out worldPos);
                    if (inRegion)
                    {
                        Gizmos.DrawCube(worldPos, new Vector3(0.1f, 0.1f, 0.1f));
                    }
                }
            }
        }

        private void Urg_OnDistanceReceived(float[] distances)
        {
            //this.distances = distances;
            this.distances = Utils.MedianFilter<float>(distances);
        }
    }
}
