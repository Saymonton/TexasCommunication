using System.IO.Ports;
using TemplateStudioTest.Core.Contracts.Services;
using TemplateStudioTest.Core.Models;

namespace TemplateStudioTest.Core.Services;
public class SerialService : ISerialService
{
    private readonly SerialPort serialPort;

    public SerialService()
    {
        serialPort = new SerialPort();

    }

    public IEnumerable<string> GetPortNames()
    {
        return SerialPort.GetPortNames();
    }

    public bool IsConnected() => serialPort?.IsOpen ?? false;

    public bool TryConnect(SerialModel serialModel)
    {
        serialPort.PortName = serialModel.PortName;
        serialPort.BaudRate = serialModel.BaudRates;
        serialPort.DataBits = serialModel.DataBits;
        serialPort.StopBits = serialModel.StopBits;
        serialPort.Parity = serialModel.Parities;

        if (IsConnected()) return false;

        serialPort.Open();
        return true;
    }

    public bool TryDisconnect()
    {
        if (!IsConnected()) return true;
        // Limpar o buffer
        while (serialPort.BytesToRead > 0)
        {
            serialPort.ReadExisting();
        }
        serialPort.Close();
        return true;
    }
}
