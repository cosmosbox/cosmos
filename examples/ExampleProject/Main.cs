using System;
using Cosmos;

namespace ExampleProject
{
	class MainClass
	{
		public static void Main (string[] args)
		{
		    ExampleServerApp.Instance.StartAll();

            ExampleServerApp.Instance.Wait();
		}
	}
}
