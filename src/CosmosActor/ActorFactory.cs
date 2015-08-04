using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Rpc;
namespace Cosmos.Actor
{
    //public class ActorConf
    //{
    //    public string AppToken;
    //    public string[] DiscoveryServers;

    //    public string Name;
        
    //    public Type ActorClass;
    //    public string Category;
    //}

    /// <summary>
    /// Main
    /// </summary>
    class ActorFactory
    {
        public static Actor Create(ActorNodeConfig conf)
        {
            var actorType = Type.GetType(conf.ActorClass);
            if (actorType == null)
                throw new Exception(string.Format("Not found Actor Class: {0}", conf.ActorClass));
            var obj = (Actor)Activator.CreateInstance(actorType);
            obj.Init(conf);
            return obj;

        }
    }
}
