//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.34209
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;

namespace Cosmos.Rpc
{

    /// <summary>
    /// 使用ZeroMQ进行RPC
    /// </summary>
    public class RpcServer : IDisposable
    {
        internal NetMQContext _context;
        private ResponseSocket _server;
        public int Port { get; private set; }
        public string Host { get; private set; }

        object RpcInstace;


        public Poller Poller;
        public RpcServer(object rpcInstance, string host = "0.0.0.0")
        {
            RpcInstace = rpcInstance;
            Poller = new Poller();
            Host = host;

            _context = NetMQContext.Create();
            _server = _context.CreateResponseSocket();

            Poller.AddSocket(_server);

            Port = _server.BindRandomPort("tcp://" + host);
            //_server.ReceiveReady += OnReceive;
            // Bind the server to a local TCP address
            //_server.Bind(uri);


            // Connect the client to the server
            //client.Connect("tcp://localhost:5556");

            //// Send a message from the client socket
            //client.Send("Hello");

            //// Receive the message from the server socket
            //string m1 = _server.re();
            //Console.WriteLine("From Client: {0}", m1);

            _server.ReceiveReady += OnReceiveReady;
            Response();

            // Send a response back from the server
            //_server.Send("Hi Back");

            // Receive the response from the client socket
            //string m2 = client.ReceiveString();
            //Console.WriteLine("From Server: {0}", m2);
        }

        private void OnReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            var data = _server.Receive();
            var req = RpcShare.RequestSerializer.UnpackSingleObject(data);

            ProcessRequest(req);
        }

        async void ProcessRequest(RpcRequestProto requestProto)
        {
            Console.WriteLine("From Client: {0}", requestProto.RequestId);
            var method = RpcInstace.GetType().GetMethod(requestProto.FuncName);

            var arguments = new object[requestProto.Arguments.Length];
            for (var i = 0; i < arguments.Length; i++) // MsgPack.MessagePackObject arg in requestProto.Arguments)
            {
                MsgPack.MessagePackObject arg = (MsgPack.MessagePackObject)requestProto.Arguments[i];
                arguments[i] = arg.ToObject();
            }
            var result = method.Invoke(RpcInstace, arguments);
            object executeResult;
            if (result is Task)
            {
                executeResult = await(result as Task<object>);
            } else
            {
                executeResult = result;
            }

            var data = RpcShare._responseSerializer.PackSingleObject(new RpcResponseProto {
                RequestId = requestProto.RequestId,
                Result = executeResult,
            });
            _server.Send(data);
        }

        //private void OnReceive(object sender, NetMQSocketEventArgs e)
        //{

        //}

        //public RpcServer(int port, string host = "0.0.0.0") : this(string.Format("tcp://{0}:{1}", host, port))
        //{


        //}

        async void Response()
        {
            await Task.Run(() =>
            {

                Poller.Start();

                while (true)
                {
                    //var recv = _server.ReceiveString();
                    //int i;
                    //i = 0;
                }
            });
        }

        public void Dispose()
        {
            Poller.RemoveSocket(_server);
            _server.Close();
            _context.Dispose();
            Poller.Dispose();
        }
    }
}

