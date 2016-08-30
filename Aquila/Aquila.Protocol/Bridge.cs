using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aquila.Protocol
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
        private object _transport;
        private bool _ready;
        private List<byte> _longadrress;
        private CurrentSecurity _currentSecurity;
        private CurrentOptions _currentOptions;
        private bool _waitingResponse;
        private List<byte> buffer;

        public Bridge(int baudrate, string port)
        {
            _ready = false;
            _longadrress = new List<byte>() {1, 2, 0, 0, 0, 0, 0, 0};

            _currentOptions = new CurrentOptions()
            {
                Prom = false,
                Pan = 0xCa5a,
                Channel = 26,
                Address = 0x00FF
            };

            _currentSecurity = new CurrentSecurity()
            {
                Enabled = false,
                Key = new byte[16]
            };

            //_transport = new Transport(baudrate, port);
        }
    }
}
