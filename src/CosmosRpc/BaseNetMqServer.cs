using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cosmos.Utils;
using MsgPack.Serialization;
using NetMQ;
using NetMQ.Actors;
using NetMQ.Sockets;
using NLog;
using NLog.LayoutRenderers;
using ZeroMQ;

namespace Cosmos.Rpc
{
    /// <summary>
    /// Mutable
    /// </summary>
    public class BaseResponseMsg : BaseNetMqMsg
    {
    }

    public abstract class BaseNetMqServer : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private ZSocket _responseSocket;
        private PublisherSocket _pubSocket;
        public int ResponsePort { get; private set; }
        public int PublishPort { get; private set; }
        public string Host { get; private set; }

        //private Task _pollerTask;

        //public Poller _poller;

        protected BaseNetMqServer(int responsePort = -1, int publishPort = 0, string host = "*")
        {
            Host = host;
            _responseSocket = new ZSocket(NetMqManager.Instance.Context, ZSocketType.ROUTER);

            if (responsePort == -1)
            {
                while (true)
                {
                    var rand = new Random();
                    ResponsePort = rand.Next(50000, 60000);
                    try
                    {
                        _responseSocket.Bind(string.Format("tcp://{0}:{1}", host, ResponsePort));
                        break;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
            else
            {
                ResponsePort = responsePort;
                _responseSocket.Bind(string.Format("tcp://{0}:{1}", host, ResponsePort));
            }
            //_responseSocket.Options.ReceiveHighWatermark = 1024;
            //_responseSocket.Options.SendHighWatermark = 1024;
            //_responseSocket.ReceiveReady += ProcescRequestMessage;
            new Thread(ProcescRequestMessage).Start();
            //if (publishPort != 0)
            //{
            //    PublishPort = publishPort;
            //    _pubSocket = NetMqManager.Instance.Context.CreatePublisherSocket();
            //    // Bind ? Connect? 
            //    _pubSocket.Bind(string.Format("tcp://{0}:{1}", Host, publishPort));

            //    _poller.AddSocket(_pubSocket);
            //}


            //_poller.PollTillCancelledNonBlocking();
        }

        /// <summary>
        /// Do Publisher
        /// </summary>
        /// <param name="topicName"></param>
        /// <param name="data"></param>
        public void Publish(string topicName, byte[] data)
        {
            _pubSocket.SendMore(topicName).Send(data);
        }

        private async void ProcescRequestMessage()
        {
            ZError error;
            while (true)
            {
                ZMessage recvMsg;
                if (null == (recvMsg = _responseSocket.ReceiveMessage(out error)))
                {
                    if (error == ZError.ETERM)
                        return;    // Interrupted
                    throw new ZException(error);
                }
                var startTime = DateTime.UtcNow;
                using (recvMsg)
                {
                    var clientAddr = recvMsg[0];
                    var clientData = recvMsg[2];
                    var baseRequestMsg = MsgPackTool.GetMsg<BaseRequestMsg>(clientData.Read());
                    var requestDataMsg = baseRequestMsg.Data;

                    var responseMsg = await ProcessRequest(requestDataMsg);

                    // if no session key, generate new
                    var sessionToken = baseRequestMsg.SessionToken;
                    if (string.IsNullOrEmpty(sessionToken))
                    {
                        sessionToken = GenerateSessionKey();
                    }
                    var baseResponseMsg = new BaseResponseMsg()
                    {
                        SessionToken = sessionToken,
                        RequestToken = baseRequestMsg.RequestToken,
                        Data = responseMsg,
                    };

                    var sendData = MsgPackTool.GetBytes(baseResponseMsg);

                    var messageToServer = new ZMessage();
                    messageToServer.Append(clientAddr);
                    messageToServer.Append(ZFrame.CreateEmpty());
                    messageToServer.Append(new ZFrame(sendData));

                    _responseSocket.SendMessage(messageToServer);
                }

                Logger.Trace("Receive Msg and Send used Time: {0:F5}s", (DateTime.UtcNow - startTime).TotalSeconds);
            }
        }
        //private async void ThreadLoopReceive()//(object sender, NetMQSocketEventArgs e)
        //{
        //    while (true)
        //    {
        //        _responseSocket.Poll();
        //        //NetMQMessage recvMsg;
        //        //recvMsg = _responseSocket.ReceiveMessage();

        
        //        //await Task.Run(() => { ProcescRequestMessage(recvMsg); });

        
        //    }

        //}

        protected abstract Task<byte[]> ProcessRequest(byte[] requestDataMsg);

        public void Dispose()
        {
            //_poller.RemoveSocket(_responseSocket);

            //_responseSocket.Disconnect();
            _responseSocket.Close();


            //_poller.CancelAndJoin();
            //_poller.Dispose();

            //_pollerTask.Dispose();
        }

        /// <summary>
        /// Create a new Session Key of Hex
        /// </summary>
        /// <returns></returns>
        public static string GenerateSessionKey()
        {
            return GenerateKey("S");
        }
        public static string GenerateRequestKey()
        {
            return GenerateKey("REQ");
        }

        static string GenerateKey(string suffix)
        {
            var now = DateTime.UtcNow;
            var random = new Random(now.Millisecond);
            var pureKeyStr = string.Format("{0}{1}{2}", suffix, now.Ticks, random.Next(int.MinValue, int.MaxValue));

            return Md5Util.String16(pureKeyStr);
        }
    }

}
