
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Actor;
using Newtonsoft.Json;

namespace Cosmos.Framework
{
    public class AppConfig
    {
        public string AppToken;
    //    public ActorNodeConfig[] Actors { get; internal set; }
        public string DiscoveryMode = "Json";
        public object DiscoveryParam;
    }
    public class ProjectJsonConfLoader
    {
        public AppConfig TheAppConfig;
        public IList<ActorNodeConfig> TheActorConfigs;
        public ProjectJsonConfLoader()
        {
            TheAppConfig = LoadAppConfig();
            TheActorConfigs = LoadActorsConfig(TheAppConfig);
        }

        public static AppConfig LoadAppConfig(string configPath = "config/app.json")
        {
            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException("Not found app config app.json file", configPath);
            }
            var text = File.ReadAllText(configPath);
            return JsonConvert.DeserializeObject<AppConfig>(text);
        }

        public static IList<ActorNodeConfig> LoadActorsConfig(AppConfig appConfig, string configPath = "config/actors.json")
        {
            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException("Not found app actors config actors.json file", configPath);
            }
            var text = File.ReadAllText(configPath);
            var configs = JsonConvert.DeserializeObject<ActorNodeConfig[]>(text);

            foreach (var config in configs)
            {
                if (!string.IsNullOrEmpty(appConfig.DiscoveryMode))
                    config.DiscoveryMode = appConfig.DiscoveryMode;

                if (appConfig.DiscoveryParam != null)
                    config.DiscoveryParam = appConfig.DiscoveryParam;
            }
            return configs;
        } 
    }
}
