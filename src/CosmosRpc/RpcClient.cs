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
    public class RpcClient : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static int ReqId = 0;
        public Dictionary<int, ResponseMsg> _responses = new Dictionary<int, ResponseMsg>();

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
        private Task _pollerTask;
        //private CancellationTokenSource _pollerTaskCancelSource;
        public RpcClient(string host, int port, string protocol = "tcp")
        {
            Host = host;
            Port = port;
            Protocol = protocol;

            _context = NetMQContext.Create();
            _client = _context.CreateRequestSocket();
            _client.Connect(Address);
            _client.ReceiveReady += OnReceiveReady;

            _poller = new Poller();
            _poller.AddSocket(_client);
            _pollerTask = Task.Run(() =>
            {
                _poller.Start();
            });
        }

        public void Dispose()
        {
            _poller.RemoveSocket(_client);
            _client.Close();
            _context.Dispose();

            _poller.Stop();
            _poller.Dispose();
            _pollerTask.Dispose(); // until release poller
        }

        private void OnReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            var recvData = _client.Receive();
            var response = RpcShare.ResponseSerializer.UnpackSingleObject(recvData);

            _responses[response.RequestId] = response;
        }

        public async Task<RpcCallResult<T>> CallResult<T>(string funcName, params object[] arguments)
        {
            Logger.Trace("RpcClient CallResult Function: {0}, Arguments: {1}", funcName, arguments);

            var proto = new RequestMsg
            {
                RequestId = ReqId++,
                FuncName = funcName,
                Arguments = arguments,
            };
            var bytes = RpcShare.RequestSerializer.PackSingleObject(proto);

            _client.Send(bytes);

            var waitResponse = Task.Run(() =>
            {
                ResponseMsg response2;
                while (!_responses.TryGetValue(proto.RequestId, out response2)) { }; // thread blocking
                return response2;
            });
            var response = await waitResponse;

            _responses.Remove(proto.RequestId); // must true!
            return new RpcCallResult<T>(response);

        }
        public async Task<T> Call<T>(string funcName, params object[] arguments)
        {
            var result = await CallResult<T>(funcName, arguments);

            return result.Value;
        }
    }
}
