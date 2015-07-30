using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Framework
{
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
