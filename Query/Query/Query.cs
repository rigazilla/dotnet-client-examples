using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infinispan.HotRod;
using Infinispan.HotRod.Config;
using Google.Protobuf;
using Org.Infinispan.Protostream;
using Org.Infinispan.Query.Remote.Client;
using QueryExampleBankAccount;
using System.IO;

namespace Query
{
    /// <summary>
    /// This sample code shows how to perform Infinispan queries using the C# client
    /// </summary>
    class Query
    {
        static void Main(string[] args)
        {
            // Cache manager setup
            RemoteCacheManager remoteManager;
            const string ERRORS_KEY_SUFFIX = ".errors";
            const string PROTOBUF_METADATA_CACHE_NAME = "___protobuf_metadata";
            ConfigurationBuilder conf = new ConfigurationBuilder();
            conf.AddServer().Host("127.0.0.1").Port(11222).ConnectionTimeout(90000).SocketTimeout(6000);
            conf.Marshaller(new BasicTypesProtoStreamMarshaller());
            remoteManager = new RemoteCacheManager(conf.Build(), true);
            IRemoteCache<String, String> metadataCache = remoteManager.GetCache<String, String>(PROTOBUF_METADATA_CACHE_NAME);
            IRemoteCache<int, User> testCache = remoteManager.GetCache<int, User>("namedCache");

            // Installing the entities model into the Infinispan __protobuf_metadata cache    
            metadataCache.Put("sample_bank_account/bank.proto", File.ReadAllText("resources/proto2/bank.proto"));
            if (metadataCache.ContainsKey(ERRORS_KEY_SUFFIX))
            {
                Console.WriteLine("fail: error in registering .proto model");
                Environment.Exit(-1);
            }

            // The application cache must contain entities only 
            testCache.Clear();
            // Fill the application cache
            User user1 = new User();
            user1.Id = 4;
            user1.Name = "Jerry";
            user1.Surname = "Mouse";
            User ret = testCache.Put(4, user1);

            // Run a query
            QueryRequest qr = new QueryRequest();
            qr.JpqlString = "from sample_bank_account.User";
            QueryResponse result = testCache.Query(qr);
            List<User> listOfUsers = new List<User>();
            unwrapResults(result, listOfUsers);

        }

        // Convert Protobuf matter into C# objects
        private static bool unwrapResults<T>(QueryResponse resp, List<T> res) where T : IMessage<T>
        {
            if (resp.ProjectionSize > 0)
            {  // Query has select
                return false;
            }
            for (int i = 0; i < resp.NumResults; i++)
            {
                WrappedMessage wm = resp.Results.ElementAt(i);

                if (wm.WrappedBytes != null)
                {
                    WrappedMessage wmr = WrappedMessage.Parser.ParseFrom(wm.WrappedBytes);
                    if (wmr.WrappedMessageBytes != null)
                    {
                        System.Reflection.PropertyInfo pi = typeof(T).GetProperty("Parser");

                        MessageParser<T> p = (MessageParser<T>)pi.GetValue(null);
                        T u = p.ParseFrom(wmr.WrappedMessageBytes);
                        res.Add(u);
                    }
                }
            }
            return true;
        }
    }
}
