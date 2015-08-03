using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cosmos.Actor;
using Cosmos.Rpc;
using Cosmos.Tool;
using NUnit.Framework;

namespace CosmosTest
{
    public class SampleRpcCaller : IActorRpcer
    {
        public int Add(int a, int b)
        {
            return a + b;
        }
    }

    public class SampleActor : Actor
    {
        public override IActorRpcer NewRpcCaller()
        {
            return new SampleRpcCaller();
        }
    }


    [TestFixture]
    class TestActor
    {
        private ActorRunner _actorA;
        private ActorRunner _actorB;

        public TestActor()
        {
            // A server
            var discoverServers = new string[] {"http://127.0.0.1:4001"};
            var actorConf = new ActorConf()
            {
                Name = "Actor-Test-A",
                DiscoverServers = discoverServers,
                ActorClass = typeof(SampleActor),
            };
            _actorA = ActorRunner.Run(actorConf);
            Assert.AreEqual(_actorA.State, ActorRunState.Running);

            // B Server
            var actorConfB = new ActorConf()
            {
                Name = "Actor-Test-B",
                DiscoverServers = discoverServers,
                ActorClass = typeof(SampleActor),
            };
            _actorB = ActorRunner.Run(actorConfB);
            Assert.AreEqual(_actorB.State, ActorRunState.Running);
        }

        [Test]
        public async void RpcFromTwoActor()
        {
            //var addResult = await _actorA.Actor.Call<int>("Actor-Test-B", "Add", 1, 2);
            //Assert.AreEqual(addResult, 3);
        }

        [Test]
        public async void CreateActorByCode()
        {
            var co = Coroutine.Start(WaitRunner());
            await co;
            
            Assert.AreEqual(1, 1);
        }

        IEnumerator WaitRunner()
        {
            var actorConf = new ActorConf()
            {
                Name = "Actor-Test-1",
                ActorClass = typeof(SampleActor),
            };
            var runner1 = ActorRunner.Run(actorConf);
            Assert.AreEqual(runner1.SecondsTick, 0);

            while (runner1.State != ActorRunState.Running)
                yield return null;

            Assert.AreEqual(runner1.State, ActorRunState.Running);
            var runner2 = ActorRunner.GetActorStateByName("Actor-Test-1");
            Assert.AreEqual(runner2.State, ActorRunState.Running);
            Assert.AreEqual(runner2.ActorName, "Actor-Test-1");
        } 
    }
}
