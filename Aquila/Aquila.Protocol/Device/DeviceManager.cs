using System.Collections.Generic;
using System.Linq;
using Aquila.Protocol.Bridge;

namespace Aquila.Protocol.Device
{
    public class DeviceManager
    {
        private static readonly object SyncRoot = new object();
        private static DeviceManager _instance = new DeviceManager();
        private readonly List<Device> _devices;

        public List<Device> Devices => _devices;

        private DeviceManager()
        {
            _devices = new List<Device>();
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
        
        public void Discover()
        {
             Bridge.Protocol.Instance.Ping(Mesh.BroadCast);
        }

        public Device AddNew(int srcAddr, byte[] euiAddr)
        {
            var d = new Device()
            {
                ShortAddress = srcAddr,
                Address = euiAddr,
                FetchComplete = false
            };

            if (_devices.FirstOrDefault(a => a.ShortAddress == srcAddr) == null)
                _devices.Add(d);

            return d;
        }

        public Device AddNew(int srcAddr)
        {
            var d = new Device()
            {
                ShortAddress = srcAddr,
            };

            if (_devices.FirstOrDefault(a => a.ShortAddress == srcAddr) == null)
                _devices.Add(d);

            return d;
        }
    }
}

