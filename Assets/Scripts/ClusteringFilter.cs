using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Urg
{
    public class ClusteringFilter
    {
        private UrgSensor urg;
        private float[] distances;
        private int[] clusteringNumber;
        private List<DetectedLocation> detectedLocations = new List<DetectedLocation>();
        private Object thisLock = new Object();
        private bool filtered = false;
        private float minDistance;

        public ClusteringFilter(UrgSensor urg, float minDistance = 0.1f)
        {
            this.urg = urg;
            this.urg.OnDistanceReceived += Urg_OnDistanceReceived;
            this.minDistance = minDistance;
        }

        public List<DetectedLocation> Fetch()
        {
            lock (thisLock)
            {
                if (!this.filtered)
                {
                    Cluster();
                    this.filtered = true;
                }
            }
            return detectedLocations;
        }

        void Urg_OnDistanceReceived(float[] rawDistances)
        {
            lock (thisLock)
            {
                this.distances = Utils.MedianFilter<float>(rawDistances);
                this.filtered = false;
            }
        }

        void Cluster()
        {
            if (clusteringNumber == null)
            {
                clusteringNumber = new int[distances.Length];
            }
            for (int i = 0; i < clusteringNumber.Length; i++)
            {
                clusteringNumber[i] = -1;
            }
            int numOfCluster = 1;
            clusteringNumber[0] = 0;
            for (int i = 1; i < distances.Length; i++)
            {
                if (Mathf.Abs(distances[i] - distances[i - 1]) < minDistance)
                {
                    clusteringNumber[i] = clusteringNumber[i - 1];
                }
                else
                {
                    numOfCluster++;
                    clusteringNumber[i] = i;
                }
            }
            detectedLocations.Clear();
            float sumOfStep = 0;
            float sumOfDistance = distances[0];
            int numOfSequence = 1;
            for (int i = 1; i < clusteringNumber.Length; i++)
            {
                if (i == clusteringNumber[i])
                {
                    float avgStep = sumOfStep / numOfSequence;
                    float avgDistance = sumOfDistance / numOfSequence;
                    detectedLocations.Add(new DetectedLocation(urg.stepAngleDegrees * avgStep + urg.offsetDegrees, avgDistance));
                    sumOfStep = 0;
                    sumOfDistance = 0;
                    numOfSequence = 0;
                }
                sumOfStep += i;
                sumOfDistance += distances[i];
                numOfSequence++;
            }
            detectedLocations.Add(new DetectedLocation(urg.stepAngleDegrees * sumOfStep / numOfSequence + urg.offsetDegrees, sumOfDistance / numOfSequence));
            // Debug.Log(string.Format("clustered {0} {1}", numOfCluster, detectedLocations.Count));
        }
    }
}
