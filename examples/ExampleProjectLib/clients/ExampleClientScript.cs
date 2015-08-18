using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cosmos.Utils;
using NLog;

namespace ExampleProjectLib
{
    public class ExampleClientScript
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public Task Task;
        private int _callCount = 0;
        private int _lastCallCount = 0;
        public ExampleClientScript()
        {
            new Thread(() =>
            {
                int id = 0;
                while (id < 1)
                {
                    id++;
                    var id_ = id;
                    Coroutine2.Start<object, int>(ClientLoop);
                    //Thread.Sleep(100); // 1秒登录一个
                }

                while (true)
                {
                    Logger.Info("Call Count One Second : {0}, Total: {1}", _callCount - _lastCallCount, _callCount);

                    _lastCallCount = _callCount;
                    Thread.Sleep(1000);
                }
            }).Start();

        }

        IEnumerator ClientLoop(CoroutineResult<object> result, int id)
        {
            Logger.Warn("Now Start Client: {0}", id);
            // Login
            string host;
            LoginResProto loginRes;
            using (var client = new GateClient("127.0.0.1", 14002))
            {
                var co = Coroutine<LoginResProto>.Start(client.Login(id));
                while (!co.IsFinished)
                    yield return null;
                loginRes = co.Result;
                if (loginRes == null)
                    yield break;

                if (loginRes.Id != id)
                    throw new Exception("Error id");
                host = loginRes.GameServerHost;
                if (host == "*")
                {
                    host = "127.0.0.1";
                }
            }

            // Connect game server
            var gameClient = new PlayerHandlerClient(host, loginRes.GameServerPort, loginRes.SubcribePort);
            var sessionToken = gameClient.SessionToken;
            if (string.IsNullOrEmpty(sessionToken))
            {
                gameClient.Handshake();
                sessionToken = gameClient.SessionToken;

                if (string.IsNullOrEmpty(sessionToken))
                    throw new Exception("No SessionToken Error!");
            }
            // 操作100次后结束客户端
            for (var i = 0; i < int.MaxValue; i++)
            {
                //Logger.Info("EnterLevel from Id: {0}, Loop: {1}", id, i);
                // Enter Level
                var rand = new Random();
                var randLevelId = rand.Next(1, 100000);
                gameClient.EnterLevel(sessionToken, randLevelId);

                // 5s in level 
                yield return null;

                //Logger.Info("FinishLevel from Id: {0}, Loop: {1}", id, i);
                // Finish Level
                gameClient.FinishLevel(sessionToken, randLevelId, true);


                _callCount++;
            }
            // Logout, Append player result
            Logger.Warn("Now End Client.................. {0}", id);
            yield break;
        }
    }
}
