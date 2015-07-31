using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MsgPack.Serialization;
using NetMQ;
using NetMQ.Sockets;

namespace Cosmos.Rpc
{

    public struct BaseRequestMsg
    {
        public string RequestToken;
        public byte[] Data;
    }

    /// <summary>
    /// Request to the CosmosNetMq Handler
    /// </summary>
    public abstract class BaseNetMqClient : IDisposable
    {
        internal NetMQContext _context;
        private RequestSocket _client;
        Poller _poller;
        private Task _pollerTask;

        public string Host { get; private set; }

        public int Port { get; private set; }

        public string Protocol { get; private set; }

        public string Address
        {
            get { return string.Format("{0}://{1}:{2}", Protocol, Host, Port); }
        }
        private Dictionary<string, BaseResponseMsg> _responses = new Dictionary<string, BaseResponseMsg>();

        public BaseNetMqClient(string host, int port, string protocol = "tcp")
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

        protected async Task<TResponse> Request<TRequest, TResponse>(TRequest obj)
        {
            var reqData = MsgPackTool.GetBytes(obj);
            var resData = await Request(reqData);

            return MsgPackTool.GetMsg<TResponse>(resData);
        }

        protected async Task<byte[]> Request(byte[] obj)
        {
            var requestMsg = new BaseRequestMsg()
            {
                RequestToken = Path.GetRandomFileName(),
                Data = ProcessRequest(obj),
            };

            var bytes = MsgPackTool.GetBytes(requestMsg);

            _client.Send(bytes);

            var waitResponse = Task.Run(() =>
            {
                BaseResponseMsg waitResponseMsg;
                while (!_responses.TryGetValue(requestMsg.RequestToken, out waitResponseMsg)) { }; // thread blocking
                return waitResponseMsg;
            });
            var responseData = await waitResponse;
            _responses.Remove(requestMsg.RequestToken); // must true!
            return OnResponse(responseData.Data);
        }

        private void OnReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            var recvData = _client.Receive();
            var recvMsg = MsgPackTool.GetMsg<BaseResponseMsg>(recvData);
            _responses[recvMsg.RequestToken] = recvMsg;
        }

        protected byte[] OnResponse(byte[] data)
        {
            return data;
        }

        protected virtual byte[] ProcessRequest(byte[] obj)
        {
            return obj;
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
    }

}
