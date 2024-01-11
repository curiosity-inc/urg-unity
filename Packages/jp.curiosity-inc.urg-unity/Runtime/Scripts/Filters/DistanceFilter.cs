using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Urg
{
    public class DistanceFilter : IFilter
    {
        private int length;
        private float distanceThreshold;
        private List<DetectedLocation> list = new List<DetectedLocation>();

        public DistanceFilter(float distanceThreshold = 5)
        {
            this.distanceThreshold = distanceThreshold;
        }

        public List<DetectedLocation> Filter(List<DetectedLocation> inputList)
        {
            int allCount = inputList.Count;
            list.Clear();
            foreach (var data in inputList)
            {
                if (data.distance < distanceThreshold)
                {
                    list.Add(data);
                }
            }
            int filteredCount = list.Count;
            //UnityEngine.Debug.LogFormat("{0} => {1}", allCount, filteredCount);
            return list;
        }
    }
}
