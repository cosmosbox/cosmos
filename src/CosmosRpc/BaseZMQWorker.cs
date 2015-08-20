using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;

namespace Cosmos.Rpc
{
    class BaseZmqWorker : IDisposable
    {
        internal ZSocket workerSocket;
        private BaseNetMqServer _server;
        public BaseZmqWorker(BaseNetMqServer server, string backendAddr, int workerIndex)
        {
            _server = server;
            workerSocket = new ZSocket(NetMqManager.Instance.Context, ZSocketType.REQ);
            workerSocket.IdentityString = string.Format("{0}-{1}", server.ServerToken, workerIndex);
            workerSocket.Connect(backendAddr);

            new Thread(MainLoop).Start();
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

            while (true)
            {
                if (null == (incoming = workerSocket.ReceiveMessage(out error)))
                {
                    if (error == ZError.ETERM)
                        return;

                    throw new ZException(error);
                }

                using (incoming)
                {
                    // Send message back
                    //worker.Send(incoming);
                    _server.OnRecvMsg(this, incoming);
                    
                }
            }
        }

        public void Dispose()
        {
            workerSocket.Dispose();
        }
    }
}
