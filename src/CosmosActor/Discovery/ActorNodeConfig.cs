using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Actor
{
    /// <summary>
    /// For discovery and Config
    /// </summary>
    public struct ActorNodeConfig
    {
        public string Name;
        public string ActorClass;
        public string Host;
        public int ResponsePort;
        public int PublisherPort;

        public string DiscoveryMode;
        public string DiscoveryUri;
    }
}
