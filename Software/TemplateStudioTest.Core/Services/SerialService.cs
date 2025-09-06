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
        if (!IsConnected() && SerialPort.GetPortNames().Contains(serialModel.PortName))
        {
            serialPort.PortName = serialModel.PortName;
            serialPort.BaudRate = serialModel.BaudRates;
            serialPort.DataBits = serialModel.DataBits;
            serialPort.StopBits = serialModel.StopBits;
            serialPort.Parity = serialModel.Parities;
            serialPort.Open();
            while (serialPort.BytesToRead > 0) serialPort.ReadExisting();
            return true;
        }        
        return false;
    }

    public bool TryDisconnect()
    {
        if (!IsConnected()) return true;
        // Limpar o buffer
        while (serialPort.BytesToRead > 0) serialPort.ReadExisting();
        serialPort.Close();
        return true;
    }

    public async Task<Tuple<bool, byte>> TryGetLedsStatus(CancellationToken cancellationToken)
    {
        byte[] ledsStatus = [ 0, 0, 0, 0 ];
        byte HEADER_1 = 0xAA;
        byte HEADER_2 = 0x55;
        // Mensagem para solicitar o status dos LEDs
        byte[] message =
        [
            HEADER_1, // Header 1
            HEADER_2, // Header 2
            0x01, // Comando
            0x00, // Length dos dados
            0x01  // Checksum Comando + Length dos dados + dados
        ];

        // Limpar buffer
        while (serialPort.BytesToRead > 0) serialPort.ReadExisting();

        serialPort.Write(message, 0, message.Length);

        await Task.Delay(50, cancellationToken);

        if(serialPort.BytesToRead >= 5)
        {
            while (serialPort.ReadByte() != HEADER_1) ;
            if (serialPort.ReadByte() == HEADER_2)
            {
                byte cmd = (byte)serialPort.ReadByte();
                byte len = (byte)serialPort.ReadByte();
                byte data = (byte)serialPort.ReadByte();
                byte chksum = (byte)serialPort.ReadByte();

                int calc = cmd + len + data;

                if (chksum == calc)
                {
                    return new(true, data);
                }
            }            
        }

        serialPort.BaseStream.Flush();
        await Task.Delay(50, cancellationToken);
        return new(false, 0);
    }
}
