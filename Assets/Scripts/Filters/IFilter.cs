using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Urg
{
    public interface IFilter
    {
        List<DetectedLocation> Filter(List<DetectedLocation> inputList);
    }
}
