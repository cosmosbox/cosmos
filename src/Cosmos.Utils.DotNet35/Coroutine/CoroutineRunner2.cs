using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
#if DOTNET45
using System.Threading.Tasks;
#endif

namespace Cosmos.Utils
{
    //internal class CoroutineContext
    //{
    //    CoroutineContext()
    //    {

    //        var id2 = Thread.CurrentThread.ManagedThreadId;
    //    }
    //}

    internal class CoroutineRunner2
    {
        /// <summary>
        /// 25 frame every seconds
        /// 1000ms / 25ms = 40
        /// </summary>
        private static int HeartbeatMilliseconds = 1;

        private static Dictionary<int, CoroutineRunner2> Pool = new Dictionary<int, CoroutineRunner2>();

        private Queue<Coroutine2> _waitQueue = new Queue<Coroutine2>();
        private LinkedList<Coroutine2> _coroutines = new LinkedList<Coroutine2>();
        private
#if DOTNET45ABC
            Task
#else
            Thread
#endif
            _coroutineRunnerThread;

        private CoroutineRunner2()
        {
            DoLoopTaskAsync();
        }

        /// <summary>
        /// 使用Task和Thread的区别：
        /// Task时，单进程性能会更高（比Thread多50%），可能由于跟其它Task拼抢切换原因，多进程时会大大影响速度
        /// Thread时，由于独霸一个线程，在多进程时性能更好
        /// </summary>
        void DoLoopTaskAsync()
        {
            _coroutineRunnerThread =
#if DOTNET45ABC
                Task.Run(() =>

#else
                new Thread(()=>
#endif
                {
                    while (true)
                    {
                        lock (_coroutines)
                        {

                            lock (_waitQueue)
                            {
                                while (_waitQueue.Count > 0)
                                {
                                    _coroutines.AddLast(_waitQueue.Dequeue());
                                }
                            }

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
#if DOTNET45ABC
                        Task.Delay(HeartbeatMilliseconds);
#else
                        Thread.Sleep(HeartbeatMilliseconds);
#endif
                    }
                });

#if !DOTNET45ABC
            _coroutineRunnerThread.Start();
#endif
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

        static CoroutineRunner2 runner;
        private static CoroutineRunner2 Get()
        {
            //CoroutineRunner2 runner;
            //var tId = Thread.CurrentThread.ManagedThreadId;
            //if (!Pool.TryGetValue(tId, out runner))
            //{
            //    runner = Pool[tId] = new CoroutineRunner2();
            //}
            if (runner == null)
                runner = new CoroutineRunner2();
            return runner;
        }
        public static Coroutine2<TResult> Start<TResult, TParam>(Coroutine2.CoroutineDelegate<TResult, TParam> coroutine, TParam param)
        {
            var coResult = new CoroutineResult<TResult>();
            var enumtor = coroutine(coResult, param);
            var co = new Coroutine2<TResult>(enumtor, coResult);
            var runner = Get();
            lock (runner._coroutines)
            {
                runner._coroutines.AddLast(co);
            }
            return co;
        }
        public static Coroutine2 Start(IEnumerator enumtor)
        {
            var co = new Coroutine2(enumtor, null);
            var runner = Get();
            lock (runner._waitQueue)
            {
                runner._waitQueue.Enqueue(co);
            }
            //lock (_coroutines)
            //{
            //    _coroutines.AddLast(co);
            //}
            return co;
        }
    }

}
