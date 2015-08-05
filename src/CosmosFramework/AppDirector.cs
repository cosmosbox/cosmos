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
		/// <summary>
		/// Create all actor on actors.json
		/// </summary>
        public virtual void StartAll()
        {
            
        }

        public virtual void StartActor(string actorName)
        {
            
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
