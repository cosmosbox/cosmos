using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Framework.Components;

namespace ExampleProjectLib
{
    public class PlayerHandlerClient : IGameHandler
    {
        private HandlerClient _handlerClient;
        public PlayerHandlerClient(string host, int port)
        {
            _handlerClient = new HandlerClient(host, port);
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
            var task = _handlerClient.CallResult<bool>("EnterLevel", sessionToken, levelTypeId);
            task.Wait();
            return task.Result.Value;
        }

        public bool FinishLevel(string sessionToken, int levelTypeId, bool isSuccess)
        {
            var task = _handlerClient.CallResult<bool>("FinishLevel", sessionToken, levelTypeId, isSuccess);
            task.Wait();
            return task.Result.Value;
        }

        /// <summary>
        /// 随机请求一次，获取SessionToken
        /// </summary>
        /// <returns></returns>
        public void Handshake()
        {
            if (string.IsNullOrEmpty(SessionToken))
            {
                var task = _handlerClient.CallResult<string>("Handshake");
                task.Wait();
            }
        }
    }
}
