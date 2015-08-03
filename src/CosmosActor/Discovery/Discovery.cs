using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using etcetera;
using NLog;
using System.IO;

namespace Cosmos.Actor
{
    /// <summary>
    /// Etcd Manager
    /// </summary>
    public class Discovery
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public event Action<ActorNodeConfig> AddNodeEvent;
        public event Action<ActorNodeConfig> RemoveNodeEvent;

        private EtcdClient _etcdClient;
        public Discovery(string appToken, string[] discoveryServers)
        {
            foreach (var etcdUrl in discoveryServers)
            {
                var etcdClient = new EtcdClient(new Uri(string.Format("{0}/v2/keys", etcdUrl)));
                try
                {
                    etcdClient.Statistics.Leader();
                    _etcdClient = etcdClient;
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message);
                    Logger.Error("Invalid Etcd Host: {0}", etcdUrl);
                    continue;
                }
            }

            if(_etcdClient == null)
                throw new Exception("Not valid EtcdClient");
            
        }
        public void RegisterActor()
        {
            
        }

        public ActorNodeConfig[] GetActorNodes()
        {
            return null;
        }

        public void BeginWatch()
        {
            
        }

        void OnActorNodesChanged(ActorNodeConfig[] nodes)
        {
            
        }
    }
}
