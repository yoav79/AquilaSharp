using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aquila.Protocol.Device
{
    public enum CommandTypes
    {
        Nack = 0x00,
        Ack = 0x01,
        Action = 0x02,
        Get = 0x03,
        Post = 0x04,
        Custom = 0x05,
        Event = 0x06
    }

    public enum Commands
    {
        NAction = 0,
        NEvents = 1,
        Class = 2,
        Size = 3,
        NEntries = 4,
        AbsEntry = 5,
        Entry = 6,
        Clear = 7,
        AddEntry = 8,
        DelAbsEntry = 9,
        DelEntry = 10,
        Action = 11,
        Event = 12,
        Name = 13,
        Eui = 14
    }
}
