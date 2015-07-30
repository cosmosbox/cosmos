using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Rpc;
using NLog;

namespace Cosmos.Actor
{
    public abstract class Actor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ActorConf Conf { get; private set; }
        public Dictionary<string, RpcClient> RpcClients = new Dictionary<string, RpcClient>();
        public Dictionary<Type, List<RpcClient>> RpcClientsOfTypes = new Dictionary<Type, List<RpcClient>>();
        public RpcServer RpcServer;
        private Discovery _discovery;

        public bool IsActive { get; set; }

        internal void Init(ActorConf conf)
        {
            IsActive = true;
            Conf = conf;
            RpcServer = new RpcServer(NewRpcCaller());
            _discovery = new Discovery(Conf.AppToken, Conf.DiscoverServers);
        }

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
        abstract public RpcCaller NewRpcCaller();
    }
}
