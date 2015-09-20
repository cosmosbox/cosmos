using System;
using System.Collections;
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

        public async Task<RES> CallResultAsync<REQ, RES>(REQ request)
        {
            var startTime = DateTime.UtcNow;

            Logger.Trace("[Start]Request: {0}, Response: {1}", request, typeof(RES));

            //var proto = new RequestMsg
            //{
            //    FuncName = funcName,
            //    Arguments = arguments,
            //};
            var responseMsg = await RequestAsync<REQ, RES>(request);

            Logger.Trace("[Finish]CallResult: {0} used time: {1:F5}s", request, (DateTime.UtcNow - startTime).TotalSeconds);

            return responseMsg;
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
        public IEnumerator Call<T>(CoroutineResult<T> resulter, string funcName, params object[] arguments)
        {
            var result = Coroutine<RpcCallResult<T>>.Start(CallResult<T>(funcName, arguments));
            while (!result.IsFinished)
                yield return default(T);

            resulter.Result = result.Result.Value;
            //yield return result.Result.Data;
        }

        public async Task<RES> CallAsync<T, RES>(T request)
        {
            var result = await CallResultAsync<T, RES>(request);
            return result;
        }
    }
}
