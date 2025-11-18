namespace TemplateStudioTest.Core.Models.EventArgs;
public class LedsDataReceivedEventArgs : System.EventArgs
{
    public byte[] ByteArrayReceived {
        get; private set;
    }
    public byte ByteReceived
    {
        get; private set;
    }
    public bool UseByteArray
    {
        get; private set;
    }
    public LedsDataReceivedEventArgs(byte? _byte)
    {
        ByteReceived = _byte ?? throw new ArgumentNullException("_byte argument cannot be null.");
        UseByteArray= false;
    }
    public LedsDataReceivedEventArgs(byte[] _bytes)
    {
        ByteArrayReceived = _bytes ?? throw new ArgumentNullException("_bytes argument cannot be null.");
        UseByteArray= true;
    }
}
