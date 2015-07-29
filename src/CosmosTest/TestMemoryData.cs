using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using StackExchange.Redis;
using Cosmos;
namespace CosmosTest
{
    [TestFixture]
    class TestMemoryData
    {

        MemoryData MemData = new MemoryData();
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
        [Test]
        public async void SimpleMemData()
        {
            var taskA = Task.Run(() =>
            {
                for (var i = 0; i < 100; i++)
                    DoTask("FromTaskA" + i);
            });

            var taskB = Task.Run(() =>
            {
                for (var i = 0; i < 100; i++)
                    DoTask("FromTaskB" + i);
            });

            taskA.Wait();
            taskB.Wait();

            //Assert.Pass();
        }

        async void DoTask(string val)
        {
            var set = await MemData.Set("TestMemData", val);
            Assert.AreEqual(set, true);
            var get = await MemData.Get("TestMemData");
            Assert.AreEqual(get, val);
        }

    }
}
