using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cosmos.Framework.Components;
using Cosmos.Utils;

namespace ExampleProjectLib
{
    public class PlayerHandlerClient
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

        public class EnterLevelParam
        {
            public string SessionToken;
            public int LevelTypeId;
        }

        public IEnumerator EnterLevel(CoroutineResult<bool> result, EnterLevelParam param)//string sessionToken, int levelTypeId)
        {
            var task = Coroutine<bool>.Start(_handlerClient.Call<bool>("EnterLevel", param.SessionToken, param.LevelTypeId));
            while (!task.IsFinished)
            {
                yield return null;
            }

            result.Result = task.Result;
        }

        public class FinishLevelParam
        {
            public string SessionToken;
            public int LevelTypeId;
            public bool IsSuccess;
        }
        public IEnumerator FinishLevel(CoroutineResult<bool> result, FinishLevelParam param)
        {
            var task = Coroutine<bool>.Start(_handlerClient.Call<bool>("FinishLevel", param.SessionToken, param.LevelTypeId, param.IsSuccess));
            while (!task.IsFinished)
            {
                yield return null;
            }
            result.Result = task.Result;
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
