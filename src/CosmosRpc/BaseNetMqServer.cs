using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cosmos.Utils;
using NLog;
using ZeroMQ;

namespace Cosmos.Rpc
{
    /// <summary>
    /// Mutable
    /// </summary>
    public class BaseResponseMsg : BaseNetMqMsg
    {
    }

    public abstract class BaseNetMqServer : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private ZSocket _responseSocket;
        private ZSocket _backendSocket;  // backend

        //private PublisherSocket _pubSocket;
        public int ResponsePort { get; private set; }
        public int PublishPort { get; private set; }
        public string Host { get; private set; }

        //private Task _pollerTask;

        //public Poller _poller;
        public string ServerToken;
        private string ServerBackendAddr
        {
            get { return string.Format("inproc://{0}", ServerToken); }
        }
        private int InitWorkerCount = 3; // one worker one thread
        private int CurWorkerIndex = 0; // I生成

        private List<BaseZmqWorker> _workers = new List<BaseZmqWorker>();

        private Thread _mainLoppThread;
        protected BaseNetMqServer(int responsePort = -1, int publishPort = 0, string host = "*")
        {
            Host = host;

            ServerToken = GenerateKey("Server");

            _backendSocket = new ZSocket(NetMqManager.Instance.Context, ZSocketType.ROUTER);
            _backendSocket.Bind(ServerBackendAddr);
            InitWorkers();

            _responseSocket = new ZSocket(NetMqManager.Instance.Context, ZSocketType.ROUTER);


            if (responsePort == -1)
            {
                while (true)
                {
                    var rand = new Random();
                    ResponsePort = rand.Next(50000, 60000);
                    try
                    {
                        _responseSocket.Bind(string.Format("tcp://{0}:{1}", host, ResponsePort));
                        break;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
            else
            {
                ResponsePort = responsePort;
                _responseSocket.Bind(string.Format("tcp://{0}:{1}", host, ResponsePort));
            }

            //Coroutine2.Start(MainLoop());
            MainLoop();
        }

        void InitWorkers()
        {
            for (var i = 0; i < InitWorkerCount; i++)
            {
                AddWorker();
            }

        }

        void AddWorker()
        {
            var worker = new BaseZmqWorker(this, ServerBackendAddr, CurWorkerIndex++);
            _workers.Add(worker);
        }

        /// <summary>
        /// Do Publisher
        /// </summary>
        /// <param name="topicName"></param>
        /// <param name="data"></param>
        //public void Publish(string topicName, byte[] data)
        //{
        //    _pubSocket.SendMore(topicName).Send(data);
        //}

        List<string> workerQueue = new List<string>();

        private async void MainLoop()
        {
            ZError error;
            ZMessage incoming;
            var poll = ZPollItem.CreateReceiver();

            while (true)
            {
                await Task.Delay(1);
                //yield return null;

                if (_backendSocket.PollIn(poll, out incoming, out error, TimeSpan.FromMilliseconds(1)))
                {
                    using (incoming)
                    {
                        // Handle worker activity on backend

                        // incoming[0] is worker_id
                        string workerName = incoming[0].ReadString();
                        // Queue worker identity for load-balancing
                        workerQueue.Add(workerName);

                        // incoming[1] is empty

                        // incoming[2] is READY or else client_id
                        var client_id = incoming[2];
                        var clientIdString = client_id.ToString();
                        if (client_id.ToString() == "READY")
                        {
                            Logger.Warn("I: ({0}) worker ready!!!", workerName);
                        }
                        else
                        {
                            // incoming[3] is empty
                            // incoming[4] is reply
                            // string reply = incoming[4].ReadString();
                            // int reply = incoming[4].ReadInt32();

                            Logger.Trace("I: ({0}) work complete", workerName);

                            using (var outgoing = new ZMessage())
                            {
                                outgoing.Add(client_id);
                                outgoing.Add(new ZFrame());
                                outgoing.Add(incoming[4]);

                                // Send
                                _responseSocket.Send(outgoing);
                            }
                        }
                    }
                }
                else
                {
                    if (error == ZError.ETERM)
                        return;
                        //yield break;
                    if (error != ZError.EAGAIN)
                        throw new ZException(error);
                }

                if (workerQueue.Count > 0)
                {
                    // Poll frontend only if we have available workers
                    if (_responseSocket.PollIn(poll, out incoming, out error, TimeSpan.FromMilliseconds(1)))
                    {
                        using (incoming)
                        {
                            // Here is how we handle a client request

                            // Dequeue the next worker identity
                            string workerId = workerQueue[0];
                            workerQueue.RemoveAt(0);

                            // incoming[0] is client_id
                            var client_id = incoming[0];
                            var clientIdStr = client_id.ToString();
                            // incoming[1] is empty

                            // incoming[2] is request
                            // string request = incoming[2].ReadString();
                            var requestData = incoming[2];

                            Logger.Trace("I: ({0}) working on ({1}) {2}", workerId, client_id, requestData);

                            using (var outgoing = new ZMessage())
                            {
                                outgoing.Add(new ZFrame(workerId));
                                outgoing.Add(new ZFrame());
                                outgoing.Add(client_id);
                                outgoing.Add(new ZFrame());
                                outgoing.Add(requestData);

                                // Send
                                _backendSocket.Send(outgoing);
                            }
                        }
                    }
                    else
                    {
                        if (error == ZError.ETERM)
                            return;
                            //yield break;
                        if (error != ZError.EAGAIN)
                            throw new ZException(error);
                    }
                }
                else
                {
                    Logger.Warn("no idle worker....");
                    AddWorker(); // 不够worker，创建一个
                }
            }
        }

        internal void OnRecvMsg(BaseZmqWorker worker, ZMessage recvMsg)
        {
            using (recvMsg)
            {
                var startTime = DateTime.UtcNow;
                using (recvMsg)
                {
                    var clientAddr = recvMsg[0];
                    var clientData = recvMsg[2].Read();
                    var baseRequestMsg = MsgPackTool.GetMsg<BaseRequestMsg>(clientData);
                    var requestDataMsg = baseRequestMsg.Data;

                    var responseTask = ProcessRequest(requestDataMsg);
                    responseTask.Wait();
                    var responseMsg = responseTask.Result;

                    // if no session key, generate new
                    var sessionToken = baseRequestMsg.SessionToken;
                    if (string.IsNullOrEmpty(sessionToken))
                    {
                        sessionToken = GenerateSessionKey();
                    }
                    var baseResponseMsg = new BaseResponseMsg()
                    {
                        SessionToken = sessionToken,
                        RequestToken = baseRequestMsg.RequestToken,
                        Data = responseMsg,
                    };

                    var sendData = MsgPackTool.GetBytes(baseResponseMsg);

                    var messageToServer = new ZMessage();
                    messageToServer.Append(clientAddr);
                    messageToServer.Append(ZFrame.CreateEmpty());
                    messageToServer.Append(new ZFrame(sendData));
                    worker.workerSocket.SendMessage(messageToServer);
                }

                Logger.Trace("Receive Msg and Send used Time: {0:F5}s", (DateTime.UtcNow - startTime).TotalSeconds);
            }

        }

        protected abstract Task<byte[]> ProcessRequest(byte[] requestDataMsg);

        public void Dispose()
        {
            _responseSocket.Dispose();
            _backendSocket.Dispose();
            foreach (var worker in _workers)
            {
                worker.Dispose();
            }
            _workers.Clear();
        }

        /// <summary>
        /// Create a new Session Key of Hex
        /// </summary>
        /// <returns></returns>
        public static string GenerateSessionKey()
        {
            return GenerateKey("S");
        }
        public static string GenerateRequestKey()
        {
            return GenerateKey("REQ");
        }
        public static string GenerateWorkerKey()
        {
            return GenerateKey("WORKER");
        }

        private static int RandomSeed = 0;
        public static string GenerateKey(string suffix)
        {
            var now = DateTime.UtcNow;
            var random = new Random(RandomSeed++);
            var pureKeyStr = string.Format("{0}{1}{2}", suffix, now.Ticks, random.Next(int.MinValue, int.MaxValue));

            return Md5Util.String16(pureKeyStr);
        }
    }

}
