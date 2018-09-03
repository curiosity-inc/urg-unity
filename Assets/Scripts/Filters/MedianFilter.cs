using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Urg
{
    public class MedianFilter : IFilter
    {
        private int length;
        private List<DetectedLocation> list = new List<DetectedLocation>();

        public MedianFilter(int length = 3)
        {
            if (length % 2 != 1)
            {
                throw new ArgumentException("length has to be a odd nuber.");
            }
            this.length = length;
        }

        public List<DetectedLocation> Filter(List<DetectedLocation> inputList)
        {
            list.Clear();
            var tmp = new float[length];
            for (var i = 0; i < inputList.Count - length; i++)
            {
                var loc = inputList[i];
                for (var j = 0; j < length; j++)
                {
                    tmp[j] = loc.distance;
                }
                float medianDistance = GetMedian(tmp);
                list.Add(new DetectedLocation(loc.angle, medianDistance));
            }
            return list;
        }

        public static float GetMedian(float[] sourceNumbers)
        {
            //Framework 2.0 version of this method. there is an easier way in F4
            if (sourceNumbers == null || sourceNumbers.Length == 0)
            {
                throw new System.Exception("Median of empty array not defined.");
            }

            //make sure the list is sorted, but use a new array
            var sortedPNumbers = (float[])sourceNumbers.Clone();
            Array.Sort(sortedPNumbers);

            //get the median
            int size = sortedPNumbers.Length;
            int mid = size / 2;
            var median = (size % 2 != 0) ? sortedPNumbers[mid] : (sortedPNumbers[mid] + sortedPNumbers[mid - 1]) / 2;
            return median;
        }
    }
}
