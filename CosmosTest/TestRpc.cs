
using NUnit.Framework;
using System;
using Cosmos;
using Cosmos.Actor;
using CosmosActor.Rpc;
using System.Threading.Tasks;

namespace Cosmos.Test
{
    [TestFixture()]
    public class TestRpc
    {
        [Test()]
        public void TestCase()
        {
            int a = 1;
            int b = 2;
            int sum = a + b;
            Assert.AreEqual(sum, 3);
        }

        [Test()]
        public async void  TestCreateAServer()
        {
            using (var server = new RpcServer())
            {
                Assert.AreEqual(server.Port.GetType(), typeof(int));
                Assert.GreaterOrEqual(server.Port, 10000);
                Assert.AreEqual(server.Host, "0.0.0.0");

                using (var server2 = new RpcServer("127.0.0.1"))
                    Assert.AreEqual(server2.Host, "127.0.0.1");

                Console.Write("HERE NOW");
                //Assert.AreEqual(server.Port, 5506);
                using (var client = new RpcClient("127.0.0.1", server.Port))
                {


                    Console.Write("HERE NOW");
                    var request = client.Request("ABCDEFG");
                    await Task.Run(() =>
                    {
                        while (!request.IsCompleted) { }

                        Assert.AreEqual(request.Result, "ABCDEFG");

                        Assert.Pass("Success async rpc");
                    });
                    

                }
            }

        }
    }
}

