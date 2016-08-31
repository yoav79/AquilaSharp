using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aquila.Protocol
{
    public class ScanPort
    {
        private readonly int _baudRate;

     
        public bool Found { get; set; }

        public string  Port { get; set; }

        public int BaudRate => _baudRate;

        public ScanPort(int baudRate)
        {
            _baudRate = baudRate;
        }

        public void LookUp()
        {
            var ports = SerialPort.GetPortNames();
            foreach (var port in ports)
            {
                var st = new SerialTransport(port, _baudRate);

                if (!st.Open()) continue;

                Console.WriteLine("  *Trying:" + port);
                

                if (!st.IsConnected) continue;

                Found = true;
                Port = port;
                return;
            }
        }
    }
}
