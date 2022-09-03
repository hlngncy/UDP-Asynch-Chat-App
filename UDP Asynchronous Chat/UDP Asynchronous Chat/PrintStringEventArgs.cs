using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDP_Asynchronous_Chat
{
    public class PrintStringEventArgs: EventArgs
    {
        public string MessageToPrint { get; set; }
        public PrintStringEventArgs(string _message) => MessageToPrint = _message;
    }

}
