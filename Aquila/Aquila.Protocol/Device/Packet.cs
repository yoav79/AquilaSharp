using System.Collections.Generic;
using System.Linq;

namespace Aquila.Protocol.Device
{
    public class Packet
    {
        public const byte CmdNack = 0x00;
        public const byte CmdAck   = 0x01;
        public const byte CmdAction    = 0x02;
        public const byte CmdGet   = 0x03;
        public const byte CmdPost  = 0x04;
        public const byte CmdCustom    = 0x05;
        public const byte CmdEvent = 0x06;
        public const byte ProtocolVersion= 3;

        private byte _version;
        
        public Packet()
        {
            SrcAddr = 0xFFFF;
            DstAddr = 0xFFFF;
            Control = null;
            Parameter = 0;
        }

        public int SrcAddr { get; set; }

        public int DstAddr { get; set; }

        public byte Parameter { get; set; }

        public Control Control { get; set; }
        public byte Command { get; set; }
        public byte[] Data { get; set; }

        public void FromRaw(byte[] frame)
        {
            _version = frame[0];
            Control = new Control(frame[1]);

            if (Control.CommandType == CmdAction ||
                Control.CommandType == CmdGet ||
                Control.CommandType == CmdPost ||
                Control.CommandType == CmdEvent
                )
            {
                Command = frame[2];

                if (Control.HasParameter)
                {
                    Parameter = frame[3];
                }

                if (Control.HasData)
                {
                    Data = frame.Skip(3 + Parameter != 0 ? 1 : 0).ToArray();
                }
            }
        }

        public byte[] GetRaw()
        {
            var bytes = new List<byte> {_version, Control.GetRaw(), Command, Parameter};
            if (Data != null && Data.Length > 0)
                bytes.AddRange(Data);

            return bytes.ToArray();
        }
    }
}