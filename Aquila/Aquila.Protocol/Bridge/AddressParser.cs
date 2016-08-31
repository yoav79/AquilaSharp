namespace Aquila.Protocol.Bridge
{


/*
  Parses protocol addresses from string to Buffer and backwards.

  Address formats:
    As string:
      Long Address: "01:02:23:A2:B5:C4:2A:5B"
      Short Address(broadcast): "FF:FF"
    As array:
      Long Address: [0x01, 0x02, 0x23, 0xA2, 0xB5, 0xC4, 0x2A, 0x5b]
      Short Address: [0xFF, 0xFF]
*/

    public class AddressParser
    {
        private string _address;
        
        private readonly bool _isValid = false;


        public bool IsValid => _isValid;
        
        public AddressParser(string addres)
        {
            _isValid = false;
        }

        public AddressParser(byte[] address)
        {
            if (address.Length == 8 || address.Length == 2)
            {
                _isValid = true;
            }
        }

        public bool IsAddress()
        {
            return true;
        }

        public byte[] ToBuffer()
        {
            return new byte[10];
        }

        public override string ToString()
        {
            return base.ToString();
        }
        
    }
}