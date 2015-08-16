using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace ExampleProjectLib
{
    public class ExampleClientScript
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public Task Task;
        public ExampleClientScript()
        {
            Task = Task.Run(() =>
            {
                int id = 0;
                while (id < 10000)
                {
                    id++;
                    var id_ = id;
                    Task.Run(() =>
                    {
                        ClientLoop(id_);
                    });
                    Thread.Sleep(500); // 1秒登录一个
                }
            });
        }

        async void ClientLoop(int id)
        {
            Logger.Warn("Now Start Client: {0}", id);
            //while (true)
            {
                // Login
                string host;
                LoginResProto loginRes;
                using (var client = new GateClient("127.0.0.1", 14002))
                {
                    loginRes = await client.Login();
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
                    Logger.Info("EnterLevel from Id: {0}, Loop: {1}", id, i);
                    // Enter Level
                    var rand = new Random();
                    var randLevelId = rand.Next(1, 100000);
                    gameClient.EnterLevel(sessionToken, randLevelId);

                    // 5s in level 
                    Thread.Sleep(1000);

                    Logger.Info("FinishLevel from Id: {0}, Loop: {1}", id, i);
                    // Finish Level
                    gameClient.FinishLevel(sessionToken, randLevelId, true);
                    Logger.Info("next call");

                }


            }
            // Logout, Append player result
            Logger.Warn("Now End Client.................. {0}", id);
        }
    }
}
