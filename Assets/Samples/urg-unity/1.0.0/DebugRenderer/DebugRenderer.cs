using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Urg
{
    public class DebugRenderer : MonoBehaviour
    {
        public UrgSensor urg;

        private float[] rawDistances;
        private List<DetectedLocation> locations = new List<DetectedLocation>();
        private List<List<int>> clusterIndices;
        private AffineConverter affineConverter;
        private List<GameObject> debugObjects;
        private Object syncLock = new Object();
        private System.Diagnostics.Stopwatch stopwatch;
        EuclidianClusterExtraction cluster;

        void Awake()
        {
            stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            // delegate method to be triggered when the new data is received from sensor.
            urg.OnDistanceReceived += Urg_OnDistanceReceived;

            // uncomment if you need some filters before clustering
            urg.AddFilter(new TemporalMedianFilter(3));
            urg.AddFilter(new SpatialMedianFilter(3));
            urg.AddFilter(new DistanceFilter(2.25f));
            urg.SetClusterExtraction(new EuclidianClusterExtraction(0.1f));
            cluster = new EuclidianClusterExtraction(0.1f);

            var cam = Camera.main;
            var plane = new Plane(Vector3.up, Vector3.zero);

            var sensorCorners = new Vector2[4];
            sensorCorners[0] = new Vector2(1.5f, 1f);
            sensorCorners[1] = new Vector2(1.5f, -1f);
            sensorCorners[2] = new Vector2(0.2f, -1f);
            sensorCorners[3] = new Vector2(0.2f, 1f);

            var worldCorners = new Vector3[4];
            worldCorners[0] = Screen2WorldPosition(new Vector2(0, Screen.height), cam, plane);
            worldCorners[1] = Screen2WorldPosition(new Vector2(Screen.width, Screen.height), cam, plane);
            worldCorners[2] = Screen2WorldPosition(new Vector2(Screen.width, 0), cam, plane);
            worldCorners[3] = Screen2WorldPosition(new Vector2(0, 0), cam, plane);
            affineConverter = new AffineConverter(sensorCorners, worldCorners);

            debugObjects = new List<GameObject>();
            for (var i = 0; i < 100; i++)
            {
                var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obj.transform.parent = transform;
                obj.transform.localScale = 0.3f * Vector3.one;
                debugObjects.Add(obj);
            }
        }

        void Update()
        {
            if (urg == null)
            {
                return;
            }

            if (rawDistances != null && rawDistances.Length > 0)
            {
                for (int i = 0; i < rawDistances.Length; i++)
                {
                    float distance = rawDistances[i];
                    float angle = urg.StepAngleRadians * i + urg.OffsetRadians;
                    var cos = Mathf.Cos(angle);
                    var sin = Mathf.Sin(angle);
                    var dir = new Vector3(cos, 0, sin);
                    var pos = distance * dir;

                    Debug.DrawRay(urg.transform.position, pos, Color.blue);
                }
            }

            if (locations == null)
            {
                return;
            }

            clusterIndices = cluster.ExtractClusters(locations);

            var locs = this.locations;
            int index = 0;
            for (var i = 0; i < clusterIndices.Count; i++)
            {
                if (clusterIndices[i].Count < 2)
                {
                    continue;
                }
                //Debug.Log(clusterIndices[i].Count);
                Vector2 center = Vector2.zero;
                foreach (var j in clusterIndices[i])
                {
                    center += locations[j].ToPosition2D();
                }
                center /= (float)clusterIndices[i].Count;

                Vector3 worldPos = new Vector3(0, 0, 0);
                var inRegion = affineConverter.Sensor2WorldPosition(center, out worldPos);
                if (inRegion && index < debugObjects.Count)
                {
                    //Gizmos.DrawCube(worldPos, new Vector3(0.1f, 0.1f, 0.1f));
                    debugObjects[index].transform.position = worldPos;
                    index++;
                }
            }

            for (var i = index; i < debugObjects.Count; i++)
            {
                debugObjects[i].transform.position = new Vector3(100, 100, 100);
            }
        }

        void Urg_OnDistanceReceived(DistanceRecord data)
        {
            Debug.LogFormat("distance received: SCIP timestamp={0} unity timer={1}", data.Timestamp, stopwatch.ElapsedMilliseconds);
            Debug.LogFormat("cluster count: {0}", data.ClusteredIndices.Count);
            this.rawDistances = data.RawDistances;
            this.locations = data.FilteredResults;
            this.clusterIndices = data.ClusteredIndices;
        }

        private static Vector3 Screen2WorldPosition(Vector2 screenPosition, Camera camera, Plane basePlane)
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
