using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Urg
{
    public class SpatialMedianFilter : IFilter
    {
        private int length;
        private List<DetectedLocation> list = new List<DetectedLocation>();

        public SpatialMedianFilter(int lengthOfIndices = 3)
        {
            if (lengthOfIndices % 2 != 1)
            {
                throw new ArgumentException("length has to be a odd nuber.");
            }
            this.length = lengthOfIndices;
        }

        public List<DetectedLocation> Filter(List<DetectedLocation> inputList)
        {
            list.Clear();
            var tmp = new List<DetectedLocation>();
            for (var i = 0; i < inputList.Count - length; i++)
            {
                tmp.Clear();
                for (var j = 0; j < length; j++)
                {
                    var loc = inputList[i + j];
                    tmp.Add(loc);
                }
                var median = GetMedian(tmp);
                list.Add((DetectedLocation)median);
            }
            return list;
        }

        public static DetectedLocation GetMedian(List<DetectedLocation> sourceNumbers)
        {
            sourceNumbers.Sort((a, b) => a.distance.CompareTo(b.distance));
            int size = sourceNumbers.Count;
            int mid = size / 2;
            return sourceNumbers[mid];
        }
    }
}
