using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Framework;

namespace Cosmos
{
    public class AppBootstrap
    {
        /// <summary>
        /// StartAll the App
        /// </summary>
        /// <param name="director">if null, auto find</param>
        public static void StartAll(AppDirector director = null)
        {
            director.StartAll();
        }

        public static void StartActor(AppDirector director, string actorName)
        {
            director.StartActor(actorName);
        }
    }
}
