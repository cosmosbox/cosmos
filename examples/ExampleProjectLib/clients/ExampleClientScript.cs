using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace ExampleProjectLib.clients
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
                while (id < 5)
                {
                    id++;
                    ClientLoop(id);
                    Thread.Sleep(2000); // 1秒登录一个
                }
            });
        }

        async void ClientLoop(int id)
        {
            Logger.Warn("Now Start Client: {0}", id);
            //while (true)
            {
                // Login
                var client = new GateClient("127.0.0.1", 14002);
                var loginRes = await client.Login();
                var host = loginRes.GameServerHost;
                if (host == "*")
                {
                    host = "127.0.0.1";
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
                for (var i = 0; i < 100; i++)
                {
                    // Enter Level
                    var rand = new Random();
                    var randLevelId = rand.Next(1, 100000);
                    gameClient.EnterLevel(sessionToken, randLevelId);

                    // 5s in level 
                    Thread.Sleep(100);

                    // Finish Level
                    gameClient.FinishLevel(sessionToken, randLevelId, true);

                }

                
            }
            // Logout, Append player result
            Logger.Warn("Now End Client.................. {0}", id);
        }
    }
}
