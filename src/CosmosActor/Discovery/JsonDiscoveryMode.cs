using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Actor;
using etcetera;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cosmos.Actor
{
    public class JsonDiscoveryMode : DiscoveryMode
    {
        public IList<ActorNodeConfig> Nodes;
        public JsonDiscoveryMode(string jsonFile)
        {

            if (!File.Exists(jsonFile))
            {
                throw new FileNotFoundException("Not found json discovery file", jsonFile);
            }

            var text = File.ReadAllText(jsonFile);
            Nodes = JsonConvert.DeserializeObject<ActorNodeConfig[]>(text);
        }
        public override IList<ActorNodeConfig> GetNodes()
        {
            return Nodes;
        }
    }

}
