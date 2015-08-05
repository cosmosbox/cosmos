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
    public partial class ActorNodeConfig
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

        public string DiscoveryMode = "Json";
        public object DiscoveryParam;


        
        public int PublisherPort;


        /// <summary>
        /// For FrontendActor
        /// </summary>
        public int ResponsePort = 0;

        public ActorNodeConfig Clone()
        {
            return (ActorNodeConfig)this.MemberwiseClone();
        }
    }
}
