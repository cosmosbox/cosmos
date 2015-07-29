using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Tool
{
    /// <summary>
    /// TODO: yield return 一个协程，本协程挂起等待
    /// </summary>
    public abstract class Coroutine
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
