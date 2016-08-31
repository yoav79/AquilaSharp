using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Aquila.Protocol;

namespace Aquila.Bridge
{
      class Program
      {
          static void Main(string[] args)
          {

              // Look up Bridge;
              var Bridge = new Protocol.Bridge();

              // find ?
              Bridge.Begin("COM3", 57600);

              while (!Bridge.IsReady)
              {
                  Thread.Sleep(100);
              }

              Console.WriteLine(String.Join(":", Bridge.LongAddress.Select(a => a.ToString("X"))));
              Console.ReadKey();

              Bridge.Ping();
              Console.ReadKey();

              byte[] key = new byte[16];
              for (var i = 0; i < 16; i++)
                  key[i] = 12;

              Bridge.SendData(new Packet()
              {
                  Lqi = 0xff,
                  Rssi = 0xff,
                  SrcAddr = 0,
                  DstAddr = 0xFFFF,
                  SrcEndPoint = 15,
                  DstEndPoint = 15,
              });
              Console.ReadKey();

          }


      }
}
