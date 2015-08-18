


using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Cosmos.Utils
{
    public class Coroutine2<T> : Coroutine2
    {
        internal Coroutine2(IEnumerator enumtor, CoroutineResult resulter) : base(enumtor, resulter)
        {
        }

        public T Result
        {
            get { return (T) InnerResult; }
        }
    }

    public class Coroutine2
    {
        internal Coroutine2 ParentCoroutine;
        internal IEnumerator Enumtor;
        protected Coroutine2(IEnumerator enumtor, CoroutineResult resulter)
        {
            Enumtor = enumtor;
            Resulter = resulter;
        }

        private CoroutineResult Resulter;
         
        public Action<object> OnYield;

        public bool IsFinished { get; private set; }

        public delegate IEnumerator CoroutineDelegate<T>(CoroutineResult<T> result, object param);

        internal bool MoveNext()
        {
            var r = Enumtor.MoveNext();
            if (!r)
                IsFinished = true;

            return r;
        }

        protected object InnerResult
        {
            get
            {
                if (IsFinished)
                    return Resulter.InnerResult;
                return default(object);
            }
        }
        public object Current
        {
            get
            {
                return Enumtor.Current;
            }
        }
        public static Coroutine2<T> Start<T>(CoroutineDelegate<T> coroutineFunc, params object[] args)
        {
            return CoroutineRunner2.StartCo<T>(coroutineFunc, args);
        }

    }

    public class CoroutineResult
    {
        public CoroutineState State;
        internal protected object InnerResult;
    }
    public class CoroutineResult<T> : CoroutineResult
    {
        public T Result
        {
            get { return (T) InnerResult; }
            set { InnerResult = value; }
        }

    }

}