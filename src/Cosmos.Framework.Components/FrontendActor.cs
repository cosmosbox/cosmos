using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Actor;

namespace Cosmos.Framework.Components
{

    /// <summary>
    /// 前端可用
    /// </summary>
    public abstract class CFrontendActor : CInternalActor
    {
        private HandlerServer _gateServer;

        protected CFrontendActor()
        {
        }

        public override void Init(ActorNodeConfig conf)
        {
            base.Init(conf);

            _gateServer = new HandlerServer(GetHandler());
        }

        public abstract IServerHandler GetHandler();
    }


    /// <summary>
    /// 仅仅用于内部RPC的Actor
    /// </summary>
    public abstract class CInternalActor : Cosmos.Actor.Actor
    {

    }


}
