using System.Collections;
using System.Collections.Generic;
using KdTree;
using KdTree.Math;
using System.Linq;
using UnityEngine;

namespace Urg
{
    public class EuclidianClusterExtraction: IClusterExtraction
    {
        private float distanceThreshold;
        public EuclidianClusterExtraction(float distanceThreshold = 0.1f)
        {
            this.distanceThreshold = distanceThreshold;
        }

        public List<List<int>> ExtractClusters(List<DetectedLocation> locations)
        {
            var kdTree = new KdTree<float, int>(2, new FloatMath());
            for (int i = 0; i < locations.Count; i++)
            {
                var loc = locations[i];
                var p = loc.ToPosition2D();
                kdTree.Add(new float[] { p.x, p.y }, i);
            }
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
                    var neighbors = kdTree.RadialSearch(new float[] { tp.x, tp.y }, distanceThreshold);
                    foreach (var neighbor in neighbors)
                    {
                        if (processed[neighbor.Value])
                        {
                            continue;
                        }
                        q.Enqueue(neighbor.Value);
                        processed[neighbor.Value] = true;
                    }
                    index++;
                }
                var l = q.ToList<int>();
                clusters.Add(l);
            }
            return clusters;
        }
    }
}
