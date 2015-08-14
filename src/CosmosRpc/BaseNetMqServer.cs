using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cosmos.Tool;
using MsgPack.Serialization;
using NetMQ;
using NetMQ.Actors;
using NetMQ.Sockets;

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
        static NetMQContext _context;
        static BaseNetMqServer()
        {
            _context = NetMQContext.Create();
            _context.MaxSockets = 10240;
            _context.ThreadPoolSize = 128;
        }

        private NetMQSocket _responseSocket;
        private PublisherSocket _pubSocket;
        public int ResponsePort { get; private set; }
        public int PublishPort { get; private set; }
        public string Host { get; private set; }

        private Task _pollerTask;

        public Poller Poller;

        public BaseNetMqServer(int responsePort = -1, int publishPort = 0, string host = "*")
        {
            Poller = new Poller(new NetMQTimer(1));
            Host = host;

            _responseSocket = _context.CreateResponseSocket();
            Poller.AddSocket(_responseSocket);

            if (responsePort == -1)
            {
                ResponsePort = _responseSocket.BindRandomPort("tcp://" + host);
                
            }
            else
            {
                ResponsePort = responsePort;
                _responseSocket.Bind(string.Format("tcp://{0}:{1}", host, ResponsePort));
            }
            _responseSocket.Options.ReceiveHighWatermark = 1024;
            _responseSocket.Options.SendHighWatermark = 1024;
            _responseSocket.ReceiveReady += OnResponseReceiveReady;

            if (publishPort != 0)
            {
                PublishPort = publishPort;
                _pubSocket = _context.CreatePublisherSocket();
                // Bind ? Connect? 
                _pubSocket.Bind(string.Format("tcp://{0}:{1}", Host, publishPort));

                Poller.AddSocket(_pubSocket);
            }


            _pollerTask = Task.Run(() =>
            {
                Poller.Start();
            });
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

        private async void OnResponseReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            //var recvMsg = _responseSocket.ReceiveMessage();
            var recvData = _responseSocket.Receive();
            //var recvData2 = _responseSocket.Receive();
            var baseRequestMsg = MsgPackTool.GetMsg<BaseRequestMsg>(recvData);
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
            
            _responseSocket.Send(sendData);
        }

        protected abstract Task<byte[]> ProcessRequest(byte[] requestDataMsg);

        public void Dispose()
        {
            Poller.RemoveSocket(_responseSocket);
            _responseSocket.Close();
            //_context.Dispose();

            Poller.Stop();
            Poller.Dispose();


            _pollerTask.Dispose();
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
            var pureKeyStr = string.Format("{0}{1}{2}",suffix, now.Ticks, random.Next(int.MinValue, int.MaxValue));

            return Md5Util.String16(pureKeyStr);
        }
    }

}
