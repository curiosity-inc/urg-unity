using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Urg
{
    public class AffineConverterFilter : IFilter
    {
        private int length;
        private AffineConverter affineConverter;
        private List<DetectedLocation> list = new List<DetectedLocation>();

        public AffineConverterFilter(AffineConverter affineConverter)
        {
            this.affineConverter = affineConverter;
        }

        public List<DetectedLocation> Filter(List<DetectedLocation> inputList)
        {
            int allCount = inputList.Count;
            list.Clear();
            foreach (var data in inputList)
            {
                if (affineConverter.Sensor2WorldPosition(data.ToPosition2D(), out _))
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
