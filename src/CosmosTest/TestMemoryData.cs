using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using StackExchange.Redis;
using Cosmos;
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


        public Dictionary<string, string> Mems = new Dictionary<string, string>();
        [Test]
        public void MemoryAsync1000()
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
        const int AsyncCount = 1000;
        [Test]
        public void RedisAsyncLock1000()
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
                var get = await data.GetValue();
                Assert.AreEqual(get, val);
            }
        }

    }
}
