using System.Collections.Generic;
using System.Dynamic;
using System.Threading;
using Aquila.Protocol.Bridge;

namespace Aquila.Protocol.Device
{
    public class DeviceManager
    {
        private static readonly object SyncRoot = new object();
        private static DeviceManager _instance = new DeviceManager();
        private Queue<dynamic> _queue;


        private DeviceManager()
        {
        }

        public static DeviceManager Instance
        {
            get
            {
                if (_instance != null) return _instance;

                lock (SyncRoot)
                {
                    if (_instance == null)
                        _instance = new DeviceManager();
                }

                return _instance;
            }
        }

        public void DeviceFetcher(int srcAddr, IEnumerable<byte> euiAddr)
        {
            dynamic device = new ExpandoObject();
            device.SrcAddr = srcAddr;
            device.EuiAddr = euiAddr;

            FetchClass(device.SrcAddr);
        }
        
        public void Discover()
        {
             Protocol.Instance.Ping(Mesh.BroadCast);
        }

        private void RequestAction(int address, byte action, byte param)
        {
            Protocol.Instance.RequestAction(address, action, param);
        }

        private void RequestGet(int address, byte action, byte param, byte[] data)
        {
            Protocol.Instance.RequestGet(address, action, param, data);
        }

        private void RequestPost(int address, byte action, byte param, byte[] data)
        {
            Protocol.Instance.RequestPost(address, action, param, data);
        }

        private void RequestCustom(int address, byte[] data)
        {
            Protocol.Instance.RequestCustom(address, data);
        }

        public void FetchAll()
        {
            
        }

        public void FetchClass(int address)
        {
            RequestGet(address, (byte) Commands.Class, 0, new List<byte>().ToArray());
        }

    }
}

