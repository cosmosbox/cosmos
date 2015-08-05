using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Actor;
using Cosmos.Framework.Components;
using NUnit.Framework;

namespace CosmosTest
{
    [TestFixture]
    class TestClientServerActor
    {
        /// <summary>
        /// 创建一个Actor，并且使用客户端联系之
        /// </summary>
        [Test]
        public async void TestClientToServer()
        {
            var actorConf = new ActorNodeConfig
            {
                AppToken = "TestApp",
                Name = "Actor-Test-1",
                ActorClass = "CosmosTest.SampleActor, CosmosTest",
                Host = "*",
                RpcPort = 12300,

                ResponsePort = 12311,
                DiscoveryMode = "Json",
                DiscoveryParam = "config/actors.json"

            };
            ActorRunner.Run(actorConf);

            // Handler
            var client = new HandlerClient("127.0.0.1", 12311);
            var result = await client.Call<string>("Test");
            Assert.AreEqual(result, "TestString");
            Assert.Pass();
        }
    }
}
