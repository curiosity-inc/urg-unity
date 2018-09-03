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

        public delegate void DistanceReceivedEventHandler(float[] rawDistances);
        public event DistanceReceivedEventHandler OnDistanceReceived;

        public delegate void LocationDetectedEventHandler(List<DetectedLocation> locations);
        public event LocationDetectedEventHandler OnLocationDetected;

        private List<IFilter> locationFilters = new List<IFilter>();

        private bool isRunning = false;
        private Thread thread;
        private float[] distances;

        private readonly string COMMAND_GET_DISTANCE_ONCE = "GD";
        private readonly string COMMAND_GET_DISTANCE_MULTI = "MD";
        private readonly string COMMAND_BEGIN_SCANNING = "BM";
        private readonly string COMMAND_STOP_SCANNING = "QT";
        private readonly string STATUS_SUCCESS = "00";
        private readonly string STATUS_GET_DISTANCE_SUCCESS = "99";

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

            if (command.StartsWith(COMMAND_GET_DISTANCE_ONCE))
            {
                if (status.StartsWith(STATUS_SUCCESS))
                {
                    ReadDistanceData();
                }
            }
            else if (command.StartsWith(COMMAND_GET_DISTANCE_MULTI))
            {
                if (status.StartsWith(STATUS_SUCCESS))
                {
                    transport.ReadLine();
                }
                else if (status.StartsWith(STATUS_GET_DISTANCE_SUCCESS))
                {
                    ReadDistanceData();
                }
            }
            else if (command.StartsWith("SCIP"))
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
            // string timestamp = 
            transport.ReadLine();
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
            decodeMulti(dataBytes, distances, 3);
            if (OnDistanceReceived != null)
            {
                OnDistanceReceived(distances);
            }

            var detectedLocations = new List<DetectedLocation>();
            for (var i = 0; i < distances.Length; i++)
            {
                detectedLocations.Add(new DetectedLocation(stepAngleDegrees * i + offsetDegrees, distances[i]));
            }
            foreach (var filter in locationFilters)
            {
                detectedLocations = filter.Filter(detectedLocations);
            }

            // pass a copy of list since the original list is not thread safe
            var copy = new List<DetectedLocation>();
            copy.AddRange(detectedLocations.Select(i => (DetectedLocation)i.Clone()));
            if (OnLocationDetected != null)
            {
                OnLocationDetected(copy);
            }
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

        void decodeMulti(byte[] code, float[] output, int numOfByte, int outputOffset = 0)
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

        int decode(string code, int numOfByte)
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
