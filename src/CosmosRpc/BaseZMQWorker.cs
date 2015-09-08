using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cosmos.Utils;
using NLog;
using ZeroMQ;

namespace Cosmos.Rpc
{
    class BaseZmqWorker : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        internal ZSocket workerSocket;
        private BaseNetMqServer _server;
        private Thread _workerThread;
        public BaseZmqWorker(BaseNetMqServer server, string backendAddr, int workerIndex)
        {
            _server = server;
            workerSocket = new ZSocket(NetMqManager.Instance.Context, ZSocketType.REQ);
            workerSocket.IdentityString = string.Format("{0}-{1}", server.ServerToken, workerIndex);
            workerSocket.Connect(backendAddr);

            _workerThread = new Thread(MainLoop);
            _workerThread.Start();

            //Coroutine2.Start(MainLoop(null));
            //MainLoop(null);
            //ThreadPool.QueueUserWorkItem(new WaitCallback(MainLoop), null);

        }

        void MainLoop(object obj)
        {
            using (var outgoing = new ZFrame("READY"))
            {
                workerSocket.Send(outgoing);
            }

            ZError error;
            ZMessage incoming;
            var poll = ZPollItem.CreateReceiver();
            while (true)
            {
                while (true)
                {
                    var result = workerSocket.PollIn(poll, out incoming, out error, TimeSpan.FromMilliseconds(1));
                    if (!result)
                    {
                        if (error == ZError.EAGAIN)
                        {
                            //await Task.Delay(1);
                            //yield return null;
                            continue;
                        }
                        if (error == ZError.ETERM)
                        {
                            Logger.Error("ETERM!");
                            return; // Interrupted
                            //yield break;
                        }
                        throw new ZException(error);
                    }
                    else
                    {
                        using (incoming)
                        {
                            // Send message back
                            //worker.Send(incoming);
                            _server.OnRecvMsg(this, incoming);
                            break;
                        }
                    }
                }
                //if (null == (incoming = workerSocket.ReceiveMessage(out error)))
                //{
                //    if (error == ZError.ETERM)
                //        return;

                //    throw new ZException(error);
                //}

                //using (incoming)
                //{
                //    // Send message back
                //    //worker.Send(incoming);
                //    _server.OnRecvMsg(this, incoming);
                    
                //}
            }
        }

        public void Dispose()
        {
            workerSocket.Dispose();
        }
    }
}
