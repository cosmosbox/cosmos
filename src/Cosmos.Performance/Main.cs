using System;
using System.Threading;
using Cosmos.Actor;

namespace Cosmos.Test.Performance
{
	class MainClass
	{
		public static void Main (string[] args)
		{
            var configA = new ActorNodeConfig();
            configA.AppToken = "AppToken1";
            configA.ActorClass = "Cosmos.Test.Performance.ActorA, Cosmos.Test.Performance";
		    configA.Name = "TestActorA";
		    configA.DiscoveryParam = "discovery.json";

            var configB = new ActorNodeConfig();
            configB.AppToken = "AppToken1";
            configB.ActorClass = "Cosmos.Test.Performance.ActorB, Cosmos.Test.Performance";
            configB.Name = "TestActorB";
            configB.DiscoveryParam = "discovery.json";

            ActorRunner.Run(configA);
            ActorRunner.Run(configB);

		    while (true)
		    {
		        Thread.Sleep(1000);
		    }
        }
	}
}
