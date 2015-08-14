using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cosmos.Actor;
using Cosmos.Framework.Components;
using Cosmos.Rpc;

namespace ExampleProjectLib
{
    class GameActorService : IActorService
    {
		GameActor _actor;
        public GameActorService(GameActor actor)
		{
			_actor = actor;
		}

		public object GetUserSession(string uid)
		{
			return null;
		}

		public int GetOnlineUsersCount()
		{
			return 0;
		}
    }

	abstract class GameBehaviour
	{
		public abstract void Update();
	}

	// as a Facade
    class GameActor : FrontendActor
    {
		public GameActor()
		{
			MainLoop();
		}

		async void MainLoop()
		{
			await Task.Run (()=> {
				while(true)
				{
					Thread.Sleep(40);  // 1 / 25
				}
			});

            Console.WriteLine("Finish main loop!");
        }

        public override IActorService NewRpcCaller()
        {
            return new GameActorService(this);
        }

        public override IHandler GetHandler()
        {
            return new GameHandler(this._handlerServer);
        }
    }
}
