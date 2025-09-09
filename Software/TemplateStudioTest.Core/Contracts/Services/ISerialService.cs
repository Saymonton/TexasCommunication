using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TemplateStudioTest.Core.Models;

namespace TemplateStudioTest.Core.Contracts.Services;
public interface ISerialService
{
    event EventHandler<byte> OnLedsStatusReceived;
    IEnumerable<string> GetPortNames();
    bool IsConnected();
    bool TryConnect(SerialModel serialModel);
    bool TryDisconnect();
    bool TryGetLedsStatus();
    bool TrySetLedsStatus(byte byteToChange);
    Task StartMonitoringRoutine(CancellationToken cancellationToken);
}
