using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cosmos.Actor;
using NLog;

namespace Cosmos.Framework
{
    public abstract class AppDirector
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ProjectJsonConfLoader ProjectConf { get; private set; }

        protected bool _hasStartAll = false;
        protected AppDirector()
        {
            // load configs
            ProjectConf = new ProjectJsonConfLoader();
        }

        /// <summary>
        /// Create all actor on actors.json
        /// 
        /// 1. load configs
        /// 2. start all actor in configs
        /// 
        /// TODO:  check if the actor running, not running then run it !  hot update !
        /// </summary>
        public virtual void StartAll()
		{
		    if (_hasStartAll)
		    {
                Logger.Error("AppDirector cannot `StartAll` twice!");
                return;
		    }
            
		    _hasStartAll = true;
            foreach (var actorConfig in ProjectConf.TheActorConfigs)
            {
                ActorRunner.Run(actorConfig);
            }
        }

        public async Task Wait()
        {
            while (true)
                await Task.Delay(1);
        }
        public virtual void StartActor(string actorName)
        {
            foreach (var actorConfig in ProjectConf.TheActorConfigs)
            {
                if (actorConfig.Name == actorName)
                {
                    ActorRunner.Run(actorConfig);
                    return;
                }
            }
            throw new Exception(string.Format("Not Found Actor on actors.json: {0}", actorName));
        }

		/// <summary>
		/// TODO: create a actor in thread!  not in config
		/// </summary>
		/// <returns>The thread actor.</returns>
		/// <param name="config">Config.</param>
		public virtual ActorNodeConfig NewThreadActor(ActorNodeConfig config)
		{
			return default(ActorNodeConfig);
		}

		/// <summary>
		/// TODO: create a actor in new process!
		/// </summary>
		/// <returns>The process actor.</returns>
		/// <param name="config">Config.</param>
		public virtual ActorNodeConfig NewProcessActor(ActorNodeConfig config)
		{
			return default(ActorNodeConfig);
		}
    }
}
