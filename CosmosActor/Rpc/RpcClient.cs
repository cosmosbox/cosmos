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

namespace Cosmos.Rpc
{
    public class RpcClient : IDisposable
    {
        static int ReqId = 0;
        public Dictionary<int, RpcResponseProto> _responses = new Dictionary<int, RpcResponseProto>();

        internal NetMQContext _context;
        private RequestSocket _client;
        public string Host { get; private set; }

        public int Port { get; private set; }

        public string Protocol { get; private set; }

        public string Address
        {
            get { return string.Format("{0}://{1}:{2}", Protocol, Host, Port); }
        }

        Poller _poller;
        public RpcClient(string host, int port, string protocol = "tcp")
        {
            Host = host;
            Port = port;
            Protocol = protocol;
            _context = NetMQContext.Create();
            _client = _context.CreateRequestSocket();
            _client.Connect(Address);

            _poller = new Poller();
            _poller.AddSocket(_client);

            _client.ReceiveReady += OnReceiveReady;

            Task.Run(() =>
            {
                _poller.Start();
            });
        }
        private void OnReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            var recvData = _client.Receive();
            var response = RpcShare._responseSerializer.UnpackSingleObject(recvData);

            _responses[response.RequestId] = response;
        }
        public void Dispose()
        {
            _poller.RemoveSocket(_client);
            _client.Close();
            _context.Dispose();

            _poller.Dispose();

        }

        public async Task<T> Call<T>(string funcName, params object[] arguments)
        {
            var proto = new RpcRequestProto
            {
                RequestId = ReqId++,
                FuncName = funcName,
                Arguments = arguments,
            };
            var bytes = RpcShare.RequestSerializer.PackSingleObject(proto);

            _client.Send(bytes);

            var waitResponse = Task<RpcResponseProto>.Run<RpcResponseProto>(() => {
                RpcResponseProto response2;
                while (!_responses.TryGetValue(proto.RequestId, out response2)) { }; // thread blocking
                return response2;
            });
            var response = await waitResponse;

            _responses.Remove(proto.RequestId); // must true!

            var msgObj = (MsgPack.MessagePackObject)response.Result;
            return (T)msgObj.ToObject();

        }
    }
}
