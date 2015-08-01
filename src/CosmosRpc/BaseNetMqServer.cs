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
    /// <summary>
    /// Mutable
    /// </summary>
    public struct BaseResponseMsg
    {
        // Fields...
        public string RequestToken;
        public byte[] Data;
    }

    public abstract class BaseNetMqServer : IDisposable
    {
        internal NetMQContext _context;
        private ResponseSocket _responseSocket;
        private PublisherSocket _pubSocket;
        public int Port { get; private set; }
        public string Host { get; private set; }

        private Task _pollerTask;

        public Poller Poller;

        public BaseNetMqServer(int port = -1, string host = "0.0.0.0")
        {
            Poller = new Poller();
            Host = host;

            _context = NetMQContext.Create();
            _responseSocket = _context.CreateResponseSocket();
            Poller.AddSocket(_responseSocket);

            if (port == -1)
            {
                Port = _responseSocket.BindRandomPort("tcp://" + host);
            }
            else
            {
                Port = port;
                _responseSocket.Bind(string.Format("tcp://{0}:{1}", host, Port));
            }

            _pubSocket = _context.CreatePublisherSocket();
            _pubSocket.Options.SendHighWatermark = 1000;
            _pubSocket.Bind(string.Format("tcp://{0}:{1}", host, Port));
            //Poller.AddSocket(_pubSocket);

            _responseSocket.ReceiveReady += OnResponseReceiveReady;
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
            var recvData = _responseSocket.Receive();
            var baseRequestMsg = MsgPackTool.GetMsg<BaseRequestMsg>(recvData);
            var requestDataMsg = baseRequestMsg.Data;

            var responseMsg = await ProcessRequest(requestDataMsg);
            var baseResponseMsg = new BaseResponseMsg()
            {
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
    }

}
