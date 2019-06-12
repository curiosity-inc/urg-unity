using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Urg
{
    public class DebugRenderer : MonoBehaviour
    {
        public UrgSensor urg;

        private float[] distances;
        private List<DetectedLocation> locations = new List<DetectedLocation>();
        private AffineConverter affineConverter;
        private List<GameObject> debugObjects;
        private Object syncLock = new Object();
        private System.Diagnostics.Stopwatch stopwatch;

        void Awake()
        {
            stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            // delegate method to be triggered when the new data is received from sensor.
            urg.OnDistanceReceived += Urg_OnDistanceReceived;

            // uncomment if you need some filters before clustering
            //urg.AddFilter(new SpatialMedianFilter(3));
            urg.SetClusterExtraction(new EuclidianClusterExtraction());

            var cam = Camera.main;
            var plane = new Plane(Vector3.up, Vector3.zero);

            var sensorCorners = new Vector2[4];
            sensorCorners[0] = new Vector2(1.5f, 0.5f);
            sensorCorners[1] = new Vector2(1.5f, -0.5f);
            sensorCorners[2] = new Vector2(0.5f, -0.5f);
            sensorCorners[3] = new Vector2(0.5f, 0.5f);

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

            var locs = this.locations;
            int index = 0;
            foreach (var loc in locs)
            {
                Vector3 worldPos = new Vector3(0, 0, 0);
                var inRegion = affineConverter.Sensor2WorldPosition(loc.ToPosition2D(), out worldPos);
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
            this.distances = data.RawDistances;
            this.locations = data.FilteredResults;
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
