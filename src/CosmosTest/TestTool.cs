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
        [Test()]
        public async void TestCoroutineGeneric()
        {
            var co = Coroutine2.Start<int>(CoTestCoroutineGeneric);
            var i = 0;
            co.OnYield += (obj) =>
            {
                var ret = (int)obj;
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
                Assert.AreEqual(100, co.Result);
            });
        }

        IEnumerator CoTestCoroutineGeneric(CoroutineResult<int> result, object arg)
        {
            // 25 tick
            for (var i = 0; i < 25; i++)
            {
                yield return i;
            }
            var co2 = Coroutine2.Start<string>(Co2);
            yield return co2;
            Assert.AreEqual("TestString", co2.Result2);

            result.Result = 100;
        }

        IEnumerator Co2(CoroutineResult<string> result, object arg)
        {
            for (var i = 0; i < 100; i++)
            {
                yield return i;
            }
            result.Result = "TestString";
        }

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
