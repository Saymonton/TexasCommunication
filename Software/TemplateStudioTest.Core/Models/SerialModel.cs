using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplateStudioTest.Core.Models;
public class SerialModel
{
    public string PortName;
    public int BaudRates;
    public Parity Parities;
    public StopBits StopBits;
    public int DataBits;
}
