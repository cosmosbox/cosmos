using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Actor;

namespace Cosmos.Test.Performance
{
    public class ActorB : Cosmos.Actor.Actor, IActorService
    {
        public override IActorService NewRpcCaller()
        {
            return this;
        }

        public override void Init(ActorNodeConfig conf)
        {
            base.Init(conf);
            Task.Factory.StartNew(async () =>
            {
                await GoRequest();
            });
        }

        public int Count;
        private int LastCount;
        private DateTime LastTime = DateTime.Now;
        protected async Task GoRequest()
        {
            while (true)
            {
                var result = await this.Call<ActorARequest, ActorAResponse>("TestActorB", new ActorARequest
                {
                    AString = "This is AString",
                    BString = " and BString",
                });

                if (result.ResultString == "This is AString and BString")
                {
                    Count++;
                    if ((DateTime.Now - LastTime).TotalSeconds > 1)
                    {
                        Console.WriteLine(string.Format("Last Seconds: {0}", Count - LastCount));
                        LastCount = Count;
                    }
                }
                else
                {

                }

            }
        }
    }
}
