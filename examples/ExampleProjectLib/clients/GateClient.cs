﻿using System;
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

        public async Task<LoginResProto> Login(int id)
        {
            var loginRes = Coroutine<LoginResProto>.Start(_gateClient.Call<LoginResProto>("Login", id));
            while (!loginRes.IsFinished)
                await Task.Delay(1);

            return loginRes.Result;
        }

        public IEnumerator Login(CoroutineResult<LoginResProto> result, int id)
        {
            var loginRes = Coroutine<LoginResProto>.Start(_gateClient.Call<LoginResProto>("Login", id));

            while (!loginRes.IsFinished)
                yield return null;

            result.Result = loginRes.Result;
        }

        public void Dispose()
        {
            _gateClient.Dispose();
        }
    }
    
}
