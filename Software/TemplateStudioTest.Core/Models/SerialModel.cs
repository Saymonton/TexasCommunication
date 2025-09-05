using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplateStudioTest.Core.Models;
public class SerialModel
{
    public string PortName
    {
        get; set;
    }
    public int BaudRates 
    {
        get; set;
    }
    public Parity Parities
    {
        get; set;
    }
    public StopBits StopBits 
    {
        get; set;
    }
    public int DataBits 
    {
        get; set;
    }
}
