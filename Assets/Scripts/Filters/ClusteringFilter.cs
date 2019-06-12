using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Urg
{
    [System.Obsolete("Use EuclidianClusterExtraction class instead.")]
    public class ClusteringFilter : IFilter
    {
        private int[] clusteringNumber;
        private List<DetectedLocation> detectedLocations = new List<DetectedLocation>();
        private float minDistance;

        ClusteringFilter()
        {
        }

        public ClusteringFilter(float minDistance = 0.1f)
        {
            this.minDistance = minDistance;
        }

        // TODO this filter implicitly assumes that inputList is ordered by its original index.
        public List<DetectedLocation> Filter(List<DetectedLocation> inputList)
        {
            if (clusteringNumber == null)
            {
                clusteringNumber = new int[inputList.Count];
            }
            for (int i = 0; i < clusteringNumber.Length; i++)
            {
                clusteringNumber[i] = -1;
            }
            int numOfCluster = 1;
            clusteringNumber[0] = 0;
            for (int i = 1; i < inputList.Count; i++)
            {
                if (Mathf.Abs(inputList[i].distance - inputList[i - 1].distance) < minDistance)
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
            float sumOfAngle = inputList[0].angle;
            float sumOfDistance = inputList[0].distance;
            int numOfSequence = 1;
            for (int i = 1; i < clusteringNumber.Length; i++)
            {
                if (i == clusteringNumber[i])
                {
                    float avgAngle = sumOfAngle / numOfSequence;
                    float avgDistance = sumOfDistance / numOfSequence;
                    detectedLocations.Add(new DetectedLocation(-1, avgAngle, avgDistance));
                    sumOfAngle = 0;
                    sumOfDistance = 0;
                    numOfSequence = 0;
                }
                sumOfAngle += inputList[i].angle;
                sumOfDistance += inputList[i].distance;
                numOfSequence++;
            }
            detectedLocations.Add(new DetectedLocation(-1, sumOfAngle / numOfSequence, sumOfDistance / numOfSequence));
            return detectedLocations;
            // Debug.Log(string.Format("clustered {0} {1}", numOfCluster, detectedLocations.Count));
        }
    }
}
