using System;
using System.Threading.Tasks;
using Cosmos.Framework;

namespace ExampleProject
{
    public class ExampleServerApp : AppDirector
    {
        public static ExampleServerApp Instance = new ExampleServerApp();  // ����

        private ExampleServerApp()
        {
        }
    }
}

