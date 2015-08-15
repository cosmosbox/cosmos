using System;
using System.Collections;

#if DOTNET45
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
#endif

namespace Cosmos.Utils
{
    /// <summary>
    /// TODO: yield return 一个协程，本协程挂起等待
    /// </summary>
    public abstract class Coroutine
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
        internal abstract bool MoveNext();
        public abstract object Current { get; }

        internal Coroutine()
        {
        }

        public static Coroutine Start(IEnumerator em)
        {
            return CoroutineRunner.Start(em);
        }

#if DOTNET45  // async/await support,      await coroutine
        
        public bool IsCompleted { get; private set; }

        public Coroutine GetAwaiter()
        {
            Task.Run(() =>
            {
                while (!IsFinished)
                {
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
    class EnumtorCoroutine : Coroutine
    {
        private IEnumerator Enumtor;
        internal EnumtorCoroutine(IEnumerator enumtor)
        {
            Enumtor = enumtor;
        }

        internal override bool MoveNext()
        {
            var r = Enumtor.MoveNext();
            if (!r)
                IsFinished = true;

            return r;
        }

        public override object Current
        {
            get
            {
                return Enumtor.Current;
            }
        }
    }
}
