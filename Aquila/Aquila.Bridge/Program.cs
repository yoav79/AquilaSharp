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
              Console.WriteLine("Start!!!!");
              // Look up Bridge;

                Protocol.Device.Protocol.Instance.Start();
              // find ?
              

              Console.ReadKey();

          }
      }
}
