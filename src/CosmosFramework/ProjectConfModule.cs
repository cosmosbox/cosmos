
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

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

    public struct AppConfig
    {
        public string AppToken;
    //    public ActorNodeConfig[] Actors { get; internal set; }
    }
    public class ProjectJsonConfLoader
    {
        public AppConfig GetAppConfig(string configPath = "config/app.json")
        {
            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException("Not found json discovery file", configPath);
            }
            var text = File.ReadAllText(configPath);
            return JsonConvert.DeserializeObject<AppConfig>(text);
        }
    }
}
