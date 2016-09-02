using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Inhouse.Sdk.Logger;

namespace Aquila.Protocol.Bridge
{
    public class Mesh
    {
        #region Const 
        //Commands
        const int DefaultPan = 0xCA5A;
        const byte DefaultChannel = 26;
        const byte EndPoint = 15;
        const byte CmdGetEui = 0;
        const byte CmdResetEui = 1;
        const int PingTimeOut = 1000;
        //
        public const int WaitKeepLive = 60; //seconds
        public const byte MaxPayLoad = 105;
        public const int BroadCast = 0xFFFF;
        #endregion

        private readonly Bridge _bridge;
        private bool _securityEnabled;
        private int _localAddr;
        private byte[] _localEuiAddr;
        private int _pan;
        private byte _channel;
        private Thread _keepLive;

        public int ShortAddress => _localAddr;

        public event PackagesReceivedEventHandler Receive;
        public event NewDeviceReceivedEventHandler NewDevice;
        public Mesh()
        {
            _bridge = new Bridge();
            _bridge.Receive += _bridge_Receive;
            _localAddr = 0x00FF;
            _localEuiAddr = new List<byte>() {0x01, 0x02, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0}.ToArray();
            _securityEnabled = false;
            _pan = DefaultPan;
            _channel = DefaultChannel;
            _keepLive = new Thread(KeepLive);

            LogProviderManager.Logger.Log(LogType.debug, "Init Mesh");
        }
        
        //recepcion de paketes 00:00:00:00:00:00:00
        private void _bridge_Receive(object sender, PackagesReceivedEventArgs e)
        {
            LogProviderManager.Logger.Log(LogType.debug, "Message Receive");

            if (e.Packet.Frame.Length <= 0) return;

            if (e.Packet.Frame[0] == CmdGetEui)
            {
                Announce(e.Packet.SrcAddr);
            }
            else if (e.Packet.Frame[0] == CmdResetEui && e.Packet.Frame.Length >= 9)
            {
                var euiAddr = e.Packet.Frame.Skip(1).Take(8).ToArray();
                NewDevice?.Invoke(this, new NewDeviceReceivedEventArgs(e.Packet.SrcAddr, euiAddr));
            }
            else
            {
                Receive?.Invoke(this, new PackagesReceivedEventArgs(e.Packet));
            }
        }

        //Keep Live Thread
        private void KeepLive()
        {
            while (_bridge.IsReady)
            {
                //try send ping 
                Ping(Mesh.BroadCast);

                //time wait
                for(int i=0 ;i< WaitKeepLive;i++)
                {
                    Thread.Sleep(1000);

                    if(!_bridge.IsReady)
                        break;
                }
            }
        }

        private void Announce(int destination)
        {
            var data = new List<byte>() {CmdResetEui};
            data.AddRange(_localEuiAddr);
            
            var p = new Packet()
            {
                Lqi = 0xff,
                Rssi = 0xff,
                SrcAddr = _localAddr,
                DstAddr = destination,
                SrcEndPoint = EndPoint,
                DstEndPoint = EndPoint,
                Size = data.Count,
                Frame = data.ToArray()
            };

            _bridge.SendData(p);
        }

        public void Ping(int destination)
        {
            var p = new Packet()
            {
                Lqi = 0xff,
                Rssi = 0xff,
                SrcAddr = _localAddr,
                DstAddr = destination,
                SrcEndPoint = EndPoint,
                DstEndPoint = EndPoint,
                Size = 1,
                Frame = new List<byte>() { CmdGetEui }.ToArray()
            };

            _bridge.SendData(p);
        }

        public void Begin(string port, int baudrate)
        {
            _bridge.Begin(port, baudrate);

            var start = DateTime.Now;

            while (!_bridge.IsReady)
            {
                Thread.Sleep(10);

                if (DateTime.Now.Subtract(start).Milliseconds > PingTimeOut)
                    throw new Exception("Time Out in Bridge");
            }

            _localEuiAddr = _bridge.LongAddress;
            _keepLive.Start();
        }

        public void SendPacket(Packet packet)
        {
            _bridge.SendData(packet);
        }
        
        public void Close()
        {
            _bridge.Close();
        }
    }
}
