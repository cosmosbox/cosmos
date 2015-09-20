using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Actor;
using Cosmos.Framework.Components;
using Cosmos.Rpc;
using ExampleProject;

namespace ExampleProjectLib
{

    class GateActor : FrontendActor
    {
        public override IActorService NewRpcCaller()
        {
            return new GateActorService();
        }

        public override IHandler GetHandler()
        {
            return new GateActorHandler(this);
        }
    }

    class GateActorHandler : IHandler
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

        [ServiceFunc]
        public LoginResponse Login(LoginRequest req)
        {
            var cfg = ExampleServerApp.Instance.ProjectConf.TheActorConfigs[2];

            return new LoginResponse()
            {
                GameServerHost = cfg.Host,
                GameServerPort = cfg.ResponsePort,
                SubcribePort = cfg.PublishPort,
                Id = req.Id,
            };
        }
    }

    class GateActorService : IActorService
    {
    }
}
