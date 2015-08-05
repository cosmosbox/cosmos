using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Actor;
using Cosmos.Rpc;
using Nancy.ModelBinding;

namespace ExampleProject
{

    internal class AdminHttpActorRpcCaller : IActorRpcer
    {
        private readonly AdminHttpActor _actor;

        public AdminHttpActorRpcCaller(AdminHttpActor actor)
        {
            _actor = actor;
        }
        public bool AddPlayerCount()
        {
            _actor.TotalPlayerCount++;
            return true;
        }
        public bool RemovePlayerCount()
        {
            _actor.TotalPlayerCount--;
            return true;
        }
    }

    class AdminHttpActor : Actor
    {
        public int TotalPlayerCount;// 总在线人数

        public AdminHttpActor()
        {
            var httpHandler = new HttpHandler();
            httpHandler.Start();

        }
        public override IActorRpcer NewRpcCaller()
        {
            return new AdminHttpActorRpcCaller(this);
        }
    }

}
