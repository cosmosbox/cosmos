
using NUnit.Framework;
using System;
using Cosmos.Rpc;
using System.Threading.Tasks;
using System.IO;
using Cosmos.Actor;
using MsgPack.Serialization;
namespace Cosmos.Test
{
    [TestFixture()]
    public class TestRpc
    {
        [Test()]
        public void TestAdd()
        {
            int a = 1;
            int b = 2;
            int sum = a + b;
            Assert.AreEqual(sum, 3);
        }

        public struct TestStruct
        {
            public string A;
            public int B;

        }
        /// <summary>
        /// Simple Test for msg pack
        /// </summary>
        [Test()]
        public void TestMsgPack()
        {
            // Creates serializer.
            var serializer = MessagePackSerializer.Get<TestStruct>();
            var stream = new MemoryStream();
            //var serializer = SerializationContext.Default.GetSerializer<TestStruct>();
            // Pack obj to stream.

            serializer.Pack(stream, new TestStruct
            {
                A = "Abc",
                B = 123123
            });
            stream.Position = 0;
            // Unpack from stream.
            var unpackedObject = serializer.Unpack(stream);
            Assert.AreEqual(unpackedObject.A, "Abc");
            Assert.AreEqual(unpackedObject.B, 123123);
        }

        public class TestActorRpcer : IActorRpcer
        {
            public string TestFunc(string arg1, string arg2)
            {
                return arg1 + arg2;
            }
            public string TestFunc2(string arg1, int arg2)
            {
                return string.Format("{0}{1}", arg1, arg2);
            }
        }

        [Test()]
        public void TestCallRpc()
        {
            using (var server = new RpcServer(new TestActorRpcer()))
            {
                Assert.AreEqual(server.Port.GetType(), typeof(int));
                Assert.GreaterOrEqual(server.Port, 0);
                Assert.AreEqual(server.Host, "0.0.0.0");

                using (var server2 = new RpcServer(new TestActorRpcer(), "127.0.0.1"))
                    Assert.AreEqual(server2.Host, "127.0.0.1");

                //Assert.AreEqual(server.Port, 5506);
                using (var client = new RpcClient("127.0.0.1", server.Port))
                {
                    var result = client.Call<string>("TestFunc", "ABC", "DEFG");
                    result.Wait();
                    Assert.AreEqual(result.Result, "ABCDEFG");

                    var result2 = client.CallResult<string>("TestFunc2", "ABC", 123);
                    result2.Wait();
                    Assert.AreEqual(result2.Result.Value, "ABC123");

                    var result3 = client.Call<string>("TestFunc3");
                    result3.Wait();
                    Assert.AreEqual(result3.IsCanceled, false);
                    Assert.AreEqual(result3.IsFaulted, false);
                    Assert.AreEqual(result3.Result, null);

                    Console.WriteLine("Async Rpc Server Done!");

                }
            }
            //Assert.Pass();
            //Assert.Pass("Success async rpc");
        }
    }
}

