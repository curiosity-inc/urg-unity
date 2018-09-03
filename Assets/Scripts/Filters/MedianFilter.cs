using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Urg
{
    public class MedianFilter : IFilter
    {
        private List<DetectedLocation> list = new List<DetectedLocation>();

        public List<DetectedLocation> Filter(List<DetectedLocation> inputList)
        {
            list.Clear();
            for (var i = 1; i < inputList.Count - 1; i++)
            {
                if (inputList[i - 1].distance >= inputList[i].distance && inputList[i - 1].distance <= inputList[i + 1].distance)
                {
                    list.Add(inputList[i - 1]);
                }
                else if (inputList[i + 1].distance >= inputList[i].distance && inputList[i + 1].distance <= inputList[i - 1].distance)
                {
                    list.Add(inputList[i + 1]);
                }
                else
                {
                    list.Add(inputList[i]);
                }
            }
            return list;
        }
    }
}
