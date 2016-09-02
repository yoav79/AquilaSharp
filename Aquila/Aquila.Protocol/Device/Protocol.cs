using System.Reflection.Emit;
using Aquila.Protocol.Bridge;

namespace Aquila.Protocol.Device
{
    public class Protocol
    {
        #region Const

        const byte EndPoint = 13;
        const byte Version = Packet.ProtocolVersion;

        const byte Nack = 0;
        const byte Ack = 1;
        const byte Action = 2;
        const byte Get = 3;
        const byte Post = 4;
        const byte Custom = 5;

        public const byte ComNAction = 0;
        public const byte ComNEvents = 1;
        public const byte ComClass = 2;
        public const byte ComSize = 3;
        public const byte ComNEntries = 4;
        public const byte ComAbsEntry = 5;
        public const byte ComEntry = 6;
        public const byte ComClear = 7;
        public const byte ComAddEntry = 8;
        public const byte ComDelAbsEntry = 9;
        public const byte ComDelEntry = 10;
        public const byte ComAction = 11;
        public const byte ComEvent = 12;
        public const byte ComName = 13;
        public const byte ComEui = 14;

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
            _mesh.Begin("COM3", 57600);
        }

        public Packet Parse(Bridge.Packet packet)
        {
            var pkt = new Packet();

            if (packet.SrcEndPoint != EndPoint || packet.DstEndPoint != EndPoint)
                return null;

            pkt.SrcAddr = packet.SrcAddr;
            pkt.DstAddr = packet.DstAddr;

            pkt.FromRaw(packet.Frame);

            return pkt;
        }

        private void Send(Packet packet)
        {
            var data = packet.GetRaw();

            var bp = new Bridge.Packet();
            bp.Lqi = 0xff;
            bp.Rssi = 0xff;
            bp.SrcAddr = packet.SrcAddr;
            bp.DstAddr = packet.DstAddr;
            bp.SrcEndPoint = EndPoint;
            bp.DstEndPoint = EndPoint;
            bp.Size = packet.GetRaw().Length;
            bp.Frame = packet.GetRaw();

            _mesh.SendPacket(bp);
        }


        private void SendAck(int dstAddr)
        {
        }

        private void SendNack(int dstAddr)
        {
        }


        public void Ping(int dstAddr)
        {
            _mesh.Ping(dstAddr);
        }

        public void RequestAction(int address, byte action, byte parameter)
        {
            var p = new Packet
            {
                DstAddr = address,
                SrcAddr = _mesh.ShortAddress,
                Control = new Control(0) {CommandType = Packet.CmdAction}
            };

            if (parameter > 0)
            {
                p.Control.HasParameter = true;
                p.Parameter = parameter;
            }
            p.Command = action;

            Send(p);
        }


        public void RequestGet(int address, byte action, byte parameter, byte[] data)
        {
            Request(false, address, action, parameter, data);
        }

        public void RequestPost(int address, byte action, byte parameter, byte[] data)
        {
            Request(true, address, action, parameter, data);
        }

        public void RequestCustom(int address, byte[] data)
        {
            var p = new Packet
            {
                DstAddr = address,
                SrcAddr = _mesh.ShortAddress,
                Control = new Control(0) {CommandType = Packet.CmdCustom}
            };

            if (data.Length > 0)
            {
                p.Control.HasData = true;
                p.Data = data;
            }

            Send(p);
        }

        private void Request(bool isPost, int address, byte action, byte parameter, byte[] data)
        {
            var p = new Packet
            {
                DstAddr = address,
                SrcAddr = _mesh.ShortAddress,
                Control = new Control(0) {CommandType = isPost ? Packet.CmdPost : Packet.CmdGet}
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

    }
}