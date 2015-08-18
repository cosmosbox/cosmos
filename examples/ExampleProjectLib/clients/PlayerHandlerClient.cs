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

        public async Task<bool> EnterLevel(string sessionToken, int levelTypeId)//string sessionToken, int levelTypeId)
        {
            var task = Coroutine<bool>.Start(_handlerClient.Call<bool>("EnterLevel", sessionToken, levelTypeId));
            while (!task.IsFinished)
            {
                await Task.Delay(0);
            }

            return task.Result;
        }

        public class FinishLevelParam
        {
            public string SessionToken;
            public int LevelTypeId;
            public bool IsSuccess;
        }

        public async Task<bool> FinishLevel(FinishLevelParam param)
        {
            var task = Coroutine<bool>.Start(_handlerClient.Call<bool>("FinishLevel", param.SessionToken, param.LevelTypeId, param.IsSuccess));
            while (!task.IsFinished)
            {
                await Task.Delay(1);
            }
            return task.Result;
        }

        public IEnumerator FinishLevel(CoroutineResult<bool> result, object param_)
        {
            var param = (PlayerHandlerClient.FinishLevelParam)param_;
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
        public async Task Handshake()
        {
            if (string.IsNullOrEmpty(SessionToken))
            {
                var task = Coroutine<object>.Start(_handlerClient.Call<object>("Handshake"));
                while (!task.IsFinished)
                {
                    await Task.Delay(1);
                }
            }
        }
    }
}
