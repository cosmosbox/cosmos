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
using ZeroMQ;

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

        private ZSocket _requestSocket;
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

        private ConcurrentDictionary<string, BaseResponseMsg> _responses = new ConcurrentDictionary<string, BaseResponseMsg>();
        public string SessionToken { get; private set; }

        protected BaseNetMqClient(string host, int responsePort, int subscribePort, string protocol = "tcp")
        {
            SessionToken = null;
            Host = host;
            ResponsePort = responsePort;
            SubscribePort = subscribePort;
            Protocol = protocol;

            // request
            _requestSocket = new ZSocket(NetMqManager.Instance.Context, ZSocketType.DEALER);
            _requestSocket.Connect(ReqAddress);
            //_requestSocket.ReceiveReady += OnRequestReceiveReady;
            //_requestSocket.Options.ReceiveHighWatermark = 1024;
            //_requestSocket.Options.SendHighWatermark = 1024;

        }

        public void Dispose()
        {
            SessionToken = null;

            _requestSocket.Dispose();
            //_requestSocket.Close();
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

        private string _clientId = null;
        public string CleintId
        {
            get
            {
                if (_clientId == null)
                    _clientId = BaseNetMqServer.GenerateSessionKey();
                return _clientId;
            }
        }
        protected async Task<byte[]> Request(byte[] obj)
        {
            return await Task.Run(() =>
            {
                while (true)
                {
                    _requestSocket.IdentityString = CleintId;
                    var requestMsg = new BaseRequestMsg()
                    {
                        SessionToken = SessionToken,
                        RequestToken = BaseNetMqServer.GenerateRequestKey(), //Path.GetRandomFileName(),
                        Data = obj,
                    };

                    var bytes = MsgPackTool.GetBytes(requestMsg);
                    ZError error;
                    //mqMsg.Append(requestMsg.RequestToken);
                    // We send a request, then we work to get a reply

                    //_requestSocket.SetOption(ZSocketOption.IDENTITY, CleintId);
                    //string idClien;
                    //_requestSocket.GetOption(ZSocketOption.IDENTITY, out idClien);
                    using (var mqMsg = new ZMessage())
                    {
                        _requestSocket.SendMore(new ZFrame(_requestSocket.IdentityString));
                        _requestSocket.SendMore(ZFrame.CreateEmpty());
                        _requestSocket.Send(new ZFrame(bytes));
                        //if (!_requestSocket.SendMessage(mqMsg, out error))
                        //{
                        //    if (error == ZError.ETERM)
                        //        return null;    // Interrupted
                        //    throw new ZException(error);
                        //}
                    }

                    var poll = ZPollItem.CreateReceiver();
                    ZMessage incoming;
                    var result = _requestSocket.PollIn(poll, out incoming, out error, TimeSpan.FromSeconds(5)); //(TimeSpan.FromSeconds(5));
                    if (!result)
                    {
                        Logger.Error("超时重试");
                        continue;
                    }
                    else
                    {
                        using (incoming)
                        {
                            // We got a reply from the server
                            //int incoming_sequence = incoming[0].ReadInt32();
                            //var recvMessage = _requestSocket.ReceiveMessage();
                            var recvMsg = MsgPackTool.GetMsg<BaseResponseMsg>(incoming[1].Read());
                            _responses[recvMsg.RequestToken] = recvMsg;
                            SessionToken = recvMsg.SessionToken;
                            if (string.IsNullOrEmpty(SessionToken))
                                throw new Exception(string.Format("Error Session token when get response"));

                            return recvMsg.Data;
                        }
                    }

                    //BaseResponseMsg tmpMsg;
                    //while (!_responses.TryGetValue(requestMsg.RequestToken, out tmpMsg))
                    //{
                    //    // must true!
                    //    //if (!removeResult)
                    //    //    throw new Exception(string.Format("Error TryRemove RequestToken Msg: {0}",
                    //    // TODO: remove

                    //    // requestMsg.RequestToken));
                    //    Thread.Sleep(1);
                    //}
                }
            });
        }
        //private void OnRequestReceiveReady(object sender, NetMQSocketEventArgs e)
        //{
        //    var recvMessage = _requestSocket.ReceiveMessage();
        //    var recvMsg = MsgPackTool.GetMsg<BaseResponseMsg>(recvMessage.Last.Buffer);
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
