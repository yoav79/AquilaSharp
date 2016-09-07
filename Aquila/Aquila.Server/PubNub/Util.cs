using System;
using System.Collections.Generic;
using System.Timers;

namespace Aquila.Server.PubNub
{
    class Util
    {
        public static void PrintDict<K, V>(Dictionary<K, V> d)
        {
            Console.WriteLine("{");
            foreach (KeyValuePair<K, V> entry in d)
            {
                Console.WriteLine("  {0}: {1}", entry.Key, entry.Value);
            }
            Console.WriteLine("}");
        }

        public static void SetTimeout(Action func, double delay)
        {
            Timer t = new Timer(delay) {AutoReset = false};
            t.Elapsed += (object src, ElapsedEventArgs e) => {
                                                                 func();
            };
            t.Enabled = true;
        }
    }
}
