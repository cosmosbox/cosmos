using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.zmq;
using ZeroMQ;

namespace Cosmos.Rpc
{
    class NetMqManager
    {
        public readonly static NetMqManager Instance = new NetMqManager();


        //public readonly Poller Poller = new Poller(new NetMQTimer(1));

        public readonly ZContext Context;

        private NetMqManager()
        {
            //Poller.PollTillCancelledNonBlocking();

            Context = new ZContext();

            //Context.SetOption(ZContextOption.IO_THREADS, 1);
            
            //Context.MaxSockets = 10240;
            //Context.ThreadPoolSize = 128;
        }


    }
}
