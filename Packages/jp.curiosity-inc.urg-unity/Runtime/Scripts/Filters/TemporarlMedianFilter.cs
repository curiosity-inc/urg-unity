using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

namespace Urg
{
    public class FixedSizedQueue<T> : ConcurrentQueue<T>
    {
        private readonly object syncObject = new object();

        public int Size { get; private set; }

        public FixedSizedQueue(int size)
        {
            Size = size;
        }

        public new void Enqueue(T obj)
        {
            base.Enqueue(obj);
            lock (syncObject)
            {
                while (base.Count > Size)
                {
                    T outObj;
                    base.TryDequeue(out outObj);
                }
            }
        }
    }

    public class TemporalMedianFilter : IFilter
    {
        private int length;
        private FixedSizedQueue<List<DetectedLocation>> queue;
        private List<DetectedLocation> list = new List<DetectedLocation>();

        public TemporalMedianFilter(int lengthOfFrames = 3)
        {
            if (lengthOfFrames % 2 != 1)
            {
                throw new ArgumentException("length has to be a odd nuber.");
            }
            this.length = lengthOfFrames;
            queue = new FixedSizedQueue<List<DetectedLocation>>(lengthOfFrames);
        }

        public List<DetectedLocation> Filter(List<DetectedLocation> inputList)
        {
            queue.Enqueue(inputList);
            if (queue.Count < length) {
                return inputList;
            }

            list.Clear();
            List<DetectedLocation> tmp = new List<DetectedLocation>();
            for (var i = 0; i < inputList.Count; i++)
            {
                tmp.Clear();
                for (var j = 0; j < length; j++) {
                    tmp.Add(queue.ElementAt(j).ElementAt(i));
                }
                var median = GetMedian(tmp);
                list.Add(median);
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
