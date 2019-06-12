using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Supercluster.KDTree;

namespace Urg
{
    public class EuclidianClusterExtraction : IClusterExtraction
    {
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
            this.distanceThreshold = distanceThreshold * distanceThreshold;
            //this.distanceThreshold = distanceThreshold;
        }

        public List<List<int>> ExtractClusters(List<DetectedLocation> locations)
        {
            var points = new float[locations.Count][];
            var nodes = new int[locations.Count];
            for (int i = 0; i < locations.Count; i++)
            {
                var loc = locations[i];
                var p = loc.ToPosition2D();
                points[i] = new float[2] { p.x, p.y };
                nodes[i] = i;
            }
            var tree = new KDTree<float, int>(2, points, nodes, L2Norm_Squared_Float, float.MinValue, float.MaxValue);
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
                    var tp = locations[targetIndex].ToPosition2D();
                    var neighbors = tree.RadialSearch(new float[] { tp.x, tp.y }, distanceThreshold);
                    foreach (var neighbor in neighbors)
                    {
                        if (processed[neighbor.Item2])
                        {
                            continue;
                        }
                        q.Enqueue(neighbor.Item2);
                        processed[neighbor.Item2] = true;
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
