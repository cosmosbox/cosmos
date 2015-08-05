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
    public class ActorNodeConfig
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

        public string Host = "*";
        public int RpcPort = -1;

        public int ResponsePort = 0;
        public int PublisherPort;

        public string DiscoveryMode = "file";
        public object DiscoveryParam;

        public ActorNodeConfig Clone()
        {
            return (ActorNodeConfig)this.MemberwiseClone();
        }
    }
}
