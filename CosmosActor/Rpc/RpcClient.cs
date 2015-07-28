using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;

namespace CosmosActor.Rpc
{
    public class RpcClient : IDisposable
    {
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
        bool waitResponse = false;
        private void OnReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            var recvData = _client.ReceiveString();

            Console.WriteLine("Recv from request: " + recvData);

            result[ReqId - 1] = recvData;
        }
        static int ReqId = 0;
        public Dictionary<int, string> result = new Dictionary<int, string>();
        public async Task<string> Request(string str)
        {
            var reqId = ReqId++;
            _client.Send(str);
            await Task.Run(() =>
            {
                while (!result.ContainsKey(reqId))
                {
                }
            });

            return result[reqId];

        }
        public void Dispose()
        {
            _poller.RemoveSocket(_client);
            _client.Close();
            _context.Dispose();

            _poller.Dispose();

        }
    }
}
