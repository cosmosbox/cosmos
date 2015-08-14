using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExampleProjectLib.clients
{
    public class ExampleClientScript
    {
        public ExampleClientScript()
        {
            ClientLoop();
        }

        async void ClientLoop()
        {
            
            while (true)
            {
                // Login
                var client = new GateClient("127.0.0.1", 14002);
                var loginRes = await client.Login();

                // Connect game server
                var gameClient = new PlayerHandlerClient(loginRes.GameServerHost, loginRes.GameServerPort);
                var sessionToken = gameClient.SessionToken;
                if (string.IsNullOrEmpty(sessionToken))
                {
                    gameClient.Handshake();
                    sessionToken = gameClient.SessionToken;

                    if (string.IsNullOrEmpty(sessionToken))
                        throw new Exception("No SessionToken Error!");
                }

                for (var i = 0; i < 1000; i++)
                {
                    // Enter Level
                    var rand = new Random();
                    var randLevelId = rand.Next(1, 100000);
                    gameClient.EnterLevel(sessionToken, randLevelId);

                    // 5s in level 
                    Thread.Sleep(5000);

                    // Finish Level
                    gameClient.FinishLevel(sessionToken, randLevelId, true);

                }

                // Logout, Append player result
            }

        }
    }
}
