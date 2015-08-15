using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cosmos.Utils;
namespace Cosmos.Actor
{
    /// <summary>
    /// Actor In Task State Share
    /// </summary>
    public enum ActorRunState
    {
        None,
        Running,
    }

    /// <summary>
    /// Control a actor's running in process
    /// 
    /// Actor的启动器，Actor线程管理，可通过该类动态创建Actor
    /// </summary>
    public class ActorRunner : IDisposable
    {
        public static Dictionary<string, ActorRunner> Runners = new Dictionary<string, ActorRunner>();

        public DateTime StartTime = DateTime.UtcNow;

        public int SecondsTick = 0;
        private ActorRunner()
        {
        }

        public ActorRunState State = ActorRunState.None;
        private Task ActorThread;
        private ActorNodeConfig Conf;
        public Actor Actor;

        public string ActorName
        {
            get { return Conf.Name; }
        }

        private ActorRunner(ActorNodeConfig conf)
        {
            Conf = conf;
            ActorThread = Task.Run(() =>
            {
                Actor = ActorFactory.Create(conf);
                State = ActorRunState.Running;
                while (Actor.IsActive)
                {
                    Thread.Sleep(1000);
                    SecondsTick++;
                }
            });

            //while (State == ActorRunState.None)
            //{
            //    // block
            //}

            Runners[conf.Name] = this;
        }



        public static ActorRunner Run(ActorNodeConfig conf)
        {
            ActorRunner runner = new ActorRunner(conf);
            return runner;
        }

        public static ActorRunner GetActorStateByName(string actorName)
        {
            return Runners[actorName];
        }

        public void Dispose()
        {
            Runners.Remove(ActorName);
            ActorThread.Dispose(); // kill the actor
        }
    }
}
