using System;
using System.Collections.Generic;
using System.Linq;

namespace Aquila.Protocol.Bridge
{
    /*
     *  802.15.4 Dongle interface utility.
     *  Transfers 802.15.4 LWM packet frames to and from a USB Dongle via a serial port bridge.
     *
     *  Author: Rodrigo Méndez Gamboa, rmendez@makerlab.mx
     *
     *  Update: 24/04/15: Changed encapsulation protocol to SLIP (RFC 1055)(http://en.wikipedia.org/wiki/Serial_Line_Internet_Protocol),
     *                    added 16bit CRC to end of packet
     *
     *  Serial packet: [SLIP END (0xC0)][DATA (SLIP Escaped 0xDB)][SLIP END (0xC0)]
     *  DATA packet: [Command][Command specific][CRC (16bit)]
     *
     *  Comands:
     *      CMD_DATA:           [Command specific] = lqi rssi srcAddr(16) dstAddr(16) srcEndpoint dstEndpoint frameSize [MAC Frame]
     *      CMD_ACK:            [Command specific] = RSSI                                               *Sent on data transmit success
     *      CMD_NACK:           [Command specific] = CAUSE                                              *Sent on data tranmit error
     *      CMD_GET_OPT:        [Command specific] = PROM* PAN_LOW PAN_HIGH CHAN**  ADDR_LOW ADDR_HIGH  *Returns the current options
     *      CMD_SET_OPT:        [Command specific] = PROM* PAN_LOW PAN_HIGH CHAN**  ADDR_LOW ADDR_HIGH  *0 or 1, **11-26
     *      CMD_GET_SECURITY:   [Command specific] = ENABLED                                            *Get if security enabled
     *      CMD_SET_SECURITY:   [Command specific] = ENABLED [SEC_KEY](16 byte, 128 bit)                *Set if security enabled and key
     *      CMD_START                                                                                   *Sent on bridge start or reset and in response to CMD_PING
     *      CMD_PING:                                                                                   *Sent by PC, response is CMD_START
     *      CMD_GET_LONG_ADDR:                                                                          *Get bridge MAC address
     */


    /*
     *  Communication Secuence Diagrams:
     *
     *  PC  | CMD_PING  ------> | Bridge
     *      | <------ CMD_START |
     *      |                   |
     *
     *  PC  | CMD_DATA  ------------------> | Bridge
     *      | <-- CMD_ACK or CMD_NACK       |
     *      |                               |
     *
     *  PC  | CMD_SET_* ------------------> | Bridge
     *      | <--- CMD_SET_* (Confirmation) |
     *      |                               |
     *
     *  On Data reception:
     *  PC  | <------ CMD_DATA  | Bridge
     *      |                   |
     *
     *  On Bridge Startup:
     *  PC  | <------ CMD_START | Bridge
     *      |                   |
     *
     */

    public class Bridge
    {
        #region Const 
        
        //Commands
        const byte CmdData = 0;
        const byte CmdAck = 1;
        const byte CmdNack = 2;
        const byte CmdGetOpt = 3;
        const byte CmdSetOpt = 4;
        const byte CmdGetSecurity = 5;
        const byte CmdSetSecurity = 6;
        const byte CmdStart = 7;
        const byte CmdPing = 8;
        const byte CmdGetLongAddr = 9;
        const byte CmdSetLongAddr = 10;

        // index CMD
        const byte Command = 0;
        const byte Lqi = 1;
        const byte Rssi = 2;
        const byte SrcAddrL = 3;
        const byte SrcAddrH = 4;
        const byte DstAddrL = 5;
        const byte DstAddrH = 6;
        const byte SrcEndPoint = 7;
        const byte DstEndPoint = 8;
        const byte Size = 9;
        const byte Data = 10;

        // index OPT
        const byte Prom = 1;
        const byte PanL = 2;
        const byte PanH = 3;
        const byte Channel = 4;
        const byte AddressL = 5;
        const byte AddressH = 6;

        // index Sec
        const byte Enable = 1;
        const byte Key = 2;
        #endregion
        
        private readonly Slip _slip;
        private bool _ready;
        private readonly List<byte> _longadrress;
        private readonly Security _currentSecurity;
        private readonly Options _currentOptions;

        public byte[] LongAddress => _longadrress.ToArray();

        public bool IsReady => _ready;

        public event PackagesReceivedEventHandler Receive;
        
        public Bridge()
        {
            _ready = false;
            _longadrress = new List<byte>() {1, 2, 0, 0, 0, 0, 0, 0};

            _currentOptions = new Options()
            {
                Prom = false,
                Pan = 0xCa5a,
                Channel = 26,
                Address = 0x00FF
            };

            _currentSecurity = new Security()
            {
                Enabled = false,
                Key = new byte[16]
            };

            _slip = new Slip();
            _slip.Receive += slip_Receive;
        }

        private void slip_Receive(object sender, DataReceivedEventArgs e)
        {
            Parse(e.Data);
        }

        public void Begin(string port, int baudrate)
        {
            _slip.Begin(port, baudrate);
            _slip.Send(new List<byte>() {CmdPing}.ToArray());
        }

        private void Parse(IReadOnlyList<byte> data)
        {
            if (data.Count == 0)
                return;

            switch (data[Command])
            {
                case CmdAck:
                case CmdGetSecurity:
                case CmdPing:
                case CmdGetLongAddr:
                case CmdGetOpt:
                    return;
                case CmdNack:
                   // throw new Exception("Nack");
                case CmdSetSecurity:
                    if (data.Count < 2) return;
                    _currentSecurity.Enabled = data[Enable] == 1;
                    break;
                case CmdSetLongAddr:
                    if (data.Count < 9) return;
                    _longadrress.Clear();
                    foreach (var b in data.Skip(1).Take(8))
                        _longadrress.Add(b);
                    break;
                case CmdStart:
                    _ready = true;
                    _slip.Send(new List<byte>() {CmdGetLongAddr}.ToArray());
                    break;
                case CmdSetOpt:
                    if (data.Count < 7) return;

                    _currentOptions.Prom = data[Prom] > 0;
                    _currentOptions.Pan = data[PanL] | data[PanH] << 8;
                    _currentOptions.Channel = data[Channel];
                    _currentOptions.Address = data[AddressL] | data[AddressH] << 8;
                    break;
                case CmdData:
                    if (data.Count < 10) return;

                    var p = new Packet()
                    {
                        Lqi = data[Lqi],
                        Rssi = data[Rssi],
                        SrcAddr = data[SrcAddrL] | data[SrcAddrH] << 8,
                        DstAddr = data[DstAddrL] | data[DstAddrH] << 8,
                        SrcEndPoint = data[SrcEndPoint],
                        DstEndPoint = data[DstEndPoint],
                        Size = data[Size],
                    };

                    if (p.Size > 0)
                        p.Frame = data.Skip(10).Take(Size).ToArray();
                    
                    Receive?.Invoke(this, new PackagesReceivedEventArgs(p));
                    break;
                default:
                    throw new Exception("CMD not found");
            }
        }

        public void Ping()
        {
            if (_ready)
                _slip.Send(new List<byte>() {CmdPing}.ToArray());
        }

        public void SetSecurity(Security sec)
        {
            if (!_ready) return;

            var payload = new List<byte> {CmdSetSecurity, sec.Enabled ? (byte) 1 : (byte) 0};
            payload.AddRange(sec.Key);

            _currentSecurity.Enabled = sec.Enabled;
            _currentSecurity.Key = sec.Key;

            _slip.Send(payload.ToArray());
        }

        public void SetOptions(Options opt)
        {
            if (!_ready) return;
            var payload = new List<byte>
            {
                CmdSetOpt,
                _currentOptions.Prom ? (byte) 1 : (byte) 0,
                (byte) ((byte) opt.Pan & 0xff),
                (byte) ((byte) opt.Pan >> 8 & 0xff),
                (byte) opt.Channel,
                (byte) ((byte) opt.Address & 0xff),
                (byte) ((byte) opt.Address >> 8 & 0xff)
            };

            _slip.Send(payload.ToArray());
        }

        public void GetOptions()
        {
            if (_ready)
                _slip.Send(new List<byte>() { CmdGetOpt }.ToArray());
        }

        public void GetSecurity()
        {
            if (_ready)
                _slip.Send(new List<byte>() {CmdGetSecurity}.ToArray());
        }

        public void GetLongAddress()
        {
            if (_ready)
                _slip.Send(new List<byte>() {CmdGetLongAddr}.ToArray());
        }

        public void SendData(Packet packet)
        {
            if (!_ready) return;
            var payload = new List<byte>
            {
                CmdData,
                (byte) packet.Lqi,
                (byte) packet.Rssi,
                (byte) ((byte) packet.SrcAddr & 0xff),
                (byte) ((byte) (packet.SrcAddr >> 8) & 0xff),
                (byte) ((byte) packet.DstAddr & 0xff),
                (byte) ((byte) (packet.DstAddr >> 8) & 0xff),
                (byte) packet.SrcEndPoint,
                (byte) packet.DstEndPoint,
                (byte) packet.Size,
            };

            payload.AddRange(packet.Frame);
            _slip.Send(payload.ToArray());
        }

        public void Close()
        {
            _slip.Close();
            _ready = false;
        }

    }
}
