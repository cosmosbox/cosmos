using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Actor;
using Cosmos.Rpc;
namespace ExampleProject
{
    class ChatRpcCaller : RpcCaller
    {
        public void SendChat()
        {
            
        }
    }

    class ChatActor : Actor
    {
        public override RpcCaller NewRpcCaller()
        {
            return new ChatRpcCaller();
        }
    }
}
