using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infinispan.HotRod;
using Infinispan.HotRod.Config;

namespace RemoteExec
{
    /// <summary>
    /// This sample code shows how to perform a server remote execution using the C# client
    /// </summary>
    class RemoteExec
    {
        static void Main(string[] args)
        {
            // Cache manager setup
            RemoteCacheManager remoteManager;
            IMarshaller marshaller;
            ConfigurationBuilder conf = new ConfigurationBuilder();
            conf.AddServer().Host("127.0.0.1").Port(11222).ConnectionTimeout(90000).SocketTimeout(6000);
            marshaller = new JBasicMarshaller();
            conf.Marshaller(marshaller);
            remoteManager = new RemoteCacheManager(conf.Build(), true);

            // Install the .js code into the Infinispan __script_cache
            const string PROTOBUF_SCRIPT_CACHE_NAME = "___script_cache";
            string valueScriptName = "getValue.js";
            string valueScript = "// mode=local,language=javascript\n "
                 + "var cache = cacheManager.getCache(\"namedCache\");\n "
                 + "var ct = cache.get(\"accessCounter\");\n "
                 + "var c = ct==null ? 0 : parseInt(ct);\n "
                 + "cache.put(\"accessCounter\",(++c).toString());\n "
                 + "cache.get(\"privateValue\") ";
            string accessScriptName = "getAccess.js";
            string accessScript = "// mode=local,language=javascript\n "
                + "var cache = cacheManager.getCache(\"namedCache\");\n "
                + "cache.get(\"accessCounter\")";
            IRemoteCache<string, string> scriptCache = remoteManager.GetCache<string, string>(PROTOBUF_SCRIPT_CACHE_NAME);
            IRemoteCache<string, string> testCache = remoteManager.GetCache<string, string>("namedCache");
            scriptCache.Put(valueScriptName, valueScript);
            scriptCache.Put(accessScriptName, accessScript);

            // Now use the scripts
            testCache.Put("privateValue", "Counted Access Value");
            Dictionary<string, string> scriptArgs = new Dictionary<string, string>();
            byte[] ret1 = testCache.Execute(valueScriptName, scriptArgs);
            string value = (string)marshaller.ObjectFromByteBuffer(ret1);
            byte[] ret2 = testCache.Execute(accessScriptName, scriptArgs);
            string accessCount = (string)marshaller.ObjectFromByteBuffer(ret2);
            Console.Write("Return value is '" + value + "' and has been accessed '" + accessCount + "' times.");

        }
    }
}
