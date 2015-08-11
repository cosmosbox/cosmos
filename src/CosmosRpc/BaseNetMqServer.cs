using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Tool;
using MsgPack.Serialization;
using NetMQ;
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
        internal NetMQContext _context;
        private NetMQSocket _responseSocket;
        private PublisherSocket _pubSocket;
        public int ResponsePort { get; private set; }
        public string Host { get; private set; }

        private Task _pollerTask;

        public Poller Poller;

        public BaseNetMqServer(int responsePort = -1, string host = "0.0.0.0")
        {
            Poller = new Poller();
            Host = host;

            _context = NetMQContext.Create();
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

            _responseSocket.ReceiveReady += OnResponseReceiveReady;


            //_pubSocket = _context.CreatePublisherSocket();
            //_pubSocket.Options.SendHighWatermark = 1000;
            //// Bind ? Connect? 
            //_pubSocket.Bind(string.Format("tcp://{0}:{1}", "*", requestPort));
            
            //Poller.AddSocket(_pubSocket);

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
            _context.Dispose();

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
            var now = DateTime.UtcNow;
            var random = new Random(now.Millisecond);
            var pureKeyStr = string.Format("{0}{1}", now.Ticks, random.Next(int.MinValue, int.MaxValue));

            return Md5Util.Hex(pureKeyStr);
        }
    }

}
