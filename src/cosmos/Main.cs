using System;
using System.Threading.Tasks;
using System.Threading;
//using NetMQ;

namespace Cosmos
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            TestAsync();
            HelloWorldAsync();

            //using (var context = NetMQContext.Create())
            //	using (var server = context.CreateResponseSocket())
            //		using (var client = context.CreateRequestSocket())
            //{
            //	// Bind the server to a local TCP address
            //	server.Bind("tcp://localhost:5556");

            //	// Connect the client to the server
            //	client.Connect("tcp://localhost:5556");

            //	// Send a message from the client socket
            //	client.Send("Hello");

            //	// Receive the message from the server socket
            //	string m1 = server.ReceiveString();
            //	Console.WriteLine("From Client: {0}", m1);

            //	// Send a response back from the server
            //	server.Send("Hi Back");

            //	// Receive the response from the client socket
            //	string m2 = client.ReceiveString();
            //	Console.WriteLine("From Server: {0}", m2);
            //}

            var mainTask = BlockingTask();
            mainTask.Wait();
        }

        private static async Task BlockingTask()
        {
            while (true)
            {
                await Task.Delay(40);
            }
        }

        async static void HelloWorldAsync()
        {
            while (true)
            {
                await Task.Delay(2000);
                Console.WriteLine("Hello World!");
            }
        }

        async static void TestAsync()
        {
            await testCo2();
            Console.WriteLine("Finished!");
        }
        static async Task testCo2()
        {
            for (var i = 0; i < 100; i++)
            {
                Console.WriteLine(string.Format("i is {0}", i));
                await Task.Delay(1000);
            }

        }
    }
}
