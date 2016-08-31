using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;

namespace Aquila.Protocol.Bridge
{

    public class Slip
    {

        public event DataReceivedEventHandler Receive;

        #region Const
        const byte End = 0xC0;
        const byte Esc = 0xDB;
        const byte EscEnd = 0xDC;
        const byte EscEsc = 0xDD;

        const short Idle = 0;
        const short Receiving = 1;
        const short Error = 2;

        const short MaxSize = 255;
        #endregion

        private readonly SerialPort _serialPort = new SerialPort();
        readonly byte[] _buffer = new byte[MaxSize];
        int _index;
        short _state;

        public Slip()
        {
            _state = Idle;
            _index = 0;
            _serialPort.DataBits = 8;
            _serialPort.Handshake = Handshake.None;
            _serialPort.Parity = Parity.None;
            _serialPort.StopBits = StopBits.One;
            _serialPort.DataReceived += serialPort_DataReceived;
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;
        }

        public void Begin(string portName, int boudRate)
        {
            _serialPort.PortName = portName;
            _serialPort.BaudRate = boudRate;
            _serialPort.Open();
        }

        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            while (_serialPort.BytesToRead > 0)
            {
                var recv = (char)_serialPort.ReadByte();

                if (Idle == _state)
                {
                    if (recv == End)
                    {
                        _index = 0;
                        _state = Receiving;
                        continue;
                    }
                }

                if (_state != Receiving) return;

                if (_index >= MaxSize - 1)
                {
                    _state = Idle;
                    return;
                }

                if (recv == End)
                {

                    var buf = new byte[_index];
                    Buffer.BlockCopy(_buffer, 0, buf, 0, _index);


                    if (_index == 0)
                        return;

                    _state = Idle;

                    if (!CheckCrc(buf))
                        Console.WriteLine("CRC FAIL");
                    
                    Receive?.Invoke(this, new DataReceivedEventArgs(buf.Take(buf.Length - 2).ToArray()));
                }
                else
                {
                    _buffer[_index++] = (byte) recv;
                }

            }
        }

        public void Send(byte[] data)
        {

            /*foreach (var item in data)
                Console.Write(((int)item).ToString("X") + " ");
            Console.WriteLine("");*/

            if (_serialPort.IsOpen)
            {

                var crc = CalculateCrc(data);
                var crcbuffer = new byte[2];
                crcbuffer[0] = (byte) crc;
                crcbuffer[1] = (byte) (crc >> 8);
                var newData = new byte[data.Length + 2];
                Buffer.BlockCopy(data, 0, newData, 0, data.Length);

                newData[newData.Length - 2] = (byte) crcbuffer[0];
                newData[newData.Length - 1] = (byte) crcbuffer[1];

                var buffer = new List<byte> {End};

                foreach (var t in newData)
                {
                    switch (t)
                    {
                        case End:
                            buffer.Add(Esc);
                            buffer.Add(EscEnd);
                            break;
                        case Esc:
                            buffer.Add(Esc);
                            buffer.Add(EscEsc);
                            break;
                        default:
                            buffer.Add(t);
                            break;
                    }
                }
                buffer.Add(End);

                _serialPort.Write(buffer.Select(c => (byte)c).ToArray(), 0, buffer.Count);
            }
            else 
                throw new Exception("is not Open");

        }

        public static int CalculateCrc(byte[] data)
        {
            int crc = 0;
            var size = data.Length;
            var i = 0;
            var index = 0;

            while (--size >= 0)
            {
                crc = (crc ^ data[index++] << 8) & 0xFFFF;
                i = 8;
                do
                {
                    if ((crc & 0x8000) == 0x8000)
                    {
                        crc = (crc << 1 ^ 0x1021) & 0xFFFF;
                    }
                    else
                    {
                        crc = (crc << 1) & 0xFFFF;
                    }
                } while (--i > 0);
            }

            return crc & 0xFFFF;
        }

        public static bool CheckCrc(byte[] data)
        {
            // Getting crc from packet
            var dataCrc = (data[data.Length - 1]) << 8;
            dataCrc |= (data[data.Length - 2]) & 0x00FF;

            // Calculating crc
            var calcdCrc = CalculateCrc(data.Take(data.Length - 2).ToArray());

            // Comparing
            return calcdCrc == dataCrc;
        }
    }
}
