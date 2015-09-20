using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Rpc;
using Cosmos.Utils;
using NLog;

namespace Cosmos.Actor
{
    public interface IActorService : IRpcService
    {

    }

    public abstract partial class Actor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ActorNodeConfig Conf { get; private set; }

        public List<ActorNodeConfig> FriendActors = new List<ActorNodeConfig>(); 
        public Dictionary<string, RpcClient> RpcClients = new Dictionary<string, RpcClient>();  // every actor know each other

        // TODO:
        public Dictionary<Type, List<RpcClient>> RpcClientsOfTypes = new Dictionary<Type, List<RpcClient>>();

        public delegate ActorNodeConfig ActorClassFilterRouteFunc(IList<ActorNodeConfig> classActors);

        private Dictionary<Type, ActorClassFilterRouteFunc> FilterRoutes = new Dictionary<Type, ActorClassFilterRouteFunc>();

        public IRpcService RpcService;
        public RpcServer RpcServer;
        private Discovery _discovery;

        
        public bool IsActive { get; set; }

        public virtual void Init(ActorNodeConfig conf)
        {
            IsActive = true;
            Conf = conf;

            RpcService = NewRpcCaller();
            RpcServer = new RpcServer(RpcService, "*", conf.RpcPort);
            _discovery = new Discovery(Conf.AppToken, Conf.DiscoveryMode, Conf.DiscoveryParam);
        }

        #region Router Call RPC

        // TODO:CallByClass
        public async Task<TRes> CallByClass<TActor, TReq, TRes>(TReq request) where TActor : Actor
        {
            ActorClassFilterRouteFunc routeFunc;
            if (!FilterRoutes.TryGetValue(typeof(TActor), out routeFunc))
            {
                Logger.Error("Not yet set the Route Rule of actor type: {0}", typeof(TActor));
                return default(TRes);
            }

            var actorConfig = routeFunc(FriendActors);
            if (actorConfig == null)
            {
                Logger.Error("Router get actor config is Null of actor type: {0}", typeof(TActor));
                return default(TRes);
            }
            var callResult = await Call<TReq, TRes>(actorConfig.Name, request);

            return callResult;
        }

        public ActorNodeConfig SetRouteRule<T>(ActorClassFilterRouteFunc route) where T : Actor
        {
            var t = typeof (T);
            if (FilterRoutes.ContainsKey(t))
            {
                Logger.Warn("Override a Route Rule of Actor Type: {0}", t);
            }

            FilterRoutes[t] = route;
            return null;
        }
        #endregion

        public async Task<TRes> Call<TReq, TRes>(string actorName, TReq request)
        {
            RpcClient client;
            if (RpcClients.TryGetValue(actorName, out client))
            {
                var resultCo = client.CallResultAsync<TReq, TRes>(request);

                var result = await resultCo;

                return result;
            }
            else
            {
                Logger.Error("Not Found Actor By Name: '{0}'", actorName);

                return default(TRes);
            }

        }
        abstract public IActorService NewRpcCaller();
    }
}
