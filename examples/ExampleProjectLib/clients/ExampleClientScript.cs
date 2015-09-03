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
            Task.Run(async () =>
            {
                int id = 0;
                while (id < 1)
                {
                    id++;
                    var id_ = id;
                    //ClientLoopAsync(id_);  // 10 client -> 200 - 250 per second,  30 clients -> 450 - 500 per seconds, not stable
                    //Coroutine2.Start(ClientLoopCo(id_));  // 10 clients -> 200 - 250 per second, 30 clients -> 300 per seconds(one thread, more thread the same)
                    //Coroutine2.Start(TestLoop(id_));
                    TestLoopAsync(id_);
                    //new Thread(ClientLoopThread).Start(id_);

                    //Thread.Sleep(100); // 1秒登录一个
                }

                while (true)
                {
                    Logger.Info("Call Count One Second : {0}, Total: {1}", _callCount - _lastCallCount, _callCount);

                    _lastCallCount = _callCount;
                    await Task.Delay(1000);
                }
            });//.Start();

        }
        void ClientLoopThread(object oid)
        {
            var id = (int) oid;

            Logger.Warn("Now Start Client: {0}", id);
            // Login
            string host;
            LoginResProto loginRes;
            using (var client = new GateClient("127.0.0.1", 14002))
            {
                var t = client.Login(id);
                t.Wait();
                loginRes = t.Result;
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
                gameClient.HandshakeAsync().Wait();
                sessionToken = gameClient.SessionToken;

                if (string.IsNullOrEmpty(sessionToken))
                    throw new Exception("No SessionToken Error!");
            }
            // 操作100次后结束客户端
            for (var i = 0; i < int.MaxValue; i++)
            {
                var rand = new Random();
                var randLevelId = rand.Next(1, 100000);
                gameClient.EnterLevel(sessionToken, randLevelId).Wait();

                _callCount++;

                // 5s in level 
                //await Task.Delay(1);
                Thread.Sleep(1);
                gameClient.FinishLevel(sessionToken, randLevelId, true).Wait();

                //var co2 = Coroutine2.Start<bool>(gameClient.FinishLevel,
                //        );

                //while (!co2.IsFinished)
                //    await Task.Delay(0);

                _callCount++;
            }
            // Logout, Append player result
            Logger.Warn("Now End Client.................. {0}", id);

        }

        private async void TestLoopAsync(int id)
        {
            Logger.Warn("Now Start Client: {0}", id);
            // Login
            string host;
            LoginResProto loginRes;
            using (var client = new GateClient("127.0.0.1", 14002))
            {
                while (true)
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
                    _callCount++;
                }

            }
        }

        async void ClientLoopAsync(int id)
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
                await gameClient.HandshakeAsync();
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
                await Task.Delay(1);
                await gameClient.FinishLevel(sessionToken, randLevelId, true);
                
                //var co2 = Coroutine2.Start<bool>(gameClient.FinishLevel,
                //        );

                //while (!co2.IsFinished)
                //    await Task.Delay(0);

                _callCount++;
            }
            // Logout, Append player result
            Logger.Warn("Now End Client.................. {0}", id);
            
        }

        private IEnumerator TestLoop(int id)
        {
            Logger.Warn("Now Start Client: {0}", id);
            // Login
            string host;
            LoginResProto loginRes;
            using (var client = new GateClient("127.0.0.1", 14002))
            {
                while (true)
                {
                    var co = Coroutine2.Start<LoginResProto, int>(client.Login, id);
                    yield return co;
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
                    _callCount++;
                }

            }
        }

        IEnumerator ClientLoopCo(int id)
        {
            Logger.Warn("Now Start Client: {0}", id);
            // Login
            string host;
            LoginResProto loginRes;
            using (var client = new GateClient("127.0.0.1", 14002))
            {
                var co = Coroutine2.Start<LoginResProto, int>(client.Login, id);
                yield return co;
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
                var co2 = Coroutine2.Start(gameClient.Handshake());
                yield return co2;

                sessionToken = gameClient.SessionToken;

                if (string.IsNullOrEmpty(sessionToken))
                    throw new Exception("No SessionToken Error!");
            }
            // 操作100次后结束客户端
            for (var i = 0; i < int.MaxValue; i++)
            {
                var rand = new Random();
                var randLevelId = rand.Next(1, 100000);
                var result = new CoroutineResult<bool>();
                yield return Coroutine2.Start(gameClient.EnterLevel(result, sessionToken, randLevelId));

                _callCount++;

                // 5s in level 
                yield return null;

                yield return Coroutine2.Start(gameClient.FinishLevel(result, new PlayerHandlerClient.FinishLevelParam
                {
                    SessionToken = sessionToken,
                    LevelTypeId = randLevelId,
                    IsSuccess = true,
                }));

                //var co2 = Coroutine2.Start<bool>(gameClient.FinishLevel,
                //        );

                //while (!co2.IsFinished)
                //    await Task.Delay(0);

                _callCount++;
            }
            // Logout, Append player result
            Logger.Warn("Now End Client.................. {0}", id);

        }
    }
}
