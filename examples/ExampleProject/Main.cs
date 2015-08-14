using System;
using Cosmos;

namespace ExampleProject
{
	class MainClass
	{
		public static void Main (string[] args)
		{
		    AppBootstrap.StartAll(ExampleServerApp.Instance);
		}
	}
}
