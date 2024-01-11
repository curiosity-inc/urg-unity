using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Urg
{
    public class DistanceRecord
    {
        public int Timestamp {
            get;
            set;
        }

        public float[] RawDistances {
            get;
            set;
        }

        public List<DetectedLocation> FilteredResults {
            get;
            set;
        }

        public List<List<int>> ClusteredIndices {
            get;
            set;
        }

    }
}
