using System.IO.Ports;
using TemplateStudioTest.Core.Contracts.Services;

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
}
