using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy.Hosting.Self;

namespace ExampleProject
{

    //public class HttpServer : Nancy.NancyModule
    //{
    //    public HttpServer()
    //    {
    //        Get["/hello"] = _ => "Hello World!";
    //        Get["/hello2"] = _ => "Hello World!2";
    //    }
    //}

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
            using (var host = new NancyHost(conf, new Uri("http://localhost:10808")))
            {
                host.Start();
                Console.ReadLine();
            }
        }
    }
}
