using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;
using MsgPack.Serialization;
using System.IO;
using System.Threading;
using NLog;

namespace Cosmos.Rpc
{
    public class RpcClient : BaseNetMqClient
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public RpcClient(string host, int responsePort, int subcribePort = 0, string protocol = "tcp") : base(host, responsePort, subcribePort, protocol)
        {
        }

        public async Task<RpcCallResult<T>> CallResult<T>(string funcName, params object[] arguments)
        {
            Logger.Trace("RpcClient CallResult Function: {0}, Arguments: {1}", funcName, arguments);

            var proto = new RequestMsg
            {
                FuncName = funcName,
                Arguments = arguments,
            };
            var responseMsg = await Request<RequestMsg, ResponseMsg>(proto);
            return new RpcCallResult<T>(responseMsg);

        }
        public async Task<T> Call<T>(string funcName, params object[] arguments)
        {
            var result = await CallResult<T>(funcName, arguments);

            return result.Value;
        }
    }
}
