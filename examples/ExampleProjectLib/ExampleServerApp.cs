using System;
using System.Threading.Tasks;
using Cosmos.Framework;
using ExampleProjectLib;

namespace ExampleProject
{
    public class ExampleServerApp : AppDirector
    {
        public static ExampleServerApp Instance = new ExampleServerApp();  // µ¥Àý

        private ExampleServerApp()
        {
            var clientScript = new ExampleClientScript();
        }
    }
}

