using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Rpc;

namespace Cosmos.Framework.Components
{
    /// <summary>
    /// A client library for Handler Client
    /// </summary>
    public class HandlerClient : RpcClient
    {
        public HandlerClient(string host, int responsePort) 
            : base(host, responsePort, "tcp")
        {

        }
    }
}
