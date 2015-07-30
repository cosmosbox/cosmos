using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using StackExchange.Redis;
using Cosmos;
using MsgPack.Serialization;
using Redlock.CSharp;

namespace CosmosTest
{
    [TestFixture]
    class TestMemoryData
    {

        MemoryDataModule _memDataModule = new MemoryDataModule();
        [Test]
        public async void SimpleGetSetRedis()
        {
            using (ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("127.0.0.1"))
            {
                IDatabase db = redis.GetDatabase();
                var isOk = await db.StringSetAsync("TestStr", "ValueOfTest");
                Assert.AreEqual(isOk, true);
                var rValue = await db.StringGetAsync("TestStr");
                Assert.AreEqual(rValue.ToString(), "ValueOfTest");
                // ^^^ store and re-use this!!!
            }
        }

        const int AsyncCount = 1000;

        public Dictionary<string, string> Mems = new Dictionary<string, string>();

        [Test]
        public void MemoryAsync100()
        {
            var taskA = Task.Run(() =>
            {
                for (var i = 0; i < AsyncCount; i++)
                    DoMem("FromTaskAA" + i);
            });

            var taskB = Task.Run(() =>
            {
                for (var i = 0; i < AsyncCount; i++)
                    DoMem("FromTaskBB" + i);
            });

            taskA.Wait();
            taskB.Wait();

            //Assert.Pass();
        }
        void DoMem(string val)
        {
            lock (Mems)
            {
                var set = Mems["TestMemData"] = val;
                Assert.AreEqual(set, val);
                var get = Mems["TestMemData"];
                Assert.AreEqual(get, val);
            }
        }

        [Test]
        public void RedisAsyncLock100()
        {
            var taskA = Task.Run(() =>
            {
                for (var i = 0; i < AsyncCount; i++)
                    DoMemDataAsync("FromTaskAA" + i);
            });

            var taskB = Task.Run(() =>
            {
                for (var i = 0; i < AsyncCount; i++)
                    DoMemDataAsync("FromTaskBB" + i);
            });

            taskA.Wait();
            taskB.Wait();

            //Assert.Pass();
        }

        async void DoMemDataAsync(string val)
        {
            using (var data = _memDataModule.GetData("TestMemData"))
            {
                var set = await data.SetValue(val);
                Assert.AreEqual(set, true);
                var get = await data.GetValue<string>();
                Assert.AreEqual(get, val);
            }
        }

        
        struct SampleMsg
        {
            public int AInt;
            public IList<int> BIntArr;
            public string AString;

        }
        [Test]
        public async void RedisMsgPack()
        {
            var serializer = MessagePackSerializer.Get<SampleMsg>();
            var msg = new SampleMsg()
            {
                AInt = 1002,
                AString = "AStringValue",
                BIntArr = new[] {1, 2, 3,}
            };
            var msgBytes = serializer.PackSingleObject(msg);
            using (var data = _memDataModule.GetData("TestMsgPack"))
            {

                var set = await data.SetValue(Convert.ToBase64String(msgBytes));
                Assert.AreEqual(set, true);
                var get = await data.GetValue<string>();

                var getBytes = Convert.FromBase64String(get);
                var getMsg = serializer.UnpackSingleObject(getBytes);

                Assert.AreEqual(getMsg.AInt, msg.AInt);
                Assert.AreEqual(getMsg.AString, msg.AString);
                Assert.AreEqual(getMsg.BIntArr[0], msg.BIntArr[0]);

            }
        }

        [Test]
        public async void RedisMsgPack2()
        {
            using (var data = _memDataModule.GetData("TestMsgPack2"))
            {
                var msg = new SampleMsg()
                {
                    AInt = 2000,
                    AString = "dafjolasjfljewf",
                    BIntArr = new[] { 1, 2, 3, }
                };
                var set = await data.SetValue<SampleMsg>(msg);
                Assert.AreEqual(set, true);
                var getMsg = await data.GetValue<SampleMsg>();

                Assert.AreEqual(getMsg.AInt, msg.AInt);
                Assert.AreEqual(getMsg.AString, msg.AString);
                Assert.AreEqual(getMsg.BIntArr[0], msg.BIntArr[0]);

            }
        }
    }
}
