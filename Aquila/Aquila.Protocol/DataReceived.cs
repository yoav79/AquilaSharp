using System;
using System.IO.Ports;

namespace Aquila.Protocol
{
    public class DataReceivedEventArgs : EventArgs
    {
        internal byte[] _data;

        internal DataReceivedEventArgs(byte[] data)
        {
            _data = data;
        }

        public byte[] Data => _data;
    }

    public delegate void DataReceivedEventHandler(object sender, DataReceivedEventArgs e);
}