using System.Collections.Generic;
using System.Text;
using System.Threading;
using Aquila.Protocol.Bridge;
using Inhouse.Sdk.Logger;

namespace Aquila.Protocol.Device
{
    public class Protocol
    {
        #region Const
        const byte EndPoint = 13;
        const byte Version = ProtoPacket.ProtocolVersion;
        #endregion

        private static readonly object SyncRoot = new object();
        private static Protocol _instance = new Protocol();
        private Mesh _mesh;

        public static Protocol Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (_instance == null)
                            _instance = new Protocol();
                    }
                }

                return _instance;
            }
        }

        private Protocol()
        {
            
        }

        public void Start()
        {
            if (_mesh != null) return;

            _mesh = new Mesh();
            _mesh.Receive += _mesh_Receive;
            _mesh.NewDevice += _mesh_NewDevice;
            _mesh.Begin("COM3", 57600);
        }

        private void _mesh_NewDevice(object sender, NewDeviceReceivedEventArgs e)
        {
            DeviceFetcher(e.SrcAddr, e.EuiAddr);
        }

        private void _mesh_Receive(object sender, PackagesReceivedEventArgs e)
        {
            var ppk = Parse(e.Packet);

            LogProviderManager.Logger.Log(LogType.warning, ((Commands)ppk.Command).ToString());


            if (ppk.Data!= null)
            LogProviderManager.Logger.Log(LogType.warning, Encoding.Default.GetString(ppk.Data));

        }

        public ProtoPacket Parse(Packet packet)
        {
            var pkt = new ProtoPacket();

            if (packet.SrcEndPoint != EndPoint || packet.DstEndPoint != EndPoint)
                return null;

            pkt.SrcAddr = packet.SrcAddr;
            pkt.DstAddr = packet.DstAddr;
            pkt.FromRaw(packet.Frame);

            return pkt.Version != Version ? null : pkt;
        }

        private void Send(ProtoPacket packet)
        {
            var bp = new Packet
            {
                Lqi = 0xff,
                Rssi = 0xff,
                SrcAddr = packet.SrcAddr,
                DstAddr = packet.DstAddr,
                SrcEndPoint = EndPoint,
                DstEndPoint = EndPoint,
                Size = packet.GetRaw().Length,
                Frame = packet.GetRaw()
            };
            _mesh.SendPacket(bp);
        }

        public void SendAck(int dstAddr)
        {
            var ppk = new ProtoPacket
            {
                DstAddr = dstAddr,
                SrcAddr = _mesh.ShortAddress,
                Control = new Control(0) {CommandType = (byte)CommandTypes.Ack}
            };

            Send(ppk);
        }

        public void SendNack(int dstAddr)
        {
            var ppk = new ProtoPacket
            {
                DstAddr = dstAddr,
                SrcAddr = _mesh.ShortAddress,
                Control = new Control(0) { CommandType = (byte)CommandTypes.Nack }
            };

            Send(ppk);
        }

        public void Ping(int dstAddr)
        {
            _mesh.Ping(dstAddr);
        }

        public void RequestAction(int dstAddr, byte action, byte parameter)
        {
            var p = new ProtoPacket
            {
                DstAddr = dstAddr,
                SrcAddr = _mesh.ShortAddress,
                Control = new Control(0) {CommandType = (byte)CommandTypes.Action }
            };

            if (parameter > 0)
            {
                p.Control.HasParameter = true;
                p.Parameter = parameter;
            }
            p.Command = action;

            Send(p);
        }
        
        public void RequestGet(int dstAddr, byte action, byte parameter, byte[] data)
        {
            Request(false, dstAddr, action, parameter, data);
        }

        public void RequestPost(int dstAddr, byte action, byte parameter, byte[] data)
        {
            Request(true, dstAddr, action, parameter, data);
        }

        public void RequestCustom(int dstAddr, byte[] data)
        {
            var p = new ProtoPacket
            {
                DstAddr = dstAddr,
                SrcAddr = _mesh.ShortAddress,
                Control = new Control(0) {CommandType = (byte)CommandTypes.Custom }
            };

            if (data.Length > 0)
            {
                p.Control.HasData = true;
                p.Data = data;
            }

            Send(p);
        }

        private void Request(bool isPost, int dstAddr, byte action, byte parameter, byte[] data)
        {
            var p = new ProtoPacket
            {
                DstAddr = dstAddr,
                SrcAddr = _mesh.ShortAddress,
                Control = new Control() {CommandType = isPost ? (byte)CommandTypes.Post : (byte)CommandTypes.Get}
            };

            if (parameter > 0)
            {
                p.Control.HasParameter = true;
                p.Parameter = parameter;
            }

            if (data.Length > 0)
            {
                p.Control.HasData = true;
                p.Data = data;
            }
            p.Command = action;

            Send(p);
        }

        public void DeviceFetcher(int srcAddr, IEnumerable<byte> euiAddr)
        {
            RequestGet(srcAddr, (byte)Commands.Class, 0, new List<byte>().ToArray());
            RequestGet(srcAddr, (byte)Commands.Name, 0, new List<byte>().ToArray());
            RequestGet(srcAddr, (byte)Commands.Eui, 0, new List<byte>().ToArray());

            RequestGet(srcAddr, (byte)Commands.Size, 0, new List<byte>().ToArray());
            RequestGet(srcAddr, (byte)Commands.Clear, 0, new List<byte>().ToArray());

            RequestGet(srcAddr, (byte)Commands.AbsEntry, 0, new List<byte>().ToArray());

            

            RequestGet(srcAddr, (byte)Commands.NEntries, 0, new List<byte>().ToArray());
            RequestGet(srcAddr, (byte)Commands.NAction, 1, new List<byte>().ToArray());
            RequestGet(srcAddr, (byte)Commands.NEvents, 1, new List<byte>().ToArray());

            RequestGet(srcAddr, (byte)Commands.Action, 1, new List<byte>().ToArray());

            RequestGet(srcAddr, (byte)Commands.Event, 1, new List<byte>().ToArray());


        }
    }
}