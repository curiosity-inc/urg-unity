using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Urg
{
    public class UrgSensor : MonoBehaviour
    {
        public string portName = "/dev/tty.usbmodem1421";
        public int baudRate = 115200;
        public int startIndex = 44;
        public int endIndex = 725;
        [Tooltip("Angle offset of URG sensor in degree. Note that 0 means the front of the sensor.")]
        public float offsetDegrees = -120;
        public int stepPerRotation = 1024;
        public bool debugMode = false;

        public float StepAngle
        {
            get
            {
                return 2.0f * Mathf.PI / stepPerRotation;
            }
        }
        public float OffsetRadians
        {
            get
            {
                return Mathf.Deg2Rad * offsetDegrees;
            }
        }

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

        public delegate void DistanceReceivedEventHandler(float[] distances);
        public event DistanceReceivedEventHandler OnDistanceReceived;

        private SerialPort serialPort;
        private Thread thread;
        private bool isRunning = false;
        private float[] distances;

        void Awake()
        {
            distances = new float[DistanceLength];

            if (Open())
            {
                Thread.Sleep(200);

                SCIP2();
                Thread.Sleep(200);
                Read();

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

        private bool Open()
        {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        var portExists = File.Exists(PortName);
        if (!portExists)
        {
            Debug.LogWarning(string.Format("Port {0} does not exist.", PortName));
            return false;
        }
#endif

            bool openSuccess = false;
            serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);
            for (int i = 0; i < 1; i++)
            {
                try
                {
                    if (debugMode)
                    {
                        Debug.Log("Opening URG Serial connection...");
                    }
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                serialPort_.ReadTimeout = 1000;
#else
                    serialPort.ReadTimeout = 10;
#endif
                    serialPort.WriteTimeout = 1000;
                    serialPort.ReadBufferSize = 4096;
                    serialPort.NewLine = "\n";
                    serialPort.Open();
                    openSuccess = true;
                    break;
                }
                catch (IOException ex)
                {
                    Debug.LogWarning("error:" + ex.ToString());
                }
                Thread.Sleep(2000);
            }

            if (!openSuccess)
            {
                return false;
            }

            if (debugMode)
            {
                Debug.Log("Opened URG Serial connection.");
            }

            return true;
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

            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
                serialPort.Dispose();
            }
        }

        private void ReadLoop()
        {
            while (isRunning && serialPort != null && serialPort.IsOpen)
            {
                try
                {
                    Read();
                }
                catch (System.TimeoutException e)
                {
                    // ignore
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning(e.Message + ":" + e.StackTrace);
                }
            }
        }

        private void Read()
        {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                while (serialPort_.BytesToRead > 0)
                {
#endif
            string command = serialPort.ReadLine();
            string status = serialPort.ReadLine();

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
                    serialPort.ReadLine();
                }
                else if (status.StartsWith(STATUS_GET_DISTANCE_SUCCESS))
                {
                    ReadDistanceData();
                }
            }
            else if (command.StartsWith("SCIP"))
            {
                // read another new line.
                serialPort.ReadLine();
            }
            else
            {
                string empty = serialPort.ReadLine();
            }
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            }
#endif
        }

        private void ReadDistanceData()
        {
            string timeStamp = serialPort.ReadLine();
            string data = "";

            while (true)
            {
                string line = serialPort.ReadLine();
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
        }

        public void Write(string message)
        {
            try
            {
                if (debugMode)
                {
                    Debug.Log(string.Format("write: {0}", message));
                }
                if (serialPort != null && serialPort.IsOpen)
                {
                    serialPort.Write(message);
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