using System.IO.Ports;
using TemplateStudioTest.Core.Contracts.Services;
using TemplateStudioTest.Core.Models;

namespace TemplateStudioTest.Core.Services;
public class SerialService : ISerialService
{
    private readonly SerialPort serialPort;
    public event EventHandler<byte> OnLedsStatusReceived;
    public event EventHandler OnSerialConnected;
    public event EventHandler OnSerialDisconnected;
    private readonly object _locker = new();
    private const byte HEADER_1 = 0xAA;
    private const byte HEADER_2 = 0x55;

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
            OnSerialConnected?.Invoke(this, null);
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
        OnSerialDisconnected?.Invoke(this, null);
        return true;
    }

    public bool TryGetLedsStatus()
    {
        byte CMD      = 1;
        byte LEN      = 0;
        int  CHKSUM   = LEN + CMD;
        // Mensagem para solicitar o status dos LEDs
        byte[] message =
        [
            HEADER_1, // Header 1
            HEADER_2, // Header 2
            CMD, // Comando
            LEN, // Length dos dados
            (byte)CHKSUM  // Checksum Comando + Length dos dados + dados
        ];

        lock (_locker)
        {
            // Limpar buffer
            while (serialPort.BytesToRead > 0) serialPort.ReadExisting();

            serialPort.Write(message, 0, message.Length);

            while (serialPort.BytesToRead < 5) ;

            if (serialPort.BytesToRead >= 5)
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
                        OnDataReceived(data);
                        return true;
                    }
                }
            }

            while (serialPort.BytesToRead > 0) serialPort.ReadExisting();
        }
        return false;
    }

    public bool TrySetLedsStatus(byte byteToChange)
    {
        byte CMD = 2;
        byte LEN = 1;
        int CHKSUM = CMD + LEN + byteToChange;
        // Mensagem para solicitar o status dos LEDs
        byte[] message =
        [
            HEADER_1, // Header 1
            HEADER_2, // Header 2
            CMD, // Comando
            LEN, // Length dos dados
            byteToChange,   // Byte a ser alterado
            (byte)CHKSUM  // Checksum Comando + Length dos dados + dados
        ];

        lock (_locker)
        {
            // Limpar buffer
            while (serialPort.BytesToRead > 0) serialPort.ReadExisting();

            serialPort.Write(message, 0, message.Length);

            while (serialPort.BytesToRead < 5) ;

            if (serialPort.BytesToRead >= 5)
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
                        OnDataReceived(data);
                        return true;
                    }
                }
            }

            while (serialPort.BytesToRead > 0) serialPort.ReadExisting();
        }
        return false;
    }

    public async Task StartMonitoringRoutine(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            ReadSerial();
            await Task.Delay(100, cancellationToken);
        }
    }
    private void ReadSerial()
    {
        byte data = 0;
        lock (_locker)
        {
            if (serialPort.BytesToRead < 6) return;
            
            while (serialPort.ReadByte() != HEADER_1 && serialPort.BytesToRead > 1) ;
            if (serialPort.ReadByte() != HEADER_2)
                return;
        }
        byte cmd = (byte)serialPort.ReadByte();
        byte len = (byte)serialPort.ReadByte();
        data = (byte)serialPort.ReadByte();
        byte chksum = (byte)serialPort.ReadByte();

        int calc = cmd + len + data;

        if (chksum != calc) return;

        OnDataReceived(data);        
    }
    protected virtual void OnDataReceived(byte data)
    {
        var handler = OnLedsStatusReceived;
        if (handler is null) return;

        try
        {
            handler(this, data);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Handler falhou: {ex}");
        }
    }
}
