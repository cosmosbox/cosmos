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
        private NetMQSocket _requestClient;
        private SubscriberSocket _subSocket;
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

        protected BaseNetMqClient(string host, int port, string protocol = "tcp")
        {
            Host = host;
            Port = port;
            Protocol = protocol;

            _poller = new Poller();

            _context = NetMQContext.Create();

            _subSocket = _context.CreateSubscriberSocket();
            _subSocket.Options.ReceiveHighWatermark = 1000;
            _subSocket.Connect(Address);
            _subSocket.ReceiveReady += OnSubscriberReceiveReady;
            _poller.AddSocket(_subSocket);

            _requestClient = _context.CreateRequestSocket();
            _requestClient.Connect(Address);
            _requestClient.ReceiveReady += OnRequestReceiveReady;

            _poller.AddSocket(_requestClient);
            _pollerTask = Task.Run(() =>
            {
                _poller.Start();
            });
        }

        private void OnSubscriberReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            string messageTopicReceived = _subSocket.ReceiveString();
            byte[] messageReceived = _subSocket.Receive();

            Console.WriteLine("Topic: {0}", messageTopicReceived);
            Console.WriteLine("Message: {0}", messageReceived);
        }

        public void Dispose()
        {
            _subSocket.Close();
            _poller.RemoveSocket(_requestClient);
            _requestClient.Close();
            _context.Dispose();

            _poller.Stop();
            _poller.Dispose();
            _pollerTask.Dispose(); // until release poller
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
                Data = obj,
            };

            var bytes = MsgPackTool.GetBytes(requestMsg);

            _requestClient.Send(bytes);

            var waitResponse = Task.Run(() =>
            {
                BaseResponseMsg waitResponseMsg;
                while (!_responses.TryGetValue(requestMsg.RequestToken, out waitResponseMsg)) { }; // thread blocking
                return waitResponseMsg;
            });
            var responseData = await waitResponse;
            _responses.Remove(requestMsg.RequestToken); // must true!
            return responseData.Data;
        }

        private void OnRequestReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            var recvData = _requestClient.Receive();
            var recvMsg = MsgPackTool.GetMsg<BaseResponseMsg>(recvData);
            _responses[recvMsg.RequestToken] = recvMsg;
        }

        #region Boardcast, Event listen

        public delegate void ActorEventListenver();

        /// <summary>
        /// Send all actor a event
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="data"></param>
        public void Boardcast(Enum eventName, object data)
        {
            
        }

        /// <summary>
        /// Listen
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="listener"></param>
        public void BindEvent(Enum eventName, ActorEventListenver listener)
        {

        }

        /// <summary>
        /// Listen Once and UnBind
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="listener"></param>
        public void OnceEvent(Enum eventName, ActorEventListenver listener)
        {

        }

        /// <summary>
        /// Stop Listen
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="listener"></param>
        public void UnBindEvent(Enum eventName, ActorEventListenver listener)
        {

        }
        #endregion

    }

}
