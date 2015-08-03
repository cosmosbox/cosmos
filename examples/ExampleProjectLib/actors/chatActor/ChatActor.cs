using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Actor;
using Cosmos.Rpc;
namespace ExampleProject
{
    class ChatRpcCaller : IActorRpcer
    {
        public void SendChat()
        {
            
        }
    }

    class ChatActor : Actor
    {
        public override IActorRpcer NewRpcCaller()
        {
            return new ChatRpcCaller();
        }
    }
}
