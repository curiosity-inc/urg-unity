using System.Collections;
using System.Collections.Generic;
using KdTree;
using KdTree.Math;
using System.Linq;

namespace Urg
{
    public class EuclidianClusteringFilter
    {
        private float distanceThreshold;
        public EuclidianClusteringFilter(float distanceThreshold = 0.1f)
        {
            this.distanceThreshold = distanceThreshold;
        }

        public List<List<int>> extractClusters(List<DetectedLocation> locations)
        {
            var kdTree = new KdTree<float, int>(2, new FloatMath());
            foreach (var loc in locations)
            {
                var p = loc.ToPosition2D();
                kdTree.Add(new float[] { p.x, p.y }, loc.index);
            }
            List<List<int>> clusters = new List<List<int>>();
            var processed = new bool[locations.Count];
            foreach (var loc in locations)
            {
                if (processed[loc.index])
                {
                    continue;
                }

                var p = loc.ToPosition2D();
                var q = new Queue<int>();
                q.Enqueue(loc.index);
                processed[loc.index] = true;
                int index = 0;
                while (index < q.Count)
                {
                    var neighbors = kdTree.RadialSearch(new float[] { p.x, p.y }, distanceThreshold);
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
