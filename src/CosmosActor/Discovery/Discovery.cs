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

        private DiscoveryMode _mode;
        public Discovery(string appToken, string discoveryMode, object discoveryParam)
        {
            var modeType = Type.GetType(string.Format("Cosmos.Actor.{0}DiscoveryMode", discoveryMode));
            _mode = (DiscoveryMode)Activator.CreateInstance(modeType, new object[]
            {
                discoveryParam
            });
            
        }
        public void RegisterActor()
        {
            
        }

        public async Task<IDictionary<string, ActorNodeConfig>> GetActorNodes()
        {
            return await _mode.GetNodes();
        }

        public void BeginWatch()
        {
            
        }

        void OnActorNodesChanged(ActorNodeConfig[] nodes)
        {
            
        }
    }
}
