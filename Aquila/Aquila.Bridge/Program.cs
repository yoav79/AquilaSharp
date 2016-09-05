using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aquila.Protocol.Bridge;
using Aquila.Protocol.Device;

namespace Aquila.Service
{
      class Program
      {
          static void Main(string[] args)
          {
              Console.WriteLine("Start!!!!");
              // Look up Bridge;

              Protocol.Bridge.Protocol.Instance.Start();
              // find ?


              Task.Factory.StartNew(() =>
              {

              });

              Console.ReadKey();

          }
      }
}
