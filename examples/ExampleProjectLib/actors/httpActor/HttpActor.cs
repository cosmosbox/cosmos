using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Actor;
using Cosmos.Rpc;
using Nancy.ModelBinding;

namespace ExampleProject
{

    internal class HttpActorActorRpcer : IActorRpcer
    {
        public int Add(int a, int b)
        {
            return a + b;
        }
    }

    class HttpActor : Actor
    {
        public HttpActor()
        {
            var httpHandler = new HttpHandler();
            httpHandler.Start();

        }
        public override IActorRpcer NewRpcCaller()
        {
            return new HttpActorActorRpcer();
        }
    }

}
