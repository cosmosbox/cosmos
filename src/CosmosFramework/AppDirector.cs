using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Actor;

namespace Cosmos.Framework
{
    public abstract class AppDirector
    {
        private ProjectJsonConfLoader _projectConf;

        protected AppDirector()
        {
            // load configs
            _projectConf = new ProjectJsonConfLoader();
        }

		/// <summary>
		/// Create all actor on actors.json
		/// 
		/// 1. load configs
		/// 2. start all actor in configs
		/// </summary>
        public virtual void StartAll()
        {
            
        }

        public virtual void StartActor(string actorName)
        {
            foreach (var actorConfig in _projectConf.TheActorConfigs)
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
