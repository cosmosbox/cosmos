using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Rpc;
namespace Cosmos.Actor
{
    public class ActorConf
    {
        public string AppToken;
        public string[] DiscoverServers;

        public string Name;
        
        public Type ActorClass;
        public string Category;


    }

    /// <summary>
    /// Main
    /// </summary>
    class ActorFactory
    {
        public static Actor Create(ActorConf conf)
        {
            var obj = (Actor)Activator.CreateInstance(conf.ActorClass);
            obj.Init(conf);
            return obj;

        }
    }
}
