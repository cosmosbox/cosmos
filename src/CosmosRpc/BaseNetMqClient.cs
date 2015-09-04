using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cosmos.Utils;
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

        ZError error;

        protected BaseNetMqClient(string host, int responsePort, int subscribePort, string protocol = "tcp")
        {
            SessionToken = null;
            Host = host;
            ResponsePort = responsePort;
            SubscribePort = subscribePort;
            Protocol = protocol;

            // request
            _requestSocket = CreateSocket(out error);
        }

        ZSocket CreateSocket(out ZError error)
        {
            var socket = new ZSocket(NetMqManager.Instance.Context, ZSocketType.REQ);
            socket.Connect(ReqAddress);
            socket.IdentityString = BaseNetMqServer.GenerateKey("CLIENT");
            socket.Linger = TimeSpan.FromMilliseconds(1);

            if (!socket.Connect(ReqAddress, out error))
            {
                return null;
            }
            return socket;
        }

        public void Dispose()
        {
            SessionToken = null;

            _requestSocket.Dispose();
            //_requestSocket.Close();
        }

        //private void OnSubscriberReceiveReady(object sender, NetMQSocketEventArgs e)
        //{
        //    string messageTopicReceived = _subSocket.ReceiveString();
        //    byte[] messageReceived = _subSocket.Receive();

        //    Console.WriteLine("Topic: {0}", messageTopicReceived);
        //    Console.WriteLine("Message: {0}", messageReceived);
        //}
        protected async Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest obj)
        {
            var reqData = MsgPackTool.GetBytes(obj);
            var resData = await RequestAsync(reqData);

            if (resData == null)
            {
                return default(TResponse);
            }
            return MsgPackTool.GetMsg<TResponse>(resData);
        }
        protected IEnumerator<TResponse> Request<TRequest, TResponse>(TRequest obj)
        {
            var reqData = MsgPackTool.GetBytes(obj);
            var resData = Coroutine<byte[]>.Start(Request(reqData));
            while (!resData.IsFinished)
                yield return default(TResponse);

            if (resData.Result == null)
            {
                yield return default(TResponse);
                yield break;
            }
            yield return MsgPackTool.GetMsg<TResponse>(resData.Result);
        }

        /// <summary>
        /// 创建请求消息
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private byte[] CreateRequestMsg(byte[] obj)
        {
            var requestMsg = new BaseRequestMsg()
            {
                SessionToken = SessionToken,
                RequestToken = BaseNetMqServer.GenerateRequestKey(), //Path.GetRandomFileName(),
                Data = obj,
            };

            return MsgPackTool.GetBytes(requestMsg);
        } 

        protected IEnumerator<byte[]> Request(byte[] obj)
        {
            var retryCount = 5;
            while (retryCount > 0)
            {
                var bytes = CreateRequestMsg(obj);

                // We send a request, then we work to get a reply
                if (!_requestSocket.Send(new ZFrame(bytes), out error))
                {
                    if (error == ZError.ETERM)
                        continue;    // Interrupted
                    throw new ZException(error);
                }
                var poll = ZPollItem.CreateReceiver();
                ZMessage incoming;

                bool result = false;
                var pollStartTime = DateTime.UtcNow;
                var timeoutTime = 5f;
                do
                {
                    result = _requestSocket.PollIn(poll, out incoming, out error, TimeSpan.FromMilliseconds(1));
                } while (
                    !result && 
                    (DateTime.UtcNow - pollStartTime).TotalSeconds < timeoutTime); // timeout

                if (!result)
                {
                    Logger.Error("超时重试");
                    if (error == ZError.EAGAIN)
                    {
                        if (--retryCount == 0)
                        {
                            Console.WriteLine("E: server seems to be offline, abandoning");
                            break;
                        }
                        // Old socket is confused; close it and open a new one
                        _requestSocket.Dispose();
                        if (null == (_requestSocket = CreateSocket(out error)))
                        {
                            if (error == ZError.ETERM)
                            {
                                Logger.Error("ETERM!");
                                break; // Interrupted
                            }
                            throw new ZException(error);
                        }

                        Console.WriteLine("I: reconnected");

                        continue;
                    }
                    if (error == ZError.ETERM)
                    {
                        Logger.Error("ETERM!!");
                        break; // Interrupted
                    }
                    throw new ZException(error);
                }
                else
                {
                    using (incoming)
                    {
                        // We got a reply from the server
                        //int incoming_sequence = incoming[0].ReadInt32();
                        //var recvMessage = _requestSocket.ReceiveMessage();
                        var recvMsg = MsgPackTool.GetMsg<BaseResponseMsg>(incoming[0].Read());
                        _responses[recvMsg.RequestToken] = recvMsg;
                        SessionToken = recvMsg.SessionToken;
                        if (string.IsNullOrEmpty(SessionToken))
                            throw new Exception(string.Format("Error Session token when get response"));

                        yield return recvMsg.Data;
                        yield break;
                    }
                }
            }

            yield return null;
        }

        protected async Task<byte[]> RequestAsync(byte[] obj)
        {
            var retryCount = 5;
            while (retryCount > 0)
            {
                var bytes = CreateRequestMsg(obj);

                // We send a request, then we work to get a reply
                if (!_requestSocket.Send(new ZFrame(bytes), out error))
                {
                    if (error == ZError.ETERM)
                        continue;    // Interrupted
                    throw new ZException(error);
                }
                var poll = ZPollItem.CreateReceiver();
                ZMessage incoming;


                bool result = false;
                var pollStartTime = DateTime.UtcNow;
                var timeoutTime = 5f;
                do
                {
                    await Task.Delay(1);
                    result = _requestSocket.PollIn(poll, out incoming, out error, TimeSpan.FromMilliseconds(1));
                } while (
                    !result &&
                    (DateTime.UtcNow - pollStartTime).TotalSeconds < timeoutTime); // timeout

                if (!result)
                {
                    Logger.Error("超时重试");
                    if (error == ZError.EAGAIN)
                    {
                        if (--retryCount == 0)
                        {
                            Console.WriteLine("E: server seems to be offline, abandoning");
                            break;
                        }
                        // Old socket is confused; close it and open a new one
                        _requestSocket.Dispose();
                        if (null == (_requestSocket = CreateSocket(out error)))
                        {
                            if (error == ZError.ETERM)
                            {
                                Logger.Error("ETERM!");
                                break; // Interrupted
                            }
                            throw new ZException(error);
                        }

                        Console.WriteLine("I: reconnected");

                        continue;
                    }
                    if (error == ZError.ETERM)
                    {
                        Logger.Error("ETERM!!");
                        break; // Interrupted
                    }
                    throw new ZException(error);
                }
                else
                {
                    using (incoming)
                    {
                        // We got a reply from the server
                        //int incoming_sequence = incoming[0].ReadInt32();
                        //var recvMessage = _requestSocket.ReceiveMessage();
                        var recvMsg = MsgPackTool.GetMsg<BaseResponseMsg>(incoming[0].Read());
                        _responses[recvMsg.RequestToken] = recvMsg;
                        SessionToken = recvMsg.SessionToken;
                        if (string.IsNullOrEmpty(SessionToken))
                            throw new Exception(string.Format("Error Session token when get response"));

                        return recvMsg.Data;
                    }
                }
            }

            return null;
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
