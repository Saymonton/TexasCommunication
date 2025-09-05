using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplateStudioTest.Core.Contracts.Services;
public interface ISerialService
{
    public IEnumerable<string> GetPortNames();
}
