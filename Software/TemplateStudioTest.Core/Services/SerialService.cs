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

    public bool TryConnect(SerialModel serialModel)
    {
        serialPort.PortName = serialModel.PortName;
        serialPort.BaudRate = serialModel.BaudRates;
        serialPort.DataBits = serialModel.DataBits;
        serialPort.StopBits = serialModel.StopBits;
        serialPort.Parity = serialModel.Parities;

        if (serialPort.IsOpen) return false;

        serialPort.Open();
        return true;
    }
}
