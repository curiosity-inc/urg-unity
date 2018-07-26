using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;

public class EthernetTransport : MonoBehaviour, ITransport
{
    public string ipAddress = "192.168.0.10";
    public int port = 10940;
    private TcpClient tcpClient;
    private NetworkStream stream;
    private TextReader reader;

    public void Close()
    {
        tcpClient.Close();
    }

    public bool IsConnected()
    {
        return tcpClient != null && tcpClient.Connected;
    }

    public bool Open()
    {
        try
        {
            tcpClient = new TcpClient();
            tcpClient.ReceiveTimeout = 5000;
            tcpClient.Connect(ipAddress, port);
            stream = tcpClient.GetStream();
            reader = new StreamReader(stream);
        }
        catch (Exception e)
        {
            Debug.LogWarningFormat("tcp open error {0}", e.ToString());
            return false;
        }
        return true;
    }

    public string ReadLine()
    {
        return reader.ReadLine();
    }

    public void Write(byte[] bytes)
    {
        stream.Write(bytes, 0, bytes.Length);
    }
}
