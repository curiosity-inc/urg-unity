using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text;

namespace Urg
{
    public class UrgSensor : MonoBehaviour
    {

        public int startIndex = 0;
        public int endIndex = 1080;
        [Tooltip("Angle offset of URG sensor in degree. Note that 0 means the front of the sensor.")]
        public float offsetDegrees = -135;
        public float stepAngleDegrees = 0.25f;
        public ITransport transport;
        public bool debugMode = false;

        public float StepAngleRadians
        {
            get
            {
                return Mathf.Deg2Rad * stepAngleDegrees;
            }
        }
        public float OffsetRadians
        {
            get
            {
                return Mathf.Deg2Rad * offsetDegrees;
            }
        }

        public delegate void DistanceReceivedEventHandler(DistanceRecord data);
        public event DistanceReceivedEventHandler OnDistanceReceived;

        List<IFilter> locationFilters = new List<IFilter>();
        IClusterExtraction clusterExtraction;

        bool isRunning = false;
        Thread thread;
        float[] distances;

        readonly string COMMAND_GET_DISTANCE_ONCE = "GD";
        readonly string COMMAND_GET_DISTANCE_MULTI = "MD";
        readonly string COMMAND_BEGIN_SCANNING = "BM";
        readonly string COMMAND_STOP_SCANNING = "QT";
        readonly string STATUS_SUCCESS = "00";
        readonly string STATUS_GET_DISTANCE_SUCCESS = "99";

        public int DistanceLength
        {
            get
            {
                return endIndex - startIndex + 1;
            }
        }

        void Awake()
        {
            transport = GetComponent<ITransport>();
            distances = new float[DistanceLength];

            if (transport.Open())
            {
                Thread.Sleep(200);

                SCIP2();
                Thread.Sleep(200);
                Read();

                //GetStatus();
                //Thread.Sleep(200);
                //Read();

                StartScanning();
                Thread.Sleep(200);
                Read();

                GetDistancesMulti(0);
                Thread.Sleep(200);
                Read();

                isRunning = true;

                thread = new Thread(ReadLoop);
                thread.Start();
            }
            else
            {
                Debug.LogWarning("Failed to open sensor connection.");
            }
        }

        void Update()
        {
        }

        void OnDestroy()
        {
            // Debug.Log("Close");
            Close();
        }

        private void Close()
        {
            isRunning = false;

            Thread.Sleep(200);

            StopScanning();

            if (thread != null && thread.IsAlive)
            {
                thread.Join();
            }

            if (transport.IsConnected())
            {
                transport.Close();
            }
        }

        private void ReadLoop()
        {
            Debug.Log("Read loop started.");
            while (isRunning && transport.IsConnected())
            {
                try
                {
                    Read();
                }
                catch (System.TimeoutException e)
                {
                    Debug.LogWarning(e.Message + ":" + e.StackTrace);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning(e.Message + ":" + e.StackTrace);
                }
            }
            Debug.Log("Read loop finished.");
        }

        private void Read()
        {
            string command = transport.ReadLine();
            string status = transport.ReadLine();

            if (debugMode)
            {
                Debug.Log(string.Format("{0}:{1}", command, status));
            }

            if (command.StartsWith(COMMAND_GET_DISTANCE_ONCE, System.StringComparison.Ordinal))
            {
                if (status.StartsWith(STATUS_SUCCESS, System.StringComparison.Ordinal))
                {
                    ReadDistanceData();
                }
            }
            else if (command.StartsWith(COMMAND_GET_DISTANCE_MULTI, System.StringComparison.Ordinal))
            {
                if (status.StartsWith(STATUS_SUCCESS, System.StringComparison.Ordinal))
                {
                    transport.ReadLine();
                }
                else if (status.StartsWith(STATUS_GET_DISTANCE_SUCCESS, System.StringComparison.Ordinal))
                {
                    ReadDistanceData();
                }
            }
            else if (command.StartsWith("SCIP", System.StringComparison.Ordinal))
            {
                // read another new line.
                transport.ReadLine();
            }
            else
            {
                transport.ReadLine();
            }
        }

        private void ReadDistanceData()
        {
            var distanceRecord = new DistanceRecord();

            string timestamp = transport.ReadLine();
            string actualTimestamp = timestamp.Substring(0, timestamp.Length - 1);
            var timestampBytes = Encoding.GetEncoding("UTF-8").GetBytes(actualTimestamp);
            if (timestampBytes.Length == 4)
            {
                int[] timestampTmp = new int[1];
                DecodeMulti(timestampBytes, timestampTmp, 4);
                distanceRecord.Timestamp = timestampTmp[0];
            }

            string data = "";

            while (true)
            {
                string line = transport.ReadLine();
                //Debug.Log(string.Format("read: {0}", line.Length));
                if (line.Length == 0)
                {
                    // last
                    break;
                }
                // last character is checksum
                string actualData = line.Substring(0, line.Length - 1);
                data += actualData;
            }
            // Debug.Log(data);
            var dataBytes = Encoding.GetEncoding("UTF-8").GetBytes(data);
            DecodeMulti(dataBytes, distances, 3);
            distanceRecord.RawDistances = distances;

            var detectedLocations = new List<DetectedLocation>();
            for (var i = 0; i < distances.Length; i++)
            {
                detectedLocations.Add(new DetectedLocation(i, stepAngleDegrees * i + offsetDegrees, distances[i]));
            }
            foreach (var filter in locationFilters)
            {
                detectedLocations = filter.Filter(detectedLocations);
            }

            // pass a copy of list since the original list is not thread safe
            var copy = new List<DetectedLocation>();
            copy.AddRange(detectedLocations.Select(i => (DetectedLocation)i.Clone()));
            distanceRecord.FilteredResults = copy;

            if (clusterExtraction != null)
            {
                distanceRecord.ClusteredIndices = clusterExtraction.ExtractClusters(copy);
            }

            OnDistanceReceived?.Invoke(distanceRecord);
        }

        public void Write(string message)
        {
            try
            {
                Debug.Log(string.Format("write: {0}", message));
                if (transport.IsConnected())
                {
                    var bytes = Encoding.GetEncoding("UTF-8").GetBytes(message);
                    transport.Write(bytes);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }

        public void SCIP2()
        {
            Write("SCIP2.0\n");
        }

        public void GetStatus()
        {
            Write("VV\n");
        }

        public void StartScanning()
        {
            Write(COMMAND_BEGIN_SCANNING + "\n");
        }

        public void StopScanning()
        {
            Write(COMMAND_STOP_SCANNING + "\n");
        }

        public void GetDistancesOnce()
        {
            Write(string.Format(COMMAND_GET_DISTANCE_ONCE + "{0:D4}{1:D4}00\n", startIndex, endIndex));
        }

        public void GetDistancesMulti(int sendCount, int bulkCount = 1, int skipCount = 0)
        {
            Write(string.Format(COMMAND_GET_DISTANCE_MULTI + "{0:D4}{1:D4}{2:D2}{3:D1}{4:D2}\n", startIndex, endIndex, bulkCount, skipCount, sendCount));
        }

        public void AddFilter(IFilter filter)
        {
            this.locationFilters.Add(filter);
        }

        public void RemoveAllFilters()
        {
            this.locationFilters.Clear();
        }

        public void SetClusterExtraction(IClusterExtraction clusterExtraction)
        {
            this.clusterExtraction = clusterExtraction;
        }

        public void ClearClusterExtraction()
        {
            this.clusterExtraction = null;
        }

        /**
         * nビットキャラクタエンコードを1000倍されたfloatの配列としてデコードする
         */
        void DecodeMulti(byte[] code, float[] output, int numOfByte, int outputOffset = 0)
        {
            int index = 0;
            for (int i = 0; i < code.Length;)
            {
                int value = 0;
                for (int j = 0; j < numOfByte; ++j)
                {
                    value <<= 6;
                    value &= ~0x3f;
                    value |= code[i + j] - 0x30;
                }
                i += numOfByte;
                output[outputOffset + index] = (float)value / 1000f;
                index++;
            }
        }

        void DecodeMulti(byte[] code, int[] output, int numOfByte, int outputOffset = 0)
        {
            int index = 0;
            for (int i = 0; i < code.Length;)
            {
                int value = 0;
                for (int j = 0; j < numOfByte; ++j)
                {
                    value <<= 6;
                    value &= ~0x3f;
                    value |= code[i + j] - 0x30;
                }
                i += numOfByte;
                output[outputOffset + index] = value;
                index++;
            }
        }

        int Decode(string code, int numOfByte)
        {
            int value = 0;
            int i;
            for (i = 0; i < numOfByte; ++i)
            {
                value <<= 6;
                value &= ~0x3f;
                value |= code[i] - 0x30;
            }
            return value;
        }
    }
}
