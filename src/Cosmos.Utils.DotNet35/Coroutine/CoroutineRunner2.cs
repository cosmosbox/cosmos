using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Cosmos.Utils
{
    internal class CoroutineRunner2
    {
        /// <summary>
        /// 25 frame every seconds
        /// 1000ms / 25ms = 40
        /// </summary>
        private static int HeartbeatMilliseconds = 1;

        private static CoroutineRunner2 Instance = new CoroutineRunner2();
        private static LinkedList<Coroutine2> _coroutines = new LinkedList<Coroutine2>();
        private Thread _coroutineRunnerThread;
        private CoroutineRunner2()
        {
            DoLoopTaskAsync();
        }

        void DoLoopTaskAsync()
        {
            _coroutineRunnerThread = new Thread(() =>
            {
                while (true)
                {
                    lock (_coroutines)
                    {
                        var node = _coroutines.First;
                        if (node != null)
                        {
                            do
                            {
                                var co = node.Value;
                                if (!co.MoveNext())
                                {
                                    // 恢复父协程
                                    if (co.ParentCoroutine != null)
                                    {
                                        _coroutines.AddAfter(node, co.ParentCoroutine);
                                    }

                                    // 删除当前完成协程
                                    var lastNode = node;
                                    node = node.Next;  // 紧接着继续父协程
                                    _coroutines.Remove(lastNode);
                                }
                                else
                                {
                                    
                                    if (co.OnYield != null)
                                        co.OnYield(co.Current);
                                    if (co.Current is Coroutine2)
                                    {
                                        // 如果Current是一个Coroutine，当前Coroutine离队，作为它的NextCoroutine...
                                        // 父协程存起来, 移除父协程
                                        // 这里没必要重新AddAfter 子协程，因为子协程的启动(Coroutine.Start)已经自动加入到队列了
                                        var subCoroutine = co.Current as Coroutine2;
                                        subCoroutine.ParentCoroutine = co; 

                                        var lastNode = node;
                                        _coroutines.Remove(lastNode);

                                    }
                                    node = node.Next;
                                }

                            }
                            while (node != null);
                        }

                    }

                    Thread.Sleep(HeartbeatMilliseconds);
                }
            });
            _coroutineRunnerThread.Start();
        }

        /// <summary>
        /// Setup every yield interval time
        /// For examples,
        /// 25 times in 1s = 1000ms / 25 = 40ms (Default)
        /// </summary>
        /// <param name="ms"></param>
        public static void SetupHeartbeatMilliseconds(int ms)
        {
            HeartbeatMilliseconds = ms;
        }
        public static Coroutine2<TResult> Start<TResult, TParam>(Coroutine2.CoroutineDelegate<TResult, TParam> coroutine, TParam param)
        {
            var coResult = new CoroutineResult<TResult>();
            var enumtor = coroutine(coResult, param);
            var co = new Coroutine2<TResult>(enumtor, coResult);

            lock (_coroutines)
            {
                _coroutines.AddLast(co);
            }
            return co;
        }
        public static Coroutine2 Start(IEnumerator enumtor)
        {
            var co = new Coroutine2(enumtor, null);

            lock (_coroutines)
            {
                _coroutines.AddLast(co);
            }
            return co;
        }
    }

}
