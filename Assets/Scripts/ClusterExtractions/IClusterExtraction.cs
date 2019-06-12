using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Urg
{
    public interface IClusterExtraction
    {
        List<List<int>> ExtractClusters(List<DetectedLocation> locations);
    }
}
