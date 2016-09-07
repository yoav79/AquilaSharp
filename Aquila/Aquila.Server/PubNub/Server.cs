using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Aquila.Server.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PubNubMessaging.Core;

namespace Aquila.Server.PubNub
{
    internal class Server
    {
        private static readonly object SyncRoot = new object();
        private static Server _instance = new Server();

        /**
         * USE YOUR OWN KEYS HERE
         **/
        private const string Channel = "AquilaServer";
        private const string Uuid = "server";
        private const string PubKey = "pub-c-6d2150b7-0ba7-431b-9355-bdf734078c28";
        private const string SubKey = "sub-c-9c31d412-7395-11e6-a014-0619f8945a4f";
        private const string SecretKey = "sec-c-NThkYzNjOTgtNjRlYy00NjI1LWIwYzUtMmZlZTUyMThhMmU3";

        public Pubnub Pubnub { get; }

        private readonly Dictionary<string, Bridge> _bridges = new Dictionary<string, Bridge>();

        public static Server Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (_instance == null)
                            _instance = new Server();
                    }
                }

                return _instance;
            }
        }


        private Server()
        {
            Pubnub = new Pubnub(PubKey, SubKey, SecretKey) {SessionUUID = Uuid};
            Pubnub.Subscribe<string>(Channel, HandleMessage, DefaultCallback, ErrorCallback);
            Console.WriteLine("Server created.");
        }

        #region PubNub Callbacks

        private void HandleMessage(string json)
        {
            var coll = JsonConvert.DeserializeObject<ReadOnlyCollection<object>>(json);
            var container = coll[0] as JContainer;

            // ReSharper disable once PossibleNullReferenceException
            var msg = container.ToObject<Dictionary<string, string>>();

            Console.WriteLine();
            Console.WriteLine("---------------------------");
            Console.WriteLine();
            Console.WriteLine("Server incoming message: {0}", container);

            if (!msg.ContainsKey("publickey") || !msg.ContainsKey("type"))
            {
                Console.WriteLine("Invalid message received");
                return;
            }

            /**
             * Local variables for the switch block
             **/
            Dictionary<string, string> response = new Dictionary<string, string>();
            CommunicationManager com;
            Bridge bridge;
            string comName;
            string message;
            bool success;

            response["type"] = msg["type"];
            try
            {
                switch (msg["type"])
                {
                    case "bridge": // Login
                        var b = GetBridge(msg["publickey"]);
                        if(b.)

                        break;
                    case "device":
                        break;
                    case "event":
                        break;
                }
            }
            catch (Exception e)
            {
                response["type"] = "exception";
            }
            finally
            {
                this.SendMessage(msg["uuid"], response);
            }
        }

        private void DefaultCallback(string msg)
        {
            // okay great
            Console.WriteLine(msg);
        }

        private static void ErrorCallback(PubnubClientError e)
        {
            Console.WriteLine("Server error occurred: {0}", e.Message);
        }

        private static void MessageSent(string e)
        {
            Console.WriteLine("Message sent: {0}", e);
        }

        #endregion PubNub Callbacks

        #region Convenience Methods

        public void SendMessage(string channel, object data)
        {
            var dict = data as Dictionary<string, string>;
            if (dict != null)
            {
                Console.Write("{0} sending <{1}>: ", "Server", channel);
                Util.PrintDict(dict);
                Console.WriteLine();
            }
            Pubnub.Publish<string>(channel, data, MessageSent, ErrorCallback);
        }

        public Bridge GetBridge(string uuid)
        {
            Bridge b;
            _bridges.TryGetValue(uuid, out b);
            return b;
        }

        #endregion Convenience Methods
    }
}
