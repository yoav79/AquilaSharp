using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;

namespace Aquila.Protocol
{
    public class SerialTransport
    {

        private readonly SerialPort _serialPort = new SerialPort();
        public event SerialDataReceivedEventHandler Receive;


        private int _dataBits = 8;
        private Handshake _handshake = Handshake.None;
        private Parity _parity = Parity.None;
        private StopBits _stopBits = StopBits.One;


        public SerialTransport(string portName, int boudRate)
        {
            _serialPort.BaudRate = boudRate;
            _serialPort.DataBits = _dataBits;
            _serialPort.Handshake = _handshake;
            _serialPort.Parity = _parity;
            _serialPort.PortName = portName;
            _serialPort.StopBits = _stopBits;
            _serialPort.DataReceived += _serialPort_DataReceived;
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;
        }

        public bool IsConnected { get; set; }

        public bool Open()
        {
            try
            {
                _serialPort.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception Received:" + e.Message);
                return false;
            }
            return true;
        }

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Receive?.Invoke(_serialPort, e);
        }

        public void Close()
        {
            _serialPort.Close();
        }

        public void Write(byte[] data)
        {
            _serialPort.Write(data, 0, data.Length);
        }

        public void Write(char data)
        {
            _serialPort.Write(new List<char>() {data}.ToArray(), 0, 1);
        }
    }
}
