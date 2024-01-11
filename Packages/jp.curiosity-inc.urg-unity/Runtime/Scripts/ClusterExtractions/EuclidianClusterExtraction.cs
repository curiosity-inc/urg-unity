using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using DataStructures.ViliWonka.KDTree;

namespace Urg
{
    public class EuclidianClusterExtraction : IClusterExtraction
    {
        KDQuery query = new KDQuery();

        public static Func<float[], float[], double> L2Norm_Squared_Float = (x, y) =>
        {
            float dist = 0f;
            for (int i = 0; i < x.Length; i++)
            {
                dist += (x[i] - y[i]) * (x[i] - y[i]);
            }

            return dist;
        };

        private float distanceThreshold;
        public EuclidianClusterExtraction(float distanceThreshold = 0.1f)
        {
            //this.distanceThreshold = distanceThreshold * distanceThreshold;
            this.distanceThreshold = distanceThreshold;
        }

        public List<List<int>> ExtractClusters(List<DetectedLocation> locations)
        {
            var points = new Vector3[locations.Count];
            var nodes = new int[locations.Count];
            for (int i = 0; i < locations.Count; i++)
            {
                var loc = locations[i];
                points[i] = loc.ToPosition();
                nodes[i] = i;
            }
            var tree = new KDTree(points, 32);
            List<List<int>> clusters = new List<List<int>>();
            var processed = new bool[locations.Count];
            for (int i = 0; i < locations.Count; i++)
            {
                if (processed[i])
                {
                    continue;
                }
                var loc = locations[i];

                var p = loc.ToPosition2D();
                var q = new Queue<int>();
                q.Enqueue(i);
                processed[i] = true;
                int index = 0;
                while (index < q.Count)
                {
                    var targetIndex = q.ElementAt<int>(index);
                    var tp = locations[targetIndex].ToPosition();
                    List<int> resultIndices = new List<int>();
                    query.Radius(tree, tp, distanceThreshold, resultIndices);
                    foreach (var neighbor in resultIndices)
                    {
                        if (processed[neighbor])
                        {
                            continue;
                        }
                        q.Enqueue(neighbor);
                        processed[neighbor] = true;
                    }
                    index++;
                }
                var l = q.ToList<int>();
                //Debug.LogFormat("cluster size={0}", l.Count);
                clusters.Add(l);
            }
            return clusters;
        }
    }
}
