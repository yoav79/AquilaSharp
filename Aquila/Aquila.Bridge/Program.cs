using System;
using System.Linq;
using System.Threading;
using Aquila.Protocol.Bridge;

namespace Aquila.Service
{
      class Program
      {
          static void Main(string[] args)
          {

              // Look up Bridge;
              var mesh = new Mesh();

              // find ?
              mesh.Begin("COM3", 57600);
              mesh.Ping(Mesh.BroadCast);
              Console.ReadKey();
          }
      }
}
