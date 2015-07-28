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

        public RpcClient(string host, int port, string protocol = "tcp")
        {
            Host = host;
            Port = port;
            Protocol = protocol;
            _context = NetMQContext.Create();
            _client = _context.CreateRequestSocket();
            _client.Connect(Address);

        }

        public string Request(string str)
        {
            _client.Send(str);

            var recv = _client.Receive();

            Console.WriteLine("Recv from request: " + recv);

            return recv.ToString();

        }
        public void Dispose()
        {
            _client.Close();
            _context.Dispose();
        }
    }
}
