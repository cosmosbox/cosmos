using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MsgPack.Serialization;
using NetMQ;
using NetMQ.Sockets;
using NLog;

namespace Cosmos.Rpc
{

    public class BaseRequestMsg : BaseNetMqMsg
    {
    }

    /// <summary>
    /// Request to the CosmosNetMq Handler
    /// </summary>
    public abstract class BaseNetMqClient : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private NetMQSocket _requestSocket;
        private SubscriberSocket _subSocket;

        //private Poller _poller;

        public string Host { get; private set; }

        public int ResponsePort { get; private set; }

        public string Protocol { get; private set; }

        public int SubscribePort { get; private set; }

        public string ReqAddress
        {
            get { return string.Format("{0}://{1}:{2}", Protocol, Host, ResponsePort); }
        }
        public string SubcribeAddress
        {
            get { return string.Format("{0}://{1}:{2}", Protocol, Host, SubscribePort); }
        }

        //private ConcurrentDictionary<string, BaseResponseMsg> _responses = new ConcurrentDictionary<string, BaseResponseMsg>();
        public string SessionToken { get; private set; }

        protected BaseNetMqClient(string host, int responsePort, int subscribePort, string protocol = "tcp")
        {
            SessionToken = null;
            Host = host;
            ResponsePort = responsePort;
            SubscribePort = subscribePort;
            Protocol = protocol;

            //_poller = new Poller();

            // subcribe
            //if (SubscribePort != 0)
            //{
            //    _subSocket = NetMqManager.Instance.Context.CreateSubscriberSocket();
            //    _subSocket.Options.ReceiveHighWatermark = 1000;
            //    _subSocket.Connect(SubcribeAddress);
            //    _subSocket.ReceiveReady += OnSubscriberReceiveReady;
            //    _poller.AddSocket(_subSocket);
            //}


            // request
            _requestSocket = NetMqManager.Instance.Context.CreateRequestSocket();
            _requestSocket.Connect(ReqAddress);
            //_requestSocket.ReceiveReady += OnRequestReceiveReady;
            _requestSocket.Options.ReceiveHighWatermark = 1024;
            _requestSocket.Options.SendHighWatermark = 1024;

            //_poller.AddSocket(_requestSocket);

            // run poller
            //_poller.PollTillCancelledNonBlocking();
        }

        public void Dispose()
        {
            SessionToken = null;

            _requestSocket.Disconnect(ReqAddress);
            _requestSocket.Close();
        }

        private void OnSubscriberReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            string messageTopicReceived = _subSocket.ReceiveString();
            byte[] messageReceived = _subSocket.Receive();

            Console.WriteLine("Topic: {0}", messageTopicReceived);
            Console.WriteLine("Message: {0}", messageReceived);
        }

        protected async Task<TResponse> Request<TRequest, TResponse>(TRequest obj)
        {
            var reqData = MsgPackTool.GetBytes(obj);
            var resData = await Request(reqData);

            return MsgPackTool.GetMsg<TResponse>(resData);
        }
        static int Reqid = 0;
        protected async Task<byte[]> Request(byte[] obj)
        {
            return await Task.Run(() =>
            {
                var requestMsg = new BaseRequestMsg()
                {
                    SessionToken = SessionToken,
                    RequestToken = BaseNetMqServer.GenerateRequestKey(), //Path.GetRandomFileName(),
                    Data = obj,
                };

                var bytes = MsgPackTool.GetBytes(requestMsg);

                _requestSocket.Send(bytes);

                var recvMessage = _requestSocket.ReceiveMessage();
                var recvMsg = MsgPackTool.GetMsg<BaseResponseMsg>(recvMessage[0].Buffer);
                if (recvMsg.RequestToken != requestMsg.RequestToken)
                    throw new Exception("not equal request token!");
                var responseData = recvMsg;

                SessionToken = recvMsg.SessionToken;
                return responseData.Data;

                //bool result = _requestSocket.Poll(TimeSpan.FromSeconds(5));

                //if (result)
                //{
                //    //var recvMessage = _requestSocket.ReceiveMessage();
                //    //var recvMsg = MsgPackTool.GetMsg<BaseResponseMsg>(recvMessage[0].Buffer);

                //    //var responseData = recvMsg;

                //    BaseResponseMsg tmpMsg;
                //    _responses.TryRemove(requestMsg.RequestToken, out tmpMsg); // must true!

                //    SessionToken = tmpMsg.SessionToken;
                //    if (string.IsNullOrEmpty(SessionToken))
                //        throw new Exception(string.Format("Error Session token when get response"));

                //    return tmpMsg.Data;
                //}
                //return null;
            });
        }
        //private void OnRequestReceiveReady(object sender, NetMQSocketEventArgs e)
        //{
        //    var recvMessage = _requestSocket.ReceiveMessage();
        //    var recvMsg = MsgPackTool.GetMsg<BaseResponseMsg>(recvMessage[0].Buffer);
        //    _responses[recvMsg.RequestToken] = recvMsg;
        //}

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


        public void Subcribe(string topic)
        {
            if (_subSocket != null)
                _subSocket.Subscribe(topic);
            else
            {
                Logger.Error("No Subcribe Socket");
            }
        }
    }

}
