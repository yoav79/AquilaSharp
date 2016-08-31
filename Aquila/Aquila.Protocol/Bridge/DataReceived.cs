using System;

namespace Aquila.Protocol.Bridge
{
    public class DataReceivedEventArgs : EventArgs
    {
        internal DataReceivedEventArgs(byte[] data)
        {
            Data = data;
        }

        public byte[] Data { get; }
    }

    public class PackagesReceivedEventArgs : EventArgs
    {
        internal PackagesReceivedEventArgs(Packet packet)
        {
            Packet = packet;
        }

        public Packet Packet { get; }
    }
    
    public delegate void DataReceivedEventHandler(object sender, DataReceivedEventArgs e);
    public delegate void PackagesReceivedEventHandler(object sender, PackagesReceivedEventArgs e);
}