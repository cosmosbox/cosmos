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
        public override IActorService NewRpcCaller()
        {
            return new GateActorService();
        }

        public override IServerHandler GetHandler()
        {
            return new GateActorHandler(this);
        }
    }

    class GateActorHandler : IServerHandler
    {
        private GateServices _services;
        private GateActor _gateActor;

        public GateActorHandler(GateActor gateActor)
        {
            _gateActor = gateActor;
            _services = new GateServices(this._gateActor);
        }
        public string TestHandler()
        {
            return "TestHandlerString";
        }

        public void RunGameServer()
        {
            //ActorRunner.Run()
        }
        public LoginResProto Login()
        {
            
            return new LoginResProto()
            {
                GameServerHost = "127.0.0.1",
                //GameServerPort = ""
            };
        }
    }

    class GateActorService : IActorService
    {
    }
}
