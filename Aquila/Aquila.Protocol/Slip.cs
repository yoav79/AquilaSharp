using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Aquila.Protocol
{
    public class Slip
    {

        const char END = (char) 0xC0;
        const char ESC = (char)0xDB;
        const char ESC_END = (char)0xDC;
        const char ESC_ESC = (char)0xDD;

        const short IDLE = 0;
        const short RECEIVING = 1;
        const short ERROR = 2;

        const short MAX_SIZE = 255;

        char[] _buffer = new char[MAX_SIZE];
        int _index;
        short _state;

        private SerialTransport _st;

        public Slip()
        {
            _state = IDLE;
            _index = 0;
            
        }

        private void ParseEscape() { }

        public void Begin(string portName, int boudRate)
        {
            _st = new SerialTransport(portName, boudRate);
            _st.Receive += SerialTransport_Receive;

        }

        private void SerialTransport_Receive(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void Send(char[] data)
        {
            _st.Write(END);

            foreach (char t in data)
            {
                if (t == END)
                {
                    _st.Write(ESC);
                    _st.Write(ESC_END);
                }
                else if (t == ESC)
                {
                    _st.Write(ESC);
                    _st.Write(ESC_ESC);
                }
                else
                {
                    _st.Write(t);
                }
            }

            _st.Write(END);
        }
    }
}
