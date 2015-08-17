using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cosmos.Framework.Components;
using Cosmos.Utils;

namespace ExampleProjectLib
{
    public class PlayerHandlerClient : IGameHandler
    {
        private HandlerClient _handlerClient;
        public PlayerHandlerClient(string host, int port, int subcribePort)
        {
            _handlerClient = new HandlerClient(host, port, subcribePort);
            SubcribePlayer();
        }

        public string SessionToken
        {
            get { return _handlerClient.SessionToken; }
        }

        private void SubcribePlayer()
        {
            _handlerClient.Subcribe("player-xxxxx");
        }

        public bool EnterLevel(string sessionToken, int levelTypeId)
        {
            var task = Coroutine<bool>.Start(_handlerClient.Call<bool>("EnterLevel", sessionToken, levelTypeId));
            while (!task.IsFinished)
            {
                Thread.Sleep(1);
            }
            return task.Result;
        }

        public bool FinishLevel(string sessionToken, int levelTypeId, bool isSuccess)
        {
            var task = Coroutine<bool>.Start(_handlerClient.Call<bool>("FinishLevel", sessionToken, levelTypeId, isSuccess));
            while (!task.IsFinished)
            {
                Thread.Sleep(1);
            }
            return task.Result;
        }

        /// <summary>
        /// 随机请求一次，获取SessionToken
        /// </summary>
        /// <returns></returns>
        public void Handshake()
        {
            if (string.IsNullOrEmpty(SessionToken))
            {
                var task = Coroutine<object>.Start(_handlerClient.Call<object>("Handshake"));
                while (!task.IsFinished)
                {
                    Thread.Sleep(1);
                }
            }
        }
    }
}
