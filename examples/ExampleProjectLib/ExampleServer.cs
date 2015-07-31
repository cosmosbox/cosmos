using System;
using System.Threading.Tasks;
using Cosmos.Framework;

namespace ExampleProject
{
    public class ExampleServer : CosmosApp
    {
        public ExampleServer()
        {
            new HttpActor();
        }
    }
}

