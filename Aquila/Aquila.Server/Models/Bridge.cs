namespace Aquila.Server.Models
{
    public class Bridge
    {
        public int UserId { get; set; }

        public int BridgeId { get; set; }

        public string PublicKey { get; set; }

        public string Name { get; set; }

        public byte[] Eui { get; set; }

        public int ShortAddress { get; set; }
    }
}
