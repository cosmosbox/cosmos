
using System;
using System.Threading.Tasks;
using Cosmos.Framework.Components;

namespace ExampleProjectLib
{
    public interface IGameHandler : IHandler
    {
        bool EnterLevel(string sessionToken, int levelTypeId);
        bool FinishLevel(string sessionToken, int levelTypeId, bool isSuccess);
        void Handshake();
    }

    /// <summary>
    /// 游戏逻辑
    /// </summary>
	public class GameHandler : IGameHandler
    {
        private HandlerServer _handlerServer;
        public GameHandler (HandlerServer handlerServer)
        {
            _handlerServer = handlerServer;
        }

        public PlayerSession GetSession(string sessionToken)
        {
            return null;
        }

        /// <summary>
        /// 进入关卡...
        /// </summary>
        /// <param name="sessionToken"></param>
        /// <param name="levelTypeId"></param>
        public bool EnterLevel(string sessionToken, int levelTypeId)
		{
		    return GetSession(sessionToken).Player.EnterLevel(levelTypeId);
		}

        /// <summary>
        /// 完成关卡...
        /// </summary>
        /// <param name="sessionToken"></param>
        /// <param name="levelTypeId"></param>
        /// <param name="isSuccess"></param>
        public bool FinishLevel(string sessionToken, int levelTypeId, bool isSuccess)
        {
            return GetSession(sessionToken).Player.FinishLevel(levelTypeId, isSuccess);
        }

        /// <summary>
        /// 请求新的SessionToken
        /// </summary>
        public void Handshake()
        {
            //无需返回，自动生成 
            
        }
    }
}

