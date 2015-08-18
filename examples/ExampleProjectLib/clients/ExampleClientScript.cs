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
                    ClientLoop(id_);
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

        async void ClientLoop(int id)
        {
            Logger.Warn("Now Start Client: {0}", id);
            // Login
            string host;
            LoginResProto loginRes;
            using (var client = new GateClient("127.0.0.1", 14002))
            {
                loginRes = await client.Login(id);
                if (loginRes == null)
                    return;

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
                await gameClient.Handshake();
                sessionToken = gameClient.SessionToken;

                if (string.IsNullOrEmpty(sessionToken))
                    throw new Exception("No SessionToken Error!");
            }
            // 操作100次后结束客户端
            for (var i = 0; i < int.MaxValue; i++)
            {
                var rand = new Random();
                var randLevelId = rand.Next(1, 100000);
                await gameClient.EnterLevel(sessionToken, randLevelId);

                _callCount++;

                // 5s in level 
                await Task.Delay(0);
                var co2 = Coroutine2.Start<bool>(gameClient.FinishLevel,
                        new PlayerHandlerClient.FinishLevelParam()
                        {
                            SessionToken = sessionToken,
                            IsSuccess = true,
                            LevelTypeId = randLevelId,
                        });

                while (!co2.IsFinished)
                    await Task.Delay(0);

                _callCount++;
            }
            // Logout, Append player result
            Logger.Warn("Now End Client.................. {0}", id);
            
        }
    }
}
