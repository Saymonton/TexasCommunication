using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TemplateStudioTest.Core.Models;

namespace TemplateStudioTest.Core.Contracts.Services;
public interface ISerialService
{
    IEnumerable<string> GetPortNames();
    bool IsConnected();
    bool TryConnect(SerialModel serialModel);
    bool TryDisconnect();
    Task<(bool sucess, byte byteResponse)> TryGetLedsStatus(CancellationToken cancellationToken);
    Task<(bool sucess, byte byteResponse)> TrySetLedsStatus(CancellationToken cancellationToken, byte byteToChange);
}
