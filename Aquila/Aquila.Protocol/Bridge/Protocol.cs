using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Aquila.Protocol.Device;
using Inhouse.Sdk.Logger;

namespace Aquila.Protocol.Bridge
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
            DeviceManager.Instance.AddNew(e.SrcAddr, e.EuiAddr);
        }

        private void _mesh_Receive(object sender, PackagesReceivedEventArgs e)
        {
            var ppk = Parse(e.Packet);

            var device = DeviceManager.Instance.Devices.FirstOrDefault(a => a.ShortAddress == e.Packet.SrcAddr) ??
                         DeviceManager.Instance.AddNew(e.Packet.SrcAddr);

            if ((CommandTypes) ppk.Control.CommandType == CommandTypes.Nack ||
                (CommandTypes)ppk.Control.CommandType == CommandTypes.Ack)
            {
                return;
            }
            else
            {
                if ((CommandTypes) ppk.Control.CommandType == CommandTypes.Event)
                {
                    LogProviderManager.Logger.LogObject(LogType.warning, "", e.Packet);
                    LogProviderManager.Logger.LogObject(LogType.warning, "", ppk);


                    LogProviderManager.Logger.Log(LogType.warning,
                        "" + string.Join(" ", ppk.Data.Take(8).ToArray().Select(a => a.ToString("X2"))));
                    LogProviderManager.Logger.Log(LogType.warning, Encoding.Default.GetString(ppk.Data.Skip(9).ToArray()));

                    RequestAction(e.Packet.SrcAddr, 0, 0);
                    return;
                }
                if ((CommandTypes) ppk.Control.CommandType == CommandTypes.Post)
                {

                    switch ((Commands) ppk.Command)
                    {
                        case Commands.Eui:
                            device.Address = ppk.Data;
                            LogProviderManager.Logger.Log(LogType.warning,
                                "" + string.Join(" ", device.Address.Select(a => a.ToString("X2"))));
                            break;
                        case Commands.Name:
                            device.Name = Encoding.Default.GetString(ppk.Data);
                            LogProviderManager.Logger.Log(LogType.warning, Encoding.Default.GetString(ppk.Data));
                            break;
                        case Commands.Class:
                            device.Class = Encoding.Default.GetString(ppk.Data);
                            LogProviderManager.Logger.Log(LogType.warning, Encoding.Default.GetString(ppk.Data));
                            break;
                        case Commands.NAction:
                            device.ActionsCount = ppk.Data[0];
                            LogProviderManager.Logger.Log(LogType.warning, device.ActionsCount.ToString());
                            for (var i = 0; i < device.ActionsCount; i++)
                                RequestGet(e.Packet.SrcAddr, (byte) Commands.Action, (byte) i, new List<byte>().ToArray());
                            break;
                        case Commands.Action:
                            if (ppk.Parameter != null)
                                if (!device.Actions.ContainsKey(ppk.Parameter.Value))
                                    device.Actions.Add((int) ppk.Parameter.Value, Encoding.Default.GetString(ppk.Data));
                                else
                                    device.Actions[ppk.Parameter.Value] = Encoding.Default.GetString(ppk.Data);

                            LogProviderManager.Logger.Log(LogType.warning, Encoding.Default.GetString(ppk.Data));
                            break;
                        case Commands.NEvents:
                            device.EventCount = ppk.Data[0];
                            LogProviderManager.Logger.Log(LogType.warning, device.EventCount.ToString());
                            for (var i = 0; i < device.EventCount; i++)
                                RequestGet(e.Packet.SrcAddr, (byte) Commands.Event, (byte) i, new List<byte>().ToArray());
                            break;
                        case Commands.Event:
                            if (ppk.Parameter != null)
                                if (!device.Events.ContainsKey(ppk.Parameter.Value))
                                    device.Events.Add((int) ppk.Parameter.Value, Encoding.Default.GetString(ppk.Data));
                                else
                                    device.Events[ppk.Parameter.Value] = Encoding.Default.GetString(ppk.Data);
                            LogProviderManager.Logger.Log(LogType.warning, Encoding.Default.GetString(ppk.Data));
                            break;
                        case Commands.Size:
                        case Commands.NEntries:
                        case Commands.AbsEntry:
                        case Commands.Entry:
                        case Commands.Clear:
                        case Commands.AddEntry:
                        case Commands.DelAbsEntry:
                        case Commands.DelEntry:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            LogProviderManager.Logger.Log(LogType.warning, ((Commands)ppk.Command).ToString());
            LogProviderManager.Logger.LogObject(LogType.warning, "", e.Packet);
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
                Lqi = 0xff, Rssi = 0xff, SrcAddr = packet.SrcAddr, DstAddr = packet.DstAddr, SrcEndPoint = EndPoint, DstEndPoint = EndPoint, Size = packet.GetRaw().Length, Frame = packet.GetRaw()
            };
            _mesh.SendPacket(bp);
        }

        public void SendAck(int dstAddr)
        {
            var ppk = new ProtoPacket
            {
                DstAddr = dstAddr, SrcAddr = _mesh.ShortAddress, Control = new Control(0) {CommandType = (byte) CommandTypes.Ack}
            };

            Send(ppk);
        }

        public void SendNack(int dstAddr)
        {
            var ppk = new ProtoPacket
            {
                DstAddr = dstAddr, SrcAddr = _mesh.ShortAddress, Control = new Control(0) {CommandType = (byte) CommandTypes.Nack}
            };

            Send(ppk);
        }

        public void Ping(int dstAddr)
        {
            _mesh.Ping(dstAddr);
        }

        public void RequestAction(int dstAddr, byte action, byte? parameter)
        {
            var p = new ProtoPacket
            {
                DstAddr = dstAddr, SrcAddr = _mesh.ShortAddress, Control = new Control() {CommandType = (byte) CommandTypes.Action}
            };

            if (parameter.HasValue)
            {
                p.Control.HasParameter = true;
                p.Parameter = parameter.Value;
            }
            p.Command = action;

            Send(p);
        }

        public void RequestGet(int dstAddr, byte action, byte? parameter, byte[] data)
        {
            Request(false, dstAddr, action, parameter, data);
        }

        public void RequestPost(int dstAddr, byte action, byte? parameter, byte[] data)
        {
            Request(true, dstAddr, action, parameter, data);
        }

        public void RequestCustom(int dstAddr, byte[] data)
        {
            var p = new ProtoPacket
            {
                DstAddr = dstAddr, SrcAddr = _mesh.ShortAddress, Control = new Control(0) {CommandType = (byte) CommandTypes.Custom}
            };

            if (data.Length > 0)
            {
                p.Control.HasData = true;
                p.Data = data;
            }

            Send(p);
        }

        private void Request(bool isPost, int dstAddr, byte action, byte? parameter, byte[] data)
        {
            var p = new ProtoPacket
            {
                DstAddr = dstAddr, SrcAddr = _mesh.ShortAddress, Control = new Control() {CommandType = isPost ? (byte) CommandTypes.Post : (byte) CommandTypes.Get}
            };

            if (parameter.HasValue)
            {
                p.Control.HasParameter = true;
                p.Parameter = parameter.Value;
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
            RequestGet(srcAddr, (byte) Commands.Eui, 0, new List<byte>().ToArray());
            RequestGet(srcAddr, (byte) Commands.Class, 0, new List<byte>().ToArray());
            RequestGet(srcAddr, (byte) Commands.Name, 0, new List<byte>().ToArray());
            RequestGet(srcAddr, (byte) Commands.NAction, 0, new List<byte>().ToArray());
            RequestGet(srcAddr, (byte) Commands.NEvents, 0, new List<byte>().ToArray());
     
            
            RequestAction(srcAddr,1,0);
            
            
        }
    }
}