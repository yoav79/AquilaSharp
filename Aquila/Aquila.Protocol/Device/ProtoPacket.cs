using System.Collections.Generic;
using System.Linq;

namespace Aquila.Protocol.Device
{
    public class ProtoPacket
    {
        public const byte ProtocolVersion= 3;
        
        public ProtoPacket()
        {
            SrcAddr = 0xFFFF;
            DstAddr = 0xFFFF;
            Control = new Control();
            Version = ProtocolVersion;
            Command = (byte)CommandTypes.Ack;
        }

        public int SrcAddr { get; set; }

        public int DstAddr { get; set; }

        public byte? Parameter { get; set; }

        public Control Control { get; set; }
        public byte Command { get; set; }
        public byte[] Data { get; set; }
        public byte Version { get; set; }

        public void FromRaw(byte[] frame)
        {
            Version = frame[0];
            Control = new Control(frame[1]);

            if (Control.CommandType != (byte) CommandTypes.Action && Control.CommandType != (byte) CommandTypes.Get &&
                Control.CommandType != (byte) CommandTypes.Post &&
                Control.CommandType != (byte) CommandTypes.Event) return;

            Command = frame[2];

            if (Control.HasParameter)
                Parameter = frame[3];

            if (Control.HasData)
                Data = frame.Skip(3 + (Control.HasParameter? 1 : 0)).ToArray();
        }

        public byte[] GetRaw()
        {
            var bytes = new List<byte> {Version, Control.GetRaw(), Command};

            if(Parameter.HasValue)
                bytes.Add(Parameter.Value);

            if (Data != null && Data.Length > 0)
                bytes.AddRange(Data);

            return bytes.ToArray();
        }
    }
}