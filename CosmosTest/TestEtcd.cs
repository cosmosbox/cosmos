
using NUnit.Framework;
using System;
using Cosmos.Rpc;
using System.Threading.Tasks;
using System.IO;
using MsgPack.Serialization;
using NLog;

namespace Cosmos.Test
{
    [TestFixture()]
    public class TestEtcd
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// 务必Etcd在执行中
        /// </summary>
        /// <returns></returns>
        bool CheckEtcdRunning()
        {
            return false;
        }

        [Test()]
        public void TestEtcdAdd()
        {
            Logger.Info("Check Etcd status...");
            int a = 1;
            int b = 2;
            int sum = a + b;
            Assert.AreEqual(sum, 3);
        }

    }
}

