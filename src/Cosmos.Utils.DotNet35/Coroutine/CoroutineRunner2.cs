using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Cosmos.Utils
{
    public enum CoroutineState
    {
        Error,
        Timeout,
        Finish
    }

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
                                    co.IsFinished = true;
                                    var lastNode = node;
                                    node = node.Next;
                                    _coroutines.Remove(lastNode);
                                }
                                else
                                {
                                    // TODO: 如果Current是一个Coroutine，塞进队列，当前Coroutine离队，作为它的NextCoroutine...
                                    if (co.OnYield != null)
                                        co.OnYield(co.Current);

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

        public static Coroutine2<T> StartCo<T>(Coroutine2.CoroutineDelegate<T> coroutine, object[] args)
        {
            var coResult = new CoroutineResult<T>();
            var enumtor = coroutine(coResult, args);
            var co = new Coroutine2<T>(enumtor, coResult);

            lock (_coroutines)
            {
                _coroutines.AddLast(co);
            }
            return co;
        }
    }

}
