﻿using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using Inhouse.Sdk.Logger;

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

        const short MaxSize = 255;
        #endregion

        private readonly SerialPort _serialPort = new SerialPort();
        readonly byte[] _buffer = new byte[MaxSize];
        int _index;
        short _state;
        private readonly Thread _read;


        private DateTime _lastSent;

        public Slip()
        {
            _state = Idle;
            _index = 0;
            _serialPort.DataBits = 8;
            _serialPort.Handshake = Handshake.None;
            _serialPort.Parity = Parity.None;
            _serialPort.StopBits = StopBits.One;
            //_serialPort.DataReceived += serialPort_DataReceived;
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;
            _lastSent = DateTime.Now;
            _read = new Thread(Read);
        }

        public void Begin(string portName, int boudRate)
        {
            _serialPort.PortName = portName;
            _serialPort.BaudRate = boudRate;
            _serialPort.Open();
            _read.Start();
        }

        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            List<byte> b = new List<byte>();

            while (_serialPort.BytesToRead > 0)
            {
                var recv = (byte)_serialPort.ReadByte();
                b.Add(recv);

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
                    {
                        LogProviderManager.Logger.Log(LogType.error, "CRC Fail");
                        return;
                    }

                    LogProviderManager.Logger.Log(LogType.debug,
                        " R -> " + string.Join(" ", b.Select(a => a.ToString("X2"))));
                    Receive?.Invoke(this, new DataReceivedEventArgs(buf.Take(buf.Length - 2).ToArray()));
                }
                else
                {
                    _buffer[_index++] = (byte) recv;
                }

            }
        }

        private void Read()
        {
            var b = new List<byte>();

            while (_serialPort.IsOpen)
            {
                while (_serialPort.BytesToRead > 0)
                {
                    var recv = (byte) _serialPort.ReadByte();

                    if (Idle == _state)
                    {
                        if (recv == End)
                        {
                            _index = 0;
                            _state = Receiving;
                            continue;
                        }
                    }

                    if (_state != Receiving)
                    {
                        b.Clear();
                        break;
                    }


                    if (_index >= MaxSize - 1)
                    {
                        _state = Idle;
                        b.Clear();
                        break;
                    }

                    if (recv == End)
                    {

                        if (b.Count == 0)
                        {
                            b.Clear();
                            break;
                        }

                        _state = Idle;

                        if (!CheckCrc(b.ToArray()))
                        {
                            LogProviderManager.Logger.Log(LogType.error, "CRC Fail");
                            b.Clear();
                            break;
                        }

                        LogProviderManager.Logger.Log(LogType.debug,
                            " R -> " + string.Join(" ", b.Select(a => a.ToString("X2"))));

                        Receive?.Invoke(this, new DataReceivedEventArgs(b.Take(b.Count - 2).ToArray()));
                        b.Clear();
                    }
                    else
                    {
                        b.Add(recv);
                    }

                }
            }
        }

        public void Send(byte[] data)
        {

            if (_serialPort.IsOpen)
            {

                var crc = CalculateCrc(data);
                var crcbuffer = new byte[2];
                crcbuffer[0] = (byte) crc;
                crcbuffer[1] = (byte) (crc >> 8);
                var newData = new byte[data.Length + 2];
                Buffer.BlockCopy(data, 0, newData, 0, data.Length);

                newData[newData.Length - 2] = crcbuffer[0];
                newData[newData.Length - 1] = crcbuffer[1];

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

                LogProviderManager.Logger.Log(LogType.debug,
                    " W -> " + string.Join(" ", buffer.Select(a => a.ToString("X2"))));

                while (DateTime.Now.Subtract(_lastSent).Milliseconds < 50)
                    Thread.Sleep(10);
                
                _serialPort.Write(buffer.Select(c => c).ToArray(), 0, buffer.Count);
                _lastSent = DateTime.Now;
            }
            else 
                throw new Exception("is not Open");

        }

        public static int CalculateCrc(byte[] data)
        {
            int crc = 0;
            var size = data.Length;
            var index = 0;

            while (--size >= 0)
            {
                crc = (crc ^ data[index++] << 8) & 0xFFFF;
                var i = 8;
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

        public void Close()
        {
            _serialPort.Close();
            
        }
    }
}
