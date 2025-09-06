using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TemplateStudioTest.Core.Models;

namespace TemplateStudioTest.Core.Contracts.Services;
public interface ISerialService
{
    public IEnumerable<string> GetPortNames();
    bool IsConnected();
    bool TryConnect(SerialModel serialModel);
    bool TryDisconnect();
    bool TryGetLedsStatus(out int[] ledsStatus);
}
