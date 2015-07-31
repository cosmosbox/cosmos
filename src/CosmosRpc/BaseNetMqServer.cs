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
    public class MsgPackTool
    {
        public static void WriteStream<T>(MemoryStream stream, T responseMsg)
        {
            var serializer = MessagePackSerializer.Get<T>();
            serializer.Pack(stream, responseMsg);
        }
        public static T ReadStream<T>(MemoryStream stream)
        {
            var serializer = MessagePackSerializer.Get<T>();
            return serializer.Unpack(stream);
        }

        public static byte[] GetBytes<T>(T msg)
        {
            var serializer = MessagePackSerializer.Get<T>();
            return serializer.PackSingleObject(msg);
        }
        public static T GetMsg<T>(byte[] data)
        {
            var serializer = MessagePackSerializer.Get<T>();
            return serializer.UnpackSingleObject(data);
        }

    }

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
        private ResponseSocket _server;
        public int Port { get; private set; }
        public string Host { get; private set; }

        private Task _pollerTask;

        public Poller Poller;

        public BaseNetMqServer(int port = -1, string host = "0.0.0.0")
        {
            Poller = new Poller();
            Host = host;

            _context = NetMQContext.Create();
            _server = _context.CreateResponseSocket();

            Poller.AddSocket(_server);

            if (port == -1)
            {
                Port = _server.BindRandomPort("tcp://" + host);
            }
            else
            {
                Port = port;
                _server.Bind(string.Format("tcp://{0}:{1}", host, Port));
            }
            
            _server.ReceiveReady += OnReceiveReady;
            _pollerTask = Task.Run(() =>
            {
                Poller.Start();
            });
        }

        private async void OnReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            var recvData = _server.Receive();
            var baseRequestMsg = MsgPackTool.GetMsg<BaseRequestMsg>(recvData);
            var requestDataMsg = baseRequestMsg.Data;

            var responseMsg = await ProcessRequest(requestDataMsg);
            var baseResponseMsg = new BaseResponseMsg()
            {
                RequestToken = baseRequestMsg.RequestToken,
                Data = responseMsg,
            };

            var sendData = MsgPackTool.GetBytes(baseResponseMsg);

            _server.Send(sendData);
        }

        protected virtual async Task<byte[]> ProcessRequest(byte[] requestDataMsg)
        {
            Console.WriteLine("[ERROR]Null Response Msg!");

            return null;
        }
        

        public void Dispose()
        {
            Poller.RemoveSocket(_server);
            _server.Close();
            _context.Dispose();

            Poller.Stop();
            Poller.Dispose();


            _pollerTask.Dispose();
        }
    }

}
