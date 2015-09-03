using System;
using Cosmos;

namespace ExampleProject
{
	class MainClass
	{
		public static void Main (string[] args)
		{
		    ExampleServerApp.Instance.StartAll();

            var task = ExampleServerApp.Instance.Wait();
		    task.Wait();
		}
	}
}
