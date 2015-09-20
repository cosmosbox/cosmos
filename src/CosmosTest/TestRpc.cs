
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using Cosmos.Rpc;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using Cosmos.Actor;
using Cosmos.Utils;
using MsgPack.Serialization;
using NLog;

namespace Cosmos.Test
{
    [TestFixture()]
    public class TestRpc
    {
        [Test]
        public void SessionTokenGenerate()
        {
            var token = BaseNetMqServer.GenerateSessionKey();
            Assert.AreEqual(16, token.Length);
        }
        [Test()]
        public void TestAdd()
        {
            int a = 1;
            int b = 2;
            int sum = a + b;
            Assert.AreEqual(sum, 3);
        }

        /// <summary>
        /// A struct for test MsgPack
        /// 一个用于测试MsgPack的构造器
        /// </summary>
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

        public class TestActorService : IActorService
        {
            public struct Test1Request
            {
                public int A;
                public int B;
            }

            public struct Test1Response
            {
                public int Result;
            }

            [ServiceFunc]
            public Test1Response Test1Call(Test1Request request)
            {
                return new Test1Response
                {
                    Result = request.A + request.B,
                };
            }

            public string TestFunc(string arg1, string arg2)
            {
                return arg1 + arg2;
            }
            public string TestFunc2(string arg1, int arg2)
            {
                return string.Format("{0}{1}", arg1, arg2);
            }
        }

        [Test]
        public void TestServerCreate()
        {
            using (var server = new RpcServer(new TestActorService()))
            {
            }
            Assert.Pass();
        }

        [Test()]
        public async void TestCallRpc()
        {
            await CoTestCallRpc();

            Assert.Pass();
        }

        public async Task CoTestCallRpc()
        {
            using (var server = new RpcServer(new TestActorService()))
            {
                Assert.AreEqual(server.ResponsePort.GetType(), typeof(int));
                Assert.GreaterOrEqual(server.ResponsePort, 0);
                Assert.AreEqual(server.Host, "*");

                //using (var server2 = new RpcServer(new TestActorService(), "127.0.0.1"))
                //    Assert.AreEqual(server2.Host, "127.0.0.1");

                using (var client = new RpcClient("127.0.0.1", server.ResponsePort))
                {
                    var result = await client.CallAsync<TestActorService.Test1Request, TestActorService.Test1Response>(new TestActorService.Test1Request
                    {
                        A = 123,
                        B = 321,
                    }); // "TestFunc", "ABC", "DEFG"

                    Assert.AreEqual(result.Result, 444);

                    // continue client 1
                    //var result2 = client.CallResultAsync<string>("TestFunc2", "ABC", 123);
                    //Assert.AreEqual(result2, "ABC123");

                    //var result3 = client.CallAsync<string>("TestFunc3");
                    ////Assert.AreEqual(result3.IsCanceled, false);
                    ////Assert.AreEqual(result3.IsFaulted, false);
                    //Assert.AreEqual(result3, null);


                    // client 2
                    //using (var client2 = new RpcClient("127.0.0.1", server.ResponsePort))
                    //{
                    //    var resultC2 = client2.Call<string>("TestFunc", "ABC", "DEFG");
                    //    resultC2.Wait();
                    //    Assert.AreEqual(resultC2.Result, "ABCDEFG");
                    //}


                    Console.WriteLine("Async Rpc Server Done!");

                }
            }
        }
    }
}

