using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Framework.Components;
using Cosmos.Utils;

namespace ExampleProjectLib
{
    /// <summary>
    /// Gate
    /// </summary>
    class GateClient : IDisposable
    {
        private HandlerClient _gateClient;
        public GateClient(string host, int port)
        {
            _gateClient = new HandlerClient(host, port);
            
        }

        public void Start()
        {
            //_gateClient.Call<LoginResProto>()
        }

        public IEnumerator<LoginResProto> Login(int id)
        {
            var loginRes = Coroutine<LoginResProto>.Start(_gateClient.Call<LoginResProto>("Login", id));
            while (!loginRes.IsFinished)
                yield return null;

            yield return loginRes.Result;
        }

        public void Dispose()
        {
            _gateClient.Dispose();
        }
    }
    
}
