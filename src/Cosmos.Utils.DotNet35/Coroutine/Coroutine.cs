using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
#if DOTNET45
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
#endif

namespace Cosmos.Utils
{
    /// <summary>
    /// TODO: yield return 一个协程，本协程挂起等待
    /// </summary>
    public class Coroutine<T>
#if DOTNET45
        // Make Coroutine can be with async/await 
        //
        // ```
        // await co;
        // ```
        : INotifyCompletion
#endif

    {
        
        public Action<object> OnYield;

        public bool IsFinished { get; internal set; }

        //internal abstract bool MoveNext();
        //public abstract object Current { get; }

        internal Coroutine()
        {
        }
        private IEnumerator<T> Enumtor;
        internal Coroutine(IEnumerator<T> enumtor)
        {
            Enumtor = enumtor;
        }

        internal bool MoveNext()
        {
            var r = Enumtor.MoveNext();
            if (!r)
                IsFinished = true;
            else
            {
                LastReturn = Current;
            }
            return r;
        }

        private T LastReturn;

        public T Result
        {
            get
            {
                if (IsFinished)
                    return LastReturn;
                return default(T);
            }
        }
        public T Current
        {
            get
            {
                return Enumtor.Current;
            }
        }

        public static Coroutine<T> Start(IEnumerator<T> em)
        {
            return CoroutineRunner<T>.Start(em);
        }

#if DOTNET45 // async/await support,      await coroutine
        
        public bool IsCompleted { get; private set; }

        public Coroutine<T> GetAwaiter()
        {
            Task.Run(() =>
            {
                while (!IsFinished)
                {
                    Thread.Sleep(1);
                }
                IsCompleted = true;
            });
            return this;
        }
        // TResult can also be void
        public void OnCompleted(Action continuation)
        {
            continuation();
        }

        public void GetResult() { }
#endif
    }

    /// <summary>
    /// 枚举
    /// </summary>
    //class EnumtorCoroutine : Coroutine
    //{

    //}

}
