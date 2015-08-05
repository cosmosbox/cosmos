using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cosmos.Actor;
using Cosmos.Rpc;

namespace ExampleProjectLib
{
    class GameActorRpcer : IActorRpcer
    {
		GameActor _actor;
        public GameActorRpcer(GameActor actor)
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
    class GameActor : Actor
    {
		GameMap _map;

		public GameActor(int mapTypeId)
		{
			_map = GameMap.Get(mapTypeId);
			MainLoop();
		}

		async void MainLoop()
		{
			await Task.Run (()=> {
				while(true)
				{
					_map.Update();
					Thread.Sleep(40);  // 1 / 25
				}

				Console.WriteLine("Finish main loop!");
			});
		}

        public override IActorRpcer NewRpcCaller()
        {
            return new GameActorRpcer(this);
        }
    }
}
