using System;
using Aquila.Protocol.Device;

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

    public class NewDeviceReceivedEventArgs : EventArgs
    {
        internal NewDeviceReceivedEventArgs(int srcAddr,  byte[] euiAddr)
        {
            SrcAddr = srcAddr;
            EuiAddr = euiAddr;
        }

        public int SrcAddr { get; }
        public byte[] EuiAddr { get; }
    }

    public delegate void DataReceivedEventHandler(object sender, DataReceivedEventArgs e);
    public delegate void PackagesReceivedEventHandler(object sender, PackagesReceivedEventArgs e);
    public delegate void NewDeviceReceivedEventHandler(object sender, NewDeviceReceivedEventArgs e);
}