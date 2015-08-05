using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Actor
{
    /// <summary>
    /// For discovery and Config
    /// </summary>
    public struct ActorNodeConfig
    {
        public string AppToken;

        public string Name;
        public string ActorClass;

        public Type ActorClassType
        {
            get
            {
                return Type.GetType(ActorClass);
            }
        }

        public string Host;
        public int RpcPort;
        public int ResponsePort;
        public int PublisherPort;

        public string DiscoveryMode;
        public string DiscoveryUri;

        public string[] DiscoveryServers;

        public ActorNodeConfig Clone()
        {
            return (ActorNodeConfig)this.MemberwiseClone();
        }
    }
}
