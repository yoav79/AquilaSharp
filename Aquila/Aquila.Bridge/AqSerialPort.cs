using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aquila.Bridge
{
    class AqSerialPort
    {
        private readonly SerialPort _serialPort = new SerialPort();
        private int _baudRate = 9600;
        private int _dataBits = 8;
        private Handshake _handshake = Handshake.None;
        private Parity _parity = Parity.None;
        private string _portName = "COM1";
        private StopBits _stopBits = StopBits.One;

        private byte _terminator = 0x4;
        private string _tString = string.Empty;

        public AqSerialPort()
        {
            
        }

        public bool Open()
        {
            try
            {
                _serialPort.BaudRate = _baudRate;
                _serialPort.DataBits = _dataBits;
                _serialPort.Handshake = _handshake;
                _serialPort.Parity = _parity;
                _serialPort.PortName = _portName;
                _serialPort.StopBits = _stopBits;
                _serialPort.DataReceived += _serialPort_DataReceived;

                _serialPort.Open();
            }
            catch { return false; }
            return true;
        }

        void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //Initialize a buffer to hold the received data 
            byte[] buffer = new byte[_serialPort.ReadBufferSize];

            //There is no accurate method for checking how many bytes are read 
            //unless you check the return from the Read method 
            int bytesRead = _serialPort.Read(buffer, 0, buffer.Length);

            //For the example assume the data we are received is ASCII data. 
            _tString += Encoding.ASCII.GetString(buffer, 0, bytesRead);
            //Check if string contains the terminator  
            if (_tString.IndexOf((char)_terminator) > -1)
            {
                //If tString does contain terminator we cannot assume that it is the last character received 
                string workingString = _tString.Substring(0, _tString.IndexOf((char)_terminator));
                //Remove the data up to the terminator from tString 
                _tString = _tString.Substring(_tString.IndexOf((char)_terminator));
                //Do something with workingString 
                Console.WriteLine(workingString);
            }
        }

    }
}
