using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cosmos.Rpc;
using NLog;

namespace Cosmos.Framework.Components
{

    public interface IServerHandler : IRpcCaller
    {

    }

    /// <summary>
    /// 使用Rpc的网络组件，与客户端沟通
    /// </summary>
    public class HandlerServer : RpcServer, IServerHandler
    {
        protected IServerHandler _handler;

        public HandlerServer(IServerHandler handler)
            : base(handler, "0.0.0.0")
        {
            _handler = handler;
        }
    }
}
