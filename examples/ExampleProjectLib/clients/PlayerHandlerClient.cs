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
        //public IEnumerator EnterLevel(CoroutineResult<bool> result, string sessionToken, int levelTypeId)//string sessionToken, int levelTypeId)
        //{
        //    var resulter = new CoroutineResult<bool>();
        //    var task = Coroutine2.Start(_handlerClient.Call<bool>(resulter, "EnterLevel", sessionToken, levelTypeId));
        //    while (!task.IsFinished)
        //    {
        //        yield return null;
        //    }

        //    result.Result = resulter.Result;
        //}

        //public async Task<bool> EnterLevel(string sessionToken, int levelTypeId)//string sessionToken, int levelTypeId)
        //{
        //    var resulter = new CoroutineResult<bool>();
        //    var task = Coroutine2.Start(_handlerClient.Call<bool>(resulter, "EnterLevel", sessionToken, levelTypeId));
        //    while (!task.IsFinished)
        //    {
        //        await Task.Delay(1);
        //    }

        //    return  resulter.Result;
        //}

        public class FinishLevelRequest
        {
            public string SessionToken;
            public int LevelTypeId;
            public bool IsSuccess;
        }

        public async Task<bool> FinishLevel(FinishLevelRequest request)//string sessionToken, int levelTypeId, bool isSuccess)
        {
            var task = await _handlerClient.CallAsync<FinishLevelRequest, bool>(request);

            return task;
        }

        //public IEnumerator FinishLevel(CoroutineResult<bool> result, FinishLevelRequest request)
        //{
        //    var param = (PlayerHandlerClient.FinishLevelRequest)request;
        //    var resulter = new CoroutineResult<bool>();
        //    var task = Coroutine2.Start(_handlerClient.Call<bool>(resulter, "FinishLevel", param.SessionToken, param.LevelTypeId, param.IsSuccess));
        //    while (!task.IsFinished)
        //    {
        //        yield return null;
        //    }
        //    result.Result = resulter.Result;
        //}

        /// <summary>
        /// 随机请求一次，获取SessionToken
        /// </summary>
        /// <returns></returns>
        //public async Task HandshakeAsync()
        //{
        //    if (string.IsNullOrEmpty(SessionToken))
        //    {
        //        var resulter = new CoroutineResult<object>();
        //        var task = Coroutine2.Start(_handlerClient.Call<object>(resulter, "Handshake"));
        //        while (!task.IsFinished)
        //        {
        //            await Task.Delay(1);
        //        }
        //    }
        //}
        //public IEnumerator Handshake()
        //{
        //    if (string.IsNullOrEmpty(SessionToken))
        //    {
        //        var resulter = new CoroutineResult<object>();
        //        var task = Coroutine2.Start(_handlerClient.Call<object>(resulter, "Handshake"));
        //        while (!task.IsFinished)
        //        {
        //            yield return null;
        //        }
        //    }
        //}
    }
}
