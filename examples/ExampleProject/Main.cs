using System;
using Cosmos;

namespace ExampleProject
{
	class MainClass
	{
		public static void Main (string[] args)
		{
		    AppBootstrap.Start(new ExampleServerApp());
		}
	}
}
