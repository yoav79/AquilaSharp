using System;
using System.Collections.Generic;

namespace Aquila.Protocol.Device
{
    public class Device
    {

        public Device()
        {
            Actions = new Dictionary<int, string>();
            Events = new Dictionary<int, string>();
        }

        public byte[] Address { get; set; }

        public int ShortAddress { get; set; }

        public string Class { get; set; }

        public string Name { get; set; }

        public bool Active { get; set; }

        public Dictionary<int, string> Actions { get; set; }

        public Dictionary<int, string> Events { get; set; }

        public bool FetchComplete { get; set; }

        public int ActionsCount { get; set; }

        public int EventCount { get; set; }
    }

}