using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;

namespace Cosmos.Rpc
{
    class NetMqManager
    {
        public readonly static NetMqManager Instance = new NetMqManager();


        //public readonly Poller Poller = new Poller(new NetMQTimer(1));

        public readonly NetMQContext Context;

        private NetMqManager()
        {
            //Poller.PollTillCancelledNonBlocking();

            Context = NetMQContext.Create();
            Context.MaxSockets = 10240;
            Context.ThreadPoolSize = 128;
        }


    }
}
