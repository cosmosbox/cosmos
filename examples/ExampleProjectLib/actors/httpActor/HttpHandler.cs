using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy.Hosting.Self;
namespace ExampleProject
{
    public class HttpHandler : Nancy.NancyModule
    {
        //private HttpServer _sampleServ;
        public HttpHandler()
        {
            Get["/"] = parameters => "Nancy Http Server Start!";
            Get["/hello"] = _ => "Hello World!";
            Get["/hello2"] = _ => "Hello World!2";
        }

        public void Start()
        {
            var conf = new HostConfiguration();
            conf.RewriteLocalhost = false;
            var uri = "http://localhost:10808";
            using (var host = new NancyHost(conf, new Uri(uri)))
            {
                Console.WriteLine("Now start server: {0}", uri);
                host.Start();
                Console.ReadLine();
            }
            //var url = "http://+:8080";

            //using (WebApp.Start<Startup>(url))
            //{
            //    Console.WriteLine("Running on {0}", url);
            //    Console.WriteLine("Press enter to exit");
            //    Console.ReadLine();
            //}
        }
    }
}
