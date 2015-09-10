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

        public override Task<IDictionary<string, ActorNodeConfig>> GetNodes()
        {
            TaskCompletionSource<IDictionary<string, ActorNodeConfig>> taskSrc =
                new TaskCompletionSource<IDictionary<string, ActorNodeConfig>>();
            var dict = new Dictionary<string, ActorNodeConfig>();
            foreach (var node in Nodes)
            {
                dict[node.Name] = node;
            }
            taskSrc.SetResult(dict);
            return taskSrc.Task;
        }

        //public override IList<ActorNodeConfig> GetNodes()
        //{
        //    var 
        //    return Nodes;
        //}
    }

}
