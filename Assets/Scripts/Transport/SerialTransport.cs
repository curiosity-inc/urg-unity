using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class SerialTransport : MonoBehaviour, ITransport
{
    public string portName = "/dev/tty.usbmodem1421";
    public int baudRate = 115200;
    private SerialPort serialPort;

    public void Close()
    {
        serialPort.Close();
        serialPort.Dispose();
    }

    public bool IsConnected()
    {
        return serialPort != null && serialPort.IsOpen;
    }

    public bool Open()
    {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        var portExists = File.Exists(portName);
        if (!portExists)
        {
            Debug.LogWarning(string.Format("Port {0} does not exist.", portName));
            return false;
        }
#endif

        bool openSuccess = false;
        serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);
        for (int i = 0; i < 1; i++)
        {
            try
            {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                serialPort.ReadTimeout = 1000;
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

        return openSuccess;
    }

    public string ReadLine()
    {
        return serialPort.ReadLine();
    }

    public void Write(byte[] bytes)
    {
        serialPort.Write(bytes, 0, bytes.Length);
    }
}
