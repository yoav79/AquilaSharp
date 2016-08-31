namespace Aquila.Protocol
{
    public class Packet
    {
        public int Lqi { get; set; }
        public int Rssi { get; set; }
        public int SrcAddr { get; set; }
        public int DstAddr { get; set; }
        public int SrcEndPoint { get; set; }
        public int DstEndPoint { get; set; }
        public int Size { get; set; }
        public byte[] Frame { get; set; }
    }
}