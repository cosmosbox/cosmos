using System;
using System.Collections;
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

        public async Task<LoginResponse> Login(int id)
        {
            var loginRes = await _gateClient.CallAsync<LoginRequest, LoginResponse>(new LoginRequest
            {
                Id = id,
            });
            
            return loginRes;
        }

        //public IEnumerator Login(CoroutineResult<LoginResProto> result, int id)
        //{
        //    var resulter = new CoroutineResult<LoginResProto>();
        //    var loginRes = Coroutine2.Start(_gateClient.Call<LoginResProto>(resulter, "Login", id));

        //    while (!loginRes.IsFinished)
        //        yield return null;

        //    result.Result = resulter.Result;
        //}

        public void Dispose()
        {
            _gateClient.Dispose();
        }
    }
    
}
