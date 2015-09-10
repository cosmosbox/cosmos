using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using etcetera;
using Newtonsoft.Json;
using NLog;

namespace Cosmos.Actor
{
	/// <summary>
	/// Etcd discovery services
	/// </summary>
	public class EtcdDiscoveryMode : DiscoveryMode
	{
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private EtcdClient _etcdClient;
	    private string _etcdTopKey;

	    /// <summary>
	    /// etcd get to async
	    /// </summary>
	    /// <param name="key"></param>
	    /// <param name="recursive">是否递归获取子节点</param>
	    /// <returns></returns>
	    public Task<EtcdResponse> GetAsync(string key, bool recursive = true)
        {
	        TaskCompletionSource<EtcdResponse> tcs = new TaskCompletionSource<EtcdResponse>();
            
            Task.Factory.StartNew(()=>
            {
                tcs.SetResult(_etcdClient.Get(key, recursive));
            });


	        return tcs.Task;
	    }
        public Task<EtcdResponse> SetAsync(string key, string value)
        {
            TaskCompletionSource<EtcdResponse> tcs = new TaskCompletionSource<EtcdResponse>();
            Task.Factory.StartNew(() =>
            {
                tcs.SetResult(_etcdClient.Set(key, value));
            });


            return tcs.Task;
        }

        public EtcdDiscoveryMode (string topKey, params string[] discoveryServers)
        {
            if (topKey.StartsWith("/"))
                topKey.Substring(1, topKey.Length - 1);

            _etcdTopKey = topKey;
            foreach (var etcdUrl in discoveryServers)
            {
                var etcdClient = new EtcdClient(new Uri(string.Format("{0}/v2/keys", etcdUrl)));
                try
                {
                    etcdClient.Statistics.Leader();
                    _etcdClient = etcdClient;
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message);
                    Logger.Error("Invalid Etcd Host: {0}", etcdUrl);
                    continue;
                }
            }

            if (_etcdClient == null)
                throw new Exception("Not valid EtcdClient");
        }
        /// <summary>
        /// Get A Etcd key from a actorNode
        /// if null, return top Key, means all actorNodes
        /// </summary>
        /// <param name="actorNodeConfig"></param>
        /// <returns></returns>
	    private string GetEtcdKeyFromActorNode(ActorNodeConfig actorNodeConfig)
	    {
            if (actorNodeConfig == null)
                return _etcdTopKey;
	        return string.Format("{0}/{1}", _etcdTopKey, actorNodeConfig.Name);
	    }
        /// <summary>
        /// 注册一个Actor节点, 通常在Actor启动时注册, 并定时进行心跳式注册, 告诉其它节点"我还在生"
        /// </summary>
        /// <param name="actorNodeConfig"></param>
        /// <returns></returns>
	    public async Task<bool> RegisterActor(ActorNodeConfig actorNodeConfig)
	    {
	        if (actorNodeConfig == null) return false;
	        var etcdKey = GetEtcdKeyFromActorNode(actorNodeConfig);
	        var response = await SetAsync(etcdKey, actorNodeConfig.ToJson());

	        return response != null;
	    }

        /// <summary>
        /// 异步获取所有的Actor节点信息
        /// </summary>
        /// <returns></returns>
		public async override Task<IDictionary<string, ActorNodeConfig>> GetNodes()
		{
		    var response = await GetAsync(GetEtcdKeyFromActorNode(null));
		    var list = new Dictionary<string, ActorNodeConfig>();
		    RecurseEtcdNode(response.Node, ref list);
            return list;
		}

        /// <summary>
        /// 递归一个Etcd节点,并解析这棵Etcd数所有的ActorNode节点数据
        /// </summary>
        /// <param name="etcdNode"></param>
        /// <param name="nodes"></param>
	    private void RecurseEtcdNode(Node etcdNode, ref Dictionary<string, ActorNodeConfig> nodes)
	    {
	        if (etcdNode.Dir)
	        {
	            foreach (var subNode in etcdNode.Nodes)
	            {
                    RecurseEtcdNode(subNode, ref nodes);
                }
	        }
	        else
	        {
                var actorNodeCfg = ActorNodeConfig.FromJson(etcdNode.Value);
                nodes.Add(actorNodeCfg.Name, actorNodeCfg);
            }
	    }
	}
}

