using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Actor;
using Cosmos.Rpc;

namespace ExampleProjectLib
{
    class GameActorRpcer : IActorRpcer
    {
        
    }
    class GameActor : Actor
    {
        public override IActorRpcer NewRpcCaller()
        {
            return new GameActorRpcer();
        }
    }
}
