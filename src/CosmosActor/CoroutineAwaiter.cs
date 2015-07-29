using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Tool;

namespace Cosmos.Tool
{
    public class CoroutineAwaiter : INotifyCompletion
    {
        private Task waitTask;
        public CoroutineAwaiter(Coroutine co)
        {
            waitTask = Task.Run(() =>
            {
                while (!co.IsFinished)
                {
                }
                IsCompleted = true;
            });
        }
        public bool IsCompleted { get; private set; }

        public CoroutineAwaiter GetAwaiter()
        {
            return this;
        }
        // TResult can also be void
        public void OnCompleted(Action continuation)
        {
            continuation();
        }

        public void GetResult() { }
    }
}
