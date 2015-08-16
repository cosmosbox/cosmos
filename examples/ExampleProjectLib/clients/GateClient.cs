using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Framework.Components;

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
            var loginRes = await _gateClient.Call<LoginResProto>("Login", id);

            return loginRes;
        }

        public void Dispose()
        {
            _gateClient.Dispose();
        }
    }
    
}
