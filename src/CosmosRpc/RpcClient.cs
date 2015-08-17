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
using Cosmos.Utils;
using NLog;

namespace Cosmos.Rpc
{
    public class RpcClient : BaseNetMqClient
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public RpcClient(string host, int responsePort, int subcribePort = 0, string protocol = "tcp") : base(host, responsePort, subcribePort, protocol)
        {
        }

        public IEnumerator<RpcCallResult<T>> CallResult<T>(string funcName, params object[] arguments)
        {
            var startTime = DateTime.UtcNow;
            
            Logger.Trace("[Start]CallResult: {0}, Arguments: {1}", funcName, arguments);

            var proto = new RequestMsg
            {
                FuncName = funcName,
                Arguments = arguments,
            };
            var responseMsg = Coroutine<ResponseMsg>.Start(Request<RequestMsg, ResponseMsg>(proto));

            while (!responseMsg.IsFinished)
                yield return null;

            Logger.Trace("[Finish]CallResult: {0} used time: {1:F5}s", funcName, (DateTime.UtcNow - startTime).TotalSeconds);

            yield return new RpcCallResult<T>(responseMsg.Result);

        }
        public IEnumerator<T> Call<T>(string funcName, params object[] arguments)
        {
            var result = Coroutine<RpcCallResult<T>>.Start(CallResult<T>(funcName, arguments));
            while (!result.IsFinished)
                yield return default(T);

            yield return result.Result.Value;
        }
    }
}
