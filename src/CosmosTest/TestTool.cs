using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Utils;
using NLog;
using NUnit.Framework;

namespace CosmosTest
{
    [TestFixture()]
    class TestTool
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        //[Test()]
        //public async void TestCoroutineGeneric()
        //{
        //    var co = Coroutine<object>.StartCo(CoTestCoroutineGeneric);
        //    var i = 0;
        //    co.OnYield += (obj) =>
        //    {
        //        var ret = (int)obj;
        //        Assert.AreEqual(ret, i);  // 约4秒
        //        i++;
        //    };

        //    await Task.Run(() =>
        //    {
        //        // 堵塞，等待协程完成
        //        while (!co.IsFinished)
        //        {
        //            // Blocking
        //        }
        //    });
        //}

        //IEnumerator<object> CoTestCoroutineGeneric(CoroutineResult<object> result, object[] args)
        //{
        //    yield break;
        //}

        [Test()]
        public async void TestCoroutine()
        {
            var co = Coroutine<int>.Start(CoTester());
            var i = 0;
            co.OnYield += (obj) =>
            {
                var ret = (int) obj;
                Assert.AreEqual(ret, i);  // 约4秒
                i++;
            };

            await Task.Run(() =>
            {
                // 堵塞，等待协程完成
                while (!co.IsFinished)
                {
                    // Blocking
                    
                }
                Assert.AreEqual(24, co.Result);
            });
        }

        IEnumerator<int> CoTester()
        {
			// 25 tick for 1 seconds
            for (var i = 0; i < 25; i++)
            {
                yield return i;
            }
        }
    }
}
