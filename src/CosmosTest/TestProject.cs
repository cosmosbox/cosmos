using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
using Cosmos.Actor;
using Cosmos.Framework.Components;
using ExampleProject;
using NUnit.Framework;

namespace CosmosTest
{
    [TestFixture]
    class TestProject
    {
        [Test]
        public async void TestStartProjectActor()
        {
            var app = new ExampleServerApp();
            app.StartActor("gate-actor-1");

            var client = new HandlerClient("127.0.0.1", 13001);
            var result = await client.Call<string>("TestHandler");
            Assert.AreEqual(result, "TestHandlerString");
            Assert.Pass();
        }
    }
}
