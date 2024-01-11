using System.Collections;
using System.Collections.Generic;

public interface ITransport
{
    bool Open();
    string ReadLine();
    bool IsConnected();
    void Close();
    void Write(byte[] bytes);
}
