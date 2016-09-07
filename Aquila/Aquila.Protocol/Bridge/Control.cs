using Aquila.Protocol.Device;

namespace Aquila.Protocol.Bridge
{
    public class Control
    {
        private byte _hasData;
        private byte _hasParameter;
        private byte _commandType;


        public bool HasData
        {
            get { return _hasData == 1; }
            set { _hasData = value ? (byte)1 : (byte)0; }
        }

        public bool HasParameter
        {
            get { return _hasParameter == 1; }
            set { _hasParameter = value ? (byte)1 : (byte)0; }
        }

        public byte CommandType
        {
            get { return _commandType; }
            set { _commandType = value; }
        }

        public Control()
        {
            _commandType = (byte)CommandTypes.Ack;
            _hasParameter = 0;
            _hasData = 0;
        }


        public Control(byte value)
        {
            _commandType = (byte)(value & 7);
            _hasParameter = (byte)((value >> 3) & 1);
            _hasData = (byte)((value >> 4) & 1);
        }
        
        public byte GetRaw()
        {
            var value = (_commandType & 0x07) | (_hasParameter & 0x01) << 3 | (_hasData & 0x01) << 4;
            return (byte) value;
        }
    }
}
