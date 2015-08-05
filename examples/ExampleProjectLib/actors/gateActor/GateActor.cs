using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Actor;
using Cosmos.Framework.Components;
using Cosmos.Rpc;

namespace ExampleProjectLib
{

    class GateActor : FrontendActor
    {
        public override IActorRpcer NewRpcCaller()
        {
            return new GateActorRpcer();
        }

        public override IServerHandler GetHandler()
        {
            return new GateActorHandler();
        }
    }

    class GateActorHandler : IServerHandler
    {

        public string TestHandler()
        {
            return "TestHandlerString";
        }
    }

    class GateActorRpcer : IActorRpcer
    {
    }
}
