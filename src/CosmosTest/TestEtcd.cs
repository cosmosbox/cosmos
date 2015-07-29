
using NUnit.Framework;
using System;
using Cosmos.Rpc;
using System.Threading.Tasks;
using System.IO;
using MsgPack.Serialization;
using NLog;
using etcetera;

namespace Cosmos.Test
{
    [TestFixture()]
    public class TestEtcd
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private EtcdClient etcd;

        [SetUp]
        public void Init()
        {
            etcd = new EtcdClient(new Uri("http://localhost:4001/v2/keys/"));
            
        }

        //[TearDown]
        //void UnInit()
        //{
        //    etcd = null;
        //}

        /// <summary>
        /// 务必Etcd在执行中
        /// </summary>
        /// <returns></returns>
        bool CheckEtcdRunning()
        {
            try
            {
                var leader = etcd.Statistics.Leader();
                return leader != null;
            }
            catch (Exception)
            {
                Logger.Warn("Etcd is not running !! Pass the test!!");
                return false;
            }
        }

        [Test]
        public void TestEtcdSimpleSetAndAdd()
        {
            if (!CheckEtcdRunning())
            {
                Assert.Ignore();
                return;
            }
            var respSet = etcd.Set("csharp-etcetera-test", "123");
            Assert.AreEqual(respSet.Node.Value, "123");
            var respGet = etcd.Get("csharp-etcetera-test");
            Assert.AreEqual(respGet.Node.Value, "123");
        }
        [Test]

        public void TestEtcdDirSetAndAdd()
        {
            if (!CheckEtcdRunning())
            {
                Assert.Ignore();
                return;
            }
            var respSet = etcd.Set("csharp-etcetera-test", "123");
            Assert.AreEqual(respSet.Node.Value, "123");
            var respGet = etcd.Get("csharp-etcetera-test");
            Assert.AreEqual(respGet.Node.Value, "123");
        }
    }
}

