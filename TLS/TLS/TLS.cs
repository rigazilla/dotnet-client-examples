using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infinispan.HotRod;
using Infinispan.HotRod.Config;

namespace TLS
{
    /// <summary>
    /// This sample code shows how to perform operations over TLS using the C# client
    /// </summary>

    class TLS
    {
        static void Main(string[] args)
        { 
            // Cache manager setup
            RemoteCacheManager remoteManager;
            ConfigurationBuilder conf = new ConfigurationBuilder();
            conf.AddServer().Host("127.0.0.1").Port(11222).ConnectionTimeout(90000).SocketTimeout(900);
            SslConfigurationBuilder sslConfB = conf.Ssl();
            // Retrieve the server public certificate, needed to do server authentication. Mandatory
            if (!System.IO.File.Exists("resources/infinispan-ca.pem"))
            {
                Console.WriteLine("File not found: resources/infinispan-ca.pem.");
                Environment.Exit(-1);
            }
            sslConfB.Enable().ServerCAFile("resources/infinispan-ca.pem");
            // Retrieve the client public certificate, needed if the server requires client authentication. Optional
            if (!System.IO.File.Exists("resources/client-ca.pem"))
            {
                Console.WriteLine("File not found: resources/client-ca.pem.");
                Environment.Exit(-1);
            }
            sslConfB.ClientCertificateFile("resources/client-ca.pem");

            // Usual business now
            conf.Marshaller(new JBasicMarshaller());
            remoteManager = new RemoteCacheManager(conf.Build(), true);
            IRemoteCache<string, string> testCache = remoteManager.GetCache<string, string>();
            testCache.Clear();
            string k1 = "key13";
            string v1 = "boron";
            testCache.Put(k1, v1);
        }
    }
}
