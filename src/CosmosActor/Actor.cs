using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Rpc;
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
            RpcServer = new RpcServer(RpcService);
            _discovery = new Discovery(Conf.AppToken, Conf.DiscoveryMode, Conf.DiscoveryParam);
        }

        #region Router Call RPC

        public async Task<TReturn> CallByClass<TActor, TReturn>(string funcName, params object[] arguments) where TActor : Actor
        {
            ActorClassFilterRouteFunc routeFunc;
            if (!FilterRoutes.TryGetValue(typeof (TActor), out routeFunc))
            {
                Logger.Error("Not yet set the Route Rule of actor type: {0}", typeof(TActor));
                return default(TReturn);
            }

            var actorConfig = routeFunc(FriendActors);
            if (actorConfig == null)
            {
                Logger.Error("Router get actor config is Null of actor type: {0}", typeof(TActor));
                return default(TReturn);
            }
            return await Call<TReturn>(actorConfig.Name, funcName, arguments);
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

        public async Task<T> Call<T>(string actorName, string funcName, params object[] arguments)
        {
            RpcClient client;
            if (RpcClients.TryGetValue(actorName, out client))
            {
                var result = await client.CallResult<T>("Add", 1, 2);
                if (result.IsError)
                {
                    Logger.Error("RPC Call Error: {0}", result.ErrorMessage);
                }

                return result.Value;
            }
            else
            {
                Logger.Error("Not Found Actor By Name: '{0}'", actorName);
            }

            return default(T);
        }
        abstract public IActorService NewRpcCaller();
    }
}
