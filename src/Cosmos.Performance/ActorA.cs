using Cosmos.Actor;
using Cosmos.Rpc;

namespace Cosmos.Test.Performance
{
    public class ActorARequest
    {
        public string AString;
        public string BString;
    }

    public class ActorAResponse
    {
        public string ResultString;
    }
    public class ActorA : Cosmos.Actor.Actor, IActorService
    {
        public override IActorService NewRpcCaller()
        {
            return this;
        }

        [ServiceFunc]
        public ActorAResponse RequestA(ActorARequest request)
        {
            return new ActorAResponse
            {
                ResultString = request.AString + request.BString,
            };
        }

    }

}