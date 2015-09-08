using System;
using System.Threading;
using ExampleProjectLib;

namespace ExampleClient
{
	class MainClass
	{
		public static void Main (string[] args)
		{

            var clientScript = new ExampleClientScript();
		    while (true)
		    {
		        Thread.Sleep(1000);
		    }
        }
	}
}
