using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Actor;

namespace Cosmos.Framework
{
	public struct AppConfigInfo
	{
		public string AppName;
		public string DirectorClass; // default: null, means "AppDirector"
		public ActorNodeConfig[] Actors;
	}



    /// <summary>
    /// A Module To Load confg
    /// </summary>
    class ProjectConfModule
    {
        public static ProjectConfModule Instance = new ProjectConfModule();

        ProjectJsonConfLoader ConfLoader = new ProjectJsonConfLoader();
    }

    public class ProjectJsonConfLoader
    {
        
    }
}
