using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Actor;

namespace Cosmos.Framework.Components
{
    /// <summary>
    /// 前端可用, 使用ResponsePort
    /// </summary>
    public abstract class FrontendActor : CInternalActor
    {
        protected HandlerServer _handlerServer;

        protected FrontendActor()
        {
        }

        public override void Init(ActorNodeConfig conf)
        {
            base.Init(conf);

            _handlerServer = new HandlerServer(GetHandler(), conf.ResponsePort);
        }

        public abstract IHandler GetHandler();
    }


    /// <summary>
    /// 仅仅用于内部RPC的Actor
    /// </summary>
    public abstract class CInternalActor : Cosmos.Actor.Actor
    {

    }


}
