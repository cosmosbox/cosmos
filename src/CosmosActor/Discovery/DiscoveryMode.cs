using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Actor
{
    public abstract class DiscoveryMode
    {
        public abstract ActorNodeConfig[] GetNodes();
    }

}
