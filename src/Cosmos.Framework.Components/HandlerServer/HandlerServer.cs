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

    public interface IHandler : IRpcService
    {

    }

    /// <summary>
    /// 使用Rpc的网络组件，与客户端沟通
    /// </summary>
    public class HandlerServer : RpcServer, IHandler
    {
        protected IHandler _handler;

        public HandlerServer(IHandler handler, int responsePort)
            : base(handler, "*", responsePort)
        {
            _handler = handler;
        }
    }
}
